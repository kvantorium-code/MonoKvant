# 02. DI и точка входа

Статус: done · Зависит от: 01 · Блокирует: 03, 04

## Цель

Единая точка входа приложения, DI-контейнер и игровой flow через state machine.
`GameBootstrap` перестаёт быть корнем уровня и в конце задачи удаляется.

## Предусловие

Подтверждение выбора **VContainer** (D1). Добавление пакета в `Packages/manifest.json`
(git URL или OpenUPM) — отдельным шагом с проверкой разрешения пакета в Unity.

## Шаги

- [x] Добавить пакет VContainer, убедиться, что проект компилируется.
- [x] Сцена `Assets/AngryBirds/Scenes/Boot.unity` — build index 0. Содержит `ProjectLifetimeScope`
      и `AppEntryPoint`.
- [x] `Infrastracture/Entry/ProjectLifetimeScope`: регистрация project-level сервисов
      (singleton): `ISceneLoader`, `IGameInput`/`GameInput` (префаб, `DontDestroyOnLoad`),
      `GameStateMachine`. Остальные сервисы (`ILevelCatalog`, `IProgressService`, `UiService`,
      локализация) добавляются в задачах 03–06.
- [x] `Infrastracture/Entry/AppEntryPoint` (`IStartable`): запускает state machine и переходит в `MainMenu`
      (до готовности меню — сразу в `Gameplay` с первым уровнем).
- [x] `Infrastracture/Flow/`:
  - `IGameState`, `IGameStateMachine`/`GameStateMachine` (`Enter<TState>()`, `Enter<TState, TPayload>()`).
  - Состояния: `BootState`, `MainMenuState`, `GameplayState(levelNumber)`, `ResultState`.
  - `ISceneLoader`/`SceneLoader`: загрузка сцен по имени/индексу, единый путь вместо `SceneManager.LoadScene` в разных местах.
- [x] Перенести из `GameBootstrap` ответственности: навигацию (Restart/Next) → состояния,
      перевод ввода в команды рогатки → задача 03 (`AimController`).
- [x] Обновить `Doc/Dependency-injection/README.md`: фреймворк, места биндингов, правила.

## Критерии приёмки

- [x] Запуск проекта из `Boot` сцены доходит до игрового уровня; запуск Level-сцены напрямую в редакторе тоже не падает (fallback через scope сцены).
- [x] В проектных скриптах нет `new` для сервисов, принадлежащих DI.
- [x] Console: 0 ошибок, 0 предупреждений.

## Риски

- `DefaultExecutionOrder(-100)` и порядок `Awake`/инъекции: MonoBehaviour использует зависимости только после `Construct`/инъекции.
- Пока уровни остаются отдельными сценами, scene-level scope создаётся на каждой уровневой сцене; в задаче 03 он переедет в единственную `Gameplay`.

Затронутые сцены: `Boot` (новая), `Level_01..03` (временно — scene scope), Build Settings.

## Итог

- VContainer 1.19.0 (git URL). Версия 1.16.9 не компилируется на Unity 6000.5 (`GetInstanceID` — ошибка), поэтому взята 1.19.0.
- Project scope — префаб `Prefabs/ProjectLifetimeScope.prefab` (экземпляр в `Boot`), `GameInput` — `Prefabs/GameInput.prefab`.
- `GameplayLifetimeScope` в каждой уровневой сцене: parent — project scope; при запуске сцены напрямую создаёт его из `fallbackProjectScope`.
- `GameBootstrap` остаётся (удаляется в задаче 03), получает зависимости через `Construct`, навигация идёт через `GameplayState`/`ResultState`.
- Подробности и места биндингов — `Doc/Dependency-injection/README.md`.

