using System;
using UnityEngine;
using AngryBirds.Core.Gameplay;

namespace AngryBirds.Core.Level
{
    public enum LevelState
    {
        Ready,
        Playing,
        Won,
        Lost
    }

    /// <summary>Final result of a level.</summary>
    public readonly struct LevelOutcome
    {
        public LevelOutcome(LevelState state, int score, int stars)
        {
            State = state;
            Score = score;
            Stars = stars;
        }

        public LevelState State { get; }

        public int Score { get; }

        public int Stars { get; }

        public bool IsWin => State == LevelState.Won;
    }

    /// <summary>
    /// Plain C# state of the running level: score, remaining birds, pigs and win or loss.
    /// Holds no scene logic, so the rules of a level can be observed and tested on their own.
    /// </summary>
    public sealed class LevelRuntime
    {
        /// <summary>Score for every unused bird at the moment the level is won.</summary>
        public const int BirdBonusScore = 10000;

        private int _pigsRemaining;
        private int _pigsTotal;
        private int _birdsRemaining;
        private int _twoStarScore;
        private int _threeStarScore;

        public LevelState State { get; private set; } = LevelState.Ready;

        public int Score { get; private set; }

        public int PigsTotal => _pigsTotal;

        public int PigsRemaining => _pigsRemaining;

        public int BirdsRemaining => _birdsRemaining;

        public event Action<LevelState> StateChanged;

        public event Action<int> ScoreChanged;

        public event Action<int> BirdsChanged;

        public event Action<Block> BlockDestroyed;

        public event Action<Pig> PigDestroyed;

        public event Action<Bird> BirdLaunched;

        public event Action<Bird> BirdSettled;

        public event Action<LevelOutcome> Finished;

        public void Initialize(int pigsTotal, int birdsTotal, int twoStarScore, int threeStarScore)
        {
            _pigsTotal = pigsTotal;
            _pigsRemaining = pigsTotal;
            _birdsRemaining = birdsTotal;
            _twoStarScore = twoStarScore;
            _threeStarScore = threeStarScore;
            Score = 0;
            State = LevelState.Ready;
        }

        public void BeginPlaying()
        {
            if (State != LevelState.Ready)
            {
                return;
            }

            State = LevelState.Playing;
            StateChanged?.Invoke(State);
        }

        public void NotifyBirdLaunched(Bird bird)
        {
            if (_birdsRemaining > 0)
            {
                _birdsRemaining--;
            }

            BeginPlaying();
            BirdsChanged?.Invoke(_birdsRemaining);
            BirdLaunched?.Invoke(bird);
        }

        public void NotifyBirdSettled(Bird bird)
        {
            BirdSettled?.Invoke(bird);
        }

        public void NotifyBlockDestroyed(Block block)
        {
            if (IsFinished)
            {
                return;
            }

            AddScore(block != null ? block.ScoreValue : 0);
            BlockDestroyed?.Invoke(block);
        }

        public void NotifyPigDestroyed(Pig pig)
        {
            if (IsFinished)
            {
                return;
            }

            if (_pigsRemaining > 0)
            {
                _pigsRemaining--;
            }

            AddScore(pig != null ? pig.ScoreValue : 0);
            PigDestroyed?.Invoke(pig);

            if (_pigsRemaining <= 0)
            {
                Win();
            }
        }

        public void NotifyOutOfBirds()
        {
            if (IsFinished)
            {
                return;
            }

            if (_pigsRemaining > 0)
            {
                Lose();
            }
        }

        public int GetStars()
        {
            if (Score >= _threeStarScore)
            {
                return 3;
            }

            return Score >= _twoStarScore ? 2 : 1;
        }

        private bool IsFinished => State == LevelState.Won || State == LevelState.Lost;

        private void Win()
        {
            if (_birdsRemaining > 0)
            {
                Score += _birdsRemaining * BirdBonusScore;
                _birdsRemaining = 0;
                ScoreChanged?.Invoke(Score);
                BirdsChanged?.Invoke(_birdsRemaining);
            }

            State = LevelState.Won;
            StateChanged?.Invoke(State);
            Finished?.Invoke(new LevelOutcome(LevelState.Won, Score, GetStars()));
        }

        private void Lose()
        {
            State = LevelState.Lost;
            StateChanged?.Invoke(State);
            Finished?.Invoke(new LevelOutcome(LevelState.Lost, Score, 0));
        }

        private void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Score += amount;
            ScoreChanged?.Invoke(Score);
        }
    }
}
