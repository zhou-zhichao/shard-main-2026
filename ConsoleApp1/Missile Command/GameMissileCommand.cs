using MissileCommand;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shard
{
    class GameMissileCommand : Game, InputListener
    {
        private enum GameState
        {
            Playing,
            GameOver
        }

        private List<Arsenal> myArsenals;
        private List<City> cities;
        private Random rand;
        private double counter;
        private int missileChance = 1;
        private const double legacyReferenceFps = 1000.0;
        private static bool logIncomingSpawnEvents = false;
        private GameState state;

        public override int getTargetFrameRate()
        {
            return 60;
        }

        public override bool isRunning()
        {
            return state != GameState.GameOver;
        }

        public override void update()
        {
            Bootstrap.getDisplay().showText("FPS: " + Bootstrap.getFPS(), 20, 20, 16, 255, 255, 255);

            switch (state)
            {
                case GameState.Playing:
                    runGameplay();
                    return;
                case GameState.GameOver:
                    Color col = Color.FromArgb(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
                    Bootstrap.getDisplay().showText("GAME OVER!", 300, 300, 128, col);
                    return;
            }
        }

        private void runGameplay()
        {
            counter += Bootstrap.getDeltaTime();

            if (hasLivingCities() == false)
            {
                state = GameState.GameOver;
                return;
            }

            if (counter > 0.5f)
            {
                bool shouldSpawn = ShouldSpawnIncomingThisFrame(Bootstrap.getDeltaTime());
                bool fired = false;

                if (shouldSpawn)
                {
                    fired = generateIncoming();
                }

                if (fired)
                {
                    counter = 0;

                    if (logIncomingSpawnEvents)
                    {
                        Debug.getInstance().log(
                            "Incoming missile spawned | t=" + Bootstrap.TimeElapsed.ToString("0.000")
                            + " | fps=" + Bootstrap.getFPS()
                            + " | frameLimit=" + Bootstrap.getTargetFrameRate(),
                            Debug.DEBUG_LEVEL_ALL
                        );
                    }
                }
            }
        }

        private bool ShouldSpawnIncomingThisFrame(double dt)
        {
            if (rand == null || dt <= 0)
            {
                return false;
            }

            double p = (missileChance + 1) / legacyReferenceFps;

            if (p <= 0)
            {
                return false;
            }

            if (p >= 1)
            {
                return true;
            }

            double pDt = 1 - Math.Pow(1 - p, dt * legacyReferenceFps);

            if (pDt < 0)
            {
                pDt = 0;
            }
            else if (pDt > 1)
            {
                pDt = 1;
            }

            return rand.NextDouble() < pDt;
        }

        public override void initialize()
        {
            Bootstrap.getInput().addListener(this);

            counter = 0;
            rand = new Random();
            cities = new List<City>();
            myArsenals = new List<Arsenal>();

            // Go straight into gameplay (menu is handled by GameLauncher)
            createGameplayWorld();
            state = GameState.Playing;
        }

        private bool hasLivingCities()
        {
            if (cities == null)
            {
                return false;
            }

            foreach (City c in cities)
            {
                if (c != null && c.ToBeDestroyed == false)
                {
                    return true;
                }
            }

            return false;
        }

        private void createGameplayWorld()
        {
            int imod = 0;

            cities = new List<City>();
            myArsenals = new List<Arsenal>();

            for (int i = 0; i < 6; i++)
            {
                City c = new City();

                cities.Add(c);

                if (i == 3)
                {
                    imod = 200;
                }

                c.Transform.translate(100 + imod + (i * 140), 750);
            }

            for (int i = 0; i < 3; i++)
            {
                Arsenal a = new Arsenal();
                a.Transform.translate(25 + (i * 550), 700);
                a.resetMissiles();
                myArsenals.Add(a);
            }
        }

        public bool generateIncoming()
        {
            if (cities == null || cities.Count == 0)
            {
                return false;
            }

            List<City> theCities;
            Missile m;
            City target;

            theCities = new List<City>();

            foreach (City c in cities)
            {
                if (c == null || c.ToBeDestroyed == true)
                {
                    continue;
                }

                theCities.Add(c);
            }

            if (theCities.Count == 0)
            {
                return false;
            }

            // generate an incoming missile
            m = new Missile();
            m.Transform.translate(rand.Next(0, Bootstrap.getDisplay().getWidth()), 0);

            m.Originx = (float)m.Transform.X;
            m.Originy = (float)m.Transform.Y;

            target = theCities[rand.Next(0, theCities.Count)];

            m.Targetx = (float)target.Transform.Centre.X;
            m.Targety = (float)target.Transform.Centre.Y;

            // Some of our missiles will split before they explode.
            if (rand.Next(0, 100) < 10)
            {
                m.Mirv = true;
            }

            m.addTag("EnemyMissile");
            m.TargetTag = "City";
            m.Speed = 10;
            m.MyColor = Color.Red;
            m.TheTargets = theCities;

            return true;
        }

        public void handleInput(InputEvent inp, string eventType)
        {

            if (state != GameState.Playing)
            {
                return;
            }

            if (eventType != "MouseDown")
            {
                return;
            }

            if (myArsenals == null || myArsenals.Count == 0)
            {
                return;
            }

            Arsenal a;
            int which = -1;

            if (inp.Button == 3)
            {
                // Right mouse button.
                which = 2;
            }

            if (inp.Button == 2)
            {
                // Middle mouse button.
                which = 1;
            }

            if (inp.Button == 1)
            {
                // Left mouse button.
                which = 0;
            }

            if (which < 0 || which >= myArsenals.Count)
            {
                // Who knows?
                which = rand.Next(0, myArsenals.Count);
            }

            Missile m = new Missile();

            Debug.Log("Pressed button " + inp.Button);
            a = myArsenals[which];

            if (a == null || a.ToBeDestroyed || a.canFireMissile() == false)
            {
                return;
            }

            a.fireMissile();

            m.Originx = (float)a.Transform.Centre.X;
            m.Originy = (float)a.Transform.Centre.Y;

            m.Transform.translate(m.Originx, m.Originy);
            m.Transform.X = m.Originx;
            m.Transform.Y = m.Originy;

            m.Targetx = inp.X;
            m.Targety = inp.Y;

            m.addTag("PlayerMissile");
            m.TargetTag = "EnemyMissile";
            m.Speed = 1000;
            m.MyColor = Color.Green;
        }
    }
}
