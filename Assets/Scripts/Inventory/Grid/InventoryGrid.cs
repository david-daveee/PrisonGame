using System.Collections.Generic;
using UnityEngine;

public class InventoryGrid
{
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }

    private readonly List<InventoryPlacement> placements = new();
    private readonly HashSet<InventoryPlacement> detachedPlacements = new();

    public IReadOnlyList<InventoryPlacement> Placements => placements;

    public InventoryGrid(string name, int width, int height)
    {
        Name = name;
        Width = Mathf.Max(1, width);
        Height = Mathf.Max(1, height);
    }

    public bool CanPlaceItem(
        InventoryItem item,
        Vector2Int position,
        bool isRotated = false,
        InventoryPlacement ignoredPlacement = null)
    {
        if (item == null ||
            item.ItemData == null ||
            item.ItemData.Size.x < 1 ||
            item.ItemData.Size.y < 1)
        {
            return false;
        }

        Vector2Int size = item.ItemData.Size;

        if (isRotated)
        {
            size = new Vector2Int(size.y, size.x);
        }

        if (position.x < 0 ||
            position.y < 0 ||
            position.x + size.x > Width ||
            position.y + size.y > Height)
        {
            return false;
        }

        foreach (InventoryPlacement placement in placements)
        {
            if (placement == ignoredPlacement)
            {
                continue;
            }

            if (RectanglesOverlap(
                position,
                size,
                placement.Position,
                placement.GetCurrentSize()))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryPlaceItem(
        InventoryItem item,
        Vector2Int position,
        bool isRotated = false)
    {
        if (!CanPlaceItem(item, position, isRotated))
        {
            return false;
        }

        InventoryPlacement placement =
            new InventoryPlacement(item, position, isRotated);

        placements.Add(placement);
        return true;
    }

    public bool TryPlaceItem(InventoryItem item)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector2Int position = new Vector2Int(x, y);

                if (TryPlaceItem(item, position))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryPlaceItemInAnyOrientation(InventoryItem item)
    {
        if (TryPlaceItem(item))
        {
            return true;
        }

        if (item?.ItemData == null ||
            item.ItemData.Size.x == item.ItemData.Size.y)
        {
            return false;
        }

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (TryPlaceItem(item, new Vector2Int(x, y), true))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryMovePlacement(
        InventoryPlacement placement,
        Vector2Int position)
    {
        if (placement == null || !placements.Contains(placement))
        {
            return false;
        }

        if (!CanPlaceItem(
            placement.Item,
            position,
            placement.IsRotated,
            placement))
        {
            return false;
        }

        placement.SetPosition(position);
        return true;
    }

    public bool TryDetachPlacementForMove(
        InventoryPlacement placement)
    {
        if (placement == null)
        {
            return false;
        }

        if (!placements.Remove(placement))
        {
            return false;
        }

        detachedPlacements.Add(placement);
        return true;
    }

    public bool TryAttachDetachedPlacement(
        InventoryPlacement placement,
        Vector2Int position)
    {
        if (placement == null ||
            !detachedPlacements.Contains(placement))
        {
            return false;
        }

        if (!CanPlaceItem(
            placement.Item,
            position,
            placement.IsRotated))
        {
            return false;
        }

        placement.SetPosition(position);
        detachedPlacements.Remove(placement);
        placements.Add(placement);
        return true;
    }

    public bool RotateDetachedPlacement(
        InventoryPlacement placement)
    {
        if (placement == null ||
            !detachedPlacements.Contains(placement))
        {
            return false;
        }

        placement.SetRotation(!placement.IsRotated);
        return true;
    }

    public bool TryTransferDetachedPlacementTo(
        InventoryGrid destination,
        InventoryPlacement placement,
        Vector2Int destinationPosition)
    {
        if (destination == null ||
            destination == this ||
            placement == null ||
            !detachedPlacements.Contains(placement))
        {
            return false;
        }

        if (!destination.CanPlaceItem(
            placement.Item,
            destinationPosition,
            placement.IsRotated))
        {
            return false;
        }

        detachedPlacements.Remove(placement);
        placement.SetPosition(destinationPosition);
        destination.placements.Add(placement);
        return true;
    }

    public bool TryRotatePlacement(InventoryPlacement placement)
    {
        if (placement == null || !placements.Contains(placement))
        {
            return false;
        }

        bool isRotated = !placement.IsRotated;

        if (!CanPlaceItem(
            placement.Item,
            placement.Position,
            isRotated,
            placement))
        {
            return false;
        }

        placement.SetRotation(isRotated);
        return true;
    }

    public bool RemoveItem(InventoryItem item)
    {
        if (item == null)
        {
            return false;
        }

        InventoryPlacement placement = placements.Find(
            currentPlacement => currentPlacement.Item == item
        );

        if (placement == null)
        {
            return false;
        }

        placements.Remove(placement);
        return true;
    }

    public InventoryPlacement GetPlacementAt(Vector2Int position)
    {
        foreach (InventoryPlacement placement in placements)
        {
            Vector2Int placementSize = placement.GetCurrentSize();

            bool isInside =
                position.x >= placement.Position.x &&
                position.x < placement.Position.x + placementSize.x &&
                position.y >= placement.Position.y &&
                position.y < placement.Position.y + placementSize.y;

            if (isInside)
            {
                return placement;
            }
        }

        return null;
    }
    private static bool RectanglesOverlap(
        Vector2Int firstPosition,
        Vector2Int firstSize,
        Vector2Int secondPosition,
        Vector2Int secondSize)
    {
        return firstPosition.x < secondPosition.x + secondSize.x &&
               firstPosition.x + firstSize.x > secondPosition.x &&
               firstPosition.y < secondPosition.y + secondSize.y &&
               firstPosition.y + firstSize.y > secondPosition.y;
    }
}
