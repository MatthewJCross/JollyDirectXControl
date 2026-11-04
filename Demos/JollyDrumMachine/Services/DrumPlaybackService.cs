using Jolly2DEngine.Audio;
using JollyDrumMachine.Models;
using NAudio.Wave;
using System.IO;

namespace JollyDrumMachine.Services
{
    /// <summary>
    /// Owns audio playback resources for individual drum instruments.
    /// </summary>
    public sealed class DrumPlaybackService : IDisposable
    {
        private readonly Dictionary<DrumInstrument, TrackAudio> _trackAudio = [];

        private float _masterVolume = 0.85f;
        private bool _disposed;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                ThrowIfDisposed();

                _masterVolume = Math.Clamp(value, 0.0f, 1.0f);

                foreach (TrackAudio audio in _trackAudio.Values)
                    audio.Manager.MasterVolume = _masterVolume;
            }
        }

        /// <summary>
        /// Plays an instrument, taking its mute, solo and volume settings into account.
        /// </summary>
        public void PlayTrack(DrumInstrument instrument, int velocity, IEnumerable<DrumInstrument> tracks)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNull(instrument);
            ArgumentNullException.ThrowIfNull(tracks);

            DrumInstrument[] trackList = tracks.ToArray();

            if (instrument.Muted)
                return;

            bool hasSolo = trackList.Any(track => track.Solo);

            if (hasSolo && !instrument.Solo)
                return;

            velocity = Math.Clamp(velocity, 0, 127);

            if (velocity == 0)
                return;

            TrackAudio audio = GetTrackAudio(instrument);

            audio.Manager.MasterVolume = _masterVolume;
            audio.Manager.SfxVolume = instrument.Volume * (velocity / 127.0f);
            audio.Manager.PlaySound(instrument.SampleKey);
        }

        /// <summary>
        /// Previews an instrument at its configured volume.
        /// </summary>
        public void PreviewTrack(DrumInstrument instrument)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNull(instrument);

            if (instrument.Muted)
                return;

            TrackAudio audio = GetTrackAudio(instrument);

            audio.Manager.MasterVolume = _masterVolume;
            audio.Manager.SfxVolume = instrument.Volume;

            audio.Manager.PlaySound(instrument.SampleKey);
        }

        /// <summary>
        /// Stops all sounds belonging to an instrument.
        /// </summary>
        public void StopTrack(DrumInstrument instrument)
        {
            if (_disposed)
                return;

            if (_trackAudio.TryGetValue(instrument, out TrackAudio? audio))
                audio.Manager.StopAllSounds();
        }

        /// <summary>
        /// Stops all instrument playback.
        /// </summary>
        public void StopAll()
        {
            if (_disposed)
                return;

            foreach (TrackAudio audio in _trackAudio.Values)
                audio.Manager.StopAllSounds();
        }

        /// <summary>
        /// Removes and disposes an instrument's audio resources.
        /// </summary>
        public void RemoveTrack(DrumInstrument instrument)
        {
            if (_disposed)
                return;

            if (!_trackAudio.Remove(instrument, out TrackAudio? audio))
                return;

            audio.Dispose();
        }

        /// <summary>
        /// Updates the cached sample if an instrument's sample has changed.
        /// </summary>
        public void RefreshTrack(DrumInstrument instrument)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNull(instrument);

            if (!_trackAudio.Remove(instrument, out TrackAudio? audio))
                return;

            audio.Dispose();
        }

        private TrackAudio GetTrackAudio(DrumInstrument instrument)
        {
            string settingsSignature = GetSettingsSignature(instrument);
            if (_trackAudio.TryGetValue(instrument, out TrackAudio? existing))
            {
                if (string.Equals(existing.SamplePath, instrument.SamplePath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existing.SampleKey, instrument.SampleKey, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existing.SettingsSignature, settingsSignature, StringComparison.Ordinal))
                    return existing;

                _trackAudio.Remove(instrument);
                existing.Dispose();
            }

            if (string.IsNullOrWhiteSpace(instrument.SamplePath))
                throw new InvalidOperationException($"Instrument '{instrument.Name}' has no sample assigned.");

            if (string.IsNullOrWhiteSpace(instrument.SampleKey))
                throw new InvalidOperationException($"Instrument '{instrument.Name}' has no sample key assigned.");

            JollyAudioManager manager = new()
            {
                MasterVolume = _masterVolume,
                SfxVolume = instrument.Volume
            };

            string processedPath = CreateProcessedSample(instrument);
            try
            {
                manager.LoadSound(instrument.SampleKey, processedPath);

                TrackAudio audio = new(manager, instrument.SamplePath, instrument.SampleKey, processedPath, settingsSignature);
                _trackAudio.Add(instrument, audio);
                return audio;
            }
            catch
            {
                manager.Dispose();
                if (!string.Equals(processedPath, instrument.SamplePath, StringComparison.OrdinalIgnoreCase) && File.Exists(processedPath))
                    File.Delete(processedPath);
                throw;
            }
        }

        public void ClearTracks()
        {
            if (_disposed)
                return;

            foreach (TrackAudio audio in _trackAudio.Values)
                audio.Dispose();

            _trackAudio.Clear();
        }


        private static string GetSettingsSignature(DrumInstrument instrument)
        {
            return string.Join("|", instrument.Pan.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                instrument.Tune.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                instrument.Attack.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                instrument.Decay.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
        }

        private static string CreateProcessedSample(DrumInstrument instrument)
        {
            if (!File.Exists(instrument.SamplePath))
                throw new FileNotFoundException("Instrument sample was not found.", instrument.SamplePath);

            // Avoid a temporary copy when no processing is required.
            if (Math.Abs(instrument.Pan) < 0.001f && Math.Abs(instrument.Tune) < 0.001f &&
                Math.Abs(instrument.Attack) < 0.001f && Math.Abs(instrument.Decay - 10000.0f) < 0.001f)
                return instrument.SamplePath;

            const int sampleRate = 44100;
            WaveFormat outputFormat = new(sampleRate, 16, 2);
            using AudioFileReader reader = new(instrument.SamplePath);
            using MediaFoundationResampler resampler = new(reader, outputFormat) { ResamplerQuality = 60 };
            using MemoryStream pcmStream = new();
            byte[] buffer = new byte[16384];
            int read;
            while ((read = resampler.Read(buffer.AsSpan())) > 0)
                pcmStream.Write(buffer, 0, read);

            byte[] raw = pcmStream.ToArray();
            int sourceFrames = raw.Length / 4;
            if (sourceFrames == 0)
                throw new InvalidDataException($"Sample '{instrument.Name}' contains no audio data.");

            float[] source = new float[sourceFrames * 2];
            for (int i = 0; i < source.Length; i++)
                source[i] = BitConverter.ToInt16(raw, i * 2) / 32768.0f;

            double pitchRatio = Math.Pow(2.0, Math.Clamp(instrument.Tune, -24.0f, 24.0f) / 12.0);
            int outputFrames = Math.Max(1, (int)Math.Ceiling(sourceFrames / pitchRatio));
            int attackFrames = Math.Max(0, (int)(Math.Clamp(instrument.Attack, 0.0f, 1000.0f) * sampleRate / 1000.0));
            int decayFrames = Math.Max(1, (int)(Math.Clamp(instrument.Decay, 1.0f, 10000.0f) * sampleRate / 1000.0));
            double pan = Math.Clamp(instrument.Pan, -1.0f, 1.0f);
            float leftPan = (float)Math.Cos((pan + 1.0) * Math.PI / 4.0);
            float rightPan = (float)Math.Sin((pan + 1.0) * Math.PI / 4.0);
            string outputPath = Path.Combine(Path.GetTempPath(), $"JollyDrums_{Guid.NewGuid():N}.wav");

            try
            {
                using WaveFileWriter writer = new(outputPath, outputFormat);
                byte[] output = new byte[8192 * 4];
                int frame = 0;
                while (frame < outputFrames)
                {
                    int frames = Math.Min(8192, outputFrames - frame);
                    for (int i = 0; i < frames; i++)
                    {
                        int currentFrame = frame + i;
                        int sourceFrame = Math.Min(sourceFrames - 1, (int)(currentFrame * pitchRatio));
                        float envelope = attackFrames > 0 ? Math.Min(1.0f, currentFrame / (float)attackFrames) : 1.0f;
                        if (instrument.Decay < 9999.0f)
                            envelope *= (float)Math.Exp(-currentFrame / (double)decayFrames);
                        float left = source[sourceFrame * 2] * envelope * leftPan;
                        float right = source[sourceFrame * 2 + 1] * envelope * rightPan;
                        short leftPcm = (short)Math.Round(Math.Clamp(left, -1.0f, 1.0f) * short.MaxValue);
                        short rightPcm = (short)Math.Round(Math.Clamp(right, -1.0f, 1.0f) * short.MaxValue);
                        int offset = i * 4;
                        output[offset] = (byte)(leftPcm & 0xFF);
                        output[offset + 1] = (byte)((leftPcm >> 8) & 0xFF);
                        output[offset + 2] = (byte)(rightPcm & 0xFF);
                        output[offset + 3] = (byte)((rightPcm >> 8) & 0xFF);
                    }
                    writer.Write(output, 0, frames * 4);
                    frame += frames;
                }
            }
            catch
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                throw;
            }

            return outputPath;
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (TrackAudio audio in _trackAudio.Values)
                audio.Dispose();

            _trackAudio.Clear();
        }

        private sealed class TrackAudio : IDisposable
        {
            public JollyAudioManager Manager { get; }
            public string SamplePath { get; }
            public string SampleKey { get; }
            public string ProcessedPath { get; }
            public string SettingsSignature { get; }

            public TrackAudio(JollyAudioManager manager, string samplePath, string sampleKey, string processedPath, string settingsSignature)
            {
                Manager = manager;
                SamplePath = samplePath;
                SampleKey = sampleKey;
                ProcessedPath = processedPath;
                SettingsSignature = settingsSignature;
            }

            public void Dispose()
            {
                Manager.Dispose();
                if (!string.Equals(ProcessedPath, SamplePath, StringComparison.OrdinalIgnoreCase) && File.Exists(ProcessedPath))
                    File.Delete(ProcessedPath);
            }
        }
    }
}

