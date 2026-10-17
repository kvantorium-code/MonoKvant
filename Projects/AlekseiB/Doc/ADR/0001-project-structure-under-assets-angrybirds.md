# ADR-0001: Project Structure Under Assets/AngryBirds

## Status

Accepted

## Context

The project was started from a default Unity template. The template placed a sample scene, URP settings, and tutorial readme files directly in the `Assets` root, which makes project-specific content indistinguishable from engine and package content.

The project needs a predictable layout so that future code and assets, whether created by a human or by an AI agent, always land in the same place.

Rules:
- All game-specific content lives under `Assets/<GameName>/`.
- New game-specific code and assets must not be placed directly in the `Assets` root.

## Decision

The project name is `AngryBirds`, and all project-specific content lives under `Assets/AngryBirds/`:

```text
Assets/AngryBirds/
  Content/       Game content: models, textures, materials, audio, animations.
  ThirdParty/    Third-party assets, plugins, and SDKs.
  Scenes/        Main project scenes and scene templates.
  Scripts/       Runtime and editor C# code.
  Settings/      ScriptableObject settings, configs, system configuration.
  Prefabs/       Prefab assets.
```

Scripts are grouped as:

```text
Assets/AngryBirds/Scripts/
  Infrastracture/  Bootstrap, composition root, DI installers/scopes, platform services.
  Ui/              UI panels, widgets, presenters, view components.
  Localization/    Localization keys, adapters, language switching, text binding.
  Core/            Main gameplay/domain logic.
```

The template files that were removed during initialization, and the packages that were pruned, are listed in `Doc/Setup/README.md`.

The remaining template assets were **moved** into `Assets/AngryBirds/` together with their `.meta` files, so every GUID is preserved and existing references from `ProjectSettings` and the sample scene stay valid.

## Alternatives Considered

- Keep a flat `Assets` root, as the Unity template does:
  - Rejected: game content cannot be told apart from package and template content.
- Use `Assets/Scripts`, `Assets/Art`, `Assets/Prefabs` as separate top-level folders:
  - Rejected: the layout is not namespaced by project and is harder to move or archive as a unit.
- `Assets/Game/` as a generic name instead of the project name:
  - Rejected: a project-specific name keeps the boundary explicit and matches the project identity.

## Consequences

- Positive:
  - Clear boundary between project content and engine/package content.
  - Predictable import paths for tools and agents.
  - The whole game content can be archived, versioned, or moved as a single folder.
- Negative:
  - Assets moved from the template root must have their references re-solved or moved together.
  - Slightly deeper paths than a flat layout.
- Risks:
  - Handlers for URP assets and the render pipeline global settings reference GUIDs, so any future move must relocate the `.meta` files along with the assets, and must be done from the Unity editor or by moving both files together.

## Follow-up

- Documentation to update:
  - `Doc/Architect-baseline/README.md`
  - `Doc/Setup/README.md`
- Code areas affected:
  - `Assets/AngryBirds/**`
- Validation needed:
  - Confirm Unity compiles with no errors and URP assets are still assigned in `ProjectSettings`.
