using System.Collections;
using UnityEngine;
using AngryBirds.Core.Gameplay;

namespace AngryBirds.Core.Level
{
    /// <summary>
    /// Builds the level from its <see cref="LevelDefinition"/> and drives the shot flow:
    /// load a bird, wait for it to settle, load the next one, finish when the pigs are dead
    /// or the birds ran out.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelDirector : MonoBehaviour
    {
        [SerializeField] private LevelDefinition level;
        [SerializeField] private Slingshot slingshot;
        [SerializeField] private Transform spawnRoot;

        [Header("Prefabs")]
        [SerializeField] private Bird birdPrefab;
        [SerializeField] private Pig pigPrefab;
        [SerializeField] private Block woodBlockPrefab;
        [SerializeField] private Block glassBlockPrefab;
        [SerializeField] private Block stoneBlockPrefab;

        [Header("Ground")]
        [SerializeField] private Sprite groundSprite;
        [SerializeField, Min(0.5f)] private float groundSegmentWidth = 4f;

        [Header("Flow")]
        [SerializeField, Min(0f)] private float timeBeforeNextBird = 1f;

        private LevelRuntime _runtime;
        private Coroutine _loadNextBirdRoutine;

        public LevelDefinition Level => level;

        private void OnDestroy()
        {
            DetachRuntime();
        }

        public void Initialize(LevelRuntime runtime)
        {
            _runtime = runtime;
            _runtime.Initialize(level.PigCount, level.BirdCount, level.TwoStarScore, level.ThreeStarScore);

            slingshot.transform.position = level.SlingshotPosition;

            BuildGround();
            SpawnActors();

            _runtime.BlockDestroyed += OnBlockDestroyed;
            _runtime.PigDestroyed += OnPigDestroyed;
            slingshot.BirdLaunched += OnBirdLaunched;
            LoadNextBird();
        }

        private void DetachRuntime()
        {
            if (_runtime == null)
            {
                return;
            }

            _runtime.BlockDestroyed -= OnBlockDestroyed;
            _runtime.PigDestroyed -= OnPigDestroyed;
            slingshot.BirdLaunched -= OnBirdLaunched;
            _runtime = null;
        }

        private void BuildGround()
        {
            float width = level.GroundWidth;
            float height = level.GroundHeight;
            int segments = Mathf.Max(1, Mathf.CeilToInt(width / groundSegmentWidth));

            GameObject ground = new GameObject("Ground");
            ground.transform.SetParent(spawnRoot, false);
            ground.transform.position = new Vector3(0f, -height * 0.5f, 0f);

            BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width, height);

            if (groundSprite == null)
            {
                return;
            }

            float segmentWidth = width / segments;

            for (int i = 0; i < segments; i++)
            {
                GameObject segment = new GameObject("GroundSegment" + i);
                segment.transform.SetParent(ground.transform, false);
                segment.transform.localPosition = new Vector3(-width * 0.5f + segmentWidth * (i + 0.5f), 0f, 0f);

                SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
                renderer.sprite = groundSprite;

                // size only takes effect in Sliced/Tiled mode.
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = new Vector2(segmentWidth, height);
            }
        }

        private void SpawnActors()
        {
            for (int i = 0; i < level.Pigs.Count; i++)
            {
                Pig pig = Instantiate(pigPrefab, level.Pigs[i].Position, Quaternion.identity, spawnRoot);
                pig.Depleted += OnPigDepleted;
            }

            for (int i = 0; i < level.Blocks.Count; i++)
            {
                LevelDefinition.BlockSpawn spawn = level.Blocks[i];
                Block prefab = ResolveBlockPrefab(spawn.Kind);

                if (prefab == null)
                {
                    Debug.LogError($"[LevelDirector] No block prefab is assigned for block kind {spawn.Kind}.", this);
                    continue;
                }

                Block block = Instantiate(prefab, spawn.Position, Quaternion.Euler(0f, 0f, spawn.Rotation), spawnRoot);
                block.Configure(spawn.Kind, spawn.Size);
                block.Depleted += OnBlockDepleted;
            }
        }

        private Block ResolveBlockPrefab(BlockKind kind)
        {
            switch (kind)
            {
                case BlockKind.Glass:
                    return glassBlockPrefab;
                case BlockKind.Stone:
                    return stoneBlockPrefab;
                default:
                    return woodBlockPrefab;
            }
        }

        private void LoadNextBird()
        {
            if (_runtime == null || _runtime.State == LevelState.Won || _runtime.State == LevelState.Lost)
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

            if (_runtime.State == LevelState.Won || _runtime.State == LevelState.Lost)
            {
                yield break;
            }

            if (slingshot.HasBird)
            {
                yield break;
            }

            LoadNextBird();
        }

        private void OnPigDepleted(DamageableActor actor)
        {
            if (actor is Pig pig)
            {
                OnPigDestroyed(pig);
            }
        }

        private void OnBlockDepleted(DamageableActor actor)
        {
            if (actor is Block block)
            {
                OnBlockDestroyed(block);
            }
        }

        private void OnPigDestroyed(Pig pig)
        {
            _runtime.NotifyPigDestroyed(pig);
        }

        private void OnBlockDestroyed(Block block)
        {
            _runtime.NotifyBlockDestroyed(block);
        }
    }
}
