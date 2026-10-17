using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngryBirds.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public event Action<GameState> StateChanged;
        public event Action<int> BirdsRemainingChanged;
        public event Action<int> PigsAliveChanged;

        private BirdDefinition[] _characters = Array.Empty<BirdDefinition>();
        private Slingshot _slingshot;
        private ILevel _level;
        private Func<Bird> _birdFactory;
        private int _birdsPerRound = 3;

        private GameState _state = GameState.None;
        private BirdDefinition _selected;
        private Bird _currentBird;
        private int _birdsRemaining;
        private int _pigsAlive;
        private bool _initialized;

        public IReadOnlyList<BirdDefinition> Characters => _characters;
        public GameState State => _state;
        public int BirdsRemaining => _birdsRemaining;
        public int PigsAlive => _pigsAlive;
        public BirdDefinition SelectedCharacter => _selected;

        public void Initialize(BirdDefinition[] characterSet, Slingshot slingshotRef, ILevel levelController, Func<Bird> birdSpawner, int roundBirdCount)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _characters = characterSet ?? Array.Empty<BirdDefinition>();
            _slingshot = slingshotRef;
            _level = levelController;
            _birdFactory = birdSpawner;
            _birdsPerRound = Mathf.Max(1, roundBirdCount);
            Pig.AnyDied += OnPigDied;
        }

        private void OnDestroy()
        {
            Pig.AnyDied -= OnPigDied;
        }

        private void Start()
        {
            BeginCharacterSelection();
        }

        public void BeginCharacterSelection()
        {
            DestroyCurrentBird();
            _selected = null;
            SetState(GameState.CharacterSelection);
        }

        public void SelectCharacter(BirdDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            _selected = definition;
            StartRound();
        }

        public void Restart()
        {
            BeginCharacterSelection();
        }

        private void StartRound()
        {
            _level.ResetLevel();
            _birdsRemaining = _birdsPerRound;
            _pigsAlive = _level.CountAlivePigs();
            BirdsRemainingChanged?.Invoke(_birdsRemaining);
            PigsAliveChanged?.Invoke(_pigsAlive);
            SpawnNextBird();
        }

        private void SpawnNextBird()
        {
            if (_birdsRemaining <= 0)
            {
                SetState(_pigsAlive > 0 ? GameState.Lost : GameState.Won);
                return;
            }

            _currentBird = _birdFactory();
            _currentBird.Initialize(_selected, OnBirdSettled);
            _slingshot.Attach(_currentBird, OnBirdLaunched);
            SetState(GameState.Ready);
        }

        private void OnBirdLaunched(Bird bird, Vector2 velocity)
        {
            _birdsRemaining = Mathf.Max(0, _birdsRemaining - 1);
            BirdsRemainingChanged?.Invoke(_birdsRemaining);
            bird.Launch(velocity);
            SetState(GameState.Flying);
        }

        private void OnBirdSettled(Bird bird)
        {
            DestroyCurrentBird();

            if (_state == GameState.Won || _state == GameState.Lost)
            {
                return;
            }

            if (_pigsAlive <= 0)
            {
                SetState(GameState.Won);
                return;
            }

            SpawnNextBird();
        }

        private void OnPigDied(Pig pig)
        {
            if (_state != GameState.Ready && _state != GameState.Flying)
            {
                return;
            }

            _pigsAlive = Mathf.Max(0, _pigsAlive - 1);
            PigsAliveChanged?.Invoke(_pigsAlive);

            if (_pigsAlive <= 0)
            {
                SetState(GameState.Won);
            }
        }

        private void DestroyCurrentBird()
        {
            if (_currentBird != null)
            {
                Destroy(_currentBird.gameObject);
                _currentBird = null;
            }

            if (_slingshot != null)
            {
                _slingshot.Clear();
            }
        }

        private void SetState(GameState state)
        {
            _state = state;
            StateChanged?.Invoke(_state);
        }
    }
}
