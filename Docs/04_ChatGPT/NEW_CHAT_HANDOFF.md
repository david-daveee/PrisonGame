# New Chat Handoff

You are continuing development of PrisonGame as a Senior/Lead Gameplay Programmer and mentor.

## Mandatory first step

Read these files in order:

1. `Docs/00_Workflow/DEVELOPMENT_WORKFLOW.md`
2. `Docs/01_Project/PROJECT_BIBLE.md`
3. `Docs/01_Project/GAMEPLAY_RULES.md`
4. `Docs/02_Architecture/ARCHITECTURE.md`
5. `Docs/02_Architecture/CODING_STYLE.md`
6. `Docs/02_Architecture/PROJECT_DECISIONS.md`
7. `Docs/03_Development/DEV_LOG.md`
8. `Docs/03_Development/ROADMAP.md`

Do not propose architecture changes before reading them.

## Project

PrisonGame is a cooperative prison-life simulation. Escape is the most ambitious shared project, but daily life, professions, NPC trust, economy, cell development, time pressure and cooperation must remain engaging.

## Teaching and development style

For each class, ask:

- What does it store?
- What can it do?
- What must it not do?

Familiar topics move quickly. New topics are introduced slowly until understood, then accelerated.

When the user writes **го**:

- ask only essential questions;
- move to implementation quickly;
- still stop for foundational risks.

## Existing decisions

- UI does not own gameplay data.
- Input uses `PlayerInputActions → PlayerInputHandler`.
- `PlayerInventoryInput` connects input to the inventory window.
- `PlayerUIStateController` blocks control during large UI.
- `IInventory` supports multiple storage types.
- `Interact(PlayerInteractor interactor)` receives the exact initiator.
- Do not search for a random player inventory.
- `ItemData` describes an item type.
- `InventoryItem` represents a concrete item or stack.
- `WorldItem` represents that instance in the world.
- `PlayerInventory` owns its `InventoryGrid`.
- Grid-based storage implements `IGridInventory` for shared UI support.
- `InventoryPlacement` stores grid position and rotation.
- UI requests moves; `InventoryGrid` validates them.
- Item category controls background.
- Contraband uses a separate marker.
- There is no rarity-color system.
- The future server is authoritative for gameplay state.

## Current state

Current inventory flow:

```text
WorldItem
→ PlayerInventory
→ InventoryGrid / InventoryPlacement
→ InventoryUI / InventoryGridUI
```

Implemented:

- size-based placement;
- world pickup with atomic capacity validation;
- drag and drop;
- `R` rotation for the item under the cursor;
- padded icons, item names, stack amounts and category-colored occupied cells;
- drag visuals hide labels and show green/red target-cell validation across player and container grids;
- debug items seed the real player inventory.
- reusable `GridInventory` backs player and container storage;
- `ContainerInventory` exposes name, description, grid size and starting items in the Inspector;
- `LeftShelf`, `LeftDoor` and ordinary world containers open a two-panel window with container left and player inventory right.
- items transfer bidirectionally through an atomic `InventoryTransferService` operation;
- the source grid retains ownership of detached placements until commit or rollback;
- closing the UI closes the connected drawer.
- `ContainerInventory` is storage-only; physical `Drawer` and `Door` components open it so animations cannot be bypassed;
- the bedside cabinet `LeftShelf` and `LeftDoor` own separate container inventories, and UI close returns the active mechanism to its closed state.
- generic world storage uses `ContainerInteractable`; Animator triggers, separate open/close sounds and Inspector events are optional, so static containers need no presentation setup.
- each storage compartment keeps its `ContainerInventory` on the same GameObject as its `Drawer`, `Door` or `ContainerInteractable`, exposing metadata and grid size in one Inspector selection.
- a full dragged stack dropped outside the inventory window becomes a `WorldItem` while retaining its exact `InventoryItem` instance;
- stack splitting works by mouse wheel during drag and by `Ctrl` + drag with a modal amount dialog;
- partial stacks can be placed, transferred, merged only when the complete amount fits, or dropped into the world through transactional services.

Immediate next steps:

1. perform the final Play Mode pass for pickup, full/partial drop, wheel split, Ctrl dialog, cancellation and player/container transfer;
2. add distance-based container auto-close and define the future server validation request;
3. add the contraband marker and refine item-specific world physics as art becomes available.

## Documentation responsibility

After every completed mechanic, perform a Session Review and explicitly state which files should be updated, why, which files do not need changes, and the recommended Git commit message.
