using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngryBirds.Core.Level
{
    /// <summary>
    /// Asset that lists every <see cref="LevelDefinition"/> of the game in play order.
    /// Registered in the project scope as the <see cref="ILevelCatalog"/> service.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "AngryBirds/Level Catalog", order = 1)]
    public sealed class LevelCatalog : ScriptableObject, ILevelCatalog
    {
        [SerializeField] private LevelDefinition[] levels = Array.Empty<LevelDefinition>();

        public int Count => levels.Length;

        public IReadOnlyList<LevelDefinition> Levels => levels;

        public LevelDefinition Get(int number)
        {
            int index = IndexOf(number);
            return index >= 0 ? levels[index] : null;
        }

        public bool TryGetNext(int number, out LevelDefinition next)
        {
            int index = IndexOf(number);

            if (index >= 0 && index + 1 < levels.Length)
            {
                next = levels[index + 1];
                return next != null;
            }

            next = null;
            return false;
        }

        private int IndexOf(int number)
        {
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] != null && levels[i].LevelNumber == number)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
