using UnityEngine;

public static class InventoryStackService
{
    public static bool CanSplit(
        IGridInventory source,
        InventoryPlacement sourcePlacement)
    {
        return source != null &&
               sourcePlacement?.Item?.ItemData != null &&
               source.Grid.ContainsPlacement(sourcePlacement) &&
               sourcePlacement.Item.ItemData.MaxStack > 1 &&
               sourcePlacement.Item.Amount > 1;
    }

    public static bool TrySplitWithinInventory(
        IGridInventory inventory,
        InventoryPlacement sourcePlacement,
        Vector2Int destinationPosition,
        bool isRotated,
        int amount)
    {
        return TryCreateSplitStack(
            inventory,
            sourcePlacement,
            inventory,
            destinationPosition,
            isRotated,
            amount
        );
    }

    public static bool TrySplitAndTransfer(
        IGridInventory source,
        InventoryPlacement sourcePlacement,
        IGridInventory destination,
        Vector2Int destinationPosition,
        bool isRotated,
        int amount)
    {
        if (source == destination)
        {
            return TrySplitWithinInventory(
                source,
                sourcePlacement,
                destinationPosition,
                isRotated,
                amount
            );
        }

        return TryCreateSplitStack(
            source,
            sourcePlacement,
            destination,
            destinationPosition,
            isRotated,
            amount
        );
    }

    public static bool TryMergeStack(
        IGridInventory source,
        InventoryPlacement sourcePlacement,
        IGridInventory destination,
        InventoryPlacement targetPlacement,
        int amount)
    {
        if (!CanSplitAmount(source, sourcePlacement, amount) ||
            destination == null ||
            targetPlacement?.Item?.ItemData == null ||
            targetPlacement == sourcePlacement ||
            !destination.Grid.ContainsPlacement(targetPlacement) ||
            targetPlacement.Item.ItemData.ItemId !=
                sourcePlacement.Item.ItemData.ItemId ||
            !targetPlacement.Item.CanAddAmount(amount))
        {
            return false;
        }

        if (!targetPlacement.Item.TryAddAmount(amount))
        {
            return false;
        }

        if (!sourcePlacement.Item.TryRemoveAmount(amount))
        {
            targetPlacement.Item.TryRemoveAmount(amount);
            Debug.LogError(
                "Stack merge could not reduce its source and was rolled back."
            );
            return false;
        }

        NotifyChanged(source, destination);
        return true;
    }

    private static bool TryCreateSplitStack(
        IGridInventory source,
        InventoryPlacement sourcePlacement,
        IGridInventory destination,
        Vector2Int destinationPosition,
        bool isRotated,
        int amount)
    {
        if (!CanSplitAmount(source, sourcePlacement, amount) ||
            destination == null)
        {
            return false;
        }

        InventoryItem splitItem = new InventoryItem(
            sourcePlacement.Item.ItemData,
            amount
        );

        if (!destination.Grid.TryPlaceItem(
            splitItem,
            destinationPosition,
            isRotated))
        {
            return false;
        }

        if (!sourcePlacement.Item.TryRemoveAmount(amount))
        {
            destination.Grid.RemoveItem(splitItem);
            Debug.LogError(
                "Split stack was placed, but the source could not be reduced. " +
                "The placement was rolled back."
            );
            return false;
        }

        NotifyChanged(source, destination);
        return true;
    }

    private static bool CanSplitAmount(
        IGridInventory source,
        InventoryPlacement sourcePlacement,
        int amount)
    {
        return CanSplit(source, sourcePlacement) &&
               sourcePlacement.Item.CanRemoveAmount(amount);
    }

    private static void NotifyChanged(
        IGridInventory source,
        IGridInventory destination)
    {
        source.NotifyChanged();

        if (destination != source)
        {
            destination.NotifyChanged();
        }
    }
}
