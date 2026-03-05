/*
*
*   ScoreEntry - Represents a single score record in the scoring system.
*   Includes game name for score isolation between different games.
*
*   @version 1.0
*
*/

using System;

namespace Shard
{
    [Serializable]
    public class ScoreEntry
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public DateTime Timestamp { get; set; }
        public string GameName { get; set; }

        public ScoreEntry()
        {
            PlayerName = "Unknown";
            Score = 0;
            Timestamp = DateTime.Now;
            GameName = "";
        }

        public ScoreEntry(string playerName, int score) : this()
        {
            PlayerName = playerName;
            Score = score;
        }

        public ScoreEntry(string playerName, int score, string gameName) : this(playerName, score)
        {
            GameName = gameName ?? "";
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(GameName))
            {
                return $"{PlayerName}: {Score} ({Timestamp:yyyy-MM-dd HH:mm})";
            }
            return $"{PlayerName}: {Score} [{GameName}] ({Timestamp:yyyy-MM-dd HH:mm})";
        }
    }
}
