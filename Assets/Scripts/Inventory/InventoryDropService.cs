using UnityEngine;

public static class InventoryDropService
{
    public static bool TryDropDetachedPlacement(
        IGridInventory source,
        InventoryPlacement placement,
        WorldItemDropper dropper)
    {
        if (source == null ||
            placement?.Item == null ||
            dropper == null ||
            !source.Grid.ContainsDetachedPlacement(placement))
        {
            return false;
        }

        if (!dropper.TrySpawn(placement.Item, out WorldItem worldItem))
        {
            return false;
        }

        if (!source.Grid.DiscardDetachedPlacement(placement))
        {
            dropper.DestroySpawned(worldItem);
            Debug.LogError(
                "World item was spawned, but its detached inventory " +
                "placement could not be finalized. The spawn was rolled back."
            );
            return false;
        }

        source.NotifyChanged();
        return true;
    }

    public static bool TryDropSplit(
        IGridInventory source,
        InventoryPlacement sourcePlacement,
        int amount,
        WorldItemDropper dropper)
    {
        if (!CanSplit(source, sourcePlacement, amount) || dropper == null)
        {
            return false;
        }

        InventoryItem splitItem = new InventoryItem(
            sourcePlacement.Item.ItemData,
            amount
        );

        if (!dropper.TrySpawn(splitItem, out WorldItem worldItem))
        {
            return false;
        }

        if (!sourcePlacement.Item.TryRemoveAmount(amount))
        {
            dropper.DestroySpawned(worldItem);
            Debug.LogError(
                "Split world drop could not reduce the source stack. " +
                "The spawned object was rolled back."
            );
            return false;
        }

        source.NotifyChanged();
        return true;
    }

    private static bool CanSplit(
        IGridInventory source,
        InventoryPlacement sourcePlacement,
        int amount)
    {
        return source != null &&
               sourcePlacement?.Item != null &&
               source.Grid.ContainsPlacement(sourcePlacement) &&
               sourcePlacement.Item.ItemData.MaxStack > 1 &&
               sourcePlacement.Item.CanRemoveAmount(amount);
    }
}
