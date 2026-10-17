using System;
using UnityEngine;

namespace AngryBirds.Core.Gameplay
{
    /// <summary>
    /// Square, wide and tall artwork of one block material.
    /// A block picks the variant whose aspect ratio is closest to the requested size,
    /// so the texture is never stretched far from its original proportions.
    /// </summary>
    [Serializable]
    public sealed class BlockArtVariants
    {
        [SerializeField] private Sprite square;
        [SerializeField] private Sprite wide;
        [SerializeField] private Sprite tall;

        public Sprite Pick(float aspectRatio)
        {
            if (aspectRatio > 1f)
            {
                return wide != null ? wide : square;
            }

            if (aspectRatio < 1f)
            {
                return tall != null ? tall : square;
            }

            return square;
        }
    }
}
