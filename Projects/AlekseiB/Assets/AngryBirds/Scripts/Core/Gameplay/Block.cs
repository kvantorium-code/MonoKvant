using UnityEngine;
using AngryBirds.Core.Damage;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// A destructible piece of the structure.
    /// Size is applied to the sprite renderer and the box collider instead of the transform,
    /// so the rigid body keeps a uniform scale and rotates correctly.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class Block : DamageableActor
    {
        [SerializeField] private BlockKind kind = BlockKind.Wood;
        [SerializeField] private BlockArtVariants art = new BlockArtVariants();
        [SerializeField] private Vector2 size = Vector2.one;
        [SerializeField] private Debris debrisPrefab;
        [SerializeField, Range(1, 8)] private int debrisCount = 4;
        [SerializeField] private SpriteBurst burstPrefab;

        private BoxCollider2D _collider;

        public BlockKind Kind => kind;

        public Vector2 Size => size;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<BoxCollider2D>();
            ApplySize(size);
        }

        public void Configure(BlockKind blockKind, Vector2 blockSize)
        {
            kind = blockKind;
            size = new Vector2(Mathf.Max(0.05f, blockSize.x), Mathf.Max(0.05f, blockSize.y));
            ApplySize(size);
        }

        private void ApplySize(Vector2 newSize)
        {
            if (SpriteRenderer != null && art != null)
            {
                SpriteRenderer.sprite = art.Pick(newSize.x / newSize.y);

                // SpriteRenderer.size is only honored in Sliced/Tiled mode, otherwise the
                // sprite keeps its native size and the collider no longer matches the art.
                SpriteRenderer.drawMode = SpriteDrawMode.Sliced;
                SpriteRenderer.size = newSize;
            }

            if (_collider != null)
            {
                _collider.size = newSize;
            }
        }

        protected override void OnDepleted(DamageInfo info)
        {
            SpawnDebris(info);
            base.OnDepleted(info);
        }

        private void SpawnDebris(DamageInfo info)
        {
            if (debrisPrefab == null)
            {
                return;
            }

            Color tint = SpriteRenderer != null ? SpriteRenderer.color : Color.white;
            int count = Mathf.Clamp(debrisCount, 1, 8);

            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2(
                    Random.Range(-size.x, size.x) * 0.35f,
                    Random.Range(-size.y, size.y) * 0.35f);

                Debris debris = Instantiate(debrisPrefab, (Vector2)transform.position + offset, transform.rotation, transform.parent);
                debris.Initialize(transform.right * Random.Range(-1f, 1f) * 1.5f, transform.up * Random.Range(1f, 3f), tint);
            }

            if (burstPrefab != null)
            {
                SpriteBurst burst = Instantiate(burstPrefab, info.Point, Quaternion.identity, transform.parent);
                burst.Initialize(tint, size * 0.8f);
            }
        }
    }
}
