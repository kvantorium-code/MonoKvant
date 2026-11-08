# 05. Экраны: HUD, Result, MainMenu, LevelSelect

Статус: todo · Зависит от: 03, 04 · Блокирует: 06

## Цель

Перенести существующий UI на единый контракт, добавить недостающие экраны.
UI собирается префабами, а не кодом в Editor-билдере.

## Шаги

- [ ] `HudScreen` (View) + `HudPresenter`: уровень, очки, птицы, живые звёзды.
      Подписка на `LevelRuntime`; HUD **не** ретранслирует события и не вызывает `ShowResult` — этим занимается state machine.
- [ ] `LevelResultScreen` + `LevelResultPresenter`: заголовок, очки, звёзды; кнопки Next / Restart / Menu — команды в `GameStateMachine`.
      Публичные делегаты `PrimaryPressed/SecondaryPressed` убираются.
- [ ] `MainMenuScreen`: Play, (опционально) Quit. `MainMenuState` показывает экран.
- [ ] `LevelSelectScreen`: сетка уровней из `ILevelCatalog`, звёзды из `IProgressService` (пока заглушка до задачи 06).
- [ ] `PauseScreen` (минимально: Resume / Restart / Menu) — желательно, но не блокирует приёмку.
- [ ] Prefab-ы экранов в `Assets/AngryBirds/Prefabs/Ui/`, привязка в реестре `UiService`.
- [ ] Удалить `UiFactory`, `CreateUi/CreateHud/CreateResultPanel`, `CreateEventSystem` из `GameContentBuilder`; удалить `HudView`, `LevelResultPanel`.
- [ ] `StarRating` (Core/Domain) — единственное место подсчёта звёзд (если ещё не вынесено).

## Критерии приёмки

- [ ] Flow: MainMenu → LevelSelect → Gameplay (HUD) → Result → Next/Restart/Menu без ошибок.
- [ ] Все экраны открываются только через `UiService`.
- [ ] Старые `HudView`, `LevelResultPanel`, `UiFactory` удалены, ссылок на них нет.
- [ ] Проверка UI на нескольких разрешениях/соотношениях (16:9, 21:9, 4:3, портрет, если актуально): ничего не перекрывает игровое поле, кнопки кликабельны мышью и тачем.
- [ ] Console: 0 ошибок, 0 предупреждений.

Затронутые сцены/префабы: `MainMenu` (новая) или экран поверх Boot, `Gameplay`, `Prefabs/Ui/*`.
