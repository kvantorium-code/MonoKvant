# Unity 1. База

Unity 6, C#, новая Input System.

## 1. Как устроен Unity

```
Сцена (уровень)
 └─ GameObject (любая вещь в сцене)
     └─ Компоненты (что объект умеет)
         Transform: позиция, поворот, масштаб (есть всегда)
         MeshRenderer: внешний вид
         Collider: форма для столкновений
         Rigidbody: физика
         Твой скрипт: поведение
```

- Объект сам по себе пустой, всё, что он умеет, дают компоненты. Скрипт — тоже компонент: пока его не повесили на объект, код не работает.
- **Префаб** — сохранённый шаблон объекта. Если изменить префаб, изменятся все его копии в сцене.
- 1 единица = 1 метр. Ось Y направлена вверх. Дочерний объект двигается вместе с родителем.

| Окно | Для чего |
|---|---|
| Hierarchy | Объекты сцены |
| Scene / Game | Редактирование сцены / вид игрока |
| Inspector | Компоненты выделенного объекта |
| Project | Файлы проекта |
| Console | Ошибки. Держи её открытой |

Горячие клавиши: `W` `E` `R` — двигать, вращать, масштабировать; `F` — показать объект; ПКМ + `WASD` — полёт по сцене; `Ctrl+D` — дублировать; `Ctrl+P` — Play.
**Изменения, сделанные в режиме Play, не сохраняются.**

📖 Подробнее: [GameObject](https://docs.unity3d.com/Manual/GameObjects.html) · [Transform](https://docs.unity3d.com/Manual/class-Transform.html) · [Префабы](https://docs.unity3d.com/Manual/Prefabs.html) · [Интерфейс редактора](https://docs.unity3d.com/Manual/unity-editor.html) · [Горячие клавиши](https://docs.unity3d.com/Manual/ShortcutsManager.html)

## 2. Скрипт

```csharp
using UnityEngine;

public class Mover : MonoBehaviour            // имя класса = имя файла
{
    [SerializeField] float speed = 5f;        // настраивается в Inspector
    Rigidbody rb;

    void Awake()  { rb = GetComponent<Rigidbody>(); } // при создании: ссылки на свои компоненты
    void Start()  { }                                  // перед первым кадром
    void Update() { }                                  // каждый кадр: ввод, таймеры
    void FixedUpdate() { }                             // шаг физики: работа с Rigidbody
}
```

- Всё, что меняется «в секунду», умножай на `Time.deltaTime`, иначе скорость игры будет зависеть от FPS:
  `transform.Translate(Vector3.forward * speed * Time.deltaTime);`
- Ссылку на другой объект проще всего получить так: объяви поле `[SerializeField] Transform player;` и перетащи объект в это поле в Inspector.
- Для вывода значений: `Debug.Log($"hp = {hp}");`

📖 Подробнее: [Создание скриптов](https://docs.unity3d.com/Manual/creating-scripts.html) · [MonoBehaviour](https://docs.unity3d.com/Manual/class-MonoBehaviour.html) · [Порядок вызова Awake/Start/Update](https://docs.unity3d.com/Manual/execution-order.html) · [Time.deltaTime](https://docs.unity3d.com/ScriptReference/Time-deltaTime.html) · [SerializeField](https://docs.unity3d.com/ScriptReference/SerializeField.html)

## 3. Ввод, движение, физика

```csharp
using UnityEngine.InputSystem;

var k = Keyboard.current;
float x = (k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0);
if (k.spaceKey.wasPressedThisFrame) Jump();
```

Как двигать объект:
- **без физики:** через `transform.position` или `Translate` в `Update`;
- **с физикой:** через `rb.AddForce(...)` или `rb.linearVelocity = ...` в `FixedUpdate`.

Если у объекта есть Rigidbody, не двигай его через `transform`, иначе он будет проходить сквозь стены.

- **Collider** — форма объекта. **Rigidbody** — масса и гравитация. **Is Trigger** — зона, сквозь которую можно пройти, но при входе срабатывает событие.
- События срабатывают, только если **хотя бы у одного** из двух объектов есть Rigidbody.

```csharp
void OnCollisionEnter(Collision c) { }        // удар (оба коллайдера не триггеры)
void OnTriggerEnter(Collider other)           // вход в триггер
{
    if (other.CompareTag("Coin")) Destroy(other.gameObject);
}
```

В 2D всё то же самое с суффиксом 2D: `Rigidbody2D`, `OnTriggerEnter2D`. 2D- и 3D-физика друг с другом не взаимодействуют.

📖 Подробнее: [Input System: быстрый старт](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/manual/QuickStartGuide.html) · [Rigidbody](https://docs.unity3d.com/Manual/RigidbodiesOverview.html) · [Коллайдеры и столкновения](https://docs.unity3d.com/Manual/CollidersOverview.html) · [Взаимодействие коллайдеров](https://docs.unity3d.com/Manual/collider-interactions.html) · [OnTriggerEnter](https://docs.unity3d.com/ScriptReference/Collider.OnTriggerEnter.html)

## 4. Префабы, UI, сцены

```csharp
[SerializeField] GameObject prefab;                      // перетащи префаб из Project
Instantiate(prefab, position, Quaternion.identity);      // создать копию
Destroy(gameObject, 2f);                                 // удалить через 2 секунды
```

**UI.** Текст создаётся через `GameObject → UI → Text - TextMeshPro`; Canvas создастся сам. В коде нужен `using TMPro;`, поле `[SerializeField] TMP_Text text;`, а вывод — `text.text = $"Счёт: {score}";`.

**Кнопка.** В её компоненте найди `On Click`, нажми `+`, перетащи объект со скриптом и выбери публичный метод.

```csharp
using UnityEngine.SceneManagement;
SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);  // перезапуск
Time.timeScale = 0;  // пауза; перед перезапуском вернуть 1
```

Сцены нужно добавить в список: `File → Build Profiles → Scene List`.

📖 Подробнее: [Instantiate](https://docs.unity3d.com/ScriptReference/Object.Instantiate.html) · [Destroy](https://docs.unity3d.com/ScriptReference/Object.Destroy.html) · [UI: разметка](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UIBasicLayout.html) · [Button](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Button.html) · [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/index.html) · [SceneManager.LoadScene](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadScene.html)

## 5. Если не работает

| Проблема | Причина |
|---|---|
| `NullReferenceException` | Поле не назначено в Inspector |
| Скрипт не вешается на объект | Имя класса не совпадает с именем файла, или в Console есть красные ошибки |
| Триггер молчит | Нет Rigidbody, не стоит Is Trigger, неверный тег, смешаны 2D и 3D |
| Ошибка про `UnityEngine.Input` | Используется старый `Input.GetKey`, нужен `Keyboard.current` |
| Розовые материалы | Шейдер не для URP, выбери `URP/Lit` |

Первым делом читай **первую** красную ошибку в Console. Двойной клик по ней открывает нужную строку кода.

📖 Подробнее: [Окно Console](https://docs.unity3d.com/Manual/Console.html)

**Дальше:** курс [Unity Essentials](https://learn.unity.com/pathway/unity-essentials) (есть на русском), затем [Junior Programmer](https://learn.unity.com/pathway/junior-programmer). Справочник по коду — [Scripting API](https://docs.unity3d.com/ScriptReference/index.html).
