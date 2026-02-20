/*
*   GameplayScene - The main gameplay scene, containing the existing GameTest logic
*       (spaceship, asteroids, mouse-click spawning) wrapped as a Scene.
*/

using Shard;
using System;
using System.Collections.Generic;

namespace GameTest
{
    class GameplayScene : Scene, InputListener
    {
        GameObject background;
        List<GameObject> asteroids;

        public override void initialize()
        {
            Bootstrap.getInput().addListener(this);

            // Create spaceship
            GameObject ship = new Spaceship();

            // Create background
            background = new GameObject();
            background.Transform.SpritePath = Bootstrap.getAssetManager().getAssetPath("background2.jpg");
            background.Transform.X = 0;
            background.Transform.Y = 0;

            asteroids = new List<GameObject>();
        }

        public override void update()
        {
            Bootstrap.getDisplay().showText("FPS: " + Bootstrap.getSecondFPS() + " / " + Bootstrap.getFPS(), 10, 10, 12, 255, 255, 255);
            Bootstrap.getDisplay().showText("Press ESC for menu", 10, 30, 12, 180, 180, 180);
            Bootstrap.getDisplay().addToDraw(background);
        }

        public override void onExit()
        {
            // Cleanup is handled automatically by SceneManager.
            // Any custom cleanup can go here.
        }

        public void handleInput(InputEvent inp, string eventType)
        {
            if (eventType == "MouseDown")
            {
                Console.WriteLine("Pressing button " + inp.Button);
            }

            if (eventType == "MouseDown" && inp.Button == 1)
            {
                Asteroid asteroid = new Asteroid();
                asteroid.Transform.X = inp.X;
                asteroid.Transform.Y = inp.Y;
                asteroids.Add(asteroid);
            }

            if (eventType == "MouseDown" && inp.Button == 3)
            {
                foreach (GameObject ast in asteroids)
                {
                    ast.ToBeDestroyed = true;
                }

                asteroids.Clear();
            }

            if (eventType == "KeyUp")
            {
                if (inp.Key == (int)SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE)
                {
                    Bootstrap.getSceneManager().loadScene(new MainMenuScene());
                }
            }
        }
    }
}
