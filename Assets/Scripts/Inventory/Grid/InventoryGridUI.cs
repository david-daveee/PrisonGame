using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryGridUI : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private RectTransform cellsLayer;
    [SerializeField] private RectTransform itemsLayer;

    [Header("Prefabs")]
    [SerializeField] private InventoryCellUI cellPrefab;
    [SerializeField] private InventoryItemUI itemPrefab;

    [Header("Layout")]
    [SerializeField, Min(1f)] private float cellSize = 70f;
    [SerializeField, Min(0f)] private float spacing = 4f;

    [Header("Category Colors")]
    [SerializeField] private Color emptyCellColor =
        new Color(0.35f, 0.33f, 0.33f, 0.4f);
    [SerializeField] private List<ItemCategoryColor> categoryColors = new()
    {
        new(ItemCategory.Tool, new Color(0.2f, 0.45f, 0.65f, 0.55f)),
        new(ItemCategory.Food, new Color(0.65f, 0.45f, 0.15f, 0.55f)),
        new(ItemCategory.Material, new Color(0.5f, 0.36f, 0.22f, 0.55f)),
        new(ItemCategory.Medicine, new Color(0.65f, 0.2f, 0.24f, 0.55f)),
        new(ItemCategory.Clothing, new Color(0.35f, 0.3f, 0.65f, 0.55f)),
        new(ItemCategory.Valuable, new Color(0.7f, 0.6f, 0.18f, 0.55f))
    };

    [Header("Placement Preview")]
    [SerializeField] private Color validPlacementColor =
        new Color(0.18f, 0.7f, 0.28f, 0.75f);
    [SerializeField] private Color invalidPlacementColor =
        new Color(0.8f, 0.18f, 0.18f, 0.75f);

    private InventoryGrid grid;
    private IGridInventory inventory;
    private InventoryUI transferCoordinator;
    private InventoryPlacement hoveredPlacement;
    private InventoryItemUI draggedItemUI;
    private Vector2Int dragStartPosition;
    private bool dragStartRotation;
    private bool hasDetachedPlacement;
    private InventoryCellUI[,] cells;

    public void Initialize(
        IGridInventory gridInventory,
        InventoryUI inventoryUI)
    {
        if (gridInventory == null || gridInventory.Grid == null)
        {
            Debug.LogError(
                "InventoryGridUI received a null InventoryGrid.",
                this
            );
            return;
        }

        inventory = gridInventory;
        grid = gridInventory.Grid;
        transferCoordinator = inventoryUI;
        ClearLayer(cellsLayer);
        ClearLayer(itemsLayer);
        ResizeLayers();
        CreateCells();
        CreateItems();
        RefreshCellColors();
    }

    public void Refresh()
    {
        if (grid == null)
        {
            return;
        }

        ClearLayer(itemsLayer);
        CreateItems();
        RefreshCellColors();
    }

    public void HandleDrop(
        InventoryItemUI itemUI,
        PointerEventData eventData)
    {
        if (itemUI == null || grid == null || !hasDetachedPlacement)
        {
            return;
        }

        if (!TryGetGridPosition(
            eventData,
            itemUI.PointerOffset,
            out Vector2Int gridPosition))
        {
            if (transferCoordinator != null &&
                transferCoordinator.TryTransferItem(
                    this,
                    itemUI,
                    eventData))
            {
                return;
            }

            RestoreDetachedPlacement(itemUI.Placement);
            return;
        }

        if (!grid.TryAttachDetachedPlacement(
            itemUI.Placement,
            gridPosition))
        {
            RestoreDetachedPlacement(itemUI.Placement);
            return;
        }

        ReturnDraggedItemToItemsLayer(itemUI);
        hasDetachedPlacement = false;
        draggedItemUI = null;
        inventory.NotifyChanged();
        Refresh();
    }

    public bool TryTransferDetachedItemTo(
        InventoryGridUI destination,
        InventoryItemUI itemUI,
        PointerEventData eventData)
    {
        if (!hasDetachedPlacement ||
            draggedItemUI != itemUI ||
            destination == null ||
            inventory == null ||
            destination.inventory == null)
        {
            return false;
        }

        if (!destination.TryGetGridPosition(
            eventData,
            itemUI.PointerOffset,
            out Vector2Int destinationPosition))
        {
            return false;
        }

        hasDetachedPlacement = false;
        draggedItemUI = null;
        ReturnDraggedItemToItemsLayer(itemUI);

        if (!InventoryTransferService.TryTransferDetachedPlacement(
            inventory,
            destination.inventory,
            itemUI.Placement,
            destinationPosition))
        {
            hasDetachedPlacement = true;
            draggedItemUI = itemUI;
            transferCoordinator.MoveItemToDragLayer(itemUI);
            return false;
        }

        hoveredPlacement = null;
        Refresh();
        return true;
    }

    public void SetHoveredPlacement(
        InventoryPlacement placement,
        bool isHovered)
    {
        if (isHovered)
        {
            hoveredPlacement = placement;
        }
        else if (hoveredPlacement == placement)
        {
            hoveredPlacement = null;
        }
    }

    public bool TryShowPlacementPreview(
        InventoryItemUI itemUI,
        PointerEventData eventData)
    {
        if (grid == null || itemUI?.Placement?.Item == null)
        {
            return false;
        }

        if (!TryGetGridPosition(
            eventData,
            itemUI.PointerOffset,
            out Vector2Int gridPosition))
        {
            return false;
        }

        InventoryPlacement placement = itemUI.Placement;
        bool canPlace = grid.CanPlaceItem(
            placement.Item,
            gridPosition,
            placement.IsRotated
        );
        Color previewColor = canPlace
            ? validPlacementColor
            : invalidPlacementColor;

        SetCellsColor(
            gridPosition,
            placement.GetCurrentSize(),
            previewColor,
            true
        );
        return true;
    }

    public void ClearPlacementPreview()
    {
        if (cells == null)
        {
            return;
        }

        foreach (InventoryCellUI cell in cells)
        {
            cell.ClearPlacementPreview();
        }
    }

    public void UpdateDragPreview(
        InventoryItemUI itemUI,
        PointerEventData eventData)
    {
        transferCoordinator?.UpdateDragPreview(
            this,
            itemUI,
            eventData
        );
    }

    public void ClearDragPreview()
    {
        transferCoordinator?.ClearDragPreview();
    }

    public bool BeginDrag(InventoryItemUI itemUI)
    {
        if (grid == null || itemUI == null)
        {
            return false;
        }

        if (hasDetachedPlacement && draggedItemUI != null)
        {
            RestoreDetachedPlacement(draggedItemUI.Placement);
        }

        InventoryPlacement placement = itemUI.Placement;

        if (!grid.TryDetachPlacementForMove(placement))
        {
            return false;
        }

        dragStartPosition = placement.Position;
        dragStartRotation = placement.IsRotated;
        hasDetachedPlacement = true;
        draggedItemUI = itemUI;
        RefreshCellColors();
        transferCoordinator?.MoveItemToDragLayer(itemUI);
        return true;
    }

    public void RotateHoveredItem()
    {
        InventoryPlacement placement = draggedItemUI != null
            ? draggedItemUI.Placement
            : hoveredPlacement;

        if (grid == null || placement == null)
        {
            return;
        }

        bool rotationSucceeded = draggedItemUI != null
            ? grid.RotateDetachedPlacement(placement)
            : grid.TryRotatePlacement(placement);

        if (!rotationSucceeded)
        {
            return;
        }

        if (draggedItemUI != null)
        {
            draggedItemUI.ApplyRotationAroundCenter(
                GetPixelSize(placement)
            );
            return;
        }

        hoveredPlacement = null;
        inventory.NotifyChanged();
        Refresh();
    }

    private bool TryGetGridPosition(
        PointerEventData eventData,
        Vector2 pointerOffset,
        out Vector2Int gridPosition)
    {
        gridPosition = default;

        if (!RectTransformUtility.RectangleContainsScreenPoint(
            itemsLayer,
            eventData.position,
            eventData.pressEventCamera))
        {
            return false;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            itemsLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition))
        {
            return false;
        }

        Vector2 desiredTopLeft = localPointerPosition - pointerOffset;
        float step = cellSize + spacing;
        gridPosition = new Vector2Int(
            Mathf.RoundToInt(desiredTopLeft.x / step),
            Mathf.RoundToInt(-desiredTopLeft.y / step)
        );
        return true;
    }

    private void OnDisable()
    {
        if (hasDetachedPlacement && draggedItemUI != null)
        {
            RestoreDetachedPlacement(draggedItemUI.Placement);
        }
    }

    private void RestoreDetachedPlacement(
        InventoryPlacement placement)
    {
        if (draggedItemUI != null)
        {
            ReturnDraggedItemToItemsLayer(draggedItemUI);
        }

        placement.SetPosition(dragStartPosition);
        placement.SetRotation(dragStartRotation);

        if (!grid.TryAttachDetachedPlacement(
            placement,
            dragStartPosition))
        {
            Debug.LogError(
                $"Could not restore {placement.Item.ItemData.DisplayName} after a failed drag.",
                this
            );
        }

        hasDetachedPlacement = false;
        draggedItemUI = null;
        Refresh();
    }

    private void ReturnDraggedItemToItemsLayer(
        InventoryItemUI itemUI)
    {
        itemUI.transform.SetParent(itemsLayer, false);
        itemUI.transform.SetAsLastSibling();
    }

    private void CreateCells()
    {
        cells = new InventoryCellUI[grid.Width, grid.Height];

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                InventoryCellUI cell =
                    Instantiate(cellPrefab, cellsLayer);
                cell.Initialize(new Vector2Int(x, y));
                cells[x, y] = cell;

                RectTransform cellRect =
                    (RectTransform)cell.transform;
                ConfigureTopLeftRect(cellRect);
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                cellRect.anchoredPosition = new Vector2(
                    x * (cellSize + spacing),
                    -y * (cellSize + spacing)
                );
            }
        }
    }

    private void CreateItems()
    {
        foreach (InventoryPlacement placement in grid.Placements)
        {
            InventoryItemUI itemUI =
                Instantiate(itemPrefab, itemsLayer);
            Vector2 pixelSize = GetPixelSize(placement);
            Vector2 pixelPosition = new Vector2(
                placement.Position.x * (cellSize + spacing),
                -placement.Position.y * (cellSize + spacing)
            );

            itemUI.Initialize(this, placement);
            itemUI.ApplyLayout(pixelPosition, pixelSize);
        }
    }

    private Vector2 GetPixelSize(InventoryPlacement placement)
    {
        Vector2Int size = placement.GetCurrentSize();

        return new Vector2(
            size.x * cellSize + (size.x - 1) * spacing,
            size.y * cellSize + (size.y - 1) * spacing
        );
    }

    private void RefreshCellColors()
    {
        if (grid == null || cells == null)
        {
            return;
        }

        foreach (InventoryCellUI cell in cells)
        {
            cell.SetBaseColor(emptyCellColor);
        }

        foreach (InventoryPlacement placement in grid.Placements)
        {
            SetCellsColor(
                placement.Position,
                placement.GetCurrentSize(),
                GetCategoryColor(placement.Item.ItemData.Category),
                false
            );
        }
    }

    private Color GetCategoryColor(ItemCategory category)
    {
        foreach (ItemCategoryColor entry in categoryColors)
        {
            if (entry.Category == category)
            {
                return entry.Color;
            }
        }

        return emptyCellColor;
    }

    private void SetCellsColor(
        Vector2Int position,
        Vector2Int size,
        Color color,
        bool isPreview)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                int cellX = position.x + x;
                int cellY = position.y + y;

                if (cellX < 0 ||
                    cellY < 0 ||
                    cellX >= grid.Width ||
                    cellY >= grid.Height)
                {
                    continue;
                }

                if (isPreview)
                {
                    cells[cellX, cellY].ShowPlacementPreview(color);
                }
                else
                {
                    cells[cellX, cellY].SetBaseColor(color);
                }
            }
        }
    }

    private void ResizeLayers()
    {
        Vector2 size = new Vector2(
            grid.Width * cellSize + (grid.Width - 1) * spacing,
            grid.Height * cellSize + (grid.Height - 1) * spacing
        );

        ConfigureLayer(cellsLayer, size);
        ConfigureLayer(itemsLayer, size);
        ((RectTransform)transform).sizeDelta = size;
    }

    private static void ConfigureLayer(
        RectTransform layer,
        Vector2 size)
    {
        ConfigureTopLeftRect(layer);
        layer.anchoredPosition = Vector2.zero;
        layer.sizeDelta = size;
    }

    private static void ConfigureTopLeftRect(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
    }

    private static void ClearLayer(RectTransform layer)
    {
        for (int i = layer.childCount - 1; i >= 0; i--)
        {
            Destroy(layer.GetChild(i).gameObject);
        }
    }
}
