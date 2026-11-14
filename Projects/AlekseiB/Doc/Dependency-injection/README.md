# Dependency Injection

DI framework: **VContainer** (`jp.hadashikick.vcontainer` 1.19.0, git package from `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.19.0`).
Versions before 1.18 do not compile on Unity 6000.5 (`Object.GetInstanceID` is an error there), keep the package at 1.18 or newer.

Rules:
- Do not instantiate services directly inside gameplay or UI classes when they belong to DI.
- Keep project-wide bindings separate from scene-specific bindings.
- Prefer constructor injection for plain C# services.
- For `MonoBehaviour`, use method injection: a public `[Inject] Construct(...)` method. Do not use injected dependencies before injection is complete (`Awake` of a scene object runs after the scope injected it, because `LifetimeScope` executes at order -5000).
- A `MonoBehaviour` that lives in a scene is injected by its scope (`container.Inject(component)` in the scope's build callback, or `RegisterComponent`), never by a service locator call from the component itself.
- Resolve through `IObjectResolver` only in composition code (the state machine does it to avoid a construction cycle between the machine and the states).
- Depend on interfaces (`IGameInput`, `ISceneLoader`, `IGameStateMachine`), concrete types stay inside the composition root.
- Document every new long-lived service binding in this file.
- Wiring lives in `Assets/AngryBirds/Scripts/Infrastracture/Entry`.

## Binding Locations

### Project level: `ProjectLifetimeScope`

`Scripts/Infrastracture/Entry/ProjectLifetimeScope.cs`. Lives on the prefab `Assets/AngryBirds/Prefabs/ProjectLifetimeScope.prefab`,
an instance of which is placed in the `Boot` scene (build index 0). It calls `DontDestroyOnLoad`, so the container lives for the whole application.
New project-wide services are registered in `Configure`, serialized references (prefabs) are fields of this component set on the prefab asset.

| Binding | Lifetime | Notes |
|---------|----------|-------|
| `ISceneLoader` -> `SceneLoader` | Singleton | Only place that calls `SceneManager`. |
| `IGameInput` -> `GameInput` | Singleton | Created from `Prefabs/GameInput.prefab`, `DontDestroyOnLoad`. Consumers must unsubscribe from its events in `OnDestroy`. |
| `ILevelCatalog` -> `LevelCatalog` | Singleton | The asset `Assets/AngryBirds/Settings/Levels/LevelCatalog.asset`, serialized field `levelCatalog` of the scope. It lists the level definitions in play order. |
| `IGameStateMachine` -> `GameStateMachine` | Singleton | Resolves states on demand through `IObjectResolver`. |
| `BootState`, `MainMenuState`, `GameplayState`, `ResultState` | Singleton | Game flow states, entered through `IGameStateMachine.Enter<TState>()` / `Enter<TState, TPayload>()`. |
| `AppEntryPoint` | Entry point (`IStartable`) | Enters `BootState` when started from the `Boot` scene (does nothing in a level scene opened directly). |

Game flow: `Boot` scene -> `AppEntryPoint` -> `BootState` -> `MainMenuState` -> `GameplayState(levelNumber)` -> `ResultState` -> `GameplayState(...)`.
`MainMenuState` forwards to the first level until the menu screen exists; `GameplayState` loads the `Gameplay` scene by number and wraps to level 1 after the last level.

### Scene level: `GameplayLifetimeScope`

`Scripts/Infrastracture/Entry/GameplayLifetimeScope.cs`, one object in the `Gameplay` scene. The parent scope is `ProjectLifetimeScope`.
If the scene is opened directly in the editor (no `Boot` scene was loaded) the scope instantiates `fallbackProjectScope`
(the project scope prefab, serialized field) and uses it as the parent, so the level is playable from any scene.

| Binding | Lifetime | Notes |
|---------|----------|-------|
| `LevelRuntime` | Singleton in the scene scope | One per level scene, recreated on every scene load. |
| `LevelSpawner`, `ShotFlow`, `AimController`, `CameraRig`, `HudView` | Scene objects | Registered with `RegisterComponent`, serialized fields of the scope. |
| `GameplayEntryPoint` | Entry point (`IStartable`) | Spawns the level of `GameplayState`, wires the HUD and the input, and connects the result / restart / next commands to the game state machine. |

## Adding a binding

1. Project-wide service: add `builder.Register<...>(Lifetime.Singleton)` to `ProjectLifetimeScope.Configure`, add a row to the table above.
2. Per-scene service or scene `MonoBehaviour`: add it to `GameplayLifetimeScope.Configure`, add a row to the table above.
3. A prefab-based service (`RegisterComponentInNewPrefab`): add a serialized prefab field to the scope and assign it on the scope prefab (project level) or in the scene (scene level).
4. Assets created by `Tools/AngryBirds/2. Build Game Content` must be wired in `GameContentBuilder` as well.

## Validation

- Verify that all injected dependencies have bindings: Play Mode from `Boot` and from a level scene opened directly, no `VContainerException` in the Console.
- Check Unity Console after changing scopes or bindings.
