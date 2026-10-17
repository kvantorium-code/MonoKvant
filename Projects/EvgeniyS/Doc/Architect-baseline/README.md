# Architect Baseline

Purpose:
- Architecture base: layers, main constraints, dependency rules, module boundaries.

Rules:
- Keep game-specific code under `Assets/AngryBirds/Scripts`.
- `Infrastracture` holds wiring, composition root and integrations, not gameplay logic.
- `Ui` must not create services directly; pass dependencies via DI or explicit initialization.
- `Localization` holds adapters and localization keys, not business logic.
- `Core` holds domain/gameplay systems without UI dependency where possible.
- Do not place new game-specific code or assets directly under the `Assets` root.

Update policy:
- Update this document when project behavior, architecture, dependencies, or workflow changes.
