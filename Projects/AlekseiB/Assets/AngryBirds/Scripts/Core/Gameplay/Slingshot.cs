using System;
using UnityEngine;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// Pull back and release. Owns the loaded bird and the elastic bands,
    /// it does not read input itself, the composition root forwards input to it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Slingshot : MonoBehaviour
    {
        public enum State
        {
            Empty,
            Idle,
            Aiming
        }

        [SerializeField] private Transform anchor;
        [SerializeField] private Transform backProng;
        [SerializeField] private Transform frontProng;
        [SerializeField] private LineRenderer backBand;
        [SerializeField] private LineRenderer frontBand;
        [SerializeField] private TrajectoryPreview trajectory;
        [SerializeField, Min(0.1f)] private float maxPullDistance = 2.2f;
        [SerializeField, Min(0.1f)] private float launchMultiplier = 8.5f;
        [SerializeField] private float minimumHeight = 0.55f;

        private Bird _bird;
        private Vector2 _grabOffset;
        private State _state = State.Empty;

        public Vector2 AnchorPosition => anchor != null ? anchor.position : transform.position;

        public float MaxPullDistance => maxPullDistance;

        public State CurrentState => _state;

        public bool HasBird => _bird != null && _bird.IsLoaded;

        public bool IsAiming => _state == State.Aiming;

        public event Action<Bird> BirdLaunched;

        private void Awake()
        {
            HideBands();
        }

        public void LoadBird(Bird bird)
        {
            _bird = bird;
            _state = bird != null ? State.Idle : State.Empty;
            _grabOffset = Vector2.zero;

            if (bird != null)
            {
                bird.LoadOnto(AnchorPosition);
            }

            HideBands();

            if (trajectory != null)
            {
                trajectory.Hide();
            }
        }

        public void BeginAim(Vector2 aimPoint)
        {
            if (!HasBird)
            {
                return;
            }

            _state = State.Aiming;
            _grabOffset = (Vector2)_bird.transform.position - aimPoint;
            UpdateAim(aimPoint);
        }

        public void UpdateAim(Vector2 aimPoint)
        {
            if (_state != State.Aiming || !HasBird)
            {
                return;
            }

            Vector2 pulled = ClampPull(aimPoint + _grabOffset);
            _bird.MoveTo(pulled);
            UpdateBands(pulled);

            if (trajectory != null)
            {
                trajectory.Show(AnchorPosition, ComputeLaunchVelocity(pulled));
            }
        }

        public void ReleaseAim()
        {
            if (_state != State.Aiming || !HasBird)
            {
                return;
            }

            Bird launched = _bird;
            Vector2 velocity = ComputeLaunchVelocity((Vector2)launched.transform.position);

            _state = State.Idle;
            _bird = null;

            HideBands();

            if (trajectory != null)
            {
                trajectory.Hide();
            }

            launched.Launch(velocity);
            BirdLaunched?.Invoke(launched);
        }

        public void CancelAim()
        {
            if (_state != State.Aiming || !HasBird)
            {
                return;
            }

            _bird.MoveTo(AnchorPosition);
            _state = State.Idle;
            _grabOffset = Vector2.zero;

            HideBands();

            if (trajectory != null)
            {
                trajectory.Hide();
            }
        }

        private Vector2 ClampPull(Vector2 desiredPosition)
        {
            Vector2 offset = desiredPosition - AnchorPosition;
            float distance = offset.magnitude;
            Vector2 result = desiredPosition;

            if (distance > maxPullDistance && distance > 0.0001f)
            {
                result = AnchorPosition + offset / distance * maxPullDistance;
            }

            // The bird must never be dragged into the ground.
            result.y = Mathf.Max(result.y, minimumHeight);
            return result;
        }

        private Vector2 ComputeLaunchVelocity(Vector2 birdPosition)
        {
            return (AnchorPosition - birdPosition) * launchMultiplier;
        }

        private void UpdateBands(Vector2 birdPosition)
        {
            SetBand(backBand, backProng != null ? backProng.position : AnchorPosition, birdPosition);
            SetBand(frontBand, frontProng != null ? frontProng.position : AnchorPosition, birdPosition);
        }

        private void HideBands()
        {
            SetBand(backBand, AnchorPosition, AnchorPosition);
            SetBand(frontBand, AnchorPosition, AnchorPosition);
        }

        private static void SetBand(LineRenderer band, Vector2 from, Vector2 to)
        {
            if (band == null)
            {
                return;
            }

            band.useWorldSpace = true;
            band.positionCount = 2;
            band.SetPosition(0, from);
            band.SetPosition(1, to);
        }
    }
}
