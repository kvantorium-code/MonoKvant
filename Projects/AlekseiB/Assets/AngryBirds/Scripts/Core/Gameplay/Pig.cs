using UnityEngine;
using AngryBirds.Core.Damage;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// The target of a shot. Killing every pig of a level wins it.
    /// </summary>
    public sealed class Pig : DamageableActor
    {
        [SerializeField] private SpriteBurst burstPrefab;
        [SerializeField, Min(0.01f)] private float popScale = 1.6f;
        [SerializeField, Min(0.05f)] private float popDuration = 0.18f;

        protected override void OnDepleted(DamageInfo info)
        {
            Pop(info.Point);
            base.OnDepleted(info);
        }

        private void Pop(Vector2 point)
        {
            if (SpriteRenderer != null)
            {
                SpriteRenderer.transform.localScale *= popScale;
            }

            if (Body != null)
            {
                Body.linearVelocity = Vector2.zero;
                Body.angularVelocity = 0f;
            }

            if (burstPrefab != null)
            {
                SpriteBurst burst = Instantiate(burstPrefab, point, Quaternion.identity, transform.parent);
                burst.Initialize(SpriteRenderer != null ? SpriteRenderer.color : Color.green, Vector2.one * 0.6f);
            }
        }
    }
}
