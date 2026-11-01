namespace Jolly2DEngine.Audio
{
    public sealed class JollyEcho
    {
        private readonly int _sampleRate;
        private readonly int _channels;
        private readonly int _maximumDelayFrames;
        private readonly float[] _delayBuffer;
        private int _writeFrame;
        private float _delayMilliseconds = 350.0f;
        private float _feedback = 0.35f;
        private float _wetMix = 0.25f;

        public bool Enabled { get; set; }

        public float DelayMilliseconds
        {
            get => _delayMilliseconds;
            set => _delayMilliseconds = System.Math.Clamp(value, 1.0f, MaximumDelayMilliseconds);
        }

        public float Feedback
        {
            get => _feedback;
            set => _feedback = System.Math.Clamp(value, 0.0f, 0.95f);
        }

        public float WetMix
        {
            get => _wetMix;
            set => _wetMix = System.Math.Clamp(value, 0.0f, 1.0f);
        }

        public float DryMix => 1.0f - WetMix;

        public float MaximumDelayMilliseconds { get; }

        public JollyEcho(int sampleRate, int channels, float maximumDelayMilliseconds = 2000.0f)
        {
            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            }

            if (channels <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channels));
            }

            if (maximumDelayMilliseconds < 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumDelayMilliseconds));
            }

            _sampleRate = sampleRate;
            _channels = channels;
            MaximumDelayMilliseconds = maximumDelayMilliseconds;

            _maximumDelayFrames = System.Math.Max(2, (int)System.Math.Ceiling(sampleRate * maximumDelayMilliseconds / 1000.0f) + 1);
            _delayBuffer = new float[_maximumDelayFrames * channels];
        }

        public void Process(Span<float> buffer)
        {
            if (!Enabled || WetMix <= 0.0f)
            {
                return;
            }

            int delayFrames = System.Math.Clamp((int)(_sampleRate * DelayMilliseconds / 1000.0f), 1, _maximumDelayFrames - 1);

            float feedback = Feedback;
            float wet = WetMix;
            float dry = 1.0f - wet;

            int frameCount = buffer.Length / _channels;

            for (int frame = 0; frame < frameCount; frame++)
            {
                int readFrame = _writeFrame - delayFrames;

                if (readFrame < 0)
                {
                    readFrame += _maximumDelayFrames;
                }

                int writeOffset = _writeFrame * _channels;
                int readOffset = readFrame * _channels;
                int inputOffset = frame * _channels;

                for (int channel = 0; channel < _channels; channel++)
                {
                    float input = buffer[inputOffset + channel];
                    float delayed = _delayBuffer[readOffset + channel];

                    _delayBuffer[writeOffset + channel] = input + delayed * feedback;
                    buffer[inputOffset + channel] = input * dry + delayed * wet;
                }

                _writeFrame++;

                if (_writeFrame >= _maximumDelayFrames)
                {
                    _writeFrame = 0;
                }
            }
        }

        public void Clear()
        {
            Array.Clear(_delayBuffer);
            _writeFrame = 0;
        }
    }
}
