using System;
using UnityEngine;
using UnityEngine.UI;
using AngryBirds.Core.Level;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Level result overlay: title, score, stars and the two navigation buttons.
    /// Knows nothing about scenes, it only reports what the player pressed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelResultPanel : MonoBehaviour
    {
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Image[] stars = Array.Empty<Image>();
        [SerializeField] private Sprite filledStar;
        [SerializeField] private Sprite emptyStar;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Text primaryLabel;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private Text secondaryLabel;

        public System.Action PrimaryPressed;
        public System.Action SecondaryPressed;

        private void Awake()
        {
            if (primaryButton != null)
            {
                primaryButton.onClick.AddListener(OnPrimaryClicked);
            }

            if (secondaryButton != null)
            {
                secondaryButton.onClick.AddListener(OnSecondaryClicked);
            }

            Hide();
        }

        public void SetButtons(string primary, string secondary)
        {
            if (primaryLabel != null)
            {
                primaryLabel.text = primary;
            }

            if (secondaryLabel != null)
            {
                secondaryLabel.text = secondary;
            }
        }

        public void Show(LevelOutcome outcome, string title)
        {
            gameObject.SetActive(true);

            if (titleLabel != null)
            {
                titleLabel.text = title;
            }

            if (scoreLabel != null)
            {
                scoreLabel.text = "SCORE  " + outcome.Score;
            }

            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] == null)
                {
                    continue;
                }

                bool earned = i < outcome.Stars;
                stars[i].sprite = earned ? filledStar : emptyStar;
                stars[i].color = earned ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnPrimaryClicked()
        {
            PrimaryPressed?.Invoke();
        }

        private void OnSecondaryClicked()
        {
            SecondaryPressed?.Invoke();
        }
    }
}
