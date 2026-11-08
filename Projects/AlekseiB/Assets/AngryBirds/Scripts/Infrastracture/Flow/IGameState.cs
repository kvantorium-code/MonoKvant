namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>Base of every game state, a state is left through <see cref="Exit"/>.</summary>
    public interface IGameState
    {
        void Exit();
    }

    /// <summary>State that needs no data to start.</summary>
    public interface IState : IGameState
    {
        void Enter();
    }

    /// <summary>State that starts with a payload, for example the level number.</summary>
    public interface IPayloadState<in TPayload> : IGameState
    {
        void Enter(TPayload payload);
    }
}
