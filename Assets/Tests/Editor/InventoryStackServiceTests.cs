using NUnit.Framework;
using UnityEngine;

public class InventoryStackServiceTests
{
    private ItemData stackableItem;
    private ItemData singleItem;

    [SetUp]
    public void SetUp()
    {
        stackableItem = CreateItem(ItemId.Cigarettes, 15);
        singleItem = CreateItem(ItemId.Book, 1);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(stackableItem);
        Object.DestroyImmediate(singleItem);
    }

    [Test]
    public void SplitWithinInventory_PreservesTotalAmount()
    {
        GridInventory inventory = new GridInventory("Test", 3, 1);
        InventoryItem sourceItem = new InventoryItem(stackableItem, 10);
        Assert.That(inventory.Grid.TryPlaceItem(sourceItem, Vector2Int.zero));
        InventoryPlacement source = inventory.Grid.GetPlacementAt(Vector2Int.zero);

        bool result = InventoryStackService.TrySplitWithinInventory(
            inventory,
            source,
            new Vector2Int(1, 0),
            false,
            3
        );

        Assert.That(result, Is.True);
        Assert.That(sourceItem.Amount, Is.EqualTo(7));
        Assert.That(
            inventory.Grid.GetPlacementAt(new Vector2Int(1, 0)).Item.Amount,
            Is.EqualTo(3)
        );
        Assert.That(GetTotalAmount(inventory), Is.EqualTo(10));
    }

    [Test]
    public void SplitIntoOccupiedCell_RollsBackCompletely()
    {
        GridInventory inventory = new GridInventory("Test", 2, 1);
        InventoryItem sourceItem = new InventoryItem(stackableItem, 10);
        InventoryItem blocker = new InventoryItem(singleItem);
        Assert.That(inventory.Grid.TryPlaceItem(sourceItem, Vector2Int.zero));
        Assert.That(
            inventory.Grid.TryPlaceItem(blocker, new Vector2Int(1, 0))
        );
        InventoryPlacement source = inventory.Grid.GetPlacementAt(Vector2Int.zero);

        bool result = InventoryStackService.TrySplitWithinInventory(
            inventory,
            source,
            new Vector2Int(1, 0),
            false,
            3
        );

        Assert.That(result, Is.False);
        Assert.That(sourceItem.Amount, Is.EqualTo(10));
        Assert.That(inventory.Grid.Placements.Count, Is.EqualTo(2));
        Assert.That(GetTotalAmount(inventory), Is.EqualTo(11));
    }

    [Test]
    public void SplitAndTransfer_PreservesAmountAcrossInventories()
    {
        GridInventory sourceInventory = new GridInventory("Source", 2, 1);
        GridInventory destinationInventory =
            new GridInventory("Destination", 2, 1);
        InventoryItem sourceItem = new InventoryItem(stackableItem, 10);
        Assert.That(
            sourceInventory.Grid.TryPlaceItem(sourceItem, Vector2Int.zero)
        );
        InventoryPlacement source =
            sourceInventory.Grid.GetPlacementAt(Vector2Int.zero);

        bool result = InventoryStackService.TrySplitAndTransfer(
            sourceInventory,
            source,
            destinationInventory,
            Vector2Int.zero,
            false,
            4
        );

        Assert.That(result, Is.True);
        Assert.That(sourceItem.Amount, Is.EqualTo(6));
        Assert.That(GetTotalAmount(sourceInventory), Is.EqualTo(6));
        Assert.That(GetTotalAmount(destinationInventory), Is.EqualTo(4));
    }

    [Test]
    public void MergeWithoutEnoughCapacity_IsRejected()
    {
        GridInventory sourceInventory = new GridInventory("Source", 1, 1);
        GridInventory destinationInventory =
            new GridInventory("Destination", 1, 1);
        InventoryItem sourceItem = new InventoryItem(stackableItem, 10);
        InventoryItem targetItem = new InventoryItem(stackableItem, 14);
        Assert.That(
            sourceInventory.Grid.TryPlaceItem(sourceItem, Vector2Int.zero)
        );
        Assert.That(
            destinationInventory.Grid.TryPlaceItem(
                targetItem,
                Vector2Int.zero
            )
        );

        bool result = InventoryStackService.TryMergeStack(
            sourceInventory,
            sourceInventory.Grid.GetPlacementAt(Vector2Int.zero),
            destinationInventory,
            destinationInventory.Grid.GetPlacementAt(Vector2Int.zero),
            3
        );

        Assert.That(result, Is.False);
        Assert.That(sourceItem.Amount, Is.EqualTo(10));
        Assert.That(targetItem.Amount, Is.EqualTo(14));
    }

    [Test]
    public void MergeWithEnoughCapacity_PreservesTotalAmount()
    {
        GridInventory sourceInventory = new GridInventory("Source", 1, 1);
        GridInventory destinationInventory =
            new GridInventory("Destination", 1, 1);
        InventoryItem sourceItem = new InventoryItem(stackableItem, 10);
        InventoryItem targetItem = new InventoryItem(stackableItem, 11);
        Assert.That(
            sourceInventory.Grid.TryPlaceItem(sourceItem, Vector2Int.zero)
        );
        Assert.That(
            destinationInventory.Grid.TryPlaceItem(
                targetItem,
                Vector2Int.zero
            )
        );

        bool result = InventoryStackService.TryMergeStack(
            sourceInventory,
            sourceInventory.Grid.GetPlacementAt(Vector2Int.zero),
            destinationInventory,
            destinationInventory.Grid.GetPlacementAt(Vector2Int.zero),
            4
        );

        Assert.That(result, Is.True);
        Assert.That(sourceItem.Amount, Is.EqualTo(6));
        Assert.That(targetItem.Amount, Is.EqualTo(15));
        Assert.That(
            GetTotalAmount(sourceInventory) +
            GetTotalAmount(destinationInventory),
            Is.EqualTo(21)
        );
    }

    [Test]
    public void SplittingEntireStack_IsRejected()
    {
        GridInventory inventory = new GridInventory("Test", 2, 1);
        InventoryItem sourceItem = new InventoryItem(stackableItem, 10);
        Assert.That(inventory.Grid.TryPlaceItem(sourceItem, Vector2Int.zero));

        bool result = InventoryStackService.TrySplitWithinInventory(
            inventory,
            inventory.Grid.GetPlacementAt(Vector2Int.zero),
            new Vector2Int(1, 0),
            false,
            10
        );

        Assert.That(result, Is.False);
        Assert.That(sourceItem.Amount, Is.EqualTo(10));
        Assert.That(inventory.Grid.Placements.Count, Is.EqualTo(1));
    }

    [Test]
    public void MaxStackOne_CannotBeSplit()
    {
        GridInventory inventory = new GridInventory("Test", 2, 1);
        InventoryItem item = new InventoryItem(singleItem);
        Assert.That(inventory.Grid.TryPlaceItem(item, Vector2Int.zero));

        Assert.That(
            InventoryStackService.CanSplit(
                inventory,
                inventory.Grid.GetPlacementAt(Vector2Int.zero)
            ),
            Is.False
        );
    }

    private static ItemData CreateItem(ItemId itemId, int maxStack)
    {
        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        item.ItemId = itemId;
        item.DisplayName = itemId.ToString();
        item.Size = Vector2Int.one;
        item.MaxStack = maxStack;
        return item;
    }

    private static int GetTotalAmount(IGridInventory inventory)
    {
        int total = 0;

        foreach (InventoryPlacement placement in inventory.Grid.Placements)
        {
            total += placement.Item.Amount;
        }

        return total;
    }
}
