using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AngryBirds.Core.Gameplay;
using AngryBirds.Core.Level;
using AngryBirds.EditorTools.Art;
using AngryBirds.Infrastructure.Bootstrap;
using AngryBirds.Infrastructure.Input;
using AngryBirds.Ui;

namespace AngryBirds.EditorTools.Builder
{
    /// <summary>
    /// Builds every generated asset of the game: placeholder art, prefabs, level definitions
    /// and the level scenes, then registers the scenes in the build settings.
    /// Running the menu item again rebuilds the same result, so the whole content pipeline
    /// is reproducible from a clean clone.
    /// </summary>
    internal static class GameContentBuilder
    {
        private const string PrefabFolder = "Assets/AngryBirds/Prefabs";
        private const string LevelFolder = "Assets/AngryBirds/Settings/Levels";
        private const string SceneFolder = "Assets/AngryBirds/Scenes";
        private const string MaterialFolder = "Assets/AngryBirds/Settings/Materials";
        private const string PhysicsFolder = "Assets/AngryBirds/Settings/Physics";
        private const string SpriteFolder = "Assets/AngryBirds/Content/Sprites";
        private const string ProjectInputActions = "Assets/AngryBirds/InputSystem_Actions.inputactions";

        [MenuItem("Tools/AngryBirds/2. Build Game Content", false, 20)]
        public static void BuildAll()
        {
            PlaceholderArtGenerator.GenerateAll();

            EnsureFolder(PrefabFolder);
            EnsureFolder(LevelFolder);
            EnsureFolder(MaterialFolder);
            EnsureFolder(PhysicsFolder);

            ArtSprites art = LoadArt();
            Material bandMaterial = CreateBandMaterial();
            var physics = CreatePhysicsMaterials();
            var prefabs = CreatePrefabs(art, bandMaterial, physics);
            List<LevelDefinition> levels = CreateLevelAssets();

            // Assets created in this frame are not importable yet, references to them would be
            // dropped when the scenes are saved. Flush the database before building scenes.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            prefabs = ReloadPrefabs(prefabs);
            levels = ReloadLevels(levels);

            CreateScenes(prefabs, art, bandMaterial, levels);
            RegisterScenes(levels);

            Debug.Log("[GameContentBuilder] Game content rebuilt.");
        }

        private static Prefabs ReloadPrefabs(Prefabs prefabs)
        {
            return new Prefabs
            {
                Bird = LoadComponent<Bird>(PrefabAssetPath("Bird")),
                Pig = LoadComponent<Pig>(PrefabAssetPath("Pig")),
                Wood = LoadComponent<Block>(PrefabAssetPath("WoodBlock")),
                Glass = LoadComponent<Block>(PrefabAssetPath("GlassBlock")),
                Stone = LoadComponent<Block>(PrefabAssetPath("StoneBlock")),
                Debris = LoadAsset<GameObject>(PrefabAssetPath("Debris")),
                Dot = LoadAsset<GameObject>(PrefabAssetPath("TrajectoryDot")),
                Burst = LoadAsset<GameObject>(PrefabAssetPath("SpriteBurst"))
            };
        }

        private static List<LevelDefinition> ReloadLevels(List<LevelDefinition> levels)
        {
            var reloaded = new List<LevelDefinition>();

            foreach (LevelDefinition level in levels)
            {
                reloaded.Add(LoadAsset<LevelDefinition>(LevelAssetPath(level.LevelNumber)));
            }

            return reloaded;
        }

        private static T LoadAsset<T>(string path) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset == null)
            {
                Debug.LogError($"[GameContentBuilder] Cannot load {path}.");
            }

            return asset;
        }

        private static T LoadComponent<T>(string path) where T : Component
        {
            return LoadAsset<GameObject>(path).GetComponent<T>();
        }

        private static string PrefabAssetPath(string name)
        {
            return $"{PrefabFolder}/{name}.prefab";
        }

        private static string LevelAssetPath(int number)
        {
            return $"{LevelFolder}/Level_{number:00}.asset";
        }

        // ------------------------------------------------------------------ art

        private struct ArtSprites
        {
            public Sprite Bird;
            public Sprite Pig;
            public Sprite Ground;
            public Sprite Dot;
            public Sprite Star;
            public Sprite StarEmpty;
            public Sprite Chip;
            public Sprite Cloud;
            public Sprite Sky;
            public Sprite SlingBack;
            public Sprite SlingFront;
            public Sprite WoodSquare;
            public Sprite WoodWide;
            public Sprite WoodTall;
            public Sprite GlassSquare;
            public Sprite GlassWide;
            public Sprite GlassTall;
            public Sprite StoneSquare;
            public Sprite StoneWide;
            public Sprite StoneTall;
        }

        private static ArtSprites LoadArt()
        {
            return new ArtSprites
            {
                Bird = LoadSprite("bird"),
                Pig = LoadSprite("pig"),
                Ground = LoadSprite("ground"),
                Dot = LoadSprite("dot"),
                Star = LoadSprite("star"),
                StarEmpty = LoadSprite("star_empty"),
                Chip = LoadSprite("chip"),
                Cloud = LoadSprite("cloud"),
                Sky = LoadSprite("sky"),
                SlingBack = LoadSprite("slingshot_back"),
                SlingFront = LoadSprite("slingshot_front"),
                WoodSquare = LoadSprite("block_wood_square"),
                WoodWide = LoadSprite("block_wood_wide"),
                WoodTall = LoadSprite("block_wood_tall"),
                GlassSquare = LoadSprite("block_glass_square"),
                GlassWide = LoadSprite("block_glass_wide"),
                GlassTall = LoadSprite("block_glass_tall"),
                StoneSquare = LoadSprite("block_stone_square"),
                StoneWide = LoadSprite("block_stone_wide"),
                StoneTall = LoadSprite("block_stone_tall")
            };
        }

        private static Sprite LoadSprite(string name)
        {
            string path = $"{SpriteFolder}/{name}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                Debug.LogError($"[GameContentBuilder] Missing sprite {path}. Run the art generator first.");
            }

            return sprite;
        }

        // ------------------------------------------------------------ materials

        private static Material CreateBandMaterial()
        {
            string path = $"{MaterialFolder}/Band.mat";
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogError("[GameContentBuilder] Shader Sprites/Default not found.");
                return null;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(shader) { name = "Band" };
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.color = new Color(0.24f, 0.14f, 0.09f, 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private struct PhysicsMaterials
        {
            public PhysicsMaterial2D Bird;
            public PhysicsMaterial2D Wood;
            public PhysicsMaterial2D Glass;
            public PhysicsMaterial2D Stone;
            public PhysicsMaterial2D Debris;
        }

        private static PhysicsMaterials CreatePhysicsMaterials()
        {
            return new PhysicsMaterials
            {
                Bird = CreatePhysicsMaterial("Bird", 0.45f, 0.28f),
                Wood = CreatePhysicsMaterial("Wood", 0.55f, 0.05f),
                Glass = CreatePhysicsMaterial("Glass", 0.2f, 0.12f),
                Stone = CreatePhysicsMaterial("Stone", 0.65f, 0.02f),
                Debris = CreatePhysicsMaterial("Debris", 0.5f, 0.3f)
            };
        }

        private static PhysicsMaterial2D CreatePhysicsMaterial(string name, float friction, float bounciness)
        {
            string path = $"{PhysicsFolder}/{name}.physicMaterial";
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);

            if (material == null)
            {
                material = new PhysicsMaterial2D(name);
                AssetDatabase.CreateAsset(material, path);
            }

            material.friction = friction;
            material.bounciness = bounciness;
            EditorUtility.SetDirty(material);
            return material;
        }

        // -------------------------------------------------------------- prefabs

        private struct Prefabs
        {
            public Bird Bird;
            public Pig Pig;
            public Block Wood;
            public Block Glass;
            public Block Stone;
            public GameObject Debris;
            public GameObject Dot;
            public GameObject Burst;
        }

        private static Prefabs CreatePrefabs(ArtSprites art, Material bandMaterial, PhysicsMaterials physics)
        {
            var prefabs = new Prefabs();

            prefabs.Dot = CreateDotPrefab(art);
            prefabs.Debris = CreateDebrisPrefab(art, physics);
            prefabs.Burst = CreateBurstPrefab(art);
            prefabs.Bird = CreateBirdPrefab(art, physics);
            prefabs.Pig = CreatePigPrefab(art, physics, prefabs.Burst);
            prefabs.Wood = CreateBlockPrefab(art, physics, prefabs, BlockKind.Wood);
            prefabs.Glass = CreateBlockPrefab(art, physics, prefabs, BlockKind.Glass);
            prefabs.Stone = CreateBlockPrefab(art, physics, prefabs, BlockKind.Stone);

            _ = bandMaterial;
            return prefabs;
        }

        private static GameObject CreateDotPrefab(ArtSprites art)
        {
            var go = new GameObject("TrajectoryDot");
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = art.Dot;
            renderer.sortingOrder = 20;
            return SavePrefab(go, "TrajectoryDot");
        }

        private static GameObject CreateDebrisPrefab(ArtSprites art, PhysicsMaterials physics)
        {
            var go = new GameObject("Debris");
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = art.Chip;
            renderer.sortingOrder = 4;

            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.07f;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.mass = 0.06f;
            body.gravityScale = 1.2f;
            body.linearDamping = 0.4f;
            body.angularDamping = 0.4f;
            body.sharedMaterial = physics.Debris;

            go.AddComponent<Debris>();
            return SavePrefab(go, "Debris");
        }

        private static GameObject CreateBurstPrefab(ArtSprites art)
        {
            var go = new GameObject("SpriteBurst");
            SpriteBurst burst = go.AddComponent<SpriteBurst>();

            new PropSetter(burst)
                .Ref("sprite", art.Chip)
                .Color("color", Color.white)
                .Vector("pieceSize", new Vector2(0.18f, 0.18f))
                .Int("pieceCount", 9)
                .Float("speed", 3.6f)
                .Float("lifeTime", 0.45f)
                .Float("gravityScale", 0.6f)
                .Apply();

            return SavePrefab(go, "SpriteBurst");
        }

        private static Bird CreateBirdPrefab(ArtSprites art, PhysicsMaterials physics)
        {
            var go = new GameObject("Bird");

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = art.Bird;
            renderer.sortingOrder = 5;

            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.44f;
            collider.offset = new Vector2(0.02f, -0.02f);

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.mass = 3f;
            body.gravityScale = 1.15f;
            body.linearDamping = 0.02f;
            body.angularDamping = 0.6f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.sharedMaterial = physics.Bird;

            Bird bird = go.AddComponent<Bird>();
            new PropSetter(bird)
                .Float("maxHealth", 45f)
                .Int("scoreValue", 0)
                .Apply();

            return SavePrefab(go, "Bird").GetComponent<Bird>();
        }

        private static Pig CreatePigPrefab(ArtSprites art, PhysicsMaterials physics, GameObject burst)
        {
            var go = new GameObject("Pig");

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = art.Pig;
            renderer.sortingOrder = 3;

            CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.44f;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.mass = 1.4f;
            body.gravityScale = 1.2f;
            body.linearDamping = 0.1f;
            body.angularDamping = 0.8f;
            body.sharedMaterial = physics.Wood;

            Pig pig = go.AddComponent<Pig>();
            new PropSetter(pig)
                .Float("maxHealth", 26f)
                .Int("scoreValue", 5000)
                .Ref("burstPrefab", burst)
                .Apply();

            return SavePrefab(go, "Pig").GetComponent<Pig>();
        }

        private static Block CreateBlockPrefab(
            ArtSprites art,
            PhysicsMaterials physics,
            Prefabs prefabs,
            BlockKind kind)
        {
            var go = new GameObject(kind + "Block");

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = art.WoodSquare;
            renderer.sortingOrder = 2;

            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.mass = 1f;
            body.gravityScale = 1f;
            body.linearDamping = 0.05f;
            body.angularDamping = 0.3f;
            body.sharedMaterial = physics.Wood;

            Block block = go.AddComponent<Block>();

            Sprite square;
            Sprite wide;
            Sprite tall;
            float health;
            int score;
            PhysicsMaterial2D material;

            switch (kind)
            {
                case BlockKind.Glass:
                    square = art.GlassSquare;
                    wide = art.GlassWide;
                    tall = art.GlassTall;
                    health = 20f;
                    score = 400;
                    material = physics.Glass;
                    break;
                case BlockKind.Stone:
                    square = art.StoneSquare;
                    wide = art.StoneWide;
                    tall = art.StoneTall;
                    health = 130f;
                    score = 700;
                    material = physics.Stone;
                    break;
                default:
                    square = art.WoodSquare;
                    wide = art.WoodWide;
                    tall = art.WoodTall;
                    health = 55f;
                    score = 500;
                    material = physics.Wood;
                    break;
            }

            body.sharedMaterial = material;
            renderer.sprite = square;

            new PropSetter(block)
                .Enum("kind", (int)kind)
                .Float("maxHealth", health)
                .Int("scoreValue", score)
                .Vector("size", Vector2.one)
                .Int("debrisCount", 4)
                .Ref("debrisPrefab", prefabs.Debris != null ? prefabs.Debris.GetComponent<Debris>() : null)
                .Ref("burstPrefab", prefabs.Burst != null ? prefabs.Burst.GetComponent<SpriteBurst>() : null)
                .Ref("art.square", square)
                .Ref("art.wide", wide)
                .Ref("art.tall", tall)
                .Apply();

            return SavePrefab(go, kind + "Block").GetComponent<Block>();
        }

        private static GameObject SavePrefab(GameObject source, string name)
        {
            string path = PrefabAssetPath(name);
            PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // ---------------------------------------------------------------- levels

        private struct LevelLayout
        {
            public int Number;
            public string Name;
            public int Birds;
            public int TwoStars;
            public int ThreeStars;
            public LevelDefinition.BlockSpawn[] Blocks;
            public LevelDefinition.PigSpawn[] Pigs;
        }

        private static LevelDefinition.BlockSpawn Block(BlockKind kind, float x, float y, float width, float height, float rotation = 0f)
        {
            return new LevelDefinition.BlockSpawn
            {
                Kind = kind,
                Position = new Vector2(x, y),
                Rotation = rotation,
                Size = new Vector2(width, height)
            };
        }

        private static LevelDefinition.PigSpawn Pig(float x, float y)
        {
            return new LevelDefinition.PigSpawn { Position = new Vector2(x, y) };
        }

        private static LevelLayout[] Layouts()
        {
            return new[]
            {
                new LevelLayout
                {
                    Number = 1,
                    Name = "LEVEL 1",
                    Birds = 3,
                    TwoStars = 20000,
                    ThreeStars = 28000,
                    Blocks = new[]
                    {
                        Block(BlockKind.Wood, 1.5f, 1f, 0.5f, 2f),
                        Block(BlockKind.Wood, 3.5f, 1f, 0.5f, 2f),
                        Block(BlockKind.Wood, 2.5f, 2.18f, 3f, 0.35f),
                        Block(BlockKind.Glass, -0.3f, 0.45f, 0.9f, 0.9f)
                    },
                    Pigs = new[]
                    {
                        Pig(2.5f, 0.45f),
                        Pig(2.5f, 2.81f)
                    }
                },
                new LevelLayout
                {
                    Number = 2,
                    Name = "LEVEL 2",
                    Birds = 4,
                    TwoStars = 25000,
                    ThreeStars = 38000,
                    Blocks = new[]
                    {
                        Block(BlockKind.Wood, 1f, 1.1f, 0.45f, 2.2f),
                        Block(BlockKind.Wood, 3f, 1.1f, 0.45f, 2.2f),
                        Block(BlockKind.Glass, 2f, 2.35f, 2.6f, 0.3f),
                        Block(BlockKind.Wood, 1.4f, 3.3f, 0.45f, 1.6f),
                        Block(BlockKind.Wood, 2.6f, 3.3f, 0.45f, 1.6f),
                        Block(BlockKind.Stone, 2f, 4.3f, 2.2f, 0.4f),
                        Block(BlockKind.Wood, 4.4f, 0.3f, 0.6f, 0.6f)
                    },
                    Pigs = new[]
                    {
                        Pig(2f, 0.45f),
                        Pig(2f, 2.94f),
                        Pig(2f, 4.94f)
                    }
                },
                new LevelLayout
                {
                    Number = 3,
                    Name = "LEVEL 3",
                    Birds = 4,
                    TwoStars = 30000,
                    ThreeStars = 42000,
                    Blocks = new[]
                    {
                        Block(BlockKind.Stone, 1f, 1.2f, 0.6f, 2.4f),
                        Block(BlockKind.Stone, 2.8f, 1.2f, 0.6f, 2.4f),
                        Block(BlockKind.Wood, 1.9f, 2.6f, 2.6f, 0.4f),
                        Block(BlockKind.Stone, 1.2f, 3.8f, 0.6f, 1.6f),
                        Block(BlockKind.Stone, 2.6f, 3.8f, 0.6f, 1.6f),
                        Block(BlockKind.Wood, 1.9f, 4.78f, 2f, 0.35f),
                        Block(BlockKind.Wood, 4.8f, 1f, 0.5f, 2f),
                        Block(BlockKind.Wood, 4.8f, 2.15f, 1.6f, 0.3f)
                    },
                    Pigs = new[]
                    {
                        Pig(1.9f, 0.45f),
                        Pig(1.9f, 3.24f),
                        Pig(4.8f, 2.72f)
                    }
                }
            };
        }

        private static List<LevelDefinition> CreateLevelAssets()
        {
            var assets = new List<LevelDefinition>();
            LevelLayout[] layouts = Layouts();

            foreach (LevelLayout layout in layouts)
            {
                string path = LevelAssetPath(layout.Number);
                var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);

                if (level == null)
                {
                    level = ScriptableObject.CreateInstance<LevelDefinition>();
                    AssetDatabase.CreateAsset(level, path);
                }

                var setter = new PropSetter(level)
                    .Int("levelNumber", layout.Number)
                    .Str("displayName", layout.Name)
                    .Int("twoStarScore", layout.TwoStars)
                    .Int("threeStarScore", layout.ThreeStars)
                    .Int("birdCount", layout.Birds)
                    .Vector("slingshotPosition", new Vector2(-8f, 0f))
                    .Float("groundWidth", 42f)
                    .Float("groundHeight", 0.6f)
                    .Color("skyColor", new Color(0.51f, 0.8f, 0.92f))
                    .Vector("cameraBoundsMin", new Vector2(-20f, -6f))
                    .Vector("cameraBoundsMax", new Vector2(24f, 16f));

                SerializedProperty blocks = setter.Property("blocks");
                blocks.arraySize = layout.Blocks.Length;

                for (int i = 0; i < layout.Blocks.Length; i++)
                {
                    SerializedProperty element = blocks.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Kind").enumValueIndex = (int)layout.Blocks[i].Kind;
                    element.FindPropertyRelative("Position").vector2Value = layout.Blocks[i].Position;
                    element.FindPropertyRelative("Rotation").floatValue = layout.Blocks[i].Rotation;
                    element.FindPropertyRelative("Size").vector2Value = layout.Blocks[i].Size;
                }

                SerializedProperty pigs = setter.Property("pigs");
                pigs.arraySize = layout.Pigs.Length;

                for (int i = 0; i < layout.Pigs.Length; i++)
                {
                    pigs.GetArrayElementAtIndex(i).FindPropertyRelative("Position").vector2Value = layout.Pigs[i].Position;
                }

                setter.Apply();
                assets.Add(level);
            }

            AssetDatabase.SaveAssets();
            return assets;
        }

        // ---------------------------------------------------------------- scenes

        private static void CreateScenes(Prefabs prefabs, ArtSprites art, Material bandMaterial, List<LevelDefinition> levels)
        {
            foreach (LevelDefinition level in levels)
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                string sceneName = $"Level_{level.LevelNumber:00}";

                Camera camera = CreateCamera(level);
                CreateBackdrop(art);

                GameObject spawnRoot = new GameObject("Level");
                Slingshot slingshot = CreateSlingshot(level, art, bandMaterial, prefabs.Dot);
                CreateEventSystem();
                HudView hud = CreateUi(art, out LevelResultPanel resultPanel);

                var systems = new GameObject("Systems");
                GameBootstrap bootstrap = systems.AddComponent<GameBootstrap>();
                LevelDirector director = systems.AddComponent<LevelDirector>();
                GameInput input = systems.AddComponent<GameInput>();

                new PropSetter(director)
                    .Ref("level", level)
                    .Ref("slingshot", slingshot)
                    .Ref("spawnRoot", spawnRoot.transform)
                    .Ref("birdPrefab", prefabs.Bird)
                    .Ref("pigPrefab", prefabs.Pig)
                    .Ref("woodBlockPrefab", prefabs.Wood)
                    .Ref("glassBlockPrefab", prefabs.Glass)
                    .Ref("stoneBlockPrefab", prefabs.Stone)
                    .Ref("groundSprite", art.Ground)
                    .Float("groundSegmentWidth", 4f)
                    .Float("timeBeforeNextBird", 1f)
                    .Apply();

                new PropSetter(input)
                    .Float("gamepadAimThreshold", 0.25f)
                    .Apply();

                new PropSetter(bootstrap)
                    .Ref("input", input)
                    .Ref("director", director)
                    .Ref("slingshot", slingshot)
                    .Ref("cameraRig", camera.GetComponent<CameraRig>())
                    .Ref("hud", hud)
                    .Str("sceneNameFormat", "Level_{0:00}")
                    .Str("firstLevelScene", "Level_01")
                    .Apply();

                new PropSetter(hud)
                    .Ref("filledStar", art.Star)
                    .Ref("emptyStar", art.StarEmpty)
                    .Ref("resultPanel", resultPanel)
                    .Apply();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, $"{SceneFolder}/{sceneName}.unity");
                AssetDatabase.SaveAssets();
            }

            // Asset references to project assets (level definitions, prefab components) can be
            // dropped while the newly created assets are still being imported. Reopening the
            // saved scenes and assigning the references again makes the result deterministic.
            RewireSceneAssetReferences(prefabs, levels);
        }

        private static void RewireSceneAssetReferences(Prefabs prefabs, List<LevelDefinition> levels)
        {
            foreach (LevelDefinition level in levels)
            {
                string path = $"{SceneFolder}/Level_{level.LevelNumber:00}.unity";
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                LevelDirector director = FindInScene<LevelDirector>(scene);

                if (director == null)
                {
                    Debug.LogError($"[GameContentBuilder] No LevelDirector found in {path}.");
                    continue;
                }

                new PropSetter(director)
                    .Ref("level", level)
                    .Ref("birdPrefab", prefabs.Bird)
                    .Ref("pigPrefab", prefabs.Pig)
                    .Ref("woodBlockPrefab", prefabs.Wood)
                    .Ref("glassBlockPrefab", prefabs.Glass)
                    .Ref("stoneBlockPrefab", prefabs.Stone)
                    .Apply();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
            }
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);

                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static Camera CreateCamera(LevelDefinition level)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            go.transform.position = new Vector3(-6.5f, 2.4f, -10f);

            Camera camera = go.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = level.SkyColor;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 100f;

            go.AddComponent<CameraRig>();
            return camera;
        }

        private static void CreateBackdrop(ArtSprites art)
        {
            var backdrop = new GameObject("Backdrop");

            GameObject sky = new GameObject("Sky");
            sky.transform.SetParent(backdrop.transform, false);
            sky.transform.position = new Vector3(2f, 5f, 1f);
            SpriteRenderer skyRenderer = sky.AddComponent<SpriteRenderer>();
            skyRenderer.sprite = art.Sky;
            skyRenderer.sortingOrder = -100;
            skyRenderer.drawMode = SpriteDrawMode.Sliced;
            skyRenderer.size = new Vector2(90f, 16f);

            Vector3[] cloudPositions =
            {
                new Vector3(-6f, 6.5f, 1f),
                new Vector3(4f, 7.5f, 1f),
                new Vector3(12f, 5.5f, 1f)
            };

            for (int i = 0; i < cloudPositions.Length; i++)
            {
                GameObject cloud = new GameObject("Cloud" + (i + 1));
                cloud.transform.SetParent(backdrop.transform, false);
                cloud.transform.position = cloudPositions[i];
                SpriteRenderer renderer = cloud.AddComponent<SpriteRenderer>();
                renderer.sprite = art.Cloud;
                renderer.sortingOrder = -50;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = new Vector2(5.5f, 2.8f);
            }
        }

        private static Slingshot CreateSlingshot(LevelDefinition level, ArtSprites art, Material bandMaterial, GameObject dotPrefab)
        {
            var root = new GameObject("Slingshot");
            root.transform.position = new Vector3(level.SlingshotPosition.x, level.SlingshotPosition.y, 0f);

            SpriteRenderer back = root.AddComponent<SpriteRenderer>();
            back.sprite = art.SlingBack;
            back.sortingOrder = 1;

            SpriteRenderer front = CreateChildRenderer(root.transform, "Front", art.SlingFront, 8);

            Transform anchor = CreateChild(root.transform, "Anchor", new Vector3(0.5f, 1.88f, 0f));
            Transform backProng = CreateChild(root.transform, "BackProng", new Vector3(0.21f, 1.94f, 0f));
            Transform frontProng = CreateChild(root.transform, "FrontProng", new Vector3(0.79f, 1.94f, 0f));

            LineRenderer backBand = CreateBand(root.transform, "BackBand", bandMaterial, 3);
            LineRenderer frontBand = CreateBand(root.transform, "FrontBand", bandMaterial, 7);

            GameObject trajectoryGo = new GameObject("Trajectory");
            trajectoryGo.transform.SetParent(root.transform, false);
            TrajectoryPreview trajectory = trajectoryGo.AddComponent<TrajectoryPreview>();

            new PropSetter(trajectory)
                .Ref("dotPrefab", dotPrefab != null ? dotPrefab.GetComponent<SpriteRenderer>() : null)
                .Int("dotCount", 22)
                .Float("timeStep", 0.055f)
                .Apply();

            Slingshot slingshot = root.AddComponent<Slingshot>();
            new PropSetter(slingshot)
                .Ref("anchor", anchor)
                .Ref("backProng", backProng)
                .Ref("frontProng", frontProng)
                .Ref("backBand", backBand)
                .Ref("frontBand", frontBand)
                .Ref("trajectory", trajectory)
                .Float("maxPullDistance", 2.2f)
                .Float("launchMultiplier", 8.5f)
                .Float("minimumHeight", 0.55f)
                .Apply();

            _ = front;
            return slingshot;
        }

        private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        private static SpriteRenderer CreateChildRenderer(Transform parent, string name, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static LineRenderer CreateBand(Transform parent, string name, Material material, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.material = material;
            line.widthMultiplier = 0.085f;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            line.sortingOrder = sortingOrder;
            line.startColor = new Color(0.24f, 0.14f, 0.09f, 1f);
            line.endColor = new Color(0.24f, 0.14f, 0.09f, 1f);
            line.positionCount = 2;
            return line;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule module = go.AddComponent<InputSystemUIInputModule>();

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ProjectInputActions);

            if (actions == null)
            {
                module.AssignDefaultActions();
                return;
            }

            module.actionsAsset = actions;
            InputActionMap uiMap = actions.FindActionMap("UI", true);

            module.point = Reference(uiMap, "Point");
            module.leftClick = Reference(uiMap, "Click");
            module.rightClick = Reference(uiMap, "RightClick");
            module.middleClick = Reference(uiMap, "MiddleClick");
            module.scrollWheel = Reference(uiMap, "ScrollWheel");
            module.move = Reference(uiMap, "Navigate");
            module.submit = Reference(uiMap, "Submit");
            module.cancel = Reference(uiMap, "Cancel");
        }

        private static InputActionReference Reference(InputActionMap map, string actionName)
        {
            InputAction action = map.FindAction(actionName);
            return action != null ? InputActionReference.Create(action) : null;
        }

        private static HudView CreateUi(ArtSprites art, out LevelResultPanel resultPanel)
        {
            var canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // The HUD is created first and the result overlay last so the overlay
            // always renders on top of the in-game widgets.
            HudView hud = CreateHud(canvas.transform, art);
            resultPanel = CreateResultPanel(canvas.transform, art);

            new PropSetter(hud)
                .Ref("resultPanel", resultPanel)
                .Apply();

            return hud;
        }

        private static HudView CreateHud(Transform parent, ArtSprites art)
        {
            var hudGo = new GameObject("HUD", typeof(RectTransform));
            hudGo.transform.SetParent(parent, false);

            RectTransform hudRect = (RectTransform)hudGo.transform;
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.pivot = new Vector2(0.5f, 0.5f);
            hudRect.anchoredPosition = Vector2.zero;
            hudRect.sizeDelta = Vector2.zero;

            RectTransform topLeft = UiFactory.CreateRect(
                "TopLeft",
                hudGo.transform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(48f, -36f),
                new Vector2(700f, 220f));

            Text levelLabel = UiFactory.CreateText(topLeft, Vector2.zero, new Vector2(700f, 62f), "LEVEL 1", 48, TextAnchor.UpperLeft, Color.white);
            Text scoreLabel = UiFactory.CreateText(topLeft, new Vector2(0f, -70f), new Vector2(700f, 56f), "SCORE  0", 40, TextAnchor.UpperLeft, Color.white);
            Text birdsLabel = UiFactory.CreateText(topLeft, new Vector2(0f, -132f), new Vector2(700f, 50f), "BIRDS  0", 36, TextAnchor.UpperLeft, Color.white);
            scoreLabel.name = "ScoreLabel";
            birdsLabel.name = "BirdsLabel";

            // Live stars sit in the top-left corner, under the level / score / birds block.
            List<Image> liveStars = UiFactory.CreateStarRow(hudGo.transform, Vector2.zero, 74f, 62f, art.StarEmpty);

            for (int i = 0; i < liveStars.Count; i++)
            {
                var starRect = (RectTransform)liveStars[i].transform;
                starRect.anchorMin = new Vector2(0f, 1f);
                starRect.anchorMax = new Vector2(0f, 1f);
                starRect.pivot = new Vector2(0f, 1f);
                starRect.anchoredPosition = new Vector2(48f + i * 74f, -252f);
            }

            // The restart button is pinned to the top-right corner so it never covers the play area.
            Button restartButton = UiFactory.CreateButton(
                hudGo.transform,
                Vector2.zero,
                new Vector2(280f, 84f),
                "RESTART (R)",
                30,
                out _);

            var restartRect = (RectTransform)restartButton.transform;
            restartRect.anchorMin = new Vector2(1f, 1f);
            restartRect.anchorMax = new Vector2(1f, 1f);
            restartRect.pivot = new Vector2(1f, 1f);
            restartRect.anchoredPosition = new Vector2(-36f, -36f);

            var hud = hudGo.AddComponent<HudView>();
            new PropSetter(hud)
                .Ref("levelLabel", levelLabel)
                .Ref("scoreLabel", scoreLabel)
                .Ref("birdsLabel", birdsLabel)
                .Refs("liveStars", ToArray(liveStars))
                .Ref("filledStar", art.Star)
                .Ref("emptyStar", art.StarEmpty)
                .Ref("restartButton", restartButton)
                .Apply();

            return hud;
        }

        private static LevelResultPanel CreateResultPanel(Transform parent, ArtSprites art)
        {
            var panelGo = new GameObject("ResultPanel", typeof(RectTransform));
            panelGo.transform.SetParent(parent, false);

            RectTransform root = (RectTransform)panelGo.transform;
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(920f, 580f);

            Image background = panelGo.AddComponent<Image>();
            background.color = new Color(0.05f, 0.07f, 0.1f, 0.88f);

            Text title = UiFactory.CreateCenteredText(root, new Vector2(0f, 190f), new Vector2(880f, 80f), "LEVEL CLEARED", 64, Color.white);
            title.name = "TitleLabel";

            List<Image> stars = UiFactory.CreateStarRow(root, new Vector2(0f, 60f), 150f, 130f, art.StarEmpty);

            Text score = UiFactory.CreateCenteredText(root, new Vector2(0f, -40f), new Vector2(880f, 60f), "SCORE  0", 40, new Color(1f, 0.92f, 0.6f));
            score.name = "ScoreLabel";

            Button primary = UiFactory.CreateButton(root, new Vector2(-190f, -180f), new Vector2(340f, 96f), "NEXT LEVEL", 34, out Text primaryLabel);
            Button secondary = UiFactory.CreateButton(root, new Vector2(190f, -180f), new Vector2(340f, 96f), "RESTART", 34, out Text secondaryLabel);

            var panel = panelGo.AddComponent<LevelResultPanel>();
            new PropSetter(panel)
                .Ref("titleLabel", title)
                .Ref("scoreLabel", score)
                .Refs("stars", ToArray(stars))
                .Ref("filledStar", art.Star)
                .Ref("emptyStar", art.StarEmpty)
                .Ref("primaryButton", primary)
                .Ref("primaryLabel", primaryLabel)
                .Ref("secondaryButton", secondary)
                .Ref("secondaryLabel", secondaryLabel)
                .Apply();

            return panel;
        }

        private static Object[] ToArray(List<Image> images)
        {
            var array = new Object[images.Count];

            for (int i = 0; i < images.Count; i++)
            {
                array[i] = images[i];
            }

            return array;
        }

        // -------------------------------------------------------- build settings

        private static void RegisterScenes(List<LevelDefinition> levels)
        {
            var scenes = new List<EditorBuildSettingsScene>();

            foreach (LevelDefinition level in levels)
            {
                string path = $"{SceneFolder}/Level_{level.LevelNumber:00}.unity";
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                bool alreadyAdded = false;

                foreach (EditorBuildSettingsScene scene in scenes)
                {
                    if (scene.path == existing.path)
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                {
                    scenes.Add(existing);
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ----------------------------------------------------------------- utils

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string[] parts = assetPath.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        /// <summary>Fluent writer for serialized private fields of a component or asset.</summary>
        private sealed class PropSetter
        {
            private readonly SerializedObject _serialized;

            public PropSetter(Object target)
            {
                _serialized = new SerializedObject(target);
            }

            public SerializedProperty Property(string path)
            {
                SerializedProperty property = _serialized.FindProperty(path);

                if (property != null)
                {
                    return property;
                }

                string[] parts = path.Split('.');
                property = _serialized.FindProperty(parts[0]);

                for (int i = 1; i < parts.Length && property != null; i++)
                {
                    property = property.FindPropertyRelative(parts[i]);
                }

                return property;
            }

            public PropSetter Ref(string path, Object value)
            {
                Set(path, p => p.objectReferenceValue = value);
                return this;
            }

            public PropSetter Refs(string path, Object[] values)
            {
                Set(path, p =>
                {
                    p.arraySize = values.Length;

                    for (int i = 0; i < values.Length; i++)
                    {
                        p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                    }
                });
                return this;
            }

            public PropSetter Float(string path, float value)
            {
                Set(path, p => p.floatValue = value);
                return this;
            }

            public PropSetter Int(string path, int value)
            {
                Set(path, p => p.intValue = value);
                return this;
            }

            public PropSetter Bool(string path, bool value)
            {
                Set(path, p => p.boolValue = value);
                return this;
            }

            public PropSetter Enum(string path, int value)
            {
                Set(path, p => p.enumValueIndex = value);
                return this;
            }

            public PropSetter Vector(string path, Vector2 value)
            {
                Set(path, p => p.vector2Value = value);
                return this;
            }

            public PropSetter Color(string path, Color value)
            {
                Set(path, p => p.colorValue = value);
                return this;
            }

            public PropSetter Str(string path, string value)
            {
                Set(path, p => p.stringValue = value);
                return this;
            }

            public void Apply()
            {
                _serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(_serialized.targetObject);
            }

            private void Set(string path, System.Action<SerializedProperty> assign)
            {
                SerializedProperty property = Property(path);

                if (property == null)
                {
                    Debug.LogError($"[GameContentBuilder] Serialized property '{path}' not found on {_serialized.targetObject.GetType().Name}.");
                    return;
                }

                assign(property);
            }
        }
    }
}
