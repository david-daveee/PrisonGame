using UnityEngine;

public class InventoryPlacement
{
    public InventoryItem Item { get; }
    public Vector2Int Position { get; private set; }
    public bool IsRotated { get; private set; }

    public InventoryPlacement(
        InventoryItem item,
        Vector2Int position,
        bool isRotated = false)
    {
        Item = item;
        Position = position;
        IsRotated = isRotated;
    }

    public Vector2Int GetCurrentSize()
    {
        Vector2Int size = Item.ItemData.Size;

        return IsRotated
            ? new Vector2Int(size.y, size.x)
            : size;
    }

    public void SetPosition(Vector2Int position)
    {
        Position = position;
    }

    public void SetRotation(bool isRotated)
    {
        IsRotated = isRotated;
    }
}