# 03. Уровни: одна сцена Gameplay, разбор LevelDirector

Статус: todo · Зависит от: 02 · Блокирует: 05

## Цель

Одна сцена `Gameplay`, уровни — только данные. Разделить ответственности `LevelDirector`
и `GameBootstrap`; потоки событий — однонаправленные.

## Шаги

- [ ] `ILevelCatalog`/`LevelCatalog` (ScriptableObject-список + сервис): `Get(number)`, `TryGetNext(number, out def)`, `Count`.
      Заменяет `"Level_{0:00}"` и `Application.CanStreamedLevelBeLoaded`.
- [ ] Сцена `Gameplay.unity`: камера, фон, рогатка, `Level` root, scene-level `LifetimeScope`
      (`GameplayLifetimeScope`). Уровень приходит из `GameplayState` (payload — номер уровня).
- [ ] `LevelDirector` разделить на:
  - `LevelSpawner` — строит землю, блоки, свиней из `LevelDefinition`; подписывает `Depleted` и пересылает в `LevelRuntime`.
  - `ShotFlow` — очередь птиц: загрузка, ожидание `Settled`, пауза, завершение по `NotifyOutOfBirds`; про счёт ничего не знает.
- [ ] `AimController` (из `GameBootstrap`): `GameInput` → `Slingshot` (`BeginAim/UpdateAim/ReleaseAim/CancelAim`),
      grab-radius, перевод screen → world.
- [ ] `LevelRuntime` инжектируется как scene-scoped сервис (`Initialize(level)` в `GameplayState`), не создаётся через `new` в игровом коде.
- [ ] Очистка уровня при рестарте/переходе: пересоздание содержимого `Level` root или перезагрузка сцены `Gameplay` — выбрать более простое и описать в отчёте.
- [ ] Удалить `Level_01..03.unity`, `SampleScene.unity` (после проверки ссылок), `GameBootstrap`.
- [ ] Обновить `GameContentBuilder`: не генерирует сцены уровней, только `Gameplay`, префабы и `LevelDefinition`;
      регистрация сцен в Build Settings: `Boot`, `MainMenu` (позже), `Gameplay`.

## Критерии приёмки

- [ ] Все три уровня проходятся в одной сцене `Gameplay`; Restart/Next корректно сбрасывают состояние.
- [ ] В `Core/Level` нет зависимостей на `Ui` и `Infrastracture`.
- [ ] Повторный запуск `Tools/AngryBirds/2. Build Game Content` воспроизводит результат с валидными ссылками.
- [ ] Console: 0 ошибок, 0 предупреждений.

Затронутые сцены/префабы: `Gameplay` (новая), удаляются `Level_01..03`, `SampleScene`; префабы `Bird/Pig/*Block` без изменений.
