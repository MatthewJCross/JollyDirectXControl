using System.Diagnostics;

namespace Jolly2DEngine.Diagnostics
{
    public sealed class JollyDiagnostics
    {
        private readonly object _sync = new();
        private readonly object _profileLock = new();

        private readonly Stopwatch _fpsTimer = Stopwatch.StartNew();
        private readonly Stopwatch _frameStopwatch = new();

        private readonly Dictionary<string, JollyProfile> _profiles = new(StringComparer.Ordinal);

        private long _frameCount;
        private long _updateCount;
        private long _renderCount;
        private long _framesAtLastSample;

        private double _fps;
        private double _frameTimeMilliseconds;
        private double _updateTimeMilliseconds;
        private double _renderTimeMilliseconds;

        private int _entityCount;
        private int _spriteCount;
        private int _textureCount;

        private int _renderBatchCount;
        private int _renderDrawCallCount;
        private int _renderMaxBatchSize;

        private int _connectedClients;

        private long _networkMessagesSent;
        private long _networkMessagesReceived;
        private long _networkBytesSent;
        private long _networkBytesReceived;

        private double _networkLatencyMilliseconds;

        public void BeginFrame()
        {
            Interlocked.Increment(ref _frameCount);
            UpdateFps();
            _frameStopwatch.Restart();
        }

        public void EndFrame()
        {
            SetFrameTime(_frameStopwatch.Elapsed.TotalMilliseconds);
        }

        public JollyDiagnosticScope MeasureUpdate()
        {
            return new JollyDiagnosticScope(this, JollyDiagnosticScopeType.Update);
        }

        public JollyDiagnosticScope MeasureRender()
        {
            return new JollyDiagnosticScope(this, JollyDiagnosticScopeType.Render);
        }

        public void SetEntityCount(int count)
        {
            Interlocked.Exchange(ref _entityCount, System.Math.Max(0, count));
        }

        public void SetSpriteCount(int count)
        {
            Interlocked.Exchange(ref _spriteCount, System.Math.Max(0, count));
        }

        public void SetTextureCount(int count)
        {
            Interlocked.Exchange(ref _textureCount, System.Math.Max(0, count));
        }

        public void SetRenderBatchCount(int count)
        {
            Volatile.Write(ref _renderBatchCount, System.Math.Max(0, count));
        }

        public void SetRenderDrawCallCount(int count)
        {
            Volatile.Write(ref _renderDrawCallCount, System.Math.Max(0, count));
        }

        public void SetRenderMaxBatchSize(int count)
        {
            Volatile.Write(ref _renderMaxBatchSize, System.Math.Max(0, count));
        }

        public void SetConnectedClients(int count)
        {
            Interlocked.Exchange(ref _connectedClients, System.Math.Max(0, count));
        }

        public void RecordNetworkMessageSent(int byteCount)
        {
            Interlocked.Increment(ref _networkMessagesSent);
            Interlocked.Add(ref _networkBytesSent, System.Math.Max(0, byteCount));
        }

        public void RecordNetworkMessageReceived(int byteCount)
        {
            Interlocked.Increment(ref _networkMessagesReceived);
            Interlocked.Add(ref _networkBytesReceived, System.Math.Max(0, byteCount));
        }

        public void SetNetworkLatency(double milliseconds)
        {
            lock (_sync)
            {
                _networkLatencyMilliseconds = System.Math.Max(0.0, milliseconds);
            }
        }

        public JollyProfile RegisterProfile(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            lock (_profileLock)
            {
                if (_profiles.TryGetValue(name, out JollyProfile? profile))
                {
                    return profile;
                }

                profile = new JollyProfile(name);
                _profiles.Add(name, profile);
                return profile;
            }
        }

        public JollyProfileScope Profile(JollyProfile? profile)
        {
            return new JollyProfileScope(profile);
        }

        internal static void RecordProfile(JollyProfile profile, double elapsedMilliseconds)
        {
            Interlocked.Increment(ref profile.CallCount);
            Volatile.Write(ref profile.LastMilliseconds, elapsedMilliseconds);

            double total;

            do
            {
                total = Volatile.Read(ref profile.TotalMilliseconds);
            }
            while (Interlocked.CompareExchange(ref profile.TotalMilliseconds, total + elapsedMilliseconds, total) != total);

            double maximum;

            do
            {
                maximum = Volatile.Read(ref profile.MaximumMilliseconds);

                if (elapsedMilliseconds <= maximum)
                {
                    break;
                }
            }
            while (Interlocked.CompareExchange(ref profile.MaximumMilliseconds, elapsedMilliseconds, maximum) != maximum);
        }

        public JollyDiagnosticsSnapshot GetSnapshot()
        {
            Dictionary<string, JollyProfileSnapshot> profiles = new(StringComparer.Ordinal);

            lock (_profileLock)
            {
                foreach (KeyValuePair<string, JollyProfile> pair in _profiles)
                {
                    JollyProfile profile = pair.Value;
                    long callCount = Volatile.Read(ref profile.CallCount);
                    double totalMilliseconds = Volatile.Read(ref profile.TotalMilliseconds);

                    profiles[pair.Key] = new JollyProfileSnapshot
                    {
                        Name = profile.Name,
                        CallCount = callCount,
                        LastMilliseconds = Volatile.Read(ref profile.LastMilliseconds),
                        TotalMilliseconds = totalMilliseconds,
                        AverageMilliseconds = callCount == 0 ? 0.0 : totalMilliseconds / callCount,
                        MaximumMilliseconds = Volatile.Read(ref profile.MaximumMilliseconds)
                    };
                }
            }

            lock (_sync)
            {
                return new JollyDiagnosticsSnapshot
                {
                    FPS = _fps,
                    FrameTimeMilliseconds = _frameTimeMilliseconds,
                    UpdateTimeMilliseconds = _updateTimeMilliseconds,
                    RenderTimeMilliseconds = _renderTimeMilliseconds,

                    FrameCount = Interlocked.Read(ref _frameCount),
                    UpdateCount = Interlocked.Read(ref _updateCount),
                    RenderCount = Interlocked.Read(ref _renderCount),

                    EntityCount = Volatile.Read(ref _entityCount),
                    SpriteCount = Volatile.Read(ref _spriteCount),
                    TextureCount = Volatile.Read(ref _textureCount),

                    RenderBatchCount = Volatile.Read(ref _renderBatchCount),
                    RenderDrawCallCount = Volatile.Read(ref _renderDrawCallCount),
                    RenderMaxBatchSize = Volatile.Read(ref _renderMaxBatchSize),

                    ConnectedClients = Volatile.Read(ref _connectedClients),

                    NetworkMessagesSent = Interlocked.Read(ref _networkMessagesSent),
                    NetworkMessagesReceived = Interlocked.Read(ref _networkMessagesReceived),
                    NetworkBytesSent = Interlocked.Read(ref _networkBytesSent),
                    NetworkBytesReceived = Interlocked.Read(ref _networkBytesReceived),

                    NetworkLatencyMilliseconds = _networkLatencyMilliseconds,
                    Profiles = profiles
                };
            }
        }

        public void Reset()
        {
            lock (_sync)
            {
                Interlocked.Exchange(ref _frameCount, 0);
                Interlocked.Exchange(ref _updateCount, 0);
                Interlocked.Exchange(ref _renderCount, 0);
                Interlocked.Exchange(ref _framesAtLastSample, 0);

                _fps = 0.0;
                _frameTimeMilliseconds = 0.0;
                _updateTimeMilliseconds = 0.0;
                _renderTimeMilliseconds = 0.0;

                Interlocked.Exchange(ref _entityCount, 0);
                Interlocked.Exchange(ref _spriteCount, 0);
                Interlocked.Exchange(ref _textureCount, 0);

                Interlocked.Exchange(ref _renderBatchCount, 0);
                Interlocked.Exchange(ref _renderDrawCallCount, 0);
                Interlocked.Exchange(ref _renderMaxBatchSize, 0);

                Interlocked.Exchange(ref _connectedClients, 0);

                Interlocked.Exchange(ref _networkMessagesSent, 0);
                Interlocked.Exchange(ref _networkMessagesReceived, 0);
                Interlocked.Exchange(ref _networkBytesSent, 0);
                Interlocked.Exchange(ref _networkBytesReceived, 0);

                _networkLatencyMilliseconds = 0.0;

                _fpsTimer.Restart();
            }

            lock (_profileLock)
            {
                foreach (JollyProfile profile in _profiles.Values)
                {
                    Interlocked.Exchange(ref profile.CallCount, 0);
                    Volatile.Write(ref profile.LastMilliseconds, 0.0);
                    Volatile.Write(ref profile.TotalMilliseconds, 0.0);
                    Volatile.Write(ref profile.MaximumMilliseconds, 0.0);
                }
            }
        }

        internal void RecordUpdate()
        {
            Interlocked.Increment(ref _updateCount);
        }

        internal void RecordRender()
        {
            Interlocked.Increment(ref _renderCount);
        }

        private void SetFrameTime(double milliseconds)
        {
            lock (_sync)
            {
                _frameTimeMilliseconds = milliseconds;
            }
        }

        private void SetUpdateTime(double milliseconds)
        {
            lock (_sync)
            {
                _updateTimeMilliseconds = milliseconds;
            }
        }

        private void SetRenderTime(double milliseconds)
        {
            lock (_sync)
            {
                _renderTimeMilliseconds = milliseconds;
            }
        }

        internal void RecordDiagnosticTime(JollyDiagnosticScopeType type, double milliseconds)
        {
            lock (_sync)
            {
                if (type == JollyDiagnosticScopeType.Update)
                {
                    _updateTimeMilliseconds = milliseconds;
                }
                else
                {
                    _renderTimeMilliseconds = milliseconds;
                }
            }
        }

        private void UpdateFps()
        {
            lock (_sync)
            {
                if (_fpsTimer.Elapsed.TotalSeconds < 0.5)
                {
                    return;
                }

                long currentFrameCount = Interlocked.Read(ref _frameCount);
                long previousFrameCount = _framesAtLastSample;
                double elapsedSeconds = _fpsTimer.Elapsed.TotalSeconds;

                if (elapsedSeconds > 0.0)
                {
                    _fps = (currentFrameCount - previousFrameCount) / elapsedSeconds;
                }

                _framesAtLastSample = currentFrameCount;
                _fpsTimer.Restart();
            }
        }
    }
}