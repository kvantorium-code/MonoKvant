# 04. Ядро UI-слоя

Статус: todo · Зависит от: 02 · Блокирует: 05

## Цель

Единый способ показывать, прятать и создавать экраны. Все экраны проекта следуют одному контракту.

## Шаги

- [ ] Подключить TextMeshPro (проверить наличие TMP Essentials/шрифта и зарегистрировать ассеты в `Assets/AngryBirds/Content`).
- [ ] `Ui/Core/UiScreenId` — enum/идентификаторы экранов.
- [ ] `Ui/Core/ScreenBase` — базовый MonoBehaviour с жизненным циклом
      `Show()` / `Hide()` и `CanvasGroup` для блокировки ввода; `ScreenBase<TViewModel>` с `Bind(model)`.
- [ ] `Ui/Core/IScreenPresenter` — `Bind/Unbind/Dispose`; презентер подписывается на сервисы, view — только поля и анимации.
- [ ] `Ui/Core/UiRoot` — Canvas, `CanvasScaler` (1920×1080), `GraphicRaycaster`, слои (HUD, Screens, Popups, Overlay) c фиксированным порядком;
      EventSystem с `InputSystemUIInputModule` живёт здесь, а не в сценах.
- [ ] `Ui/Core/UiService` (project-level singleton): `Show<TScreen>()`, `Hide<TScreen>()`, `Get<TScreen>()`;
      реестр `UiScreenId → prefab`, ленивая инстанциация.
- [ ] `Ui/Widgets/`: `StarRow`, `LabeledCounter`, `PrimaryButton` — переиспользуемые компоненты (на них перейдут HUD и Result).
- [ ] Регистрации в `ProjectLifetimeScope`: `UiRoot` (префаб), `UiService`.

## Критерии приёмки

- [ ] `UiService` умеет показать и спрятать тестовый экран из Boot-сцены без обращений к `FindObject`/`Camera.main`.
- [ ] Ни один класс `Ui` не создаёт сервисы сам; зависимости только через DI/явную инициализацию.
- [ ] Console: 0 ошибок, 0 предупреждений.

Затронутые префабы: `UiRoot.prefab` (новый), префабы виджетов.
