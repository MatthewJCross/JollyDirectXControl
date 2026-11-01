using Jolly2DEngine.Animation;
using Jolly2DEngine.Math;
using Jolly2DEngine.Physics;
using Jolly2DEngine.Rendering;

namespace Jolly2DEngine.Graphics
{
    public sealed class JollySprite
    {
        private readonly Dictionary<string, JollyAnimation> _animations = new(StringComparer.OrdinalIgnoreCase);

        private JollyVector2 _position;
        private JollyVector2 _size;
        private JollyVector2 _scale = JollyVector2.One;
        private float _rotation;
        private float _opacity = 1.0f;
        private bool _visible = true;
        private int _layer;
        private bool _flipHorizontal;
        private bool _flipVertical;
        private float _sourceX;
        private float _sourceY;
        private float _sourceWidth = 1.0f;
        private float _sourceHeight = 1.0f;

        private JollySpriteSheet? _animationSheet;
        private float _animationFPS;
        private float _animationTime;
        private int _animationFrame;
        private bool _animationLoop;
        private bool _animationPlaying;
        private JollyAnimation? _currentAnimation;

        internal event Action<JollySprite>? Changed;
        public event Action<JollySprite, string>? AnimationCompleted;

        public JollyCollisionMode CollisionMode { get; set; } = JollyCollisionMode.None;

        public byte CollisionAlphaThreshold { get; set; } = 1;

        internal JollySprite(JollyTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            Texture = texture;
            _size = new JollyVector2(texture.Width, texture.Height);
        }

        internal JollySpriteSnapshot CreateSnapshot()
        {
            return new JollySpriteSnapshot(this);
        }

        public string Name { get; set; } = string.Empty;

        public JollyTexture Texture { get; }

        public JollyVector2 Position
        {
            get => _position;
            set
            {
                _position = value;
                Changed?.Invoke(this);
            }
        }

        public JollyVector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                Changed?.Invoke(this);
            }
        }

        public JollyVector2 Scale
        {
            get => _scale;
            set
            {
                if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Scale must contain finite values.");
                }

                if (value.X < 0.0f || value.Y < 0.0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Scale cannot be negative.");
                }

                _scale = value;
                Changed?.Invoke(this);
            }
        }

        public float X
        {
            get => _position.X;
            set
            {
                _position.X = value;
                Changed?.Invoke(this);
            }
        }

        public float Y
        {
            get => _position.Y;
            set
            {
                _position.Y = value;
                Changed?.Invoke(this);
            }
        }

        public float Width
        {
            get => _size.X;
            set
            {
                _size.X = value;
                Changed?.Invoke(this);
            }
        }

        public float Height
        {
            get => _size.Y;
            set
            {
                _size.Y = value;
                Changed?.Invoke(this);
            }
        }

        public float Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                Changed?.Invoke(this);
            }
        }

        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = System.Math.Clamp(value, 0.0f, 1.0f);
                Changed?.Invoke(this);
            }
        }

        public JollyColor Color { get; set; } = JollyColor.White;

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                Changed?.Invoke(this);
            }
        }

        public int Layer
        {
            get => _layer;
            set
            {
                _layer = value;
                Changed?.Invoke(this);
            }
        }

        public bool FlipHorizontal
        {
            get => _flipHorizontal;
            set
            {
                _flipHorizontal = value;
                Changed?.Invoke(this);
            }
        }

        public bool FlipVertical
        {
            get => _flipVertical;
            set
            {
                _flipVertical = value;
                Changed?.Invoke(this);
            }
        }

        public float SourceX
        {
            get => _sourceX;
            set
            {
                _sourceX = System.Math.Clamp(value, 0.0f, 1.0f);
                Changed?.Invoke(this);
            }
        }

        public float SourceY
        {
            get => _sourceY;
            set
            {
                _sourceY = System.Math.Clamp(value, 0.0f, 1.0f);
                Changed?.Invoke(this);
            }
        }

        public float SourceWidth
        {
            get => _sourceWidth;
            set
            {
                _sourceWidth = System.Math.Clamp(value, 0.0f, 1.0f);
                Changed?.Invoke(this);
            }
        }

        public float SourceHeight
        {
            get => _sourceHeight;
            set
            {
                _sourceHeight = System.Math.Clamp(value, 0.0f, 1.0f);
                Changed?.Invoke(this);
            }
        }

        public bool AnimationPlaying => _animationPlaying;

        public int AnimationFrame => _animationFrame;

        public float AnimationFPS => _animationFPS;

        public int AnimationFrameCount => _currentAnimation?.FrameCount ?? _animationSheet?.FrameCount ?? 0;

        public bool AnimationLoop
        {
            get => _animationLoop;
            set => _animationLoop = value;
        }

        public JollySpriteSheet? AnimationSheet => _animationSheet;

        public string? CurrentAnimation => _currentAnimation?.Name;

        public bool IsAnimationComplete
        {
            get
            {
                if (_currentAnimation == null)
                {
                    return false;
                }

                return !_animationPlaying && !_currentAnimation.Loop && _animationFrame >= _currentAnimation.FrameCount - 1;
            }
        }

        public IReadOnlyDictionary<string, JollyAnimation> Animations => _animations;

        public void AddAnimation(JollyAnimation animation)
        {
            ArgumentNullException.ThrowIfNull(animation);

            if (!ReferenceEquals(Texture, animation.Sheet.Texture))
            {
                throw new ArgumentException("The animation texture does not match the sprite texture.", nameof(animation));
            }

            _animations[animation.Name] = animation;
        }

        public bool RemoveAnimation(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (_currentAnimation != null && string.Equals(_currentAnimation.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                _currentAnimation = null;
            }

            return _animations.Remove(name);
        }

        public bool HasAnimation(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return _animations.ContainsKey(name);
        }

        public JollyAnimation? GetAnimation(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return _animations.TryGetValue(name, out JollyAnimation? animation) ? animation : null;
        }

        public void Play(string animationName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(animationName);

            if (!_animations.TryGetValue(animationName, out JollyAnimation? animation))
            {
                throw new ArgumentException($"Animation '{animationName}' does not exist.", nameof(animationName));
            }

            if (ReferenceEquals(_currentAnimation, animation) && _animationPlaying)
            {
                return;
            }

            _currentAnimation = animation;
            _animationSheet = animation.Sheet;
            _animationFPS = (float)animation.FPS;
            _animationLoop = animation.Loop;
            _animationTime = 0.0f;
            _animationFrame = 0;
            _animationPlaying = true;

            SetFrame(animation.GetFrame(0), animation.Sheet);
        }

        public void SetAnimation(JollySpriteSheet sheet, float framesPerSecond, bool loop = true)
        {
            ArgumentNullException.ThrowIfNull(sheet);

            if (!ReferenceEquals(Texture, sheet.Texture))
            {
                throw new ArgumentException("The sprite texture does not match the sprite sheet texture.", nameof(sheet));
            }

            if (framesPerSecond > 0.0f)
            {
                _currentAnimation = null;
                _animationSheet = sheet;
                _animationFPS = framesPerSecond;
                _animationLoop = loop;
                _animationTime = 0.0f;
                _animationFrame = 0;
                _animationPlaying = false;

                JollySpriteSheetFrame frame = sheet.GetFrame(0);

                SetFrame(frame, sheet);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(framesPerSecond));
            }
        }

        public void Play()
        {
            if (_animationSheet == null)
            {
                return;
            }

            _animationPlaying = true;
        }

        public void Pause()
        {
            _animationPlaying = false;
        }

        public void Stop()
        {
            if (_animationSheet == null)
            {
                return;
            }

            _animationPlaying = false;
            _animationTime = 0.0f;
            _animationFrame = 0;

            SetFrame(_animationSheet.GetFrame(0), _animationSheet);
        }

        public void SetAnimationFrame(int frame)
        {
            if (_animationSheet == null)
            {
                throw new InvalidOperationException("No sprite sheet animation has been configured.");
            }

            if (frame < 0 || frame >= _animationSheet.FrameCount)
            {
                throw new ArgumentOutOfRangeException(nameof(frame));
            }

            _animationFrame = frame;
            _animationTime = 0.0f;

            SetFrame(_animationSheet.GetFrame(_animationFrame), _animationSheet);
        }

        public void SetFrame(JollySpriteSheetFrame frame, JollySpriteSheet sheet)
        {
            ArgumentNullException.ThrowIfNull(sheet);

            if (!ReferenceEquals(Texture, sheet.Texture))
            {
                throw new ArgumentException("The sprite texture does not match the sprite sheet texture.", nameof(sheet));
            }

            float sourceX = (float)frame.X / sheet.Texture.Width;
            float sourceY = (float)frame.Y / sheet.Texture.Height;
            float sourceWidth = (float)frame.Width / sheet.Texture.Width;
            float sourceHeight = (float)frame.Height / sheet.Texture.Height;

            SourceX = sourceX;
            SourceY = sourceY;
            SourceWidth = sourceWidth;
            SourceHeight = sourceHeight;
            Width = frame.Width;
            Height = frame.Height;
        }

        internal void UpdateAnimation(float deltaTime)
        {
            if (!_animationPlaying || _animationSheet == null)
            {
                return;
            }

            if (deltaTime <= 0.0f)
            {
                return;
            }

            _animationTime += deltaTime;

            float frameDuration = 1.0f / _animationFPS;

            while (_animationTime >= frameDuration)
            {
                _animationTime -= frameDuration;

                int nextFrame = _animationFrame + 1;

                if (_currentAnimation != null)
                {
                    if (nextFrame >= _currentAnimation.FrameCount)
                    {
                        if (_currentAnimation.Loop)
                        {
                            nextFrame = 0;
                        }
                        else
                        {
                            nextFrame = _currentAnimation.FrameCount - 1;
                            _animationPlaying = false;

                            _animationFrame = nextFrame;

                            SetFrame(_currentAnimation.GetFrame(_animationFrame), _currentAnimation.Sheet);

                            AnimationCompleted?.Invoke(this, _currentAnimation.Name);

                            break;
                        }
                    }

                    _animationFrame = nextFrame;

                    SetFrame(_currentAnimation.GetFrame(_animationFrame), _currentAnimation.Sheet);
                }
                else
                {
                    if (nextFrame >= _animationSheet.FrameCount)
                    {
                        if (_animationLoop)
                        {
                            nextFrame = 0;
                        }
                        else
                        {
                            nextFrame = _animationSheet.FrameCount - 1;
                            _animationPlaying = false;

                            _animationFrame = nextFrame;

                            SetFrame(_animationSheet.GetFrame(_animationFrame), _animationSheet);

                            break;
                        }
                    }

                    _animationFrame = nextFrame;

                    SetFrame(_animationSheet.GetFrame(_animationFrame), _animationSheet);
                }

                if (!_animationPlaying)
                {
                    break;
                }
            }
        }
    }
}