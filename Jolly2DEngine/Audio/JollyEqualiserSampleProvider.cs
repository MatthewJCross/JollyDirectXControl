using NAudio.Wave;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyEqualiserSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly JollyEqualiser _equaliser;
        private int _samplePosition;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public JollyEqualiserSampleProvider(ISampleProvider source, JollyEqualiser equaliser)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _equaliser = equaliser ?? throw new ArgumentNullException(nameof(equaliser));
        }

        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);
            int channels = WaveFormat.Channels;

            for (int i = 0; i < read; i++)
            {
                int channel = _samplePosition++ % channels;
                buffer[i] = _equaliser.ProcessSample(buffer[i], channel);
            }

            return read;
        }
    }
}
