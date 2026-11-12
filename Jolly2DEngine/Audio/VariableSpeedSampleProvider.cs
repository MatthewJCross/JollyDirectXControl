using NAudio.Wave;

namespace Jolly2DEngine.Audio
{
    public sealed class VariableSpeedSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly int _channels;
        private readonly float[] _current;
        private readonly float[] _next;

        private double _fraction;
        private float _speed = 1.0f;
        private bool _initialized;
        private bool _finished;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public float Speed
        {
            get => Volatile.Read(ref _speed);
            set => Volatile.Write(ref _speed, System.Math.Clamp(value, 0.5f, 2.0f));
        }

        public VariableSpeedSampleProvider(ISampleProvider source)
        {
            _source = source;
            _channels = source.WaveFormat.Channels;

            _current = new float[_channels];
            _next = new float[_channels];
        }

        public int Read(Span<float> buffer)
        {
            int framesRequested = buffer.Length / _channels;
            int framesWritten = 0;

            if (!_initialized)
            {
                if (!ReadFrame(_current))
                    return 0;

                if (!ReadFrame(_next))
                    Array.Copy(_current, _next, _channels);

                _initialized = true;
            }

            while (framesWritten < framesRequested)
            {
                int offset = framesWritten * _channels;

                for (int channel = 0; channel < _channels; channel++)
                {
                    buffer[offset + channel] =
                        _current[channel] +
                        (_next[channel] - _current[channel]) * (float)_fraction;
                }

                framesWritten++;

                _fraction += Speed;

                while (_fraction >= 1.0)
                {
                    _fraction -= 1.0;

                    Array.Copy(_next, _current, _channels);

                    if (!_finished)
                    {
                        if (!ReadFrame(_next))
                        {
                            _finished = true;
                            Array.Copy(_current, _next, _channels);
                        }
                    }
                    else
                    {
                        return framesWritten * _channels;
                    }
                }
            }

            return framesWritten * _channels;
        }

        private bool ReadFrame(float[] frame)
        {
            int offset = 0;

            while (offset < frame.Length)
            {
                int read = _source.Read(frame.AsSpan(offset));

                if (read <= 0)
                    return false;

                offset += read;
            }

            return true;
        }
    }
}