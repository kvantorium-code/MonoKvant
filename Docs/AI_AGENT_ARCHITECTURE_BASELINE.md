# Базовая архитектура Unity-проекта

Этот документ — спутник `Docs/AI_AGENT_PROJECT_SETUP.md`. Setup создает скелет проекта (папки, `Doc/`, `AGENTS.md`), этот документ определяет, какая архитектура должна жить внутри скелета.

Действует для всех Unity-проектов воркспейса. Документ проектонезависим: вместо имени игры используется `<GameName>`. Конкретный проект фиксирует свои детали в `Doc/Architect-baseline/README.md`, отклонения от этого базлайна — через ADR в `Doc/ADR/`.

Назначение: короткий, обязательный к соблюдению свод правил — для людей и ИИ-агентов.

## 1. Структура папок

Весь game-specific код и контент — только внутри `Assets/<GameName>/`. В корне `Assets/` ничего своего не размещаем.

```text
Assets/<GameName>/
  Content/         Игровой контент: модели, текстуры, материалы, аудио, анимации.
  ThirdParty/      Сторонние ассеты, плагины, SDK (если не требуют другого расположения).
  Scenes/          Сцены проекта: Boot, Loader, игровые сцены.
  Scripts/         Runtime- и editor-код проекта.
    Infrastracture/  Bootstrap, composition root, DI-скопы, загрузка сцен, платформенные сервисы.
    Ui/              View и Controller экранов, виджеты.
    Localization/    Ключи локализации, адаптеры, переключение языка.
    Core/            Доменная/gameplay-логика, по возможности без Unity-зависимостей.
    Editor/          Editor-only код (обязательная папка `Editor`).
  Settings/        ScriptableObject-конфиги и настройки систем.
  Prefabs/         Префабы проекта (в том числе префаб ProjectLifetimeScope).
```

Правила зависимостей между слоями (`Scripts/`):

```text
Infrastracture ──► Core ◄── Ui
        └────────────┴─────► Localization (все могут, Localization не зависит ни от кого)
```

- Направление зависимостей — только внутрь: `Ui` и `Infrastracture` зависят от `Core`, никогда наоборот.
- `Core` не знает про UI, сцены и DI-фреймворк — это чистая логика, удобная для тестов.
- `Infrastracture` — только wiring и интеграции, без gameplay-логики.
- `Ui` не создает сервисы и не ходит в `Core` напрямую в обход контроллеров/интерфейсов.

## 2. Сцены: Boot → Loader → игровые сцены

Три вида сцен:

| Сцена | Build settings | Назначение |
| --- | --- | --- |
| `Boot` | index 0, единственная в build при старте | Composition root проекта: `ProjectLifetimeScope`, точка входа. Ничего не рисует, сразу передает управление дальше. |
| `Loader` | всегда в build | «Шторка» с прогресс-баром. Грузится **аддитивно** поверх текущей сцены на время перехода и выгружается после. |
| Игровые (`MainMenu`, `Gameplay`, ...) | всегда в build | Реальный контент. Активна одна; у каждой — свой scene-скоп. |

Важно: `LoadSceneMode.Single` выгружает и аддитивные сцены, поэтому шторка умрет вместе с переходом. Все переходы делаются аддитивно, предыдущая сцена выгружается вручную:

```text
Boot (index 0, живет всегда)
  └► GameSceneManager.Load("Gameplay")
       1. LoadSceneAdditive(Loader)      — шторка появляется, закрывает экран
       2. LoadSceneAdditive(Gameplay)    — прогресс 0..1 (op.progress: 0..0.9 ≈ 0..90%)
       3. Unload(prev scene)             — старая сцена выгружается
       4. Unload(Loader)                 — шторка уходит после инициализации новой сцены
```

`GameSceneManager` (интерфейс `IGameSceneManager`) — сервис, оборачивающий всю работу со сценами:

```csharp
public interface IGameSceneManager
{
    Task Load(string sceneName, object payload = null, CancellationToken ct = default);
}
```

Правила:

- `IGameSceneManager` — единственное место в проекте, откуда вызывается `UnityEngine.SceneManagement.SceneManager`. Прямые вызовы `LoadScene`/`UnloadScene` в другом коде запрещены.
- Шторка, прогресс и порядок выгрузки — целиком внутри `GameSceneManager`, наружу не торчат.
- Имена сцен — константы или enum (например, класс `SceneNames`), без магических строк.
- Данные для новой сцены (номер уровня и т.п.) передаются через `payload`, а не через статику.
- QoL-правило: игровая сцена, открытая напрямую в редакторе, должна запускаться (scene-скоп сам создает fallback project-scope), чтобы можно было работать без прохождения Boot.

## 3. Зависимости: DI на VContainer

Стандарт воркспейса — **VContainer**. Zenject (или другой фреймворк) допускается только по отдельному решению, зафиксированному в ADR проекта.

Два уровня composition root:

| Уровень | Где живет | Что регистрирует |
| --- | --- | --- |
| `ProjectLifetimeScope` | Префаб в `Prefabs/`, экземпляр в сцене `Boot`, `DontDestroyOnLoad` | Сервисы на все время жизни приложения: `IGameSceneManager`, инпут, каталоги уровней, state machine, аналитика и т.п. |
| Scene scope (`<Scene>LifetimeScope`) | Объект в игровой сцене, parent = project scope | Зависимости конкретной сцены: `LevelRuntime`, спавнеры, view-компоненты, entry point сцены. Пересоздается на каждой загрузке сцены. |

Правила инъекций:

- Проводится через конструктор для обычных C#-классов; для `MonoBehaviour` — метод `[Inject] Construct(...)`. Не использовать зависимости до завершения инъекции.
- Зависимся от интерфейсов (`ISceneLoader`, `IGameInput`), конкретные типы упоминаются только в composition root.
- Service locator (`IObjectResolver.Resolve` в бизнес-коде) и статические синглтоны запрещены — только DI.
- Проектные binding'и отдельно от сценовых; каждый долгоживущий биндинг записывается в таблицу `Doc/Dependency-injection/README.md` проекта.

## 4. Модульная архитектура сервисов

Сервис — самодостаточный класс за интерфейсом, с одной зоной ответственности, зарегистрированный ровно в одном скопе (проектном или сценовом).

Правила модульности:

- Сервис не знает о существовании других сервисов. Взаимодействие — только через их интерфейсы или через события/сигналы (C# `event`, `IObservable`), никогда — через конкретные типы.
- Тест на модульность: сервис можно удалить, и правки понадобятся только в composition root. Если удаление сервиса ломает чужой код — архитектура нарушена.
- У сервиса явный lifetime: проектный (живет всегда) или сценовый (умирает со сценой). Сценовые сервисы отписываются от проектных событий в `Dispose`.
- Доменная логика — в `Core`, по возможности plain C#. Все платформенное (сохранения, реклама, аналитика, инпут) — в `Infrastracture` за интерфейсом, чтобы `Core` оставался тестируемым.
- Новый сервис начинается с интерфейса в папке своего слоя + регистрация в скопе + строка в `Doc/Dependency-injection/README.md`.

## 5. UI: View + Controller

Каждый экран/виджет состоит из двух частей с четким разделением:

**View** (`XxxView : MonoBehaviour`, в `Scripts/Ui/...`) — чистый визуал:

- Показывает данные: `SetStars(int)`, `SetScore(int)`, `Show()` / `Hide()`, анимации появления.
- Сообщает о действиях пользователя событиями: `event Action RestartClicked`.
- НЕ содержит: бизнес-логику, ссылки на сервисы, инъекции DI, обращение к `Core`. Единственная «логика» — display-хелперы (твин альфы, форматирование строки).

**Controller** (обычный C#-класс, при необходимости `MonoBehaviour`, в `Scripts/Ui/...` рядом со своей View):

- Получает зависимости через DI: сервисы, `IGameSceneManager`, другие контроллеры.
- Подписывается на события View в `Initialize`, отписывается в `Dispose`.
- Инициализирует View данными, реагирует на его события, зовет сервисы, решает навигацию.
- View никогда не ищет и не знает свой контроллер; связка выполняется в скопе/entry point сцены.

```csharp
// View — только визуал
public sealed class LevelResultView : MonoBehaviour
{
    public event Action RestartClicked;

    public void SetStars(int count) { /* обновить иконки */ }
    public void Show() { /* ... */ }
    public void Hide() { /* ... */ }
}

// Controller — вся логика, зависимости через DI
public sealed class LevelResultController : IInitializable, IDisposable
{
    private readonly LevelResultView _view;
    private readonly ILevelProgressService _progress; // инжект

    public LevelResultController(LevelResultView view, ILevelProgressService progress)
    {
        _view = view;
        _progress = progress;
    }

    public void Initialize()
    {
        _view.SetStars(_progress.LastStars);
        _view.RestartClicked += OnRestartClicked;
    }

    public void Dispose() => _view.RestartClicked -= OnRestartClicked;

    private void OnRestartClicked() => /* перезапуск уровня */;
}
```

Связка: view-компоненты регистрируются в scene scope (`RegisterComponent`), контроллеры — как обычные сервисы; либо связка выполняется в entry point сцены.

## Сводка для агентов

- Новый код и ассеты — только в `Assets/<GameName>/`, слой выбирается по назначению (см. §1).
- Направление зависимостей: `Ui`/`Infrastracture` → `Core`, никогда наоборот.
- Переходы между сценами — только через `IGameSceneManager`; никаких прямых `LoadScene`.
- Все зависимости — через DI (VContainer). Интерфейс вместо конкретного типа; конкретика только в скопах.
- Новые сервисы: интерфейс → регистрация в скопе → строка в `Doc/Dependency-injection/README.md`.
- UI: View — визуал и события; Controller — логика и DI. View без сервисов, Controller без рендера.
- Отклонение от любого правила здесь — сначала ADR в `Doc/ADR/`, потом код.

## Что где документируется

| Тема | Место |
| --- | --- |
| Первичная настройка нового проекта | `Docs/AI_AGENT_PROJECT_SETUP.md` (этот репозиторий) |
| Базовая архитектура (этот документ) | `Docs/AI_AGENT_ARCHITECTURE_BASELINE.md` (этот репозиторий) |
| Архитектурный базлайн конкретного проекта | `Doc/Architect-baseline/README.md` проекта |
| DI-биндинги проекта | `Doc/Dependency-injection/README.md` проекта |
| Архитектурные решения и отклонения | `Doc/ADR/` проекта |
