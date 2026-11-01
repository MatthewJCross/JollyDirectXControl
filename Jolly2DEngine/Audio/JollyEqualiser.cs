namespace Jolly2DEngine.Audio
{
    public sealed class JollyEqualiserBand
    {
        public float Frequency { get; internal set; }
        public float GainDb { get; internal set; }
        public float Q { get; internal set; }
        internal JollyBiquadFilter[] Filters { get; }

        internal JollyEqualiserBand(float frequency, float gainDb, float q, int channels)
        {
            Frequency = frequency;
            GainDb = gainDb;
            Q = q;
            Filters = Enumerable.Range(0, channels).Select(_ => new JollyBiquadFilter()).ToArray();
        }
    }

    public sealed class JollyEqualiser
    {
        private readonly List<JollyEqualiserBand> _bands = [];
        private readonly int _sampleRate;
        private readonly int _channels;

        public bool Enabled { get; set; } = true;
        public IReadOnlyList<JollyEqualiserBand> Bands => _bands;

        public JollyEqualiser(int sampleRate, int channels)
        {
            if (sampleRate <= 0) 
                throw new ArgumentOutOfRangeException(nameof(sampleRate));

            if (channels <= 0) 
                throw new ArgumentOutOfRangeException(nameof(channels));

            _sampleRate = sampleRate;
            _channels = channels;
        }

        public int AddBand(float frequency, float gainDb = 0.0f, float q = 1.0f)
        {
            ValidateBand(frequency, gainDb, q);
            JollyEqualiserBand band = new(frequency, gainDb, q, _channels);
            Configure(band);
            _bands.Add(band);
            return _bands.Count - 1;
        }

        public void SetBand(int index, float frequency, float gainDb, float q = 1.0f)
        {
            ValidateBand(frequency, gainDb, q);
            JollyEqualiserBand band = _bands[index];
            band.Frequency = frequency;
            band.GainDb = gainDb;
            band.Q = q;
            Configure(band);
        }

        public void SetBandFrequency(int index, float frequency)
        {
            JollyEqualiserBand band = _bands[index];
            SetBand(index, frequency, band.GainDb, band.Q);
        }

        public void SetBandQ(int index, float q)
        {
            JollyEqualiserBand band = _bands[index];
            SetBand(index, band.Frequency, band.GainDb, q);
        }

        public void RemoveBand(int index) => _bands.RemoveAt(index);

        public void ClearBands() => _bands.Clear();

        public float ProcessSample(float sample, int channel)
        {
            if (!Enabled) return sample;
            if ((uint)channel >= (uint)_channels) 
                throw new ArgumentOutOfRangeException(nameof(channel));

            float output = sample;
            foreach (JollyEqualiserBand band in _bands) 
                output = band.Filters[channel].ProcessSample(output);

            return output;
        }

        public void Reset()
        {
            foreach (JollyEqualiserBand band in _bands)
            {
                foreach (JollyBiquadFilter filter in band.Filters) 
                    filter.Reset();
            }
        }

        private void Configure(JollyEqualiserBand band)
        {
            foreach (JollyBiquadFilter filter in band.Filters) 
                filter.SetPeaking(_sampleRate, band.Frequency, band.GainDb, band.Q);
        }

        private void ValidateBand(float frequency, float gainDb, float q)
        {
            if (!float.IsFinite(frequency) || frequency <= 0.0f || frequency >= _sampleRate * 0.5f) 
                throw new ArgumentOutOfRangeException(nameof(frequency));

            if (!float.IsFinite(gainDb) || gainDb < -24.0f || gainDb > 24.0f) 
                throw new ArgumentOutOfRangeException(nameof(gainDb));

            if (!float.IsFinite(q) || q <= 0.0f || q > 20.0f) 
                throw new ArgumentOutOfRangeException(nameof(q));
        }

        public void ConfigureBands(IEnumerable<float> frequencies, float gainDb = 0.0f, float q = 1.0f)
        {
            ArgumentNullException.ThrowIfNull(frequencies);

            List<float> frequencyList = frequencies.ToList();

            if (frequencyList.Count == 0)
            {
                ClearBands();
                return;
            }

            foreach (float frequency in frequencyList)
            {
                ValidateBand(frequency, gainDb, q);
            }

            ClearBands();

            foreach (float frequency in frequencyList)
            {
                AddBand(frequency, gainDb, q);
            }
        }

        public void ConfigureLogarithmicBands(int bandCount, float minimumFrequency, float maximumFrequency, float gainDb = 0.0f, float q = 1.0f)
        {
            if (bandCount < 1)
                throw new ArgumentOutOfRangeException(nameof(bandCount));

            if (minimumFrequency <= 0.0f || minimumFrequency >= maximumFrequency)
                throw new ArgumentOutOfRangeException(nameof(minimumFrequency));

            if (maximumFrequency >= _sampleRate * 0.5f)
                throw new ArgumentOutOfRangeException(nameof(maximumFrequency));

            List<float> frequencies = [];

            for (int i = 0; i < bandCount; i++)
            {
                float position = bandCount == 1 ? 0.5f : (float)i / (bandCount - 1);
                float frequency = minimumFrequency * MathF.Pow(maximumFrequency / minimumFrequency, position);
                frequencies.Add(frequency);
            }

            ConfigureBands(frequencies, gainDb, q);
        }

        public void SetBandGain(int index, float gainDb)
        {
            JollyEqualiserBand band = _bands[index];
            SetBand(index, band.Frequency, gainDb, band.Q);
        }
    }
}
