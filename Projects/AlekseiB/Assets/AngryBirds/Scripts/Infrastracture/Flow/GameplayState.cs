using AngryBirds.Core.Level;

namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>
    /// Playing a level. Takes the level from the catalog and (re)loads the Gameplay scene, which
    /// builds the level from <see cref="Level"/>. Entering it again restarts the level, a number
    /// that is not in the catalog (for instance past the last level) falls back to the first level.
    /// Reloading the scene is also the level cleanup: everything a level spawned goes away with it.
    /// </summary>
    public sealed class GameplayState : IPayloadState<int>
    {
        public const int FirstLevel = 1;

        private readonly IGameSceneManager _scenes;
        private readonly ILevelCatalog _levels;

        public GameplayState(IGameSceneManager scenes, ILevelCatalog levels)
        {
            _scenes = scenes;
            _levels = levels;
        }

        /// <summary>The level the Gameplay scene has to play, null before the state was entered.</summary>
        public LevelDefinition Level { get; private set; }

        public void Enter(int levelNumber)
        {
            Level = _levels.Get(levelNumber) ?? _levels.Get(FirstLevel);

            // The state machine is synchronous, the transition runs to completion on its own.
            _scenes.Load(SceneName.Gameplay);
        }

        public void Exit()
        {
        }
    }
}
