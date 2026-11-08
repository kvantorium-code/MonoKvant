namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>
    /// First state of the application. Project-wide initialization (progress, localization)
    /// goes here, then the flow moves on to the main menu.
    /// </summary>
    public sealed class BootState : IState
    {
        private readonly IGameStateMachine _states;

        public BootState(IGameStateMachine states)
        {
            _states = states;
        }

        public void Enter()
        {
            _states.Enter<MainMenuState>();
        }

        public void Exit()
        {
        }
    }
}
