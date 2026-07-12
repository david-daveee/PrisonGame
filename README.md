# PrisonGame

Cooperative prison-life simulation where escape is the players' most ambitious shared project, while daily life, professions, relationships, economy and cell development remain engaging on their own.

## Current status

Early gameplay and architecture prototype.

Implemented:

- first-person movement and camera;
- interaction system;
- doors, drawers, vents and removable bolts;
- ScriptableObject item definitions;
- inventory foundation;
- Unity Input System;
- UI mode;
- initial item stack model;
- architecture prepared for future multiplayer authority.

Current focus:

- migrating inventory from `ItemData` to `InventoryItem`;
- restoring inventory UI after the refactor;
- item icons and category visuals;
- grid inventory;
- drag and drop;
- containers.

## Documentation

Read in this order:

1. `Docs/00_Workflow/DEVELOPMENT_WORKFLOW.md`
2. `Docs/01_Project/PROJECT_BIBLE.md`
3. `Docs/02_Architecture/ARCHITECTURE.md`
4. `Docs/03_Development/DEV_LOG.md`
5. `Docs/03_Development/ROADMAP.md`

## Project principles

- One class has one main responsibility.
- UI does not own gameplay data.
- Fundamental decisions are documented.
- Multiplayer and server authority are considered early.
- New systems are built only after the previous foundation is stable.
- The project should be understandable from the repository, not only from chat history.
