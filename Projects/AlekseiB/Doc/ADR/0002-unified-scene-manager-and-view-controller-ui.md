# ADR-0002: Unified Scene Manager Behind DI and View/Controller UI

## Status

Accepted

## Context

The workspace adopted a project-agnostic architecture baseline (`Docs/AI_AGENT_ARCHITECTURE_BASELINE.md` in the repo root). Aligning the AngryBirds project to it surfaced three deviations:

1. **Two competing scene loaders.** A static `Infrastructure.SceneManager` (singleton pattern, no DI, never called at runtime) coexisted with a DI-registered `SceneLoader` that used synchronous `LoadScene`. The static loader also loaded the target scene with `LoadSceneMode.Single`, which unloads the additively loaded Loader curtain scene together with the old content.
2. **UI logic leaked into views.** `HudView` subscribed to `LevelRuntime` itself and showed the result overlay; `GameplayEntryPoint` formatted result titles. The baseline requires View (pure display) + Controller (all logic, dependencies via DI).
3. **Dead code.** `Scripts/Ui/Core` (an unused ScreenBase/IScreenPresenter framework referencing a nonexistent `IUiService`) and an empty `Bootstrap/Entrypoint.cs`.

## Decision

- A single `IGameSceneManager` / `GameSceneManager` service (project scope, replaces `ISceneLoader` / `SceneLoader` and the static `SceneManager`). It is the only place that calls the Unity SceneManager. Transitions are fully additive so the curtain survives: additively load `Loader` -> additively load the target with deferred activation while reporting progress to `ILoadingScreen` -> activate and set active -> unload the previous content scene -> unload `Loader`. The `Boot` scene (build index 0) is never unloaded.
- A dedicated `Loader` scene (curtain with progress bar, `LoadingScreenView` implementing `ILoadingScreen`) created by `Tools/AngryBirds/4. Build Loader Scene` (`LoaderSceneBuilder`), also built by the full content pipeline, and registered in the build settings.
- Scene names come from the generated `SceneName` enum only (moved to `Scripts/Infrastracture/Flow/`, namespace `AngryBirds.Infrastructure.Flow`); the string-constant `SceneNames` class is deleted.
- HUD is split: `HudView` is pure display (`SetScore`, `SetBirdsRemaining`, `SetLiveStars`, `ShowResult`, press events), `HudController` (registered in `GameplayLifetimeScope`) owns the `LevelRuntime` subscriptions and event wiring; `LevelResultPanel` events are now `event Action`. `GameplayEntryPoint` shrinks to wiring.
- Deleted dead code: `Scripts/Ui/Core/*`, `Scripts/Infrastracture/Bootstrap/Entrypoint.cs`.

## Alternatives Considered

- Keep the static singleton `SceneManager`: rejected, it bypasses DI and cannot be mocked or scoped.
- Keep `SceneLoader` and load the curtain as overlay UI instead of a scene: rejected, the curtain scene approach already existed and keeps the curtain out of every content scene.
- Keep `SceneNames` constants alongside the enum: rejected, two sources of truth for scene names.

## Consequences

- Positive: one scene-transition path, DI-consistent, matches the workspace baseline; UI is testable and view logic stays inert.
- Negative: transitions are async; states that switch scenes fire and forget (`GameplayState.Enter` does not await).
- Risks: the `Loader.unity` asset must exist and be registered in the build settings, otherwise transitions log a warning and run without the curtain. Editor tooling must be re-run after checkout.

## Follow-up

- Documentation to update: `Doc/Dependency-injection/README.md`, `Doc/Architect-baseline/README.md`, `Doc/Tasks/0006-*`.
- Code areas affected: `Scripts/Infrastracture/Flow`, `Scripts/Infrastracture/Entry`, `Scripts/Ui`, `Scripts/Editor/Builder`.
- Validation needed: run the Unity content build (`Tools/AngryBirds/2. Build Game Content`), play from `Boot` and from the `Gameplay` scene opened directly; no `VContainerException`, curtain shows during transitions.
