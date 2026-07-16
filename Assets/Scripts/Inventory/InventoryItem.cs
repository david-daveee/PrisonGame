using System;

[Serializable]
public class InventoryItem
{
    public ItemData ItemData { get; private set; }
    public int Amount { get; private set; }

    public InventoryItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null)
        {
            throw new ArgumentNullException(nameof(itemData));
        }

        if (amount < 1 || amount > itemData.MaxStack)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                $"Amount must be between 1 and {itemData.MaxStack}."
            );
        }

        ItemData = itemData;
        Amount = amount;
    }

    public bool CanAddAmount(int amount)
    {
        return amount > 0 && Amount + amount <= ItemData.MaxStack;
    }

    public bool CanRemoveAmount(int amount)
    {
        return amount > 0 && Amount - amount >= 1;
    }

    public bool TryAddAmount(int amount)
    {
        if (!CanAddAmount(amount))
        {
            return false;
        }

        Amount += amount;
        return true;
    }

    public bool TryRemoveAmount(int amount)
    {
        if (!CanRemoveAmount(amount))
        {
            return false;
        }

        Amount -= amount;
        return true;
    }
}
