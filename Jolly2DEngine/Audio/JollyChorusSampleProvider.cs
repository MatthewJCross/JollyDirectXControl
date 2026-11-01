using NAudio.Wave;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyChorusSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly JollyChorus _chorus;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public JollyChorusSampleProvider(ISampleProvider source, JollyChorus chorus)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _chorus = chorus ?? throw new ArgumentNullException(nameof(chorus));
        }

        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);

            _chorus.Process(buffer[..read]);

            return read;
        }
    }
}
