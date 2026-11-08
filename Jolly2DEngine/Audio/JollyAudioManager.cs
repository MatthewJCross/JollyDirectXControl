using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace Jolly2DEngine.Audio
{
    public sealed class JollyAudioManager : IDisposable
    {
        private readonly Dictionary<string, CachedSound> _sounds = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<SoundInstance> _activeSounds = [];
        private readonly Lock _lock = new();

        private WaveOut? _musicOutput;
        private AudioFileReader? _musicReader;
        private JollyAudioEffects? _musicEffects;
        private string? _musicPath;

        private float _masterVolume = 1.0f;
        private float _musicVolume = 1.0f;
        private float _sfxVolume = 1.0f;
        private bool _muted;
        private float _musicSpeed = 1.0f;
        private bool _musicLoop;
        private bool _disposed;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = System.Math.Clamp(value, 0.0f, 1.0f);
                UpdateVolumes();
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = System.Math.Clamp(value, 0.0f, 1.0f);
                UpdateMusicVolume();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = System.Math.Clamp(value, 0.0f, 1.0f);
                UpdateSfxVolumes();
            }
        }

        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                UpdateVolumes();
            }
        }

        public double MusicProgress
        {
            get
            {
                if (_musicReader == null || _musicReader.TotalTime.TotalSeconds <= 0)
                {
                    return 0.0;
                }

                return _musicReader.CurrentTime.TotalSeconds / _musicReader.TotalTime.TotalSeconds;
            }
        }


        public bool IsMusicPlaying => _musicOutput?.PlaybackState == PlaybackState.Playing;
        public float MusicSpeed => _musicSpeed;
        public bool IsAnySoundPlaying
        {
            get
            {
                lock (_lock)
                {
                    return _activeSounds.Count > 0;
                }
            }
        }
        public string? CurrentMusic => _musicPath;
        public JollyAudioEffects? MusicEffects => _musicEffects;
        public TimeSpan MusicPosition => _musicReader?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan MusicDuration => _musicReader?.TotalTime ?? TimeSpan.Zero;


        public void LoadSound(string name, string path)
        {
            ThrowIfDisposed();

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            string key = Path.GetFileNameWithoutExtension(name);
            string fullPath = Path.GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Audio file was not found.", fullPath);
            }

            lock (_lock)
            {
                if (_sounds.ContainsKey(key))
                {
                    return;
                }

                _sounds.Add(key, new CachedSound(key, fullPath));
            }
        }

        public void UnloadSound(string name)
        {
            ThrowIfDisposed();

            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            string key = Path.GetFileNameWithoutExtension(name);

            lock (_lock)
            {
                _sounds.Remove(key);
            }
        }

        public JollySoundPlayback PlaySound(string name)
        {
            ThrowIfDisposed();

            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            string key = Path.GetFileNameWithoutExtension(name);

            CachedSound sound;

            lock (_lock)
            {
                if (!_sounds.TryGetValue(key, out sound!))
                {
                    throw new KeyNotFoundException($"Sound '{name}' has not been loaded.");
                }
            }

            return PlaySoundCore(sound, null);
        }

        public JollySoundPlaybackGroup PlaySound(string name, int count)
        {
            ThrowIfDisposed();

            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Sound playback count must be greater than zero.");
            }

            string key = Path.GetFileNameWithoutExtension(name);

            CachedSound sound;

            lock (_lock)
            {
                if (!_sounds.TryGetValue(key, out sound!))
                {
                    throw new KeyNotFoundException($"Sound '{name}' has not been loaded.");
                }
            }

            JollySoundPlaybackGroup group = new(count);

            for (int index = 0; index < count; index++)
            {
                PlaySoundCore(sound, group);
            }

            return group;
        }

        public bool IsSoundPlaying(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            string key = Path.GetFileNameWithoutExtension(name);

            lock (_lock)
            {
                return _activeSounds.Any(instance => string.Equals(instance.Sound.Name, key, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void StopAllSounds()
        {
            SoundInstance[] instances;

            lock (_lock)
            {
                if (_activeSounds.Count == 0)
                {
                    return;
                }

                instances = [.. _activeSounds];
                _activeSounds.Clear();
            }

            foreach (SoundInstance instance in instances)
            {
                CompleteSoundInstance(instance, true);
            }
        }

        public void PlayMusic(string path, bool loop = true, JollyAudioEffects? effects = null)
        {
            ThrowIfDisposed();

            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            string fullPath = Path.GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Music file was not found.", fullPath);
            }

            StopMusic();

            _musicPath = fullPath;
            _musicReader = new AudioFileReader(fullPath)
            {
                Volume = GetMusicVolume()
            };
            _musicOutput = new WaveOut();
            _musicEffects = effects;

            if (effects == null)
            {
                if (loop)
                {
                    _musicOutput.Init(new LoopStream(_musicReader));
                }
                else
                {
                    _musicOutput.Init(_musicReader);
                }
            }
            else
            {
                ISampleProvider source = loop ? new WaveToSampleProvider(new LoopStream(_musicReader)) : _musicReader;
                ISampleProvider processed = effects.CreateSampleProvider(source);
                _musicOutput.Init(new SampleToWaveProvider(processed));
            }

            _musicOutput.Play();
        }

        public void SeekMusic(TimeSpan position)
        {
            if (_musicReader == null)
            {
                return;
            }

            if (position < TimeSpan.Zero)
            {
                position = TimeSpan.Zero;
            }

            if (position > _musicReader.TotalTime)
            {
                position = _musicReader.TotalTime;
            }

            _musicReader.CurrentTime = position;
        }

        public void PauseMusic()
        {
            _musicOutput?.Pause();
        }

        public void ResumeMusic()
        {
            _musicOutput?.Play();
        }

        public void StopMusic()
        {
            WaveOut? output = _musicOutput;
            AudioFileReader? reader = _musicReader;

            _musicOutput = null;
            _musicReader = null;
            _musicPath = null;
            _musicEffects = null;

            output?.Stop();
            output?.Dispose();
            reader?.Dispose();
        }

        public void SetMusicSpeed(float speed)
        {
            ThrowIfDisposed();

            if (!float.IsFinite(speed) || speed < 0.5f || speed > 2.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speed), "Music speed must be between 0.5 and 2.0.");
            }

            if (MathF.Abs(_musicSpeed - speed) < 0.001f)
            {
                return;
            }

            _musicSpeed = speed;

            if (_musicReader == null)
            {
                return;
            }

            PlaybackState playbackState = _musicOutput?.PlaybackState ?? PlaybackState.Stopped;

            _musicOutput?.Stop();

            TimeSpan position = _musicReader.CurrentTime;

            _musicOutput?.Dispose();
            _musicOutput = null;

            _musicReader.CurrentTime = position;

            CreateMusicOutput(playbackState == PlaybackState.Playing, playbackState == PlaybackState.Paused);
        }

        private void CreateMusicOutput(bool play, bool pause)
        {
            if (_musicReader == null)
            {
                return;
            }

            ISampleProvider source = _musicLoop ? new WaveToSampleProvider(new LoopStream(_musicReader)) : _musicReader;

            if (_musicEffects != null)
            {
                source = _musicEffects.CreateSampleProvider(source);
            }

            source = new PlaybackRateSampleProvider(source, _musicSpeed);

            _musicOutput = new WaveOut();
            _musicOutput.Init(new SampleToWaveProvider(source));

            if (play || pause)
            {
                _musicOutput.Play();
            }

            if (pause)
            {
                _musicOutput.Pause();
            }
        }

        public void ClearSounds()
        {
            StopAllSounds();

            lock (_lock)
            {
                _sounds.Clear();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            StopMusic();
            StopAllSounds();

            lock (_lock)
            {
                _sounds.Clear();
            }
        }

        private JollySoundPlayback PlaySoundCore(CachedSound sound, JollySoundPlaybackGroup? group)
        {
            JollySoundPlayback playback = group == null ? new JollySoundPlayback() : new JollySoundPlayback(group.PlaybackFinished);
            SoundInstance instance = new(sound, GetSfxVolume(), playback);

            lock (_lock)
            {
                if (_disposed)
                {
                    instance.Dispose();
                    throw new ObjectDisposedException(nameof(JollyAudioManager));
                }

                _activeSounds.Add(instance);
                instance.Output.PlaybackStopped += OnSoundPlaybackStopped;
            }

            instance.Start();

            return playback;
        }

        private void OnSoundPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (sender is not WaveOut output)
            {
                return;
            }

            SoundInstance? instance = null;

            lock (_lock)
            {
                for (int index = 0; index < _activeSounds.Count; index++)
                {
                    SoundInstance candidate = _activeSounds[index];

                    if (!ReferenceEquals(candidate.Output, output))
                    {
                        continue;
                    }

                    instance = candidate;
                    _activeSounds.RemoveAt(index);
                    break;
                }
            }

            if (instance == null)
            {
                return;
            }

            CompleteSoundInstance(instance, false);

            if (e.Exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"AUDIO PLAYBACK ERROR: {e.Exception}");
            }
        }

        private void CompleteSoundInstance(SoundInstance instance, bool stopped)
        {
            if (!instance.TryBeginCleanup())
            {
                return;
            }

            instance.Output.PlaybackStopped -= OnSoundPlaybackStopped;

            if (stopped)
            {
                instance.Stop();
            }

            instance.Playback.MarkFinished();
            instance.Dispose();
        }

        private void UpdateVolumes()
        {
            UpdateMusicVolume();
            UpdateSfxVolumes();
        }

        private void UpdateMusicVolume()
        {
            AudioFileReader? reader = _musicReader;

            if (reader == null)
            {
                return;
            }

            reader.Volume = GetMusicVolume();
        }

        private void UpdateSfxVolumes()
        {
            float volume = GetSfxVolume();

            SoundInstance[] instances;

            lock (_lock)
            {
                instances = [.. _activeSounds];
            }

            foreach (SoundInstance instance in instances)
            {
                instance.SetVolume(volume);
            }
        }

        private float GetMusicVolume()
        {
            if (_muted)
            {
                return 0.0f;
            }

            return _masterVolume * _musicVolume;
        }

        private float GetSfxVolume()
        {
            if (_muted)
            {
                return 0.0f;
            }

            return _masterVolume * _sfxVolume;
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        private sealed class SoundInstance : IDisposable
        {
            private readonly Lock _stateLock = new();
            private bool _cleanupStarted;
            private bool _started;
            private bool _disposed;

            public CachedSound Sound { get; }

            public WaveOut Output { get; }

            public JollySoundPlayback Playback { get; }

            public SoundInstance(CachedSound sound, float volume, JollySoundPlayback playback)
            {
                Sound = sound;
                Playback = playback;

                Output = new WaveOut();
                Output.Init(new CachedSoundSampleProvider(sound));
                Output.Volume = volume;
            }

            public void Start()
            {
                lock (_stateLock)
                {
                    if (_cleanupStarted || _disposed)
                    {
                        return;
                    }

                    _started = true;
                    Output.Play();
                }
            }

            public void Stop()
            {
                lock (_stateLock)
                {
                    if (_disposed || !_started)
                    {
                        return;
                    }

                    Output.Stop();
                }
            }

            public void SetVolume(float volume)
            {
                lock (_stateLock)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    Output.Volume = volume;
                }
            }

            public bool TryBeginCleanup()
            {
                lock (_stateLock)
                {
                    if (_cleanupStarted)
                    {
                        return false;
                    }

                    _cleanupStarted = true;
                    return true;
                }
            }

            public void Dispose()
            {
                lock (_stateLock)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _disposed = true;
                    Output.Dispose();
                }
            }
        }

        private sealed class CachedSound
        {
            public string Name { get; }
            public WaveFormat WaveFormat { get; }
            public float[] AudioData { get; }

            public CachedSound(string name, string audioFileName)
            {
                Name = name;
                using AudioFileReader reader = new(audioFileName);

                WaveFormat = reader.WaveFormat;

                int bytesPerSample = WaveFormat.BitsPerSample / 8;
                int channels = WaveFormat.Channels;

                List<float> data = [];
                byte[] buffer = new byte[WaveFormat.SampleRate * channels * bytesPerSample];

                int bytesRead;

                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    int sampleCount = bytesRead / bytesPerSample;

                    for (int i = 0; i < sampleCount; i++)
                    {
                        int offset = i * bytesPerSample;

                        if (bytesPerSample == 4)
                        {
                            data.Add(BitConverter.ToSingle(buffer, offset));
                        }
                        else if (bytesPerSample == 2)
                        {
                            short sample = BitConverter.ToInt16(buffer, offset);
                            data.Add(sample / 32768.0f);
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported audio format: {WaveFormat.BitsPerSample}-bit.");
                        }
                    }
                }

                AudioData = [.. data];
            }
        }

        private sealed class CachedSoundSampleProvider(CachedSound sound) : ISampleProvider
        {
            private int _position;

            public WaveFormat WaveFormat => sound.WaveFormat;

            public int Read(Span<float> buffer)
            {
                int availableSamples = sound.AudioData.Length - _position;

                if (availableSamples <= 0)
                {
                    return 0;
                }

                int samplesToCopy = System.Math.Min(buffer.Length, availableSamples);

                sound.AudioData.AsSpan(_position, samplesToCopy).CopyTo(buffer);

                _position += samplesToCopy;

                return samplesToCopy;
            }
        }

        private sealed class LoopStream(WaveStream source) : WaveStream
        {
            public override WaveFormat WaveFormat => source.WaveFormat;

            public override long Length => source.Length;

            public override long Position
            {
                get => source.Position;
                set => source.Position = value;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalBytesRead = 0;

                while (totalBytesRead < count)
                {
                    int bytesRead = source.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

                    if (bytesRead == 0)
                    {
                        source.Position = 0;
                        continue;
                    }

                    totalBytesRead += bytesRead;
                }

                return totalBytesRead;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    source.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        private sealed class LoopingSampleProvider(ISampleProvider source, Action rewind) : ISampleProvider
        {
            public WaveFormat WaveFormat => source.WaveFormat;

            public int Read(Span<float> buffer)
            {
                int totalSamplesRead = 0;

                while (totalSamplesRead < buffer.Length)
                {
                    int samplesRead = source.Read(buffer[totalSamplesRead..]);

                    if (samplesRead == 0)
                    {
                        rewind();

                        samplesRead = source.Read(buffer[totalSamplesRead..]);

                        if (samplesRead == 0)
                        {
                            break;
                        }
                    }

                    totalSamplesRead += samplesRead;
                }

                return totalSamplesRead;
            }
        }

        private sealed class PlaybackRateSampleProvider : ISampleProvider
        {
            private readonly ISampleProvider _source;

            public WaveFormat WaveFormat { get; }

            public PlaybackRateSampleProvider(ISampleProvider source, float speed)
            {
                _source = source;
                int sampleRate = (int)MathF.Round(source.WaveFormat.SampleRate * speed);
                WaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, source.WaveFormat.Channels);
            }

            public int Read(Span<float> buffer)
            {
                return _source.Read(buffer);
            }
        }
    }

    public sealed class JollySoundPlayback
    {
        private readonly Action? _onFinished;
        private int _finished;

        internal JollySoundPlayback(Action? onFinished = null)
        {
            _onFinished = onFinished;
        }

        public bool IsFinished => Volatile.Read(ref _finished) != 0;

        internal void MarkFinished()
        {
            if (Interlocked.Exchange(ref _finished, 1) != 0)
            {
                return;
            }

            _onFinished?.Invoke();
        }
    }

    public sealed class JollySoundPlaybackGroup
    {
        private readonly int _playCount;
        private int _completedCount;

        internal JollySoundPlaybackGroup(int playCount)
        {
            _playCount = playCount;
        }

        public int PlayCount => _playCount;

        public int CompletedCount => Volatile.Read(ref _completedCount);

        public bool IsFinished => CompletedCount >= PlayCount;

        internal void PlaybackFinished()
        {
            Interlocked.Increment(ref _completedCount);
        }
    }
}