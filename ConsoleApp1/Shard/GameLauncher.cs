/*
*   GameLauncher - A visual game launcher with main menu, settings, and game selection.
*       Uses the UISystem (same pattern as Missile Command) for a polished menu experience.
*       Press ESC during a game to return to this launcher.
*/

using System;
using System.Drawing;
using ScoreDemo;

namespace Shard
{
    class GameLauncher : Game, InputListener
    {
        private enum LauncherState
        {
            MainMenu,
            Settings,
            GameSelect
        }

        private UISystem uiSystem;
        private LauncherState state;
        private DemoEnemy menuEnemy;

        // Remember user's FPS choice so it persists across game launches
        private static int userFpsChoice = 60;

        public override void initialize()
        {
            Bootstrap.getInput().addListener(this);

            uiSystem = new UISystem();
            uiSystem.LoadFromAsset("ui_layouts_launcher.json");

            // Main menu actions
            uiSystem.BindButtonAction("show_games", ShowGameSelect);
            uiSystem.BindButtonAction("open_settings", OpenSettings);
            uiSystem.BindButtonAction("exit_game", ExitGame);
            uiSystem.BindButtonAction("back_main", EnterMainMenu);
            uiSystem.BindButtonAction("show_scores", ShowScoreSystem);

            // Settings actions
            uiSystem.BindDropdownAction("set_frame_limit", ApplyFrameLimit);

            // Game launch actions
            uiSystem.BindButtonAction("launch_GameTest", () => LaunchGame("GameTest"));
            uiSystem.BindButtonAction("launch_GameMissileCommand", () => LaunchGame("GameMissileCommand"));
            uiSystem.BindButtonAction("launch_GameSpaceInvaders", () => LaunchGame("GameSpaceInvaders"));
            uiSystem.BindButtonAction("launch_GameBreakout", () => LaunchGame("GameBreakout"));
            uiSystem.BindButtonAction("launch_GameManicMiner", () => LaunchGame("GameManicMiner"));

            Bootstrap.setTargetFrameRate(userFpsChoice);
            EnterMainMenu();
        }

        public override void update()
        {
            Bootstrap.getDisplay().showText("FPS: " + Bootstrap.getFPS(), 20, 20, 16, 255, 255, 255);
            uiSystem.Render();
        }

        public override int getTargetFrameRate()
        {
            return userFpsChoice;
        }

        public override bool isRunning()
        {
            return true;
        }

        // --- State transitions ---

        private void EnterMainMenu()
        {
            state = LauncherState.MainMenu;
            uiSystem.SetScreen("main_menu");
            CreateMenuEnemy();
        }

        private void OpenSettings()
        {
            DestroyMenuEnemy();
            state = LauncherState.Settings;
            uiSystem.SetScreen("settings");
        }

        private void ShowGameSelect()
        {
            DestroyMenuEnemy();
            state = LauncherState.GameSelect;
            uiSystem.SetScreen("game_select");
        }

        private void ShowScoreSystem()
        {
            DestroyMenuEnemy();
            Bootstrap.getInput().removeListener(this);
            Bootstrap.getSceneManager().loadScene(new ScoreUI());
        }

        private void ExitGame()
        {
            Environment.Exit(0);
        }

        private void ApplyFrameLimit(string selectedOption)
        {
            if (string.IsNullOrWhiteSpace(selectedOption))
            {
                return;
            }

            if (selectedOption.Equals("Unlimited", StringComparison.OrdinalIgnoreCase))
            {
                userFpsChoice = 0;
                Bootstrap.setTargetFrameRate(0);
                return;
            }

            if (Int32.TryParse(selectedOption, out int fps))
            {
                userFpsChoice = fps;
                Bootstrap.setTargetFrameRate(fps);
            }
        }

        // --- Animated demo enemy on main menu ---

        private void CreateMenuEnemy()
        {
            if (menuEnemy != null && menuEnemy.ToBeDestroyed == false)
            {
                return;
            }
            menuEnemy = new DemoEnemy();
        }

        private void DestroyMenuEnemy()
        {
            if (menuEnemy == null) return;
            if (menuEnemy.ToBeDestroyed == false)
            {
                menuEnemy.ToBeDestroyed = true;
            }
            menuEnemy = null;
        }

        // --- Game launching ---

        private void LaunchGame(string className)
        {
            DestroyMenuEnemy();

            // Clean up launcher
            Bootstrap.getInput().removeListener(this);
            GameObjectManager.getInstance().clearAll();
            PhysicsManager.getInstance().clearAll();
            Bootstrap.getInput().clearListeners();
            Bootstrap.getDisplay().clearDisplay();

            // Create and switch to the selected game
            Type t = Type.GetType("Shard." + className);
            if (t == null)
            {
                Debug.getInstance().log("Failed to find game class: " + className, Debug.DEBUG_LEVEL_ERROR);
                Bootstrap.getInput().addListener(this);
                return;
            }

            Game newGame = (Game)Activator.CreateInstance(t);
            Bootstrap.setRunningGame(newGame);
            // Use the user's FPS choice, not the game's default
            Bootstrap.setTargetFrameRate(userFpsChoice);
            newGame.initialize();
        }

        // --- Input ---

        public void handleInput(InputEvent inp, string eventType)
        {
            uiSystem.HandleInput(inp, eventType);
        }
    }
}
