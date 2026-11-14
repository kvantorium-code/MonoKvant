using VContainer.Unity;
using AngryBirds.Infrastructure.Flow;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace AngryBirds.Infrastructure.Entry
{
    /// <summary>
    /// Single entry point of the application: starts the game state machine.
    /// </summary>
    public sealed class AppEntryPoint : IStartable
    {
        private readonly IGameStateMachine _states;

        public AppEntryPoint(IGameStateMachine states)
        {
            _states = states;
        }

        public void Start()
        {
            // A level scene opened directly in the editor already is the screen,
            // running the boot flow would replace it.
            if (UnitySceneManager.GetActiveScene().name != SceneName.Boot.ToSceneName())
            {
                return;
            }

            _states.Enter<BootState>();
        }
    }
}
