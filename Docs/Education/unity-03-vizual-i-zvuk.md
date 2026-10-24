# Unity 3. Визуал, анимация, звук, сборка

Как сделать так, чтобы игра выглядела и звучала как игра, и как отдать её другим.

## 1. Материалы и свет

- **Материал** определяет вид поверхности. Создаётся так: ПКМ в Project → `Create → Material`, шейдер `Universal Render Pipeline/Lit`. Чтобы назначить материал, перетащи его на объект.
  - `Base Map` — цвет или текстура;
  - `Metallic` / `Smoothness` — металл и блеск;
  - `Emission` — свечение (заметно при включённом Bloom).
- **Свет:**
  - `Directional Light` — солнце: важно только направление;
  - `Point Light` — лампочка;
  - `Spot Light` — фонарик.
- **Post Processing** (Bloom, цветокоррекция, виньетка) настраивается в объекте `Global Volume` в сцене. У камеры должна стоять галочка `Post Processing`.

📖 Подробнее: [Материалы](https://docs.unity3d.com/Manual/materials-introduction.html) · [Шейдер URP Lit](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/lit-shader.html) · [Типы источников света](https://docs.unity3d.com/Manual/Lighting.html) · [Постобработка в URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/integration-with-post-processing.html)

## 2. Камера

- `Projection: Perspective` — 3D-вид, `Orthographic` — плоский вид для 2D и изометрии.
- Простое следование за игроком (код камеры пиши в `LateUpdate`):

```csharp
[SerializeField] Transform target;
[SerializeField] Vector3 offset = new Vector3(0, 8, -6);

void LateUpdate()
{
    transform.position = Vector3.Lerp(transform.position, target.position + offset, 5f * Time.deltaTime);
    transform.LookAt(target);
}
```

- Для нормальной камеры без кода используй пакет **Cinemachine** (`Window → Package Manager`). Создай `Cinemachine Camera` и укажи цель в поле `Tracking Target`.

📖 Подробнее: [Камеры](https://docs.unity3d.com/Manual/CamerasOverview.html) · [Cinemachine: начало работы](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/get-started.html)

## 3. Анимация

**Animation** — запись того, как меняются свойства во времени. **Animator** — схема состояний (Idle → Run → Jump) и переходов между ними.

1. Выдели объект, открой `Window → Animation → Animation`, нажми `Create` и запиши ключевые кадры (кнопка записи, затем меняешь Transform или спрайт).
2. В окне `Animator` перетащи клипы, соедини их переходами (ПКМ → `Make Transition`).
3. В разделе `Parameters` создай параметры, на переходах задай условия (`Conditions`). Для мгновенных переходов сними галочку `Has Exit Time`.
4. Управляй анимацией из кода:

```csharp
Animator anim;
void Awake() => anim = GetComponent<Animator>();

anim.SetFloat("Speed", velocity.magnitude);   // Idle ↔ Run при Speed > 0.1
anim.SetBool("Grounded", isGrounded);
anim.SetTrigger("Jump");                      // одноразовое событие
```

Готовых 3D-персонажей с анимациями можно бесплатно взять на [mixamo.com](https://www.mixamo.com). При импорте выставь `Rig → Animation Type: Humanoid`.

📖 Подробнее: [Система анимации](https://docs.unity3d.com/Manual/AnimationOverview.html) · [Animator Controller](https://docs.unity3d.com/Manual/class-AnimatorController.html) · [Параметры анимации](https://docs.unity3d.com/Manual/AnimationParameters.html)

## 4. Звук

- На объект добавь компонент `AudioSource`, на камере уже есть `AudioListener` («уши»).
- **Музыка:** назначь клип в `AudioSource`, поставь галочки `Play On Awake` и `Loop`.
- **Эффекты из кода:**

```csharp
[SerializeField] AudioSource source;
[SerializeField] AudioClip jumpSound;

source.PlayOneShot(jumpSound);   // звуки не обрывают друг друга
```

- `Spatial Blend = 1` делает звук 3D, громкость зависит от расстояния. Значение 0 — звук слышен везде одинаково (подходит для UI и музыки).
- Подсказка: если объект уничтожается через `Destroy`, его звук обрывается. Для звука взрыва используй `AudioSource.PlayClipAtPoint(clip, transform.position);`.

📖 Подробнее: [Обзор звука](https://docs.unity3d.com/Manual/AudioOverview.html) · [Audio Source](https://docs.unity3d.com/Manual/class-AudioSource.html) · [PlayOneShot](https://docs.unity3d.com/ScriptReference/AudioSource.PlayOneShot.html) · [PlayClipAtPoint](https://docs.unity3d.com/ScriptReference/AudioSource.PlayClipAtPoint.html)

## 5. Частицы

`GameObject → Effects → Particle System`: искры, дым, взрывы, сбор монеты.
- Главные параметры: `Duration`, `Looping`, `Start Lifetime`, `Start Speed`, `Start Size`, `Start Color`, модуль `Emission → Bursts` (выброс сразу N частиц).
- Эффект взрыва: сделай префаб с `Looping` выключенным и `Stop Action: Destroy`, затем создай его в нужный момент:

```csharp
Instantiate(explosionPrefab, transform.position, Quaternion.identity);
```

📖 Подробнее: [Particle System](https://docs.unity3d.com/Manual/Built-inParticleSystem.html) · [Главный модуль](https://docs.unity3d.com/Manual/PartSysMainModule.html)

## 6. Сборка игры

1. `File → Build Profiles` → добавь все сцены в `Scene List`. Сцена с индексом 0 запускается первой (обычно это меню).
2. Выбери `Windows`, в `Player Settings` укажи название игры, компанию и иконку.
3. Нажми `Build` и выбери **пустую** папку. Вся эта папка целиком и есть игра: `.exe` без неё не запустится.
4. Выход из игры по кнопке делается через `Application.Quit();` (в редакторе эта команда не работает, это нормально).

Показать игру другим можно, загрузив zip-архив папки на [itch.io](https://itch.io), либо собрать под `Web` и играть прямо в браузере.

📖 Подробнее: [Build Profiles](https://docs.unity3d.com/Manual/build-profiles.html) · [Сборка из редактора](https://docs.unity3d.com/Manual/BuildSettings.html) · [Первая страница на itch.io (англ.)](https://itch.io/docs/creators/getting-started) · [Загрузка веб-игры на itch.io (англ.)](https://itch.io/docs/creators/html5)

**Дальше:** Unity Learn, курс [Creative Core](https://learn.unity.com/pathway/creative-core) (англ.): свет, материалы, анимация, звук, эффекты.
