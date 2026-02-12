using System.Collections.Generic;

namespace Shard
{
    class DemoEnemy : GameObject
    {
        private float speed;
        private int direction;
        private float leftBoundary;
        private float rightMargin;

        public override void initialize()
        {
            speed = 180.0f;
            direction = 1;
            leftBoundary = 80.0f;
            rightMargin = 80.0f;

            Transform.X = leftBoundary;
            Transform.Y = 420.0f;

            Animator.RegisterCodeClip(new AnimationClip(
                "demo.enemy.left",
                new List<string> { "enemyleft1.png", "enemyleft2.png", "enemyleft3.png", "enemyleft4.png" },
                8.0f,
                AnimationPlayMode.Loop
            ));

            Animator.RegisterCodeClip(new AnimationClip(
                "demo.enemy.right",
                new List<string> { "enemyright1.png", "enemyright2.png", "enemyright3.png", "enemyright4.png" },
                8.0f,
                AnimationPlayMode.Loop
            ));

            Animator.Play("demo.enemy.right", true);
        }

        public override void update()
        {
            float deltaTime = (float)Bootstrap.getDeltaTime();

            Transform.translate(speed * direction * deltaTime, 0);

            float rightBoundary = Bootstrap.getDisplay().getWidth() - rightMargin - Transform.Wid;

            if (rightBoundary < leftBoundary)
            {
                rightBoundary = leftBoundary;
            }

            if (Transform.X <= leftBoundary)
            {
                Transform.X = leftBoundary;
                if (direction < 0)
                {
                    direction = 1;
                    Animator.Play("demo.enemy.right");
                }
            }
            else if (Transform.X >= rightBoundary)
            {
                Transform.X = rightBoundary;
                if (direction > 0)
                {
                    direction = -1;
                    Animator.Play("demo.enemy.left");
                }
            }

            Bootstrap.getDisplay().addToDraw(this);
        }
    }
}
