using System.Numerics;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyAudioAnalyser : IDisposable
    {
        private readonly object _lock = new();
        private readonly int _sampleRate;
        private readonly int _fftSize;
        private readonly int _hopSize;
        private readonly float[] _samples;
        private readonly float[] _input;
        private readonly Complex[] _complex;
        private readonly float[] _frequencies;
        private readonly AutoResetEvent _workAvailable = new(false);
        private readonly Thread _worker;

        private readonly float[] _previousMagnitudes;
        private readonly float[] _beatHistory;

        private int _beatHistoryPosition;
        private int _beatHistoryCount;
        private int _bpmUpdateCounter;

        private float _bpm;
        private float _beatConfidence;
        private long _lastBeatSample;
        
        private int _writePosition;
        private long _totalSamples;
        private float _rms;
        private float _peak;
        private int _window = (int)JollyFourierWindow.Hann;
        private volatile bool _enabled = true;
        private volatile bool _disposed;
        private JollySpectrumData? _latestSpectrum;

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public JollyFourierWindow Window
        {
            get => (JollyFourierWindow)Volatile.Read(ref _window);
            set => Volatile.Write(ref _window, (int)value);
        }

        public int FftSize => _fftSize;
        public int SampleRate => _sampleRate;
        public float Bpm => Volatile.Read(ref _bpm);
        public float BeatConfidence => Volatile.Read(ref _beatConfidence);
        public bool IsBeat => Interlocked.Read(ref _totalSamples) - Interlocked.Read(ref _lastBeatSample) < _sampleRate * 0.15;

        public JollyAudioAnalyser(int sampleRate, int fftSize = 2048)
        {
            if (sampleRate <= 0) 
                throw new ArgumentOutOfRangeException(nameof(sampleRate));

            if (fftSize < 256 || (fftSize & (fftSize - 1)) != 0) 
                throw new ArgumentOutOfRangeException(nameof(fftSize));

            _sampleRate = sampleRate;
            _fftSize = fftSize;
            _hopSize = fftSize / 4;

            _samples = new float[fftSize];
            _input = new float[fftSize];
            _complex = new Complex[fftSize];

            _frequencies = new float[fftSize / 2 + 1];

            for (int i = 0; i < _frequencies.Length; i++)
            {
                _frequencies[i] = JollyFourierTransform.GetFrequencyForBin(i, fftSize, sampleRate);
            }

            _previousMagnitudes = new float[_frequencies.Length];
            int historySize = (int)(_sampleRate / (double)_hopSize * 8.0);
            _beatHistory = new float[historySize];
            
            _latestSpectrum = new JollySpectrumData(sampleRate, fftSize, new float[_frequencies.Length], _frequencies, 0.0f, 0.0f);

            _worker = new Thread(AnalyseLoop)
            {
                IsBackground = true,
                Name = "Jolly Audio Analyser"
            };

            _worker.Start();
        }

        public void AddSample(float sample)
        {
            if (!_enabled || _disposed) return;

            lock (_lock)
            {
                _samples[_writePosition] = sample;
                _writePosition = (_writePosition + 1) % _fftSize;
                _totalSamples++;

                float absolute = MathF.Abs(sample);

                _peak = MathF.Max(_peak * 0.995f, absolute);
                _rms = (_rms * 0.999f) + (sample * sample * 0.001f);

                if (_totalSamples >= _fftSize && _totalSamples % _hopSize == 0)
                {
                    _workAvailable.Set();
                }
            }
        }

        public JollySpectrumData? GetSpectrum()
        {
            return Volatile.Read(ref _latestSpectrum);
        }

        private void AnalyseLoop()
        {
            while (true)
            {
                _workAvailable.WaitOne();

                if (_disposed) return;

                if (_enabled)
                {
                    Analyse();
                }
            }
        }

        private void Analyse()
        {
            float rms;
            float peak;

            lock (_lock)
            {
                int start = _writePosition;

                int firstCount = System.Math.Min(_fftSize - start, _fftSize);

                Array.Copy(_samples, start, _input, 0, firstCount);

                if (firstCount < _fftSize)
                {
                    Array.Copy(_samples, 0, _input, firstCount, _fftSize - firstCount);
                }

                rms = MathF.Sqrt(_rms);
                peak = _peak;
            }

            JollyFourierTransform.Transform(_input, _complex, Window);
            float[] magnitudes = new float[_frequencies.Length];
            JollyFourierTransform.GetMagnitudeSpectrum(_complex, magnitudes);

            long samplePosition;
            lock (_lock)
            {
                samplePosition = _totalSamples;
            }
            UpdateBeatDetection(magnitudes, samplePosition);
            
            JollySpectrumData spectrum = new(_sampleRate, _fftSize, magnitudes, _frequencies, rms, peak);
            Volatile.Write(ref _latestSpectrum, spectrum);
        }

        private void UpdateBeatDetection(float[] magnitudes, long samplePosition)
        {
            float flux = 0.0f;
            float total = 0.0f;

            for (int i = 2; i < magnitudes.Length; i++)
            {
                float current = magnitudes[i];
                float previous = _previousMagnitudes[i];

                if (current > previous)
                {
                    flux += current - previous;
                }

                total += previous;
                _previousMagnitudes[i] = current;
            }

            flux /= total + 0.000001f;

            _beatHistory[_beatHistoryPosition] = flux;
            _beatHistoryPosition = (_beatHistoryPosition + 1) % _beatHistory.Length;
            _beatHistoryCount = System.Math.Min(_beatHistoryCount + 1, _beatHistory.Length);

            int baselineCount = System.Math.Min(_beatHistoryCount - 1, _sampleRate / _hopSize);

            if (baselineCount > 0)
            {
                float baseline = 0.0f;

                for (int i = 1; i <= baselineCount; i++)
                {
                    int index = (_beatHistoryPosition - 1 - i + _beatHistory.Length) % _beatHistory.Length;
                    baseline += _beatHistory[index];
                }

                baseline /= baselineCount;

                long previousBeat = Interlocked.Read(ref _lastBeatSample);
                if (_beatHistoryCount > 8 && flux > MathF.Max(0.025f, baseline * 1.8f) && samplePosition - previousBeat > _sampleRate * 0.25)
                {
                    Interlocked.Exchange(ref _lastBeatSample, samplePosition);
                }
            }

            _bpmUpdateCounter++;

            if (_bpmUpdateCounter >= 8)
            {
                _bpmUpdateCounter = 0;
                CalculateBpm();
            }
        }

        private void CalculateBpm()
        {
            double hopRate = _sampleRate / (double)_hopSize;

            int requiredSamples = (int)(hopRate * 3.0);

            if (_beatHistoryCount < requiredSamples)
            {
                return;
            }

            int count = System.Math.Min(_beatHistoryCount, (int)(hopRate * 4.0));

            double mean = 0.0;

            for (int i = 0; i < count; i++)
            {
                int index = (_beatHistoryPosition - 1 - i + _beatHistory.Length) % _beatHistory.Length;
                mean += _beatHistory[index];
            }

            mean /= count;

            double bestCorrelation = 0.0;
            int bestLag = 0;

            int minimumLag = System.Math.Max(1, (int)(hopRate * 60.0 / 180.0));
            int maximumLag = (int)(hopRate * 60.0 / 60.0);

            for (int lag = minimumLag; lag <= maximumLag; lag++)
            {
                double correlation = 0.0;
                double energyA = 0.0;
                double energyB = 0.0;

                for (int i = 0; i < count - lag; i++)
                {
                    int indexA = (_beatHistoryPosition - 1 - i + _beatHistory.Length) % _beatHistory.Length;
                    int indexB = (_beatHistoryPosition - 1 - i - lag + _beatHistory.Length) % _beatHistory.Length;

                    double a = _beatHistory[indexA] - mean;
                    double b = _beatHistory[indexB] - mean;

                    correlation += a * b;
                    energyA += a * a;
                    energyB += b * b;
                }

                double normalised = correlation / System.Math.Sqrt(energyA * energyB + 0.0000001);

                if (normalised > bestCorrelation)
                {
                    bestCorrelation = normalised;
                    bestLag = lag;
                }
            }

            float confidence = (float)System.Math.Clamp(bestCorrelation, 0.0, 1.0);

            Volatile.Write(ref _beatConfidence, confidence);

            if (bestLag == 0 || confidence < 0.15f)
            {
                return;
            }

            float detectedBpm = (float)(60.0 * hopRate / bestLag);

            if (detectedBpm < 60.0f || detectedBpm > 180.0f)
            {
                return;
            }

            if (confidence < 0.35f)
            {
                return;
            }

            float currentBpm = Volatile.Read(ref _bpm);

            if (currentBpm > 0.0f)
            {
                float halfBpm = currentBpm * 0.5f;
                float doubleBpm = currentBpm * 2.0f;

                float directDifference = MathF.Abs(detectedBpm - currentBpm);
                float halfDifference = MathF.Abs(detectedBpm - halfBpm);
                float doubleDifference = MathF.Abs(detectedBpm - doubleBpm);

                if (halfDifference < directDifference)
                {
                    detectedBpm = currentBpm;
                }
                else if (doubleDifference < directDifference)
                {
                    detectedBpm = currentBpm;
                }

                if (MathF.Abs(detectedBpm - currentBpm) > currentBpm * 0.15f)
                {
                    return;
                }
            }

            float smoothing = currentBpm <= 0.0f ? 1.0f : 0.08f;

            float smoothedBpm = currentBpm + (detectedBpm - currentBpm) * smoothing;

            Volatile.Write(ref _bpm, smoothedBpm);
        }

        public void Dispose()
        {
            if (_disposed) 
                return;

            _disposed = true;
            _workAvailable.Set();
            _worker.Join();
            _workAvailable.Dispose();
        }
    }
}
