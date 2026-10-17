using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AngryBirds.Core
{
    public sealed class Slingshot : MonoBehaviour
    {
        [SerializeField] private float _maxPull = 2.5f;
        [SerializeField] private float _grabRadius = 1.8f;
        [SerializeField] private float _minLaunchPull = 0.25f;

        private Camera _camera;
        private Bird _bird;
        private Action<Bird, Vector2> _onLaunch;
        private Vector3 _anchor;
        private bool _dragging;

        public Bird AttachedBird => _bird;
        public bool IsDragging => _dragging;
        public Vector3 Anchor => _anchor;

        public void Initialize(Camera renderCamera, Vector3 anchorPosition)
        {
            _camera = renderCamera;
            _anchor = anchorPosition;
            transform.position = anchorPosition;
        }

        public void Attach(Bird newBird, Action<Bird, Vector2> launchCallback)
        {
            _bird = newBird;
            _onLaunch = launchCallback;
            _dragging = false;
            newBird.MoveTo(_anchor);
        }

        public void Clear()
        {
            _bird = null;
            _onLaunch = null;
            _dragging = false;
        }

        private void Update()
        {
            if (_bird == null || _onLaunch == null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            var pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            if (!_dragging && pointer.press.wasPressedThisFrame)
            {
                Vector2 pressedWorld = ScreenToWorld(pointer.position.ReadValue());
                if (Vector2.Distance(pressedWorld, _anchor) <= _grabRadius)
                {
                    _dragging = true;
                }
            }

            if (!_dragging)
            {
                return;
            }

            Vector2 pointerWorld = ScreenToWorld(pointer.position.ReadValue());
            Vector2 pull = ClampPull((Vector2)_anchor - pointerWorld);

            if (pointer.press.isPressed)
            {
                _bird.MoveTo(_anchor + (Vector3)pull);
            }
            else if (pointer.press.wasReleasedThisFrame)
            {
                _dragging = false;
                if (pull.magnitude < _minLaunchPull)
                {
                    _bird.MoveTo(_anchor);
                    return;
                }

                Bird launchingBird = _bird;
                Action<Bird, Vector2> callback = _onLaunch;
                _bird = null;
                _onLaunch = null;
                callback?.Invoke(launchingBird, pull * launchingBird.Definition.LaunchPower);
            }
#endif
        }

        private Vector2 ClampPull(Vector2 rawPull)
        {
            if (rawPull.magnitude > _maxPull)
            {
                rawPull = rawPull.normalized * _maxPull;
            }

            return rawPull;
        }

        private Vector2 ScreenToWorld(Vector2 screenPoint)
        {
            if (_camera == null)
            {
                return screenPoint;
            }

            float depth = Mathf.Abs(_camera.transform.position.z);
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));
            return new Vector2(world.x, world.y);
        }
    }
}
