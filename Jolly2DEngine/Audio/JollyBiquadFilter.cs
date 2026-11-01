namespace Jolly2DEngine.Audio
{
    public enum JollyBiquadFilterType
    {
        LowPass,
        HighPass,
        Peaking,
        LowShelf,
        HighShelf
    }

    public sealed class JollyBiquadFilter
    {
        private float _b0 = 1.0f;
        private float _b1;
        private float _b2;
        private float _a1;
        private float _a2;
        private float _z1;
        private float _z2;

        public float ProcessSample(float input)
        {
            float output = _b0 * input + _z1;
            _z1 = _b1 * input - _a1 * output + _z2;
            _z2 = _b2 * input - _a2 * output;
            return output;
        }

        public void Reset()
        {
            _z1 = 0.0f;
            _z2 = 0.0f;
        }

        public void SetLowPass(float sampleRate, float frequency, float q = 0.7071f)
        {
            Validate(sampleRate, frequency, q);
            float omega = 2.0f * MathF.PI * frequency / sampleRate;
            float cos = MathF.Cos(omega);
            float alpha = MathF.Sin(omega) / (2.0f * q);
            SetCoefficients((1.0f - cos) * 0.5f, 1.0f - cos, (1.0f - cos) * 0.5f, 1.0f + alpha, -2.0f * cos, 1.0f - alpha);
        }

        public void SetHighPass(float sampleRate, float frequency, float q = 0.7071f)
        {
            Validate(sampleRate, frequency, q);
            float omega = 2.0f * MathF.PI * frequency / sampleRate;
            float cos = MathF.Cos(omega);
            float alpha = MathF.Sin(omega) / (2.0f * q);
            SetCoefficients((1.0f + cos) * 0.5f, -(1.0f + cos), (1.0f + cos) * 0.5f, 1.0f + alpha, -2.0f * cos, 1.0f - alpha);
        }

        public void SetPeaking(float sampleRate, float frequency, float gainDb, float q = 1.0f)
        {
            Validate(sampleRate, frequency, q);
            if (!float.IsFinite(gainDb)) throw new ArgumentOutOfRangeException(nameof(gainDb));
            float a = MathF.Pow(10.0f, gainDb / 40.0f);
            float omega = 2.0f * MathF.PI * frequency / sampleRate;
            float cos = MathF.Cos(omega);
            float alpha = MathF.Sin(omega) / (2.0f * q);
            SetCoefficients(1.0f + alpha * a, -2.0f * cos, 1.0f - alpha * a, 1.0f + alpha / a, -2.0f * cos, 1.0f - alpha / a);
        }

        public void SetLowShelf(float sampleRate, float frequency, float gainDb) => SetShelf(sampleRate, frequency, gainDb, false);

        public void SetHighShelf(float sampleRate, float frequency, float gainDb) => SetShelf(sampleRate, frequency, gainDb, true);

        private void SetShelf(float sampleRate, float frequency, float gainDb, bool highShelf)
        {
            Validate(sampleRate, frequency, 0.7071f);
            if (!float.IsFinite(gainDb)) throw new ArgumentOutOfRangeException(nameof(gainDb));
            float a = MathF.Pow(10.0f, gainDb / 40.0f);
            float omega = 2.0f * MathF.PI * frequency / sampleRate;
            float cos = MathF.Cos(omega);
            float sin = MathF.Sin(omega);
            float alpha = sin * 0.5f * MathF.Sqrt(2.0f);
            float beta = 2.0f * MathF.Sqrt(a) * alpha;
            float b0, b1, b2, a0, a1, a2;

            if (!highShelf)
            {
                b0 = a * ((a + 1.0f) - (a - 1.0f) * cos + beta);
                b1 = 2.0f * a * ((a - 1.0f) - (a + 1.0f) * cos);
                b2 = a * ((a + 1.0f) - (a - 1.0f) * cos - beta);
                a0 = (a + 1.0f) + (a - 1.0f) * cos + beta;
                a1 = -2.0f * ((a - 1.0f) + (a + 1.0f) * cos);
                a2 = (a + 1.0f) + (a - 1.0f) * cos - beta;
            }
            else
            {
                b0 = a * ((a + 1.0f) + (a - 1.0f) * cos + beta);
                b1 = -2.0f * a * ((a - 1.0f) + (a + 1.0f) * cos);
                b2 = a * ((a + 1.0f) + (a - 1.0f) * cos - beta);
                a0 = (a + 1.0f) - (a - 1.0f) * cos + beta;
                a1 = 2.0f * ((a - 1.0f) - (a + 1.0f) * cos);
                a2 = (a + 1.0f) - (a - 1.0f) * cos - beta;
            }

            SetCoefficients(b0, b1, b2, a0, a1, a2);
        }

        private void SetCoefficients(float b0, float b1, float b2, float a0, float a1, float a2)
        {
            _b0 = b0 / a0;
            _b1 = b1 / a0;
            _b2 = b2 / a0;
            _a1 = a1 / a0;
            _a2 = a2 / a0;
        }

        private static void Validate(float sampleRate, float frequency, float q)
        {
            if (!float.IsFinite(sampleRate) || sampleRate <= 0.0f) throw new ArgumentOutOfRangeException(nameof(sampleRate));
            if (!float.IsFinite(frequency) || frequency <= 0.0f || frequency >= sampleRate * 0.5f) throw new ArgumentOutOfRangeException(nameof(frequency));
            if (!float.IsFinite(q) || q <= 0.0f) throw new ArgumentOutOfRangeException(nameof(q));
        }
    }
}
