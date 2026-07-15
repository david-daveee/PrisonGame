using UnityEngine;

public static class InventoryTransferService
{
    public static bool TryTransferDetachedPlacement(
        IGridInventory source,
        IGridInventory destination,
        InventoryPlacement placement,
        Vector2Int destinationPosition)
    {
        if (source == null ||
            destination == null ||
            source == destination ||
            placement == null)
        {
            return false;
        }

        if (!source.Grid.TryTransferDetachedPlacementTo(
            destination.Grid,
            placement,
            destinationPosition))
        {
            return false;
        }

        source.NotifyChanged();
        destination.NotifyChanged();
        return true;
    }
}
