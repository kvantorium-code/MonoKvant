using UnityEngine.SceneManagement;
using VContainer.Unity;
using AngryBirds.Infrastructure.Flow;

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
            if (SceneManager.GetActiveScene().name != SceneNames.Boot)
            {
                return;
            }

            _states.Enter<BootState>();
        }
    }
}
