using UnityEngine;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// A piece of rubble left behind when a block breaks.
    /// Purely decorative, it never collides and removes itself after a while.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Debris : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 2.2f;
        [SerializeField] private float fadeOut = 0.6f;

        private SpriteRenderer _renderer;
        private Rigidbody2D _body;
        private float _life;

        private void Awake()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 horizontalPush, Vector2 verticalPush, Color tint)
        {
            if (_renderer != null)
            {
                _renderer.color = tint;
            }

            if (_body != null)
            {
                _body.linearVelocity = horizontalPush + verticalPush;
                _body.angularVelocity = Random.Range(-360f, 360f);
            }

            if (isActiveAndEnabled)
            {
                StartCoroutine(LifeRoutine());
            }
        }

        private System.Collections.IEnumerator LifeRoutine()
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, lifeTime - fadeOut));

            if (_renderer == null)
            {
                yield break;
            }

            Color start = _renderer.color;
            float elapsed = 0f;

            while (elapsed < fadeOut)
            {
                elapsed += Time.deltaTime;
                Color color = start;
                color.a = start.a * (1f - elapsed / fadeOut);
                _renderer.color = color;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
