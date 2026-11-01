using NAudio.Wave;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyPitchShiftSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly JollyPitchShifter _pitchShifter;
        private int _samplePosition;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public JollyPitchShiftSampleProvider(ISampleProvider source, JollyPitchShifter pitchShifter)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _pitchShifter = pitchShifter ?? throw new ArgumentNullException(nameof(pitchShifter));
        }

        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);
            int channels = WaveFormat.Channels;

            for (int i = 0; i < read; i++)
            {
                int channel = _samplePosition++ % channels;
                buffer[i] = _pitchShifter.ProcessSample(buffer[i], channel);
            }

            return read;
        }
    }
}
