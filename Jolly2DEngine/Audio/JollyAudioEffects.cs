using NAudio.Wave;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyAudioEffects : IDisposable
    {
        private readonly int _sampleRate;
        private readonly int _channels;

        public JollyEqualiser Equaliser { get; }
        public JollyPitchShifter PitchShifter { get; }
        public JollyEcho Echo { get; }
        public JollyChorus Chorus { get; }
        public JollyReverb Reverb { get; }
        public JollyAudioAnalyser Analyser { get; }
        public JollyAnalyserSampleProvider? AnalyserProvider { get; private set; }

        private int AnalyserSampleRate => _sampleRate;
        private int EqualiserChannels => _channels;

        public JollyAudioEffects(int sampleRate, int channels, int fftSize = 2048, int pitchWindowSize = 2048)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            Equaliser = new JollyEqualiser(sampleRate, channels);
            PitchShifter = new JollyPitchShifter(channels, pitchWindowSize);
            Echo = new JollyEcho(sampleRate, channels);
            Chorus = new JollyChorus(sampleRate, channels);
            Reverb = new JollyReverb(sampleRate, channels);
            Analyser = new JollyAudioAnalyser(sampleRate, fftSize);
        }

        public ISampleProvider CreateSampleProvider(ISampleProvider source)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (source.WaveFormat.SampleRate <= 0 || source.WaveFormat.Channels <= 0)
            {
                throw new ArgumentException("The source has an invalid wave format.", nameof(source));
            }

            if (source.WaveFormat.SampleRate != AnalyserSampleRate || source.WaveFormat.Channels != EqualiserChannels)
            {
                throw new ArgumentException("The effects were created for a different sample rate or channel count.", nameof(source));
            }

            ISampleProvider result = new JollyEqualiserSampleProvider(source, Equaliser);
            result = new JollyPitchShiftSampleProvider(result, PitchShifter);
            result = new JollyChorusSampleProvider(result, Chorus);
            result = new JollyEchoSampleProvider(result, Echo);
            result = new JollyReverbSampleProvider(result, Reverb);
            AnalyserProvider = new JollyAnalyserSampleProvider(result, Analyser);
            return AnalyserProvider;
        }

        public void Dispose()
        {
            Analyser.Dispose();
        }
    }
}
