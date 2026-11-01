using NAudio.Wave;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyEchoSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly JollyEcho _echo;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public JollyEchoSampleProvider(ISampleProvider source, JollyEcho echo)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _echo = echo ?? throw new ArgumentNullException(nameof(echo));
        }

        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);
            _echo.Process(buffer[..read]);

            return read;
        }
    }
}

