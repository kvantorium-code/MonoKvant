using System;
using UnityEngine;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Base class of every screen prefab. A screen is a view: serialized fields and animations only.
    /// It is shown and hidden by <see cref="IUiService"/>, game logic lives in an <see cref="IScreenPresenter"/>.
    /// A <see cref="CanvasGroup"/> blocks input while the screen is hidden or locked.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ScreenBase : MonoBehaviour
    {
        [SerializeField] private UiLayer layer = UiLayer.Screens;

        private CanvasGroup _canvasGroup;
        private bool _visible;

        /// <summary>Raised after the screen became visible.</summary>
        public event Action Shown;

        /// <summary>Raised after the screen was hidden.</summary>
        public event Action Hidden;

        /// <summary>Layer of <see cref="UiRoot"/> the screen is created in.</summary>
        public UiLayer Layer => layer;

        public bool IsVisible => _visible;

        private CanvasGroup Group
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }

                return _canvasGroup;
            }
        }

        public void Show()
        {
            if (_visible)
            {
                return;
            }

            _visible = true;
            gameObject.SetActive(true);
            Group.alpha = 1f;
            SetInputBlocked(false);
            OnShown();
            Shown?.Invoke();
        }

        public void Hide()
        {
            if (!_visible)
            {
                return;
            }

            _visible = false;
            SetInputBlocked(true);
            Group.alpha = 0f;
            OnHidden();
            Hidden?.Invoke();
            gameObject.SetActive(false);
        }

        /// <summary>Locks or unlocks pointer and navigation input without hiding the screen.</summary>
        public void SetInputBlocked(bool blocked)
        {
            Group.interactable = !blocked;
            Group.blocksRaycasts = !blocked;
        }

        /// <summary>Called when the screen becomes visible, the place to start show animations.</summary>
        protected virtual void OnShown()
        {
        }

        /// <summary>Called when the screen is hidden, before the object is deactivated.</summary>
        protected virtual void OnHidden()
        {
        }
    }

    /// <summary>
    /// Screen that is filled with a view model. The presenter or the caller of
    /// <see cref="IUiService.Show{TScreen, TViewModel}"/> passes the data, the screen only renders it.
    /// </summary>
    public abstract class ScreenBase<TViewModel> : ScreenBase
    {
        public TViewModel Model { get; private set; }

        public void Bind(TViewModel model)
        {
            Model = model;
            OnBound(model);
        }

        /// <summary>Called every time a new model is assigned, refresh the widgets here.</summary>
        protected abstract void OnBound(TViewModel model);
    }
}
