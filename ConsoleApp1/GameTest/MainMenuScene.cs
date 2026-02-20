/*
*   MainMenuScene - A simple scene demonstrating scene management.
*       Displays a title and prompt, then transitions to GameplayScene on SPACE.
*/

using SDL;
using Shard;

namespace GameTest
{
    class MainMenuScene : Scene, InputListener
    {
        public override void initialize()
        {
            Bootstrap.getInput().addListener(this);
        }

        public override void update()
        {
            Display disp = Bootstrap.getDisplay();
            int cx = disp.getWidth() / 2;
            int cy = disp.getHeight() / 2;

            disp.showText("SHARD ENGINE", cx - 100, cy - 60, 28, 255, 255, 255);
            disp.showText("Press SPACE to start", cx - 120, cy + 10, 18, 200, 200, 200);
            disp.showText("FPS: " + Bootstrap.getSecondFPS(), 10, 10, 12, 255, 255, 255);
        }

        public override void onExit()
        {
            // Input listeners are cleared automatically by SceneManager,
            // but we override onExit here as a demonstration.
        }

        public void handleInput(InputEvent inp, string eventType)
        {
            if (eventType == "KeyUp")
            {
                if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_SPACE)
                {
                    Bootstrap.getSceneManager().loadScene(new GameplayScene());
                }
            }
        }
    }
}
