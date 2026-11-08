using VContainer;

namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>
    /// Switches between the game states registered in the project scope.
    /// States are resolved on demand: they depend on the machine themselves,
    /// so injecting them into its constructor would make a cycle.
    /// </summary>
    public sealed class GameStateMachine : IGameStateMachine
    {
        private readonly IObjectResolver _resolver;
        private IGameState _current;

        public GameStateMachine(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void Enter<TState>() where TState : class, IState
        {
            ChangeState<TState>().Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>
        {
            ChangeState<TState>().Enter(payload);
        }

        private TState ChangeState<TState>() where TState : class, IGameState
        {
            _current?.Exit();

            // The new state is current before its Enter runs, so Enter may switch to another state.
            TState state = _resolver.Resolve<TState>();
            _current = state;
            return state;
        }
    }
}
