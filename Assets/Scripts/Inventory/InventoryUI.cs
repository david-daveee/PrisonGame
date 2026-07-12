using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryRoot;
    [SerializeField] private InventorySlotUI[] inventorySlots;

    private IInventory currentInventory;

    private void Awake()
    {
        Close();
    }

    public void Open(IInventory inventory)
    {
        currentInventory = inventory;
        inventoryRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        inventoryRoot.SetActive(false);
        currentInventory = null;
    }

    public void Refresh()
    {
        if (currentInventory == null)
        {
            return;
        }

        IReadOnlyList<InventoryItem> items = currentInventory.GetItems();

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < items.Count)
            {
                inventorySlots[i].SetItem(items[i]);
            }
            else
            {
                inventorySlots[i].Clear();
            }
        }
    }
}