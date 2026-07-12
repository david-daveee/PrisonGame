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
- Item category controls background.
- Contraband uses a separate marker.
- There is no rarity-color system.
- The future server is authoritative for gameplay state.

## Current state

Current refactor:

```text
List<ItemData> → List<InventoryItem>
```

Current bug: `Added: Screwdriver` appears in Console, but the name is not displayed.

First task:

1. inspect `InventoryUI`;
2. inspect `InventorySlotUI`;
3. verify Inspector references;
4. restore the display;
5. update `DEV_LOG.md` after the fix.

## Documentation responsibility

After every completed mechanic, perform a Session Review and explicitly state which files should be updated, why, which files do not need changes, and the recommended Git commit message.
