using System;
using UnityEngine;
using UnityEngine.UI;
using AngryBirds.Core.Level;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Level information shown while playing: level name, score, birds left and the live
    /// star rating. Shows the result overlay when the level finishes.
    /// Pure display: it renders the values pushed by <see cref="HudController"/> and reports
    /// what the player pressed, it knows nothing about the level runtime or services.
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

        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (resultPanel != null)
            {
                resultPanel.PrimaryPressed += OnNextLevelClicked;
                resultPanel.SecondaryPressed += OnRestartClicked;
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
        }

        public void SetLevelName(string displayName)
        {
            if (levelLabel != null)
            {
                levelLabel.text = displayName;
            }
        }

        public void SetScore(int score)
        {
            if (scoreLabel != null)
            {
                scoreLabel.text = "SCORE  " + score;
            }
        }

        public void SetBirdsRemaining(int remaining)
        {
            if (birdsLabel != null)
            {
                birdsLabel.text = "BIRDS  " + remaining;
            }
        }

        public void SetLiveStars(int stars)
        {
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

        public void ShowResult(LevelOutcome outcome, string title, bool hasNextLevel)
        {
            if (resultPanel == null)
            {
                return;
            }

            resultPanel.SetButtons(hasNextLevel ? "NEXT LEVEL" : "PLAY AGAIN", "RESTART");
            resultPanel.Show(outcome, title);
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
