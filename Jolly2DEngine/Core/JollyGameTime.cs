namespace Jolly2DEngine.Core
{
    public sealed class JollyGameTime
    {
        public double TotalSeconds { get; internal set; }
        public double DeltaSeconds { get; internal set; }
        public double DeltaMilliseconds => DeltaSeconds * 1000.0;
        public double FPS { get; internal set; }
        public double FrameTime => DeltaMilliseconds;
        public long FrameCount { get; internal set; }
    }
}
