using JollyDrumMachine.Models;
using NAudio.Wave;
using System.IO;

namespace JollyDrumMachine.Services
{
    /// <summary>Renders a pattern arrangement offline to a stereo WAV file.</summary>
    public sealed class ArrangementRenderer
    {
        private const int SampleRate = 44100;

        public async Task RenderWavAsync(IReadOnlyList<PatternFile> patterns, IReadOnlyList<ArrangementEntry> arrangement, string outputPath, float masterVolume, double swing, bool humanise, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            if (arrangement.Count == 0)
                throw new InvalidOperationException("Add at least one pattern to the arrangement before exporting.");

            try
            {
                await Task.Run(() => RenderWav(patterns, arrangement, outputPath, masterVolume, swing, humanise, cancellationToken, progress), cancellationToken);
            }
            catch
            {
                if (cancellationToken.IsCancellationRequested && File.Exists(outputPath))
                    File.Delete(outputPath);
                throw;
            }
        }

        private static void RenderWav(IReadOnlyList<PatternFile> patterns, IReadOnlyList<ArrangementEntry> arrangement, string outputPath, float masterVolume, double swing, bool humanise, CancellationToken token, IProgress<double>? progress)
        {
            List<(int PatternIndex, int Repeat, int StartSample, int StepCount, double Tempo)> sections = [];
            int cursor = 0;

            foreach (ArrangementEntry entry in arrangement)
            {
                if (entry.PatternIndex < 0 || entry.PatternIndex >= patterns.Count)
                    continue;
                PatternFile pattern = patterns[entry.PatternIndex];
                int stepCount = Math.Clamp(pattern.StepsPerBar, 16, 64);
                double tempo = Math.Clamp(pattern.Tempo, 40, 240);
                int sectionSamples = (int)Math.Ceiling(stepCount * 60000.0 / tempo / 4.0 * SampleRate / 1000.0);
                for (int repeat = 0; repeat < Math.Clamp(entry.Repeats, 1, 64); repeat++)
                {
                    sections.Add((entry.PatternIndex, repeat, cursor, stepCount, tempo));
                    cursor += sectionSamples;
                }
            }

            if (cursor <= 0)
                throw new InvalidOperationException("The arrangement has no renderable sections.");

            Dictionary<string, float[]> sampleCache = new(StringComparer.OrdinalIgnoreCase);
            PatternInstrument[] allInstruments = patterns.SelectMany(pattern => pattern.Instruments).ToArray();
            int loadedInstrumentCount = 0;
            foreach (PatternFile pattern in patterns)
            {
                foreach (PatternInstrument instrument in pattern.Instruments)
                {
                    token.ThrowIfCancellationRequested();
                    loadedInstrumentCount++;
                    if (!File.Exists(instrument.SamplePath) || sampleCache.ContainsKey(instrument.SamplePath))
                    {
                        progress?.Report(allInstruments.Length == 0 ? 0.2 : 0.2 * loadedInstrumentCount / allInstruments.Length);
                        continue;
                    }
                    using AudioFileReader reader = new(instrument.SamplePath);
                    using MediaFoundationResampler resampler = new(reader, new WaveFormat(SampleRate, 16, 2)) { ResamplerQuality = 60 };
                    using MemoryStream pcm = new();
                    byte[] buffer = new byte[16384];
                    int read;
                    while ((read = resampler.Read(buffer.AsSpan())) > 0)
                        pcm.Write(buffer, 0, read);
                    byte[] raw = pcm.ToArray();
                    float[] decoded = new float[raw.Length / 2];
                    for (int i = 0; i < decoded.Length; i++)
                        decoded[i] = BitConverter.ToInt16(raw, i * 2) / 32768.0f;
                    sampleCache[instrument.SamplePath] = decoded;
                    progress?.Report(allInstruments.Length == 0 ? 0.2 : 0.2 * loadedInstrumentCount / allInstruments.Length);
                }
            }

            int tailFrames = sampleCache.Count == 0 ? 0 : checked(sampleCache.Values.Max(sample => sample.Length / 2) * 4);
            int totalFrames = checked(cursor + tailFrames);
            float[] mix = new float[checked(totalFrames * 2)];
            int renderedSectionCount = 0;
            foreach (var section in sections)
            {
                token.ThrowIfCancellationRequested();
                PatternFile pattern = patterns[section.PatternIndex];
                double stepDuration = SampleRate * 60.0 / section.Tempo / 4.0;
                bool hasSolo = pattern.Instruments.Any(instrument => instrument.Solo);
                foreach (PatternInstrument instrument in pattern.Instruments)
                {
                    if (instrument.Muted || (hasSolo && !instrument.Solo) || !sampleCache.TryGetValue(instrument.SamplePath, out float[]? source) || source.Length < 2)
                        continue;

                    double pitchRatio = Math.Pow(2.0, Math.Clamp(instrument.Tune, -24.0f, 24.0f) / 12.0);
                    int sourceFrames = source.Length / 2;
                    int renderedFrames = Math.Max(1, (int)Math.Ceiling(sourceFrames / pitchRatio));
                    int attackFrames = Math.Max(0, (int)(Math.Clamp(instrument.Attack, 0.0f, 1000.0f) * SampleRate / 1000.0));
                    int decayFrames = Math.Max(1, (int)(Math.Clamp(instrument.Decay, 1.0f, 10000.0f) * SampleRate / 1000.0));
                    double pan = Math.Clamp(instrument.Pan, -1.0f, 1.0f);
                    float leftPan = (float)Math.Cos((pan + 1.0) * Math.PI / 4.0);
                    float rightPan = (float)Math.Sin((pan + 1.0) * Math.PI / 4.0);

                    for (int step = 0; step < section.StepCount; step++)
                    {
                        if (step >= instrument.Steps.Length || !instrument.Steps[step])
                            continue;

                        double swingOffset = step % 2 == 1 ? stepDuration * Math.Clamp(swing, 0, 0.45) : 0;
                        double timingOffset = humanise ? Random.Shared.NextDouble() * stepDuration * 0.03 : 0;
                        int destination = Math.Max(0, section.StartSample + (int)(step * stepDuration + swingOffset + timingOffset));
                        int velocity = step < instrument.StepVelocities.Length ? instrument.StepVelocities[step] : 127;
                        if (humanise)
                            velocity = Math.Clamp(velocity + Random.Shared.Next(-8, 9), 1, 127);

                        float gain = Math.Clamp(instrument.Volume, 0, 1) * Math.Clamp(velocity / 127.0f, 0, 1) * Math.Clamp(masterVolume, 0, 1);
                        int framesToMix = Math.Min(renderedFrames, Math.Max(0, totalFrames - destination));
                        for (int frame = 0; frame < framesToMix; frame++)
                        {
                            int sourceFrame = Math.Min(sourceFrames - 1, (int)(frame * pitchRatio));
                            float envelope = attackFrames > 0 ? Math.Min(1.0f, frame / (float)attackFrames) : 1.0f;
                            if (instrument.Decay < 9999.0f)
                                envelope *= (float)Math.Exp(-frame / (double)decayFrames);
                            int sourceIndex = sourceFrame * 2;
                            int destinationIndex = (destination + frame) * 2;
                            mix[destinationIndex] += source[sourceIndex] * gain * envelope * leftPan;
                            mix[destinationIndex + 1] += source[sourceIndex + 1] * gain * envelope * rightPan;
                        }
                    }
                }
                renderedSectionCount++;
                progress?.Report(0.2 + 0.6 * renderedSectionCount / sections.Count);
            }

            WaveFormat format = new(SampleRate, 16, 2);
            using WaveFileWriter writer = new(outputPath, format);
            byte[] bytes = new byte[8192 * 4];
            int position = 0;
            while (position < mix.Length)
            {
                token.ThrowIfCancellationRequested();
                int floatCount = Math.Min(bytes.Length / 2, mix.Length - position);
                for (int i = 0; i < floatCount; i++)
                {
                    float value = Math.Clamp(mix[position + i], -1.0f, 1.0f);
                    short pcm = (short)Math.Round(value * short.MaxValue);
                    bytes[i * 2] = (byte)(pcm & 0xFF);
                    bytes[i * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
                }
                writer.Write(bytes, 0, floatCount * 2);
                position += floatCount;
                progress?.Report(0.8 + 0.2 * position / mix.Length);
            }
        }

    }
}
