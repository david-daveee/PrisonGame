using System;

[Serializable]
public class InventoryItem
{
    public ItemData ItemData { get; private set; }
    public int Amount { get; private set; }

    public InventoryItem(ItemData itemData, int amount = 1)
    {
        ItemData = itemData;
        Amount = amount;
    }

    public void AddAmount(int amount)
    {
        Amount += amount;
    }

    public void RemoveAmount(int amount)
    {
        Amount -= amount;
    }
}