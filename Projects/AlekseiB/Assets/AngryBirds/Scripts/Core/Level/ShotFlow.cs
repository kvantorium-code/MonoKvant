using System.Collections;
using UnityEngine;
using AngryBirds.Core.Gameplay;

namespace AngryBirds.Core.Level
{
    /// <summary>
    /// The queue of birds: puts a bird on the slingshot, waits for it to settle, pauses and loads
    /// the next one. When the birds run out it reports that to <see cref="LevelRuntime"/>.
    /// It knows nothing about score or pigs, only about the birds.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShotFlow : MonoBehaviour
    {
        [SerializeField] private Slingshot slingshot;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private Bird birdPrefab;
        [SerializeField, Min(0f)] private float timeBeforeNextBird = 1f;

        private LevelRuntime _runtime;
        private Coroutine _loadNextBirdRoutine;

        private bool IsLevelFinished => _runtime.State == LevelState.Won || _runtime.State == LevelState.Lost;

        private void OnDestroy()
        {
            if (_runtime == null)
            {
                return;
            }

            slingshot.BirdLaunched -= OnBirdLaunched;
            _runtime = null;
        }

        public void Begin(LevelRuntime runtime)
        {
            _runtime = runtime;

            slingshot.BirdLaunched += OnBirdLaunched;
            LoadNextBird();
        }

        private void LoadNextBird()
        {
            if (_runtime == null || IsLevelFinished)
            {
                return;
            }

            if (_runtime.BirdsRemaining <= 0)
            {
                _runtime.NotifyOutOfBirds();
                return;
            }

            Bird bird = Instantiate(birdPrefab, slingshot.AnchorPosition, Quaternion.identity, spawnRoot);
            bird.Settled += OnBirdSettled;

            slingshot.LoadBird(bird);
        }

        private void OnBirdLaunched(Bird bird)
        {
            _runtime.NotifyBirdLaunched(bird);
        }

        private void OnBirdSettled(Bird bird)
        {
            _runtime.NotifyBirdSettled(bird);

            if (_loadNextBirdRoutine == null)
            {
                _loadNextBirdRoutine = StartCoroutine(LoadNextBirdAfterDelay());
            }
        }

        private IEnumerator LoadNextBirdAfterDelay()
        {
            yield return new WaitForSeconds(timeBeforeNextBird);
            _loadNextBirdRoutine = null;

            if (IsLevelFinished)
            {
                yield break;
            }

            if (slingshot.HasBird)
            {
                yield break;
            }

            LoadNextBird();
        }
    }
}
