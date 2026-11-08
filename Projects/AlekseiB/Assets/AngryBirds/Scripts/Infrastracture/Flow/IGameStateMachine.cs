namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>
    /// Owns the current game state. Entering a state first exits the previous one.
    /// </summary>
    public interface IGameStateMachine
    {
        void Enter<TState>() where TState : class, IState;

        void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>;
    }
}
