using System;
using System.Collections;
using UnityEngine;
using AngryBirds.Core.Damage;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// Base class for every actor that has hit points and reacts to collisions.
    /// Damage is derived from the momentum of the contact, see <see cref="ImpactDamage"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class DamageableActor : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 50f;
        [SerializeField] private int scoreValue = 500;
        [SerializeField] private bool flashOnHit = true;
        [SerializeField, Range(0f, 0.5f)] private float hitFlashDuration = 0.1f;

        private Rigidbody2D _body;
        private SpriteRenderer _spriteRenderer;
        private Color _baseColor = Color.white;
        private Coroutine _flashRoutine;
        private bool _isDepleted;

        public float MaxHealth => maxHealth;

        public float CurrentHealth { get; private set; }

        public float HealthNormalized => maxHealth > 0f ? Mathf.Clamp01(CurrentHealth / maxHealth) : 0f;

        public bool IsAlive => !_isDepleted;

        public int ScoreValue => scoreValue;

        public Rigidbody2D Body => _body;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        /// <summary>Raised once, right after the actor ran out of health.</summary>
        public event Action<DamageableActor> Depleted;

        protected virtual void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _baseColor = _spriteRenderer.color;
            }

            CurrentHealth = maxHealth;
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (_isDepleted || info.Amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - info.Amount);
            OnDamaged(info);

            if (CurrentHealth > 0f)
            {
                return;
            }

            _isDepleted = true;
            OnDepleted(info);
            Depleted?.Invoke(this);
        }

        protected virtual void OnDamaged(DamageInfo info)
        {
            if (flashOnHit)
            {
                Flash();
            }
        }

        protected virtual void OnDepleted(DamageInfo info)
        {
            Destroy(gameObject);
        }

        private void Flash()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }

            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(hitFlashDuration);
            _spriteRenderer.color = _baseColor;
            _flashRoutine = null;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isDepleted)
            {
                return;
            }

            float damage = ImpactDamage.Calculate(GetTotalNormalImpulse(collision));
            if (damage <= 0f)
            {
                return;
            }

            Vector2 direction = collision.relativeVelocity.sqrMagnitude > 0.0001f
                ? -collision.relativeVelocity.normalized
                : Vector2.up;

            Vector2 point = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;

            GameObject source = collision.otherRigidbody != null
                ? collision.otherRigidbody.gameObject
                : null;

            ApplyDamage(new DamageInfo(damage, point, direction, source));
        }

        private static float GetTotalNormalImpulse(Collision2D collision)
        {
            float total = 0f;

            for (int i = 0; i < collision.contactCount; i++)
            {
                total += collision.GetContact(i).normalImpulse;
            }

            return total;
        }
    }
}
