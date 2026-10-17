using UnityEngine;

namespace AngryBirds.Core.Damage
{
    /// <summary>
    /// A single damage event.
    /// </summary>
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, Vector2 point, Vector2 direction, GameObject source)
        {
            Amount = amount;
            Point = point;
            Direction = direction;
            Source = source;
        }

        /// <summary>Hit points to remove.</summary>
        public float Amount { get; }

        /// <summary>World position of the hit.</summary>
        public Vector2 Point { get; }

        /// <summary>Normalized direction the damage travels into the object.</summary>
        public Vector2 Direction { get; }

        /// <summary>Object that caused the damage, may be null.</summary>
        public GameObject Source { get; }
    }
}
