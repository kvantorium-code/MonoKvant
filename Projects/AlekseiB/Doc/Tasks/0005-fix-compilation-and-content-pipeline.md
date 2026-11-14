# Task 0005: Fix the compilation break and align the content pipeline with the DI architecture

## Goal

The project did not compile, so it could not be opened or played. The compilation break was
caused by the refactoring in the commits `2223ef8` and `f82950d`: the new dependency injection
architecture replaced the old bootstrap classes, but the code that referenced them was not
updated.

## Affected areas

- `Assets/AngryBirds/Scripts/Infrastracture/SceneManager.cs`
- `Assets/AngryBirds/Scripts/Infrastracture/Entry/AppEntryPoint.cs`
- `Assets/AngryBirds/Scripts/Infrastracture/Flow/SceneLoader.cs`
- `Assets/AngryBirds/Scripts/Editor/Builder/GameContentBuilder.cs`
- `Assets/AngryBirds/Settings/Levels/LevelCatalog.asset` (new)
- `Assets/AngryBirds/Scenes/Gameplay.unity` (new), `Assets/AngryBirds/Scenes/Level_01..03.unity` (removed)
- `Assets/AngryBirds/Prefabs/ProjectLifetimeScope.prefab`, `Assets/AngryBirds/Scripts/Infrastracture/SceneName.cs`
- `Doc/Dependency-injection/README.md`, `Doc/Features/core-gameplay.md`

## Plan

1. Fix the compilation errors one layer at a time, they hide each other: a type that does not
   resolve makes its class an error type, which suppresses the member errors inside it.
   - `SceneManager.cs` uses `ILoadingScreen` but was missing `using AngryBirds.Core.Loading;`.
   - The project class `AngryBirds.Infrastructure.SceneManager` shadows Unity's `SceneManager`
     for every type in the `AngryBirds.Infrastructure.*` namespaces. `AppEntryPoint` and
     `SceneLoader` called the Unity API through the plain name, which resolved to the project
     class. They use the `UnitySceneManager` alias now, the pattern the project already used.
   - `GameContentBuilder` still referenced the deleted `GameBootstrap` and `LevelDirector` and
     the namespace `AngryBirds.Infrastructure.Bootstrap` that no longer exists.
2. Port the content builder to the current architecture: `LevelSpawner`, `ShotFlow`,
   `AimController` and `GameplayLifetimeScope` replace `LevelDirector` and `GameBootstrap`.
3. The levels are data now and are all played in one `Gameplay` scene
   (`SceneNames.Gameplay`), so the builder writes that scene instead of one scene per level and
   removes the obsolete `Level_NN` scenes.
4. Create the `LevelCatalog` asset that `ProjectLifetimeScope.levelCatalog` expects and wire it
   on the scope prefab, otherwise the project scope would register a null catalog.
5. Wire `CameraRig._camera`: the field became a serialized reference but nothing ever assigned
   it, so `CameraRig.Awake()` would have thrown a `NullReferenceException`.
6. Load the assets again after every asset refresh and scene switch. A refresh unloads the
   asset objects that were loaded before it, a reference held across one is dead and throws
   `MissingReferenceException`.

## Acceptance criteria

- [x] `Assembly-CSharp` and `Assembly-CSharp-Editor` compile without errors.
- [x] The game starts from the `Boot` scene and reaches the `Gameplay` scene.
- [x] The level of the catalog is spawned: pigs, blocks and the first bird exist.
- [x] No error, exception or `VContainerException` in the Console while starting.
- [x] The build settings contain `Boot` (first) and `Gameplay`.
- [x] `Tools/AngryBirds/2. Build Game Content` is reproducible and regenerates the same result.

## Validation performed

- `Unity 6000.5.3f1` in batch mode: 0 compilation errors. The only warning is the pre-existing
  `Pig.popDuration` field that is assigned but never used.
- `Tools/AngryBirds/2. Build Game Content` in batch mode through
  `-executeMethod AngryBirds.EditorTools.Builder.GameContentBuilder.BuildAll`: finished with
  "Game content rebuilt." and no exception.
- Play mode smoke test (temporary, removed afterwards): from `Boot.unity` the game reached the
  `Gameplay` scene with 2 pigs, 4 blocks, 1 bird and 1 `GameplayLifetimeScope`, and no error
  was logged.

## Outcome

- The project compiles, starts and plays a level.
- The content pipeline produces the scenes of the current architecture instead of the scenes of
  the removed one, so `Tools/AngryBirds/2. Build Game Content` is the single source of truth
  again.
- `Infrastructure.SceneManager` with the `Loader` scene and the generated `SceneName` enum is
  still there but is not part of the game flow: the flow switches scenes through
  `ISceneLoader` / `SceneLoader`. It is left in place, it compiles and does not affect the game.
- `Assets/AngryBirds/Scripts/Infrastracture/Bootstrap/Entrypoint.cs` is an empty leftover of the
  refactoring. Nothing references it, it was left untouched.
