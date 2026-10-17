using UnityEngine;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// Dotted prediction of the shot while the player is aiming.
    /// Dots are pooled at start up, aiming never allocates.
    /// The prediction stops at the first obstacle, it does not simulate bouncing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrajectoryPreview : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer dotPrefab;
        [SerializeField, Range(4, 64)] private int dotCount = 22;
        [SerializeField, Range(0.01f, 0.2f)] private float timeStep = 0.055f;
        [SerializeField] private LayerMask obstacleMask = ~0;

        private SpriteRenderer[] _dots;
        private int _visibleDots;

        private void Awake()
        {
            Build();
            Hide();
        }

        private void Build()
        {
            _dots = new SpriteRenderer[dotCount];

            for (int i = 0; i < dotCount; i++)
            {
                SpriteRenderer dot = Instantiate(dotPrefab, transform);
                dot.name = "Dot" + i;
                _dots[i] = dot;
            }
        }

        public void Show(Vector2 origin, Vector2 velocity)
        {
            if (_dots == null)
            {
                return;
            }

            Vector2 gravity = Physics2D.gravity;
            Vector2 position = origin;
            Vector2 currentVelocity = velocity;
            int index = 0;

            for (int i = 0; i < _dots.Length; i++)
            {
                Vector2 next = position + currentVelocity * timeStep + 0.5f * gravity * timeStep * timeStep;
                currentVelocity += gravity * timeStep;

                bool blocked = Physics2D.Linecast(position, next, obstacleMask).collider != null;
                position = next;

                if (blocked)
                {
                    break;
                }

                SpriteRenderer dot = _dots[i];
                if (dot == null)
                {
                    continue;
                }

                dot.transform.position = position;
                dot.enabled = true;

                Color color = dot.color;
                color.a = Mathf.Lerp(0.85f, 0.15f, i / (float)_dots.Length);
                dot.color = color;

                index = i + 1;
            }

            for (int i = index; i < _dots.Length; i++)
            {
                if (_dots[i] != null)
                {
                    _dots[i].enabled = false;
                }
            }

            _visibleDots = index;
        }

        public void Hide()
        {
            if (_dots == null)
            {
                return;
            }

            for (int i = 0; i < _dots.Length; i++)
            {
                if (_dots[i] != null)
                {
                    _dots[i].enabled = false;
                }
            }

            _visibleDots = 0;
        }

        public int VisibleDots => _visibleDots;
    }
}
