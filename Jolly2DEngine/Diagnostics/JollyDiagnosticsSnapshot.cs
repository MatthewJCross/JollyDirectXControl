using System.Collections.Generic;

namespace Jolly2DEngine.Diagnostics
{
    public sealed class JollyDiagnosticsSnapshot
    {
        public double FPS { get; init; }
        public double FrameTimeMilliseconds { get; init; }
        public double UpdateTimeMilliseconds { get; init; }
        public double RenderTimeMilliseconds { get; init; }

        public long FrameCount { get; init; }
        public long UpdateCount { get; init; }
        public long RenderCount { get; init; }

        public int EntityCount { get; init; }
        public int SpriteCount { get; init; }
        public int TextureCount { get; init; }
        public int RenderBatchCount { get; init; }
        public int RenderDrawCallCount { get; init; }
        public int RenderMaxBatchSize { get; init; }

        public int ConnectedClients { get; init; }
        public long NetworkMessagesSent { get; init; }
        public long NetworkMessagesReceived { get; init; }
        public long NetworkBytesSent { get; init; }
        public long NetworkBytesReceived { get; init; }

        public double NetworkLatencyMilliseconds { get; init; }

        public IReadOnlyDictionary<string, JollyProfileSnapshot> Profiles { get; init; } = new Dictionary<string, JollyProfileSnapshot>();
    }

    public sealed class JollyProfileSnapshot
    {
        public string Name { get; init; } = string.Empty;
        public long CallCount { get; init; }
        public double LastMilliseconds { get; init; }
        public double TotalMilliseconds { get; init; }
        public double AverageMilliseconds { get; init; }
        public double MaximumMilliseconds { get; init; }
    }
}