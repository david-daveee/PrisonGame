# Architecture

## Input flow

```text
PlayerInputActions
→ PlayerInputHandler
→ PlayerInventoryInput
→ InventoryUI
→ InventoryGridUI
→ InventoryItemUI
```

Data flow:

```text
PlayerInventory
→ InventoryGrid
→ InventoryPlacement
→ InventoryUI reads data
```

UI state flow:

```text
PlayerInventoryInput
→ PlayerUIStateController
→ movement / camera / interaction / cursor / Gameplay UI
```

## Responsibilities

### PlayerInputActions
Generated Unity Input System class. Contains Actions and bindings.

### PlayerInputHandler
Single access layer for player input.

### PlayerInventoryInput
Connects Inventory input to opening and closing the inventory.

### PlayerUIStateController
Switches between Game Mode and UI Mode.

### IInventory

```csharp
bool TryAddItem(InventoryItem inventoryItem);
bool TryRemoveItem(InventoryItem inventoryItem);
bool HasItem(ItemId itemId, int amount = 1);
IReadOnlyList<InventoryItem> GetItems();
```

### IGridInventory
Extends `IInventory` with an `InventoryGrid` and a change notification. `InventoryUI` depends on this contract so player and future container inventories can share the same grid presentation.

### PlayerInventory
Unity-facing inventory component for the player. It delegates storage rules to `GridInventory`.

### GridInventory
Reusable non-MonoBehaviour storage model shared by players and containers. It handles atomic stacking, adding, removing and counting. If the complete incoming amount cannot fit, it does not change the inventory.

### ContainerInventory
Unity component with Inspector-configured name, description, grid size and starting items. It should live on the same GameObject as that compartment's `Drawer`, `Door` or `ContainerInteractable`, keeping all per-compartment settings visible together in the Inspector. It owns a `GridInventory`, but is deliberately not an `IInteractable`; the physical interaction component calls `OpenFor` for the exact player.

### ContainerInteractable
Reusable world interaction for ordinary containers. Adding it automatically adds the required `ContainerInventory`, exposing metadata and grid size beside it. It optionally drives an `Animator` with `Open` and `Close` triggers and plays separate open/close clips through an assigned `AudioSource`. Inspector events remain an extension point for other effects or procedural animation. All presentation fields are optional.

### Drawer and Door
Physical interaction components. They animate the moving part and can optionally reference a `ContainerInventory`. Closing that container's UI raises `Closed`, which returns the connected mechanism to its closed state. Each physical storage compartment should normally own a separate `ContainerInventory`; sharing one is reserved for multiple access points that intentionally expose the same contents.

### InventoryTransferService
Commits an atomic transfer of a detached placement from a verified source grid to a validated destination grid. This local service is the seam that a future authoritative server command will replace.

### InventoryGrid
Owns placements and validates bounds, overlap, movement and rotation. During drag it supports detach/attach as a transaction so the held item can rotate independently of its old grid position. It does not process input or render UI.

### InventoryPlacement
Connects one `InventoryItem` instance to a grid position and orientation.

### ItemData
ScriptableObject describing an item type.

### InventoryItem
Concrete item instance or stack.

### WorldItem
World representation of a concrete instance.

### InventoryUI
Opens and closes inventory windows, subscribes to inventory changes and connects models to `InventoryGridUI`. Container mode displays the container grid on the left and player grid on the right. A shared topmost `DragLayer` keeps the temporary dragged visual above both panels without owning gameplay state.

### InventoryGridUI
Displays cells and placements. Converts pointer positions to grid positions, but delegates placement validation to `InventoryGrid`. Its Inspector palette maps `ItemCategory` values to occupied-cell colors. During drag it asks the same `InventoryGrid.CanPlaceItem` rule used by drop and temporarily overrides the target cells with valid or invalid preview colors.

### InventoryItemUI
Displays one placement and reports pointer hover and drag/drop gestures to `InventoryGridUI`. Name and amount are presentation-only and hidden while dragging, leaving the centred icon unobstructed. Rotation input comes from `PlayerInputHandler`; `R` rotates the hovered placement and immediately refreshes the cell preview.

### PlayerInteractor
Finds `IInteractable` and calls `Interact(this)`.

### IInteractable

```csharp
void Interact(PlayerInteractor interactor);
string GetInteractionText();
```

## Container authoring rules

### One compartment, one storage model

Every independently stored drawer, door compartment, box or hidden cache owns a separate `ContainerInventory`. Sharing a reference is valid only when multiple access points intentionally expose the same contents.

Keep `ContainerInventory` on the same GameObject as its opening component:

```text
Static or Animator-driven container
├── ContainerInventory
└── ContainerInteractable

Procedural drawer
├── ContainerInventory
└── Drawer

Procedural door compartment
├── ContainerInventory
└── Door
```

### Collider ownership

The Collider used by `PlayerInteractor` must be on the interactable GameObject or below it in the hierarchy because interaction is resolved with `GetComponentInParent<IInteractable>()`. Avoid broad body colliders that intercept the ray before it reaches the physical mechanism.

### Optional presentation

`ContainerInteractable` supports three independent optional presentation paths:

- Animator triggers for `Open` and `Close`;
- `AudioSource.PlayOneShot` with separate open and close clips;
- `On Opened` and `On Closed` Inspector events for custom effects.

An ordinary container works with all presentation fields empty. `Drawer` and `Door` remain specialized for their current procedural movement.

For Animator-driven hinged containers, the Animator belongs to the interactable root and the clip animates a child path such as `Pivot`. The Pivot is positioned at the hinge and owns the mesh plus both physical and interaction colliders. A typical small opening clip records `localEulerAnglesRaw.y` from `0` to `10` degrees; controller transitions listen to the `Open` and `Close` triggers raised by `ContainerInteractable`.

Animator-driven sliding drawers use the same hierarchy and triggers, but animate the Pivot's local position rather than its rotation. The bedside `DrawerContainer` moves local X from `0` to `-0.3` over 0.25 seconds; placing both colliders below the Pivot keeps interaction and collision geometry synchronized with the visible drawer.

## Item model

```text
ItemData = immutable item-type definition
InventoryItem = mutable state of one instance or stack
WorldItem = world representation of that instance
Inventory = storage and rules for instances
```

## Multiplayer authority

Local-only:

- input;
- cursor;
- windows;
- local highlights;
- local camera behavior.

Server-authoritative:

- inventory contents;
- item transfers;
- pickup and drop;
- container state;
- doors;
- removed bolts;
- important world state.

Future flow:

```text
Client requests action
→ Server validates
→ Server changes state
→ Clients receive synchronized state
→ UI refreshes
```

## Intentionally postponed

Future container transfer requests will contain the source inventory, destination inventory, item instance, requested position and rotation. The server must validate ownership and capacity before clients update presentation.

- network serialization;
- item factory;
- saving;
- durability;
- complex Player hierarchy;
- separate UI Action Maps.
