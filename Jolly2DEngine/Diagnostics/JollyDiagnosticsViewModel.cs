using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jolly2DEngine.Diagnostics
{
    public sealed class JollyDiagnosticsViewModel : INotifyPropertyChanged
    {
        private double _fps;
        private double _frameTimeMilliseconds;
        private double _updateTimeMilliseconds;
        private double _renderTimeMilliseconds;
        private long _frameCount;
        private long _updateCount;
        private long _renderCount;
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
        private IReadOnlyDictionary<string, JollyProfileSnapshot> _profiles = new Dictionary<string, JollyProfileSnapshot>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public double FPS
        {
            get => _fps;
            private set => SetProperty(ref _fps, value);
        }

        public double FrameTimeMilliseconds
        {
            get => _frameTimeMilliseconds;
            private set => SetProperty(ref _frameTimeMilliseconds, value);
        }

        public double UpdateTimeMilliseconds
        {
            get => _updateTimeMilliseconds;
            private set => SetProperty(ref _updateTimeMilliseconds, value);
        }

        public double RenderTimeMilliseconds
        {
            get => _renderTimeMilliseconds;
            private set => SetProperty(ref _renderTimeMilliseconds, value);
        }

        public long FrameCount
        {
            get => _frameCount;
            private set => SetProperty(ref _frameCount, value);
        }

        public long UpdateCount
        {
            get => _updateCount;
            private set => SetProperty(ref _updateCount, value);
        }

        public long RenderCount
        {
            get => _renderCount;
            private set => SetProperty(ref _renderCount, value);
        }

        public int EntityCount
        {
            get => _entityCount;
            private set => SetProperty(ref _entityCount, value);
        }

        public int SpriteCount
        {
            get => _spriteCount;
            private set => SetProperty(ref _spriteCount, value);
        }

        public int TextureCount
        {
            get => _textureCount;
            private set => SetProperty(ref _textureCount, value);
        }

        public int RenderBatchCount
        {
            get => _renderBatchCount;
            private set => SetProperty(ref _renderBatchCount, value);
        }

        public int RenderDrawCallCount
        {
            get => _renderDrawCallCount;
            private set => SetProperty(ref _renderDrawCallCount, value);
        }

        public int RenderMaxBatchSize
        {
            get => _renderMaxBatchSize;
            private set => SetProperty(ref _renderMaxBatchSize, value);
        }

        public int ConnectedClients
        {
            get => _connectedClients;
            private set => SetProperty(ref _connectedClients, value);
        }

        public long NetworkMessagesSent
        {
            get => _networkMessagesSent;
            private set => SetProperty(ref _networkMessagesSent, value);
        }

        public long NetworkMessagesReceived
        {
            get => _networkMessagesReceived;
            private set => SetProperty(ref _networkMessagesReceived, value);
        }

        public long NetworkBytesSent
        {
            get => _networkBytesSent;
            private set => SetProperty(ref _networkBytesSent, value);
        }

        public long NetworkBytesReceived
        {
            get => _networkBytesReceived;
            private set => SetProperty(ref _networkBytesReceived, value);
        }

        public double NetworkLatencyMilliseconds
        {
            get => _networkLatencyMilliseconds;
            private set => SetProperty(ref _networkLatencyMilliseconds, value);
        }

        public IReadOnlyDictionary<string, JollyProfileSnapshot> Profiles
        {
            get => _profiles;
            private set => SetProperty(ref _profiles, value);
        }

        public void Refresh(JollyDiagnostics diagnostics)
        {
            ArgumentNullException.ThrowIfNull(diagnostics);

            JollyDiagnosticsSnapshot snapshot = diagnostics.GetSnapshot();

            FPS = snapshot.FPS;
            FrameTimeMilliseconds = snapshot.FrameTimeMilliseconds;
            UpdateTimeMilliseconds = snapshot.UpdateTimeMilliseconds;
            RenderTimeMilliseconds = snapshot.RenderTimeMilliseconds;
            FrameCount = snapshot.FrameCount;
            UpdateCount = snapshot.UpdateCount;
            RenderCount = snapshot.RenderCount;
            EntityCount = snapshot.EntityCount;
            SpriteCount = snapshot.SpriteCount;
            TextureCount = snapshot.TextureCount;
            ConnectedClients = snapshot.ConnectedClients;
            NetworkMessagesSent = snapshot.NetworkMessagesSent;
            NetworkMessagesReceived = snapshot.NetworkMessagesReceived;
            NetworkBytesSent = snapshot.NetworkBytesSent;
            NetworkBytesReceived = snapshot.NetworkBytesReceived;
            NetworkLatencyMilliseconds = snapshot.NetworkLatencyMilliseconds;
            Profiles = snapshot.Profiles;
        }

        public string GetSummary()
        {
            return $"FPS: {FPS:F1} | Frame: {FrameTimeMilliseconds:F2} ms | Update: {UpdateTimeMilliseconds:F2} ms | Render: {RenderTimeMilliseconds:F2} ms | Sprites: {SpriteCount} | Textures: {TextureCount}";
        }

        public string GetRenderSummary()
        {
            JollyProfileSnapshot? spritePrepare = GetProfile("Render.SpritePrepare");
            JollyProfileSnapshot? spriteUpload = GetProfile("Render.SpriteUpload");
            JollyProfileSnapshot? spriteDraw = GetProfile("Render.SpriteDrawCall");

            return $"Sprites: {SpriteCount} | Batches: {GetProfileCallCount("Render.SpriteDraw")} | Upload: {FormatMilliseconds(spriteUpload)} | Draw: {FormatMilliseconds(spriteDraw)} | Prepare: {FormatMilliseconds(spritePrepare)}";
        }

        public JollyProfileSnapshot? GetProfile(string name)
        {
            if (Profiles.TryGetValue(name, out JollyProfileSnapshot? profile))
            {
                return profile;
            }

            return null;
        }

        public long GetProfileCallCount(string name)
        {
            return GetProfile(name)?.CallCount ?? 0;
        }

        private static string FormatMilliseconds(JollyProfileSnapshot? profile)
        {
            return profile == null ? "0.00 ms" : $"{profile.LastMilliseconds:F2} ms";
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}