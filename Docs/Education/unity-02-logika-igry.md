# Unity 2. Логика игры

Как объекты находят друг друга, как работать со временем и как устроен игровой цикл.

## 1. Как объекты находят друг друга

```csharp
[SerializeField] Transform player;                    // 1. Через Inspector: самый простой способ
rb = GetComponent<Rigidbody>();                       // 2. Компонент на этом же объекте
if (other.TryGetComponent(out Health h)) h.Damage(10);// 3. Компонент того, с кем столкнулись
player = GameObject.FindWithTag("Player").transform;  // 4. Поиск: только в Awake/Start, не в Update
```

Чтобы один скрипт мог вызвать метод другого, этот метод должен быть `public`:

```csharp
public class Health : MonoBehaviour
{
    [SerializeField] int hp = 100;
    public void Damage(int value)
    {
        hp -= value;
        if (hp <= 0) Destroy(gameObject);
    }
}
```

📖 Подробнее: [GetComponent](https://docs.unity3d.com/ScriptReference/Component.GetComponent.html) · [TryGetComponent](https://docs.unity3d.com/ScriptReference/Component.TryGetComponent.html) · [FindWithTag](https://docs.unity3d.com/ScriptReference/GameObject.FindWithTag.html) · [Теги](https://docs.unity3d.com/Manual/Tags.html)

## 2. Менеджер игры

Один объект `GameManager` хранит счёт и состояние игры. Остальные скрипты обращаются к нему через `GameManager.Instance`.

```csharp
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event Action<int> ScoreChanged;   // событие «счёт изменился»
    int score;

    void Awake() => Instance = this;

    public void AddScore(int value)
    {
        score += value;
        ScoreChanged?.Invoke(score);         // оповестить всех подписчиков
    }
}
```

- Монета при сборе вызывает `GameManager.Instance.AddScore(1);`.
- UI подписывается на событие и сам обновляет текст. Менеджеру не нужно знать про UI:

```csharp
void Start()     => GameManager.Instance.ScoreChanged += OnScore;
void OnDestroy() { if (GameManager.Instance) GameManager.Instance.ScoreChanged -= OnScore; }
void OnScore(int s) => text.text = $"Счёт: {s}";
```

📖 Подробнее: [Паттерн Singleton (англ.)](https://gameprogrammingpatterns.com/singleton.html) · [Паттерн Observer (англ.)](https://gameprogrammingpatterns.com/observer.html) · [События в C# (рус.)](https://learn.microsoft.com/ru-ru/dotnet/csharp/programming-guide/events/)

## 3. Время: таймеры и корутины

**Таймер в Update**, например стрельба не чаще раза в полсекунды:

```csharp
float cooldown;
void Update()
{
    cooldown -= Time.deltaTime;
    if (Keyboard.current.spaceKey.isPressed && cooldown <= 0)
    {
        Shoot();
        cooldown = 0.5f;
    }
}
```

**Корутина** — действие, растянутое во времени: «сделай, подожди, сделай ещё».

```csharp
using System.Collections;

IEnumerator SpawnWaves()
{
    for (int wave = 1; wave <= 3; wave++)
    {
        for (int i = 0; i < wave * 3; i++) Instantiate(enemy, RandomPoint(), Quaternion.identity);
        yield return new WaitForSeconds(10f);   // пауза 10 секунд
    }
}
// запуск: StartCoroutine(SpawnWaves());
```

📖 Подробнее: [Корутины](https://docs.unity3d.com/Manual/Coroutines.html) · [WaitForSeconds](https://docs.unity3d.com/ScriptReference/WaitForSeconds.html)

## 4. Луч (Raycast) и слои

Луч отвечает на вопрос, что находится в заданном направлении на заданном расстоянии. Через него проверяют, стоит ли персонаж на земле, стреляют без пуль и выбирают объект мышкой.

```csharp
[SerializeField] LayerMask groundMask;    // в Inspector выбери слой Ground

bool IsGrounded() =>
    Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);

// Клик мышкой по объекту
Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
if (Physics.Raycast(ray, out RaycastHit hit)) Debug.Log(hit.collider.name);
```

**Слой (Layer)** назначается в правом верхнем углу Inspector. Слои позволяют лучу видеть только землю, а в `Project Settings → Physics → Layer Collision Matrix` можно запретить, например, пулям сталкиваться друг с другом.

📖 Подробнее: [Physics.Raycast](https://docs.unity3d.com/ScriptReference/Physics.Raycast.html) · [Слои и LayerMask](https://docs.unity3d.com/Manual/layers-and-layermasks.html) · [Матрица столкновений слоёв](https://docs.unity3d.com/Manual/LayerBasedCollision.html)

## 5. Данные отдельно от кода

**ScriptableObject** — файл с параметрами. Пример: один скрипт врага и разные файлы для «Гоблина» и «Орка».

```csharp
[CreateAssetMenu(menuName = "Game/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    public int hp = 100;
    public float speed = 3f;
}
```

Файл создаётся так: ПКМ в Project → `Create → Game → Enemy Config`. В скрипте врага объяви поле `[SerializeField] EnemyConfig config;` и используй `config.speed`.

**Сохранение рекорда:**

```csharp
PlayerPrefs.SetInt("best", score);
int best = PlayerPrefs.GetInt("best", 0);
```

📖 Подробнее: [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html) · [PlayerPrefs](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html)

**Дальше:** Unity Learn, курс [Junior Programmer](https://learn.unity.com/pathway/junior-programmer) (англ.).
