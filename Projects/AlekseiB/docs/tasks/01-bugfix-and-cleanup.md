# 01. Багфикс и уборка

Статус: done · Зависит от: — · Блокирует: 02

## Цель

Сделать игру стабильно играбельной до начала структурных изменений и убрать очевидный мусор.

## Проблемы

1. **StackOverflow при первом разрушении.** В `LevelDirector.Initialize` стоит
   `_runtime.BlockDestroyed += OnBlockDestroyed` / `PigDestroyed += OnPigDestroyed`, а эти
   обработчики вызывают `_runtime.NotifyBlockDestroyed` / `NotifyPigDestroyed`, которые снова
   поднимают те же события — бесконечная рекурсия.
2. **Дубль путей уничтожения.** `OnPigDepleted` → `OnPigDestroyed` и подписка на события runtime
   делают одно и то же двумя путями.
3. **Утечка птиц.** После `Bird.Settle()` объект не уничтожается: невидимое kinematic-тело с
   включённым коллайдером остаётся в сцене.
4. **Дубль Input-ассета.** `Infrastracture/Input/AngryBirdsActionScheme.inputactions` и его
   сгенерированный `.cs` — шаблон Unity, не используется. `GameInput` создаёт лишний
   `new AngryBirdsActionScheme()` и держит неиспользуемое поле.
5. **Мусорная сцена `SampleScene`** в Build Settings.

## Шаги

- [x] `LevelDirector`: убрать подписки на `LevelRuntime.BlockDestroyed/PigDestroyed`; единственный путь —
      `actor.Depleted` → `runtime.NotifyXxxDestroyed`.
- [x] `Bird`: после `Settle` отключить коллайдер и уничтожить объект (с учётом того, что камера и
      `ShotFlow` ещё читают `transform` в обработчике `Settled`; уничтожение — после события).
- [x] `GameInput`: удалить `_actionScheme` и создание `AngryBirdsActionScheme`.
- [x] Удалить `AngryBirdsActionScheme.inputactions` + `.cs` + `.meta`
      (предварительно проверить, что на них нет ссылок в сценах/префабах/ProjectSettings:
      `Grep` по GUID из `.meta`). Откат изменений `AngryBirdsActionScheme.cs` в рабочей копии не нужен — файл удаляется.
- [x] Убрать `SampleScene` из Build Settings (сам файл удалить только после проверки ссылок).
- [x] Вынести подсчёт звёзд для HUD в `LevelRuntime` (`GetLiveStars()`), `HudView` использует его;
      убрать дубль формулы.

## Критерии приёмки

- [x] В Play Mode разрушение блока и свиньи не приводит к исключениям/StackOverflow.
- [x] Полный цикл: запуск → попадание → победа и поражение → Restart → Next level работает на всех трёх уровнях.
- [x] После нескольких выстрелов в иерархии нет «мёртвых» птиц.
- [x] Console: 0 ошибок, 0 новых предупреждений.

## Валидация

Unity MCP: `read_console` после компиляции, затем Play Mode прогон `Level_01`.
Затронутые сцены: `Level_01..03` (только если пересохраняются), `Build Settings`.
