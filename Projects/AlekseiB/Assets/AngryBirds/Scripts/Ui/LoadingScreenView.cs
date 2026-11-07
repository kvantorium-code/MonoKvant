using UnityEngine;
using UnityEngine.UI;
using AngryBirds.Core.Loading;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Loader scene overlay: a progress bar and a status label.
    /// Knows nothing about scenes, it only renders the values pushed by SceneManager.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LoadingScreenView : MonoBehaviour, ILoadingScreen
    {
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressLabel;
        [SerializeField] private Text statusLabel;

        private void Awake()
        {
            if (progressFill != null)
            {
                progressFill.type = Image.Type.Filled;
                progressFill.fillMethod = Image.FillMethod.Horizontal;
                progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            }

            SetProgress(0f);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (progressFill != null)
            {
                progressFill.fillAmount = progress;
            }

            if (progressLabel != null)
            {
                progressLabel.text = Mathf.RoundToInt(progress * 100f) + "%";
            }
        }

        public void SetStatus(string status)
        {
            if (statusLabel != null)
            {
                statusLabel.text = status;
            }
        }
    }
}
