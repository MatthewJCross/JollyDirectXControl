using Jolly2DEngine.Audio;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;

namespace Jolly2DTronCycles.Game
{
    public sealed class TronCycle
    {
        private const float MaximumMovementStep = 4.0f;

        private readonly JollySprite[] _sprites;

        private TronDirection _currentDirection;
        public TronDirection CurrentDirection => _currentDirection;

        public float X { get; private set; }
        public float Y { get; private set; }
        public float VelocityX { get; private set; }
        public float VelocityY { get; private set; }
        public float Speed { get; set; } = 160.0f;
        public float BoostSpeed { get; set; } = 240.0f;
        public float SlowSpeed { get; set; } = 100.0f;
        public bool BoostAvailable { get; set; } = true;
        public bool Boosting { get; private set; }
        public bool Slowing { get; private set; }
        public bool IsDead { get; private set; }
        public JollySprite Sprite => _sprites[(int)_currentDirection];

        public IReadOnlyList<JollySprite> Sprites => _sprites;

        public TronTrail Trail { get; }

        public TronCycle(DirectXJollyRenderer renderer, float x, float y, JollyTexture[] textures, JollyTexture trailTexture, TronDirection initialDirection)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(textures);
            ArgumentNullException.ThrowIfNull(trailTexture);

            if (textures.Length != 4)
            {
                throw new ArgumentException("A Tron cycle requires four textures.", nameof(textures));
            }

            if ((int)initialDirection < 0 || (int)initialDirection > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDirection));
            }

            X = x;
            Y = y;
            VelocityX = 0.0f;
            VelocityY = 0.0f;

            float trailX = x;
            float trailY = y;

            switch (initialDirection)
            {
                case TronDirection.Up:
                    trailY += textures[0].Height * 0.5f;
                    break;

                case TronDirection.Down:
                    trailY -= textures[0].Height * 0.5f;
                    break;

                case TronDirection.Left:
                    trailX += textures[0].Width * 0.5f;
                    break;

                case TronDirection.Right:
                    trailX -= textures[0].Width * 0.5f;
                    break;
            }

            Trail = new TronTrail(renderer, trailTexture);
            _sprites = new JollySprite[4];

            for (int i = 0; i < 4; i++)
            {
                _sprites[i] = renderer.CreateSprite(textures[i]);
                _sprites[i].Name = "TronCycle";
                _sprites[i].Position = new JollyVector2(X, Y);
                _sprites[i].Size = new JollyVector2(textures[i].Width, textures[i].Height);
                _sprites[i].Rotation = 0.0f;
                _sprites[i].Opacity = 1.0f;
                _sprites[i].Visible = i == (int)_currentDirection;
                _sprites[i].Layer = 100;
                _sprites[i].FlipHorizontal = false;
                _sprites[i].FlipVertical = false;
            }

            _currentDirection = initialDirection;
            ApplyCurrentSpeed();

            for (int i = 0; i < _sprites.Length; i++)
            {
                _sprites[i].Visible = i == (int)_currentDirection;
            }
        }

        public void Reset(float x, float y, TronDirection direction)
        {
            if ((int)direction < 0 || (int)direction > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(direction));
            }

            X = x;
            Y = y;
            _currentDirection = direction;
            VelocityX = 0.0f;
            VelocityY = 0.0f;
            Boosting = false;
            Slowing = false;
            IsDead = false;
            Trail.Clear();

            for (int i = 0; i < _sprites.Length; i++)
            {
                _sprites[i].Position = new JollyVector2(X, Y);
                _sprites[i].Visible = i == (int)_currentDirection;
            }

            ApplyCurrentSpeed();
        }

        public void Deactivate()
        {
            IsDead = true;
            VelocityX = 0.0f;
            VelocityY = 0.0f;
            Boosting = false;
            Slowing = false;
            Trail.Clear();

            foreach (JollySprite sprite in _sprites)
            {
                sprite.Visible = false;
            }
        }

        public void SetDirection(TronDirection direction)
        {
            if ((_currentDirection == TronDirection.Up && direction == TronDirection.Down) || (_currentDirection == TronDirection.Down && direction == TronDirection.Up) || (_currentDirection == TronDirection.Left && direction == TronDirection.Right) || (_currentDirection == TronDirection.Right && direction == TronDirection.Left))
            {
                return;
            }

            _currentDirection = direction;

            ApplyCurrentSpeed();

            for (int i = 0; i < _sprites.Length; i++)
            {
                _sprites[i].Visible = i == (int)_currentDirection;
            }
        }

        public void SetVelocity(float velocityX, float velocityY)
        {
            if (velocityX == 0.0f && velocityY == 0.0f)
            {
                return;
            }

            float currentSpeed = GetCurrentSpeed();

            if (MathF.Abs(velocityX) >= MathF.Abs(velocityY))
            {
                VelocityX = MathF.Sign(velocityX) * currentSpeed;
                VelocityY = 0.0f;

                if (velocityX > 0.0f)
                {
                    SetDirection(TronDirection.Right);
                }
                else
                {
                    SetDirection(TronDirection.Left);
                }
            }
            else
            {
                VelocityX = 0.0f;
                VelocityY = MathF.Sign(velocityY) * currentSpeed;

                if (velocityY > 0.0f)
                {
                    SetDirection(TronDirection.Down);
                }
                else
                {
                    SetDirection(TronDirection.Up);
                }
            }
        }

        public void SetBoost(bool boosting)
        {
            Boosting = boosting && BoostAvailable;

            if (Boosting)
            {
                Slowing = false;
            }

            ApplyCurrentSpeed();
        }

        public void SetSlow(bool slowing)
        {
            Slowing = slowing;

            if (Slowing)
            {
                Boosting = false;
            }

            ApplyCurrentSpeed();
        }

        public void Update(float deltaTime)
        {
            if (IsDead || deltaTime <= 0.0f)
            {
                return;
            }

            float distanceX = VelocityX * deltaTime;
            float distanceY = VelocityY * deltaTime;
            float distance = MathF.Sqrt(distanceX * distanceX + distanceY * distanceY);
            int steps = Math.Max(1, (int)MathF.Ceiling(distance / MaximumMovementStep));
            float stepX = distanceX / steps;
            float stepY = distanceY / steps;

            for (int i = 0; i < steps; i++)
            {
                X += stepX;
                Y += stepY;
            }

            for (int spriteIndex = 0; spriteIndex < _sprites.Length; spriteIndex++)
            {
                _sprites[spriteIndex].Position = new JollyVector2(X, Y);
            }

            JollySprite currentSprite = _sprites[(int)_currentDirection];

            Trail.Update(X, Y, _currentDirection, currentSprite.Size.X, currentSprite.Size.Y);
        }

        public bool IsCollidingWithTrail(TronTrail trail)
        {
            float collisionRadius = MathF.Min(Sprite.Size.X, Sprite.Size.Y) * 0.2f;

            float headX = X + MathF.Sign(VelocityX) * Sprite.Size.X * 0.5f;
            float headY = Y + MathF.Sign(VelocityY) * Sprite.Size.Y * 0.5f;

            if (VelocityX == 0.0f)
            {
                headX = X;
            }

            if (VelocityY == 0.0f)
            {
                headY = Y;
            }

            return trail.IsColliding(headX, headY, collisionRadius);
        }

        public void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            VelocityX = 0.0f;
            VelocityY = 0.0f;
            Boosting = false;
            Slowing = false;
        }

        public bool IsCollidingWithCycle(TronCycle other)
        {
            ArgumentNullException.ThrowIfNull(other);

            float thisRadius = MathF.Min(Sprite.Size.X, Sprite.Size.Y) * 0.5f;
            float otherRadius = MathF.Min(other.Sprite.Size.X, other.Sprite.Size.Y) * 0.5f;
            float collisionDistance = thisRadius + otherRadius;

            float dx = X - other.X;
            float dy = Y - other.Y;

            return dx * dx + dy * dy <= collisionDistance * collisionDistance;
        }

        public bool IsCollidingWithTrail(TronTrail trail, bool includeCurrentSegment)
        {
            ArgumentNullException.ThrowIfNull(trail);

            float width = Sprite.Size.X;
            float height = Sprite.Size.Y;

            JollyVector2 head = GetHeadPosition();

            float checkWidth = width * 0.10f;
            float checkHeight = height * 0.10f;

            switch (_currentDirection)
            {
                case TronDirection.Up:
                    head.Y -= height * 0.10f;
                    break;

                case TronDirection.Down:
                    head.Y += height * 0.10f;
                    break;

                case TronDirection.Left:
                    head.X -= width * 0.10f;
                    break;

                case TronDirection.Right:
                    head.X += width * 0.10f;
                    break;
            }

            if (includeCurrentSegment)
            {
                return trail.IsCollidingRectangleIncludingCurrent(head.X, head.Y, checkWidth, checkHeight);
            }

            return trail.IsCollidingRectangle(head.X, head.Y, checkWidth, checkHeight);
        }

        public JollyVector2 GetHeadPosition()
        {
            float headX = X;
            float headY = Y;

            switch (_currentDirection)
            {
                case TronDirection.Up:
                    headY -= Sprite.Size.Y * 0.5f;
                    break;

                case TronDirection.Down:
                    headY += Sprite.Size.Y * 0.5f;
                    break;

                case TronDirection.Left:
                    headX -= Sprite.Size.X * 0.5f;
                    break;

                case TronDirection.Right:
                    headX += Sprite.Size.X * 0.5f;
                    break;
            }

            return new JollyVector2(headX, headY);
        }

        private float GetCurrentSpeed()
        {
            if (Boosting && BoostAvailable)
            {
                return BoostSpeed;
            }

            if (Slowing)
            {
                return SlowSpeed;
            }

            return Speed;
        }

        private void ApplyCurrentSpeed()
        {
            float currentSpeed = GetCurrentSpeed();

            switch (_currentDirection)
            {
                case TronDirection.Up:
                    VelocityX = 0.0f;
                    VelocityY = -currentSpeed;
                    break;

                case TronDirection.Down:
                    VelocityX = 0.0f;
                    VelocityY = currentSpeed;
                    break;

                case TronDirection.Left:
                    VelocityX = -currentSpeed;
                    VelocityY = 0.0f;
                    break;

                case TronDirection.Right:
                    VelocityX = currentSpeed;
                    VelocityY = 0.0f;
                    break;
            }
        }
    }
}
