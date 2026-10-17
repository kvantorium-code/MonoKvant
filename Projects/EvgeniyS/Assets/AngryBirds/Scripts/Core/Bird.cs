using System;
using UnityEngine;

namespace AngryBirds.Core
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class Bird : MonoBehaviour
    {
        [SerializeField] private float _settleSpeed = 0.35f;
        [SerializeField] private float _settleTime = 1.1f;
        [SerializeField] private float _despawnX = 15f;
        [SerializeField] private float _despawnY = -9f;

        private Rigidbody2D _body;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private BirdDefinition _definition;
        private Action<Bird> _onSettled;
        private bool _launched;
        private bool _settled;
        private float _idleTimer;

        public BirdDefinition Definition => _definition;
        public Rigidbody2D Body => _body;
        public bool IsLaunched => _launched;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(BirdDefinition birdDefinition, Action<Bird> settledCallback)
        {
            _definition = birdDefinition;
            _onSettled = settledCallback;
            _launched = false;
            _settled = false;
            _idleTimer = 0f;

            _renderer.color = birdDefinition.Color;
            _renderer.sortingOrder = 5;
            transform.localScale = Vector3.one * (birdDefinition.Radius * 2f);

            _collider.radius = 0.5f;
            _body.mass = birdDefinition.Mass;
            _body.gravityScale = 1f;
            _body.linearDamping = 0.1f;
            _body.angularDamping = 0.05f;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
        }

        public void MoveTo(Vector3 position)
        {
            transform.position = position;
            _body.position = new Vector2(position.x, position.y);
        }

        public void Launch(Vector2 velocity)
        {
            _body.bodyType = RigidbodyType2D.Dynamic;
            _body.linearVelocity = velocity;
            _launched = true;
            _settled = false;
            _idleTimer = 0f;
        }

        private void FixedUpdate()
        {
            if (!_launched || _settled)
            {
                return;
            }

            Vector3 position = transform.position;
            if (position.x > _despawnX || position.y < _despawnY)
            {
                Settle();
                return;
            }

            if (_body.linearVelocity.magnitude <= _settleSpeed)
            {
                _idleTimer += Time.fixedDeltaTime;
                if (_idleTimer >= _settleTime)
                {
                    Settle();
                }
            }
            else
            {
                _idleTimer = 0f;
            }
        }

        private void Settle()
        {
            if (_settled)
            {
                return;
            }

            _settled = true;
            _onSettled?.Invoke(this);
        }
    }
}
