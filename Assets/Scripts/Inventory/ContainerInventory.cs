using System;
using System.Collections.Generic;
using UnityEngine;

public class ContainerInventory : MonoBehaviour,
    IGridInventory
{
    [Header("Container")]
    [SerializeField] private string containerName = "Container";
    [SerializeField, TextArea] private string description;

    [Header("Grid")]
    [SerializeField, Min(1)] private int width = 4;
    [SerializeField, Min(1)] private int height = 4;

    [Header("Starting Items")]
    [SerializeField] private ItemData[] startingItems =
        Array.Empty<ItemData>();

    private GridInventory inventory;

    public event Action Closed;

    public string ContainerName => containerName;
    public string Description => description;
    public InventoryGrid Grid => GetInventory().Grid;

    public event Action Changed
    {
        add => GetInventory().Changed += value;
        remove => GetInventory().Changed -= value;
    }

    private void Awake()
    {
        GetInventory();

        foreach (ItemData itemData in startingItems)
        {
            if (itemData == null)
            {
                continue;
            }

            if (!TryAddItem(new InventoryItem(itemData)))
            {
                Debug.LogWarning(
                    $"No room for {itemData.DisplayName} in {containerName}.",
                    this
                );
            }
        }
    }

    public void OpenFor(PlayerInteractor interactor)
    {
        if (interactor == null)
        {
            return;
        }

        PlayerInventoryInput inventoryInput =
            interactor.GetInventoryInput();

        if (inventoryInput != null)
        {
            inventoryInput.OpenContainer(this);
        }
    }

    public bool TryAddItem(InventoryItem inventoryItem)
    {
        return GetInventory().TryAddItem(inventoryItem);
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

    public void NotifyClosed()
    {
        Closed?.Invoke();
    }

    private GridInventory GetInventory()
    {
        inventory ??= new GridInventory(
            containerName,
            width,
            height
        );

        return inventory;
    }
}
