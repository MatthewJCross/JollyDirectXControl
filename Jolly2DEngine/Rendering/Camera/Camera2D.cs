using System.Numerics;

namespace Jolly2DEngine.Rendering.Camera
{
    public sealed class Camera2D
    {
        private Vector2 _position;
        private float _zoom = 1.0f;
        private float _rotation;
        private float _followSpeed = 8.0f;

        private bool _boundsEnabled;
        private float _boundsLeft;
        private float _boundsTop;
        private float _boundsRight;
        private float _boundsBottom;

        private float _shakeDuration;
        private float _shakeTime;
        private float _shakeIntensity;
        private Vector2 _shakeOffset;
        private readonly Random _shakeRandom = new();

        #region Properties
        public Vector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                ClampPosition();
            }
        }

        public float X
        {
            get => _position.X;
            set
            {
                _position.X = value;
                ClampPosition();
            }
        }

        public float Y
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                ClampPosition();
            }
        }

        public float Zoom
        {
            get => _zoom;
            set
            {
                if (!float.IsFinite(value) || value <= 0.0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Camera zoom must be greater than zero.");
                }

                _zoom = value;
                ClampPosition();
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                if (!float.IsFinite(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Camera rotation must be finite.");
                }

                _rotation = value;
                ClampPosition();
            }
        }

        public float FollowSpeed
        {
            get => _followSpeed;
            set => _followSpeed = MathF.Max(0.0f, value);
        }

        public float ViewportScale
        {
            get
            {
                if (!UseFixedAspectRatio)
                {
                    return 1.0f;
                }

                return MathF.Min(ViewportSize.X / VirtualResolution.X, ViewportSize.Y / VirtualResolution.Y);
            }
        }

        public Vector2 ViewportOffset
        {
            get
            {
                if (!UseFixedAspectRatio)
                {
                    return Vector2.Zero;
                }

                return new Vector2((ViewportSize.X - VirtualResolution.X * ViewportScale) * 0.5f, (ViewportSize.Y - VirtualResolution.Y * ViewportScale) * 0.5f);
            }
        }        
        #endregion
        
        public Vector2 ViewportSize { get; private set; }
        public Vector2 VirtualResolution { get; private set; }
        public bool UseFixedAspectRatio { get; private set; }

        public bool BoundsEnabled => _boundsEnabled;
        public float BoundsLeft => _boundsLeft;
        public float BoundsTop => _boundsTop;
        public float BoundsRight => _boundsRight;
        public float BoundsBottom => _boundsBottom;
        public bool IsShaking => _shakeTime > 0.0f;
        public Vector2 ShakeOffset => _shakeOffset;

        public void SetViewport(float width, float height)
        {
            ViewportSize = new Vector2(MathF.Max(1.0f, width), MathF.Max(1.0f, height));
            ClampPosition();
        }

        public void SetBounds(float left, float top, float right, float bottom)
        {
            if (!float.IsFinite(left) || !float.IsFinite(top) || !float.IsFinite(right) || !float.IsFinite(bottom))
            {
                throw new ArgumentOutOfRangeException(nameof(left), "Camera bounds must be finite.");
            }

            if (right <= left)
            {
                throw new ArgumentOutOfRangeException(nameof(right), "Camera right bound must be greater than left.");
            }

            if (bottom <= top)
            {
                throw new ArgumentOutOfRangeException(nameof(bottom), "Camera bottom bound must be greater than top.");
            }

            _boundsLeft = left;
            _boundsTop = top;
            _boundsRight = right;
            _boundsBottom = bottom;
            _boundsEnabled = true;

            ClampPosition();
        }

        public void ClearBounds()
        {
            _boundsEnabled = false;
        }

        public void Follow(Vector2 targetPosition)
        {
            Position = targetPosition;
        }

        public void FollowSmooth(Vector2 targetPosition, float deltaSeconds)
        {
            if (deltaSeconds <= 0.0f)
            {
                return;
            }

            if (_followSpeed <= 0.0f)
            {
                Position = targetPosition;
                return;
            }

            float amount = 1.0f - MathF.Exp(-_followSpeed * deltaSeconds);

            Position = Vector2.Lerp(_position, targetPosition, amount);
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, GetWorldToScreenMatrix());
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            Matrix3x2 matrix = GetWorldToScreenMatrix();

            if (!Matrix3x2.Invert(matrix, out Matrix3x2 inverse))
            {
                return Vector2.Zero;
            }

            return Vector2.Transform(screenPosition, inverse);
        }

        public Matrix3x2 GetWorldToScreenMatrix()
        {
            Matrix3x2 matrix = Matrix3x2.CreateTranslation(-_position);
            matrix *= Matrix3x2.CreateRotation(-_rotation);
            matrix *= Matrix3x2.CreateScale(_zoom);
            matrix *= Matrix3x2.CreateTranslation(ViewportSize.X * 0.5f, ViewportSize.Y * 0.5f);

            return matrix;
        }

        public CameraState CaptureState()
        {
            if (!UseFixedAspectRatio)
            {
                return new CameraState(_position + _shakeOffset, _zoom, _rotation, ViewportSize);
            }

            return new CameraState(_position + _shakeOffset, _zoom, _rotation, VirtualResolution, ViewportScale, ViewportOffset);
        }

        public void Shake(float intensity, float duration)
        {
            if (!float.IsFinite(intensity) || intensity < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity), "Camera shake intensity must be zero or greater.");
            }

            if (!float.IsFinite(duration) || duration <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Camera shake duration must be greater than zero.");
            }

            _shakeIntensity = intensity;
            _shakeDuration = duration;
            _shakeTime = duration;
            _shakeOffset = Vector2.Zero;
        }

        public void UpdateShake(float deltaSeconds)
        {
            if (_shakeTime <= 0.0f)
            {
                _shakeOffset = Vector2.Zero;
                return;
            }

            _shakeTime -= MathF.Max(0.0f, deltaSeconds);

            if (_shakeTime <= 0.0f)
            {
                _shakeTime = 0.0f;
                _shakeOffset = Vector2.Zero;
                return;
            }

            float strength = _shakeTime / _shakeDuration;

            float offsetX = ((float)_shakeRandom.NextDouble() * 2.0f - 1.0f) * _shakeIntensity * strength;
            float offsetY = ((float)_shakeRandom.NextDouble() * 2.0f - 1.0f) * _shakeIntensity * strength;

            _shakeOffset = new Vector2(offsetX, offsetY);
        }

        private void ClampPosition()
        {
            if (!_boundsEnabled)
            {
                return;
            }

            Vector2 effectiveViewportSize = UseFixedAspectRatio ? VirtualResolution : ViewportSize;

            float halfViewportWidth = effectiveViewportSize.X / (2.0f * _zoom);
            float halfViewportHeight = effectiveViewportSize.Y / (2.0f * _zoom);
            float minimumX = _boundsLeft + halfViewportWidth;
            float maximumX = _boundsRight - halfViewportWidth;
            float minimumY = _boundsTop + halfViewportHeight;
            float maximumY = _boundsBottom - halfViewportHeight;

            if (minimumX > maximumX)
            {
                _position.X = (_boundsLeft + _boundsRight) * 0.5f;
            }
            else
            {
                _position.X = System.Math.Clamp(_position.X, minimumX, maximumX);
            }

            if (minimumY > maximumY)
            {
                _position.Y = (_boundsTop + _boundsBottom) * 0.5f;
            }
            else
            {
                _position.Y = System.Math.Clamp(_position.Y, minimumY, maximumY);
            }
        }

        public void SetVirtualResolution(float width, float height)
        {
            if (!float.IsFinite(width) || width <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Virtual width must be greater than zero.");
            }

            if (!float.IsFinite(height) || height <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Virtual height must be greater than zero.");
            }

            VirtualResolution = new Vector2(width, height);
            UseFixedAspectRatio = true;

            ClampPosition();
        }

        public void ClearVirtualResolution()
        {
            UseFixedAspectRatio = false;
            VirtualResolution = Vector2.Zero;

            ClampPosition();
        }
    }
}
