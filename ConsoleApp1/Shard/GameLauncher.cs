/*
*   GameLauncher - Demo-focused launcher for the platform showcase.
*/

using System;
using GameTest;

namespace Shard
{
    class GameLauncher : Game, InputListener
    {
        private UISystem uiSystem;
        private DemoEnemy menuEnemy;

        public override void initialize()
        {
            Bootstrap.getInput().addListener(this);

            uiSystem = new UISystem();
            uiSystem.LoadFromAsset("ui_layouts_launcher.json");
            DemoSettings.Bind(uiSystem);

            uiSystem.BindButtonAction("open_games", OpenGames);
            uiSystem.BindButtonAction("open_settings", OpenSettings);
            uiSystem.BindButtonAction("exit_game", ExitGame);
            uiSystem.BindButtonAction("back_launcher_main", EnterMainMenu);

            // Game launch buttons
            uiSystem.BindButtonAction("start_game_test", () => LaunchGame(new GameTest()));
            uiSystem.BindButtonAction("start_breakout", () => LaunchGame(new GameBreakout()));
            uiSystem.BindButtonAction("start_space_invaders", () => LaunchGame(new GameSpaceInvaders()));
            uiSystem.BindButtonAction("start_manic_miner", () => LaunchGame(new GameManicMiner()));
            uiSystem.BindButtonAction("start_missile_command", () => LaunchGame(new GameMissileCommand()));

            DemoSettings.ApplyCurrentRuntimeValues();
            DemoSettings.SyncCurrentScreen(uiSystem);
            EnterMainMenu();
        }

        public override void update()
        {
            Bootstrap.getDisplay().showText("FPS: " + Bootstrap.getFPS(), 20, 20, 16, 255, 255, 255);
            uiSystem.Render();
        }

        public override int getTargetFrameRate()
        {
            return DemoSettings.GetTargetFrameRate();
        }

        public override bool isRunning()
        {
            return true;
        }

        private void EnterMainMenu()
        {
            uiSystem.SetScreen("launcher_main");
            CreateMenuEnemy();
        }

        private void OpenGames()
        {
            DestroyMenuEnemy();
            uiSystem.SetScreen("launcher_games");
        }

        private void OpenSettings()
        {
            DestroyMenuEnemy();
            uiSystem.SetScreen("launcher_settings");
            DemoSettings.SyncCurrentScreen(uiSystem);
        }

        private void ExitGame()
        {
            Environment.Exit(0);
        }

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

        private void LaunchGame(Game newGame)
        {
            DestroyMenuEnemy();

            Bootstrap.getInput().removeListener(this);
            Bootstrap.getSound().StopMusic();
            Bootstrap.getSound().stopAllSounds();
            GameObjectManager.getInstance().clearAll();
            PhysicsManager.getInstance().clearAll();
            Bootstrap.getInput().clearListeners();
            Bootstrap.getDisplay().clearDisplay();
            Bootstrap.getSceneManager().reset();

            Bootstrap.setRunningGame(newGame);
            Bootstrap.setTargetFrameRate(newGame.getTargetFrameRate());
            newGame.initialize();
        }

        public void handleInput(InputEvent inp, string eventType)
        {
            uiSystem.HandleInput(inp, eventType);
        }
    }
}

