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

---

## 2026-07-12 — Home profession stations are progression bonuses

### Decision
Professions unlock small home stations at meaningful trust thresholds.

### Reason
The cell visibly reflects progression without replacing workplaces.
