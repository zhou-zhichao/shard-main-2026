using Shard;
using System;
using System.Drawing;

namespace GameTest
{
    static class DemoAtlas
    {
        public static void Apply(Transform transform, string texture, int frameWidth, int frameHeight, int column, int row, float scale)
        {
            transform.SpritePath = Bootstrap.getAssetManager().getAssetPath(texture);
            transform.SetSpriteSourceRect(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
            transform.Wid = frameWidth;
            transform.Ht = frameHeight;
            transform.Scalex = scale;
            transform.Scaley = scale;
        }

        public static void Apply(Transform transform, DemoTilePlacement placement)
        {
            transform.SpritePath = Bootstrap.getAssetManager().getAssetPath(placement.Texture);

            int srcX, srcY, srcW, srcH;
            if (placement.SourceWidth > 0 && placement.SourceHeight > 0)
            {
                srcX = placement.SourceX;
                srcY = placement.SourceY;
                srcW = placement.SourceWidth;
                srcH = placement.SourceHeight;
            }
            else
            {
                srcX = placement.Column * placement.FrameWidth;
                srcY = placement.Row * placement.FrameHeight;
                srcW = placement.FrameWidth;
                srcH = placement.FrameHeight;
            }

            transform.SetSpriteSourceRect(srcX, srcY, srcW, srcH);
            transform.Wid = srcW;
            transform.Ht = srcH;

            transform.Scalex = placement.Scale;
            transform.Scaley = placement.Scale;
        }
    }

    class DemoTileSprite : GameObject
    {
        private bool configured;

        public void Configure(DemoTilePlacement placement)
        {
            configured = true;
            Transform.X = placement.X;
            Transform.Y = placement.Y;
            DemoAtlas.Apply(Transform, placement);
        }

        public override void update()
        {
            if (configured)
            {
                Bootstrap.getDisplay().addToDraw(this);
            }
        }
    }

    class DemoPickup : GameObject
    {
        private int value;
        private float boundsWidth;
        private float boundsHeight;
        private float scale;
        private bool configured;
        private bool collected;

        public int Value { get => value; }
        public bool Collected { get => collected; }
        public RectangleF Bounds
        {
            get
            {
                return new RectangleF(Transform.X + 6, Transform.Y + 6, boundsWidth, boundsHeight);
            }
        }

        public void Configure(DemoPickupPlacement placement)
        {
            configured = true;
            collected = false;
            value = placement.Value;
            boundsWidth = placement.Width;
            boundsHeight = placement.Height;
            scale = placement.Scale;

            Transform.X = placement.X;
            Transform.Y = placement.Y;
            Transform.Scalex = scale;
            Transform.Scaley = scale;
            Animator.Play(placement.ClipId, true);
        }

        public bool TryCollect(RectangleF playerBounds)
        {
            if (!configured || collected)
            {
                return false;
            }

            if (!Bounds.IntersectsWith(playerBounds))
            {
                return false;
            }

            collected = true;
            ToBeDestroyed = true;
            return true;
        }

        public override void update()
        {
            if (configured && !collected)
            {
                Bootstrap.getDisplay().addToDraw(this);
            }
        }
    }

    class DemoSlime : GameObject
    {
        private const float gravity = 1450.0f;
        private const float maxSimulationStep = 1.0f / 30.0f;

        private DemoLevelScene scene;
        private float minX;
        private float maxX;
        private float speed;
        private int direction;
        private float velocityY;
        private float boundsOffsetX;
        private float boundsOffsetY;
        private float boundsWidth;
        private float boundsHeight;
        private bool grounded;
        private bool configured;

        public RectangleF Bounds
        {
            get
            {
                return new RectangleF(Transform.X + boundsOffsetX, Transform.Y + boundsOffsetY, boundsWidth, boundsHeight);
            }
        }

        public float CenterX
        {
            get
            {
                RectangleF bounds = Bounds;
                return bounds.X + (bounds.Width / 2.0f);
            }
        }

        public void Configure(DemoLevelScene ownerScene, DemoEnemyPlacement placement)
        {
            configured = true;
            scene = ownerScene;
            minX = placement.MinX;
            maxX = placement.MaxX;
            speed = placement.Speed;
            direction = 1;
            velocityY = 0;
            grounded = false;
            float drawWidth = 24.0f * placement.Scale;
            float drawHeight = 24.0f * placement.Scale;
            boundsOffsetX = drawWidth * 0.2f;
            boundsOffsetY = drawHeight * 0.4f;
            boundsWidth = drawWidth * 0.6f;
            boundsHeight = drawHeight - boundsOffsetY - 2.0f;

            Transform.X = placement.X;
            Transform.Y = placement.Y;
            Transform.Scalex = placement.Scale;
            Transform.Scaley = placement.Scale;
            Animator.Play("demo.slime.move", true);
        }

        public override void update()
        {
            if (!configured || scene == null)
            {
                return;
            }

            float deltaTime = Math.Min((float)Bootstrap.getDeltaTime(), maxSimulationStep);
            float supportProbeOffset = Math.Max(8.0f, speed * deltaTime);

            if (grounded && !hasSupportAhead(direction, supportProbeOffset))
            {
                direction *= -1;
            }

            moveHorizontally(speed * direction * deltaTime);
            velocityY += gravity * deltaTime;
            moveVertically(velocityY * deltaTime);
            Bootstrap.getDisplay().addToDraw(this);
        }

        private void moveHorizontally(float deltaX)
        {
            Transform.X += deltaX;
            RectangleF bounds = Bounds;

            foreach (RectangleF solid in scene.GetSolidRects())
            {
                if (!bounds.IntersectsWith(solid))
                {
                    continue;
                }

                if (deltaX > 0)
                {
                    Transform.X = solid.Left - boundsWidth - boundsOffsetX;
                    direction = -1;
                }
                else if (deltaX < 0)
                {
                    Transform.X = solid.Right - boundsOffsetX;
                    direction = 1;
                }

                bounds = Bounds;
            }

            if (Transform.X <= minX)
            {
                Transform.X = minX;
                direction = 1;
            }
            else if (Transform.X >= maxX)
            {
                Transform.X = maxX;
                direction = -1;
            }
        }

        private void moveVertically(float deltaY)
        {
            grounded = false;
            Transform.Y += deltaY;
            RectangleF bounds = Bounds;

            foreach (RectangleF solid in scene.GetSolidRects())
            {
                if (!bounds.IntersectsWith(solid))
                {
                    continue;
                }

                if (deltaY >= 0)
                {
                    Transform.Y = solid.Top - boundsHeight - boundsOffsetY;
                    grounded = true;
                }
                else
                {
                    Transform.Y = solid.Bottom - boundsOffsetY;
                }

                velocityY = 0;
                bounds = Bounds;
            }
        }

        private bool hasSupportAhead(int moveDirection, float probeOffset)
        {
            RectangleF bounds = Bounds;
            float probeX = moveDirection > 0 ? bounds.Right + probeOffset : bounds.Left - probeOffset;
            float probeY = bounds.Bottom + 4;

            foreach (RectangleF solid in scene.GetSolidRects())
            {
                if (probeX >= solid.Left && probeX <= solid.Right && probeY >= solid.Top && probeY <= solid.Bottom + 4)
                {
                    return true;
                }
            }

            return false;
        }
    }

    class DemoExitPortal : GameObject
    {
        private RectangleF bounds;
        private bool configured;
        private bool unlocked;

        public RectangleF Bounds { get => bounds; }
        public bool Unlocked { get => unlocked; }

        public void Configure(DemoTilePlacement visual, RectangleF exitBounds)
        {
            configured = true;
            bounds = exitBounds;
            Transform.X = visual.X;
            Transform.Y = visual.Y;
            DemoAtlas.Apply(Transform, visual);
        }

        public void SetUnlocked(bool value)
        {
            unlocked = value;
        }

        public override void update()
        {
            if (configured)
            {
                Bootstrap.getDisplay().addToDraw(this);
            }
        }
    }

    class DemoPlayer : GameObject
    {
        private const float moveSpeed = 260.0f;
        private const float gravity = 1450.0f;
        private const float jumpVelocity = 680.0f;
        private const float playerScale = 2.4f;
        private const float spawnInvulnerabilityDuration = 0.75f;
        private const float maxSimulationStep = 1.0f / 30.0f;

        private DemoLevelScene scene;
        private bool moveLeft;
        private bool moveRight;
        private bool jumpRequested;
        private float velocityX;
        private float velocityY;
        private bool grounded;
        private float invulnerabilityTimer;
        private float hurtAnimationTimer;
        private float boundsOffsetX;
        private float boundsOffsetY;
        private float boundsWidth;
        private float boundsHeight;
        private bool facingLeft;
        private bool configured;

        public RectangleF Bounds
        {
            get
            {
                return new RectangleF(Transform.X + boundsOffsetX, Transform.Y + boundsOffsetY, boundsWidth, boundsHeight);
            }
        }

        public void Configure(DemoLevelScene ownerScene, float x, float y)
        {
            configured = true;
            scene = ownerScene;
            Transform.X = x;
            Transform.Y = y;
            Transform.Scalex = playerScale;
            Transform.Scaley = playerScale;
            float drawSize = 32.0f * playerScale;
            boundsOffsetX = drawSize * 0.15625f;
            boundsOffsetY = drawSize * 0.09375f;
            boundsWidth = drawSize * 0.65625f;
            boundsHeight = drawSize * 0.90625f;
            grounded = false;
            invulnerabilityTimer = spawnInvulnerabilityDuration;
            hurtAnimationTimer = 0;
            velocityX = 0;
            velocityY = 0;
            facingLeft = false;
            Transform.FlipX = false;
            Animator.Play("demo.knight.idle", true);
        }

        public void SetMoveLeft(bool enabled)
        {
            moveLeft = enabled;
        }

        public void SetMoveRight(bool enabled)
        {
            moveRight = enabled;
        }

        public void QueueJump()
        {
            jumpRequested = true;
        }

        public void ClearInput()
        {
            moveLeft = false;
            moveRight = false;
            jumpRequested = false;
            velocityX = 0;
        }

        public override void update()
        {
            if (!configured || scene == null)
            {
                return;
            }

            float deltaTime = Math.Min((float)Bootstrap.getDeltaTime(), maxSimulationStep);

            if (invulnerabilityTimer > 0)
            {
                invulnerabilityTimer -= deltaTime;
            }

            if (hurtAnimationTimer > 0)
            {
                hurtAnimationTimer -= deltaTime;
            }

            int moveDirection = 0;
            if (moveLeft)
            {
                moveDirection -= 1;
            }
            if (moveRight)
            {
                moveDirection += 1;
            }

            if (moveDirection < 0)
            {
                facingLeft = true;
            }
            else if (moveDirection > 0)
            {
                facingLeft = false;
            }

            Transform.FlipX = facingLeft;

            velocityX = moveDirection * moveSpeed;

            if (jumpRequested && grounded)
            {
                velocityY = -jumpVelocity;
                grounded = false;
                Bootstrap.getSound().playSound("jump.wav");
            }

            jumpRequested = false;
            velocityY += gravity * deltaTime;

            moveHorizontally(velocityX * deltaTime);
            moveVertically(velocityY * deltaTime);

            if (Transform.Y > Bootstrap.getDisplay().getDesignHeight() + 160)
            {
                scene.FailLevel();
                return;
            }

            scene.CheckPickupCollisions(this);
            scene.CheckEnemyCollisions(this);
            scene.CheckExitCollision(this);

            updateAnimation();
            Bootstrap.getDisplay().addToDraw(this);
        }

        public void TakeHit(float sourceX)
        {
            if (invulnerabilityTimer > 0 || scene == null)
            {
                return;
            }

            bool defeated = DemoRunState.TakeDamage(1, 50);
            invulnerabilityTimer = 1.0f;
            hurtAnimationTimer = 0.35f;
            velocityY = -220.0f;
            velocityX = Bounds.X + (Bounds.Width / 2.0f) < sourceX ? -180.0f : 180.0f;
            Bootstrap.getSound().playSound("hurt.wav");

            if (defeated)
            {
                scene.FailLevel();
            }
        }

        private void moveHorizontally(float deltaX)
        {
            Transform.X += deltaX;
            RectangleF bounds = Bounds;

            foreach (RectangleF solid in scene.GetSolidRects())
            {
                if (!bounds.IntersectsWith(solid))
                {
                    continue;
                }

                if (deltaX > 0)
                {
                    Transform.X = solid.Left - boundsWidth - boundsOffsetX;
                }
                else if (deltaX < 0)
                {
                    Transform.X = solid.Right - boundsOffsetX;
                }

                bounds = Bounds;
            }
        }

        private void moveVertically(float deltaY)
        {
            grounded = false;
            Transform.Y += deltaY;
            RectangleF bounds = Bounds;

            foreach (RectangleF solid in scene.GetSolidRects())
            {
                if (!bounds.IntersectsWith(solid))
                {
                    continue;
                }

                if (deltaY >= 0)
                {
                    Transform.Y = solid.Top - boundsHeight - boundsOffsetY;
                    grounded = true;
                }
                else
                {
                    Transform.Y = solid.Bottom - boundsOffsetY;
                }

                velocityY = 0;
                bounds = Bounds;
            }
        }

        private void updateAnimation()
        {
            if (hurtAnimationTimer > 0)
            {
                Animator.Play("demo.knight.hurt");
                return;
            }

            if (!grounded)
            {
                Animator.Play("demo.knight.jump");
                return;
            }

            if (Math.Abs(velocityX) > 1.0f)
            {
                Animator.Play("demo.knight.run");
                return;
            }

            Animator.Play("demo.knight.idle");
        }
    }
}
