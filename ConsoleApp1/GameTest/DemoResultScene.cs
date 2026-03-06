using SDL;
using Shard;
using System.Collections.Generic;
using System.Drawing;

namespace GameTest
{
    class DemoResultScene : Scene, InputListener
    {
        private readonly bool success;
        private List<ScoreEntry> topScores;

        public DemoResultScene(bool success)
        {
            this.success = success;
        }

        public override void initialize()
        {
            Bootstrap.getInput().addListener(this);
            DemoRunState.SetPaused(false);
            Bootstrap.getSound().StopMusic();
            Bootstrap.getSound().stopAllSounds();
            Bootstrap.getSound().playSound(success ? "power_up.wav" : "explosion.wav");
            DemoRunState.SubmitScoreIfNeeded();
            topScores = ScoreManager.getInstance().GetTopScoresForGame(DemoRunState.ScoreGameName, 5);
        }

        public override void update()
        {
            Display display = Bootstrap.getDisplay();
            Color bg = success ? Color.FromArgb(32, 74, 42) : Color.FromArgb(74, 34, 34);
            Color accent = success ? Color.FromArgb(110, 190, 120) : Color.FromArgb(220, 120, 120);

            display.drawFilledRect(0, 0, display.getDesignWidth(), display.getDesignHeight(), bg.R, bg.G, bg.B, 255);
            display.drawFilledRect(0, 0, display.getDesignWidth(), 180, accent.R, accent.G, accent.B, 85);

            display.showText(success ? "Showcase Complete" : "Run Failed", 420, 90, 40, 255, 255, 255);
            display.showText("Final Score: " + DemoRunState.Score, 470, 170, 24, 255, 230, 140);
            display.showText("Top Scores", 520, 260, 28, 255, 255, 255);

            if (topScores.Count == 0)
            {
                display.showText("No scores recorded yet.", 460, 330, 20, 210, 210, 210);
            }
            else
            {
                int y = 320;
                for (int i = 0; i < topScores.Count; i++)
                {
                    ScoreEntry entry = topScores[i];
                    string text = $"{i + 1}. {entry.PlayerName} - {entry.Score}";
                    display.showText(text, 450, y, 20, 255, 255, 255);
                    y += 36;
                }
            }

            display.showText("Press ENTER or SPACE to return to launcher.", 360, 720, 20, 220, 220, 240);
        }

        public override void onExit()
        {
            Bootstrap.getInput().removeListener(this);
        }

        public void handleInput(InputEvent inp, string eventType)
        {
            if (eventType != "KeyUp")
            {
                return;
            }

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_RETURN ||
                inp.Key == (int)SDL_Scancode.SDL_SCANCODE_KP_ENTER ||
                inp.Key == (int)SDL_Scancode.SDL_SCANCODE_SPACE)
            {
                Bootstrap.returnToLauncher();
            }
        }
    }
}
