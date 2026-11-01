namespace Jolly2DEngine.Audio
{
    public sealed class JollySpectrumData
    {
        public int SampleRate { get; }
        public int FftSize { get; }
        public float[] Magnitudes { get; }
        public float[] Frequencies { get; }
        public float Rms { get; }
        public float Peak { get; }

        internal JollySpectrumData(int sampleRate, int fftSize, float[] magnitudes, float[] frequencies, float rms, float peak)
        {
            SampleRate = sampleRate;
            FftSize = fftSize;
            Magnitudes = magnitudes;
            Frequencies = frequencies;
            Rms = rms;
            Peak = peak;
        }
    }
}
