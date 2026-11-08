using System.Collections.Generic;

namespace AngryBirds.Core.Level
{
    /// <summary>
    /// Ordered list of all levels of the game. Replaces scene names and build index lookups.
    /// </summary>
    public interface ILevelCatalog
    {
        int Count { get; }

        IReadOnlyList<LevelDefinition> Levels { get; }

        /// <summary>Level with the given number, null when there is no such level.</summary>
        LevelDefinition Get(int number);

        /// <summary>The level that follows the given one, false after the last level.</summary>
        bool TryGetNext(int number, out LevelDefinition next);
    }
}
