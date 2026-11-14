using System;
using AngryBirds.Core.Level;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Logic of the HUD: pushes the level runtime state into <see cref="HudView"/> and turns
    /// the view events into its own events. Owns all subscriptions, the view stays pure
    /// display. Created by the scene scope, disposed with it.
    /// </summary>
    public sealed class HudController : IDisposable
    {
        private readonly HudView _view;
        private readonly ILevelCatalog _levels;

        private LevelRuntime _runtime;
        private LevelDefinition _level;

        public event Action RestartRequested;

        public event Action NextLevelRequested;

        public HudController(HudView view, ILevelCatalog levels)
        {
            _view = view;
            _levels = levels;
        }

        public void Initialize(LevelRuntime runtime, LevelDefinition level)
        {
            _runtime = runtime;
            _level = level;

            _view.SetLevelName(level.DisplayName);
            _view.SetScore(runtime.Score);
            _view.SetBirdsRemaining(runtime.BirdsRemaining);
            _view.SetLiveStars(runtime.GetLiveStars());

            runtime.ScoreChanged += OnScoreChanged;
            runtime.BirdsChanged += OnBirdsChanged;
            runtime.Finished += OnFinished;

            _view.RestartRequested += OnRestartRequested;
            _view.NextLevelRequested += OnNextLevelRequested;
        }

        public void Dispose()
        {
            if (_runtime != null)
            {
                _runtime.ScoreChanged -= OnScoreChanged;
                _runtime.BirdsChanged -= OnBirdsChanged;
                _runtime.Finished -= OnFinished;
            }

            _view.RestartRequested -= OnRestartRequested;
            _view.NextLevelRequested -= OnNextLevelRequested;
        }

        private void OnScoreChanged(int score)
        {
            _view.SetScore(score);
            _view.SetLiveStars(_runtime.GetLiveStars());
        }

        private void OnBirdsChanged(int remaining)
        {
            _view.SetBirdsRemaining(remaining);
        }

        private void OnFinished(LevelOutcome outcome)
        {
            bool hasNext = _levels.TryGetNext(_level.LevelNumber, out _);
            _view.ShowResult(outcome, outcome.IsWin ? "LEVEL CLEARED" : "TRY AGAIN", hasNext);
        }

        private void OnRestartRequested()
        {
            RestartRequested?.Invoke();
        }

        private void OnNextLevelRequested()
        {
            NextLevelRequested?.Invoke();
        }
    }
}
