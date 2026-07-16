# Changelog

All notable playable project versions will be documented here.

The project currently has no formal public release.

## [Unreleased]

### Added

- first-person movement;
- camera control;
- interaction system;
- doors;
- drawer prototype;
- ventilation;
- removable bolts;
- item definitions;
- inventory foundation;
- Unity Input System;
- UI Mode;
- grid inventory with size, rotation and cross-container transfer;
- item category colors and valid/invalid placement feedback;
- atomic pickup and drop into world;
- stack splitting by wheel drag and Ctrl amount dialog.

### Changed

- interactions now receive the exact `PlayerInteractor`;
- inventories store concrete `InventoryItem` instances backed by `ItemData`;
- full world transitions preserve the same item instance;
- partial stack operations are validated by model-level transaction services.

### Fixed

- removed random player-inventory lookup from interactables.

## Versioning rule

Create a version entry only when there is a meaningful playable build.
