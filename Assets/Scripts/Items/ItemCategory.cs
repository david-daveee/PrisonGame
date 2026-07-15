using System;
using UnityEngine;

public enum ItemCategory
{
    Tool,
    Food,
    Material,
    Medicine,
    Clothing,
    Valuable
}

[Serializable]
public struct ItemCategoryColor
{
    public ItemCategory Category;
    public Color Color;

    public ItemCategoryColor(ItemCategory category, Color color)
    {
        Category = category;
        Color = color;
    }
}
