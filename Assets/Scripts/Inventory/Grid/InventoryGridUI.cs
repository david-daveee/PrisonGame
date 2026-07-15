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

    private InventoryGrid grid;
    private IGridInventory inventory;
    private InventoryUI transferCoordinator;
    private InventoryPlacement hoveredPlacement;
    private InventoryItemUI draggedItemUI;
    private Vector2Int dragStartPosition;
    private bool dragStartRotation;
    private bool hasDetachedPlacement;

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
    }

    public void Refresh()
    {
        if (grid == null)
        {
            return;
        }

        ClearLayer(itemsLayer);
        CreateItems();
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

    public void BeginDrag(InventoryItemUI itemUI)
    {
        if (grid == null || itemUI == null)
        {
            return;
        }

        if (hasDetachedPlacement && draggedItemUI != null)
        {
            RestoreDetachedPlacement(draggedItemUI.Placement);
        }

        InventoryPlacement placement = itemUI.Placement;

        if (!grid.TryDetachPlacementForMove(placement))
        {
            return;
        }

        dragStartPosition = placement.Position;
        dragStartRotation = placement.IsRotated;
        hasDetachedPlacement = true;
        draggedItemUI = itemUI;
        transferCoordinator?.MoveItemToDragLayer(itemUI);
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
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                InventoryCellUI cell =
                    Instantiate(cellPrefab, cellsLayer);
                cell.Initialize(new Vector2Int(x, y));

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
