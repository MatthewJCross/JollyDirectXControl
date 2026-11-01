using System.Numerics;

namespace Jolly2DEngine.Audio
{
    public enum JollyFourierWindow
    {
        Rectangular,
        Hann,
        Hamming,
        Blackman
    }

    public static class JollyFourierTransform
    {
        public static void Transform(ReadOnlySpan<float> samples, Span<Complex> spectrum, JollyFourierWindow window = JollyFourierWindow.Hann)
        {
            int length = samples.Length;

            if (length < 2 || (length & (length - 1)) != 0)
            {
                throw new ArgumentException("FFT input length must be a power of two greater than one.", nameof(samples));
            }

            if (spectrum.Length < length)
            {
                throw new ArgumentException("FFT output buffer is too small.", nameof(spectrum));
            }

            for (int i = 0; i < length; i++)
            {
                spectrum[i] = new Complex(samples[i] * GetWindowValue(i, length, window), 0.0);
            }

            int j = 0;

            for (int i = 1; i < length; i++)
            {
                int bit = length >> 1;

                while ((j & bit) != 0)
                {
                    j ^= bit;
                    bit >>= 1;
                }

                j ^= bit;

                if (i < j)
                {
                    (spectrum[i], spectrum[j]) = (spectrum[j], spectrum[i]);
                }
            }

            for (int size = 2; size <= length; size <<= 1)
            {
                int halfSize = size >> 1;
                Complex step = Complex.FromPolarCoordinates(1.0, -2.0 * System.Math.PI / size);

                for (int start = 0; start < length; start += size)
                {
                    Complex twiddle = Complex.One;

                    for (int k = 0; k < halfSize; k++)
                    {
                        Complex even = spectrum[start + k];
                        Complex odd = spectrum[start + k + halfSize] * twiddle;
                        spectrum[start + k] = even + odd;
                        spectrum[start + k + halfSize] = even - odd;
                        twiddle *= step;
                    }
                }
            }
        }

        public static void GetMagnitudeSpectrum(ReadOnlySpan<Complex> spectrum, Span<float> magnitudes)
        {
            int fftSize = spectrum.Length;

            if (fftSize < 2 || (fftSize & (fftSize - 1)) != 0)
            {
                throw new ArgumentException("FFT spectrum length must be a power of two greater than one.", nameof(spectrum));
            }

            int count = fftSize / 2 + 1;

            if (magnitudes.Length < count)
            {
                throw new ArgumentException("Magnitude output buffer is too small.", nameof(magnitudes));
            }

            for (int i = 0; i < count; i++)
            {
                float value = (float)(spectrum[i].Magnitude / fftSize);
                magnitudes[i] = i > 0 && i < fftSize / 2 ? value * 2.0f : value;
            }
        }

        public static float GetFrequencyForBin(int bin, int fftSize, int sampleRate)
        {
            if (fftSize < 2 || sampleRate <= 0 || bin < 0 || bin > fftSize / 2)
            {
                throw new ArgumentOutOfRangeException(nameof(bin));
            }

            return (float)bin * sampleRate / fftSize;
        }

        private static float GetWindowValue(int index, int length, JollyFourierWindow window)
        {
            if (window == JollyFourierWindow.Rectangular)
            {
                return 1.0f;
            }

            float phase = 2.0f * MathF.PI * index / (length - 1);

            return window switch
            {
                JollyFourierWindow.Hann => 0.5f - 0.5f * MathF.Cos(phase),
                JollyFourierWindow.Hamming => 0.54f - 0.46f * MathF.Cos(phase),
                JollyFourierWindow.Blackman => 0.42f - 0.5f * MathF.Cos(phase) + 0.08f * MathF.Cos(2.0f * phase),
                _ => throw new ArgumentOutOfRangeException(nameof(window))
            };
        }
    }
}
