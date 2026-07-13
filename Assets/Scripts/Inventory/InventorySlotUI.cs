using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text amountText;

    private InventoryItem currentItem;

    public void SetItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.ItemData == null)
        {
            Clear();
            return;
        }

        currentItem = inventoryItem;

        itemIcon.sprite = inventoryItem.ItemData.Icon;
        itemIcon.enabled = inventoryItem.ItemData.Icon != null;

        itemNameText.text = inventoryItem.ItemData.DisplayName;

        amountText.text = inventoryItem.Amount > 1
            ? $"x{inventoryItem.Amount}"
            : string.Empty;
    }

    public void Clear()
    {
        currentItem = null;

        itemIcon.sprite = null;
        itemIcon.enabled = false;

        itemNameText.text = string.Empty;
        amountText.text = string.Empty;
    }
}