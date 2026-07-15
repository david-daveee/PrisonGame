# Project Decisions

## 2026-07-12 — Inventory stores InventoryItem, not ItemData

### Context
`ItemData` describes an item type, but concrete items need mutable state.

### Decision
Inventories store `InventoryItem`.

### Reason
A concrete screwdriver may be damaged or unique.

### Consequences
- `IInventory` works with `InventoryItem`.
- `PlayerInventory` stores `List<InventoryItem>`.
- `WorldItem` preserves instance state.

### Rejected alternative
Store only `ItemData`.

---

## 2026-07-12 — Interaction receives PlayerInteractor

### Decision
Use `Interact(PlayerInteractor interactor)`.

### Reason
The exact initiating player must be known.

### Consequences
Safer multiplayer architecture and no random player lookup.

---

## 2026-07-12 — Input uses Unity Input System

### Decision
Use `PlayerInputActions → PlayerInputHandler → gameplay input component`.

### Reason
Bindings become data rather than hard-coded gameplay logic.

---

## 2026-07-12 — Large UI windows block player control

### Decision
Disable movement, camera and interaction while large UI is open.

### Reason
Prevents accidental movement and creates consistent container/shop/crafting behavior.

---

## 2026-07-12 — Item category controls background

### Decision
Category controls background, contraband uses a separate marker, and there is no rarity color.

### Reason
Category and contraband are independent properties, and item value should be understood naturally.

### Implementation
`InventoryGridUI` exposes a `Category Colors` list in the Inspector. Each entry maps an `ItemCategory` to the base color of its occupied cells; adding a category only requires adding the enum value and one palette entry. Valid and invalid drag-preview colors temporarily override those base colors and use the same `InventoryGrid.CanPlaceItem` validation as the final drop.

---

## 2026-07-12 — Home profession stations are progression bonuses

### Decision
Professions unlock small home stations at meaningful trust thresholds.

### Reason
The cell visibly reflects progression without replacing workplaces.

---

## 2026-07-15 — PlayerInventory owns grid state

### Context
The grid prototype previously existed only in a debug script while `PlayerInventory` stored a separate item list.

### Decision
`PlayerInventory` owns one `InventoryGrid`. Item positions and rotations live in `InventoryPlacement`; UI only displays and requests changes.

### Reason
A single source of truth prevents pickup, UI and future containers from disagreeing about inventory contents.

### Consequences
- `WorldItem` pickup is accepted only when the complete item or stack fits.
- Drag/drop and rotation are validated by the gameplay model.
- Future container inventories can implement `IGridInventory` and reuse `InventoryGrid` and the same UI presentation.

### Rejected alternative
Keep a list in `PlayerInventory` and a separate grid inside the UI/debug script.

---

## 2026-07-15 — Player and containers share GridInventory

### Context
Containers need the same placement, stack and capacity rules as the player inventory.

### Decision
Use the reusable non-MonoBehaviour `GridInventory` model behind both `PlayerInventory` and `ContainerInventory`.

### Reason
Transfers must be validated by identical rules on both sides, without copying storage logic into every container type.

### Consequences
- Container metadata stays in `ContainerInventory`; Unity interaction stays in `Drawer`, `Door` or `ContainerInteractable`.
- Storage rules stay in `GridInventory`.
- `InventoryUI` can display both through `IGridInventory`.
- Cross-grid movement is committed through `InventoryTransferService`, not by directly mutating both panels in UI code.
- Detached placements remain registered by their source grid until a transfer or rollback is committed.

### Rejected alternative
Duplicate `PlayerInventory` logic in a cabinet-specific script.

---

## 2026-07-15 — Physical mechanisms own container interaction

### Context
When both the cabinet body and its drawer implemented `IInteractable`, the camera ray could open the inventory directly through the body and skip the drawer animation.

### Decision
`ContainerInventory` owns only storage data. `Drawer` and `Door` own world interaction, animation and the call to `ContainerInventory.OpenFor`.

### Reason
One collider hit maps to one visible physical action, and separate cabinet compartments keep independent contents.

### Consequences
- The cabinet body may keep a collision collider, but it must not intercept the interaction ray.
- Every independently stored compartment connects its moving `Drawer` or `Door` to its own `ContainerInventory` in the Inspector.
- A container is shared only when several mechanisms intentionally access exactly the same contents.
- Future multiplayer authority can validate the interaction and container opening as one server action.

---

## 2026-07-15 — Generic containers use optional presentation hooks

### Context
Many world objects need storage, but not every box, mattress or cache has a door animation.

### Decision
Use `ContainerInteractable` as the reusable interaction component. It always opens a referenced `ContainerInventory`, while Animator triggers, open/close audio clips and Inspector events remain optional presentation hooks.

### Reason
Storage rules and interaction stay reusable without forcing every container to own a bespoke animation script.

### Consequences
- Adding `ContainerInteractable` automatically adds the required `ContainerInventory`.
- A static container needs those components and a reachable collider; all presentation fields may remain empty.
- An animated container may assign an Animator with `Open` and `Close` triggers.
- Open and close clips play through an optional assigned `AudioSource`.
- Custom procedural presentation can subscribe through `On Opened` and `On Closed` without moving storage state into animation code.
