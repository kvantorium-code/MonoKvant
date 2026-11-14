using System;
using UnityEngine;
using VContainer.Unity;
using AngryBirds.Core.Level;
using AngryBirds.Infrastructure.Flow;
using AngryBirds.Infrastructure.Input;
using AngryBirds.Ui;

namespace AngryBirds.Infrastructure.Entry
{
    /// <summary>
    /// Starts the Gameplay scene: takes the level chosen by <see cref="GameplayState"/>, initializes
    /// the runtime and hands it to the scene parts (spawner, shot flow, aim, camera, HUD controller).
    /// Connects the result and the restart / next commands to the game state machine.
    /// Contains no gameplay rules, it only wires parts together.
    /// </summary>
    public sealed class GameplayEntryPoint : IStartable, IDisposable
    {
        private readonly LevelRuntime _runtime;
        private readonly ILevelCatalog _levels;
        private readonly GameplayState _gameplay;
        private readonly IGameStateMachine _states;
        private readonly IGameInput _input;
        private readonly LevelSpawner _spawner;
        private readonly ShotFlow _shotFlow;
        private readonly AimController _aim;
        private readonly CameraRig _cameraRig;
        private readonly HudController _hud;

        private LevelDefinition _level;
        private bool _isLevelFinished;
        private bool _isLeaving;

        public GameplayEntryPoint(
            LevelRuntime runtime,
            ILevelCatalog levels,
            GameplayState gameplay,
            IGameStateMachine states,
            IGameInput input,
            LevelSpawner spawner,
            ShotFlow shotFlow,
            AimController aim,
            CameraRig cameraRig,
            HudController hud)
        {
            _runtime = runtime;
            _levels = levels;
            _gameplay = gameplay;
            _states = states;
            _input = input;
            _spawner = spawner;
            _shotFlow = shotFlow;
            _aim = aim;
            _cameraRig = cameraRig;
            _hud = hud;
        }

        public void Start()
        {
            // The Gameplay scene opened directly in the editor has no chosen level, play the first one.
            _level = _gameplay.Level != null ? _gameplay.Level : _levels.Get(GameplayState.FirstLevel);

            if (_level == null)
            {
                Debug.LogError("[GameplayEntryPoint] There is no level to play, the level catalog is empty.");
                return;
            }

            _runtime.Initialize(_level);

            _spawner.Spawn(_level, _runtime);
            _cameraRig.Initialize(_runtime, _level);
            _hud.Initialize(_runtime, _level);
            _aim.Initialize(_runtime, _input);

            _runtime.Finished += OnLevelFinished;
            _hud.RestartRequested += RestartLevel;
            _hud.NextLevelRequested += LoadNextLevel;

            // The input outlives the scene, so the handlers must be removed in Dispose.
            _input.RestartPressed += RestartLevel;
            _input.NextLevelPressed += OnNextLevelPressed;

            _shotFlow.Begin(_runtime);
        }

        public void Dispose()
        {
            _runtime.Finished -= OnLevelFinished;
            _input.RestartPressed -= RestartLevel;
            _input.NextLevelPressed -= OnNextLevelPressed;
            _hud.RestartRequested -= RestartLevel;
            _hud.NextLevelRequested -= LoadNextLevel;
        }

        private void OnLevelFinished(LevelOutcome outcome)
        {
            _isLevelFinished = true;

            // The result overlay itself is shown by the HUD controller.
            _states.Enter<ResultState, ResultPayload>(new ResultPayload(_level.LevelNumber, outcome));
        }

        private void OnNextLevelPressed()
        {
            if (_isLevelFinished)
            {
                LoadNextLevel();
            }
        }

        private void RestartLevel()
        {
            Enter(_level.LevelNumber);
        }

        private void LoadNextLevel()
        {
            // After the last level the flow wraps around to the first one.
            Enter(_levels.TryGetNext(_level.LevelNumber, out LevelDefinition next) ? next.LevelNumber : GameplayState.FirstLevel);
        }

        private void Enter(int levelNumber)
        {
            // The scene reload is deferred to the next frame, a second command must not enter the state twice.
            if (_isLeaving)
            {
                return;
            }

            _isLeaving = true;
            _states.Enter<GameplayState, int>(levelNumber);
        }
    }
}
