# Architect Baseline

Purpose:
- Architectural baseline of the project: layers, main constraints, dependency rules, module boundaries.
- Single source of truth for structural decisions that future code must respect.
- Detailed decisions are recorded as ADR files in `Doc/ADR`.

Current baseline:
- Unity `6000.5.3f1`, Universal Render Pipeline.
- All game-specific content lives under `Assets/AngryBirds/`.
- New game-specific code and assets must not be placed directly in the `Assets` root.

Layers:
- `Scripts/Infrastracture` - bootstrap, composition root, DI installers/scopes, platform services. Wiring only, no gameplay logic.
- `Scripts/Ui` - UI panels, widgets, presenters, view components. No direct service creation.
- `Scripts/Localization` - localization keys, adapters, language switching, text binding. No business logic.
- `Scripts/Core` - gameplay/domain logic, free of UI dependencies where possible.

Rules:
- Dependencies point inwards: `Ui` -> `Core`, `Infrastracture` -> `Core`. `Core` never depends on `Ui` or `Infrastracture`.
- `Infrastracture` contains wiring, composition root and integrations, never gameplay logic.
- `Ui` must not create services; pass dependencies through DI or explicit initialization.
- `Localization` holds adapters and keys only, no business logic.
- Assembly definition files (`.asmdef`) are created only after explicit agreement or when a task explicitly requires them.
- Avoid heavy logic in `Update()`, cache component references, prefer event-driven logic.

Update policy:
- Update this document when project behavior, architecture, dependencies, or workflow changes.
