using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour, IGridInventory
{
    [Header("Grid")]
    [SerializeField, Min(1)] private int width = 4;
    [SerializeField, Min(1)] private int height = 4;

    private GridInventory inventory;

    public InventoryGrid Grid => GetInventory().Grid;

    public event Action Changed
    {
        add => GetInventory().Changed += value;
        remove => GetInventory().Changed -= value;
    }

    private void Awake()
    {
        GetInventory();
    }

    public bool TryAddItem(InventoryItem inventoryItem)
    {
        bool added = GetInventory().TryAddItem(inventoryItem);

        if (added)
        {
            Debug.Log($"Added: {inventoryItem.ItemData.DisplayName}");
        }

        return added;
    }

    public bool TryRemoveItem(InventoryItem inventoryItem)
    {
        return GetInventory().TryRemoveItem(inventoryItem);
    }

    public bool HasItem(ItemId itemId, int amount = 1)
    {
        return GetInventory().HasItem(itemId, amount);
    }

    public IReadOnlyList<InventoryItem> GetItems()
    {
        return GetInventory().GetItems();
    }

    public void NotifyChanged()
    {
        GetInventory().NotifyChanged();
    }

    private GridInventory GetInventory()
    {
        inventory ??= new GridInventory(
            "Player Inventory",
            width,
            height
        );

        return inventory;
    }
}
