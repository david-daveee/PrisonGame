# Dev Log

## 2026-07-15

### Completed

- Moved player inventory contents from a separate list into `InventoryGrid` placements.
- Connected `WorldItem` pickup to grid capacity.
- Made stack pickup atomic: a failed pickup leaves both the world item and inventory unchanged.
- Added drag and drop with bounds and overlap validation.
- Added `R` item rotation for the placement under the cursor, with validation.
- Connected the inventory window to the player's live grid.
- Changed `InventoryDebug` to seed `PlayerInventory` instead of creating an unrelated grid.
- Replaced the square crowbar icon with a transparent `1:3` portrait sprite matching its grid footprint.
- Centered dragged items under the cursor and made in-drag rotation preserve that center point.
- Made drag movement transactional: the placement is detached while held, can rotate freely, and restores its original position and rotation after an invalid drop.
- Added reusable `GridInventory` storage shared by players and containers.
- Added Inspector-configurable `ContainerInventory` interaction.
- Added a two-panel container window: container left, player inventory right.
- Configured `ShelfTableSingle` as the first 4x4 bedside cabinet container.
- Connected the active bedside drawer interaction to its `ContainerInventory`, avoiding the competing `IInteractable` hierarchy issue.
- Made closing the container UI trigger the drawer's closing animation.
- Added bidirectional drag transfer between player and container grids.
- Added source ownership tracking for detached placements and atomic transfer validation through `InventoryTransferService`.
- Added a shared topmost `DragLayer` so items remain visible above both inventory panels during cross-grid dragging.
- Removed direct interaction from `ContainerInventory` so cabinet-body ray hits cannot bypass physical animation.
- Added an independent 4x4 `Left Cabinet` inventory behind `LeftDoor`, separate from the `LeftShelf` inventory, while preserving its `10°` opening angle.
- Made closing the container UI close an inventory-connected `Door`, matching the existing drawer behavior.
- Added reusable `ContainerInteractable` for arbitrary static or animated world containers.
- Added optional Animator `Open`/`Close` triggers and Inspector events without requiring animation.
- Connected the new 2x1 bed container to `ContainerInteractable` as the first non-animated example.
- Moved the `LeftShelf` storage component onto the drawer GameObject so its name, description, grid size and starting items are editable beside the drawer setup.
- Made `ContainerInteractable` automatically require and add `ContainerInventory` for new ordinary containers.
- Added optional `AudioSource`, open clip and close clip fields to generic container interaction.

### Current limitations

- No drop from inventory back into the world yet.
- No stack splitting or contraband marker yet.
- Play Mode interaction still needs a final manual pass in Unity.

### Exact next step

Test opening from the left shelf and `LeftDoor` at several view angles, then test bidirectional transfer, full-destination rollback and mechanism closing through Tab in Play Mode. Then add distance-based auto-close and define the multiplayer request/response boundary.

### Recommended commit

```text
feat: add reusable grid containers and two-panel inventory UI
```

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
