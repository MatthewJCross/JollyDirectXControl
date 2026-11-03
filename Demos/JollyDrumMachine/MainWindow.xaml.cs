using Jolly2DEngine.Audio;
using JollyDrumMachine.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using JollyDrumMachine.Services;
using JollyDrumMachine.ViewModels;

namespace JollyDrumMachine
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();
        private readonly DrumSequencerService _sequencer = new();
        private readonly JollyAudioManager _audio = new();
        private readonly ObservableCollection<SampleItem> _library = [];
        private readonly ObservableCollection<DrumInstrument> _tracks = [];
        private readonly Dictionary<DrumInstrument, JollyAudioManager> _trackAudio = [];
        private readonly DispatcherTimer _uiTimer = new() { Interval = TimeSpan.FromMilliseconds(25) };
        private readonly CancellationTokenSource _lifetime = new();
        private CancellationTokenSource? _playbackCancellation;
        private Task? _playbackTask;
        private bool _dragStarted;
        private bool _allowClose;
        private readonly string _userSamplesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JollyDrums", "Samples");

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            Directory.CreateDirectory(_userSamplesFolder);
            InstrumentLibrary.ItemsSource = _library;
            BuildStepHeader();
            LoadBuiltInSamples();
            AddInitialTrack("Kick");
            AddInitialTrack("Snare");
            AddInitialTrack("Closed Hat");
            AddInitialTrack("Clap");
            SetDefaultPattern();
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();
            MasterVolumeSlider_ValueChanged(this, null!);
        }

        private void LoadBuiltInSamples()
        {
            string folder = Path.Combine(AppContext.BaseDirectory, "Assets", "Samples");
            if (!Directory.Exists(folder)) return;
            foreach (string path in Directory.GetFiles(folder).Where(IsSupportedAudio).OrderBy(Path.GetFileName))
            {
                try 
                { 
                    RegisterSample(path, Path.GetFileNameWithoutExtension(path));
                }
                catch (Exception ex)
                {
                    _viewModel.Status = $"Could not load {Path.GetFileName(path)}: {ex.Message}"; 
                }
            }
        }

        private void RegisterSample(string path, string? displayName = null)
        {
            string fullPath = Path.GetFullPath(path);
            string key = Path.GetFileNameWithoutExtension(fullPath);
            if (_library.Any(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)))
            {
                key = $"{key}_{Guid.NewGuid().ToString("N")[..8]}";
            }
            _audio.LoadSound(key, fullPath);
            _library.Add(new SampleItem(displayName ?? Path.GetFileNameWithoutExtension(fullPath), fullPath, key));
        }

        private void AddInitialTrack(string sampleName)
        {
            SampleItem? sample = _library.FirstOrDefault(item => item.Name.Equals(sampleName, StringComparison.OrdinalIgnoreCase));
            if (sample != null) 
                AddTrack(sample, false);
        }

        private void AddTrack(SampleItem sample, bool report = true)
        {
            if (_tracks.Count >= 24)
            {
                _viewModel.Status = "Track limit reached (24).";
                return;
            }

            DrumInstrument track = new(sample.Name, sample.Path, sample.Key);
            _tracks.Add(track);
            BuildTrackRow(track);
            DropHint.Visibility = Visibility.Collapsed;
            if (report) 
                _viewModel.Status = $"Added {sample.Name}.";
        }

        private void BuildStepHeader()
        {
            StepHeader.Children.Clear();
            StepHeader.ColumnDefinitions.Clear();
            StepHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
            for (int i = 0; i < 16; i++) 
                StepHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });

            TextBlock label = new() { Text = "TRACK / LEVEL", Foreground = Brush("#8DA2B4"), FontSize = 10, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(label, 0);
            StepHeader.Children.Add(label);
            for (int i = 0; i < 16; i++)
            {
                TextBlock step = new() { Text = (i + 1).ToString("00"), HorizontalAlignment = HorizontalAlignment.Center, Foreground = i % 4 == 0 ? Brush("#55D6BE") : Brush("#8DA2B4"), FontSize = 10 };
                Grid.SetColumn(step, i + 1);
                StepHeader.Children.Add(step);
            }
        }

        private void BuildTrackRow(DrumInstrument track)
        {
            Grid row = new() { Height = 48, Margin = new Thickness(0, 0, 0, 5), Tag = track, AllowDrop = true, Background = Brush("#172430") };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
            for (int i = 0; i < 16; i++) row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
            Border left = new() { BorderBrush = Brush("#2A4051"), BorderThickness = new Thickness(0, 0, 1, 0), Padding = new Thickness(7, 2, 5, 2) };
            StackPanel details = new() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            Button preview = new() { Content = "▶", Padding = new Thickness(5, 2, 5, 2), Margin = new Thickness(0, 0, 6, 0), ToolTip = "Preview sample" };
            preview.Click += (_, _) => PlayPreview(track);
            StackPanel nameAndVolume = new() { Width = 138 };
            TextBlock name = new() { Text = track.Name, FontWeight = FontWeights.SemiBold, FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis };
            JollyAudioManager trackAudio = EnsureTrackAudio(track);
            Slider volume = new() { Minimum = 0, Maximum = 1, Value = track.Volume, Height = 16, Margin = new Thickness(0, 1, 0, 0), ToolTip = "Track volume" };
            volume.ValueChanged += (_, e) => { track.Volume = (float)e.NewValue; trackAudio.SfxVolume = track.Volume; };
            nameAndVolume.Children.Add(name);
            nameAndVolume.Children.Add(volume);
            Button mute = new() { Content = "M", Padding = new Thickness(5, 2, 5, 2), Margin = new Thickness(4, 0, 0, 0), ToolTip = "Mute track" };
            mute.Click += (_, _) => { track.Muted = !track.Muted; mute.Background = track.Muted ? Brush("#8E3945") : Brush("#223545"); };
            Button remove = new() { Content = "×", Padding = new Thickness(5, 2, 5, 2), Margin = new Thickness(2, 0, 0, 0), ToolTip = "Remove track" };
            remove.Click += (_, _) => RemoveTrack(track, row);
            details.Children.Add(preview); details.Children.Add(nameAndVolume); details.Children.Add(mute); details.Children.Add(remove);
            left.Child = details;
            Grid.SetColumn(left, 0);
            row.Children.Add(left);

            for (int i = 0; i < 16; i++)
            {
                int stepIndex = i;
                Button pad = new() { Margin = new Thickness(2), Padding = new Thickness(0), BorderThickness = new Thickness(1), BorderBrush = Brush(i % 4 == 0 ? "#3C596D" : "#263B4B"), Background = track.Steps[i] ? Brush("#55D6BE") : Brush(i % 4 == 0 ? "#243746" : "#1C2B38"), Tag = stepIndex, ToolTip = $"{track.Name}, step {i + 1}" };
                pad.Click += (_, _) => { track.Steps[stepIndex] = !track.Steps[stepIndex]; UpdatePad(pad, track.Steps[stepIndex], stepIndex); };
                Grid.SetColumn(pad, i + 1);
                row.Children.Add(pad);
            }
            row.DragOver += TrackRow_DragOver;
            row.Drop += TrackRow_Drop;
            TrackPanel.Children.Add(row);
        }

        private void RemoveTrack(DrumInstrument track, Grid row)
        {
            _tracks.Remove(track);
            if (_trackAudio.Remove(track, out JollyAudioManager? manager)) manager.Dispose();
            TrackPanel.Children.Remove(row);
            DropHint.Visibility = _tracks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdatePad(Button pad, bool active, int stepIndex)
        {
            pad.Background = active ? Brush("#55D6BE") : Brush(stepIndex % 4 == 0 ? "#243746" : "#1C2B38");
            pad.BorderBrush = Brush(stepIndex % 4 == 0 ? "#3C596D" : "#263B4B");
        }

        private void SetDefaultPattern()
        {
            foreach (DrumInstrument track in _tracks)
            {
                if (track.Name.Contains("Kick", StringComparison.OrdinalIgnoreCase)) foreach (int i in new[] { 0, 4, 8, 12 }) track.Steps[i] = true;
                else if (track.Name.Contains("Snare", StringComparison.OrdinalIgnoreCase) || track.Name.Contains("Clap", StringComparison.OrdinalIgnoreCase)) foreach (int i in new[] { 4, 12 }) track.Steps[i] = true;
                else if (track.Name.Contains("Hat", StringComparison.OrdinalIgnoreCase)) for (int i = 0; i < 16; i += 2) track.Steps[i] = true;
            }
            RebuildTrackPanel();
        }

        private void RebuildTrackPanel()
        {
            TrackPanel.Children.Clear();
            foreach (DrumInstrument track in _tracks) BuildTrackRow(track);
            DropHint.Visibility = _tracks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private JollyAudioManager EnsureTrackAudio(DrumInstrument track)
        {
            if (_trackAudio.TryGetValue(track, out JollyAudioManager? existing)) return existing;
            JollyAudioManager manager = new() { MasterVolume = (float)_viewModel.MasterVolume, SfxVolume = track.Volume };
            manager.LoadSound(track.SampleKey, track.SamplePath);
            _trackAudio.Add(track, manager);
            return manager;
        }

        private void PlayPreview(DrumInstrument track)
        {
            try
            {
                EnsureTrackAudio(track).PlaySound(track.SampleKey);
                _viewModel.Status = $"Previewing {track.Name}.";
            }
            catch (Exception ex) { _viewModel.Status = $"Audio error: {ex.Message}"; }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playbackTask is { IsCompleted: false }) return;
            _playbackCancellation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
            CancellationToken token = _playbackCancellation.Token;
            _viewModel.CurrentStep = -1;
            _viewModel.IsPlaying = true;
            _playbackTask = Task.Run(() => PlaybackLoopAsync(token), token);
        }

        private Task PlaybackLoopAsync(CancellationToken token)
        {
            return _sequencer.RunAsync(
                () => _viewModel.Tempo,
                () => Dispatcher.Invoke(() => _tracks.ToArray()),
                track => Dispatcher.Invoke(() => EnsureTrackAudio(track).PlaySound(track.SampleKey)),
                step => Dispatcher.InvokeAsync(() => _viewModel.CurrentStep = step),
                token);
        }

        private void StopPlayback()
        {
            _playbackCancellation?.Cancel();
            _audio.StopAllSounds();
            foreach (JollyAudioManager manager in _trackAudio.Values) manager.StopAllSounds();
            _viewModel.IsPlaying = false;
            _viewModel.CurrentStep = -1;
            UpdatePlayhead();
        }

        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            UpdatePlayhead();
        }

        private void UpdatePlayhead()
        {
            foreach (Grid row in TrackPanel.Children.OfType<Grid>())
            {
                foreach (Button pad in row.Children.OfType<Button>().Where(button => button.Tag is int))
                {
                    int step = (int)pad.Tag;
                    bool active = row.Tag is DrumInstrument track && track.Steps[step];
                    if (step == _viewModel.CurrentStep)
                    {
                        pad.BorderBrush = Brush("#FFFFFF");
                        pad.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        pad.BorderBrush = Brush(step % 4 == 0 ? "#3C596D" : "#263B4B");
                        pad.BorderThickness = new Thickness(1);
                    }
                    if (active) pad.Background = Brush(step == _viewModel.CurrentStep ? "#B5FFF0" : "#55D6BE");
                    else pad.Background = Brush(step == _viewModel.CurrentStep ? "#526B7B" : step % 4 == 0 ? "#243746" : "#1C2B38");
                }
            }
        }

        private void TempoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel != null) _viewModel.Tempo = TempoSlider.Value;
            TempoText.Text = $"{TempoSlider.Value:0} BPM";
        }
        private void MasterVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _viewModel.MasterVolume = MasterVolumeSlider?.Value ?? 0.85;
            foreach (JollyAudioManager manager in _trackAudio.Values) manager.MasterVolume = (float)_viewModel.MasterVolume;
        }
        private void StopButton_Click(object sender, RoutedEventArgs e) => StopPlayback();

        private void ClearPattern_Click(object sender, RoutedEventArgs e)
        {
            foreach (DrumInstrument track in _tracks) Array.Clear(track.Steps);
            RebuildTrackPanel();
            _viewModel.Status = "Pattern cleared.";
        }

        private void InstrumentLibrary_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) { _dragStarted = false; return; }
            if (InstrumentLibrary.SelectedItem is not SampleItem sample) return;
            if (_dragStarted) return;
            _dragStarted = true;
            DragDrop.DoDragDrop(InstrumentLibrary, sample, DragDropEffects.Copy);
            _dragStarted = false;
        }

        private void TrackPanel_DragOver(object sender, DragEventArgs e) => e.Effects = e.Data.GetDataPresent(typeof(SampleItem)) ? DragDropEffects.Copy : DragDropEffects.None;
        private void TrackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(SampleItem)) is SampleItem sample) AddTrack(sample);
        }
        private void TrackRow_DragOver(object sender, DragEventArgs e) => e.Effects = e.Data.GetDataPresent(typeof(SampleItem)) ? DragDropEffects.Copy : DragDropEffects.None;
        private void TrackRow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(SampleItem)) is not SampleItem sample || sender is not Grid row || row.Tag is not DrumInstrument oldTrack) return;
            e.Handled = true;
            int index = _tracks.IndexOf(oldTrack);
            AddTrackAt(sample, index);
        }
        private void AddTrackAt(SampleItem sample, int index)
        {
            if (_tracks.Count >= 24) { _viewModel.Status = "Track limit reached (24)."; return; }
            DrumInstrument track = new(sample.Name, sample.Path, sample.Key);
            _tracks.Insert(index, track);
            RebuildTrackPanel();
            _viewModel.Status = $"Added {sample.Name}.";
        }
        private void InstrumentLibrary_Drop(object sender, DragEventArgs e) { }

        private void AddSample_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new() { Title = "Add drum sample", Filter = "Audio files|*.wav;*.mp3;*.aiff;*.aif;*.wma;*.mp4|All files|*.*", Multiselect = true };
            if (dialog.ShowDialog(this) != true) return;
            foreach (string source in dialog.FileNames)
            {
                try
                {
                    string destination = Path.Combine(_userSamplesFolder, Path.GetFileName(source));
                    if (!string.Equals(Path.GetFullPath(source), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
                    {
                        string stem = Path.GetFileNameWithoutExtension(destination);
                        string extension = Path.GetExtension(destination);
                        int suffix = 1;
                        while (File.Exists(destination)) destination = Path.Combine(_userSamplesFolder, $"{stem}_{suffix++}{extension}");
                        File.Copy(source, destination, false);
                    }
                    RegisterSample(destination);
                }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, "Could not add sample", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
            _viewModel.Status = "Samples added to the instrument library.";
        }

        private void SavePattern_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new() { Filter = "JollyDrums pattern (*.jdp.json)|*.jdp.json|JSON file (*.json)|*.json", FileName = "MyBeat.jdp.json" };
            if (dialog.ShowDialog(this) != true) return;
            PatternFile pattern = new() { Tempo = (int)_viewModel.Tempo, Instruments = _tracks.Select(track => new PatternInstrument { Name = track.Name, SamplePath = track.SamplePath, Volume = track.Volume, Muted = track.Muted, Steps = track.Steps.ToArray() }).ToList() };
            try { File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(pattern, new JsonSerializerOptions { WriteIndented = true })); _viewModel.Status = $"Saved {Path.GetFileName(dialog.FileName)}."; }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void LoadPattern_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new() { Filter = "JollyDrums pattern (*.jdp.json;*.json)|*.jdp.json;*.json|All files|*.*" };
            if (dialog.ShowDialog(this) != true) return;
            try
            {
                PatternFile? pattern = JsonSerializer.Deserialize<PatternFile>(File.ReadAllText(dialog.FileName));
                if (pattern == null) throw new InvalidDataException("The selected pattern is empty or invalid.");
                StopPlayback();
                _tracks.Clear();
                foreach (PatternInstrument item in pattern.Instruments)
                {
                    string path = item.SamplePath;
                    if (!File.Exists(path))
                    {
                        string local = Path.Combine(_userSamplesFolder, Path.GetFileName(path));
                        string builtIn = Path.Combine(AppContext.BaseDirectory, "Assets", "Samples", Path.GetFileName(path));
                        path = File.Exists(local) ? local : builtIn;
                    }
                    if (!File.Exists(path)) continue;
                    SampleItem? sample = _library.FirstOrDefault(entry => string.Equals(entry.Path, path, StringComparison.OrdinalIgnoreCase));
                    if (sample == null) { RegisterSample(path, item.Name); sample = _library.Last(); }
                    DrumInstrument track = new(item.Name, sample.Path, sample.Key) { Volume = item.Volume, Muted = item.Muted, Steps = item.Steps.Length == 16 ? item.Steps.ToArray() : new bool[16] };
                    _tracks.Add(track);
                }
                _viewModel.Tempo = Math.Clamp(pattern.Tempo, 40, 240);
                TempoSlider.Value = _viewModel.Tempo;
                RebuildTrackPanel();
                _viewModel.Status = $"Loaded {Path.GetFileName(dialog.FileName)}.";
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Load failed", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private async void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_allowClose) return;

            e.Cancel = true;
            _uiTimer.Stop();
            _lifetime.Cancel();
            _playbackCancellation?.Cancel();

            if (_playbackTask != null)
            {
                try { await _playbackTask; }
                catch (OperationCanceledException) { }
                catch (Exception ex) { _viewModel.Status = $"Playback stopped with an error: {ex.Message}"; }
            }

            _audio.Dispose();
            foreach (JollyAudioManager manager in _trackAudio.Values) manager.Dispose();
            _trackAudio.Clear();
            _playbackCancellation?.Dispose();
            _lifetime.Dispose();
            _allowClose = true;
            Close();
        }

        private static bool IsSupportedAudio(string path) => new[] { ".wav", ".mp3", ".aiff", ".aif", ".wma", ".mp4" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        private static SolidColorBrush Brush(string colour) => new((Color)ColorConverter.ConvertFromString(colour));

        private sealed record SampleItem(string Name, string Path, string Key)
        {
            public override string ToString() => Name;
        }
    }
}