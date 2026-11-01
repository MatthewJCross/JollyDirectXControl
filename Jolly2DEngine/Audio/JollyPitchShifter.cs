namespace Jolly2DEngine.Audio
{
    // Lightweight dual-head granular pitch shifter. It preserves stream rate and duration,
    // but can produce more modulation artefacts than a phase-vocoder implementation.
    public sealed class JollyPitchShifter
    {
        private readonly float[][] _buffers;
        private readonly int _windowSize;
        private readonly int _bufferSize;
        private readonly int _channels;
        private readonly int[] _writePositions;
        private float _phase;
        private float _pitchFactor = 1.0f;

        public bool Enabled { get; set; } = true;

        public float PitchFactor
        {
            get => _pitchFactor;
            set
            {
                if (!float.IsFinite(value) || value < 0.5f || value > 2.0f) throw new ArgumentOutOfRangeException(nameof(value), "Pitch factor must be between 0.5 and 2.0.");
                _pitchFactor = value;
            }
        }

        public float PitchSemitones
        {
            get => 12.0f * MathF.Log2(_pitchFactor);
            set => PitchFactor = MathF.Pow(2.0f, value / 12.0f);
        }

        public float PitchCents
        {
            get => PitchSemitones * 100.0f;
            set => PitchSemitones = value / 100.0f;
        }

        public int LatencySamples => _windowSize;

        public JollyPitchShifter(int channels, int windowSize = 2048)
        {
            if (channels <= 0) throw new ArgumentOutOfRangeException(nameof(channels));
            if (windowSize < 256) throw new ArgumentOutOfRangeException(nameof(windowSize));
            _channels = channels;
            _windowSize = windowSize;
            _bufferSize = windowSize * 3 + 8;
            _buffers = Enumerable.Range(0, channels).Select(_ => new float[_bufferSize]).ToArray();
            _writePositions = new int[channels];
        }

        public float ProcessSample(float input, int channel)
        {
            if ((uint)channel >= (uint)_channels) throw new ArgumentOutOfRangeException(nameof(channel));
            if (!Enabled || MathF.Abs(_pitchFactor - 1.0f) < 0.0001f) return input;

            float[] buffer = _buffers[channel];
            int write = _writePositions[channel];
            buffer[write] = input;

            float phaseA = Wrap(_phase);
            float phaseB = Wrap(_phase + 0.5f);
            float delayA = _windowSize + phaseA * _windowSize;
            float delayB = _windowSize + phaseB * _windowSize;
            float weightA = 0.5f - 0.5f * MathF.Cos(2.0f * MathF.PI * phaseA);
            float weightB = 1.0f - weightA;

            float sampleA = ReadInterpolated(buffer, write - delayA);
            float sampleB = ReadInterpolated(buffer, write - delayB);
            float output = sampleA * weightA + sampleB * weightB;

            _writePositions[channel] = (write + 1) % _bufferSize;

            if (channel == _channels - 1)
            {
                _phase = Wrap(_phase + (1.0f - _pitchFactor) / _windowSize);
            }

            return output;
        }

        public void Reset()
        {
            foreach (float[] buffer in _buffers) Array.Clear(buffer);
            Array.Clear(_writePositions);
            _phase = 0.0f;
        }

        private float ReadInterpolated(float[] buffer, float position)
        {
            while (position < 0.0f) position += _bufferSize;
            while (position >= _bufferSize) position -= _bufferSize;
            int indexA = (int)position;
            int indexB = (indexA + 1) % _bufferSize;
            float fraction = position - indexA;
            return buffer[indexA] + (buffer[indexB] - buffer[indexA]) * fraction;
        }

        private static float Wrap(float value)
        {
            value %= 1.0f;
            return value < 0.0f ? value + 1.0f : value;
        }
    }
}
