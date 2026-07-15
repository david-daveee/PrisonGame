using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private const float LabelHeight = 20f;
    private const float LabelInset = 4f;
    private const float LabelGap = 5f;
    private const float AmountWidth = 22f;

    [Header("Visuals")]
    [SerializeField] private Image itemIcon;
    [SerializeField, Min(0f)] private float iconPadding = 5f;

    [Header("Labels")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text amountText;

    private InventoryGridUI owner;
    private InventoryPlacement placement;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image visualIcon;
    private PointerEventData lastDragEventData;
    private bool isDragging;

    public InventoryPlacement Placement => placement;
    public Vector2 PointerOffset { get; private set; }

    public void Initialize(
        InventoryGridUI inventoryGridUI,
        InventoryPlacement inventoryPlacement)
    {
        owner = inventoryGridUI;
        placement = inventoryPlacement;
        rectTransform = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        InventoryItem item = placement.Item;
        CreateVisualIconIfNeeded();
        visualIcon.sprite = item.ItemData.Icon;
        visualIcon.enabled = item.ItemData.Icon != null;
        visualIcon.preserveAspect = true;
        ConfigureLabels();
        itemNameText.text = item.ItemData.DisplayName;
        amountText.text = $"x{item.Amount}";
    }

    public void ApplyLayout(Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        ApplyIconLayout(size);
        ApplyLabelLayout();
    }

    private void ApplyIconLayout(Vector2 size)
    {
        RectTransform iconRect = visualIcon.rectTransform;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(
            0f,
            isDragging ? 0f : LabelHeight * 0.5f
        );

        float reservedLabelHeight = isDragging ? 0f : LabelHeight;
        Vector2 availableIconSize = new Vector2(
            Mathf.Max(1f, size.x - iconPadding * 2f),
            Mathf.Max(
                1f,
                size.y - iconPadding * 2f - reservedLabelHeight
            )
        );
        iconRect.sizeDelta = placement.IsRotated
            ? new Vector2(availableIconSize.y, availableIconSize.x)
            : availableIconSize;
        iconRect.localEulerAngles = placement.IsRotated
            ? new Vector3(0f, 0f, -90f)
            : Vector3.zero;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastDragEventData = eventData;
        PointerOffset = GetCenterOffset(rectTransform.sizeDelta);
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();

        if (!owner.BeginDrag(this))
        {
            canvasGroup.blocksRaycasts = true;
            lastDragEventData = null;
            return;
        }

        SetDragging(true);
        UpdateDragPosition(eventData);
        owner.UpdateDragPreview(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        lastDragEventData = eventData;
        UpdateDragPosition(eventData);
        owner.UpdateDragPreview(this, eventData);
    }

    public void ApplyRotationAroundCenter(Vector2 size)
    {
        Vector2 currentCenter =
            rectTransform.anchoredPosition +
            GetCenterOffset(rectTransform.sizeDelta);
        Vector2 newCenterOffset = GetCenterOffset(size);

        ApplyLayout(currentCenter - newCenterOffset, size);
        PointerOffset = newCenterOffset;

        if (lastDragEventData != null)
        {
            owner.UpdateDragPreview(this, lastDragEventData);
        }
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rectTransform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition))
        {
            return;
        }

        rectTransform.anchoredPosition =
            localPointerPosition - PointerOffset;
    }

    private static Vector2 GetCenterOffset(Vector2 size)
    {
        return new Vector2(size.x * 0.5f, -size.y * 0.5f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        owner.ClearDragPreview();
        SetDragging(false);
        lastDragEventData = null;
        canvasGroup.blocksRaycasts = true;
        owner.HandleDrop(this, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        owner.SetHoveredPlacement(placement, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        owner.SetHoveredPlacement(placement, false);
    }

    private void CreateVisualIconIfNeeded()
    {
        if (itemIcon.rectTransform != rectTransform)
        {
            visualIcon = itemIcon;
            return;
        }

        itemIcon.sprite = null;
        itemIcon.color = Color.clear;
        itemIcon.enabled = true;

        GameObject iconObject = new GameObject(
            "Icon",
            typeof(RectTransform),
            typeof(Image)
        );
        iconObject.layer = gameObject.layer;
        iconObject.transform.SetParent(transform, false);
        iconObject.transform.SetAsFirstSibling();

        visualIcon = iconObject.GetComponent<Image>();
        visualIcon.raycastTarget = false;
    }

    private void ConfigureLabels()
    {
        ConfigureLabel(itemNameText, TextAlignmentOptions.BottomLeft);
        ConfigureLabel(amountText, TextAlignmentOptions.BottomRight);

        itemNameText.enableAutoSizing = true;
        itemNameText.fontSizeMin = 8f;
        itemNameText.fontSizeMax = 13f;
        itemNameText.overflowMode = TextOverflowModes.Ellipsis;

        amountText.enableAutoSizing = false;
        amountText.fontSize = 13f;
    }

    private static void ConfigureLabel(
        TMP_Text label,
        TextAlignmentOptions alignment)
    {
        label.alignment = alignment;
        label.color = Color.white;
        label.fontStyle = FontStyles.Bold;
        label.outlineColor = Color.black;
        label.outlineWidth = 0.2f;
        label.raycastTarget = false;
    }

    private void ApplyLabelLayout()
    {
        RectTransform nameRect = itemNameText.rectTransform;
        nameRect.anchorMin = Vector2.zero;
        nameRect.anchorMax = new Vector2(1f, 0f);
        nameRect.pivot = new Vector2(0.5f, 0f);
        nameRect.anchoredPosition = new Vector2(
            -(AmountWidth + LabelGap) * 0.5f,
            LabelInset
        );
        nameRect.sizeDelta = new Vector2(
            -(
                AmountWidth +
                LabelGap +
                LabelInset * 2f
            ),
            LabelHeight
        );

        RectTransform amountRect = amountText.rectTransform;
        amountRect.anchorMin = new Vector2(1f, 0f);
        amountRect.anchorMax = new Vector2(1f, 0f);
        amountRect.pivot = new Vector2(1f, 0f);
        amountRect.anchoredPosition = new Vector2(
            -LabelInset,
            LabelInset
        );
        amountRect.sizeDelta = new Vector2(AmountWidth, LabelHeight);
    }

    private void SetDragging(bool dragging)
    {
        isDragging = dragging;
        itemNameText.gameObject.SetActive(!dragging);
        amountText.gameObject.SetActive(!dragging);
        ApplyIconLayout(rectTransform.sizeDelta);
    }
}
