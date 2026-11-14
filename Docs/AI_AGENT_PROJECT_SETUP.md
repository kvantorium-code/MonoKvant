# Первоначальная настройка Unity-проекта для ИИ-агента

Этот файл предназначен для нового или пустого Unity-проекта. Агент должен прочитать его и выполнить первичную настройку проекта: создать структуру папок, базовую документацию, ignore-файлы, `AGENTS.md` и подготовить место для подключения MCP.

Инструкция не должна содержать название конкретного проекта. Название проекта агент обязан запросить у пользователя перед созданием папок и файлов.

## Обязательный первый вопрос

Перед любыми изменениями агент должен спросить:

```text
Какое название проекта использовать для структуры Assets/<GameName>/?
```

Ответ пользователя использовать как `GameName`.

Правила для `GameName`:

- использовать короткое PascalCase-имя без пробелов;
- если пользователь дал имя с пробелами или спецсимволами, предложить нормализованный вариант и дождаться подтверждения;
- не создавать `Assets/<GameName>/` до получения названия проекта.

Пример:

```text
Название: Space Farm
Папка: Assets/SpaceFarm/
```

## Цель настройки

После выполнения инструкции в проекте должны появиться:

```text
Assets/
  <GameName>/
    Content/
    ThirdParty/
    Scenes/
    Scripts/
      Infrastracture/
      Ui/
      Localization/
      Core/
    Settings/
    Prefabs/

Doc/
  Architect-baseline/
  Dependency-injection/
  Features/
  Tasks/
  Reviews/
  ADR/
  Setup/

AGENTS.md
.gitignore
.agentignore
```

Если в проекте уже есть часть этих папок или файлов, агент должен сохранить существующее содержимое и дополнять его минимально, без перезаписи пользовательских данных.

## Структура папок Assets

Вся проектная game-specific структура должна находиться внутри:

```text
Assets/<GameName>/
```

Базовая структура:

```text
Assets/<GameName>/
  Content/       Игровой контент: модели, текстуры, материалы, аудио, анимации.
  ThirdParty/    Сторонние ассеты, плагины и SDK, если они не требуют другого расположения.
  Scenes/        Основные сцены проекта и scene templates.
  Scripts/       Runtime и editor C# код проекта.
  Settings/      ScriptableObject settings, конфиги, настройки систем.
  Prefabs/       Prefab-ассеты проекта.
```

Не размещать новый game-specific код и ассеты напрямую в корне `Assets`, если нет технической причины.

## Структура Scripts

Скрипты проекта должны быть сгруппированы так:

```text
Assets/<GameName>/Scripts/
  Infrastracture/  Bootstrap, composition root, DI installers/scopes, platform services.
  Ui/              UI panels, widgets, presenters, view components.
  Localization/    Localization keys, adapters, language switching, text binding.
  Core/            Основная gameplay/domain логика.
```

Правила:

- `Infrastracture` содержит wiring, composition root и интеграции, а не gameplay-логику.
- `Ui` не должен напрямую создавать сервисы; зависимости передавать через DI или явную инициализацию.
- `Localization` хранит адаптеры и ключи локализации, но не бизнес-логику.
- `Core` содержит доменные/gameplay системы без зависимости от UI, где это возможно.

Если проект использует asmdef, агент должен создавать их только после отдельного согласования или когда это явно требуется задачей.

## Структура Doc / Memory Bank

`Doc` является memory bank проекта. В нем хранится долговременный контекст для людей и ИИ-агентов.

Создать такую структуру:

```text
Doc/
  Architect-baseline/
    README.md
  Dependency-injection/
    README.md
  Features/
    README.md
  Tasks/
    README.md
  Reviews/
    README.md
  ADR/
    README.md
  Setup/
    README.md
```

Назначение разделов:

- `Doc/Architect-baseline` - архитектурная база проекта: слои, главные ограничения, правила зависимостей, границы модулей.
- `Doc/Dependency-injection` - правила работы с DI-контейнером. Если используется Zenject, описать installers, bindings, scene/project context и правила внедрения зависимостей.
- `Doc/Features` - описание реализованных и планируемых игровых features.
- `Doc/Tasks` - задачи на реализацию для агентов и разработчиков.
- `Doc/Reviews` - результаты code review, agent review и замечания по качеству.
- `Doc/ADR` - Architecture Decision Records: долговременные архитектурные решения, причины выбора и последствия.
- `Doc/Setup` - инструкции по локальной настройке, подключению SDK, MCP, билд-пайплайну.

Минимальное содержимое `README.md` в каждом разделе:

```markdown
# Section Name

Purpose:
- ...

Rules:
- ...

Update policy:
- Update this document when project behavior, architecture, dependencies, or workflow changes.
```

## ADR-подход

ADR (`Architecture Decision Record`) - это короткий документ, который фиксирует важное архитектурное решение проекта. Цель ADR - сохранить не только итоговое решение, но и контекст: почему выбран именно этот вариант, какие альтернативы рассматривались и какие последствия у решения.

Агент должен создавать ADR в `Doc/ADR`, когда изменение:

- влияет на архитектуру проекта, слои, границы модулей или направление зависимостей;
- добавляет, заменяет или удаляет важную технологию, SDK, DI-фреймворк, сохранения, локализацию, сетевой слой;
- вводит новый устойчивый паттерн, которому должен следовать будущий код;
- меняет правила работы с ресурсами, сценами, prefab wiring или bootstrap flow;
- имеет долгосрочные последствия и должно быть понятно будущему агенту без повторного анализа истории.

Не нужно создавать ADR для мелких багфиксов, локальных UI-правок, переименований, форматирования или задач без архитектурных последствий.

Формат имени ADR:

```text
Doc/ADR/0001-short-decision-title.md
Doc/ADR/0002-use-zenject-for-dependency-injection.md
```

Нумерация должна быть монотонной. Новый ADR получает следующий свободный номер.

Статусы ADR:

- `Proposed` - решение предложено, но еще не принято.
- `Accepted` - решение принято и должно соблюдаться.
- `Deprecated` - решение устарело, но исторически важно.
- `Superseded` - решение заменено другим ADR; нужно указать ссылку на новый ADR.

Шаблон ADR:

```markdown
# ADR-0001: Short Decision Title

## Status

Accepted

## Context

What problem are we solving?
What constraints, project rules, or existing systems matter?

## Decision

What decision was made?

## Alternatives Considered

- Option A:
- Option B:
- Option C:

## Consequences

- Positive:
- Negative:
- Risks:

## Follow-up

- Documentation to update:
- Code areas affected:
- Validation needed:
```

`Doc/ADR/README.md` должен содержать индекс ADR:

```markdown
# Architecture Decision Records

| ADR | Status | Title | Date |
| --- | --- | --- | --- |
| 0001 | Accepted | Short Decision Title | YYYY-MM-DD |
```

При изменении архитектурного решения агент не должен переписывать старый ADR задним числом. Нужно создать новый ADR и пометить старый как `Superseded`, если решение заменено.

## AGENTS.md

Агент должен создать root `AGENTS.md`, если его еще нет.

Если `AGENTS.md` уже существует, не перезаписывать его. Вместо этого:

1. прочитать текущий файл;
2. определить, каких базовых разделов не хватает;
3. предложить минимальный patch;
4. применить patch только после подтверждения пользователя, если есть риск затереть существующие правила.

Минимальная структура `AGENTS.md` для нового проекта:

```markdown
# AGENTS.md

## Project Context

- Read `Doc/Architect-baseline/README.md` before architectural changes.
- Read `Doc/Dependency-injection/README.md` before adding or modifying dependencies.
- Read relevant files in `Doc/Features` before changing feature behavior.
- Create implementation task notes in `Doc/Tasks` when the work is task-based.
- Put review results in `Doc/Reviews`.
- Record long-lived architecture decisions as ADR files in `Doc/ADR`.

## General Rules

- Make minimal, targeted changes.
- Preserve existing architecture and public APIs unless the task explicitly requires changes.
- Prefer Unity APIs and established project patterns.
- Avoid unnecessary allocations in frequently called runtime code.
- Do not rewrite unrelated systems.

## Unity Rules

- Avoid heavy logic in `Update()`.
- Cache component references instead of repeatedly calling `GetComponent`.
- Prefer event-driven logic where practical.
- Use serialized references for scene and prefab wiring when appropriate.
- Keep runtime code separate from editor-only code.
- Put editor scripts inside `Editor` folders.

## Dependency Injection

- Follow `Doc/Dependency-injection/README.md`.
- Do not create services directly inside gameplay or UI classes when they belong in DI.
- Register project-wide services in the project-level composition root.
- Register scene-specific dependencies in scene-level installers/scopes.
- For `MonoBehaviour` classes, prefer method/property injection patterns supported by the selected DI framework.

## Documentation Routing

- Architecture rules: `Doc/Architect-baseline/README.md`.
- Dependency rules: `Doc/Dependency-injection/README.md`.
- Feature behavior: `Doc/Features/`.
- Architecture decisions: `Doc/ADR/`.
- Setup and tooling: `Doc/Setup/`.
- Task execution notes: `Doc/Tasks/`.
- Review notes: `Doc/Reviews/`.

## Validation

- After script changes, verify Unity compilation.
- Check Console errors and warnings.
- Mention affected scenes and prefabs when manual inspector setup is required.
- If validation cannot be run, explain why.
```

## Dependency Injection: Zenject

Если проект использует Zenject, агент должен создать или обновить:

```text
Doc/Dependency-injection/README.md
```

Минимальный стартовый текст:

```markdown
# Dependency Injection

DI framework: Zenject.

## Rules

- Use Zenject installers/composition roots for service registration.
- Do not instantiate services directly inside gameplay or UI classes.
- Keep project-wide bindings separate from scene-specific bindings.
- Prefer constructor injection for plain C# services.
- For `MonoBehaviour`, use Zenject-supported injection patterns and avoid using injected dependencies before injection is complete.
- Document every new long-lived service binding in this file.

## Binding Locations

- Project-level bindings:
  - TBD
- Scene-level bindings:
  - TBD

## Validation

- Verify that installers are present in the relevant scene/context.
- Verify that all injected dependencies have bindings.
- Check Unity Console after changing installers or bindings.
```

Если проект использует другой DI-фреймворк или DI не используется, агент должен спросить пользователя, какой вариант выбрать, перед заполнением `Doc/Dependency-injection/README.md`.

## Ignore-файлы

Агент должен создать базовые `.gitignore` и `.agentignore`, если их нет.

Если файлы уже существуют, агент не должен перезаписывать их полностью. Нужно добавить только отсутствующие правила.

Минимальный `.gitignore` для Unity:

```gitignore
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/

*.csproj
*.sln
*.slnx
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

.vs/
.vscode/
.idea/

*.apk
*.aab
*.unitypackage
*.log
```

Минимальный `.agentignore`:

```gitignore
Library/
Temp/
Obj/
Logs/
Build/
Builds/
UserSettings/
.vs/
.idea/
node_modules/

*.csproj
*.sln
*.slnx
*.dll
*.exe
*.pdb
*.log
*.tmp
*.cache
```

Если используется конкретный агентский инструмент (`.codexignore`, `.cursorignore`, `.windsurfignore` и т.п.), агент может создать соответствующий файл на основе `.agentignore`.

## Cleanup после создания проекта

После первоначальной настройки агент должен провести cleanup стандартного пустого Unity-проекта. Цель cleanup - убрать шаблонные файлы, tutorial artifacts и лишние зависимости, которые Unity создает автоматически, но которые не являются частью проекта.

Перед удалением агент должен:

1. показать пользователю список найденных кандидатов на удаление;
2. объяснить, почему они считаются шаблонными/лишними;
3. удалить их только после подтверждения пользователя, если есть риск затронуть полезный файл или пакет.

Типичные кандидаты на cleanup:

```text
README.md
TutorialInfo/
TutorialInfo.asset
Assets/TutorialInfo/
Assets/Readme.asset
Assets/Scenes/SampleScene.unity
Assets/Scenes/SampleScene.unity.meta
```

Также проверить Unity packages:

```text
com.unity.inputsystem
com.unity.learn.iet-framework
com.unity.learn.iet-framework.authoring
com.unity.feature.development
```

Правила для packages cleanup:

- `com.unity.inputsystem` удалять только если проект точно не использует новый Input System.
- Если проект планирует мобильный/геймпадный/сложный input, сначала спросить пользователя, нужен ли Input System.
- Tutorial/learn packages можно удалить, если они пришли из шаблона и не используются в проекте.
- После изменения `Packages/manifest.json` обязательно дождаться обновления `Packages/packages-lock.json`.
- После удаления пакетов открыть Unity, дождаться package restore и проверить Console.

Правила для asset cleanup:

- Не удалять пользовательские сцены, ассеты и документацию.
- Не удалять `ProjectSettings`, `Packages`, `Assets/<GameName>` и созданный memory bank.
- Если `SampleScene` уже используется как рабочая сцена, перенести или переименовать ее в `Assets/<GameName>/Scenes/` вместо удаления.
- При удалении Unity assets удалять соответствующие `.meta` файлы.

Результат cleanup нужно записать в:

```text
Doc/Setup/Cleanup.md
```

Стартовый шаблон `Doc/Setup/Cleanup.md`:

```markdown
# Initial Project Cleanup

## Removed Files

- TBD

## Removed Packages

- TBD

## Kept Intentionally

- TBD

## Validation

- Unity opened successfully.
- Package restore completed.
- Console has no cleanup-related errors.
```

## MCP-сервер для Unity

Использовать MCP for Unity от CoplayDev.

Unity Package Manager git URL:

```text
https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
```

Package name после установки:

```text
com.coplaydev.unity-mcp
```

Агент должен только добавить этот пакет в Unity. Отдельную документацию по настройке MCP-клиентов создавать не нужно: после установки пакета MCP for Unity настраивается через окно Unity Editor.

Агент не должен автоматически конфигурировать MCP-клиенты:

- не запускать `Auto-Setup`;
- не запускать `Configure All Detected Clients`;
- не создавать и не изменять `.codex/config.toml`;
- не создавать и не изменять `opencode.json`;
- не создавать и не изменять `.opencode/` ради MCP;
- не запускать MCP server/bridge без отдельной команды пользователя.

Подключение Unity package:

1. открыть `Packages/manifest.json`;
2. проверить, нет ли уже dependency `com.coplaydev.unity-mcp`;
3. добавить package через Unity Package Manager git URL:
   `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`;
4. дождаться обновления `Packages/packages-lock.json`;
5. открыть Unity и дождаться завершения компиляции;
6. открыть `Window -> MCP for Unity`;
7. не нажимать `Auto-Setup` / `Configure All Detected Clients` без отдельного запроса пользователя;
8. сообщить пользователю, что дальнейшая настройка выполняется в Unity window `MCP for Unity`.

## Алгоритм выполнения настройки

Агент должен выполнять настройку в таком порядке:

1. Спросить название проекта (`GameName`).
2. При необходимости нормализовать название и получить подтверждение.
3. Создать `Assets/<GameName>/` и базовые подпапки.
4. Создать `Assets/<GameName>/Scripts/` и подпапки `Infrastracture`, `Ui`, `Localization`, `Core`.
5. Создать дерево `Doc`.
6. Создать README-файлы в разделах `Doc`.
7. Создать или дополнить `AGENTS.md`.
8. Создать или дополнить `.gitignore`.
9. Создать или дополнить `.agentignore`.
10. Подготовить подключение Unity MCP package по git URL.
11. Провести cleanup стандартных template/tutorial файлов и лишних packages.
12. Создать `Doc/Setup/Cleanup.md` с результатами cleanup.
13. Проверить итоговое дерево файлов.
14. Сообщить пользователю, какие файлы созданы, изменены или удалены.

## Правила безопасности

- Не удалять существующие папки и файлы.
- Не перезаписывать существующий `AGENTS.md`, ignore-файлы или docs без анализа.
- Не добавлять другой MCP package вместо `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` без явного указания пользователя.
- Не конфигурировать MCP-клиенты автоматически: `.codex/config.toml`, `opencode.json` и `.opencode/` менять только по отдельному явному запросу пользователя.
- Не удалять template/tutorial файлы и packages без проверки, что они не используются.
- Не создавать проектно-специфичные названия из головы.
- Не указывать версию Unity в документации инициализации; агент должен определить ее из проекта при необходимости.
- Не создавать сложную архитектуру заранее. На старте нужны только папки, правила и места для документации.
