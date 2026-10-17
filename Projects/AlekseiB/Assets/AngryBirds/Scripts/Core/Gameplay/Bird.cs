using UnityEngine;
using AngryBirds.Core.Damage;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// A bird. Sits on the slingshot as a kinematic body, becomes dynamic when launched.
    /// Reports when the shot is over so the level can load the next bird or finish.
    /// </summary>
    public sealed class Bird : DamageableActor
    {
        [SerializeField] private float settleSpeedThreshold = 0.35f;
        [SerializeField] private float settleDelay = 0.55f;
        [SerializeField] private float killBelowY = -14f;
        [SerializeField] private float killBeyondX = 60f;

        private float _settleTimer;
        private bool _isFlying;

        public bool IsFlying => _isFlying;

        public bool IsLoaded { get; private set; }

        /// <summary>Raised when a bird is pulled off the slingshot.</summary>
        public event System.Action<Bird> Launched;

        /// <summary>Raised once the shot is over: the bird stopped, popped or left the level.</summary>
        public event System.Action<Bird> Settled;

        protected override void Awake()
        {
            base.Awake();
            Body.bodyType = RigidbodyType2D.Kinematic;
        }

        /// <summary>Places the bird on the slingshot and freezes it.</summary>
        public void LoadOnto(Vector2 position)
        {
            IsLoaded = true;
            _isFlying = false;
            _settleTimer = 0f;

            if (SpriteRenderer != null)
            {
                SpriteRenderer.enabled = true;
            }

            if (Body == null)
            {
                return;
            }

            Body.bodyType = RigidbodyType2D.Kinematic;
            Body.linearVelocity = Vector2.zero;
            Body.angularVelocity = 0f;
            Body.position = position;
            Body.rotation = 0f;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>Moves the bird while the player is aiming. Physics independent.</summary>
        public void MoveTo(Vector2 position)
        {
            if (Body != null)
            {
                Body.position = position;
            }
            else
            {
                transform.position = position;
            }
        }

        public void Launch(Vector2 velocity)
        {
            if (Body == null || !IsLoaded)
            {
                return;
            }

            IsLoaded = false;
            _isFlying = true;
            _settleTimer = 0f;

            Body.bodyType = RigidbodyType2D.Dynamic;
            Body.linearVelocity = velocity;
            Body.angularVelocity = Random.Range(-90f, 90f);

            Launched?.Invoke(this);
        }

        private void Update()
        {
            if (!_isFlying || Body == null)
            {
                return;
            }

            Vector2 position = Body.position;

            if (position.y < killBelowY || position.x > killBeyondX)
            {
                Settle();
                return;
            }

            if (Body.linearVelocity.sqrMagnitude > settleSpeedThreshold * settleSpeedThreshold)
            {
                _settleTimer = 0f;
                return;
            }

            _settleTimer += Time.deltaTime;

            if (_settleTimer >= settleDelay)
            {
                Settle();
            }
        }

        protected override void OnDepleted(DamageInfo info)
        {
            // A bird that takes a hit too hard simply disappears, the shot still counts as spent.
            Settle();
        }

        private void Settle()
        {
            if (!_isFlying)
            {
                return;
            }

            _isFlying = false;

            if (Body != null)
            {
                Body.linearVelocity = Vector2.zero;
                Body.angularVelocity = 0f;
                Body.bodyType = RigidbodyType2D.Kinematic;
            }

            if (SpriteRenderer != null)
            {
                SpriteRenderer.enabled = false;
            }

            Settled?.Invoke(this);
        }
    }
}
