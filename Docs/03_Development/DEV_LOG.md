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
- Configured `DoorContainer` as the first Animator-driven hinged container: `Pivot` rotates from 0 to 10 degrees over 0.25 seconds and owns its mesh and colliders.
- Configured `DrawerContainer` with its own Animator Controller: `Pivot` slides on local X from 0 to -0.3 over 0.25 seconds, with both colliders moving beneath it.
- Added a small inset around grid-item icons, bottom item names and bottom-right amounts generated from the live item data.
- Reserved a fixed gap between item names and amounts so long labels cannot overlap `x1` on 1x1 items.
- Reserved a separate bottom label strip inside every placement and fitted the icon into the remaining area, preventing item art from sitting beneath its name or amount.
- Added Inspector-configurable colors for every `ItemCategory`, applied to the cells occupied by each placement.
- Added live green/red placement previews driven by `InventoryGrid.CanPlaceItem`; drag visuals now hide name and amount until drop.
- Added atomic full-stack drop outside the inventory window, preserving the concrete `InventoryItem` instance through `WorldItem.Initialize`.
- Added a player-owned `WorldItemDropper` with an Inspector-configured drop point, ground probe and safe forward offset.
- Added separate 3D world-item prefabs for Book, Cigarettes, Crowbar, Screwdriver and Soap and connected each `ItemData` through its `World Prefab` field.
- Added guarded stack amount operations and model-level split, cross-inventory partial transfer and compatible-stack merge transactions.
- Added mouse-wheel split drag with a temporary amount preview, rotation, target-cell feedback and complete cancellation safety.
- Added `Ctrl` + drag splitting through a reusable slider/input dialog that revalidates the remembered destination on Apply.
- Added partial-stack world dropping and editor tests for amount conservation, occupied destinations, cross-inventory splitting, full merge targets and `MaxStack = 1`.
- Added full-stack merging through ordinary drag inside one grid and across player/container grids, with green/red capacity preview and atomic detached-placement finalization.

### Current limitations

- No contraband marker yet.
- Dropped items use item-specific 3D prefabs; physical throw impulses are postponed.
- Play Mode interaction still needs a final manual pass in Unity.

### Exact next step

Run the documented Play Mode pass for pickup, full/partial world drop, both split controls, cancellation and player/container transfers. Then add distance-based container auto-close, contraband markers and define the multiplayer request/response boundary.

### Recommended commit

```text
feat: add atomic world drop and stack splitting
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
