namespace Shard
{
    class GameAnimationDemo : Game
    {
        public override int getTargetFrameRate()
        {
            return 120;
        }

        public override void initialize()
        {
            new DemoEnemy();
        }

        public override void update()
        {
            Bootstrap.getDisplay().showText("Animation Demo", 20, 20, 20, 255, 255, 255);
            Bootstrap.getDisplay().showText("FPS: " + Bootstrap.getFPS(), 20, 48, 16, 255, 255, 255);
        }
    }
}
