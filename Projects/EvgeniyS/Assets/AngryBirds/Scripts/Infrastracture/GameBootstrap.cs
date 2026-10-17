using UnityEngine;
using AngryBirds.Core;
using AngryBirds.Ui;

namespace AngryBirds.Infrastracture
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private BirdDefinition[] _characters;
        [SerializeField] private int _birdsPerRound = 3;
        [SerializeField] private Vector2 _slingshotAnchor = new Vector2(-6f, -1.5f);

        private GameManager _gameManager;
        private LevelFactory _level;
        private Transform _levelRoot;

        private void Start()
        {
            Camera mainCamera = SetupCamera();

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            _levelRoot = new GameObject("LevelRoot").transform;
            _levelRoot.SetParent(transform, false);

            _level = gameObject.AddComponent<LevelFactory>();
            _level.Initialize(_levelRoot);

            BuildStaticWorld();

            var slingshotObject = new GameObject("Slingshot");
            slingshotObject.transform.SetParent(transform, false);
            var slingshot = slingshotObject.AddComponent<Slingshot>();
            slingshot.Initialize(mainCamera, _slingshotAnchor);

            _gameManager = gameObject.AddComponent<GameManager>();
            _gameManager.Initialize(_characters, slingshot, _level, CreateBird, _birdsPerRound);

            var ui = gameObject.AddComponent<GameUi>();
            ui.Build();
            ui.SetRestartHandler(RestartGame);

            _gameManager.StateChanged += OnStateChanged;
            _gameManager.BirdsRemainingChanged += ui.UpdateBirds;
            _gameManager.PigsAliveChanged += ui.UpdatePigs;
            _ui = ui;
        }

        private GameUi _ui;

        private void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.CharacterSelection:
                    _ui.ShowCharacterSelection(_gameManager.Characters, SelectCharacter);
                    break;
                case GameState.Ready:
                case GameState.Flying:
                    _ui.ShowHud(_gameManager.PigsAlive, _gameManager.BirdsRemaining);
                    break;
                case GameState.Won:
                    _ui.ShowResult(true);
                    break;
                case GameState.Lost:
                    _ui.ShowResult(false);
                    break;
            }
        }

        private void SelectCharacter(BirdDefinition definition)
        {
            _gameManager.SelectCharacter(definition);
        }

        private void RestartGame()
        {
            _gameManager.Restart();
        }

        private Bird CreateBird()
        {
            var go = new GameObject("Bird", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Bird));
            go.transform.SetParent(_levelRoot, false);

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Circle;
            renderer.sortingOrder = 5;

            return go.GetComponent<Bird>();
        }

        private Camera SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraGo.tag = "MainCamera";
                mainCamera = cameraGo.GetComponent<Camera>();
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 6.5f;
            mainCamera.transform.position = new Vector3(0f, 0.5f, -10f);
            mainCamera.transform.rotation = Quaternion.identity;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.49f, 0.78f, 0.95f);
            return mainCamera;
        }

        private void BuildStaticWorld()
        {
            var ground = new GameObject("Ground", typeof(SpriteRenderer), typeof(BoxCollider2D));
            ground.transform.SetParent(_levelRoot, false);
            ground.transform.position = new Vector3(0f, -3.5f, 0f);
            ground.transform.localScale = new Vector3(40f, 1f, 1f);
            var groundRenderer = ground.GetComponent<SpriteRenderer>();
            groundRenderer.sprite = SpriteFactory.Square;
            groundRenderer.color = new Color(0.35f, 0.6f, 0.28f);
            var groundCollider = ground.GetComponent<BoxCollider2D>();
            groundCollider.size = Vector2.one;

            CreateDecor("PostLeft", new Vector2(_slingshotAnchor.x - 0.25f, -2.3f), new Vector2(0.18f, 1.6f));
            CreateDecor("PostRight", new Vector2(_slingshotAnchor.x + 0.25f, -2.3f), new Vector2(0.18f, 1.6f));
        }

        private void CreateDecor(string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(_levelRoot, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Square;
            renderer.color = new Color(0.5f, 0.32f, 0.15f);
            renderer.sortingOrder = 1;
        }
    }
}
