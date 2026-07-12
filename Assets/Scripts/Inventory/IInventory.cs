using System.Collections.Generic;

public interface IInventory
{
    bool TryAddItem(InventoryItem inventoryItem);
    bool TryRemoveItem(InventoryItem inventoryItem);

    bool HasItem(ItemId itemId, int amount = 1);

    IReadOnlyList<InventoryItem> GetItems();
}