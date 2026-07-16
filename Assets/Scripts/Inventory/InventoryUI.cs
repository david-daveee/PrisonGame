using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private InventoryGridUI inventoryGridUI;
    [SerializeField] private WorldItemDropper worldItemDropper;
    [SerializeField] private StackSplitDialog stackSplitDialogPrefab;
    [SerializeField, Min(0f)] private float gridSpacing = 80f;
    [SerializeField, Min(0f)] private float panelPadding = 50f;

    private IGridInventory currentInventory;
    private ContainerInventory currentContainer;
    private InventoryGridUI containerGridUI;
    private TMP_Text playerTitleText;
    private TMP_Text containerInfoText;
    private RectTransform dragLayer;
    private StackSplitDialog stackSplitDialog;
    private PendingSplit pendingSplit;
    private CanvasGroup playerGridCanvasGroup;
    private CanvasGroup containerGridCanvasGroup;

    public bool HasOpenModal =>
        stackSplitDialog != null && stackSplitDialog.IsOpen;

    private sealed class PendingSplit
    {
        public InventoryGridUI SourceGrid;
        public InventoryPlacement SourcePlacement;
        public InventoryGridUI DestinationGrid;
        public Vector2Int DestinationPosition;
        public InventoryPlacement MergeTarget;
        public bool IsRotated;
        public bool IsWorldDrop;
    }

    private void Awake()
    {
        Close();
    }

    private void OnValidate()
    {
        if (inventoryRoot == null || inventoryGridUI == null)
        {
            Debug.LogError(
                $"InventoryUI on '{name}' is missing its root or grid UI.",
                this
            );
        }

        if (worldItemDropper == null)
        {
            Debug.LogWarning(
                $"InventoryUI on '{name}' cannot drop items without a " +
                "WorldItemDropper.",
                this
            );
        }

        if (stackSplitDialogPrefab == null)
        {
            Debug.LogWarning(
                $"InventoryUI on '{name}' cannot open the stack split " +
                "dialog until its prefab is assigned.",
                this
            );
        }
    }

    public void Open(IInventory inventory)
    {
        if (inventory is not IGridInventory gridInventory)
        {
            Debug.LogError(
                "InventoryUI requires an IGridInventory.",
                this
            );
            return;
        }

        CloseCurrentInventories();
        currentInventory = gridInventory;
        inventoryRoot.SetActive(true);
        currentInventory.Changed += RefreshPlayerInventory;
        inventoryGridUI.Initialize(currentInventory, this);

        EnsureContainerUI();
        containerGridUI.gameObject.SetActive(false);
        playerTitleText.gameObject.SetActive(false);
        containerInfoText.gameObject.SetActive(false);
        LayoutSingleGrid();
    }

    public void OpenContainer(
        IGridInventory playerInventory,
        ContainerInventory container)
    {
        if (playerInventory == null || container == null)
        {
            return;
        }

        CloseCurrentInventories();
        currentInventory = playerInventory;
        currentContainer = container;
        inventoryRoot.SetActive(true);
        EnsureContainerUI();

        currentInventory.Changed += RefreshPlayerInventory;
        currentContainer.Changed += RefreshContainerInventory;

        inventoryGridUI.Initialize(currentInventory, this);
        containerGridUI.gameObject.SetActive(true);
        containerGridUI.Initialize(currentContainer, this);

        playerTitleText.gameObject.SetActive(true);
        containerInfoText.gameObject.SetActive(true);
        playerTitleText.text = "Inventory";
        containerInfoText.text = string.IsNullOrWhiteSpace(
            currentContainer.Description)
                ? currentContainer.ContainerName
                : $"{currentContainer.ContainerName}\n" +
                  $"<size=18>{currentContainer.Description}</size>";

        LayoutContainerGrids();
    }

    public void Close()
    {
        CancelPendingSplit();
        CloseCurrentInventories();
        inventoryRoot.SetActive(false);
    }

    public void Refresh()
    {
        RefreshPlayerInventory();
    }

    public void RotateHoveredItem()
    {
        if (currentInventory == null)
        {
            return;
        }

        inventoryGridUI.RotateHoveredItem();

        if (currentContainer != null)
        {
            containerGridUI.RotateHoveredItem();
        }
    }

    public bool TryTransferItem(
        InventoryGridUI source,
        InventoryItemUI itemUI,
        PointerEventData eventData)
    {
        if (currentContainer == null || source == null)
        {
            return false;
        }

        InventoryGridUI destination;

        if (source == inventoryGridUI)
        {
            destination = containerGridUI;
        }
        else if (source == containerGridUI)
        {
            destination = inventoryGridUI;
        }
        else
        {
            return false;
        }

        return source.TryTransferDetachedItemTo(
            destination,
            itemUI,
            eventData
        );
    }

    public bool TryDropItemIntoWorld(
        InventoryGridUI source,
        InventoryItemUI itemUI,
        PointerEventData eventData)
    {
        if (source == null ||
            itemUI == null ||
            worldItemDropper == null ||
            IsPointerInsideInventoryWindow(eventData))
        {
            return false;
        }

        return source.TryDropDetachedItemIntoWorld(
            itemUI,
            worldItemDropper
        );
    }

    public bool TryCommitSplit(
        InventoryGridUI sourceGrid,
        InventoryPlacement sourcePlacement,
        InventoryPlacement splitVisual,
        Vector2 pointerOffset,
        PointerEventData eventData)
    {
        if (sourceGrid?.Inventory == null ||
            sourcePlacement?.Item == null ||
            splitVisual?.Item == null)
        {
            return false;
        }

        if (TryResolveSplitDestination(
            splitVisual,
            pointerOffset,
            eventData,
            out InventoryGridUI destinationGrid,
            out Vector2Int destinationPosition,
            out InventoryPlacement mergeTarget))
        {
            if (mergeTarget != null)
            {
                return InventoryStackService.TryMergeStack(
                    sourceGrid.Inventory,
                    sourcePlacement,
                    destinationGrid.Inventory,
                    mergeTarget,
                    splitVisual.Item.Amount
                );
            }

            return InventoryStackService.TrySplitAndTransfer(
                sourceGrid.Inventory,
                sourcePlacement,
                destinationGrid.Inventory,
                destinationPosition,
                splitVisual.IsRotated,
                splitVisual.Item.Amount
            );
        }

        if (IsPointerInsideInventoryWindow(eventData))
        {
            return false;
        }

        return InventoryDropService.TryDropSplit(
            sourceGrid.Inventory,
            sourcePlacement,
            splitVisual.Item.Amount,
            worldItemDropper
        );
    }

    public bool TryOpenSplitDialog(
        InventoryGridUI sourceGrid,
        InventoryPlacement sourcePlacement,
        bool isRotated,
        Vector2 pointerOffset,
        PointerEventData eventData)
    {
        if (pendingSplit != null ||
            !InventoryStackService.CanSplit(
                sourceGrid?.Inventory,
                sourcePlacement))
        {
            return false;
        }

        InventoryItem previewItem = new InventoryItem(
            sourcePlacement.Item.ItemData,
            1
        );
        InventoryPlacement previewPlacement = new InventoryPlacement(
            previewItem,
            sourcePlacement.Position,
            isRotated
        );

        PendingSplit request = new PendingSplit
        {
            SourceGrid = sourceGrid,
            SourcePlacement = sourcePlacement,
            IsRotated = isRotated
        };

        if (TryResolveSplitDestination(
            previewPlacement,
            pointerOffset,
            eventData,
            out InventoryGridUI destinationGrid,
            out Vector2Int destinationPosition,
            out InventoryPlacement mergeTarget))
        {
            request.DestinationGrid = destinationGrid;
            request.DestinationPosition = destinationPosition;
            request.MergeTarget = mergeTarget;
        }
        else if (!IsPointerInsideInventoryWindow(eventData))
        {
            request.IsWorldDrop = true;
        }
        else
        {
            return false;
        }

        EnsureStackSplitDialog();

        if (stackSplitDialog == null)
        {
            return false;
        }

        if (!stackSplitDialog.Open(
            1,
            sourcePlacement.Item.Amount - 1,
            ApplyPendingSplit,
            CancelPendingSplit
        ))
        {
            return false;
        }

        pendingSplit = request;
        SetGridInteraction(false);
        return true;
    }

    public bool TryCancelModal()
    {
        if (stackSplitDialog == null || !stackSplitDialog.IsOpen)
        {
            return false;
        }

        stackSplitDialog.Cancel();
        return true;
    }

    public void MoveItemToDragLayer(InventoryItemUI itemUI)
    {
        if (itemUI == null)
        {
            return;
        }

        EnsureDragLayer();
        itemUI.transform.SetParent(dragLayer, true);
        dragLayer.SetAsLastSibling();
        itemUI.transform.SetAsLastSibling();
    }

    public void UpdateDragPreview(
        InventoryGridUI source,
        InventoryItemUI itemUI,
        PointerEventData eventData)
    {
        if (source != inventoryGridUI && source != containerGridUI)
        {
            return;
        }

        ClearDragPreview();
        inventoryGridUI.TryShowPlacementPreview(itemUI, eventData);

        if (currentContainer != null)
        {
            containerGridUI.TryShowPlacementPreview(itemUI, eventData);
        }
    }

    public void ClearDragPreview()
    {
        inventoryGridUI.ClearPlacementPreview();

        if (containerGridUI != null)
        {
            containerGridUI.ClearPlacementPreview();
        }
    }

    private void RefreshPlayerInventory()
    {
        if (currentInventory != null)
        {
            inventoryGridUI.Refresh();
        }
    }

    private bool IsPointerInsideInventoryWindow(
        PointerEventData eventData)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            (RectTransform)inventoryRoot.transform,
            eventData.position,
            eventData.pressEventCamera
        );
    }

    private bool TryResolveSplitDestination(
        InventoryPlacement splitVisual,
        Vector2 pointerOffset,
        PointerEventData eventData,
        out InventoryGridUI destinationGrid,
        out Vector2Int destinationPosition,
        out InventoryPlacement mergeTarget)
    {
        destinationGrid = null;
        destinationPosition = default;
        mergeTarget = null;

        if (inventoryGridUI.TryResolveSplitDestination(
            splitVisual.Item,
            splitVisual.IsRotated,
            eventData,
            pointerOffset,
            out destinationPosition,
            out mergeTarget))
        {
            destinationGrid = inventoryGridUI;
            return true;
        }

        if (currentContainer != null &&
            containerGridUI.TryResolveSplitDestination(
                splitVisual.Item,
                splitVisual.IsRotated,
                eventData,
                pointerOffset,
                out destinationPosition,
                out mergeTarget))
        {
            destinationGrid = containerGridUI;
            return true;
        }

        return false;
    }

    private void RefreshContainerInventory()
    {
        if (currentContainer != null)
        {
            containerGridUI.Refresh();
        }
    }

    private void CloseCurrentInventories()
    {
        if (currentInventory != null)
        {
            currentInventory.Changed -= RefreshPlayerInventory;
        }

        if (currentContainer != null)
        {
            currentContainer.Changed -= RefreshContainerInventory;
            currentContainer.NotifyClosed();
        }

        currentInventory = null;
        currentContainer = null;
    }

    private void EnsureContainerUI()
    {
        if (containerGridUI != null)
        {
            return;
        }

        containerGridUI = Instantiate(
            inventoryGridUI,
            inventoryGridUI.transform.parent
        );
        containerGridUI.name = "ContainerGridRoot";

        InventoryDebug clonedDebug =
            containerGridUI.GetComponent<InventoryDebug>();

        if (clonedDebug != null)
        {
            clonedDebug.enabled = false;
            Destroy(clonedDebug);
        }

        playerTitleText = CreateInfoText("PlayerInventoryTitle");
        containerInfoText = CreateInfoText("ContainerInfo");
        EnsureDragLayer();
    }

    private void EnsureStackSplitDialog()
    {
        if (stackSplitDialog != null)
        {
            return;
        }

        if (stackSplitDialogPrefab == null)
        {
            Debug.LogError(
                "InventoryUI has no Stack Split Dialog Prefab assigned.",
                this
            );
            return;
        }

        stackSplitDialog = Instantiate(
            stackSplitDialogPrefab,
            inventoryRoot.transform
        );
        stackSplitDialog.name = "StackSplitDialog";
    }

    private void ApplyPendingSplit(int amount)
    {
        if (pendingSplit == null)
        {
            return;
        }

        bool succeeded;

        if (pendingSplit.IsWorldDrop)
        {
            succeeded = InventoryDropService.TryDropSplit(
                pendingSplit.SourceGrid.Inventory,
                pendingSplit.SourcePlacement,
                amount,
                worldItemDropper
            );
        }
        else if (pendingSplit.MergeTarget != null)
        {
            succeeded = InventoryStackService.TryMergeStack(
                pendingSplit.SourceGrid.Inventory,
                pendingSplit.SourcePlacement,
                pendingSplit.DestinationGrid.Inventory,
                pendingSplit.MergeTarget,
                amount
            );
        }
        else
        {
            succeeded = InventoryStackService.TrySplitAndTransfer(
                pendingSplit.SourceGrid.Inventory,
                pendingSplit.SourcePlacement,
                pendingSplit.DestinationGrid.Inventory,
                pendingSplit.DestinationPosition,
                pendingSplit.IsRotated,
                amount
            );
        }

        if (!succeeded)
        {
            Debug.LogWarning(
                "Stack split destination is no longer valid. " +
                "No inventory state was changed.",
                this
            );
            return;
        }

        pendingSplit = null;
        stackSplitDialog.CloseAfterApply();
        SetGridInteraction(true);
    }

    private void CancelPendingSplit()
    {
        pendingSplit = null;
        SetGridInteraction(true);

        if (stackSplitDialog != null && stackSplitDialog.IsOpen)
        {
            stackSplitDialog.CloseAfterApply();
        }
    }

    private void SetGridInteraction(bool interactable)
    {
        playerGridCanvasGroup ??=
            GetOrAddCanvasGroup(inventoryGridUI.gameObject);
        playerGridCanvasGroup.interactable = interactable;
        playerGridCanvasGroup.blocksRaycasts = interactable;

        if (containerGridUI != null)
        {
            containerGridCanvasGroup ??=
                GetOrAddCanvasGroup(containerGridUI.gameObject);
            containerGridCanvasGroup.interactable = interactable;
            containerGridCanvasGroup.blocksRaycasts = interactable;
        }
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        return canvasGroup != null
            ? canvasGroup
            : target.AddComponent<CanvasGroup>();
    }

    private void EnsureDragLayer()
    {
        if (dragLayer != null)
        {
            dragLayer.SetAsLastSibling();
            return;
        }

        GameObject layerObject = new GameObject(
            "DragLayer",
            typeof(RectTransform)
        );
        layerObject.layer = inventoryRoot.layer;
        layerObject.transform.SetParent(inventoryRoot.transform, false);

        dragLayer = (RectTransform)layerObject.transform;
        dragLayer.anchorMin = Vector2.zero;
        dragLayer.anchorMax = Vector2.one;
        dragLayer.pivot = new Vector2(0f, 1f);
        dragLayer.anchoredPosition = Vector2.zero;
        dragLayer.sizeDelta = Vector2.zero;
        dragLayer.SetAsLastSibling();
    }

    private TMP_Text CreateInfoText(string objectName)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        textObject.layer = inventoryRoot.layer;
        textObject.transform.SetParent(inventoryRoot.transform, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 26f;
        text.raycastTarget = false;

        RectTransform textRect = (RectTransform)text.transform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(420f, 80f);

        return text;
    }

    private void LayoutSingleGrid()
    {
        RectTransform rootRect = (RectTransform)inventoryRoot.transform;
        RectTransform playerGridRect =
            (RectTransform)inventoryGridUI.transform;

        playerGridRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(
            playerGridRect.rect.width + panelPadding * 2f,
            playerGridRect.rect.height + panelPadding * 2f
        );
    }

    private void LayoutContainerGrids()
    {
        RectTransform rootRect = (RectTransform)inventoryRoot.transform;
        RectTransform playerGridRect =
            (RectTransform)inventoryGridUI.transform;
        RectTransform containerGridRect =
            (RectTransform)containerGridUI.transform;

        float playerWidth = playerGridRect.rect.width;
        float containerWidth = containerGridRect.rect.width;
        float totalWidth =
            containerWidth + gridSpacing + playerWidth;
        float leftEdge = -totalWidth * 0.5f;

        containerGridRect.anchoredPosition = new Vector2(
            leftEdge + containerWidth * 0.5f,
            -20f
        );
        playerGridRect.anchoredPosition = new Vector2(
            leftEdge + containerWidth + gridSpacing +
            playerWidth * 0.5f,
            -20f
        );

        float contentHeight = Mathf.Max(
            playerGridRect.rect.height,
            containerGridRect.rect.height
        );
        rootRect.sizeDelta = new Vector2(
            totalWidth + panelPadding * 2f,
            contentHeight + panelPadding * 2f + 80f
        );

        PositionInfoText(
            containerInfoText,
            containerGridRect.anchoredPosition.x,
            contentHeight * 0.5f + 30f
        );
        PositionInfoText(
            playerTitleText,
            playerGridRect.anchoredPosition.x,
            contentHeight * 0.5f + 30f
        );
    }

    private static void PositionInfoText(
        TMP_Text text,
        float x,
        float y)
    {
        ((RectTransform)text.transform).anchoredPosition =
            new Vector2(x, y);
    }
}
