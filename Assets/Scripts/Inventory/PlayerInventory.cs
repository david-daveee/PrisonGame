using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour, IInventory
{
    private readonly List<InventoryItem> items = new List<InventoryItem>();
    
    public bool TryAddItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null ||
            inventoryItem.ItemData == null ||
            inventoryItem.Amount <= 0)
        {
            return false;
        }

        int remainingAmount = inventoryItem.Amount;
        ItemData itemData = inventoryItem.ItemData;

        // Сначала заполняем уже существующие неполные стаки.
        foreach (InventoryItem existingItem in items)
        {
            if (existingItem.ItemData.ItemId != itemData.ItemId)
            {
                continue;
            }

            int freeSpace = itemData.MaxStack - existingItem.Amount;

            if (freeSpace <= 0)
            {
                continue;
            }

            int amountToAdd = Mathf.Min(freeSpace, remainingAmount);

            existingItem.AddAmount(amountToAdd);
            remainingAmount -= amountToAdd;

            if (remainingAmount == 0)
            {
                Debug.Log($"Added: {itemData.DisplayName}");
                return true;
            }
        }

        // Если предметы ещё остались — создаём новые стаки.
        while (remainingAmount > 0)
        {
            int stackAmount = Mathf.Min(itemData.MaxStack, remainingAmount);

            items.Add(new InventoryItem(itemData, stackAmount));
            remainingAmount -= stackAmount;
        }

        Debug.Log($"Added: {itemData.DisplayName}");
        return true;
    }

    public bool TryRemoveItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null)
        {
            return false;
        }

        return items.Remove(inventoryItem);
    }

    public bool HasItem(ItemId itemId, int amount = 1)
    {
        if (amount <= 0)
        {
            return true;
        }

        int totalAmount = 0;

        foreach (InventoryItem inventoryItem in items)
        {
            if (inventoryItem.ItemData.ItemId != itemId)
            {
                continue;
            }

            totalAmount += inventoryItem.Amount;

            if (totalAmount >= amount)
            {
                return true;
            }
        }

        return false;
    }

    public IReadOnlyList<InventoryItem> GetItems()
    {
        return items;
    }
}