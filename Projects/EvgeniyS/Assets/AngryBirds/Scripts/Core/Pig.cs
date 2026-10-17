using System;
using UnityEngine;

namespace AngryBirds.Core
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class Pig : MonoBehaviour
    {
        public static event Action<Pig> AnyDied;

        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _impactDamageScale = 6f;
        [SerializeField] private float _minImpactSpeed = 4f;
        [SerializeField] private float _despawnX = 20f;
        [SerializeField] private float _despawnY = -9f;

        private float _currentHealth;
        private bool _alive = true;

        public bool IsAlive => _alive;
        public float Health => _currentHealth;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (!_alive || amount <= 0f)
            {
                return;
            }

            _currentHealth -= amount;
            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_alive)
            {
                return;
            }

            float speed = collision.relativeVelocity.magnitude;
            if (speed < _minImpactSpeed)
            {
                return;
            }

            float multiplier = 1f;
            var bird = collision.collider.GetComponentInParent<Bird>();
            if (bird != null && bird.Definition != null)
            {
                multiplier = bird.Definition.DamageMultiplier;
            }

            TakeDamage(speed * _impactDamageScale * multiplier);
        }

        private void Die()
        {
            _alive = false;
            AnyDied?.Invoke(this);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (!_alive)
            {
                return;
            }

            Vector3 position = transform.position;
            if (position.y < _despawnY || position.x > _despawnX || position.x < -_despawnX)
            {
                Die();
            }
        }
    }
}
