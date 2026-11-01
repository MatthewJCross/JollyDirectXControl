
using Jolly2DEngine.Audio;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace JollyAmp.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private const float SpectrumMinimumFrequency = 35.0f;
        private const float SpectrumMaximumFrequency = 16000.0f;
        private const float SpectrumMinimumMagnitude = 0.00001f;
        
        private static readonly float[] EqualiserFrequencies =
        [
            30.0f,
            60.0f,
            125.0f,
            250.0f,
            500.0f,
            1000.0f,
            2000.0f,
            4000.0f,
            8000.0f,
            16000.0f
        ];

        private readonly JollyAudioManager _audio;
        private readonly DispatcherTimer _updateTimer;

        private TimeSpan _lastSpectrumRenderTime;
        private bool _hasSpectrumRenderTime;
        
        private JollyAudioEffects? _effects;
        private string? _currentTrackPath;
        private TrackItem? _selectedTrack;
        private string _trackTitle = "No track loaded";
        private string _trackArtist = "Open an audio file to begin";
        private string _playbackStatus = "STOPPED";
        private float _volume = 0.8f;
        private float _pitchSemitones;
        private bool _paused;
        private bool _disposed;

        private bool _echoEnabled;
        private float _echoDelayMilliseconds = 350.0f;
        private float _echoFeedback = 0.35f;
        private float _echoWetMix = 0.25f;

        private bool _chorusEnabled;
        private float _chorusRate = 0.8f;
        private float _chorusDepth = 8.0f;
        private float _chorusWetMix = 0.3f;

        private bool _reverbEnabled;
        private JollyReverbPreset _reverbPreset = JollyReverbPreset.Hall;
        private float _reverbDecayTime = 2.5f;
        private float _reverbDamping = 0.35f;
        private float _reverbPreDelayMilliseconds = 20.0f;
        private float _reverbWetMix = 0.25f;

        private int _currentSampleRate = 44100;

        private double _titleUpdateElapsed;
        private int _lastTitleBpm = -1;

        private double _playbackPosition;
        private double _trackDuration;
        private bool _isSeeking;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TrackItem> Playlist { get; } = [];

        public ObservableCollection<EqualiserBand> EqualiserBands { get; } = [];

        public ObservableCollection<SpectrumBar> SpectrumBars { get; } = [];

        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PreviousTrackCommand { get; }
        public ICommand NextTrackCommand { get; }
        public ICommand ResetEqualiserCommand { get; }
        public ICommand AddEqualiserBandCommand { get; }
        public ICommand RemoveEqualiserBandCommand { get; }

        public VuMeterDisplay LeftVuMeter { get; } = new(24);
        public VuMeterDisplay RightVuMeter { get; } = new(24);

        #region Properties
        public string TrackTitle
        {
            get => _trackTitle;
            private set => SetProperty(ref _trackTitle, value);
        }

        public string TrackArtist
        {
            get => _trackArtist;
            private set => SetProperty(ref _trackArtist, value);
        }

        public string PlaybackStatus
        {
            get => _playbackStatus;
            private set => SetProperty(ref _playbackStatus, value);
        }

        public TrackItem? SelectedTrack
        {
            get => _selectedTrack;
            set => SetProperty(ref _selectedTrack, value);
        }

        public float Volume
        {
            get => _volume;
            set
            {
                float newValue = Math.Clamp(value, 0.0f, 1.0f);

                if (SetProperty(ref _volume, newValue))
                {
                    _audio.MusicVolume = newValue;
                }
            }
        }

        public float PitchSemitones
        {
            get => _pitchSemitones;
            set
            {
                if (SetProperty(ref _pitchSemitones, value) && _effects != null)
                {
                    _effects.PitchShifter.Enabled = true;
                    _effects.PitchShifter.PitchSemitones = value;
                }
            }
        }

        public bool EchoEnabled
        {
            get => _echoEnabled;
            set
            {
                if (SetProperty(ref _echoEnabled, value))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float EchoDelayMilliseconds
        {
            get => _echoDelayMilliseconds;
            set
            {
                if (SetProperty(ref _echoDelayMilliseconds, Math.Clamp(value, 1.0f, 2000.0f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float EchoFeedback
        {
            get => _echoFeedback;
            set
            {
                if (SetProperty(ref _echoFeedback, Math.Clamp(value, 0.0f, 0.95f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float EchoWetMix
        {
            get => _echoWetMix;
            set
            {
                if (SetProperty(ref _echoWetMix, Math.Clamp(value, 0.0f, 1.0f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public bool ChorusEnabled
        {
            get => _chorusEnabled;
            set
            {
                if (SetProperty(ref _chorusEnabled, value))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float ChorusRate
        {
            get => _chorusRate;
            set
            {
                if (SetProperty(ref _chorusRate, Math.Clamp(value, 0.05f, 5.0f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float ChorusDepth
        {
            get => _chorusDepth;
            set
            {
                if (SetProperty(ref _chorusDepth, Math.Clamp(value, 0.0f, 10.0f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float ChorusWetMix
        {
            get => _chorusWetMix;
            set
            {
                if (SetProperty(ref _chorusWetMix, Math.Clamp(value, 0.0f, 1.0f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public bool ReverbEnabled
        {
            get => _reverbEnabled;
            set
            {
                if (SetProperty(ref _reverbEnabled, value))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public JollyReverbPreset ReverbPreset
        {
            get => _reverbPreset;
            set
            {
                if (SetProperty(ref _reverbPreset, value))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float ReverbDecayTime
        {
            get => _reverbDecayTime;
            set
            {
                if (SetProperty(ref _reverbDecayTime, Math.Clamp(value, 0.1f, 20.0f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float ReverbDamping
        {
            get => _reverbDamping;
            set
            {
                if (SetProperty(ref _reverbDamping, Math.Clamp(value, 0.0f, 0.95f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float ReverbPreDelayMilliseconds
        {
            get => _reverbPreDelayMilliseconds;
            set
            {
                if (SetProperty(ref _reverbPreDelayMilliseconds, Math.Clamp(value, 0.0f, 200.0f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public float ReverbWetMix
        {
            get => _reverbWetMix;
            set
            {
                if (SetProperty(ref _reverbWetMix, Math.Clamp(value, 0.0f, 1.0f)))
                {
                    ApplyAudioEffects();
                }
            }
        }

        public string WindowTitle
        {
            get
            {
                float bpm = _effects?.Analyser.Bpm ?? 0.0f;
                return bpm >= 60.0f ? $"JollyAMP - Digital Audio Player | {bpm:0} BPM" : "JollyAMP - Digital Audio Player";
            }
        }
        public double PlaybackPosition
        {
            get => _playbackPosition;
            set
            {
                double newValue = Math.Clamp(value, 0.0, TrackDuration);

                if (SetProperty(ref _playbackPosition, newValue))
                {
                    _audio.SeekMusic(TimeSpan.FromSeconds(newValue));
                    OnPropertyChanged(nameof(PlaybackPositionText));
                }
            }
        }

        public double TrackDuration
        {
            get => _trackDuration;
            private set
            {
                if (SetProperty(ref _trackDuration, value))
                {
                    OnPropertyChanged(nameof(TrackDurationText));
                }
            }
        }

        public bool IsSeeking
        {
            get => _isSeeking;
            set => SetProperty(ref _isSeeking, value);
        }
        #endregion

        public int NumberOfSpectrumBars { get; } = 32;

        public int PlaylistCount => Playlist.Count;
        public bool IsPlaying => _audio.IsMusicPlaying;
        public bool IsPaused => _paused;
        public Array ReverbPresets => Enum.GetValues(typeof(JollyReverbPreset));
        public string PlaybackPositionText => FormatPlaybackTime(TimeSpan.FromSeconds(PlaybackPosition));
        public string TrackDurationText => FormatPlaybackTime(TimeSpan.FromSeconds(TrackDuration));

        public MainViewModel()
        {
            _audio = new JollyAudioManager();
            _audio.MusicVolume = _volume;

            ConfigureEqualiserBands(EqualiserFrequencies);
            
            Color[] spectrumColours =
            [
                Colors.DeepSkyBlue,
                Colors.LimeGreen,
                Colors.Yellow,
                Colors.Orange,
                Colors.Red
            ];

            for (int i = 0; i < NumberOfSpectrumBars; i++)
            {
                int colourIndex = Math.Min(spectrumColours.Length - 1, i * spectrumColours.Length / NumberOfSpectrumBars);
                SpectrumBars.Add(new SpectrumBar(new SolidColorBrush(spectrumColours[colourIndex])));
            }

            PlayPauseCommand = new RelayCommand(TogglePlayback);
            StopCommand = new RelayCommand(StopPlayback);
            PreviousTrackCommand = new RelayCommand(PlayPreviousTrack);
            NextTrackCommand = new RelayCommand(PlayNextTrack);
            ResetEqualiserCommand = new RelayCommand(ResetEqualiser);
            AddEqualiserBandCommand = new RelayCommand(AddEqualiserBand);
            RemoveEqualiserBandCommand = new RelayCommand(RemoveEqualiserBand);

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (e is not RenderingEventArgs args)
            {
                return;
            }

            double deltaSeconds = _hasSpectrumRenderTime ? (args.RenderingTime - _lastSpectrumRenderTime).TotalSeconds : 1.0 / 60.0;

            _lastSpectrumRenderTime = args.RenderingTime;
            _hasSpectrumRenderTime = true;

            deltaSeconds = Math.Clamp(deltaSeconds, 0.001, 0.05);

            UpdateVuMeters(deltaSeconds);
            UpdateSpectrum(deltaSeconds);
            UpdateWindowTitle(deltaSeconds);
        }

        private static string FormatPlaybackTime(TimeSpan time)
        {
            return time.TotalHours >= 1.0 ? time.ToString(@"h\:mm\:ss") : time.ToString(@"m\:ss");
        }

        public void ConfigureEqualiserBands(IEnumerable<float> frequencies)
        {
            ArgumentNullException.ThrowIfNull(frequencies);

            float[] frequencyList = frequencies.ToArray();

            if (frequencyList.Any(frequency => !float.IsFinite(frequency) || frequency <= 0.0f))
            {
                throw new ArgumentException("All equaliser frequencies must be positive and finite.", nameof(frequencies));
            }

            Color[] colours =
            [
                Colors.DeepSkyBlue,
                Colors.LimeGreen,
                Colors.Gold,
                Colors.Orange,
                Colors.OrangeRed,
                Colors.MediumPurple,
                Colors.Cyan
            ];

            EqualiserBands.Clear();

            for (int i = 0; i < frequencyList.Length; i++)
            {
                float frequency = frequencyList[i];
                string label = frequency >= 1000.0f ? $"{frequency / 1000.0f:0.##} kHz" : $"{frequency:0} Hz";
                EqualiserBands.Add(new EqualiserBand(frequency, label, new SolidColorBrush(colours[i % colours.Length]), SetEqualiserBand));
            }
        }

        public void SetPlaylist(IEnumerable<string> paths, bool playFirst = true)
        {
            ArgumentNullException.ThrowIfNull(paths);

            Playlist.Clear();

            foreach (string path in paths)
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    continue;
                }

                string fullPath = Path.GetFullPath(path);

                Playlist.Add(new TrackItem(fullPath, Path.GetFileNameWithoutExtension(fullPath)));
            }

            OnPropertyChanged(nameof(PlaylistCount));

            SelectedTrack = Playlist.FirstOrDefault();

            if (playFirst && SelectedTrack != null)
            {
                PlaySelectedTrack();
            }
            else
            {
                StopPlayback();
                TrackTitle = "No track loaded";
                TrackArtist = "Select a track to begin";
            }
        }

        public void PlaySelectedTrack()
        {
            if (SelectedTrack == null)
            {
                return;
            }

            PlayTrack(SelectedTrack.Path);
        }

        public void PlayTrack(string path, bool loop = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!File.Exists(path))
            {
                return;
            }

            using AudioFileReader reader = new(path);

            int sampleRate = reader.WaveFormat.SampleRate;
            int channels = reader.WaveFormat.Channels;
            _currentSampleRate = sampleRate;

            _audio.StopMusic();
            _effects?.Dispose();
            _effects = null;
            JollyAudioEffects effects = new(sampleRate, channels, 1024);

            foreach (EqualiserBand band in EqualiserBands)
            {
                band.EffectIndex = -1;
            }

            effects.Equaliser.Enabled = true;
            effects.PitchShifter.Enabled = PitchSemitones != 0.0f;
            effects.PitchShifter.PitchSemitones = PitchSemitones;
            effects.Analyser.Enabled = true;

            _effects = effects;
            RebuildEqualiser();
            ApplyAudioEffects();

            _currentTrackPath = Path.GetFullPath(path);
            _audio.PlayMusic(_currentTrackPath, loop, _effects);
            _paused = false;
            TrackDuration = _audio.MusicDuration.TotalSeconds;
            PlaybackPosition = 0.0;

            TrackTitle = Path.GetFileNameWithoutExtension(path);
            TrackArtist = Path.GetExtension(path).TrimStart('.').ToUpperInvariant() + " AUDIO FILE";

            PlaybackStatus = "PLAYING";

            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
        }

        public void TogglePlayback()
        {
            if (_audio.IsMusicPlaying)
            {
                _audio.PauseMusic();
                _paused = true;
            }
            else if (_paused)
            {
                _audio.ResumeMusic();
                _paused = false;
            }
            else if (_currentTrackPath != null)
            {
                PlayTrack(_currentTrackPath);
            }

            UpdatePlaybackStatus();
        }

        public void StopPlayback()
        {
            _audio.StopMusic();
            _paused = false;
            PlaybackPosition = 0.0;
            TrackDuration = 0.0;

            UpdatePlaybackStatus();
        }

        public void PlayNextTrack()
        {
            if (Playlist.Count == 0)
            {
                return;
            }

            int index = SelectedTrack == null ? -1 : Playlist.IndexOf(SelectedTrack);
            index = (index + 1) % Playlist.Count;

            SelectedTrack = Playlist[index];
            PlaySelectedTrack();
        }

        public void PlayPreviousTrack()
        {
            if (Playlist.Count == 0)
            {
                return;
            }

            int index = SelectedTrack == null ? 0 : Playlist.IndexOf(SelectedTrack);
            index = (index - 1 + Playlist.Count) % Playlist.Count;

            SelectedTrack = Playlist[index];
            PlaySelectedTrack();
        }

        public void ResetEqualiser()
        {
            foreach (EqualiserBand band in EqualiserBands)
            {
                band.GainDb = 0.0f;
            }
        }

        private void SetEqualiserBand(EqualiserBand band, bool frequencyChanged)
        {
            if (frequencyChanged)
            {
                ReorderEqualiserBand(band);
                return;
            }

            ApplyEqualiserBand(band);
        }

        private void ApplyEqualiserBand(EqualiserBand band)
        {
            if (_effects == null || band.EffectIndex < 0)
            {
                return;
            }

            if (band.Frequency <= 0.0f || band.Frequency >= _currentSampleRate * 0.5f)
            {
                return;
            }

            _effects.Equaliser.SetBand(band.EffectIndex, band.Frequency, band.GainDb, 1.0f);
        }

        private void AddEqualiserBand()
        {
            float frequency = EqualiserBands.Count == 0 ? 1000.0f : 900.0f;

            ConfigureNewEqualiserBand(frequency);
        }

        private void RemoveEqualiserBand(object? parameter)
        {
            if (parameter is not EqualiserBand band)
            {
                return;
            }

            if (!EqualiserBands.Remove(band))
            {
                return;
            }

            RebuildEqualiser();
        }

        private void ConfigureNewEqualiserBand(float frequency)
        {
            Color[] colours =
            [
                Colors.DeepSkyBlue,
                Colors.LimeGreen,
                Colors.Gold,
                Colors.Orange,
                Colors.OrangeRed,
                Colors.MediumPurple,
                Colors.Cyan
            ];

            int colourIndex = EqualiserBands.Count % colours.Length;

            EqualiserBand band = new(frequency, GetFrequencyLabel(frequency), new SolidColorBrush(colours[colourIndex]), SetEqualiserBand);

            int insertIndex = 0;
            while (insertIndex < EqualiserBands.Count && EqualiserBands[insertIndex].Frequency < band.Frequency)
            {
                insertIndex++;
            }
            EqualiserBands.Insert(insertIndex, band);

            RebuildEqualiser();
        }

        private void ReorderEqualiserBand(EqualiserBand band)
        {
            int currentIndex = EqualiserBands.IndexOf(band);

            if (currentIndex < 0)
            {
                return;
            }

            int newIndex = 0;

            for (int i = 0; i < EqualiserBands.Count; i++)
            {
                if (i == currentIndex)
                {
                    continue;
                }

                if (EqualiserBands[i].Frequency < band.Frequency)
                {
                    newIndex++;
                }
            }

            if (currentIndex != newIndex)
            {
                EqualiserBands.Move(currentIndex, newIndex);
            }

            RebuildEqualiser();
        }

        private static string GetFrequencyLabel(float frequency)
        {
            return frequency >= 1000.0f ? $"{frequency / 1000.0f:0.##} kHz" : $"{frequency:0} Hz";
        }

        private void RebuildEqualiser()
        {
            if (_effects == null)
            {
                foreach (EqualiserBand band in EqualiserBands)
                {
                    band.EffectIndex = -1;
                }

                return;
            }

            _effects.Equaliser.ClearBands();

            int sampleRate = _effects.Analyser.SampleRate;

            foreach (EqualiserBand band in EqualiserBands)
            {
                band.EffectIndex = -1;

                if (band.Frequency > 0.0f && band.Frequency < sampleRate * 0.5f)
                {
                    band.EffectIndex = _effects.Equaliser.AddBand(band.Frequency, band.GainDb, 1.0f);
                }
            }
        }

        private void ApplyAudioEffects()
        {
            if (_effects == null)
            {
                return;
            }

            _effects.Echo.Enabled = EchoEnabled;
            _effects.Echo.DelayMilliseconds = EchoDelayMilliseconds;
            _effects.Echo.Feedback = EchoFeedback;
            _effects.Echo.WetMix = EchoWetMix;

            _effects.Chorus.Enabled = ChorusEnabled;
            _effects.Chorus.Rate = ChorusRate;
            _effects.Chorus.Depth = ChorusDepth;
            _effects.Chorus.WetMix = ChorusWetMix;

            _effects.Reverb.Preset = ReverbPreset;
            _effects.Reverb.DecayTime = ReverbDecayTime;
            _effects.Reverb.Damping = ReverbDamping;
            _effects.Reverb.PreDelayMilliseconds = ReverbPreDelayMilliseconds;
            _effects.Reverb.WetMix = ReverbWetMix;
            _effects.Reverb.Enabled = ReverbEnabled;
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdatePlaybackStatus();
            UpdatePlaybackProgress();
        }

        private void UpdatePlaybackStatus()
        {
            string status;

            if (_audio.IsMusicPlaying)
            {
                status = "PLAYING";
            }
            else if (_paused)
            {
                status = "PAUSED";
            }
            else
            {
                status = "STOPPED";
            }

            PlaybackStatus = status;

            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
        }

        private void UpdateSpectrum(double deltaSeconds)
        {
            JollySpectrumData? spectrum = _effects?.Analyser.GetSpectrum();

            if (spectrum == null || spectrum.Frequencies.Length == 0 || spectrum.Magnitudes.Length == 0)
            {
                foreach (SpectrumBar bar in SpectrumBars)
                {
                    bar.SetLevel(0.0, deltaSeconds);
                }

                return;
            }

            int binCount = Math.Min(spectrum.Frequencies.Length, spectrum.Magnitudes.Length);

            float minimumFrequency = SpectrumMinimumFrequency;
            float maximumFrequency = Math.Min(SpectrumMaximumFrequency, spectrum.SampleRate * 0.5f);

            if (maximumFrequency <= minimumFrequency)
            {
                return;
            }

            float logarithmicRange = MathF.Log(maximumFrequency / minimumFrequency);
            int barCount = SpectrumBars.Count;

            for (int i = 0; i < barCount; i++)
            {
                float lowerPosition = i / (float)barCount;
                float upperPosition = (i + 1) / (float)barCount;

                float lowerFrequency = minimumFrequency * MathF.Exp(logarithmicRange * lowerPosition);
                float upperFrequency = minimumFrequency * MathF.Exp(logarithmicRange * upperPosition);

                float magnitudeSum = 0.0f;
                int magnitudeCount = 0;

                float nearestMagnitude = 0.0f;
                float nearestDifference = float.MaxValue;
                float centreFrequency = MathF.Sqrt(lowerFrequency * upperFrequency);

                for (int bin = 1; bin < binCount; bin++)
                {
                    float frequency = spectrum.Frequencies[bin];
                    float magnitude = spectrum.Magnitudes[bin];

                    if (frequency >= lowerFrequency && frequency < upperFrequency)
                    {
                        magnitudeSum += magnitude * magnitude;
                        magnitudeCount++;
                    }

                    float difference = MathF.Abs(frequency - centreFrequency);

                    if (difference < nearestDifference)
                    {
                        nearestDifference = difference;
                        nearestMagnitude = magnitude;
                    }
                }

                float level = magnitudeCount > 0 ? MathF.Sqrt(magnitudeSum / magnitudeCount) : nearestMagnitude;

                float decibels = 20.0f * MathF.Log10(MathF.Max(level, SpectrumMinimumMagnitude));
                float normalised = Math.Clamp((decibels + 90.0f) / 90.0f, 0.0f, 1.0f);
                float response = MathF.Pow(normalised, 1.6f);
                double height = 2.0 + (response * 108.0f);

                SpectrumBars[i].SetLevel(height, deltaSeconds);
            }
        }

        private void UpdateVuMeters(double deltaSeconds)
        {
            float left = 0.0f;
            float right = 0.0f;

            if (_audio.IsMusicPlaying && _effects?.AnalyserProvider is JollyAnalyserSampleProvider provider)
            {
                left = provider.LeftLevel;
                right = provider.RightLevel;
            }

            LeftVuMeter.Update(left, deltaSeconds);
            RightVuMeter.Update(right, deltaSeconds);
        }

        private void UpdateWindowTitle(double deltaSeconds)
        {
            _titleUpdateElapsed += deltaSeconds;

            if (_titleUpdateElapsed < 0.5)
            {
                return;
            }

            _titleUpdateElapsed = 0.0;

            float bpm = _effects?.Analyser.Bpm ?? 0.0f;
            int displayedBpm = bpm >= 60.0f ? (int)MathF.Round(bpm) : 0;

            if (displayedBpm == _lastTitleBpm)
            {
                return;
            }

            _lastTitleBpm = displayedBpm;

            OnPropertyChanged(nameof(WindowTitle));
        }

        private void UpdatePlaybackProgress()
        {
            if (IsSeeking)
            {
                return;
            }

            double position = _audio.MusicPosition.TotalSeconds;

            if (Math.Abs(position - _playbackPosition) >= 0.05)
            {
                _playbackPosition = position;
                OnPropertyChanged(nameof(PlaybackPosition));
                OnPropertyChanged(nameof(PlaybackPositionText));
            }
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            CompositionTarget.Rendering -= CompositionTarget_Rendering; 
            
            _updateTimer.Stop();
            _updateTimer.Tick -= UpdateTimer_Tick;

            _audio.Dispose();
            _effects?.Dispose();
            _effects = null;
        }
    }

    public sealed class TrackItem
    {
        public string Path { get; }
        public string Title { get; }

        public TrackItem(string path, string title)
        {
            Path = path;
            Title = title;
        }
    }

    public sealed class EqualiserBand : INotifyPropertyChanged
    {
        private readonly Action<EqualiserBand, bool> _valueChanged;
        private float _frequency;
        private float _gainDb;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Brush Colour { get; }
        public int EffectIndex { get; set; } = -1;

        public float Frequency
        {
            get => _frequency;
            set
            {
                if (!float.IsFinite(value) || value < 20.0f || value > 20000.0f)
                {
                    return;
                }

                if (Math.Abs(_frequency - value) < 0.1f)
                {
                    return;
                }

                _frequency = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Frequency)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));

                _valueChanged(this, true);
            }
        }

        public string Label => Frequency >= 1000.0f ? $"{Frequency / 1000.0f:0.##} kHz" : $"{Frequency:0} Hz";

        public float GainDb
        {
            get => _gainDb;
            set
            {
                float newValue = Math.Clamp(value, -12.0f, 12.0f);

                if (Math.Abs(_gainDb - newValue) < 0.001f)
                {
                    return;
                }

                _gainDb = newValue;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GainDb)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GainText)));

                _valueChanged(this, false);
            }
        }

        public string GainText => $"{GainDb:+0.0;-0.0;0.0} dB";

        public EqualiserBand(float frequency, string label, Brush colour, Action<EqualiserBand, bool> valueChanged)
        {
            _frequency = frequency;
            Colour = colour;
            _valueChanged = valueChanged;
        }
    }

    public sealed class SpectrumBar : INotifyPropertyChanged
    {
        private double _height = 2.0;
        public event PropertyChangedEventHandler? PropertyChanged;
        public Brush Fill { get; }

        public double Height
        {
            get => _height;
            private set
            {
                if (Math.Abs(_height - value) < 0.1)
                {
                    return;
                }

                _height = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Height)));
            }
        }

        public SpectrumBar(Brush fill)
        {
            Fill = fill;
        }

        public void SetLevel(double height, double deltaSeconds)
        {
            double responseTime = height > _height ? 0.045 : 0.16;
            double smoothing = 1.0 - Math.Exp(-deltaSeconds / responseTime);

            double smoothedHeight = _height + ((height - _height) * smoothing);

            Height = Math.Clamp(smoothedHeight, 2.0, 110.0);
        }
    }

    public sealed class VuMeterDisplay
    {
        private float _level;
        private float _peakLevel;
        private double _peakHoldTime;

        public ObservableCollection<VuMeterSegment> Segments { get; } = [];

        public VuMeterDisplay(int segmentCount)
        {
            Color[] colours =
            [
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.LimeGreen,
                Colors.Yellow,
                Colors.Yellow,
                Colors.Yellow,
                Colors.Yellow,
                Colors.Orange,
                Colors.Orange,
                Colors.OrangeRed,
                Colors.Red,
                Colors.Red
            ];

            for (int i = 0; i < segmentCount; i++)
            {
                int colourIndex = i * colours.Length / segmentCount;
                Segments.Add(new VuMeterSegment(new SolidColorBrush(colours[colourIndex])));
            }
        }

        public void Update(float inputLevel, double deltaSeconds)
        {
            inputLevel = Math.Clamp(inputLevel, 0.0f, 1.0f);

            double attackTime = 0.025;
            double decayTime = 0.20;
            double responseTime = inputLevel > _level ? attackTime : decayTime;
            float smoothing = (float)(1.0 - Math.Exp(-deltaSeconds / responseTime));

            _level += (inputLevel - _level) * smoothing;

            if (_level > _peakLevel)
            {
                _peakLevel = _level;
                _peakHoldTime = 0.6;
            }
            else
            {
                _peakHoldTime -= deltaSeconds;

                if (_peakHoldTime <= 0.0)
                {
                    _peakLevel = Math.Max(0.0f, _peakLevel - (float)(deltaSeconds * 0.65));
                }
            }

            for (int i = 0; i < Segments.Count; i++)
            {
                float threshold = (i + 1) / (float)Segments.Count;
                float lowerThreshold = i / (float)Segments.Count;

                Segments[i].Update(_level >= threshold, _peakLevel > lowerThreshold && _peakLevel <= threshold);
            }
        }
    }

    public sealed class VuMeterSegment : INotifyPropertyChanged
    {
        private bool _isLit;
        private bool _isPeak;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Brush Colour { get; }

        public bool IsLit
        {
            get => _isLit;
            private set
            {
                if (_isLit == value)
                {
                    return;
                }

                _isLit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLit)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Opacity)));
            }
        }

        public bool IsPeak
        {
            get => _isPeak;
            private set
            {
                if (_isPeak == value)
                {
                    return;
                }

                _isPeak = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPeak)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Opacity)));
            }
        }

        public double Opacity => IsLit || IsPeak ? 1.0 : 0.18;
        
        public VuMeterSegment(Brush colour)
        {
            Colour = colour;
        }

        public void Update(bool isLit, bool isPeak)
        {
            IsLit = isLit;
            IsPeak = isPeak;
        }
    }
}
