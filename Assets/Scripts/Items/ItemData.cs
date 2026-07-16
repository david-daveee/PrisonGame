using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    public ItemId ItemId;
    public string DisplayName;

    public Sprite Icon;

    public ItemCategory Category;

    public Vector2Int Size = Vector2Int.one;

    [Min(1)]
    public int MaxStack = 1;

    [Header("World Representation")]
    [SerializeField] private WorldItem worldPrefab;

    public WorldItem WorldPrefab => worldPrefab;
}
