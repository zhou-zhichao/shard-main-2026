using SDL;
using Shard;
using System.Collections.Generic;
using System.Drawing;

namespace GameTest
{
    class DemoLevelScene : Scene, InputListener
    {
        private readonly DemoLevelDefinition level;
        private DemoPlayer player;
        private DemoExitPortal exitPortal;
        private UISystem pauseUi;
        private List<DemoPickup> pickups;
        private List<DemoSlime> enemies;
        private float exitHintTimer;
        private bool transitionPending;
        private bool pauseSettingsOpen;

        public DemoLevelScene(DemoLevelDefinition level)
        {
            this.level = level;
        }

        public override void initialize()
        {
            DemoRunState.AdvanceToLevel(level.LevelNumber);
            transitionPending = false;
            exitHintTimer = 0;
            pauseSettingsOpen = false;
            pickups = new List<DemoPickup>();
            enemies = new List<DemoSlime>();

            Bootstrap.getDisplay().setClearColor(level.BackgroundColor);
            Bootstrap.getInput().addListener(this);
            setupPauseUi();
            spawnWorld();
            Bootstrap.getSound().StopMusic();
            Bootstrap.getSound().PlayMusic(level.MusicAsset, true);
        }

        public override void update()
        {
            renderBackground();
            renderHud();

            if (exitHintTimer > 0)
            {
                exitHintTimer -= (float)Bootstrap.getDeltaTime();
            }

            if (pauseUi != null && DemoRunState.Paused)
            {
                Bootstrap.getDisplay().drawFilledRect(0, 0, Bootstrap.getDisplay().getDesignWidth(), Bootstrap.getDisplay().getDesignHeight(), 0, 0, 0, 160);
                pauseUi.Render();
            }
        }

        public override void onExit()
        {
            DemoRunState.SetPaused(false);
            Bootstrap.getDisplay().setClearColor(0, 0, 0, 255);
            Bootstrap.getInput().removeListener(this);
            Bootstrap.getSound().StopMusic();
        }

        public void handleInput(InputEvent inp, string eventType)
        {
            if (DemoRunState.Paused)
            {
                if (eventType == "KeyUp" && inp.Key == (int)SDL_Scancode.SDL_SCANCODE_ESCAPE)
                {
                    if (pauseSettingsOpen)
                    {
                        openPauseMenu();
                    }
                    else
                    {
                        resumeDemo();
                    }
                    return;
                }

                pauseUi.HandleInput(inp, eventType);
                return;
            }

            if (eventType == "KeyDown")
            {
                if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_A)
                {
                    player.SetMoveLeft(true);
                }
                else if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_D)
                {
                    player.SetMoveRight(true);
                }
                else if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_SPACE)
                {
                    player.QueueJump();
                }
            }
            else if (eventType == "KeyUp")
            {
                if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_A)
                {
                    player.SetMoveLeft(false);
                }
                else if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_D)
                {
                    player.SetMoveRight(false);
                }
                else if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_ESCAPE)
                {
                    openPauseMenu();
                }
            }
        }

        public IReadOnlyList<RectangleF> GetSolidRects()
        {
            return level.Solids;
        }

        public void CheckPickupCollisions(DemoPlayer demoPlayer)
        {
            RectangleF playerBounds = demoPlayer.Bounds;
            foreach (DemoPickup pickup in pickups)
            {
                if (pickup.TryCollect(playerBounds))
                {
                    DemoRunState.AddScore(pickup.Value);
                    Bootstrap.getSound().playSound("coin.wav");
                }
            }

            exitPortal.SetUnlocked(GetRemainingCollectibles() == 0);
        }

        public void CheckEnemyCollisions(DemoPlayer demoPlayer)
        {
            RectangleF playerBounds = demoPlayer.Bounds;
            foreach (DemoSlime enemy in enemies)
            {
                if (enemy.Bounds.IntersectsWith(playerBounds))
                {
                    demoPlayer.TakeHit(enemy.CenterX);
                }
            }
        }

        public void CheckExitCollision(DemoPlayer demoPlayer)
        {
            if (!exitPortal.Bounds.IntersectsWith(demoPlayer.Bounds))
            {
                return;
            }

            if (GetRemainingCollectibles() > 0)
            {
                if (exitHintTimer <= 0)
                {
                    Bootstrap.getSound().playSound("tap.wav");
                }

                exitHintTimer = 1.5f;
                return;
            }

            completeLevel();
        }

        public void FailLevel()
        {
            if (transitionPending)
            {
                return;
            }

            transitionPending = true;
            Bootstrap.getSceneManager().loadScene(new DemoResultScene(false));
        }

        private void setupPauseUi()
        {
            pauseUi = new UISystem();
            pauseUi.LoadFromAsset("ui_layouts_launcher.json");
            DemoSettings.Bind(pauseUi);
            pauseUi.BindButtonAction("resume_demo", resumeDemo);
            pauseUi.BindButtonAction("open_pause_settings", openPauseSettings);
            pauseUi.BindButtonAction("return_to_launcher", Bootstrap.returnToLauncher);
            pauseUi.BindButtonAction("back_pause_menu", openPauseMenu);
            openPauseMenu();
            resumeDemo();
        }

        private void openPauseMenu()
        {
            DemoRunState.SetPaused(true);
            pauseSettingsOpen = false;
            player?.ClearInput();
            pauseUi.SetScreen("pause_menu");
        }

        private void openPauseSettings()
        {
            DemoRunState.SetPaused(true);
            pauseSettingsOpen = true;
            pauseUi.SetScreen("pause_settings");
            DemoSettings.SyncCurrentScreen(pauseUi);
        }

        private void resumeDemo()
        {
            pauseSettingsOpen = false;
            DemoRunState.SetPaused(false);
        }

        private void spawnWorld()
        {
            foreach (DemoTilePlacement tile in level.BackgroundTiles)
            {
                DemoTileSprite sprite = new DemoTileSprite();
                sprite.Configure(tile);
            }

            foreach (DemoTilePlacement tile in level.Tiles)
            {
                DemoTileSprite sprite = new DemoTileSprite();
                sprite.Configure(tile);
            }

            foreach (DemoPickupPlacement pickup in level.Pickups)
            {
                DemoPickup collectible = new DemoPickup();
                collectible.Configure(pickup);
                pickups.Add(collectible);
            }

            foreach (DemoEnemyPlacement enemy in level.Enemies)
            {
                DemoSlime slime = new DemoSlime();
                slime.Configure(this, enemy);
                enemies.Add(slime);
            }

            exitPortal = new DemoExitPortal();
            exitPortal.Configure(level.ExitVisual, level.ExitBounds);

            player = new DemoPlayer();
            player.Configure(this, level.PlayerSpawnX, level.PlayerSpawnY);
        }

        private void renderBackground()
        {
            Display display = Bootstrap.getDisplay();
            display.setClearColor(level.BackgroundColor);

            foreach (DemoGroundBand groundBand in level.GroundBands)
            {
                int x = (int)groundBand.Bounds.X;
                int y = (int)groundBand.Bounds.Y;
                int width = (int)groundBand.Bounds.Width;
                int height = (int)groundBand.Bounds.Height;
                int topHeight = groundBand.TopHeight <= height ? groundBand.TopHeight : height;
                int bodyHeight = height - topHeight;
                int shadowHeight = bodyHeight >= 12 ? 12 : bodyHeight;

                display.drawFilledRect(x, y, width, height, groundBand.FillColor.R, groundBand.FillColor.G, groundBand.FillColor.B, 255);
                display.drawFilledRect(x, y, width, topHeight, groundBand.TopColor.R, groundBand.TopColor.G, groundBand.TopColor.B, 255);

                if (shadowHeight > 0)
                {
                    display.drawFilledRect(x, y + height - shadowHeight, width, shadowHeight, groundBand.ShadowColor.R, groundBand.ShadowColor.G, groundBand.ShadowColor.B, 255);
                }
            }
        }

        private void renderHud()
        {
            Display display = Bootstrap.getDisplay();

            display.showText(level.Title, 24, 20, 24, 255, 255, 255);
            display.showText("Score: " + DemoRunState.Score, 24, 54, 18, 245, 230, 140);
            display.showText("HP: " + DemoRunState.Health, 24, 80, 18, 255, 180, 180);
            display.showText("Collectibles Left: " + GetRemainingCollectibles(), 24, 106, 18, 200, 245, 200);
            display.showText("ESC: Pause / Settings", 24, 132, 16, 210, 210, 230);

            if (GetRemainingCollectibles() == 0)
            {
                display.showText("Exit unlocked - reach the portal.", 860, 24, 18, 180, 255, 180);
            }
            else
            {
                display.showText("Collect every shard to unlock the exit.", 760, 24, 18, 255, 255, 255);
            }

            if (exitHintTimer > 0)
            {
                display.showText("The exit is locked until every collectible is picked up.", 300, 180, 20, 255, 215, 120);
            }
        }

        private int GetRemainingCollectibles()
        {
            int remaining = 0;
            foreach (DemoPickup pickup in pickups)
            {
                if (!pickup.Collected)
                {
                    remaining += 1;
                }
            }

            return remaining;
        }

        private void completeLevel()
        {
            if (transitionPending)
            {
                return;
            }

            transitionPending = true;
            DemoRunState.AddScore(500);
            Bootstrap.getSound().playSound("power_up.wav");

            if (level.LevelNumber >= 2)
            {
                Bootstrap.getSceneManager().loadScene(new DemoResultScene(true));
                return;
            }

            Bootstrap.getSceneManager().loadScene(new DemoLevelScene(DemoLevelCatalog.GetLevel(level.LevelNumber + 1)));
        }
    }
}
