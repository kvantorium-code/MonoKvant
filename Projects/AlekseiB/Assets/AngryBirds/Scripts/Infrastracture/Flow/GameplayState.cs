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

        private readonly ISceneLoader _sceneLoader;
        private readonly ILevelCatalog _levels;

        public GameplayState(ISceneLoader sceneLoader, ILevelCatalog levels)
        {
            _sceneLoader = sceneLoader;
            _levels = levels;
        }

        /// <summary>The level the Gameplay scene has to play, null before the state was entered.</summary>
        public LevelDefinition Level { get; private set; }

        public void Enter(int levelNumber)
        {
            Level = _levels.Get(levelNumber) ?? _levels.Get(FirstLevel);

            _sceneLoader.Load(SceneNames.Gameplay);
        }

        public void Exit()
        {
        }
    }
}
