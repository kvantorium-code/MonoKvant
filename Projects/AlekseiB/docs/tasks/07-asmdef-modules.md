# 07. Assembly definitions по модулям

Статус: todo · Зависит от: 06 · Блокирует: 08

## Цель

Закрепить направление зависимостей на уровне компиляции и ускорить итерации.

## Предусловие

Явное согласие владельца на `.asmdef` (правило `AGENTS.md`). Выполняется последним среди
структурных задач, когда границы папок уже стабильны.

## Шаги

- [ ] `AngryBirds.Core` — `Scripts/Core` (Damage, Domain, Gameplay, Level). Ссылки: только Unity-модули, Unity.InputSystem если нужен.
- [ ] `AngryBirds.Localization` — `Scripts/Localization`. Ссылок на Ui/Infrastracture нет.
- [ ] `AngryBirds.Ui` — `Scripts/Ui`. Ссылки: `Core`, `Localization`, TMP, ugui.
- [ ] `AngryBirds.Infrastracture` — `Scripts/Infrastracture`. Ссылки: `Core`, `Ui`, `Localization`, VContainer, Input System.
- [ ] `AngryBirds.Editor` — `Scripts/Editor`, `includePlatforms: [Editor]`. Ссылки на все рантайм-сборки.
- [ ] Обновить пространства имён там, где они расходятся со структурой (в т.ч. `AngryBirds.Infrastructure` vs. папка `Infrastracture`) — только если это не ломает сериализованные ссылки; иначе оставить как есть и описать в ADR.
- [ ] Проверить, что `Core` не ссылается на `Ui`/`Infrastracture` (цикл не должен компилироваться).
- [ ] Проверить сцены и префабы: ссылки на скрипты сохранились (GUID `.meta` не менялись).

## Критерии приёмки

- [ ] Проект компилируется, все ссылки в сценах и префабах целы, Play Mode проходит.
- [ ] Console: 0 ошибок, 0 предупреждений.
- [ ] Добавление `using AngryBirds.Ui` в `Core` приводит к ошибке компиляции (проверить и откатить).
