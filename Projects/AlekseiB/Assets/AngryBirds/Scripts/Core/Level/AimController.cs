using UnityEngine;
using AngryBirds.Core.Gameplay;

namespace AngryBirds.Core.Level
{
    /// <summary>
    /// Translates player input into slingshot calls: grab, pull, release and cancel.
    /// Owns the grab radius and the screen to world conversion, the slingshot itself
    /// does not read input. Stops reacting once the level has finished.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AimController : MonoBehaviour
    {
        [SerializeField] private Slingshot slingshot;
        [SerializeField] private Camera worldCamera;
        [SerializeField, Min(0.1f)] private float pointerGrabRadius = 1.75f;

        private LevelRuntime _runtime;
        private IAimInput _input;

        private bool IsLevelFinished => _runtime == null || _runtime.State == LevelState.Won || _runtime.State == LevelState.Lost;

        private void OnDestroy()
        {
            Detach();
        }

        public void Initialize(LevelRuntime runtime, IAimInput input)
        {
            Detach();

            _runtime = runtime;
            _input = input;

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            _runtime.Finished += OnLevelFinished;

            // The input outlives the scene, so the handlers must be removed in Detach.
            _input.GrabStarted += OnGrabStarted;
            _input.GrabReleased += OnGrabReleased;
            _input.CancelPressed += OnCancelPressed;
        }

        private void Detach()
        {
            if (_runtime != null)
            {
                _runtime.Finished -= OnLevelFinished;
                _runtime = null;
            }

            if (_input != null)
            {
                _input.GrabStarted -= OnGrabStarted;
                _input.GrabReleased -= OnGrabReleased;
                _input.CancelPressed -= OnCancelPressed;
                _input = null;
            }
        }

        private void Update()
        {
            if (_input != null && slingshot.IsAiming)
            {
                slingshot.UpdateAim(ResolveAimPoint());
            }
        }

        private Vector2 ResolveAimPoint()
        {
            if (_input.IsGamepadAim)
            {
                return slingshot.AnchorPosition + _input.StickDirection * slingshot.MaxPullDistance;
            }

            return PointerToWorld(_input.PointerPosition);
        }

        /// <summary>Converts a screen-space pointer position into a world position on the play plane.</summary>
        private Vector2 PointerToWorld(Vector2 screenPosition)
        {
            if (worldCamera == null)
            {
                return screenPosition;
            }

            Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -worldCamera.transform.position.z));
            return world;
        }

        private void OnGrabStarted()
        {
            if (IsLevelFinished || !slingshot.HasBird)
            {
                return;
            }

            if (!_input.IsGamepadAim)
            {
                if (!_input.HasPointer)
                {
                    return;
                }

                // Pointer aiming only starts when the press is close to the loaded bird,
                // so UI buttons and stray clicks never grab the slingshot.
                Vector2 pointerWorld = PointerToWorld(_input.PointerPosition);

                if (Vector2.Distance(pointerWorld, slingshot.AnchorPosition) > pointerGrabRadius)
                {
                    return;
                }
            }

            slingshot.BeginAim(ResolveAimPoint());
        }

        private void OnGrabReleased()
        {
            slingshot.ReleaseAim();
        }

        private void OnCancelPressed()
        {
            if (!slingshot.IsAiming)
            {
                return;
            }

            slingshot.CancelAim();
        }

        private void OnLevelFinished(LevelOutcome outcome)
        {
            slingshot.CancelAim();
        }
    }
}
