using System;
using UnityEngine;
using UnityEngine.UI;
using AngryBirds.Core.Level;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Level information shown while playing: level name, score, birds left and the live
    /// star rating. Shows the result overlay when the level finishes.
    /// Renders <see cref="LevelRuntime"/> only, it never changes the state of the level.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private Text levelLabel;
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Text birdsLabel;
        [SerializeField] private Image[] liveStars = Array.Empty<Image>();
        [SerializeField] private Sprite filledStar;
        [SerializeField] private Sprite emptyStar;
        [SerializeField] private Button restartButton;
        [SerializeField] private LevelResultPanel resultPanel;

        public event Action RestartRequested;

        public event Action NextLevelRequested;

        public event Action<LevelOutcome> LevelFinished;

        private LevelRuntime _runtime;
        private int _twoStarScore;
        private int _threeStarScore;

        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }

            if (resultPanel != null)
            {
                resultPanel.PrimaryPressed -= OnNextLevelClicked;
                resultPanel.SecondaryPressed -= OnRestartClicked;
            }

            if (_runtime != null)
            {
                _runtime.ScoreChanged -= OnScoreChanged;
                _runtime.BirdsChanged -= OnBirdsChanged;
                _runtime.Finished -= OnRuntimeFinished;
            }
        }

        public void Initialize(LevelRuntime runtime, LevelDefinition level)
        {
            _runtime = runtime;
            _twoStarScore = level.TwoStarScore;
            _threeStarScore = level.ThreeStarScore;

            if (levelLabel != null)
            {
                levelLabel.text = level.DisplayName;
            }

            if (resultPanel != null)
            {
                resultPanel.PrimaryPressed -= OnNextLevelClicked;
                resultPanel.SecondaryPressed -= OnRestartClicked;
                resultPanel.PrimaryPressed += OnNextLevelClicked;
                resultPanel.SecondaryPressed += OnRestartClicked;
            }

            _runtime.ScoreChanged += OnScoreChanged;
            _runtime.BirdsChanged += OnBirdsChanged;
            _runtime.Finished += OnRuntimeFinished;

            OnScoreChanged(runtime.Score);
            OnBirdsChanged(runtime.BirdsRemaining);
        }

        public void ShowResult(LevelOutcome outcome, string title, bool hasNextLevel)
        {
            if (resultPanel == null)
            {
                return;
            }

            resultPanel.SetButtons(hasNextLevel ? "NEXT LEVEL" : "PLAY AGAIN", "RESTART");
            resultPanel.Show(outcome, title);
        }

        private void OnScoreChanged(int score)
        {
            if (scoreLabel != null)
            {
                scoreLabel.text = "SCORE  " + score;
            }

            int stars = score >= _threeStarScore ? 3 : score >= _twoStarScore ? 2 : score > 0 ? 1 : 0;

            for (int i = 0; i < liveStars.Length; i++)
            {
                if (liveStars[i] == null)
                {
                    continue;
                }

                bool earned = i < stars;
                liveStars[i].sprite = earned ? filledStar : emptyStar;
                liveStars[i].color = earned ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            }
        }

        private void OnBirdsChanged(int remaining)
        {
            if (birdsLabel != null)
            {
                birdsLabel.text = "BIRDS  " + remaining;
            }
        }

        private void OnRuntimeFinished(LevelOutcome outcome)
        {
            LevelFinished?.Invoke(outcome);
        }

        private void OnRestartClicked()
        {
            RestartRequested?.Invoke();
        }

        private void OnNextLevelClicked()
        {
            NextLevelRequested?.Invoke();
        }
    }
}
