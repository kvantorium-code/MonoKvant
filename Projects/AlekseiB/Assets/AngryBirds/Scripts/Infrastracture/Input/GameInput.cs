using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AngryBirds.Infrastructure.Input
{
    /// <summary>
    /// Adapter around the Input System gameplay actions.
    /// The only place in the project that knows about action names and devices,
    /// everything above it talks in plain values and events.
    /// The action map comes from the generated wrapper of
    /// Assets/AngryBirds/Settings/Input/AngryBirdsGameplay.inputactions, so action names
    /// are checked by the compiler instead of by strings.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameInput : MonoBehaviour
    {
        [SerializeField, Range(0.05f, 0.9f)] private float gamepadAimThreshold = 0.25f;


        private AngryBirdsActionScheme _actionScheme;

        private AngryBirdsGameplayActions _actions;
        private InputActionMap _map;
        private InputAction _aimPoint;
        private InputAction _aimStick;
        private InputAction _grab;
        private InputAction _cancel;
        private InputAction _restart;
        private InputAction _nextLevel;

        /// <summary>Screen position of the pointer.</summary>
        public Vector2 PointerPosition => _aimPoint != null ? _aimPoint.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>Direction the gamepad stick pulls the bird, zero while the stick rests.</summary>
        public Vector2 StickDirection => _aimStick != null ? _aimStick.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>True while the gamepad stick drives the aim instead of the pointer.</summary>
        public bool IsGamepadAim => StickDirection.sqrMagnitude > gamepadAimThreshold * gamepadAimThreshold;

        /// <summary>False on platforms without a pointer, aiming is then gamepad only.</summary>
        public bool HasPointer => _aimPoint != null && _aimPoint.activeControl != null;

        public event Action GrabStarted;

        public event Action GrabReleased;

        public event Action CancelPressed;

        public event Action RestartPressed;

        public event Action NextLevelPressed;

        private void Awake()
        {
            _actionScheme = new AngryBirdsActionScheme();

            _actions = new AngryBirdsGameplayActions();
            _map = _actions.Gameplay.Get();

            AngryBirdsGameplayActions.GameplayActions gameplay = _actions.Gameplay;
            _aimPoint = gameplay.AimPoint;
            _aimStick = gameplay.AimStick;
            _grab = gameplay.Grab;
            _cancel = gameplay.Cancel;
            _restart = gameplay.Restart;
            _nextLevel = gameplay.NextLevel;

            _grab.started += OnGrabStarted;
            _grab.canceled += OnGrabReleased;
            _cancel.performed += OnCancelPressed;
            _restart.performed += OnRestartPressed;
            _nextLevel.performed += OnNextLevelPressed;
        }

        private void OnEnable()
        {
            _map?.Enable();
        }

        private void OnDisable()
        {
            _map?.Disable();
        }

        private void OnGrabStarted(InputAction.CallbackContext context)
        {
            GrabStarted?.Invoke();
        }

        private void OnGrabReleased(InputAction.CallbackContext context)
        {
            GrabReleased?.Invoke();
        }

        private void OnCancelPressed(InputAction.CallbackContext context)
        {
            CancelPressed?.Invoke();
        }

        private void OnRestartPressed(InputAction.CallbackContext context)
        {
            RestartPressed?.Invoke();
        }

        private void OnNextLevelPressed(InputAction.CallbackContext context)
        {
            NextLevelPressed?.Invoke();
        }
    }
}
