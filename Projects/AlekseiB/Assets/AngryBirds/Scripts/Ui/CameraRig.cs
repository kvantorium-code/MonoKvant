using UnityEngine;
using AngryBirds.Core.Gameplay;
using AngryBirds.Core.Level;

namespace AngryBirds.Ui
{
    /// <summary>
    /// Follows the launched bird and returns to the slingshot when the shot is over.
    /// Purely a view: it only listens to <see cref="LevelRuntime"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float followSharpness = 3.5f;
        [SerializeField, Min(0f)] private float lookAheadTime = 0.25f;
        [SerializeField, Min(0f)] private float maxLookAhead = 2.5f;
        [SerializeField, Min(0f)] private float flightZoomOut = 0.8f;
        [SerializeField, Min(0.1f)] private float zoomSharpness = 2.5f;

        private Camera _camera;
        private LevelRuntime _runtime;
        private LevelDefinition _level;
        private Transform _target;
        private Rigidbody2D _targetBody;
        private Vector2 _focus;
        private float _targetSize;
        private float _baseSize;
        private bool _isInitialized;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _baseSize = _camera.orthographicSize;
            _targetSize = _baseSize;
        }

        private void OnDestroy()
        {
            if (_runtime == null)
            {
                return;
            }

            _runtime.BirdLaunched -= OnBirdLaunched;
            _runtime.BirdSettled -= OnBirdSettled;
        }

        public void Initialize(LevelRuntime runtime, LevelDefinition level)
        {
            _runtime = runtime;
            _level = level;
            _focus = DefaultFocus;
            _target = null;

            _runtime.BirdLaunched += OnBirdLaunched;
            _runtime.BirdSettled += OnBirdSettled;
            _isInitialized = true;
        }

        private Vector2 DefaultFocus => _level != null
            ? _level.SlingshotPosition + new Vector2(1.6f, 2.4f)
            : transform.position;

        private void LateUpdate()
        {
            if (!_isInitialized)
            {
                return;
            }

            UpdateFocus();
            UpdateZoom();

            _camera.orthographicSize = _targetSize;
            transform.position = ClampToBounds(new Vector3(_focus.x, _focus.y, transform.position.z));
        }

        private void UpdateFocus()
        {
            Vector2 desired = DefaultFocus;

            if (_target != null)
            {
                Vector2 velocity = _targetBody != null ? _targetBody.linearVelocity : Vector2.zero;
                Vector2 lookAhead = Vector2.ClampMagnitude(velocity * lookAheadTime, maxLookAhead);
                desired = (Vector2)_target.position + lookAhead;
            }

            _focus = Vector2.Lerp(_focus, desired, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
        }

        private void UpdateZoom()
        {
            float desired = _baseSize + (_target != null ? flightZoomOut : 0f);
            _targetSize = Mathf.Lerp(_targetSize, desired, 1f - Mathf.Exp(-zoomSharpness * Time.deltaTime));
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            if (_level == null || !_camera.orthographic)
            {
                return position;
            }

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            Vector2 min = _level.CameraBoundsMin;
            Vector2 max = _level.CameraBoundsMax;

            float minX = min.x + halfWidth;
            float maxX = max.x - halfWidth;
            float minY = min.y + halfHeight;
            float maxY = max.y - halfHeight;

            float x = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : (min.x + max.x) * 0.5f;
            float y = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : (min.y + max.y) * 0.5f;

            return new Vector3(x, y, position.z);
        }

        private void OnBirdLaunched(Bird bird)
        {
            _target = bird != null ? bird.transform : null;
            _targetBody = _target != null ? _target.GetComponent<Rigidbody2D>() : null;
        }

        private void OnBirdSettled(Bird bird)
        {
            _target = null;
            _targetBody = null;
        }
    }
}
