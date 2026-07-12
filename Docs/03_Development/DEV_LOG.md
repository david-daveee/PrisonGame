# Dev Log

## 2026-07-12

### Completed

- Created `IInventory`.
- Made `PlayerInventory` implement it.
- Created `InventoryUI`, `InventorySlotUI`, `PlayerInventoryInput`, `PlayerInputHandler`, `PlayerUIStateController`, `InventoryItem` and `ItemCategory`.
- Added `Player/Inventory` action bound to Tab.
- Migrated inventory opening to Unity Input System.
- Added UI Mode.
- Changed `Interact()` to `Interact(PlayerInteractor interactor)`.
- Removed random PlayerInventory lookup.
- Split Canvas into Gameplay UI and independent windows.
- Started migration `List<ItemData> → List<InventoryItem>`.
- Added initial stack logic.

### Current issue

The item is added successfully:

```text
Added: Screwdriver
```

but the name no longer appears in the inventory UI.

Check:

1. `Inventory Slots` array references;
2. `Item Name Text` references;
3. current UI method signatures;
4. Inspector references lost after field renaming.

### Exact next step

Restore `Screwdriver` display before adding new inventory features.

Then:

1. finish `WorldItem` under `InventoryItem`;
2. add icon display;
3. add amount display;
4. add category background;
5. design grid placement;
6. implement drag and drop;
7. create the first container.

### Recommended commit

```text
docs: add project development documentation
```
