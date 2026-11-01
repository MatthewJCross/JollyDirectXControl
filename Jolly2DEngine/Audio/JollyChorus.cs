namespace Jolly2DEngine.Audio
{
    public sealed class JollyChorus
    {
        private readonly int _sampleRate;
        private readonly int _channels;
        private readonly int _maximumDelayFrames;
        private readonly float[] _delayBuffer;
        private int _writeFrame;
        private double _phase;
        private float _rate = 0.8f;
        private float _depthMilliseconds = 8.0f;
        private float _delayMilliseconds = 20.0f;
        private float _feedback = 0.1f;
        private float _wetMix = 0.3f;

        public bool Enabled { get; set; }

        public float Rate
        {
            get => _rate;
            set => _rate = System.Math.Clamp(value, 0.05f, 5.0f);
        }

        public float Depth
        {
            get => _depthMilliseconds;
            set => _depthMilliseconds = System.Math.Clamp(value, 0.0f, 10.0f);
        }

        public float DelayMilliseconds
        {
            get => _delayMilliseconds;
            set => _delayMilliseconds = System.Math.Clamp(value, 5.0f, MaximumDelayMilliseconds);
        }

        public float Feedback
        {
            get => _feedback;
            set => _feedback = System.Math.Clamp(value, -0.7f, 0.7f);
        }

        public float WetMix
        {
            get => _wetMix;
            set => _wetMix = System.Math.Clamp(value, 0.0f, 1.0f);
        }

        public float DryMix => 1.0f - WetMix;

        public float MaximumDelayMilliseconds { get; }

        public JollyChorus(int sampleRate, int channels, float maximumDelayMilliseconds = 50.0f)
        {
            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            }

            if (channels <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channels));
            }

            if (maximumDelayMilliseconds < 20.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumDelayMilliseconds));
            }

            _sampleRate = sampleRate;
            _channels = channels;
            MaximumDelayMilliseconds = maximumDelayMilliseconds;

            _maximumDelayFrames = System.Math.Max(2, (int)System.Math.Ceiling(sampleRate * maximumDelayMilliseconds / 1000.0f) + 2);
            _delayBuffer = new float[_maximumDelayFrames * channels];
        }

        public void Process(Span<float> buffer)
        {
            if (!Enabled || WetMix <= 0.0f)
            {
                return;
            }

            float wet = WetMix;
            float dry = 1.0f - wet;
            float feedback = Feedback;
            float depth = Depth * _sampleRate / 1000.0f;
            float baseDelay = DelayMilliseconds * _sampleRate / 1000.0f;
            double phaseIncrement = 2.0 * System.Math.PI * Rate / _sampleRate;
            int frameCount = buffer.Length / _channels;

            for (int frame = 0; frame < frameCount; frame++)
            {
                int inputOffset = frame * _channels;
                float leftLfo = (float)System.Math.Sin(_phase);
                float rightLfo = (float)System.Math.Sin(_phase + System.Math.PI * 0.5);

                for (int channel = 0; channel < _channels; channel++)
                {
                    float lfo = channel == 1 ? rightLfo : leftLfo;
                    float delayFrames = System.Math.Clamp(baseDelay + lfo * depth, 1.0f, _maximumDelayFrames - 2.0f);

                    int readFrame = _writeFrame - (int)delayFrames;

                    if (readFrame < 0)
                    {
                        readFrame += _maximumDelayFrames;
                    }

                    int nextReadFrame = readFrame - 1;

                    if (nextReadFrame < _maximumDelayFrames)
                    {
                        nextReadFrame = 0;
                    }

                    float fraction = delayFrames - (int)delayFrames;
                    int readOffset = readFrame * _channels + channel;
                    int nextReadOffset = nextReadFrame * _channels + channel;
                    int writeOffset = _writeFrame * _channels + channel;

                    float delayedA = _delayBuffer[readOffset];
                    float delayedB = _delayBuffer[nextReadOffset];
                    float delayed = delayedA + (delayedB - delayedA) * fraction;
                    float input = buffer[inputOffset + channel];

                    _delayBuffer[writeOffset] = input + delayed * feedback;
                    buffer[inputOffset + channel] = input * dry + delayed * wet;
                }

                _writeFrame++;

                if (_writeFrame >= _maximumDelayFrames)
                {
                    _writeFrame = 0;
                }

                _phase += phaseIncrement;

                if (_phase >= 2.0 * System.Math.PI)
                {
                    _phase -= 2.0 * System.Math.PI;
                }
            }
        }

        public void Clear()
        {
            Array.Clear(_delayBuffer);
            _writeFrame = 0;
            _phase = 0.0;
        }
    }
}

