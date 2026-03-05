/*
*
*   ScoreUI - Demo UI for the scoring system
*
*   This is an example/demo implementation showing how to use the ScoreManager.
*   It provides:
*       - Player name input
*       - Game selection for score isolation
*       - Score display
*       - Add/Clear functionality
*
*   Games should implement their own UI using ScoreManager APIs directly.
*
*   @version 1.0
*
*/

using SDL;
using Shard;
using System;
using System.Collections.Generic;

namespace ScoreDemo
{
    class ScoreUI : Scene, InputListener
    {
        private enum UIState
        {
            MainMenu,
            InputName,
            SelectGame,
            DisplayScores,
            AddScore
        }

        private UIState currentState;
        private string inputBuffer;
        private int selectedIndex;
        private ScoreManager scoreManager;
        private List<string> menuItems;
        private List<string> availableGames;
        private int gameSelectedIndex;

        public override void initialize()
        {
            Bootstrap.getInput().addListener(this);
            scoreManager = ScoreManager.getInstance();
            currentState = UIState.MainMenu;
            inputBuffer = "";
            selectedIndex = 0;
            gameSelectedIndex = 0;

            menuItems = new List<string>
            {
                "Enter Name",
                "Select Game",
                "Add Score (Demo)",
                "Show High Scores",
                "Clear Scores",
                "Back to Game"
            };

            availableGames = new List<string> { "(Global)", "GameTest", "GameMissileCommand", "GameSpaceInvaders", "GameBreakout", "GameManicMiner" };
        }

        public override void update()
        {
            Display disp = Bootstrap.getDisplay();
            int cx = disp.getWidth() / 2;
            int cy = disp.getHeight() / 2;

            switch (currentState)
            {
                case UIState.MainMenu:
                    DrawMainMenu(disp, cx, cy);
                    break;
                case UIState.InputName:
                    DrawInputName(disp, cx, cy);
                    break;
                case UIState.SelectGame:
                    DrawSelectGame(disp, cx, cy);
                    break;
                case UIState.DisplayScores:
                    DrawScores(disp, cx, cy);
                    break;
                case UIState.AddScore:
                    DrawAddScore(disp, cx, cy);
                    break;
            }
        }

        private void DrawMainMenu(Display disp, int cx, int cy)
        {
            disp.showText("=== SCORE SYSTEM DEMO ===", cx - 150, cy - 180, 28, 255, 255, 255);
            disp.showText($"Player: {scoreManager.GetPlayerName()}", cx - 80, cy - 130, 18, 200, 200, 200);
            disp.showText($"Game: {GetCurrentGameDisplay()}", cx - 80, cy - 100, 18, 200, 200, 200);

            int startY = cy - 40;
            int spacing = 40;

            for (int i = 0; i < menuItems.Count; i++)
            {
                string prefix = (i == selectedIndex) ? "> " : "  ";
                int r = (i == selectedIndex) ? 255 : 180;
                int g = (i == selectedIndex) ? 215 : 180;
                int b = (i == selectedIndex) ? 100 : 180;

                disp.showText(prefix + menuItems[i], cx - 100, startY + (i * spacing), 20, r, g, b);
            }

            disp.showText("UP/DOWN: Navigate | ENTER: Select", cx - 180, cy + 220, 14, 120, 120, 120);
        }

        private string GetCurrentGameDisplay()
        {
            string game = scoreManager.GetCurrentGame();
            return string.IsNullOrWhiteSpace(game) ? "(Global)" : game;
        }

        private void DrawInputName(Display disp, int cx, int cy)
        {
            disp.showText("=== ENTER NAME ===", cx - 100, cy - 100, 28, 255, 255, 255);
            disp.showText($"Current: {inputBuffer}_", cx - 80, cy, 24, 255, 255, 255);
            disp.showText("Press ENTER to confirm", cx - 100, cy + 50, 16, 200, 200, 200);
            disp.showText("Press ESC to go back", cx - 90, cy + 80, 14, 150, 150, 150);
        }

        private void DrawSelectGame(Display disp, int cx, int cy)
        {
            disp.showText("=== SELECT GAME ===", cx - 120, cy - 140, 28, 255, 255, 255);
            disp.showText("Scores will be isolated per game", cx - 130, cy - 100, 16, 180, 180, 180);

            int startY = cy - 40;
            int spacing = 36;

            for (int i = 0; i < availableGames.Count; i++)
            {
                string prefix = (i == gameSelectedIndex) ? "> " : "  ";
                int r = (i == gameSelectedIndex) ? 255 : 180;
                int g = (i == gameSelectedIndex) ? 215 : 180;
                int b = (i == gameSelectedIndex) ? 100 : 180;

                disp.showText(prefix + availableGames[i], cx - 80, startY + (i * spacing), 20, r, g, b);
            }

            disp.showText("Press ENTER to select, ESC to go back", cx - 160, cy + 180, 14, 150, 150, 150);
        }

        private void DrawScores(Display disp, int cx, int cy)
        {
            string currentGame = scoreManager.GetCurrentGame();
            string title = string.IsNullOrWhiteSpace(currentGame)
                ? "=== GLOBAL HIGH SCORES ==="
                : $"=== HIGH SCORES: {currentGame} ===";

            disp.showText(title, cx - 160, cy - 180, 24, 255, 255, 255);

            List<ScoreEntry> scores = string.IsNullOrWhiteSpace(currentGame)
                ? scoreManager.GetAllScores()
                : scoreManager.GetScoresForGame(currentGame);

            if (scores.Count == 0)
            {
                disp.showText("No scores yet!", cx - 60, cy - 50, 20, 200, 200, 200);
            }
            else
            {
                int startY = cy - 120;
                int maxDisplay = System.Math.Min(scores.Count, 10);

                for (int i = 0; i < maxDisplay; i++)
                {
                    string entry = $"{i + 1}. {scores[i].PlayerName}: {scores[i].Score}";
                    int r = (i == 0) ? 255 : 200;
                    int g = (i == 0) ? 215 : 200;
                    int b = (i == 0) ? 100 : 200;

                    disp.showText(entry, cx - 100, startY + (i * 30), 18, r, g, b);
                }
            }

            disp.showText("Press ESC to go back", cx - 90, cy + 180, 14, 150, 150, 150);
        }

        private void DrawAddScore(Display disp, int cx, int cy)
        {
            disp.showText("=== ADD SCORE (DEMO) ===", cx - 130, cy - 80, 28, 255, 255, 255);
            disp.showText("Score added!", cx - 60, cy, 24, 100, 255, 100);
            disp.showText($"Player: {scoreManager.GetPlayerName()}", cx - 70, cy + 40, 18, 200, 200, 200);
            disp.showText($"Game: {GetCurrentGameDisplay()}", cx - 70, cy + 70, 18, 200, 200, 200);
            disp.showText("Press any key to continue...", cx - 120, cy + 110, 14, 150, 150, 150);
        }

        public void handleInput(InputEvent inp, string eventType)
        {
            if (currentState == UIState.AddScore)
            {
                if (eventType == "KeyUp")
                {
                    currentState = UIState.MainMenu;
                }
                return;
            }

            if (currentState == UIState.InputName)
            {
                HandleNameInput(inp, eventType);
                return;
            }

            if (currentState == UIState.SelectGame)
            {
                HandleGameSelectionInput(inp, eventType);
                return;
            }

            if (currentState == UIState.DisplayScores)
            {
                if (eventType == "KeyUp" && inp.Key == (int)SDL_Scancode.SDL_SCANCODE_ESCAPE)
                {
                    currentState = UIState.MainMenu;
                }
                return;
            }

            if (currentState == UIState.MainMenu)
            {
                HandleMenuInput(inp, eventType);
            }
        }

        private void HandleMenuInput(InputEvent inp, string eventType)
        {
            if (eventType != "KeyUp")
                return;

            switch (inp.Key)
            {
                case (int)SDL_Scancode.SDL_SCANCODE_UP:
                    selectedIndex = (selectedIndex - 1 + menuItems.Count) % menuItems.Count;
                    break;
                case (int)SDL_Scancode.SDL_SCANCODE_DOWN:
                    selectedIndex = (selectedIndex + 1) % menuItems.Count;
                    break;
                case (int)SDL_Scancode.SDL_SCANCODE_RETURN:
                case (int)SDL_Scancode.SDL_SCANCODE_KP_ENTER:
                    ExecuteMenuAction();
                    break;
            }
        }

        private void ExecuteMenuAction()
        {
            switch (selectedIndex)
            {
                case 0:
                    inputBuffer = scoreManager.GetPlayerName();
                    currentState = UIState.InputName;
                    break;
                case 1:
                    gameSelectedIndex = FindCurrentGameIndex();
                    currentState = UIState.SelectGame;
                    break;
                case 2:
                    int demoScore = new System.Random().Next(100, 1000);
                    scoreManager.AddScore(demoScore);
                    currentState = UIState.AddScore;
                    break;
                case 3:
                    currentState = UIState.DisplayScores;
                    break;
                case 4:
                    string currentGame = scoreManager.GetCurrentGame();
                    if (string.IsNullOrWhiteSpace(currentGame))
                    {
                        scoreManager.ClearScores();
                    }
                    else
                    {
                        scoreManager.ClearScoresForGame(currentGame);
                    }
                    break;
                case 5:
                    onExit();
                    break;
            }
        }

        private int FindCurrentGameIndex()
        {
            string currentGame = scoreManager.GetCurrentGame();
            for (int i = 0; i < availableGames.Count; i++)
            {
                if (availableGames[i].Equals(currentGame, StringComparison.OrdinalIgnoreCase) ||
                    (string.IsNullOrWhiteSpace(currentGame) && availableGames[i] == "(Global)"))
                {
                    return i;
                }
            }
            return 0;
        }

        private void HandleGameSelectionInput(InputEvent inp, string eventType)
        {
            if (eventType != "KeyUp")
                return;

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_ESCAPE)
            {
                currentState = UIState.MainMenu;
                return;
            }

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_RETURN ||
                inp.Key == (int)SDL_Scancode.SDL_SCANCODE_KP_ENTER)
            {
                string selectedGame = availableGames[gameSelectedIndex];
                if (selectedGame == "(Global)")
                {
                    scoreManager.SetCurrentGame("");
                }
                else
                {
                    scoreManager.SetCurrentGame(selectedGame);
                }
                currentState = UIState.MainMenu;
                return;
            }

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_UP)
            {
                gameSelectedIndex = (gameSelectedIndex - 1 + availableGames.Count) % availableGames.Count;
            }
            else if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_DOWN)
            {
                gameSelectedIndex = (gameSelectedIndex + 1) % availableGames.Count;
            }
        }

        private void HandleNameInput(InputEvent inp, string eventType)
        {
            if (eventType != "KeyUp")
                return;

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_ESCAPE)
            {
                currentState = UIState.MainMenu;
                return;
            }

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_RETURN ||
                inp.Key == (int)SDL_Scancode.SDL_SCANCODE_KP_ENTER)
            {
                scoreManager.SetPlayerName(inputBuffer);
                currentState = UIState.MainMenu;
                return;
            }

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_BACKSPACE)
            {
                if (inputBuffer.Length > 0)
                {
                    inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1);
                }
                return;
            }

            if (inp.Key >= (int)SDL_Scancode.SDL_SCANCODE_A && inp.Key <= (int)SDL_Scancode.SDL_SCANCODE_Z)
            {
                char c = (char)('a' + (inp.Key - (int)SDL_Scancode.SDL_SCANCODE_A));
                if (inputBuffer.Length < 20)
                {
                    inputBuffer += c;
                }
            }
        }

        public override void onExit()
        {
            Bootstrap.getInput().removeListener(this);
            Bootstrap.returnToLauncher();
        }
    }
}
