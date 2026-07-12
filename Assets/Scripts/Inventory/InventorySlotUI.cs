using TMPro;
using UnityEngine;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;

    public void SetItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.ItemData == null)
        {
            Clear();
            return;
        }

        itemNameText.text = inventoryItem.ItemData.DisplayName;
    }

    public void Clear()
    {
        itemNameText.text = string.Empty;
    }
}