# План рефакторинга AngryBirds

Цель: привести проект к чистой модульной архитектуре — единая точка входа, DI, игровой
flow через state machine, один унифицированный UI-слой (MVP + prefab-экраны), локализация
и сохранение прогресса. Исходный анализ и обоснование — в обсуждении с владельцем проекта.

## Принятые решения (рекомендованные варианты)

| # | Решение | Вариант |
|---|---------|---------|
| D1 | DI-фреймворк | VContainer (project-level + scene-level `LifetimeScope`) |
| D2 | Уровни | Одна сцена `Gameplay`, уровни — только данные `LevelDefinition` |
| D3 | Assembly definitions | По модулям: `Core`, `Ui`, `Infrastracture`, `Localization` (отдельная задача 07) |
| D4 | UI | MVP + prefab-экраны + TextMeshPro, единый `UiService` |
| D5 | Объём | Включая `MainMenu` и `LevelSelect` |
| D6 | Незакоммиченные правки редактора (`AngryBirdsActionScheme.cs`, URP/ProjectSettings) | Не включать в коммиты рефакторинга |

Пункты D1 и D3 требуют отдельного явного подтверждения перед началом задач 02 и 07
(правила `AGENTS.md`: новые зависимости и `.asmdef`).

## Задачи

| Задача | Название | Зависит от | Статус |
|--------|----------|------------|--------|
| [01](01-bugfix-and-cleanup.md) | Багфикс и уборка | — | done |
| [02](02-di-and-entry-point.md) | DI и точка входа (Boot, state machine) | 01 | done |
| [03](03-level-flow-single-scene.md) | Уровни: одна сцена Gameplay, разбор `LevelDirector` | 02 | todo |
| [04](04-ui-core.md) | Ядро UI-слоя (`UiService`, `ScreenBase`, виджеты) | 02 | todo |
| [05](05-ui-screens-migration.md) | Экраны: HUD, Result, MainMenu, LevelSelect | 03, 04 | todo |
| [06](06-localization-and-progress.md) | Локализация и сохранение прогресса | 05 | todo |
| [07](07-asmdef-modules.md) | Assembly definitions по модулям | 06 | todo |
| [08](08-documentation.md) | Документация, ADR, закрытие ревью | 07 | todo |

Задачи 03 и 04 независимы друг от друга и могут выполняться параллельно после 02
(они трогают разные папки: `Core/Level` и `Ui`).

```text
01 → 02 ─┬→ 03 ─┐
         └→ 04 ─┴→ 05 → 06 → 07 → 08
```

## Целевая структура

```text
Assets/AngryBirds/Scripts/
  Infrastracture/
    Entry/        AppEntryPoint, ProjectLifetimeScope
    Flow/         GameStateMachine, состояния, ISceneLoader/SceneLoader
    Input/        GameInput (+ IGameInput), один .inputactions
    Services/     ILevelCatalog, IProgressService
  Core/
    Domain/       LevelRuntime, StarRating, ScoreRules (чистый C#)
    Gameplay/     Bird, Pig, Block, Slingshot, Damage…
    Level/        LevelDefinition, LevelSpawner, ShotFlow, AimController
  Ui/
    Core/         UiService, UiRoot, ScreenBase, UiScreenId
    Screens/      Hud, LevelResult, MainMenu, LevelSelect, Pause
    Widgets/      StarRow, LabeledCounter, PrimaryButton
  Localization/   ключи, ILocalizationService, LocalizedText
  Editor/         Art, Builder
```

Зависимости направлены внутрь: `Ui → Core`, `Infrastracture → Core`; `Core` не знает про
`Ui` и `Infrastracture`.

## Общие правила для всех задач

- Минимальные целевые изменения, публичные API меняем только когда задача этого требует.
- Каждая задача заканчивается компилируемым и играбельным проектом.
- После изменений скриптов: проверить компиляцию, Console (ошибки и предупреждения).
- В отчёте по задаче перечислять затронутые сцены и префабы, где нужна ручная настройка в Inspector.
- Перед правкой читать `Doc/Architect-baseline/README.md`, `Doc/Dependency-injection/README.md`,
  `Doc/Features/core-gameplay.md`.
- Не удалять и не перезаписывать файлы без проверки, что они не используются
  (сцены и префабы ссылаются на скрипты по GUID — `.meta` перемещать вместе с файлами).
- Комментарии и именование в стиле окружающего кода; `UnityEngine.Object`-ссылки через
  serialized-поля там, где это уместно.
- Коммиты и PR — на английском, без строк авторства Claude/Anthropic.

## Заметка о расположении

Этот каталог `docs/tasks/` создан по просьбе владельца. В проекте уже есть `Doc/` с
`Doc/Tasks`, `Doc/ADR` и др. (правила в `AGENTS.md`). Итоговые ADR и обновления
архитектурных документов (задача 08) остаются в `Doc/`; здесь живёт только план рефакторинга.
