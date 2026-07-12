# Architecture

## Input flow

```text
PlayerInputActions
→ PlayerInputHandler
→ PlayerInventoryInput
→ InventoryUI
→ InventorySlotUI
```

Data flow:

```text
PlayerInventory
→ IInventory
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

### PlayerInventory
Stores `List<InventoryItem>` and handles stacking, adding, removing and counting.

### ItemData
ScriptableObject describing an item type.

### InventoryItem
Concrete item instance or stack.

### WorldItem
World representation of a concrete instance.

### InventoryUI
Displays any `IInventory`.

### InventorySlotUI
Displays one item or slot.

### PlayerInteractor
Finds `IInteractable` and calls `Interact(this)`.

### IInteractable

```csharp
void Interact(PlayerInteractor interactor);
string GetInteractionText();
```

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

- network serialization;
- item factory;
- inventory change events;
- full grid placement;
- drag and drop;
- saving;
- durability;
- complex Player hierarchy;
- separate UI Action Maps.
