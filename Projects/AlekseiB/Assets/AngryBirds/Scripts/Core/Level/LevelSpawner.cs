using UnityEngine;
using AngryBirds.Core.Gameplay;

namespace AngryBirds.Core.Level
{
    /// <summary>
    /// Builds the world of a level from its <see cref="LevelDefinition"/>: places the slingshot,
    /// the ground, the blocks and the pigs under the level root, and forwards their destruction
    /// to <see cref="LevelRuntime"/>. Everything it creates lives under the root, so a level is
    /// thrown away together with its scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelSpawner : MonoBehaviour
    {
        [SerializeField] private Slingshot slingshot;
        [SerializeField] private Transform spawnRoot;

        [Header("Prefabs")]
        [SerializeField] private Pig pigPrefab;
        [SerializeField] private Block woodBlockPrefab;
        [SerializeField] private Block glassBlockPrefab;
        [SerializeField] private Block stoneBlockPrefab;

        [Header("Ground")]
        [SerializeField] private Sprite groundSprite;
        [SerializeField, Min(0.5f)] private float groundSegmentWidth = 4f;

        private LevelRuntime _runtime;

        public void Spawn(LevelDefinition level, LevelRuntime runtime)
        {
            _runtime = runtime;

            slingshot.transform.position = level.SlingshotPosition;

            BuildGround(level);
            SpawnActors(level);
        }

        private void BuildGround(LevelDefinition level)
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

        private void SpawnActors(LevelDefinition level)
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
                    Debug.LogError($"[LevelSpawner] No block prefab is assigned for block kind {spawn.Kind}.", this);
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

        private void OnPigDepleted(DamageableActor actor)
        {
            if (actor is Pig pig)
            {
                _runtime.NotifyPigDestroyed(pig);
            }
        }

        private void OnBlockDepleted(DamageableActor actor)
        {
            if (actor is Block block)
            {
                _runtime.NotifyBlockDestroyed(block);
            }
        }
    }
}
