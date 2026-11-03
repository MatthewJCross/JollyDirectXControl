using Jolly2DEngine.Diagnostics;
using Jolly2DEngine.Graphics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Jolly2DEngine.Rendering.DirectX
{
    public partial class DirectXControl : System.Windows.Controls.UserControl//, System.ComponentModel.INotifyPropertyChanged
    {
        private readonly System.Windows.Forms.Panel _panel;
        private DirectXRenderer? _renderer;
        private DirectXImage? _pendingImage;
        private readonly List<DirectXTexture> _pendingTextures = [];
        private DirectXStretchMode _stretchMode = DirectXStretchMode.Uniform;
        private DirectXImageFilter _imageFilter = DirectXImageFilter.Linear;
        private double _lastDisplayedFps = -1.0;
        private readonly System.Windows.Threading.DispatcherTimer _fpsTimer;
        private double _maxFps;
        private bool _vsync = true;
        private bool _clearEveryFrame = true;
        private System.Windows.Media.Color _clearColor = System.Windows.Media.Colors.Black;

        private readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public event Action<double, double>? MouseMoved;
        public event Action? MouseButtonDown;
        public event Action? MouseButtonUp;
        public event Action<float, float>? RenderSizeChanged;

        public bool LineGlowEnabled
        {
            get => _renderer?.LineGlowEnabled ?? true;
            set
            {
                if (_renderer != null)
                {
                    _renderer.LineGlowEnabled = value;
                }
            }
        }

        public float LineGlowWidth
        {
            get => _renderer?.LineGlowWidth ?? 4.0f;
            set
            {
                if (_renderer != null)
                {
                    _renderer.LineGlowWidth = value;
                }
            }
        }

        public float LineGlowStrength
        {
            get => _renderer?.LineGlowStrength ?? 0.75f;
            set
            {
                if (_renderer != null)
                {
                    _renderer.LineGlowStrength = value;
                }
            }
        }

        public float LineGlowFalloff
        {
            get => _renderer?.LineGlowFalloff ?? 2.0f;
            set
            {
                if (_renderer != null)
                {
                    _renderer.LineGlowFalloff = value;
                }
            }
        }

        public JollyDiagnostics? Diagnostics { get; set; }

        public DirectXControl()
        {
            InitializeComponent();

            _panel = new System.Windows.Forms.Panel
            {
                BackColor = System.Drawing.Color.Black,
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            _panel.MouseMove += DirectXControl_MouseMove;
            _panel.MouseDown += DirectXControl_PanelMouseDown;
            _panel.MouseUp += DirectXControl_PanelMouseUp;
            _panel.Resize += DirectXControl_PanelResize;

            Host.Child = _panel;

            Loaded += DirectXControl_Loaded;
            Unloaded += DirectXControl_Unloaded;
            //SizeChanged += DirectXControl_SizeChanged;

            _fpsTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _fpsTimer.Tick += FpsTimer_Tick;
        }

        public DirectXStretchMode StretchMode
        {
            get => _stretchMode;

            set
            {
                _stretchMode = value;
                _renderer?.SetStretchMode(value);
            }
        }

        public DirectXImageFilter ImageFilter
        {
            get => _imageFilter;

            set
            {
                _imageFilter = value;
                _renderer?.SetImageFilter(value);
            }
        }

        public double MaxFPS
        {
            get => _maxFps;

            set
            {
                _maxFps = System.Math.Max(0.0, value);
                _renderer?.MaxFPS = _maxFps;
            }
        }

        public bool VSync
        {
            get => _vsync;

            set
            {
                _vsync = value;
                _renderer?.VSync = value;
            }
        }

        public bool ClearEveryFrame
        {
            get => _clearEveryFrame;

            set
            {
                _clearEveryFrame = value;

                _renderer?.ClearEveryFrame = value;
            }
        }

        public System.Windows.Media.Color ClearColor
        {
            get => _clearColor;

            set
            {
                _clearColor = value;

                _renderer?.ClearColor = value;
            }
        }

        public static readonly System.Windows.DependencyProperty FPSProperty = System.Windows.DependencyProperty.Register(nameof(FPS), typeof(double), typeof(DirectXControl), new System.Windows.PropertyMetadata(0.0));
        public double FPS
        {
            get => (double)GetValue(FPSProperty);
            private set => SetValue(FPSProperty, value);
        }

        public static readonly System.Windows.DependencyProperty FrameTimeProperty = System.Windows.DependencyProperty.Register(nameof(FrameTime), typeof(double), typeof(DirectXControl), new System.Windows.PropertyMetadata(0.0));
        public double FrameTime
        {
            get => (double)GetValue(FrameTimeProperty);
            private set => SetValue(FrameTimeProperty, value);
        }

        public bool IsDirectXReady => _renderer != null && _renderer.IsInitialized;

        public event EventHandler<System.Windows.Point>? MouseScreenPositionChanged;

        private void FpsTimer_Tick(object? sender, EventArgs e)
        {
            if (_renderer == null)
            {
                return;
            }

            FPS = _renderer.FPS;
            FrameTime = _renderer.FrameTime;

            if (System.Math.Abs(_renderer.FPS - _lastDisplayedFps) >= 0.1)
            {
                _lastDisplayedFps = _renderer.FPS;
            }
        }

        private void DirectXControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_renderer != null)
            {
                return;
            }

            _renderer = new DirectXRenderer();

            _renderer.SetStretchMode(_stretchMode);
            _renderer.SetImageFilter(_imageFilter);

            _renderer.Initialize(_panel.Handle, System.Math.Max(1, _panel.ClientSize.Width), System.Math.Max(1, _panel.ClientSize.Height));

            _renderer.MaxFPS = _maxFps;
            _renderer.VSync = _vsync;
            _renderer.ClearColor = _clearColor;
            _renderer.ClearEveryFrame = _clearEveryFrame;

            _renderer.StartRenderLoop();

            if (_pendingImage != null)
            {
                DirectXImage image = _pendingImage;
                _pendingImage = null;

                _renderer.SetImage(image);
            }

            if (_pendingTextures.Count > 0)
            {
                foreach (DirectXTexture texture in _pendingTextures)
                {
                    _renderer.AddTexture(texture);
                }

                _pendingTextures.Clear();
            }

            _ready.TrySetResult(true);
            _fpsTimer.Start();
        }

        private void DirectXControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _fpsTimer.Stop();

            _renderer?.StopRenderLoop();
            _renderer?.Dispose();

            _renderer = null;
        }

        private void DirectXControl_MouseMove(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Windows.Point position = new(e.X, e.Y);
            MouseScreenPositionChanged?.Invoke(this, position);
            MouseMoved?.Invoke(position.X, position.Y);
        }

        private void DirectXControl_PanelMouseDown(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
            {
                return;
            }

            MouseMoved?.Invoke(e.X, e.Y);
            MouseButtonDown?.Invoke();
        }

        private void DirectXControl_PanelMouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
            {
                return;
            }

            MouseMoved?.Invoke(e.X, e.Y);
            MouseButtonUp?.Invoke();
        }

        private void DirectXControl_PanelResize(object? sender, EventArgs e)
        {
            if (_renderer == null || _panel.ClientSize.Width <= 0 || _panel.ClientSize.Height <= 0)
            {
                return;
            }

            int width = _panel.ClientSize.Width;
            int height = _panel.ClientSize.Height;

            _renderer.Resize(width, height,
            _renderer.Get_renderTargetView1());
            RenderSizeChanged?.Invoke(width, height);
        }

        public async Task WaitForReadyAsync()
        {
            await _ready.Task.ConfigureAwait(false);
        }

        public void LoadImage(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            DirectXImage image = DirectXImage.FromFile(filePath);
            SetImage(image);
        }

        public void SetImage(BitmapSource source)
        {
            ArgumentNullException.ThrowIfNull(source);
            DirectXImage image = DirectXImage.FromBitmapSource(source);
            SetImage(image);
        }

        public void SetImage(byte[] pixels, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(pixels);
            DirectXImage image = new(width, height, pixels);
            SetImage(image);
        }

        public void SetImage(DirectXImage image)
        {
            ArgumentNullException.ThrowIfNull(image);

            if (_renderer == null)
            {
                _pendingImage = image;
                return;
            }

            _renderer.SetImage(image);
        }

        public void ClearScreen()
        {
            _renderer?.ClearScreen();
        }

        public void Clear()
        {
            _pendingImage = null;
            _renderer?.Clear();
        }

        public DirectXTexture CreateTexture(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Texture file not found: {filePath}", filePath);
            }

            BitmapImage bitmap = new();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmap.EndInit();
            bitmap.Freeze();

            DirectXImage image = DirectXImage.FromBitmapSource(bitmap);

            return CreateTexture(image);
        }

        public DirectXTexture CreateTexture(DirectXImage image)
        {
            ArgumentNullException.ThrowIfNull(image);

            if (_renderer == null)
            {
                throw new InvalidOperationException("DirectXControl is not loaded.");
            }

            return _renderer.CreateTextureResource(image);
        }

        public DirectXJollyRenderer CreateJollyRenderer()
        {
            return new DirectXJollyRenderer(this);
        }

        internal void UpdateTexture(DirectXTexture texture, DirectXImage image)
        {
            ArgumentNullException.ThrowIfNull(texture);
            ArgumentNullException.ThrowIfNull(image);

            if (_renderer == null)
            {
                throw new InvalidOperationException("DirectXControl is not loaded.");
            }

            _renderer.UpdateTexture(texture, image);
        }

        internal void UnregisterTexture(DirectXTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (_renderer == null)
            {
                _pendingTextures.Remove(texture);
                return;
            }

            _renderer.RemoveTexture(texture);
        }

        internal void RegisterTexture(DirectXTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (_renderer == null)
            {
                if (!_pendingTextures.Contains(texture))
                {
                    _pendingTextures.Add(texture);
                }

                return;
            }

            _renderer.AddTexture(texture);
        }

        internal void SubmitRenderFrame(JollyRenderFrame frame, Jolly2DEngine.Rendering.Camera.Camera2D camera)
        {
            ArgumentNullException.ThrowIfNull(frame);
            ArgumentNullException.ThrowIfNull(camera);

            _renderer?.Diagnostics = Diagnostics;
            _renderer?.SubmitRenderFrame(frame, camera);
        }
    }
}
