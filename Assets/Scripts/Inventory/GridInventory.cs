using System;
using System.Collections.Generic;
using UnityEngine;

public class GridInventory : IGridInventory
{
    public InventoryGrid Grid { get; }
    public event Action Changed;

    public GridInventory(string name, int width, int height)
    {
        Grid = new InventoryGrid(name, width, height);
    }

    public bool TryAddItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null ||
            inventoryItem.ItemData == null ||
            inventoryItem.Amount <= 0 ||
            inventoryItem.ItemData.MaxStack < 1 ||
            inventoryItem.ItemData.Size.x < 1 ||
            inventoryItem.ItemData.Size.y < 1)
        {
            return false;
        }

        ItemData itemData = inventoryItem.ItemData;
        List<InventoryItem> existingStacks =
            GetMatchingStacks(itemData.ItemId);
        int stackCapacity = 0;

        foreach (InventoryItem existingItem in existingStacks)
        {
            stackCapacity += Mathf.Max(
                0,
                itemData.MaxStack - existingItem.Amount
            );
        }

        int amountForExistingStacks = Mathf.Min(
            inventoryItem.Amount,
            stackCapacity
        );
        int amountNeedingCells =
            inventoryItem.Amount - amountForExistingStacks;
        List<InventoryItem> addedStacks = new List<InventoryItem>();

        while (amountNeedingCells > 0)
        {
            int stackAmount = Mathf.Min(
                itemData.MaxStack,
                amountNeedingCells
            );
            InventoryItem newStack =
                amountNeedingCells == inventoryItem.Amount &&
                stackAmount == inventoryItem.Amount
                    ? inventoryItem
                    : new InventoryItem(itemData, stackAmount);

            if (!Grid.TryPlaceItemInAnyOrientation(newStack))
            {
                foreach (InventoryItem addedStack in addedStacks)
                {
                    Grid.RemoveItem(addedStack);
                }

                return false;
            }

            addedStacks.Add(newStack);
            amountNeedingCells -= stackAmount;
        }

        foreach (InventoryItem existingItem in existingStacks)
        {
            int freeSpace = itemData.MaxStack - existingItem.Amount;
            int amountToAdd = Mathf.Min(
                freeSpace,
                amountForExistingStacks
            );

            existingItem.AddAmount(amountToAdd);
            amountForExistingStacks -= amountToAdd;

            if (amountForExistingStacks == 0)
            {
                break;
            }
        }

        Changed?.Invoke();
        return true;
    }

    public bool TryRemoveItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || !Grid.RemoveItem(inventoryItem))
        {
            return false;
        }

        Changed?.Invoke();
        return true;
    }

    public bool HasItem(ItemId itemId, int amount = 1)
    {
        if (amount <= 0)
        {
            return true;
        }

        int totalAmount = 0;

        foreach (InventoryPlacement placement in Grid.Placements)
        {
            InventoryItem inventoryItem = placement.Item;

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
        List<InventoryItem> items = new List<InventoryItem>();

        foreach (InventoryPlacement placement in Grid.Placements)
        {
            items.Add(placement.Item);
        }

        return items;
    }

    public void NotifyChanged()
    {
        Changed?.Invoke();
    }

    private List<InventoryItem> GetMatchingStacks(ItemId itemId)
    {
        List<InventoryItem> matchingStacks = new List<InventoryItem>();

        foreach (InventoryPlacement placement in Grid.Placements)
        {
            InventoryItem item = placement.Item;

            if (item.ItemData.ItemId == itemId)
            {
                matchingStacks.Add(item);
            }
        }

        return matchingStacks;
    }
}
