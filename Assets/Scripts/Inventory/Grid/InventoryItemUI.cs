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
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;

    private InventoryGridUI owner;
    private InventoryPlacement placement;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image visualIcon;

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
        amountText.text = item.Amount > 1
            ? $"x{item.Amount}"
            : string.Empty;
    }

    public void ApplyLayout(Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        RectTransform iconRect = visualIcon.rectTransform;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = placement.IsRotated
            ? new Vector2(size.y, size.x)
            : size;
        iconRect.localEulerAngles = placement.IsRotated
            ? new Vector3(0f, 0f, -90f)
            : Vector3.zero;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        PointerOffset = GetCenterOffset(rectTransform.sizeDelta);
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
        owner.BeginDrag(this);
        UpdateDragPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateDragPosition(eventData);
    }

    public void ApplyRotationAroundCenter(Vector2 size)
    {
        Vector2 currentCenter =
            rectTransform.anchoredPosition +
            GetCenterOffset(rectTransform.sizeDelta);
        Vector2 newCenterOffset = GetCenterOffset(size);

        ApplyLayout(currentCenter - newCenterOffset, size);
        PointerOffset = newCenterOffset;
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
}
