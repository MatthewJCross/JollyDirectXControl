
namespace Jolly2DEngine.Audio
{
    public enum JollyReverbPreset
    {
        Room,
        Studio,
        Hall,
        Cathedral,
        Plate
    }

    public sealed class JollyReverb
    {
        private sealed class CombFilter
        {
            private readonly float[] _buffer;
            private int _position;
            private float _filterState;

            public CombFilter(int length)
            {
                _buffer = new float[System.Math.Max(1, length)];
            }

            public float Process(float input, float feedback, float damping)
            {
                float delayed = _buffer[_position];

                _filterState = delayed * (1.0f - damping) + _filterState * damping;
                _buffer[_position] = input + _filterState * feedback;

                _position++;

                if (_position >= _buffer.Length)
                {
                    _position = 0;
                }

                return delayed;
            }

            public void Clear()
            {
                Array.Clear(_buffer);
                _position = 0;
                _filterState = 0.0f;
            }
        }

        private sealed class AllPassFilter
        {
            private readonly float[] _buffer;
            private int _position;

            public AllPassFilter(int length)
            {
                _buffer = new float[System.Math.Max(1, length)];
            }

            public float Process(float input, float feedback)
            {
                float delayed = _buffer[_position];
                float output = delayed - input;

                _buffer[_position] = input + delayed * feedback;

                _position++;

                if (_position >= _buffer.Length)
                {
                    _position = 0;
                }

                return output;
            }

            public void Clear()
            {
                Array.Clear(_buffer);
                _position = 0;
            }
        }

        private readonly int _sampleRate;
        private readonly int _channels;
        private readonly CombFilter[][] _combFilters;
        private readonly AllPassFilter[][] _allPassFilters;
        private readonly float[][] _preDelayBuffers;
        private readonly int[] _preDelayPositions;

        private float _decayTime = 2.5f;
        private float _damping = 0.35f;
        private float _preDelayMilliseconds = 20.0f;
        private float _wetMix = 0.25f;
        private JollyReverbPreset _preset = JollyReverbPreset.Hall;

        public bool Enabled { get; set; }

        public float DecayTime
        {
            get => _decayTime;
            set => _decayTime = System.Math.Clamp(value, 0.1f, 20.0f);
        }

        public float Damping
        {
            get => _damping;
            set => _damping = System.Math.Clamp(value, 0.0f, 0.95f);
        }

        public float PreDelayMilliseconds
        {
            get => _preDelayMilliseconds;
            set => _preDelayMilliseconds = System.Math.Clamp(value, 0.0f, 200.0f);
        }

        public float WetMix
        {
            get => _wetMix;
            set => _wetMix = System.Math.Clamp(value, 0.0f, 1.0f);
        }

        public float DryMix => 1.0f - WetMix;

        public JollyReverbPreset Preset
        {
            get => _preset;
            set
            {
                _preset = value;

                switch (value)
                {
                    case JollyReverbPreset.Room:
                        DecayTime = 0.8f;
                        Damping = 0.55f;
                        PreDelayMilliseconds = 8.0f;
                        break;

                    case JollyReverbPreset.Studio:
                        DecayTime = 0.45f;
                        Damping = 0.65f;
                        PreDelayMilliseconds = 5.0f;
                        break;

                    case JollyReverbPreset.Hall:
                        DecayTime = 2.5f;
                        Damping = 0.35f;
                        PreDelayMilliseconds = 20.0f;
                        break;

                    case JollyReverbPreset.Cathedral:
                        DecayTime = 6.0f;
                        Damping = 0.2f;
                        PreDelayMilliseconds = 35.0f;
                        break;

                    case JollyReverbPreset.Plate:
                        DecayTime = 1.8f;
                        Damping = 0.15f;
                        PreDelayMilliseconds = 10.0f;
                        break;
                }
            }
        }

        public JollyReverb(int sampleRate, int channels)
        {
            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            }

            if (channels <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channels));
            }

            _sampleRate = sampleRate;
            _channels = channels;

            int[][] combDelayMilliseconds =
            [
                [29, 37, 41, 43],
                [31, 39, 47, 53]
            ];

            int[][] allPassDelayMilliseconds =
            [
                [5, 11],
                [7, 13]
            ];

            _combFilters = new CombFilter[channels][];
            _allPassFilters = new AllPassFilter[channels][];
            _preDelayBuffers = new float[channels][];
            _preDelayPositions = new int[channels];

            for (int channel = 0; channel < channels; channel++)
            {
                int layout = channel % 2;

                _combFilters[channel] = new CombFilter[4];

                for (int i = 0; i < 4; i++)
                {
                    int length = System.Math.Max(1, sampleRate * combDelayMilliseconds[layout][i] / 1000);
                    _combFilters[channel][i] = new CombFilter(length);
                }

                _allPassFilters[channel] = new AllPassFilter[2];

                for (int i = 0; i < 2; i++)
                {
                    int length = System.Math.Max(1, sampleRate * allPassDelayMilliseconds[layout][i] / 1000);
                    _allPassFilters[channel][i] = new AllPassFilter(length);
                }

                _preDelayBuffers[channel] = new float[sampleRate / 5 + 1];
            }
        }

        public void Process(Span<float> buffer)
        {
            if (!Enabled || WetMix <= 0.0f)
            {
                return;
            }

            float wet = WetMix;
            float dry = 1.0f - wet;
            float feedback = CalculateFeedback();
            float damping = Damping;
            int preDelayFrames = (int)(_sampleRate * PreDelayMilliseconds / 1000.0f);
            int frameCount = buffer.Length / _channels;

            for (int frame = 0; frame < frameCount; frame++)
            {
                int inputOffset = frame * _channels;

                for (int channel = 0; channel < _channels; channel++)
                {
                    float input = buffer[inputOffset + channel];
                    float delayedInput = input;

                    if (preDelayFrames > 0)
                    {
                        float[] preDelayBuffer = _preDelayBuffers[channel];
                        int readPosition = _preDelayPositions[channel] - preDelayFrames;

                        if (readPosition < 0)
                        {
                            readPosition += preDelayBuffer.Length;
                        }

                        delayedInput = preDelayBuffer[readPosition];
                        preDelayBuffer[_preDelayPositions[channel]] = input;

                        _preDelayPositions[channel]++;

                        if (_preDelayPositions[channel] >= preDelayBuffer.Length)
                        {
                            _preDelayPositions[channel] = 0;
                        }
                    }

                    float reverb = 0.0f;

                    for (int i = 0; i < _combFilters[channel].Length; i++)
                    {
                        reverb += _combFilters[channel][i].Process(delayedInput, feedback, damping);
                    }

                    reverb *= 0.25f;

                    for (int i = 0; i < _allPassFilters[channel].Length; i++)
                    {
                        reverb = _allPassFilters[channel][i].Process(reverb, 0.5f);
                    }

                    buffer[inputOffset + channel] = input * dry + reverb * wet;
                }
            }
        }

        public void Clear()
        {
            for (int channel = 0; channel < _channels; channel++)
            {
                foreach (CombFilter filter in _combFilters[channel])
                {
                    filter.Clear();
                }

                foreach (AllPassFilter filter in _allPassFilters[channel])
                {
                    filter.Clear();
                }

                Array.Clear(_preDelayBuffers[channel]);
                _preDelayPositions[channel] = 0;
            }
        }

        private float CalculateFeedback()
        {
            float averageDelayMilliseconds = 38.0f;
            float delaySeconds = averageDelayMilliseconds / 1000.0f;

            return System.Math.Clamp((float)System.Math.Pow(0.001, delaySeconds / DecayTime), 0.0f, 0.98f);
        }
    }
}

