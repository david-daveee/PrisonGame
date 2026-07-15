using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private InventoryGridUI inventoryGridUI;
    [SerializeField, Min(0f)] private float gridSpacing = 80f;
    [SerializeField, Min(0f)] private float panelPadding = 50f;

    private IGridInventory currentInventory;
    private ContainerInventory currentContainer;
    private InventoryGridUI containerGridUI;
    private TMP_Text playerTitleText;
    private TMP_Text containerInfoText;
    private RectTransform dragLayer;

    private void Awake()
    {
        Close();
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
