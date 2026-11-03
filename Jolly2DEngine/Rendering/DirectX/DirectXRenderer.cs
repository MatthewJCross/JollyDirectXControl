using Jolly2DEngine.Diagnostics;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.Camera;
using SharpGen.Runtime;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using MediaColor = System.Windows.Media.Color;
using MediaColors = System.Windows.Media.Colors;

namespace Jolly2DEngine.Rendering.DirectX
{
    public enum DirectXImageFilter
    {
        Point,
        Linear
    }

    public enum DirectXStretchMode
    {
        None,
        Stretch,
        Uniform,
        UniformToFill
    }

    public readonly struct SpriteVertex
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float U;
        public readonly float V;

        public SpriteVertex(float x, float y, float z, float u, float v)
        {
            X = x;
            Y = y;
            Z = z;
            U = u;
            V = v;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SpriteConstants
    {
        public float PositionX;
        public float PositionY;
        public float Width;
        public float Height;
        public float Rotation;
        public float Opacity;
        public float ViewportWidth;
        public float ViewportHeight;
        public float FlipHorizontal;
        public float FlipVertical;
        public float SourceX;
        public float SourceY;
        public float SourceWidth;
        public float SourceHeight;
        public float Padding0;
        public float Padding1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SpriteInstance
    {
        public float PositionX;
        public float PositionY;
        public float Width;
        public float Height;
        public float Rotation;
        public float Opacity;
        public float FlipHorizontal;
        public float FlipVertical;
        public float SourceX;
        public float SourceY;
        public float SourceWidth;
        public float SourceHeight;
        public float Padding0;
        public float Padding1;
        public float Padding2;
        public float Padding3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct LineVertex
    {
        public float X;
        public float Y;
        public float R;
        public float G;
        public float B;
        public float A;
        public float Edge;
        public float CoreRatio;

        public LineVertex(float x, float y, JollyColor color, float edge, float coreRatio)
        {
            X = x;
            Y = y;
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
            Edge = edge;
            CoreRatio = coreRatio;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct LineConstants
    {
        public float ViewportWidth;
        public float ViewportHeight;
        public float GlowStrength;
        public float GlowFalloff;
        public float GlowEnabled;
        public float Padding0;
        public float Padding1;
        public float Padding2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TriangleVertex
    {
        public float X;
        public float Y;
        public float R;
        public float G;
        public float B;
        public float A;

        public TriangleVertex(float x, float y, JollyColor color)
        {
            X = x;
            Y = y;
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TriangleConstants
    {
        public float ViewportWidth;
        public float ViewportHeight;
        public float Padding0;
        public float Padding1;
    }

    public sealed class DirectXRenderer : IDisposable
    {
        private ID3D11Device? _device;
        private ID3D11DeviceContext? _context;
        private IDXGISwapChain1? _swapChain;
        private ID3D11RenderTargetView? _renderTargetView;

        private ID3D11VertexShader? _vertexShader;
        private ID3D11PixelShader? _pixelShader;
        private ID3D11InputLayout? _inputLayout;
        private ID3D11Buffer? _vertexBuffer;
        private ID3D11ShaderResourceView? _shaderResourceView;
        private ID3D11Texture2D? _texture;
        private ID3D11SamplerState? _sampler;
        private ID3D11BlendState? _blendState;

        private ID3D11Buffer? _spriteConstantBuffer;
        private ID3D11VertexShader? _spriteVertexShader;
        private ID3D11PixelShader? _spritePixelShader;
        private ID3D11InputLayout? _spriteInputLayout;
        private ID3D11SamplerState? _spriteSampler;
        private ID3D11Buffer? _spriteVertexBuffer;
        private ID3D11Buffer? _spriteInstanceBuffer;
        private float _spriteConstantBufferWidth;
        private float _spriteConstantBufferHeight;

        private ID3D11VertexShader? _lineVertexShader;
        private ID3D11PixelShader? _linePixelShader;
        private ID3D11InputLayout? _lineInputLayout;
        private ID3D11Buffer? _lineVertexBuffer;
        private ID3D11Buffer? _lineConstantBuffer;
        private ID3D11RasterizerState? _lineRasterizerState;

        private ID3D11VertexShader? _triangleVertexShader;
        private ID3D11PixelShader? _trianglePixelShader;
        private ID3D11InputLayout? _triangleInputLayout;
        private ID3D11Buffer? _triangleVertexBuffer;
        private ID3D11Buffer? _triangleConstantBuffer;


        private static readonly uint SpriteInstanceStride = (uint)Marshal.SizeOf<SpriteInstance>();
        private static readonly uint SpriteVertexStride = (uint)Marshal.SizeOf<SpriteVertex>();
        private static readonly uint LineVertexStride = (uint)Marshal.SizeOf<LineVertex>();
        private static readonly uint TriangleVertexStride = (uint)Marshal.SizeOf<TriangleVertex>();

        private const int InitialSpriteInstanceCapacity = 256;
        private const int InitialLineVertexCapacity = 256;
        private const int InitialTriangleVertexCapacity = 256;
        private readonly List<SpriteInstance> _spriteBatchInstances = new(InitialSpriteInstanceCapacity);
        private readonly List<LineVertex> _lineVertices = new(InitialLineVertexCapacity);
        private readonly List<TriangleVertex> _triangleVertices = new(InitialTriangleVertexCapacity);

        private int _spriteInstanceCapacity;
        private int _lineVertexCapacity;
        private int _triangleVertexCapacity;

        private readonly Dictionary<DirectXTexture, ID3D11ShaderResourceView> _textureViews = new();
        private readonly Dictionary<JollyTexture, ID3D11ShaderResourceView> _spriteTextureViews = new();
        private readonly ConcurrentQueue<Action> _renderCommands = new();
        private readonly List<DirectXTexture> _textures = new();

        private JollyRenderSubmission? _latestRenderSubmission;

        private JollyProfile? _profileRenderSetup;
        private JollyProfile? _profileRenderClear;
        private JollyProfile? _profileRenderImage;
        private JollyProfile? _profileRenderLines;
        private JollyProfile? _profileRenderTriangles;
        private JollyProfile? _profileRenderSprites;
        private JollyProfile? _profileRenderPresent;
        private JollyProfile? _profileSpriteSetup;
        private JollyProfile? _profileSpritePrepare;
        private JollyProfile? _profileSpriteDraw;
        private JollyProfile? _profileSpriteBuffer;
        private JollyProfile? _profileSpriteUpload;
        private JollyProfile? _profileSpriteDrawCall;

        private IntPtr _hwnd;
        private int _width;
        private int _height;
        private int _imageWidth;
        private int _imageHeight;

        private bool _initialized;
        private bool _disposed;
        private bool _hasImage;
        private bool _vertexBufferDirty = true;
        private bool _tearingSupported;

        private Thread? _renderThread;
        private volatile bool _renderThreadRunning;
        private int _renderThreadId;

        private long _fpsFrameCount;
        private long _fpsLastTimestamp;
        private double _fps;
        private double _frameTime;

        private long _nextFrameTimestamp;
        private double _maxFps;

        private int _renderSpriteCount;
        private int _renderBatchCount;
        private int _renderDrawCallCount;
        private int _renderMaxBatchSize;

        private bool _vsync = true;
        private bool _clearEveryFrame = true;
        private MediaColor _clearColor = MediaColors.Red;
        private bool _lineGlowEnabled = true;
        private float _lineGlowWidth = 4.0f;
        private float _lineGlowStrength = 0.75f;
        private float _lineGlowFalloff = 2.0f;

        private const float DegreesToRadians = MathF.PI / 180.0f;

        private JollyDiagnostics? _diagnostics;

        public double FPS => Volatile.Read(ref _fps);
        public double FrameTime => Volatile.Read(ref _frameTime);
        public bool IsInitialized => _initialized && !_disposed;
        public int RenderSpriteCount => _renderSpriteCount;
        public int RenderBatchCount => _renderBatchCount;
        public int RenderDrawCallCount => _renderDrawCallCount;
        public int RenderMaxBatchSize => _renderMaxBatchSize;
        public int RenderInstanceBufferCapacity => _spriteInstanceCapacity;
        public DirectXImageFilter ImageFilter { get; private set; } = DirectXImageFilter.Linear;
        public DirectXStretchMode StretchMode { get; private set; } = DirectXStretchMode.Uniform;

        private bool IsRenderThread => Environment.CurrentManagedThreadId == _renderThreadId;

        #region Properties
        public double MaxFPS
        {
            get => _maxFps;
            set
            {
                if (_renderThreadRunning && !IsRenderThread)
                {
                    QueueRenderCommand(() => MaxFPS = value);
                    return;
                }

                _maxFps = System.Math.Max(0.0, value);
                _nextFrameTimestamp = Stopwatch.GetTimestamp();
            }
        }

        public bool VSync
        {
            get => _vsync;
            set
            {
                if (_renderThreadRunning && !IsRenderThread)
                {
                    QueueRenderCommand(() => VSync = value);
                    return;
                }

                _vsync = value;
            }
        }

        public bool ClearEveryFrame
        {
            get => _clearEveryFrame;
            set => _clearEveryFrame = value;
        }

        public MediaColor ClearColor
        {
            get => _clearColor;
            set
            {
                if (_renderThreadRunning && !IsRenderThread)
                {
                    QueueRenderCommand(() => ClearColor = value);
                    return;
                }

                _clearColor = value;
            }
        }

        public bool LineGlowEnabled
        {
            get => _lineGlowEnabled;
            set
            {
                if (_renderThreadRunning && !IsRenderThread)
                {
                    QueueRenderCommand(() => LineGlowEnabled = value);
                    return;
                }

                _lineGlowEnabled = value;
            }
        }

        public float LineGlowWidth
        {
            get => _lineGlowWidth;
            set
            {
                if (_renderThreadRunning && !IsRenderThread)
                {
                    QueueRenderCommand(() => LineGlowWidth = value);
                    return;
                }

                _lineGlowWidth = MathF.Max(0.0f, value);
            }
        }

        public float LineGlowStrength
        {
            get => _lineGlowStrength;
            set
            {
                if (_renderThreadRunning && !IsRenderThread)
                {
                    QueueRenderCommand(() => LineGlowStrength = value);
                    return;
                }

                _lineGlowStrength = MathF.Max(0.0f, value);
            }
        }

        public float LineGlowFalloff
        {
            get => _lineGlowFalloff;
            set
            {
                if (_renderThreadRunning && !IsRenderThread)
                {
                    QueueRenderCommand(() => LineGlowFalloff = value);
                    return;
                }

                _lineGlowFalloff = MathF.Max(0.01f, value);
            }
        }

        public JollyDiagnostics? Diagnostics
        {
            get => _diagnostics;
            set
            {
                _diagnostics = value;

                if (value == null)
                {
                    _profileRenderSetup = null;
                    _profileRenderClear = null;
                    _profileRenderImage = null;
                    _profileRenderLines = null;
                    _profileRenderSprites = null;
                    _profileRenderPresent = null;
                    _profileRenderTriangles = null;
                    _profileSpriteSetup = null;
                    _profileSpritePrepare = null;
                    _profileSpriteDraw = null;
                    _profileSpriteBuffer = null;
                    _profileSpriteUpload = null;
                    _profileSpriteDrawCall = null;
                    return;
                }

                _profileRenderSetup = value.RegisterProfile("Render.Setup");
                _profileRenderClear = value.RegisterProfile("Render.Clear");
                _profileRenderImage = value.RegisterProfile("Render.Image");
                _profileRenderLines = value.RegisterProfile("Render.Lines");
                _profileRenderSprites = value.RegisterProfile("Render.Sprites");
                _profileRenderPresent = value.RegisterProfile("Render.Present");
                _profileRenderTriangles = value.RegisterProfile("Render.Triangles");
                _profileSpriteSetup = value.RegisterProfile("Render.SpriteSetup");
                _profileSpritePrepare = value.RegisterProfile("Render.SpritePrepare");
                _profileSpriteDraw = value.RegisterProfile("Render.SpriteDraw");
                _profileSpriteBuffer = value.RegisterProfile("Render.SpriteBuffer");
                _profileSpriteUpload = value.RegisterProfile("Render.SpriteUpload");
                _profileSpriteDrawCall = value.RegisterProfile("Render.SpriteDrawCall");
            }
        }

        #endregion

        public void Initialize(IntPtr hwnd, int width, int height)
        {
            if (_initialized)
            {
                return;
            }

            if (hwnd == IntPtr.Zero)
            {
                throw new ArgumentException("Invalid window handle.", nameof(hwnd));
            }

            _hwnd = hwnd;
            _width = System.Math.Max(1, width);
            _height = System.Math.Max(1, height);

            CreateDevice();
            CreateSwapChain();
            CreateRenderTarget();
            CreateShaders();
            CreateSampler();
            CreateVertexBuffer();
            CreateBlendState();
            CreateSpriteVertexBuffer();
            CreateSpriteInstanceBuffer();
            CreateSpriteConstantBuffer();
            CreateSpriteSampler();
            CreateLineVertexBuffer();
            CreateLineConstantBuffer();
            CreateLineRasterizerState();
            CreateTriangleVertexBuffer();
            CreateTriangleConstantBuffer();

            _initialized = true;
        }

        public void StartRenderLoop()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DirectXRenderer));
            }

            if (!_initialized)
            {
                throw new InvalidOperationException("DirectXRenderer must be initialized before starting the render loop.");
            }

            if (_renderThreadRunning)
            {
                return;
            }

            _renderThreadRunning = true;
            _renderThread = new Thread(RenderLoop)
            {
                IsBackground = true,
                Name = "Jolly2D DirectX Render Thread",
                Priority = ThreadPriority.AboveNormal
            };
            _renderThread.Start();
        }

        public void StopRenderLoop()
        {
            _renderThreadRunning = false;

            if (_renderThread != null && _renderThread.IsAlive && !IsRenderThread)
            {
                _renderThread.Join();
            }

            _renderThread = null;
            _renderThreadId = 0;
        }

        private void RenderLoop()
        {
            _renderThreadId = Environment.CurrentManagedThreadId;

            while (_renderThreadRunning)
            {
                ProcessRenderCommands();
                WaitForFrameLimit();

                if (!_renderThreadRunning)
                {
                    break;
                }

                Render();
            }

            ProcessRenderCommands();
        }

        private void WaitForFrameLimit()
        {
            if (_maxFps <= 0.0)
            {
                return;
            }

            double frameSeconds = 1.0 / _maxFps;
            long frameTicks = System.Math.Max(1, (long)(frameSeconds * Stopwatch.Frequency));
            long now = Stopwatch.GetTimestamp();

            if (_nextFrameTimestamp < now)
            {
                _nextFrameTimestamp = now;
            }

            long remaining = _nextFrameTimestamp - now;

            if (remaining > 0)
            {
                int milliseconds = (int)(remaining * 1000L / Stopwatch.Frequency);

                if (milliseconds > 1)
                {
                    Thread.Sleep(milliseconds - 1);
                }

                while (Stopwatch.GetTimestamp() < _nextFrameTimestamp)
                {
                    Thread.SpinWait(10);
                }
            }

            _nextFrameTimestamp += frameTicks;
        }

        private void ProcessRenderCommands()
        {
            while (_renderCommands.TryDequeue(out Action? command))
            {
                try
                {
                    command();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DirectX render command exception: {ex}");
                }
            }
        }

        private void QueueRenderCommand(Action command)
        {
            ArgumentNullException.ThrowIfNull(command);
            _renderCommands.Enqueue(command);
        }

        public void SetStretchMode(DirectXStretchMode mode)
        {
            if (_renderThreadRunning && !IsRenderThread)
            {
                QueueRenderCommand(() => SetStretchMode(mode));
                return;
            }

            StretchMode = mode;
            _vertexBufferDirty = true;
        }

        public void SetImageFilter(DirectXImageFilter filter)
        {
            if (!_initialized)
            {
                ImageFilter = filter;
                return;
            }

            if (_renderThreadRunning && !IsRenderThread)
            {
                QueueRenderCommand(() => SetImageFilter(filter));
                return;
            }

            ImageFilter = filter;

            SamplerDescription description = filter == DirectXImageFilter.Point ? SamplerDescription.PointClamp : SamplerDescription.LinearClamp;

            _sampler?.Dispose();
            _sampler = _device!.CreateSamplerState(description);

            _spriteSampler?.Dispose();
            _spriteSampler = _device.CreateSamplerState(description);
        }

        private void CreateDevice()
        {
            Result result = D3D11.D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.BgraSupport, Array.Empty<FeatureLevel>(), out _device, out _context);
            result.CheckError();
        }

        private void CreateSwapChain()
        {
            using IDXGIFactory2 factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
            using IDXGIDevice dxgiDevice = _device!.QueryInterface<IDXGIDevice>();

            _tearingSupported = false;

            using IDXGIFactory5? factory5 = factory.QueryInterfaceOrNull<IDXGIFactory5>();

            if (factory5 != null)
            {
                _tearingSupported = factory5.PresentAllowTearing;
            }

            SwapChainFlags flags = _tearingSupported ? SwapChainFlags.AllowTearing : SwapChainFlags.None;

            SwapChainDescription1 description = new()
            {
                Width = (uint)_width,
                Height = (uint)_height,
                Format = Format.R8G8B8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                BufferUsage = Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore,
                Flags = flags
            };

            _swapChain = factory.CreateSwapChainForHwnd(dxgiDevice, _hwnd, description);
        }

        private void CreateRenderTarget()
        {
            if (_swapChain == null || _device == null)
            {
                return;
            }

            using ID3D11Texture2D backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
            _renderTargetView = _device.CreateRenderTargetView(backBuffer);
        }

        private void CreateShaders()
        {
            string shaderPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Rendering", "DirectX", "Shaders");

            string vertexShaderPath = System.IO.Path.Combine(shaderPath, "ImageVS.cso");
            string pixelShaderPath = System.IO.Path.Combine(shaderPath, "ImagePS.cso");
            string spriteVertexShaderPath = System.IO.Path.Combine(shaderPath, "SpriteVS.cso");
            string spritePixelShaderPath = System.IO.Path.Combine(shaderPath, "SpritePS.cso");
            string lineVertexShaderPath = System.IO.Path.Combine(shaderPath, "LineVS.cso");
            string linePixelShaderPath = System.IO.Path.Combine(shaderPath, "LinePS.cso");
            string triangleVertexShaderPath = System.IO.Path.Combine(shaderPath, "TriangleVS.cso");
            string trianglePixelShaderPath = System.IO.Path.Combine(shaderPath, "TrianglePS.cso");

            if (!File.Exists(vertexShaderPath))
            {
                throw new FileNotFoundException($"Vertex shader not found: {vertexShaderPath}", vertexShaderPath);
            }

            if (!File.Exists(pixelShaderPath))
            {
                throw new FileNotFoundException($"Pixel shader not found: {pixelShaderPath}", pixelShaderPath);
            }

            if (!File.Exists(spriteVertexShaderPath))
            {
                throw new FileNotFoundException($"Sprite vertex shader not found: {spriteVertexShaderPath}", spriteVertexShaderPath);
            }

            if (!File.Exists(spritePixelShaderPath))
            {
                throw new FileNotFoundException($"Sprite pixel shader not found: {spritePixelShaderPath}", spritePixelShaderPath);
            }

            if (!File.Exists(lineVertexShaderPath))
            {
                throw new FileNotFoundException($"Line vertex shader not found: {lineVertexShaderPath}", lineVertexShaderPath);
            }

            if (!File.Exists(linePixelShaderPath))
            {
                throw new FileNotFoundException($"Line pixel shader not found: {linePixelShaderPath}", linePixelShaderPath);
            }

            if (!File.Exists(triangleVertexShaderPath))
            {
                throw new FileNotFoundException($"Triangle vertex shader not found: {triangleVertexShaderPath}", triangleVertexShaderPath);
            }

            if (!File.Exists(trianglePixelShaderPath))
            {
                throw new FileNotFoundException($"Triangle pixel shader not found: {trianglePixelShaderPath}", trianglePixelShaderPath);
            }

            byte[] vertexShaderByteCode = File.ReadAllBytes(vertexShaderPath);
            byte[] pixelShaderByteCode = File.ReadAllBytes(pixelShaderPath);
            byte[] spriteVertexShaderByteCode = File.ReadAllBytes(spriteVertexShaderPath);
            byte[] spritePixelShaderByteCode = File.ReadAllBytes(spritePixelShaderPath);
            byte[] lineVertexShaderByteCode = File.ReadAllBytes(lineVertexShaderPath);
            byte[] linePixelShaderByteCode = File.ReadAllBytes(linePixelShaderPath);
            byte[] triangleVertexShaderByteCode = File.ReadAllBytes(triangleVertexShaderPath);
            byte[] trianglePixelShaderByteCode = File.ReadAllBytes(trianglePixelShaderPath);

            _vertexShader = _device!.CreateVertexShader(vertexShaderByteCode);
            _pixelShader = _device.CreatePixelShader(pixelShaderByteCode);
            _spriteVertexShader = _device.CreateVertexShader(spriteVertexShaderByteCode);
            _spritePixelShader = _device.CreatePixelShader(spritePixelShaderByteCode);
            _lineVertexShader = _device.CreateVertexShader(lineVertexShaderByteCode);
            _linePixelShader = _device.CreatePixelShader(linePixelShaderByteCode);
            _triangleVertexShader = _device.CreateVertexShader(triangleVertexShaderByteCode);
            _trianglePixelShader = _device.CreatePixelShader(trianglePixelShaderByteCode);

            InputElementDescription[] inputElements =
            [
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
            ];

            InputElementDescription[] spriteInputElements =
            [
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 1, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
                new InputElementDescription("TEXCOORD", 2, Format.R32G32B32A32_Float, 16, 1, InputClassification.PerInstanceData, 1),
                new InputElementDescription("TEXCOORD", 3, Format.R32G32B32A32_Float, 32, 1, InputClassification.PerInstanceData, 1),
                new InputElementDescription("TEXCOORD", 4, Format.R32G32B32A32_Float, 48, 1, InputClassification.PerInstanceData, 1)
            ];

            InputElementDescription[] lineInputElements =
            [
                new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 8, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
            ];

            InputElementDescription[] triangleInputElements =
            [
                new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 8, 0)
            ];

            _inputLayout = _device.CreateInputLayout(inputElements, vertexShaderByteCode);
            _spriteInputLayout = _device.CreateInputLayout(spriteInputElements, spriteVertexShaderByteCode);
            _lineInputLayout = _device.CreateInputLayout(lineInputElements, lineVertexShaderByteCode);
            _triangleInputLayout = _device.CreateInputLayout(triangleInputElements, triangleVertexShaderByteCode);
        }

        private void CreateSampler()
        {
            SamplerDescription description = ImageFilter == DirectXImageFilter.Point ? SamplerDescription.PointClamp : SamplerDescription.LinearClamp;
            _sampler = _device!.CreateSamplerState(description);
        }

        private void CreateSpriteSampler()
        {
            SamplerDescription description = ImageFilter == DirectXImageFilter.Point ? SamplerDescription.PointClamp : SamplerDescription.LinearClamp;
            _spriteSampler = _device!.CreateSamplerState(description);
        }

        private void CreateBlendState()
        {
            if (_device == null)
            {
                return;
            }

            BlendDescription description = new();
            description.RenderTarget[0].BlendEnable = true;
            description.RenderTarget[0].SourceBlend = Blend.SourceAlpha;
            description.RenderTarget[0].DestinationBlend = Blend.InverseSourceAlpha;
            description.RenderTarget[0].BlendOperation = BlendOperation.Add;
            description.RenderTarget[0].SourceBlendAlpha = Blend.One;
            description.RenderTarget[0].DestinationBlendAlpha = Blend.InverseSourceAlpha;
            description.RenderTarget[0].BlendOperationAlpha = BlendOperation.Add;
            description.RenderTarget[0].RenderTargetWriteMask = ColorWriteEnable.All;

            _blendState?.Dispose();
            _blendState = _device.CreateBlendState(description);
        }

        private void CreateVertexBuffer()
        {
            if (_device == null || _imageWidth <= 0 || _imageHeight <= 0 || _width <= 0 || _height <= 0)
            {
                return;
            }

            float left = -1.0f;
            float right = 1.0f;
            float top = 1.0f;
            float bottom = -1.0f;

            if (StretchMode == DirectXStretchMode.Uniform || StretchMode == DirectXStretchMode.UniformToFill || StretchMode == DirectXStretchMode.None)
            {
                float imageAspect = (float)_imageWidth / _imageHeight;
                float controlAspect = (float)_width / _height;

                if (StretchMode == DirectXStretchMode.Uniform)
                {
                    if (imageAspect > controlAspect)
                    {
                        float displayHeight = controlAspect / imageAspect;
                        top = displayHeight;
                        bottom = -displayHeight;
                    }
                    else
                    {
                        float displayWidth = imageAspect / controlAspect;
                        left = -displayWidth;
                        right = displayWidth;
                    }
                }
                else if (StretchMode == DirectXStretchMode.UniformToFill)
                {
                    if (imageAspect > controlAspect)
                    {
                        float displayWidth = imageAspect / controlAspect;
                        left = -displayWidth;
                        right = displayWidth;
                    }
                    else
                    {
                        float displayHeight = controlAspect / imageAspect;
                        top = displayHeight;
                        bottom = -displayHeight;
                    }
                }
                else
                {
                    float displayWidth = _imageWidth;
                    float displayHeight = _imageHeight;

                    left = -(displayWidth / _width);
                    right = displayWidth / _width;
                    top = displayHeight / _height;
                    bottom = -(displayHeight / _height);
                }
            }

            ImageVertex[] vertices =
            [
                new ImageVertex(left, top, 0.0f, 0.0f, 0.0f),
                new ImageVertex(right, top, 0.0f, 1.0f, 0.0f),
                new ImageVertex(left, bottom, 0.0f, 0.0f, 1.0f),
                new ImageVertex(right, bottom, 0.0f, 1.0f, 1.0f)
            ];

            _vertexBuffer?.Dispose();
            _vertexBuffer = _device.CreateBuffer(vertices, BindFlags.VertexBuffer);
        }

        private void CreateSpriteVertexBuffer()
        {
            if (_device == null)
            {
                return;
            }

            SpriteVertex[] vertices =
            [
                new SpriteVertex(-0.5f, -0.5f, 0.0f, 0.0f, 0.0f),
                new SpriteVertex(0.5f, -0.5f, 0.0f, 1.0f, 0.0f),
                new SpriteVertex(-0.5f, 0.5f, 0.0f, 0.0f, 1.0f),
                new SpriteVertex(0.5f, 0.5f, 0.0f, 1.0f, 1.0f)
            ];

            _spriteVertexBuffer?.Dispose();
            _spriteVertexBuffer = _device.CreateBuffer(vertices, BindFlags.VertexBuffer);
        }

        private void CreateSpriteInstanceBuffer()
        {
            CreateSpriteInstanceBuffer(InitialSpriteInstanceCapacity);
        }

        private void CreateSpriteInstanceBuffer(int capacity)
        {
            if (_device == null)
            {
                return;
            }

            _spriteInstanceBuffer?.Dispose();
            _spriteInstanceBuffer = _device.CreateBuffer(new BufferDescription
            {
                ByteWidth = (uint)(Marshal.SizeOf<SpriteInstance>() * capacity),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write
            });

            _spriteInstanceCapacity = capacity;
        }

        private void CreateSpriteConstantBuffer()
        {
            if (_device == null)
            {
                return;
            }

            _spriteConstantBuffer?.Dispose();
            _spriteConstantBuffer = _device.CreateConstantBuffer<SpriteConstants>();
        }

        private void CreateLineVertexBuffer()
        {
            CreateLineVertexBuffer(InitialLineVertexCapacity);
        }

        private void CreateLineVertexBuffer(int capacity)
        {
            if (_device == null)
            {
                return;
            }

            _lineVertexBuffer?.Dispose();
            _lineVertexBuffer = _device.CreateBuffer(new BufferDescription
            {
                ByteWidth = (uint)(Marshal.SizeOf<LineVertex>() * capacity),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write
            });

            _lineVertexCapacity = capacity;
        }

        private void CreateLineConstantBuffer()
        {
            if (_device == null)
            {
                return;
            }

            _lineConstantBuffer?.Dispose();
            _lineConstantBuffer = _device.CreateConstantBuffer<LineConstants>();
        }

        private void CreateLineRasterizerState()
        {
            if (_device == null)
            {
                return;
            }

            _lineRasterizerState?.Dispose();

            _lineRasterizerState = _device.CreateRasterizerState(new RasterizerDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                FrontCounterClockwise = false,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                SlopeScaledDepthBias = 0.0f,
                DepthClipEnable = true,
                ScissorEnable = false,
                MultisampleEnable = false,
                AntialiasedLineEnable = false
            });
        }

        private void EnsureLineVertexCapacity(int requiredCapacity)
        {
            if (requiredCapacity <= _lineVertexCapacity)
            {
                return;
            }

            int capacity = _lineVertexCapacity > 0 ? _lineVertexCapacity : InitialLineVertexCapacity;

            while (capacity < requiredCapacity)
            {
                capacity *= 2;
            }

            CreateLineVertexBuffer(capacity);
        }

        private void CreateTriangleVertexBuffer()
        {
            CreateTriangleVertexBuffer(InitialTriangleVertexCapacity);
        }

        private void CreateTriangleVertexBuffer(int capacity)
        {
            if (_device == null)
            {
                return;
            }

            _triangleVertexBuffer?.Dispose();

            _triangleVertexBuffer = _device.CreateBuffer(new BufferDescription
            {
                ByteWidth = (uint)(Marshal.SizeOf<TriangleVertex>() * capacity),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.VertexBuffer,
                CPUAccessFlags = CpuAccessFlags.Write
            });

            _triangleVertexCapacity = capacity;
        }

        private void EnsureTriangleVertexCapacity(int requiredCapacity)
        {
            if (requiredCapacity <= _triangleVertexCapacity)
            {
                return;
            }

            int capacity = _triangleVertexCapacity > 0 ? _triangleVertexCapacity : InitialTriangleVertexCapacity;

            while (capacity < requiredCapacity)
            {
                capacity *= 2;
            }

            CreateTriangleVertexBuffer(capacity);
        }

        private void CreateTriangleConstantBuffer()
        {
            if (_device == null)
            {
                return;
            }

            _triangleConstantBuffer?.Dispose();

            _triangleConstantBuffer = _device.CreateBuffer(new BufferDescription
            {
                ByteWidth = (uint)Marshal.SizeOf<TriangleConstants>(),
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = CpuAccessFlags.Write
            });
        }

        private void DrawSpriteBatch(List<SpriteInstance> instances, ID3D11ShaderResourceView? shaderResourceView)
        {
            if (_context == null || _spriteInstanceBuffer == null || shaderResourceView == null || instances.Count == 0)
            {
                return;
            }

            _renderBatchCount++;
            _renderMaxBatchSize = System.Math.Max(_renderMaxBatchSize, instances.Count);

            bool bufferRecreated;

            using (JollyProfileScope profile = new(_profileSpriteBuffer))
            {
                bufferRecreated = EnsureSpriteInstanceCapacity(instances.Count);

                if (bufferRecreated)
                {
                    _context.IASetVertexBuffer(1, _spriteInstanceBuffer, SpriteInstanceStride, 0);
                }
            }

            using (JollyProfileScope profile = new(_profileSpriteUpload))
            {
                MappedSubresource mapped = _context.Map(_spriteInstanceBuffer, 0, MapMode.WriteDiscard);

                try
                {
                    unsafe
                    {
                        Span<SpriteInstance> destination = new((void*)mapped.DataPointer, instances.Count);
                        instances.CopyTo(destination);
                    }
                }
                finally
                {
                    _context.Unmap(_spriteInstanceBuffer, 0);
                }
            }

            using (JollyProfileScope profile = new(_profileSpriteDrawCall))
            {
                _context.PSSetShaderResource(0, shaderResourceView);
                _context.DrawInstanced(4, (uint)instances.Count, 0, 0);
                _renderDrawCallCount++;
            }
        }

        private bool EnsureSpriteInstanceCapacity(int requiredCapacity)
        {
            if (requiredCapacity <= _spriteInstanceCapacity)
            {
                return false;
            }

            int capacity = _spriteInstanceCapacity > 0 ? _spriteInstanceCapacity : InitialSpriteInstanceCapacity;

            while (capacity < requiredCapacity)
            {
                capacity *= 2;
            }

            CreateSpriteInstanceBuffer(capacity);
            return true;
        }

        public void LoadImage(string filePath)
        {
            if (!_initialized || _disposed)
            {
                throw new InvalidOperationException("DirectXRenderer is not initialized.");
            }

            DirectXImage image = DirectXImage.FromFile(filePath);
            SetImage(image);
        }

        public void SetImage(BitmapSource source)
        {
            if (!_initialized || _disposed)
            {
                throw new InvalidOperationException("DirectXRenderer is not initialized.");
            }

            ArgumentNullException.ThrowIfNull(source);

            DirectXImage image = DirectXImage.FromBitmapSource(source);
            SetImage(image);
        }

        public void SetImage(byte[] pixels, int width, int height)
        {
            if (!_initialized || _disposed)
            {
                throw new InvalidOperationException("DirectXRenderer is not initialized.");
            }

            ArgumentNullException.ThrowIfNull(pixels);

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            int requiredSize = checked(width * height * 4);

            if (pixels.Length < requiredSize)
            {
                throw new ArgumentException("Pixel buffer is too small for the specified image dimensions.", nameof(pixels));
            }

            DirectXImage image = new(width, height, pixels.AsMemory(0, requiredSize));
            SetImage(image);
        }

        public void SetImage(DirectXImage image)
        {
            ArgumentNullException.ThrowIfNull(image);

            if (!_initialized || _disposed)
            {
                throw new InvalidOperationException("DirectXRenderer is not initialized.");
            }

            if (_renderThreadRunning && !IsRenderThread)
            {
                QueueRenderCommand(() => SetImage(image));
                return;
            }

            ID3D11Texture2D? newTexture = null;
            ID3D11ShaderResourceView? newShaderResourceView = null;

            try
            {
                newTexture = CreateTexture(image);
                newShaderResourceView = _device!.CreateShaderResourceView(newTexture);

                _context?.PSSetShaderResource(0, null!);

                _shaderResourceView?.Dispose();
                _texture?.Dispose();

                _texture = newTexture;
                _shaderResourceView = newShaderResourceView;

                _imageWidth = image.Width;
                _imageHeight = image.Height;
                _hasImage = true;
                _vertexBufferDirty = true;

                newTexture = null;
                newShaderResourceView = null;
            }
            finally
            {
                newShaderResourceView?.Dispose();
                newTexture?.Dispose();
            }
        }

        private ID3D11Texture2D CreateTexture(DirectXImage image)
        {
            if (_device == null)
            {
                throw new InvalidOperationException("DirectX device has not been initialized.");
            }

            Texture2DDescription description = new()
            {
                Width = (uint)image.Width,
                Height = (uint)image.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            };

            unsafe
            {
                ReadOnlySpan<byte> pixels = image.Pixels.Span;

                fixed (byte* pixelPointer = pixels)
                {
                    SubresourceData subresourceData = new((IntPtr)pixelPointer, (uint)(image.Width * 4), 0);
                    return _device.CreateTexture2D(description, new[] { subresourceData });
                }
            }
        }

        public DirectXTexture CreateTextureResource(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Texture file not found: {filePath}", filePath);
            }

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(System.IO.Path.GetFullPath(filePath), UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmap.EndInit();
            bitmap.Freeze();

            DirectXImage image = DirectXImage.FromBitmapSource(bitmap);
            return CreateTextureResource(image);
        }

        public DirectXTexture CreateTextureResource(DirectXImage image)
        {
            ArgumentNullException.ThrowIfNull(image);

            DirectXTexture texture = new(image);

            if (!_renderThreadRunning || IsRenderThread)
            {
                AddTexture(texture);
                return texture;
            }

            using ManualResetEventSlim completed = new(false);
            Exception? exception = null;

            QueueRenderCommand(() =>
            {
                try
                {
                    AddTexture(texture);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    completed.Set();
                }
            });

            completed.Wait();

            if (exception != null)
            {
                throw new InvalidOperationException("Failed to create DirectX texture.", exception);
            }

            return texture;
        }

        internal void RemoveTexture(DirectXTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (!IsRenderThread)
            {
                QueueRenderCommand(() => RemoveTexture(texture));
                return;
            }

            if (_textureViews.Remove(texture, out ID3D11ShaderResourceView? shaderResourceView))
            {
                shaderResourceView.Dispose();
                _textures.Remove(texture);
            }
        }

        internal void UpdateTexture(DirectXTexture texture, DirectXImage image)
        {
            ArgumentNullException.ThrowIfNull(texture);
            ArgumentNullException.ThrowIfNull(image);

            if (!_renderThreadRunning || IsRenderThread)
            {
                UpdateTextureOnRenderThread(texture, image);
                return;
            }

            QueueRenderCommand(() => UpdateTextureOnRenderThread(texture, image));
        }

        private void UpdateTextureOnRenderThread(DirectXTexture texture, DirectXImage image)
        {
            if (_device == null)
            {
                throw new InvalidOperationException("DirectX device has not been initialized.");
            }

            if (!_textureViews.ContainsKey(texture))
            {
                throw new InvalidOperationException("The texture is not registered with this renderer.");
            }

            ID3D11Texture2D gpuTexture = CreateTexture(image);
            ID3D11ShaderResourceView newShaderResourceView;

            try
            {
                newShaderResourceView = _device.CreateShaderResourceView(gpuTexture);
            }
            finally
            {
                gpuTexture.Dispose();
            }

            ID3D11ShaderResourceView oldShaderResourceView = _textureViews[texture];
            _textureViews[texture] = newShaderResourceView;
            texture.Image = image;
            oldShaderResourceView.Dispose();
        }

        internal void AddTexture(DirectXTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (!IsRenderThread)
            {
                QueueRenderCommand(() => AddTexture(texture));
                return;
            }

            if (_device == null)
            {
                throw new InvalidOperationException("DirectX device has not been initialized.");
            }

            if (_textureViews.ContainsKey(texture))
            {
                return;
            }

            ID3D11Texture2D gpuTexture = CreateTexture(texture.Image);
            ID3D11ShaderResourceView shaderResourceView = _device.CreateShaderResourceView(gpuTexture);

            gpuTexture.Dispose();

            _textureViews.Add(texture, shaderResourceView);
            _textures.Add(texture);
        }

        public void SubmitRenderFrame(JollyRenderFrame frame, Camera2D camera)
        {
            ArgumentNullException.ThrowIfNull(frame);
            ArgumentNullException.ThrowIfNull(camera);

            camera.SetViewport(_width, _height);

            JollyRenderSubmission submission = new(frame, camera);

            Interlocked.Exchange(ref _latestRenderSubmission, submission);
        }

        public void Clear()
        {
            if (_renderThreadRunning && !IsRenderThread)
            {
                QueueRenderCommand(Clear);
                return;
            }

            _context?.PSSetShaderResource(0, null!);

            _shaderResourceView?.Dispose();
            _shaderResourceView = null;

            _texture?.Dispose();
            _texture = null;

            _imageWidth = 0;
            _imageHeight = 0;
            _hasImage = false;
            _vertexBufferDirty = true;
        }

        public void ClearScreen()
        {
            if (_renderThreadRunning && !IsRenderThread)
            {
                QueueRenderCommand(ClearScreen);
                return;
            }

            if (!_initialized || _disposed || _context == null || _renderTargetView == null)
            {
                return;
            }

            MediaColor color = _clearColor;

            _context.ClearRenderTargetView(_renderTargetView, new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f));
        }

        public void Render()
        {
            _renderBatchCount = 0;
            _renderDrawCallCount = 0;
            _renderMaxBatchSize = 0;

            _diagnostics?.BeginFrame();

            try
            {
                UpdateFps();

                if (!_initialized || _disposed)
                {
                    return;
                }

                if (_context == null || _swapChain == null || _renderTargetView == null)
                {
                    return;
                }

                using (JollyDiagnosticScope renderProfile = _diagnostics == null ? default : _diagnostics.MeasureRender())
                {
                    JollyRenderSubmission? submission = Volatile.Read(ref _latestRenderSubmission);
                    using (JollyProfileScope profile = new(_profileRenderSetup))
                    {
                        _context.OMSetRenderTargets(_renderTargetView);

                        if (submission != null && submission.Camera.ViewportScale != 1.0f)
                        {
                            CameraState camera = submission.Camera;

                            float viewportWidth = camera.ViewportSize.X * camera.ViewportScale;
                            float viewportHeight = camera.ViewportSize.Y * camera.ViewportScale;

                            _context.RSSetViewport(new Viewport(camera.ViewportOffset.X, camera.ViewportOffset.Y, viewportWidth, viewportHeight, 0.0f, 1.0f));
                        }
                        else
                        {
                            _context.RSSetViewport(new Viewport(0, 0, _width, _height, 0.0f, 1.0f));
                        }
                    }

                    if (_clearEveryFrame)
                    {
                        using (JollyProfileScope profile = new(_profileRenderClear))
                        {
                            MediaColor color = _clearColor;
                            _context.ClearRenderTargetView(_renderTargetView, new Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f));
                        }
                    }

                    if (_hasImage && _shaderResourceView != null)
                    {
                        using (JollyProfileScope profile = new(_profileRenderImage))
                        {
                            RenderImage();
                        }
                    }

                    if (submission != null)
                    {
                        JollyRenderFrame renderFrame = submission.Frame;
                        CameraState camera = submission.Camera;

                        using (JollyProfileScope profile = new(_profileRenderTriangles))
                        {
                            RenderTriangles(renderFrame, camera);
                        }

                        using (JollyProfileScope profile = new(_profileRenderLines))
                        {
                            RenderLines(renderFrame, camera);
                        }

                        using (JollyProfileScope profile = new(_profileRenderSprites))
                        {
                            RenderSprites(renderFrame, camera, true);
                            RenderSprites(renderFrame, camera, false);
                        }
                    }

                    using (JollyProfileScope profile = new(_profileRenderPresent))
                    {
                        PresentFlags presentFlags = !VSync && _tearingSupported ? PresentFlags.AllowTearing : PresentFlags.None;
                        _swapChain.Present(VSync ? 1u : 0u, presentFlags);
                    }
                }
            }
            finally
            {
                _diagnostics?.SetRenderBatchCount(_renderBatchCount);
                _diagnostics?.SetRenderDrawCallCount(_renderDrawCallCount);
                _diagnostics?.SetRenderMaxBatchSize(_renderMaxBatchSize);
                _diagnostics?.EndFrame();
                _diagnostics?.RecordRender();
            }
        }

        private void RenderImage()
        {
            if (_context == null || _inputLayout == null || _vertexShader == null || _pixelShader == null || _shaderResourceView == null || _sampler == null)
            {
                return;
            }

            if (_vertexBufferDirty || _vertexBuffer == null)
            {
                CreateVertexBuffer();
                _vertexBufferDirty = false;
            }

            if (_vertexBuffer == null)
            {
                return;
            }

            _context.OMSetBlendState(_blendState);
            _context.IASetInputLayout(_inputLayout);
            _context.IASetVertexBuffer(0, _vertexBuffer, 20);
            _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
            _context.VSSetShader(_vertexShader);
            _context.PSSetShader(_pixelShader);
            _context.PSSetShaderResource(0, _shaderResourceView);
            _context.PSSetSampler(0, _sampler);
            _context.Draw(4, 0);
        }

        private void RenderLines(JollyRenderFrame frame, CameraState camera)
        {
            if (_context == null || _lineVertexBuffer == null || _lineConstantBuffer == null || _lineInputLayout == null || _lineVertexShader == null || _linePixelShader == null)
            {
                return;
            }

            if (frame.Lines.Count == 0)
            {
                return;
            }

            _lineVertices.Clear();

            Vector2 cameraPosition = camera.Position;
            float cameraZoom = camera.Zoom;
            float cameraRotation = camera.Rotation;
            float viewportCenterX = camera.ViewportOffset.X + camera.ViewportSize.X * 0.5f;
            float viewportCenterY = camera.ViewportOffset.Y + camera.ViewportSize.Y * 0.5f;
            float effectiveZoom = cameraZoom * camera.ViewportScale;

            foreach (JollyLineSnapshot line in frame.Lines)
            {
                float glowWidth = _lineGlowEnabled && line.GlowEnabled ? _lineGlowWidth * effectiveZoom : 0.0f;

                Vector2 start = new(line.Start.X - cameraPosition.X, line.Start.Y - cameraPosition.Y);
                Vector2 end = new(line.End.X - cameraPosition.X, line.End.Y - cameraPosition.Y);

                if (cameraRotation != 0.0f)
                {
                    Matrix3x2 rotation = Matrix3x2.CreateRotation(-cameraRotation);
                    start = Vector2.Transform(start, rotation);
                    end = Vector2.Transform(end, rotation);
                }

                start *= effectiveZoom;
                end *= effectiveZoom;

                start += new Vector2(viewportCenterX, viewportCenterY);
                end += new Vector2(viewportCenterX, viewportCenterY);

                Vector2 direction = end - start;

                if (direction.LengthSquared() <= 0.000001f)
                {
                    continue;
                }

                direction = Vector2.Normalize(direction);

                Vector2 normal = new(-direction.Y, direction.X);

                float baseHalfThickness = MathF.Abs(line.Thickness * effectiveZoom) * 0.5f;

                float startCoreHalfThickness = baseHalfThickness * line.StartThickness;
                float endCoreHalfThickness = baseHalfThickness * line.EndThickness;

                float startOuterHalfThickness = startCoreHalfThickness + glowWidth;
                float endOuterHalfThickness = endCoreHalfThickness + glowWidth;

                if (startOuterHalfThickness < 0.0001f && endOuterHalfThickness < 0.0001f)
                {
                    continue;
                }

                float startCoreRatio = startOuterHalfThickness > 0.0001f ? System.Math.Clamp(startCoreHalfThickness / startOuterHalfThickness, 0.0f, 1.0f) : 1.0f;
                float endCoreRatio = endOuterHalfThickness > 0.0001f ? System.Math.Clamp(endCoreHalfThickness / endOuterHalfThickness, 0.0f, 1.0f) : 1.0f;

                Vector2 startOffset = normal * startOuterHalfThickness;
                Vector2 endOffset = normal * endOuterHalfThickness;

                Vector2 p0 = start - startOffset;
                Vector2 p1 = start + startOffset;
                Vector2 p2 = end + endOffset;
                Vector2 p3 = end - endOffset;

                _lineVertices.Add(new LineVertex(p0.X, p0.Y, line.Color, -1.0f, startCoreRatio));
                _lineVertices.Add(new LineVertex(p1.X, p1.Y, line.Color, 1.0f, startCoreRatio));
                _lineVertices.Add(new LineVertex(p2.X, p2.Y, line.Color, 1.0f, endCoreRatio));
                _lineVertices.Add(new LineVertex(p0.X, p0.Y, line.Color, -1.0f, startCoreRatio));
                _lineVertices.Add(new LineVertex(p2.X, p2.Y, line.Color, 1.0f, endCoreRatio));
                _lineVertices.Add(new LineVertex(p3.X, p3.Y, line.Color, -1.0f, endCoreRatio));
            }

            if (_lineVertices.Count == 0)
            {
                return;
            }

            EnsureLineVertexCapacity(_lineVertices.Count);

            MappedSubresource mapped = _context.Map(_lineVertexBuffer, 0, MapMode.WriteDiscard);

            try
            {
                unsafe
                {
                    Span<LineVertex> destination = new((void*)mapped.DataPointer, _lineVertices.Count);
                    _lineVertices.CopyTo(destination);
                }
            }
            finally
            {
                _context.Unmap(_lineVertexBuffer, 0);
            }

            _lineConstantBuffer.SetData(_context, new LineConstants
            {
                ViewportWidth = _width,
                ViewportHeight = _height,
                GlowStrength = _lineGlowStrength,
                GlowFalloff = _lineGlowFalloff,
                GlowEnabled = _lineGlowEnabled ? 1.0f : 0.0f
            }, MapMode.WriteDiscard);

            _context.OMSetBlendState(_blendState);
            _context.OMSetDepthStencilState(null);
            _context.RSSetState(_lineRasterizerState);
            _context.IASetInputLayout(_lineInputLayout);
            _context.IASetVertexBuffer(0, _lineVertexBuffer, LineVertexStride, 0);
            _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            _context.VSSetConstantBuffer(0, _lineConstantBuffer);
            _context.PSSetConstantBuffer(0, _lineConstantBuffer);
            _context.VSSetShader(_lineVertexShader);
            _context.PSSetShader(_linePixelShader);
            _context.Draw((uint)_lineVertices.Count, 0);

            _renderDrawCallCount++;
        }

        private void RenderTriangles(JollyRenderFrame frame, CameraState camera)
        {
            if (_context == null || _triangleVertexBuffer == null || _triangleInputLayout == null || _triangleVertexShader == null || _trianglePixelShader == null)
            {
                return;
            }

            if (frame.Triangles.Count == 0)
            {
                return;
            }

            _triangleVertices.Clear();

            Vector2 cameraPosition = camera.Position;
            float cameraRotation = camera.Rotation;
            float viewportCenterX = camera.ViewportOffset.X + camera.ViewportSize.X * 0.5f;
            float viewportCenterY = camera.ViewportOffset.Y + camera.ViewportSize.Y * 0.5f;
            float effectiveZoom = camera.Zoom * camera.ViewportScale;

            foreach (JollyTriangleSnapshot triangle in frame.Triangles)
            {
                Vector2 a = triangle.A - cameraPosition;
                Vector2 b = triangle.B - cameraPosition;
                Vector2 c = triangle.C - cameraPosition;

                if (cameraRotation != 0.0f)
                {
                    Matrix3x2 rotation = Matrix3x2.CreateRotation(-cameraRotation);
                    a = Vector2.Transform(a, rotation);
                    b = Vector2.Transform(b, rotation);
                    c = Vector2.Transform(c, rotation);
                }

                a = a * effectiveZoom + new Vector2(viewportCenterX, viewportCenterY);
                b = b * effectiveZoom + new Vector2(viewportCenterX, viewportCenterY);
                c = c * effectiveZoom + new Vector2(viewportCenterX, viewportCenterY);

                _triangleVertices.Add(new TriangleVertex(a.X, a.Y, triangle.Colour));
                _triangleVertices.Add(new TriangleVertex(b.X, b.Y, triangle.Colour));
                _triangleVertices.Add(new TriangleVertex(c.X, c.Y, triangle.Colour));
            }

            if (_triangleVertices.Count == 0)
            {
                return;
            }

            EnsureTriangleVertexCapacity(_triangleVertices.Count);

            MappedSubresource mapped = _context.Map(_triangleVertexBuffer, 0, MapMode.WriteDiscard);

            try
            {
                unsafe
                {
                    Span<TriangleVertex> destination = new((void*)mapped.DataPointer, _triangleVertices.Count);
                    _triangleVertices.CopyTo(destination);
                }
            }
            finally
            {
                _context.Unmap(_triangleVertexBuffer, 0);
            }

            _triangleConstantBuffer!.SetData(_context, new TriangleConstants
            {
                ViewportWidth = _width,
                ViewportHeight = _height
            }, MapMode.WriteDiscard);

            _context.OMSetBlendState(_blendState);
            _context.OMSetDepthStencilState(null);
            _context.IASetInputLayout(_triangleInputLayout);
            _context.IASetVertexBuffer(0, _triangleVertexBuffer, TriangleVertexStride, 0);
            _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            _context.VSSetConstantBuffer(0, _triangleConstantBuffer);
            _context.VSSetShader(_triangleVertexShader);
            _context.PSSetShader(_trianglePixelShader);
            _context.Draw((uint)_triangleVertices.Count, 0);

            _renderDrawCallCount++;
        }

        private void RenderSprites(JollyRenderFrame frame, CameraState camera, bool backgroundPass)
        {
            if (_context == null || _spriteVertexBuffer == null || _spriteInstanceBuffer == null || _spriteInputLayout == null || _spriteVertexShader == null || _spritePixelShader == null || _spriteConstantBuffer == null)
            {
                return;
            }

            _renderSpriteCount = 0;

            Vector2 cameraPosition = camera.Position;
            float cameraZoom = camera.Zoom * camera.ViewportScale;
            float cameraRotation = camera.Rotation;
            float viewportCenterX = camera.ViewportOffset.X + camera.ViewportSize.X * 0.5f;
            float viewportCenterY = camera.ViewportOffset.Y + camera.ViewportSize.Y * 0.5f;

            using (JollyProfileScope profile = new(_profileSpriteSetup))
            {
                if (_spriteConstantBufferWidth != _width || _spriteConstantBufferHeight != _height)
                {
                    _spriteConstantBuffer.SetData(_context, new SpriteConstants
                    {
                        ViewportWidth = _width,
                        ViewportHeight = _height
                    }, MapMode.WriteDiscard);

                    _spriteConstantBufferWidth = _width;
                    _spriteConstantBufferHeight = _height;
                }

                _context.OMSetBlendState(_blendState);
                _context.OMSetDepthStencilState(null);
                _context.IASetInputLayout(_spriteInputLayout);
                _context.IASetVertexBuffer(0, _spriteVertexBuffer, SpriteVertexStride, 0);
                _context.IASetVertexBuffer(1, _spriteInstanceBuffer, SpriteInstanceStride, 0);
                _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
                _context.VSSetConstantBuffer(0, _spriteConstantBuffer);
                _context.VSSetShader(_spriteVertexShader);
                _context.PSSetShader(_spritePixelShader);
                _context.PSSetSampler(0, _spriteSampler);
            }

            List<SpriteInstance> instances = _spriteBatchInstances;
            instances.Clear();

            DirectXTexture? batchTexture = null;
            ID3D11ShaderResourceView? batchShaderResourceView = null;

            using (JollyProfileScope profile = new(_profileSpritePrepare))
            {
                foreach (JollySpriteSnapshot sprite in frame.Sprites.OrderBy(sprite => sprite.Layer))
                {
                    if (!sprite.Visible)
                    {
                        continue;
                    }

                    if (backgroundPass && sprite.Layer >= 0)
                    {
                        continue;
                    }

                    if (!backgroundPass && sprite.Layer < 0)
                    {
                        continue;
                    }

                    _renderSpriteCount++;

                    if (!IsSpriteVisible(sprite, camera))
                    {
                        continue;
                    }

                    if (sprite.Texture.RendererTexture is not DirectXTexture texture)
                    {
                        continue;
                    }

                    ID3D11ShaderResourceView? shaderResourceView;

                    if (ReferenceEquals(batchTexture, texture))
                    {
                        shaderResourceView = batchShaderResourceView;
                    }
                    else if (!_textureViews.TryGetValue(texture, out shaderResourceView))
                    {
                        continue;
                    }

                    if (batchTexture != null && !ReferenceEquals(batchTexture, texture))
                    {
                        using (JollyProfileScope drawProfile = new(_profileSpriteDraw))
                        {
                            DrawSpriteBatch(instances, batchShaderResourceView);
                        }

                        instances.Clear();
                    }

                    batchTexture = texture;
                    batchShaderResourceView = shaderResourceView;

                    Vector2 relativePosition = new(sprite.X - cameraPosition.X, sprite.Y - cameraPosition.Y);

                    if (cameraRotation != 0.0f)
                    {
                        relativePosition = Vector2.Transform(relativePosition, Matrix3x2.CreateRotation(-cameraRotation));
                    }

                    float screenX = relativePosition.X * cameraZoom + viewportCenterX;
                    float screenY = relativePosition.Y * cameraZoom + viewportCenterY;
                    float scaledWidth = sprite.Width * sprite.Scale.X * cameraZoom;
                    float scaledHeight = sprite.Height * sprite.Scale.Y * cameraZoom;
                    float finalRotation = sprite.Rotation * DegreesToRadians - cameraRotation;

                    instances.Add(new SpriteInstance
                    {
                        PositionX = screenX,
                        PositionY = screenY,
                        Width = scaledWidth,
                        Height = scaledHeight,
                        Rotation = finalRotation,
                        Opacity = sprite.Opacity,
                        FlipHorizontal = sprite.FlipHorizontal ? 1.0f : 0.0f,
                        FlipVertical = sprite.FlipVertical ? 1.0f : 0.0f,
                        SourceX = sprite.SourceX,
                        SourceY = sprite.SourceY,
                        SourceWidth = sprite.SourceWidth,
                        SourceHeight = sprite.SourceHeight,
                        Padding0 = sprite.Color.R,
                        Padding1 = sprite.Color.G,
                        Padding2 = sprite.Color.B,
                        Padding3 = sprite.Color.A
                    });
                }
            }

            if (instances.Count > 0)
            {
                using (JollyProfileScope drawProfile = new(_profileSpriteDraw))
                {
                    DrawSpriteBatch(instances, batchShaderResourceView);
                }
            }

            _diagnostics?.SetSpriteCount(_renderSpriteCount);
            _diagnostics?.SetRenderBatchCount(_renderBatchCount);
            _diagnostics?.SetRenderDrawCallCount(_renderDrawCallCount);
            _diagnostics?.SetRenderMaxBatchSize(_renderMaxBatchSize);
            _diagnostics?.SetTextureCount(_textureViews.Count);
        }

        private bool IsSpriteVisible(JollySpriteSnapshot sprite, CameraState camera)
        {
            float halfWidth = MathF.Abs(sprite.Width * sprite.Scale.X) * 0.5f;
            float halfHeight = MathF.Abs(sprite.Height * sprite.Scale.Y) * 0.5f;

            Vector2 center = new(sprite.X, sprite.Y);

            Vector2[] corners =
            [
                new(-halfWidth, -halfHeight),
                new(halfWidth, -halfHeight),
                new(halfWidth, halfHeight),
                new(-halfWidth, halfHeight)
            ];

            Matrix3x2 spriteRotation = Matrix3x2.CreateRotation(sprite.Rotation * DegreesToRadians);
            Matrix3x2 cameraRotation = Matrix3x2.CreateRotation(-camera.Rotation);

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 worldPosition = center + Vector2.Transform(corners[i], spriteRotation);
                Vector2 relativePosition = worldPosition - camera.Position;
                relativePosition = Vector2.Transform(relativePosition, cameraRotation);

                float effectiveZoom = camera.Zoom * camera.ViewportScale;

                float screenX = relativePosition.X * effectiveZoom + camera.ViewportOffset.X + camera.ViewportSize.X * 0.5f;
                float screenY = relativePosition.Y * effectiveZoom + camera.ViewportOffset.Y + camera.ViewportSize.Y * 0.5f;

                minX = MathF.Min(minX, screenX);
                maxX = MathF.Max(maxX, screenX);
                minY = MathF.Min(minY, screenY);
                maxY = MathF.Max(maxY, screenY);
            }

            float viewportLeft = camera.ViewportOffset.X;
            float viewportTop = camera.ViewportOffset.Y;
            float viewportRight = viewportLeft + camera.ViewportSize.X * camera.ViewportScale;
            float viewportBottom = viewportTop + camera.ViewportSize.Y * camera.ViewportScale;

            if (maxX < viewportLeft || minX > viewportRight)
            {
                return false;
            }

            if (maxY < viewportTop || minY > viewportBottom)
            {
                return false;
            }

            return true;
        }

        public ID3D11RenderTargetView? Get_renderTargetView1()
        {
            return _renderTargetView;
        }

        public void Resize(int width, int height, ID3D11RenderTargetView? _renderTargetView1)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            if (_renderThreadRunning && !IsRenderThread)
            {
                QueueRenderCommand(() => Resize(width, height, Get_renderTargetView1()));
                return;
            }

            if (!_initialized || _disposed || _swapChain == null || _context == null)
            {
                return;
            }

            if (width == _width && height == _height)
            {
                return;
            }

            _context.OMSetRenderTargets(Array.Empty<ID3D11RenderTargetView>());

            _context.Flush();

            _renderTargetView?.Dispose();
            _renderTargetView = null;

            _swapChain.ResizeBuffers(2, (uint)width, (uint)height, Format.R8G8B8A8_UNorm, _tearingSupported ? SwapChainFlags.AllowTearing : SwapChainFlags.None);

            _width = width;
            _height = height;

            CreateRenderTarget();

            _vertexBufferDirty = true;
            _spriteConstantBufferWidth = 0.0f;
            _spriteConstantBufferHeight = 0.0f;

            _context.OMSetRenderTargets(renderTargetView: _renderTargetView);
            _context.RSSetViewport(new Viewport(0, 0, _width, _height, 0.0f, 1.0f));
        }

        private void UpdateFps()
        {
            long timestamp = Stopwatch.GetTimestamp();

            if (_fpsLastTimestamp == 0)
            {
                _fpsLastTimestamp = timestamp;
                _fpsFrameCount = 0;
                return;
            }

            _fpsFrameCount++;

            double elapsed = (timestamp - _fpsLastTimestamp) / (double)Stopwatch.Frequency;

            if (elapsed >= 1.0)
            {
                double fps = _fpsFrameCount / elapsed;

                Volatile.Write(ref _fps, fps);
                Volatile.Write(ref _frameTime, fps > 0.0 ? 1000.0 / fps : 0.0);

                _fpsFrameCount = 0;
                _fpsLastTimestamp = timestamp;
            }
        }

        private readonly struct ImageVertex
        {
            public readonly float X;
            public readonly float Y;
            public readonly float Z;
            public readonly float U;
            public readonly float V;

            public ImageVertex(float x, float y, float z, float u, float v)
            {
                X = x;
                Y = y;
                Z = z;
                U = u;
                V = v;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            StopRenderLoop();

            _disposed = true;

            if (_context != null)
            {
                _context.ClearState();
                _context.Flush();
            }

            _shaderResourceView?.Dispose();
            _shaderResourceView = null;

            _texture?.Dispose();
            _texture = null;

            _vertexBuffer?.Dispose();
            _vertexBuffer = null;

            _sampler?.Dispose();
            _sampler = null;

            _blendState?.Dispose();
            _blendState = null;

            _inputLayout?.Dispose();
            _inputLayout = null;

            _vertexShader?.Dispose();
            _vertexShader = null;

            _pixelShader?.Dispose();
            _pixelShader = null;

            _renderTargetView?.Dispose();
            _renderTargetView = null;

            _swapChain?.Dispose();
            _swapChain = null;

            _spriteConstantBuffer?.Dispose();
            _spriteConstantBuffer = null;

            _spriteVertexBuffer?.Dispose();
            _spriteVertexBuffer = null;

            _spriteInstanceBuffer?.Dispose();
            _spriteInstanceBuffer = null;

            _spriteSampler?.Dispose();
            _spriteSampler = null;

            _spriteInputLayout?.Dispose();
            _spriteInputLayout = null;

            _spriteVertexShader?.Dispose();
            _spriteVertexShader = null;

            _spritePixelShader?.Dispose();
            _spritePixelShader = null;

            _lineVertexBuffer?.Dispose();
            _lineVertexBuffer = null;

            _lineConstantBuffer?.Dispose();
            _lineConstantBuffer = null;

            _lineInputLayout?.Dispose();
            _lineInputLayout = null;

            _lineVertexShader?.Dispose();
            _lineVertexShader = null;

            _linePixelShader?.Dispose();
            _linePixelShader = null;

            _lineRasterizerState?.Dispose();
            _lineRasterizerState = null;

            foreach (ID3D11ShaderResourceView shaderResourceView in _textureViews.Values)
            {
                shaderResourceView.Dispose();
            }

            _textureViews.Clear();
            _spriteTextureViews.Clear();
            _textures.Clear();
            _renderCommands.Clear();
            _lineVertices.Clear();
            _spriteBatchInstances.Clear();

            _context?.Dispose();
            _context = null;

            _device?.Dispose();
            _device = null;

            _initialized = false;
        }
    }
}