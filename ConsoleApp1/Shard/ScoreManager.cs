/*
*
*   ScoreManager - Core scoring system that handles:
*       - File-based score storage and retrieval
*       - Per-game score isolation
*       - Player name management
*       - Load, save, and clear functionality
*
*   This is a game engine-level service that can be used by any game.
*   Games can store scores separately by game name, or use global scores.
*
*   @version 1.0
*
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Shard
{
    public class ScoreManager
    {
        private static ScoreManager instance;
        private List<ScoreEntry> scores;
        private string saveFilePath;
        private string currentPlayerName;
        private string currentGameName;

        public static ScoreManager getInstance()
        {
            if (instance == null)
            {
                instance = new ScoreManager();
            }
            return instance;
        }

        private ScoreManager()
        {
            scores = new List<ScoreEntry>();
            currentPlayerName = "";
            currentGameName = "";

            string baseDir = Bootstrap.getBaseDir();
            saveFilePath = Path.Combine(baseDir, "scores.dat");

            LoadScores();
        }

        public void SetPlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                currentPlayerName = "Player";
            }
            else
            {
                currentPlayerName = name.Trim();
            }
        }

        public string GetPlayerName()
        {
            return currentPlayerName;
        }

        public void SetCurrentGame(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                currentGameName = "";
            }
            else
            {
                currentGameName = gameName.Trim();
            }
        }

        public string GetCurrentGame()
        {
            return currentGameName;
        }

        public void AddScore(int score)
        {
            AddScore(score, currentGameName);
        }

        public void AddScore(int score, string gameName)
        {
            string playerName = string.IsNullOrWhiteSpace(currentPlayerName) ? "Player" : currentPlayerName;
            string game = string.IsNullOrWhiteSpace(gameName) ? "" : gameName.Trim();

            ScoreEntry entry = new ScoreEntry(playerName, score, game);
            scores.Add(entry);

            Debug.getInstance().log($"Score added: {entry}", Debug.DEBUG_LEVEL_ALL);

            SaveScores();
        }

        public void LoadScores()
        {
            try
            {
                if (!File.Exists(saveFilePath))
                {
                    Debug.getInstance().log("Score file does not exist, starting fresh", Debug.DEBUG_LEVEL_ALL);
                    scores = new List<ScoreEntry>();
                    return;
                }

                string jsonContent = File.ReadAllText(saveFilePath);

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    scores = new List<ScoreEntry>();
                    return;
                }

                scores = JsonSerializer.Deserialize<List<ScoreEntry>>(jsonContent) ?? new List<ScoreEntry>();

                Debug.getInstance().log($"Loaded {scores.Count} scores from file", Debug.DEBUG_LEVEL_ALL);
            }
            catch (Exception ex)
            {
                Debug.getInstance().log($"Error loading scores: {ex.Message}", Debug.DEBUG_LEVEL_ERROR);
                scores = new List<ScoreEntry>();
            }
        }

        public void SaveScores()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonContent = JsonSerializer.Serialize(scores, options);
                File.WriteAllText(saveFilePath, jsonContent);

                Debug.getInstance().log($"Saved {scores.Count} scores to file", Debug.DEBUG_LEVEL_ALL);
            }
            catch (Exception ex)
            {
                Debug.getInstance().log($"Error saving scores: {ex.Message}", Debug.DEBUG_LEVEL_ERROR);
            }
        }

        public List<ScoreEntry> GetAllScores()
        {
            return scores.OrderByDescending(s => s.Score).ToList();
        }

        public List<ScoreEntry> GetScoresForGame(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return GetAllScores();
            }
            return scores
                .Where(s => s.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.Score)
                .ToList();
        }

        public List<ScoreEntry> GetTopScores(int count)
        {
            return scores.OrderByDescending(s => s.Score).Take(count).ToList();
        }

        public List<ScoreEntry> GetTopScoresForGame(string gameName, int count)
        {
            return GetScoresForGame(gameName).Take(count).ToList();
        }

        public string GetScoresDisplay()
        {
            return GetScoresDisplay(currentGameName);
        }

        public string GetScoresDisplay(string gameName)
        {
            List<ScoreEntry> filteredScores = string.IsNullOrWhiteSpace(gameName)
                ? GetAllScores()
                : GetScoresForGame(gameName);

            if (filteredScores.Count == 0)
            {
                return "No scores yet!";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== HIGH SCORES ===");

            for (int i = 0; i < filteredScores.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {filteredScores[i].PlayerName}: {filteredScores[i].Score}");
            }

            return sb.ToString();
        }

        public void ClearScores()
        {
            scores.Clear();
            SaveScores();
            Debug.getInstance().log("All scores cleared", Debug.DEBUG_LEVEL_ALL);
        }

        public void ClearScoresForGame(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                ClearScores();
                return;
            }

            scores.RemoveAll(s => s.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
            SaveScores();
            Debug.getInstance().log($"Scores cleared for game: {gameName}", Debug.DEBUG_LEVEL_ALL);
        }

        public int GetScoreCount()
        {
            return scores.Count;
        }

        public int GetScoreCountForGame(string gameName)
        {
            return GetScoresForGame(gameName).Count;
        }

        public List<string> GetAllGameNames()
        {
            return scores
                .Select(s => s.GameName)
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }
    }
}
