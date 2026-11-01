using Jolly2DEngine.Core;
using Jolly2DEngine.Math;

namespace Jolly2DEngine.Physics
{
    public sealed class JollyCharacterController
    {
        private readonly JollyGameObject _gameObject;

        private float _moveInput;
        private float _coyoteTimer;
        private float _jumpBufferTimer;

        public JollyGameObject GameObject => _gameObject;
        public float MoveAcceleration { get; set; } = 2200.0f;
        public float MaxMoveSpeed { get; set; } = 300.0f;
        public float GroundDeceleration { get; set; } = 2600.0f;
        public float AirDeceleration { get; set; } = 800.0f;
        public float JumpVelocity { get; set; } = 450.0f;
        public float JumpGravity { get; set; } = 980.0f;
        public float FallGravity { get; set; } = 1400.0f;
        public float JumpCutGravity { get; set; } = 1800.0f;
        public float MaxFallSpeed { get; set; } = 900.0f;
        public float CoyoteTime { get; set; } = 0.10f;
        public float JumpBufferTime { get; set; } = 0.10f;
        public bool JumpHeld { get; private set; }

        public bool IsGrounded => _gameObject.IsGrounded;

        public JollyCharacterController(JollyGameObject gameObject)
        {
            ArgumentNullException.ThrowIfNull(gameObject);
            _gameObject = gameObject;
        }

        public void Reset()
        {
            _moveInput = 0.0f;
            _coyoteTimer = 0.0f;
            _jumpBufferTimer = 0.0f;
            JumpHeld = false;
        }

        public void Move(float direction)
        {
            _moveInput = System.Math.Clamp(direction, -1.0f, 1.0f);
        }

        public void Stop()
        {
            _moveInput = 0.0f;
        }

        public void SetJumpHeld(bool held)
        {
            JumpHeld = held;
        }

        public void RequestJump()
        {
            _jumpBufferTimer = JumpBufferTime;
        }

        public void Update(JollyGameTime gameTime)
        {
            ArgumentNullException.ThrowIfNull(gameTime);

            float deltaTime = (float)gameTime.DeltaSeconds;

            if (deltaTime <= 0.0f)
            {
                return;
            }

            UpdateCoyoteTime(deltaTime);
            UpdateJumpBuffer(deltaTime);
            UpdateHorizontalMovement(deltaTime);
            UpdateVerticalMovement(deltaTime);
            TryPerformBufferedJump();
        }

        private void UpdateCoyoteTime(float deltaTime)
        {
            if (_gameObject.IsGrounded)
            {
                _coyoteTimer = CoyoteTime;
                return;
            }

            _coyoteTimer -= deltaTime;

            if (_coyoteTimer < 0.0f)
            {
                _coyoteTimer = 0.0f;
            }
        }

        private void UpdateJumpBuffer(float deltaTime)
        {
            if (_jumpBufferTimer <= 0.0f)
            {
                return;
            }

            _jumpBufferTimer -= deltaTime;

            if (_jumpBufferTimer < 0.0f)
            {
                _jumpBufferTimer = 0.0f;
            }
        }

        private void UpdateHorizontalMovement(float deltaTime)
        {
            float velocityX = _gameObject.Velocity.X;

            if (_moveInput != 0.0f)
            {
                velocityX += _moveInput * MoveAcceleration * deltaTime;
                velocityX = System.Math.Clamp(velocityX, -MaxMoveSpeed, MaxMoveSpeed);
            }
            else
            {
                float deceleration = _gameObject.IsGrounded ? GroundDeceleration : AirDeceleration;
                float amount = deceleration * deltaTime;

                if (MathF.Abs(velocityX) <= amount)
                {
                    velocityX = 0.0f;
                }
                else
                {
                    velocityX -= MathF.Sign(velocityX) * amount;
                }
            }

            _gameObject.Velocity = new JollyVector2(velocityX, _gameObject.Velocity.Y);
        }

        private void UpdateVerticalMovement(float deltaTime)
        {
            float velocityY = _gameObject.Velocity.Y;

            if (velocityY < 0.0f)
            {
                velocityY += JumpGravity * deltaTime;

                if (!JumpHeld)
                {
                    velocityY += JumpCutGravity * deltaTime;
                }
            }
            else
            {
                velocityY += FallGravity * deltaTime;
            }

            velocityY = MathF.Min(velocityY, MaxFallSpeed);

            _gameObject.Velocity = new JollyVector2(_gameObject.Velocity.X, velocityY);
        }

        private void TryPerformBufferedJump()
        {
            if (_jumpBufferTimer <= 0.0f)
            {
                return;
            }

            if (!_gameObject.IsGrounded && _coyoteTimer <= 0.0f)
            {
                return;
            }

            JollyPhysicsSystem.Jump(_gameObject, JumpVelocity);

            _jumpBufferTimer = 0.0f;
            _coyoteTimer = 0.0f;
        }
    }
}
