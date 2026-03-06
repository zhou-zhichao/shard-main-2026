using Shard;
using System;

namespace GameTest
{
    static class DemoRunState
    {
        public const string ScoreGameName = "GameTest";
        public const int StartingHealth = 30;

        public static int Score { get; private set; }
        public static int Health { get; private set; }
        public static int CurrentLevel { get; private set; }
        public static bool Paused { get; private set; }
        public static bool ScoreSubmitted { get; private set; }

        public static void StartNewRun()
        {
            Score = 0;
            Health = StartingHealth;
            CurrentLevel = 1;
            Paused = false;
            ScoreSubmitted = false;

            ScoreManager.getInstance().SetCurrentGame(ScoreGameName);
        }

        public static void AdvanceToLevel(int level)
        {
            CurrentLevel = level;
            Paused = false;
        }

        public static void SetPaused(bool paused)
        {
            Paused = paused;
        }

        public static void AddScore(int amount)
        {
            Score = Math.Max(0, Score + amount);
        }

        public static bool TakeDamage(int amount, int scorePenalty)
        {
            Health = Math.Max(0, Health - amount);
            AddScore(-scorePenalty);
            return Health <= 0;
        }

        public static void SubmitScoreIfNeeded()
        {
            if (ScoreSubmitted)
            {
                return;
            }

            ScoreManager.getInstance().SetCurrentGame(ScoreGameName);
            ScoreManager.getInstance().AddScore(Score, ScoreGameName);
            ScoreSubmitted = true;
        }

        public static void SubmitScoreWithName(string playerName)
        {
            if (ScoreSubmitted)
            {
                return;
            }

            ScoreManager.getInstance().SetPlayerName(playerName);
            ScoreManager.getInstance().SetCurrentGame(ScoreGameName);
            ScoreManager.getInstance().AddScore(Score, ScoreGameName);
            ScoreSubmitted = true;
        }
    }
}
