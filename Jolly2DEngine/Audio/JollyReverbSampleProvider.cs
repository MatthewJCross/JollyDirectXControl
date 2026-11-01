using NAudio.Wave;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyReverbSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly JollyReverb _reverb;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public JollyReverbSampleProvider(ISampleProvider source, JollyReverb reverb)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _reverb = reverb ?? throw new ArgumentNullException(nameof(reverb));
        }

        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);

            _reverb.Process(buffer[..read]);

            return read;
        }
    }
}
