using Jolly2DEngine.Diagnostics;
using Jolly2DEngine.Input;
using Jolly2DEngine.Scene;
using System.Diagnostics;

namespace Jolly2DEngine.Core
{
    public abstract class JollyGame : IDisposable
    {
        private Thread? _gameThread;
        private volatile bool _running;
        private bool _disposed;
        private double _gameLoopTargetFPS = 0.0;
        private double _gameLoopActualFPS;
        private double _gameLoopFpsAccumulator;
        private int _gameLoopFpsFrameCount;
        private long _nextGameLoopTimestamp;
        private double _renderFPS;
  
        private JollyProfile? _profileScenes;
        private JollyProfile? _profileGame;

        protected JollyGame()
        {
            Scenes = new JollySceneManager();
            Input = new JollyInput();
            Diagnostics = new JollyDiagnostics();
            _profileScenes = Diagnostics.RegisterProfile("Scenes");
            _profileGame = Diagnostics.RegisterProfile("Game");
        }

        public JollySceneManager Scenes { get; }

        public JollyInput Input { get; }

        public JollyGameTime GameTime { get; } = new JollyGameTime();

        public JollyDiagnostics Diagnostics { get; }

        public double GameLoopFPS
        {
            get => Volatile.Read(ref _gameLoopTargetFPS);
            set
            {
                double newValue = System.Math.Max(0.0, value);
                Volatile.Write(ref _gameLoopTargetFPS, newValue);
                _nextGameLoopTimestamp = Stopwatch.GetTimestamp();
            }
        }

        public double GameLoopActualFPS => Volatile.Read(ref _gameLoopActualFPS);

        public double GameFrameTime => GameTime.FrameTime;

        public double RenderFPS
        {
            get => Volatile.Read(ref _renderFPS);
            set
            {
                double newValue = System.Math.Max(0.0, value);
                Volatile.Write(ref _renderFPS, newValue);
                OnRenderFPSChanged(newValue);
            }
        }

        public bool IsRunning => _running;

        public void Start()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_running)
            {
                return;
            }

            _running = true;
            _gameThread = new Thread(GameLoop);
            _gameThread.IsBackground = true;
            _gameThread.Name = "Jolly2D Game Thread";
            _gameThread.Priority = ThreadPriority.AboveNormal;
            _gameThread.Start();
        }

        public void Stop()
        {
            _running = false;

            if (_gameThread != null && _gameThread.IsAlive && !ReferenceEquals(Thread.CurrentThread, _gameThread))
            {
                _gameThread.Join();
            }

            _gameThread = null;
        }

        private void GameLoop()
        {
            OnInitialize();

            long previousTimestamp = Stopwatch.GetTimestamp();
            long nextTimestamp = previousTimestamp;
            _nextGameLoopTimestamp = previousTimestamp;

            double totalSeconds = 0.0;
            long frameCount = 0;

            _gameLoopFpsAccumulator = 0.0;
            _gameLoopFpsFrameCount = 0;
            _gameLoopActualFPS = 0.0;

            while (_running)
            {
                double targetFPS = Volatile.Read(ref _gameLoopTargetFPS);

                if (targetFPS > 0.0)
                {
                    double secondsPerFrame = 1.0 / targetFPS;
                    long targetTicks = System.Math.Max(1L, (long)(Stopwatch.Frequency * secondsPerFrame));

                    long currentTimestamp = Stopwatch.GetTimestamp();

                    if (nextTimestamp <= currentTimestamp)
                    {
                        nextTimestamp = currentTimestamp;
                    }

                    WaitForGameLoopTimestamp(nextTimestamp);

                    if (!_running)
                    {
                        break;
                    }

                    long timestamp = Stopwatch.GetTimestamp();

                    if (timestamp - nextTimestamp > Stopwatch.Frequency)
                    {
                        nextTimestamp = timestamp;
                    }
                    else
                    {
                        nextTimestamp += targetTicks;
                    }

                    previousTimestamp = timestamp;
                }
                else
                {
                    long timestamp = Stopwatch.GetTimestamp();
                    previousTimestamp = timestamp;
                }

                long frameTimestamp = Stopwatch.GetTimestamp();
                double deltaSeconds = (frameTimestamp - previousTimestamp) / (double)Stopwatch.Frequency;

                if (targetFPS > 0.0)
                {
                    deltaSeconds = 1.0 / targetFPS;
                }

                previousTimestamp = frameTimestamp;

                if (deltaSeconds < 0.0)
                {
                    deltaSeconds = 0.0;
                }

                if (deltaSeconds > 0.25)
                {
                    deltaSeconds = 0.25;
                }

                totalSeconds += deltaSeconds;
                frameCount++;

                GameTime.DeltaSeconds = deltaSeconds;
                GameTime.TotalSeconds = totalSeconds;
                GameTime.FrameCount = frameCount;

                _gameLoopFpsAccumulator += deltaSeconds;
                _gameLoopFpsFrameCount++;

                if (_gameLoopFpsAccumulator >= 0.25)
                {
                    double actualFPS = _gameLoopFpsFrameCount / _gameLoopFpsAccumulator;
                    Volatile.Write(ref _gameLoopActualFPS, actualFPS);
                    GameTime.FPS = actualFPS;
                    _gameLoopFpsAccumulator = 0.0;
                    _gameLoopFpsFrameCount = 0;
                }

                using (JollyDiagnosticScope updateProfile = Diagnostics.MeasureUpdate())
                {
                    using (JollyProfileScope profile = new(_profileScenes))
                    {
                        Scenes.Update(GameTime);
                    }

                    using (JollyProfileScope profile = new(_profileGame))
                    {
                        OnUpdate(GameTime);
                    }
                }

                Diagnostics.SetEntityCount(Scenes.EntityCount);
                Diagnostics.RecordUpdate();
                Input.EndFrame();
            }

            OnShutdown();
            Scenes.Shutdown();
        }

        private void WaitForGameLoopTimestamp(long targetTimestamp)
        {
            while (_running)
            {
                long currentTimestamp = Stopwatch.GetTimestamp();
                long remainingTicks = targetTimestamp - currentTimestamp;

                if (remainingTicks <= 0)
                {
                    return;
                }

                double remainingMilliseconds = remainingTicks * 1000.0 / Stopwatch.Frequency;

                if (remainingMilliseconds > 2.0)
                {
                    Thread.Sleep(System.Math.Max(1, (int)(remainingMilliseconds - 1.0)));
                }
                else
                {
                    Thread.SpinWait(100);
                }
            }
        }

        protected virtual void OnRenderFPSChanged(double renderFPS)
        {
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnUpdate(JollyGameTime gameTime)
        {
        }

        protected virtual void OnShutdown()
        {
        }

        void IDisposable.Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Stop();
            _disposed = true;
        }
    }
}
