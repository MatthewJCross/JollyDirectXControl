using NAudio.Wave;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyAnalyserSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly JollyAudioAnalyser _analyser;
        private int _channelPosition;
        private float _monoAccumulator;
        private int _leftLevelBits;
        private int _rightLevelBits;


        public WaveFormat WaveFormat => _source.WaveFormat;
        public float LeftLevel => BitConverter.Int32BitsToSingle(Volatile.Read(ref _leftLevelBits));
        public float RightLevel => BitConverter.Int32BitsToSingle(Volatile.Read(ref _rightLevelBits));

        public JollyAnalyserSampleProvider(ISampleProvider source, JollyAudioAnalyser analyser)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _analyser = analyser ?? throw new ArgumentNullException(nameof(analyser));
        }

        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);
            int channels = WaveFormat.Channels;

            float leftPeak = 0.0f;
            float rightPeak = 0.0f;

            for (int i = 0; i < read; i++)
            {
                float sample = buffer[i];

                _monoAccumulator += sample;
                _channelPosition++;

                int channel = (_channelPosition - 1) % channels;

                if (channel == 0)
                {
                    leftPeak = MathF.Max(leftPeak, MathF.Abs(sample));
                }
                else if (channel == 1)
                {
                    rightPeak = MathF.Max(rightPeak, MathF.Abs(sample));
                }

                if (_channelPosition >= channels)
                {
                    _analyser.AddSample(_monoAccumulator / channels);
                    _monoAccumulator = 0.0f;
                    _channelPosition = 0;
                }
            }

            if (channels == 1)
            {
                rightPeak = leftPeak;
            }

            Volatile.Write(ref _leftLevelBits, BitConverter.SingleToInt32Bits(System.Math.Clamp(leftPeak, 0.0f, 1.0f)));
            Volatile.Write(ref _rightLevelBits, BitConverter.SingleToInt32Bits(System.Math.Clamp(rightPeak, 0.0f, 1.0f)));

            return read;
        }
    }
}
