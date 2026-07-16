# PrisonGame

Cooperative prison-life simulation where escape is the players' most ambitious shared project, while daily life, professions, relationships, economy and cell development remain engaging on their own.

## Current status

Early gameplay prototype with a functional size-based inventory and reusable world containers.

Implemented:

- first-person movement, camera and raycast interaction;
- doors, drawers, vents and removable bolts;
- concrete `InventoryItem` instances backed by `ItemData` definitions;
- grid placement with per-item width, height and rotation;
- world-item pickup with atomic capacity validation;
- drag and drop, invalid-drop rollback and `R` rotation;
- padded transparent item icons, bottom item names, stack amounts and Inspector-configurable category colors;
- Tarkov-style green/red occupied-cell preview while dragging, with labels hidden until drop;
- dropping a full stack outside the inventory window back into the world without replacing its `InventoryItem` instance;
- stack splitting by mouse wheel during drag or by a `Ctrl` + drag amount dialog;
- two-panel player/container UI;
- bidirectional atomic item transfer;
- independent inventories for separate cabinet compartments;
- reusable static or animated containers with optional audio;
- UI mode that blocks player control while a large window is open;
- architecture seams for future server-authoritative multiplayer.

Current focus:

- final Play Mode validation of the complete pickup/drop/split/container loop;
- contraband markers;
- distance-based container closing;
- defining multiplayer validation requests.

## Controls

- `E` — interact with a world item, door, drawer or container;
- `Tab` — open or close inventory UI;
- hold left mouse button — drag an inventory item;
- mouse wheel while dragging a stack — choose a partial amount;
- `Ctrl` + drag a stack — open the amount dialog after choosing a destination;
- `R` — rotate the hovered or currently dragged item.

Dropping outside the dark inventory window places the held item in front of the player. Dropping in the gap between grids is treated as a cancelled move, so an accidental release cannot throw the item away.

## Item authoring

Every `ItemData` asset must reference a `World Prefab` before that item can be dropped. Book, Cigarettes, Crowbar, Screwdriver and Soap now use separate 3D prefabs built from their FBX models. A new prefab must keep `WorldItem` and an interaction collider on its root.

The player owns `WorldItemDropper`. Its `Drop Point` should be the camera or a child transform facing forward; `Forward Distance`, `Floor Offset`, `Ground Probe Distance`, `Ground Layers` and `Blocking Layers` control safe placement. A forward ray prevents choosing a point behind a wall, a downward ray finds the surface, and the real lower collider/renderer bound is aligned to it. Collider penetration is resolved horizontally before the transaction commits. If no surface or free volume is found, the drop is cancelled and the inventory item is restored. `InventoryUI` references that dropper and the `StackSplitDialog` prefab.

## Container authoring

Every physically separate compartment owns its own `ContainerInventory`. Keep that component on the same GameObject as the component that opens it.

### Ordinary container

1. Add `ContainerInteractable`. Unity automatically adds `ContainerInventory`.
2. Configure `Container Name`, `Description`, `Width`, `Height` and `Starting Items` in `ContainerInventory`.
3. Add a Collider to the same GameObject or one of its children.
4. Leave `Animator` empty for a static container.
5. Optionally assign an `AudioSource`, `Open Sound` and `Close Sound`.

For an animated container, assign an Animator whose trigger parameters match `Open Trigger` and `Close Trigger` (defaults: `Open` and `Close`). For a door, animate a child `Pivot` placed at the physical hinge: the closed key is normally `Y = 0`, and the open key is the desired angle. For a sliding drawer, animate the Pivot's local position instead; `DrawerContainer` currently moves local X from `0` to `-0.3` over 0.25 seconds. Keep the visible mesh and moving colliders below that Pivot. `On Opened` and `On Closed` are optional hooks for custom presentation code, lights, particles or other effects.

For world audio, disable `Play On Awake` and `Loop` on the AudioSource and set `Spatial Blend` to `1` for 3D sound. The AudioSource clip itself may stay empty because `ContainerInteractable` uses `PlayOneShot` with the assigned open and close clips.

```text
Container
├── ContainerInventory
├── ContainerInteractable
└── InteractionCollider
    └── Collider
```

### Drawer or door compartment

Use the specialized `Drawer` or `Door` component for procedural movement. Put a separate `ContainerInventory` on the same GameObject and assign it to the component's `Container Inventory` field.

```text
LeftShelf
├── ContainerInventory
├── Drawer
└── moving cover / state points / colliders
```

Do not make the cabinet body open storage directly. The interactive collider belongs to the physical drawer, door or container so UI opening cannot bypass its visible action.

## Documentation

Read in this order:

1. [Development workflow](Docs/00_Workflow/DEVELOPMENT_WORKFLOW.md)
2. [Project bible](Docs/01_Project/PROJECT_BIBLE.md)
3. [Architecture](Docs/02_Architecture/ARCHITECTURE.md)
4. [Project decisions](Docs/02_Architecture/PROJECT_DECISIONS.md)
5. [Development log](Docs/03_Development/DEV_LOG.md)
6. [Roadmap](Docs/03_Development/ROADMAP.md)

## Project principles

- One class has one main responsibility.
- UI does not own gameplay data.
- Fundamental decisions are documented.
- Multiplayer and server authority are considered early.
- New systems are built only after the previous foundation is stable.
- The project should be understandable from the repository, not only from chat history.
