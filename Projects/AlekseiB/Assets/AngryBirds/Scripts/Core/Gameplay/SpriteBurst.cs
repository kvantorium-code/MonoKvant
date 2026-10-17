using System.Collections;
using UnityEngine;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// Short lived visual effect: a handful of sprites flying out of a point and fading away.
    /// Used for hits and for destroyed actors. Never collides with anything.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpriteBurst : MonoBehaviour
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Vector2 pieceSize = new Vector2(0.18f, 0.18f);
        [SerializeField, Range(1, 24)] private int pieceCount = 8;
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private float lifeTime = 0.45f;
        [SerializeField] private float gravityScale = 0.6f;

        private SpriteRenderer[] _pieces;
        private Vector2[] _velocities;
        private float[] _rotations;
        private bool _isPlaying;

        private void Awake()
        {
            Build();
            Play();
        }

        /// <summary>
        /// Re-tints an already spawned burst, so callers can pass level specific colors
        /// even though the effect is a prefab.
        /// </summary>
        public void Initialize(Color tint, Vector2 burstSize)
        {
            color = tint;
            pieceSize = burstSize * 0.22f;

            if (_pieces == null)
            {
                return;
            }

            for (int i = 0; i < _pieces.Length; i++)
            {
                if (_pieces[i] == null)
                {
                    continue;
                }

                _pieces[i].color = color;

                if (_isPlaying)
                {
                    _pieces[i].size = pieceSize * Random.Range(0.7f, 1.3f);
                }
            }
        }

        private void Build()
        {
            int count = Mathf.Clamp(pieceCount, 1, 24);
            _pieces = new SpriteRenderer[count];
            _velocities = new Vector2[count];
            _rotations = new float[count];

            for (int i = 0; i < count; i++)
            {
                GameObject piece = new GameObject("Piece");
                piece.transform.SetParent(transform, false);
                piece.transform.localPosition = Vector3.zero;

                SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = color;
                renderer.sortingOrder = 10;
                renderer.drawMode = SpriteDrawMode.Sliced;

                _pieces[i] = renderer;
            }
        }

        private void Play()
        {
            _isPlaying = true;

            for (int i = 0; i < _pieces.Length; i++)
            {
                float angle = (360f / _pieces.Length) * i + Random.Range(-12f, 12f);
                _velocities[i] = Quaternion.Euler(0f, 0f, angle) * Vector2.up * speed * Random.Range(0.6f, 1.2f);
                _rotations[i] = Random.Range(-540f, 540f);

                SpriteRenderer renderer = _pieces[i];
                renderer.color = color;
                renderer.size = pieceSize * Random.Range(0.7f, 1.3f);
                renderer.transform.localPosition = Vector3.zero;
                renderer.transform.localRotation = Quaternion.identity;
            }

            StartCoroutine(AnimateRoutine());
        }

        private IEnumerator AnimateRoutine()
        {
            Vector2 gravity = Physics2D.gravity * gravityScale;
            float elapsed = 0f;

            while (elapsed < lifeTime)
            {
                float delta = Time.deltaTime;
                elapsed += delta;
                float fade = 1f - elapsed / lifeTime;

                for (int i = 0; i < _pieces.Length; i++)
                {
                    SpriteRenderer renderer = _pieces[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    _velocities[i] += gravity * delta;
                    renderer.transform.localPosition += (Vector3)(_velocities[i] * delta);
                    renderer.transform.Rotate(0f, 0f, _rotations[i] * delta);

                    Color pieceColor = renderer.color;
                    pieceColor.a = fade;
                    renderer.color = pieceColor;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
