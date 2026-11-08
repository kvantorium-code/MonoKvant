namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>
    /// Main menu of the game. The menu screen does not exist yet, so the state
    /// forwards the player straight to the first level.
    /// </summary>
    public sealed class MainMenuState : IState
    {
        private readonly IGameStateMachine _states;

        public MainMenuState(IGameStateMachine states)
        {
            _states = states;
        }

        public void Enter()
        {
            _states.Enter<GameplayState, int>(GameplayState.FirstLevel);
        }

        public void Exit()
        {
        }
    }
}
