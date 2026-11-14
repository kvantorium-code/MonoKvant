# Task 0006: Align the project to the workspace architecture baseline

## Goal

Bring the AngryBirds project in line with `Docs/AI_AGENT_ARCHITECTURE_BASELINE.md` (repo root):

- one scene-transition service behind DI, with the additive Loader curtain scene;
- View + Controller UI pattern;
- no dead code or duplicate scene-name sources.

Decision record: `Doc/ADR/0002-unified-scene-manager-and-view-controller-ui.md`.

## Changes

### Scene flow

- Added `Scripts/Infrastracture/Flow/IGameSceneManager.cs` + `GameSceneManager.cs`: the only place that calls the Unity SceneManager. Fully additive transitions: load `Loader` additively -> load the target additively with `allowSceneActivation = false` while forwarding progress to `ILoadingScreen` -> activate and set active -> unload the previous content scene -> unload `Loader`. Registered as a singleton in `ProjectLifetimeScope`.
- Deleted the static `Scripts/Infrastracture/SceneManager.cs` (singleton, unused, its `Single` load killed the additive curtain), `Scripts/Infrastracture/Flow/SceneLoader.cs` + `ISceneLoader.cs` (synchronous `LoadScene`, no curtain), and `SceneNames.cs` (string constants; the generated `SceneName` enum is the single source).
- `SceneName.cs` moved to `Scripts/Infrastracture/Flow/` (namespace `AngryBirds.Infrastructure.Flow`); `SceneNameGenerator` output path updated to match.
- `GameplayState` now switches scenes via `IGameSceneManager.Load(SceneName.Gameplay)`; `AppEntryPoint` compares against `SceneName.Boot`.
- Added `Scripts/Editor/Builder/LoaderSceneBuilder.cs` (`Tools/AngryBirds/4. Build Loader Scene`): builds `Loader.unity` (curtain canvas + `LoadingScreenView` wiring) and registers it in the build settings; also invoked from `Tools/AngryBirds/2. Build Game Content`.

### UI split

- `HudView` is now pure display: `SetLevelName`, `SetScore`, `SetBirdsRemaining`, `SetLiveStars`, `ShowResult` and `event Action RestartRequested / NextLevelRequested`. It no longer subscribes to `LevelRuntime`.
- New `Scripts/Ui/HudController.cs`: owns the `LevelRuntime` subscriptions, pushes state into the view, re-emits view presses; registered in `GameplayLifetimeScope`, disposed with the scope.
- `LevelResultPanel` events changed from `System.Action` fields to `event Action`.
- `GameplayEntryPoint` no longer formats the result UI; it only wires the parts and forwards commands to the state machine.

### Dead code removal

- Deleted `Scripts/Ui/Core/` (unused ScreenBase / IScreenPresenter framework referencing a nonexistent `IUiService`).
- Deleted the empty `Scripts/Infrastracture/Bootstrap/Entrypoint.cs` (folder removed).

## Affected areas

- `Assets/AngryBirds/Scripts/Infrastracture/Flow/*`, `Scripts/Infrastracture/Entry/*`, `Scripts/Ui/*`
- `Assets/AngryBirds/Scripts/Editor/Builder/` (`LoaderSceneBuilder`, `GameContentBuilder`, `SceneNameGenerator`)
- `Assets/AngryBirds/Scenes/Loader.unity` (created by the Unity content build)
- `ProjectSettings/EditorBuildSettings.asset` (Loader scene registration)

## Acceptance criteria

- [x] Both script assemblies compile with 0 errors (`dotnet build` of the regenerated csproj files).
- [ ] `Loader.unity` exists and is registered in the build settings (run `Tools/AngryBirds/2. Build Game Content` or `4. Build Loader Scene` in Unity).
- [ ] Play from `Boot`: transitions show the curtain with progress, the previous scene unloads, no `VContainerException`.
- [ ] Play the `Gameplay` scene opened directly: the fallback project scope still works, HUD shows and updates.
- [ ] No remaining references to `ISceneLoader`, `SceneNames`, the static `SceneManager`, or `Ui/Core` types.

## Notes

- The csproj files are Unity-generated and gitignored; they were temporarily regenerated for the build check. Unity will regenerate them on next script compile.
- `Pig.popDuration` warning (CS0414) is pre-existing and unrelated.
