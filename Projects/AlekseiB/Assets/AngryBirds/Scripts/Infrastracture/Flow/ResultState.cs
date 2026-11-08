using AngryBirds.Core.Level;

namespace AngryBirds.Infrastructure.Flow
{
    /// <summary>What the result state needs to know about the finished level.</summary>
    public readonly struct ResultPayload
    {
        public ResultPayload(int levelNumber, LevelOutcome outcome)
        {
            LevelNumber = levelNumber;
            Outcome = outcome;
        }

        public int LevelNumber { get; }

        public LevelOutcome Outcome { get; }
    }

    /// <summary>
    /// A level has ended and the result is on screen. The result screen itself is still
    /// owned by the HUD, from here the player continues with <see cref="GameplayState"/>.
    /// </summary>
    public sealed class ResultState : IPayloadState<ResultPayload>
    {
        public ResultPayload Result { get; private set; }

        public void Enter(ResultPayload payload)
        {
            Result = payload;
        }

        public void Exit()
        {
        }
    }
}
