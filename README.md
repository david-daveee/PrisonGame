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
- transparent item icons, stack amounts and category-ready visuals;
- two-panel player/container UI;
- bidirectional atomic item transfer;
- independent inventories for separate cabinet compartments;
- reusable static or animated containers with optional audio;
- UI mode that blocks player control while a large window is open;
- architecture seams for future server-authoritative multiplayer.

Current focus:

- final Play Mode validation of the complete container loop;
- dropping inventory items back into the world;
- stack splitting, category backgrounds and contraband markers;
- distance-based container closing;
- defining multiplayer validation requests.

## Controls

- `E` — interact with a world item, door, drawer or container;
- `Tab` — open or close inventory UI;
- hold left mouse button — drag an inventory item;
- `R` — rotate the hovered or currently dragged item.

## Container authoring

Every physically separate compartment owns its own `ContainerInventory`. Keep that component on the same GameObject as the component that opens it.

### Ordinary container

1. Add `ContainerInteractable`. Unity automatically adds `ContainerInventory`.
2. Configure `Container Name`, `Description`, `Width`, `Height` and `Starting Items` in `ContainerInventory`.
3. Add a Collider to the same GameObject or one of its children.
4. Leave `Animator` empty for a static container.
5. Optionally assign an `AudioSource`, `Open Sound` and `Close Sound`.

For an animated container, assign an Animator whose trigger parameters match `Open Trigger` and `Close Trigger` (defaults: `Open` and `Close`). `On Opened` and `On Closed` are optional hooks for custom presentation code, lights, particles or other effects.

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
