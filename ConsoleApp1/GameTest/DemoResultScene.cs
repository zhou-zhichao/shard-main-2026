using SDL;
using Shard;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GameTest
{
    class DemoResultScene : Scene, InputListener
    {
        private readonly bool success;
        private List<ScoreEntry> topScores;
        private string playerName;
        private bool isEnteringName;
        private StringBuilder nameInput;
        private const string DefaultPlayerName = "Player";

        private RectangleF clearButtonBounds;

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

            playerName = DefaultPlayerName;
            isEnteringName = true;
            nameInput = new StringBuilder();

            clearButtonBounds = new RectangleF(800, 260, 120, 40);

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

            if (isEnteringName)
            {
                display.showText("Enter Your Name:", 460, 260, 28, 255, 255, 255);
                display.showText(nameInput.ToString() + (Bootstrap.getCurrentFrame() % 60 < 30 ? "_" : " "), 460, 320, 32, 255, 230, 140);
                display.showText("Press ENTER to confirm", 420, 400, 20, 210, 210, 210);
                display.showText("Press SPACE to skip (default: Player)", 380, 440, 20, 180, 180, 180);
            }
            else
            {
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

                int btnX = (int)clearButtonBounds.X;
                int btnY = (int)clearButtonBounds.Y;
                int btnW = (int)clearButtonBounds.Width;
                int btnH = (int)clearButtonBounds.Height;
                display.drawFilledRect(btnX, btnY, btnW, btnH, 180, 60, 60, 255);
                display.showText("Clear", btnX + 30, btnY + 12, 18, 255, 255, 255);

                display.showText("Press ENTER or SPACE to return to launcher.", 360, 720, 20, 220, 220, 240);
            }
        }

        public override void onExit()
        {
            Bootstrap.getInput().removeListener(this);
        }

        public void handleInput(InputEvent inp, string eventType)
        {
            if (eventType == "MouseUp")
            {
                if (!isEnteringName)
                {
                    int mouseX = inp.X;
                    int mouseY = inp.Y;

                    if (mouseX >= clearButtonBounds.X && mouseX <= clearButtonBounds.X + clearButtonBounds.Width &&
                        mouseY >= clearButtonBounds.Y && mouseY <= clearButtonBounds.Y + clearButtonBounds.Height)
                    {
                        ScoreManager.getInstance().ClearScoresForGame(DemoRunState.ScoreGameName);
                        topScores = ScoreManager.getInstance().GetTopScoresForGame(DemoRunState.ScoreGameName, 5);
                    }
                }
                return;
            }

            if (eventType != "KeyUp")
            {
                return;
            }

            if (isEnteringName)
            {
                handleNameInput(inp);
                return;
            }

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_RETURN ||
                inp.Key == (int)SDL_Scancode.SDL_SCANCODE_KP_ENTER ||
                inp.Key == (int)SDL_Scancode.SDL_SCANCODE_SPACE)
            {
                Bootstrap.returnToLauncher();
            }
        }

        private void handleNameInput(InputEvent inp)
        {
            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_RETURN ||
                inp.Key == (int)SDL_Scancode.SDL_SCANCODE_KP_ENTER)
            {
                submitScore();
                return;
            }

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_SPACE)
            {
                playerName = DefaultPlayerName;
                submitScore();
                return;
            }

            if (inp.Key == (int)SDL_Scancode.SDL_SCANCODE_BACKSPACE)
            {
                if (nameInput.Length > 0)
                {
                    nameInput.Length--;
                }
                return;
            }

            if (inp.Key >= (int)SDL_Scancode.SDL_SCANCODE_A && inp.Key <= (int)SDL_Scancode.SDL_SCANCODE_Z)
            {
                if (nameInput.Length < 16)
                {
                    char c = (char)('a' + (inp.Key - (int)SDL_Scancode.SDL_SCANCODE_A));
                    nameInput.Append(c);
                }
                return;
            }

            if (inp.Key >= (int)SDL_Scancode.SDL_SCANCODE_0 && inp.Key <= (int)SDL_Scancode.SDL_SCANCODE_9)
            {
                if (nameInput.Length < 16)
                {
                    nameInput.Append((char)('0' + (inp.Key - (int)SDL_Scancode.SDL_SCANCODE_0)));
                }
                return;
            }
        }

        private void submitScore()
        {
            if (nameInput.Length > 0)
            {
                playerName = nameInput.ToString();
            }
            else
            {
                playerName = DefaultPlayerName;
            }

            ScoreManager.getInstance().SetPlayerName(playerName);
            DemoRunState.SubmitScoreWithName(playerName);
            isEnteringName = false;
            topScores = ScoreManager.getInstance().GetTopScoresForGame(DemoRunState.ScoreGameName, 5);
        }
    }
}
