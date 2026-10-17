using System;
using System.Collections.Generic;
using UnityEngine;
using AngryBirds.Core.Gameplay;

namespace AngryBirds.Core.Level
{
    /// <summary>
    /// Data of a single level: where things stand, how many birds the player gets and
    /// which score is needed for two and three stars.
    /// The level contains no logic, it is filled in by the editor and read by
    /// <see cref="LevelDirector"/> at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "AngryBirds/Level Definition", order = 0)]
    public sealed class LevelDefinition : ScriptableObject
    {
        [Serializable]
        public struct BlockSpawn
        {
            public BlockKind Kind;
            public Vector2 Position;
            [Range(-180f, 180f)] public float Rotation;
            public Vector2 Size;
        }

        [Serializable]
        public struct PigSpawn
        {
            public Vector2 Position;
        }

        [Header("Progress")]
        [SerializeField, Min(1)] private int levelNumber = 1;
        [SerializeField] private string displayName = "Level";
        [SerializeField, Min(0)] private int twoStarScore = 20000;
        [SerializeField, Min(0)] private int threeStarScore = 35000;

        [Header("Birds")]
        [SerializeField, Min(1)] private int birdCount = 3;

        [Header("World")]
        [SerializeField] private Vector2 slingshotPosition = new Vector2(-8f, 0f);
        [SerializeField, Min(8f)] private float groundWidth = 42f;
        [SerializeField, Min(0.1f)] private float groundHeight = 0.6f;
        [SerializeField] private Color skyColor = new Color(0.51f, 0.8f, 0.92f);
        [SerializeField] private Vector2 cameraBoundsMin = new Vector2(-20f, -6f);
        [SerializeField] private Vector2 cameraBoundsMax = new Vector2(24f, 16f);

        [Header("Layout")]
        [SerializeField] private BlockSpawn[] blocks = Array.Empty<BlockSpawn>();
        [SerializeField] private PigSpawn[] pigs = Array.Empty<PigSpawn>();

        public int LevelNumber => levelNumber;

        public string DisplayName => displayName;

        public int TwoStarScore => twoStarScore;

        public int ThreeStarScore => threeStarScore;

        public int BirdCount => birdCount;

        public Vector2 SlingshotPosition => slingshotPosition;

        public float GroundWidth => groundWidth;

        public float GroundHeight => groundHeight;

        public Color SkyColor => skyColor;

        public Vector2 CameraBoundsMin => cameraBoundsMin;

        public Vector2 CameraBoundsMax => cameraBoundsMax;

        public int PigCount => pigs?.Length ?? 0;

        public IReadOnlyList<BlockSpawn> Blocks => blocks;

        public IReadOnlyList<PigSpawn> Pigs => pigs;
    }
}
