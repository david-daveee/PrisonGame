# Coding Style

## Responsibilities

One class must have one main reason to change.

## Naming

Prefer clear names such as:

```csharp
inventoryRoot
itemNameText
currentInventory
playerInputHandler
isInventoryOpen
```

Avoid vague names such as `thing`, `data`, `obj`, `temp`.

## Fields

Prefer private serialized fields:

```csharp
[SerializeField] private GameObject inventoryRoot;
```

## Methods that can fail

Use the `Try` prefix when returning `bool` for an operation that may fail.

## Interfaces

Use interfaces when multiple implementations are expected.

## Dependencies

Prefer explicit references or passed context over scene searches.

Avoid `FindObject...` when the dependency can be assigned or passed.

## Input

Physical keys must not be hard-coded in gameplay classes.

## UI

UI displays state; it does not own gameplay state.

## Multiplayer preparation

Distinguish local input and presentation from authoritative gameplay state.

## Collections

Do not modify a collection inside a `foreach` loop.

## Validation

Validate important inputs early.

## Comments

Comments should explain why, not repeat the code.

## Refactoring

Perform one fundamental refactor at a time:

1. contract;
2. core implementation;
3. dependencies;
4. UI;
5. test;
6. docs.

## Avoid premature architecture

Do not add factories, event buses or deep hierarchies without a real need.
