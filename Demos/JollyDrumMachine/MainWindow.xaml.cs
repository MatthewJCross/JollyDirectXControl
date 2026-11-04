using JollyDrumMachine.Models;
using JollyDrumMachine.Services;
using JollyDrumMachine.ViewModels;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace JollyDrumMachine
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel = new();
        private readonly DrumSequencerService _sequencer = new();
        private readonly ObservableCollection<SampleItem> _library = [];
        private ObservableCollection<DrumInstrument> Tracks => _viewModel.Tracks;
        private ObservableCollection<ArrangementEntry> Arrangement => _viewModel.Arrangement;
        private readonly ProjectFileService _projectFiles = new();
        private readonly ArrangementExportService _exportService = new();
        private readonly string _settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JollyDrums");
        private readonly DrumPlaybackService _playback = new();
        private PatternFile?[] Patterns => _viewModel.Patterns;
        private int ActivePatternIndex { get => _viewModel.ActivePatternIndex; set => _viewModel.ActivePatternIndex = value; }
        private int StepsPerPattern { get => _viewModel.StepsPerPattern; set => _viewModel.StepsPerPattern = value; }
        private bool _changingStepCount;
        private readonly DispatcherTimer _uiTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(25)
        };
        private readonly CancellationTokenSource _lifetime = new();
        private CancellationTokenSource? _playbackCancellation;
        private Task? _playbackTask;
        private CancellationTokenSource? _exportCancellation;
        private bool _restoringHistory;
        private bool _dragStarted;
        private bool _allowClose;
        private readonly string _userSamplesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JollyDrums", "Samples");

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            Directory.CreateDirectory(_userSamplesFolder);
            InstrumentLibrary.ItemsSource = _library;
            ArrangementListBox.ItemsSource = Arrangement;
            Directory.CreateDirectory(_settingsFolder);
            BuildStepHeader();
            LoadBuiltInSamples();
            AddInitialTrack("Kick");
            AddInitialTrack("Snare");
            AddInitialTrack("Closed Hat");
            AddInitialTrack("Clap");
            SetDefaultPattern();
            Patterns[0] = _viewModel.CapturePattern();
            _viewModel.IsDirty = false;
            Title = "JollyDrumMachine - New Project";
            UpdatePatternSlotButtons();
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();
            MasterVolumeSlider_ValueChanged(this, null!);
            _viewModel.ResetHistory(CaptureProjectSnapshot());
            _viewModel.ProjectChanged += (_, _) => MarkDirty();
        }

        private void LoadBuiltInSamples()
        {
            string folder = Path.Combine(AppContext.BaseDirectory, "Assets", "Samples");
            if (!Directory.Exists(folder))
                return;

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
            if (Tracks.Count >= 24)
            {
                _viewModel.Status = "Track limit reached (24).";
                return;
            }

            DrumInstrument? track = _viewModel.AddTrack(sample.Name, sample.Path, sample.Key);
            if (track == null)
                return;

            BuildTrackRow(track);
            DropHint.Visibility = Visibility.Collapsed;
            if (report)
                _viewModel.Status = $"Added {sample.Name}.";
        }

        private void BuildStepHeader()
        {
            StepHeader.Children.Clear();
            StepHeader.ColumnDefinitions.Clear();
            StepHeader.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(280)
            });
            for (int i = 0; i < StepsPerPattern; i++)
                StepHeader.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(34)
                });

            TextBlock label = new()
            {
                Text = "TRACK / LEVEL / MIX",
                Foreground = Brush("#8DA2B4"),
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(label, 0);
            StepHeader.Children.Add(label);
            for (int i = 0; i < StepsPerPattern; i++)
            {
                TextBlock step = new()
                {
                    Text = (i + 1).ToString("00"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = i % 4 == 0 ? Brush("#55D6BE") : Brush("#8DA2B4"),
                    FontSize = 10
                };
                Grid.SetColumn(step, i + 1);
                StepHeader.Children.Add(step);
            }
        }

        private void BuildTrackRow(DrumInstrument track)
        {
            Grid row = new()
            {
                Height = 48,
                Margin = new Thickness(0, 0, 0, 5),
                Tag = track,
                AllowDrop = true,
                Background = Brush("#172430")
            };
            row.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(280)
            });
            for (int i = 0; i < StepsPerPattern; i++)
                row.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(34)
                });

            Border left = new()
            {
                BorderBrush = Brush("#2A4051"),
                BorderThickness = new Thickness(0, 0, 1, 0),
                Padding = new Thickness(7, 2, 5, 2)
            };
            StackPanel details = new()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Button preview = new()
            {
                Content = "▶",
                Padding = new Thickness(5, 2, 5, 2),
                Margin = new Thickness(0, 0, 6, 0),
                ToolTip = "Preview sample"
            };
            preview.Click += (_, _) => PlayPreview(track);
            StackPanel nameAndVolume = new()
            {
                Width = 138
            };
            TextBlock name = new()
            {
                Text = track.Name,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Slider volume = new()
            {
                Minimum = 0,
                Maximum = 1,
                Value = track.Volume,
                Height = 16,
                Margin = new Thickness(0, 1, 0, 0),
                ToolTip = "Track volume"
            };
            volume.ValueChanged += (_, e) =>
            {
                track.Volume = (float)e.NewValue;
                MarkDirty();
            };
            nameAndVolume.Children.Add(name);
            nameAndVolume.Children.Add(volume);
            Button mute = new()
            {
                Content = "M",
                Padding = new Thickness(5, 2, 5, 2),
                Margin = new Thickness(4, 0, 0, 0),
                ToolTip = "Mute track",
                Background = track.Muted ? Brush("#8E3945") : Brush("#223545")
            };
            mute.Click += (_, _) => { track.Muted = !track.Muted; mute.Background = track.Muted ? Brush("#8E3945") : Brush("#223545"); MarkDirty(); };
            Button solo = new()
            {
                Content = "S",
                Padding = new Thickness(5, 2, 5, 2),
                Margin = new Thickness(2, 0, 0, 0),
                ToolTip = "Solo track",
                Background = track.Solo ? Brush("#B17A32") : Brush("#223545")
            };
            solo.Click += (_, _) =>
            {
                track.Solo = !track.Solo;
                solo.Background = track.Solo ? Brush("#B17A32") : Brush("#223545");
                MarkDirty();
            };
            Button settings = new()
            {
                Content = "FX",
                Padding = new Thickness(4, 2, 4, 2),
                Margin = new Thickness(2, 0, 0, 0),
                ToolTip = "Pan, tune, attack and decay"
            };
            settings.Click += (_, _) =>
            {
                InstrumentSettingsWindow window = new(track) { Owner = this };
                window.Closed += (_, _) => MarkDirty();
                window.ShowDialog();
            };
            Button remove = new()
            {
                Content = "×",
                Padding = new Thickness(5, 2, 5, 2),
                Margin = new Thickness(2, 0, 0, 0),
                ToolTip = "Remove track"
            };
            remove.Click += (_, _) => RemoveTrack(track, row);
            details.Children.Add(preview);
            details.Children.Add(nameAndVolume);
            details.Children.Add(mute);
            details.Children.Add(solo);
            details.Children.Add(settings);
            details.Children.Add(remove);
            left.Child = details;
            Grid.SetColumn(left, 0);
            row.Children.Add(left);
            for (int i = 0; i < StepsPerPattern; i++)
            {
                int stepIndex = i;
                Button pad = new()
                {
                    Margin = new Thickness(2),
                    Padding = new Thickness(0),
                    FontSize = 9,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brush(i % 4 == 0 ? "#3C596D" : "#263B4B"),
                    Background = track.Steps[i] ? Brush("#55D6BE") : Brush(i % 4 == 0 ? "#243746" : "#1C2B38"),
                    Content = track.Steps[i] ? track.StepVelocities[i].ToString() : "",
                    Tag = stepIndex,
                    ToolTip = StepToolTip(track, stepIndex)
                };
                pad.Click += (_, _) => { track.Steps[stepIndex] = !track.Steps[stepIndex]; pad.Content = track.Steps[stepIndex] ? track.StepVelocities[stepIndex].ToString() : ""; pad.ToolTip = StepToolTip(track, stepIndex); UpdatePad(pad, track.Steps[stepIndex], stepIndex); MarkDirty(); };
                pad.PreviewMouseWheel += (_, e) => { if (track.Steps[stepIndex]) { int change = e.Delta > 0 ? 8 : -8; track.StepVelocities[stepIndex] = Math.Clamp(track.StepVelocities[stepIndex] + change, 1, 127); pad.Content = track.StepVelocities[stepIndex].ToString(); pad.ToolTip = StepToolTip(track, stepIndex); UpdatePad(pad, true, stepIndex); MarkDirty(); } e.Handled = true; };
                Grid.SetColumn(pad, i + 1);
                row.Children.Add(pad);
            }

            row.DragOver += TrackRow_DragOver;
            row.Drop += TrackRow_Drop;
            TrackPanel.Children.Add(row);
        }

        private void RemoveTrack(DrumInstrument track, Grid row)
        {
            _viewModel.RemoveTrack(track);
            _playback.RemoveTrack(track);

            TrackPanel.Children.Remove(row);
            DropHint.Visibility = Tracks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static string StepToolTip(DrumInstrument track, int stepIndex) => $"{track.Name}, step {stepIndex + 1} · Velocity {track.StepVelocities[stepIndex]}/127. Use the mouse wheel to adjust velocity.";

        private static void UpdatePad(Button pad, bool active, int stepIndex)
        {
            pad.Background = active ? Brush("#55D6BE") : Brush(stepIndex % 4 == 0 ? "#243746" : "#1C2B38");
            pad.BorderBrush = Brush(stepIndex % 4 == 0 ? "#3C596D" : "#263B4B");
        }

        private void SetDefaultPattern()
        {
            _viewModel.SetDefaultPattern();
            RebuildTrackPanel();
        }

        private void RebuildTrackPanel()
        {
            TrackPanel.Children.Clear();
            foreach (DrumInstrument track in Tracks)
                BuildTrackRow(track);

            DropHint.Visibility = Tracks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PlayPreview(DrumInstrument track)
        {
            try
            {
                _playback.PreviewTrack(track);
                _viewModel.Status = $"Previewing {track.Name}";
            }
            catch (Exception ex)
            {
                _viewModel.Status = $"Preview failed: {ex.Message}";
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsPlaying || _playbackTask is { IsCompleted: false })
                return;

            _playbackCancellation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
            CancellationToken token = _playbackCancellation.Token;
            _viewModel.CurrentStep = -1;
            _viewModel.IsPlaying = true;
            _playbackTask = Task.Run(() => PlaybackLoopAsync(token), token);
        }

        private Task PlaybackLoopAsync(CancellationToken token)
        {
            return DrumSequencerService.RunAsync(
                () => _viewModel.Tempo,
                () => Dispatcher.Invoke(() => Tracks.ToArray()),
                (track, velocity) => Dispatcher.Invoke(() =>
                {
                    _playback.PlayTrack(track, velocity, Tracks);
                }),
                step => Dispatcher.InvokeAsync(() => _viewModel.CurrentStep = step),
                () => StepsPerPattern,
                () => _viewModel.Swing / 100.0,
                () => _viewModel.Humanise,
                token);
        }

        private void StopPlayback()
        {
            _playbackCancellation?.Cancel();
            _playback.StopAll();
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
                    DrumInstrument? track = row.Tag as DrumInstrument;
                    bool active = track != null && track.Steps[step];
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

                    if (active)
                    {
                        pad.Background = Brush(step == _viewModel.CurrentStep ? "#B5FFF0" : "#55D6BE");
                        pad.Content = track != null ? track.StepVelocities[step].ToString() : "";
                    }
                    else
                        pad.Background = Brush(step == _viewModel.CurrentStep ? "#526B7B" : step % 4 == 0 ? "#243746" : "#1C2B38");
                }
            }
        }

        private void TempoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_viewModel is null || TempoSlider is null || TempoText is null)
                return;

            _viewModel.Tempo = TempoSlider.Value;
            MarkDirty();
            TempoText.Text = $"{TempoSlider.Value:0} BPM";
        }

        private void MasterVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _viewModel.MasterVolume = MasterVolumeSlider?.Value ?? 0.85;
            _playback.MasterVolume = (float)_viewModel.MasterVolume;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e) => StopPlayback();

        private void ClearPattern_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearPatternCommand.Execute(null);
            RebuildTrackPanel();
            _viewModel.Status = "Pattern cleared.";
        }

        private void InstrumentLibrary_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _dragStarted = false;
                return;
            }

            if (InstrumentLibrary.SelectedItem is not SampleItem sample)
                return;

            if (_dragStarted)
                return;

            _dragStarted = true;
            DragDrop.DoDragDrop(InstrumentLibrary, sample, DragDropEffects.Copy);
            _dragStarted = false;
        }

        private void TrackPanel_DragOver(object sender, DragEventArgs e) => e.Effects = e.Data.GetDataPresent(typeof(SampleItem)) ? DragDropEffects.Copy : DragDropEffects.None;

        private void TrackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(SampleItem)) is SampleItem sample)
                AddTrack(sample);
        }

        private void TrackRow_DragOver(object sender, DragEventArgs e) => e.Effects = e.Data.GetDataPresent(typeof(SampleItem)) ? DragDropEffects.Copy : DragDropEffects.None;

        private void TrackRow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(SampleItem)) is not SampleItem sample ||
                sender is not Grid row ||
                row.Tag is not DrumInstrument oldTrack)
                return;

            e.Handled = true;

            _playback.RefreshTrack(oldTrack);

            _viewModel.ReplaceTrackSample(oldTrack, sample.Name, sample.Path, sample.Key);

            RebuildTrackPanel();

            _viewModel.Status = $"Changed {oldTrack.Name} sample.";
        }

        private void InstrumentLibrary_Drop(object sender, DragEventArgs e)
        { 
        }

        private void AddSample_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "Add drum sample",
                Filter = "Audio files|*.wav;*.mp3;*.aiff;*.aif;*.wma;*.mp4|All files|*.*",
                Multiselect = true
            };
            if (dialog.ShowDialog(this) != true)
                return;

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
                        while (File.Exists(destination))
                            destination = Path.Combine(_userSamplesFolder, $"{stem}_{suffix++}{extension}");

                        File.Copy(source, destination, false);
                    }

                    RegisterSample(destination);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Could not add sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            _viewModel.Status = "Samples added to the instrument library.";
        }

        private void StepCountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingStepCount || StepCountComboBox?.SelectedItem is not ComboBoxItem item || !int.TryParse(item.Content?.ToString(), out int count))
                return;

            if (count == StepsPerPattern)
                return;

            Patterns[ActivePatternIndex] = _viewModel.CapturePattern();
            StopPlayback();
            SetStepCount(count, true);
            Patterns[ActivePatternIndex] = _viewModel.CapturePattern();
            MarkDirty();
            _viewModel.Status = $"Pattern length set to {count} steps.";
        }

        private void SetStepCount(int count, bool preserveCurrent)
        {
            count = count is 16 or 32 or 64 ? count : 16;
            if (preserveCurrent)
            {
                foreach (DrumInstrument track in Tracks)
                {
                    track.Steps = PatternDataService.ResizeSteps(track.Steps, count);
                    track.StepVelocities = PatternDataService.ResizeVelocities(track.StepVelocities, count);
                }
            }

            StepsPerPattern = count;
            if (StepCountComboBox != null)
            {
                _changingStepCount = true;
                StepCountComboBox.SelectedIndex = count == 16 ? 0 : count == 32 ? 1 : 2;
                _changingStepCount = false;
            }

            BuildStepHeader();
            if (preserveCurrent)
            {
                RebuildTrackPanel();
                UpdatePlayhead();
            }
        }

        private void PatternSlot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || !int.TryParse(button.Tag?.ToString(), out int index))
                return;

            if (index == ActivePatternIndex)
                return;

            StopPlayback();
            PatternFile pattern = _viewModel.SetActivePattern(index);
            ApplyPattern(pattern);
            Patterns[index] = _viewModel.CapturePattern();
            UpdatePatternSlotButtons();
            MarkDirty();
            _viewModel.Status = $"Switched to pattern {(char)('A' + index)}.";
        }

        private void UpdatePatternSlotButtons()
        {
            Button[] buttons = [PatternButtonA, PatternButtonB, PatternButtonC, PatternButtonD];
            for (int index = 0; index < buttons.Length; index++)
            {
                Button button = buttons[index];
                button.Background = index == ActivePatternIndex ? Brush("#176B61") : Brush("#243746");
                button.BorderBrush = index == ActivePatternIndex ? Brush("#55D6BE") : Brush("#355064");
                button.FontWeight = index == ActivePatternIndex ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        private void ApplyPattern(PatternFile pattern)
        {
            SetStepCount(pattern.StepsPerBar is 16 or 32 or 64 ? pattern.StepsPerBar : 16, false);
            _playback.StopAll();
            _playback.ClearTracks();
            _viewModel.ApplyPatternData(pattern, item =>
            {
                string path = item.SamplePath;
                if (!File.Exists(path))
                {
                    string local = Path.Combine(_userSamplesFolder, Path.GetFileName(path));
                    string builtIn = Path.Combine(AppContext.BaseDirectory, "Assets", "Samples", Path.GetFileName(path));
                    path = File.Exists(local) ? local : builtIn;
                }

                if (!File.Exists(path))
                    return null;

                SampleItem? sample = _library.FirstOrDefault(entry => string.Equals(entry.Path, path, StringComparison.OrdinalIgnoreCase));
                if (sample == null)
                {
                    RegisterSample(path, item.Name);
                    sample = _library.Last();
                }

                return (sample.Path, sample.Key);
            });

            TempoSlider.Value = _viewModel.Tempo;
            RebuildTrackPanel();
        }

        private void SavePattern_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Filter = "JollyDrums project (*.jdp.json)|*.jdp.json|JSON file (*.json)|*.json",
                FileName = _viewModel.CurrentProjectPath != null ? Path.GetFileName(_viewModel.CurrentProjectPath) : "MyDrumProject.jdp.json",
                InitialDirectory = _viewModel.CurrentProjectPath != null ? Path.GetDirectoryName(_viewModel.CurrentProjectPath) : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (dialog.ShowDialog(this) != true)
                return;

            Patterns[ActivePatternIndex] = _viewModel.CapturePattern();
            PatternProject project = _viewModel.CreateProjectSnapshot();

            try
            {
                ProjectFileService.SaveProject(dialog.FileName, project);
                _viewModel.CurrentProjectPath = dialog.FileName;
                _viewModel.IsDirty = false;
                _viewModel.ResetHistory(CaptureProjectSnapshot());
                ProjectFileService.SaveRecentProjectPath(_settingsFolder, dialog.FileName);
                Title = $"JollyDrumMachine - {Path.GetFileNameWithoutExtension(dialog.FileName)}";
                _viewModel.Status = $"Saved project to {Path.GetFileName(dialog.FileName)}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPattern_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "JollyDrums project or pattern (*.jdp.json;*.json)|*.jdp.json;*.json|All files|*.*"
            };
            if (dialog.ShowDialog(this) != true)
                return;

            try
            {
                ProjectDocument document = ProjectFileService.LoadDocument(dialog.FileName);
                StopPlayback();

                if (document.Project is PatternProject project)
                {

                    PatternFile activePattern = _viewModel.LoadProjectData(project);
                    ApplyPattern(activePattern);
                    UpdatePatternSlotButtons();
                    _viewModel.CurrentProjectPath = dialog.FileName;
                    _viewModel.IsDirty = false;
                    ProjectFileService.SaveRecentProjectPath(_settingsFolder, dialog.FileName);
                    Title = $"JollyDrumMachine - {Path.GetFileNameWithoutExtension(dialog.FileName)}";
                    _viewModel.Status = $"Loaded project from {Path.GetFileName(dialog.FileName)}.";
                }
                else
                {
                    // Backward compatibility with files containing a single PatternFile.
                    PatternFile pattern = document.Pattern ?? throw new InvalidDataException("The selected pattern is empty or invalid.");

                    ApplyPattern(pattern);
                    Patterns[ActivePatternIndex] = _viewModel.CapturePattern();
                    UpdatePatternSlotButtons();
                    _viewModel.IsDirty = false;
                    _viewModel.Status = $"Loaded single pattern into slot {(char)('A' + ActivePatternIndex)}.";
                }

                _viewModel.ResetHistory(CaptureProjectSnapshot());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Load failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkDirty()
        {
            _viewModel.RecordProjectEdit(CaptureProjectSnapshot(), _restoringHistory);
            _viewModel.IsDirty = true;
            string projectName = _viewModel.CurrentProjectPath == null ? "New Project" : Path.GetFileNameWithoutExtension(_viewModel.CurrentProjectPath);
            Title = $"JollyDrumMachine - {projectName}*";
        }

        private string CaptureProjectSnapshot()
        {
            if (Patterns.Length > ActivePatternIndex && ActivePatternIndex >= 0)
                Patterns[ActivePatternIndex] = _viewModel.CapturePattern();

            return System.Text.Json.JsonSerializer.Serialize(_viewModel.CreateProjectSnapshot());
        }

        private void RestoreHistorySnapshot(string snapshot)
        {
            PatternProject project = System.Text.Json.JsonSerializer.Deserialize<PatternProject>(snapshot) ?? throw new InvalidDataException("Undo history snapshot is invalid.");
            _restoringHistory = true;
            try
            {
                StopPlayback();
                PatternFile activePattern = _viewModel.LoadProjectData(project);
                ApplyPattern(PatternDataService.ClonePattern(activePattern));
                UpdatePatternSlotButtons();
            }
            finally
            {
                _restoringHistory = false;
            }

            _viewModel.SetHistoryBaseline(CaptureProjectSnapshot());
            _viewModel.IsDirty = true;
            string projectName = _viewModel.CurrentProjectPath == null ? "New Project" : Path.GetFileNameWithoutExtension(_viewModel.CurrentProjectPath);
            Title = $"JollyDrumMachine - {projectName}*";
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.TryUndo(CaptureProjectSnapshot(), out string snapshot))
                return;

            RestoreHistorySnapshot(snapshot);
            _viewModel.Status = "Undo completed.";
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.TryRedo(CaptureProjectSnapshot(), out string snapshot))
                return;

            RestoreHistorySnapshot(snapshot);
            _viewModel.Status = "Redo completed.";
        }

        private string PresetFolder => Path.Combine(_settingsFolder, "Presets");

        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(PresetFolder);
            SaveFileDialog dialog = new()
            {
                Title = "Save current pattern preset",
                Filter = "JollyDrums pattern preset (*.jdpreset.json)|*.jdpreset.json|JSON file (*.json)|*.json",
                InitialDirectory = PresetFolder,
                FileName = $"Pattern-{(char)('A' + ActivePatternIndex)}.jdpreset.json"
            };
            if (dialog.ShowDialog(this) != true)
                return;

            try
            {
                ProjectFileService.SavePattern(dialog.FileName, _viewModel.CapturePattern());
                _viewModel.Status = $"Saved preset {Path.GetFileName(dialog.FileName)}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Preset save failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(PresetFolder);
            OpenFileDialog dialog = new()
            {
                Title = "Load pattern preset",
                Filter = "Pattern presets and JSON (*.jdpreset.json;*.json)|*.jdpreset.json;*.json|All files (*.*)|*.*",
                InitialDirectory = PresetFolder
            };
            if (dialog.ShowDialog(this) != true)
                return;

            try
            {
                PatternFile pattern = ProjectFileService.LoadPattern(dialog.FileName);
                ApplyPattern(pattern);
                Patterns[ActivePatternIndex] = _viewModel.CapturePattern();
                MarkDirty();
                _viewModel.Status = $"Loaded preset {Path.GetFileName(dialog.FileName)}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Preset load failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DataContext == null || SwingSlider == null)
                return;
            _viewModel.Swing = SwingSlider.Value;
            if (!_restoringHistory)
                MarkDirty();
        }

        private void HumaniseCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (DataContext == null || HumaniseCheckBox == null)
                return;
            _viewModel.Humanise = HumaniseCheckBox.IsChecked == true;
            if (!_restoringHistory)
                MarkDirty();
        }

        private void CopyPattern_Click(object sender, RoutedEventArgs e)
        {
            if (CopyPatternComboBox.SelectedIndex < 0)
                return;
            int destination = CopyPatternComboBox.SelectedIndex;
            if (destination == ActivePatternIndex)
            {
                _viewModel.Status = "Choose a different destination pattern.";
                return;
            }
            Patterns[ActivePatternIndex] = _viewModel.CapturePattern();
            Patterns[destination] = PatternDataService.ClonePattern(Patterns[ActivePatternIndex]!);
            MarkDirty();
            _viewModel.Status = $"Copied pattern {(char)('A' + ActivePatternIndex)} to {(char)('A' + destination)}.";
        }

        private void AddArrangementEntry_Click(object sender, RoutedEventArgs e)
        {
            int pattern = Math.Clamp(ArrangementPatternComboBox.SelectedIndex, 0, 3);
            if (!int.TryParse(ArrangementRepeatsTextBox.Text, out int repeats))
                repeats = 1;
            _viewModel.AddArrangementEntry(pattern, repeats);
            ArrangementListBox.SelectedItem = _viewModel.SelectedArrangementEntry;
        }

        private void PlayArrangement_Click(object sender, RoutedEventArgs e)
        {
            if (Arrangement.Count == 0)
            {
                _viewModel.Status = "Add patterns to the arrangement first.";
                return;
            }
            Patterns[ActivePatternIndex] = _viewModel.CapturePattern();
            StopPlayback();
            _playbackTask = PlayArrangementAsync();
        }


        private Task PlayArrangementAsync()
        {
            _playbackCancellation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
            CancellationToken token = _playbackCancellation.Token;

            return _viewModel.PlayArrangementAsync(
                async (pattern, patternIndex) =>
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ActivePatternIndex = patternIndex;
                        ApplyPattern(pattern);
                        UpdatePatternSlotButtons();
                    });
                },
                (repeats, cancellationToken) => DrumSequencerService.RunAsync(
                    () => _viewModel.Tempo,
                    () => Dispatcher.Invoke(() => Tracks.ToArray()),
                    (track, velocity) => Dispatcher.Invoke(() => _playback.PlayTrack(track, velocity, Tracks)),
                    step => Dispatcher.InvokeAsync(() => _viewModel.CurrentStep = step),
                    () => StepsPerPattern,
                    () => _viewModel.Swing / 100.0,
                    () => _viewModel.Humanise,
                    cancellationToken,
                    repeats),
                token);
        }

        private void CancelExport_Click(object sender, RoutedEventArgs e)
        {
            _exportCancellation?.Cancel();
            _viewModel.Status = "Cancelling export…";
        }

        private async void ExportWav_Click(object sender, RoutedEventArgs e) => await ExportArrangementAsync(false);
        private async void ExportMp3_Click(object sender, RoutedEventArgs e) => await ExportArrangementAsync(true);

        private async Task ExportArrangementAsync(bool mp3)
        {
            if (Arrangement.Count == 0)
            {
                MessageBox.Show(this, "Add at least one pattern to the arrangement first.", "Nothing to export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dialog = new()
            {
                Title = mp3 ? "Export arrangement to MP3" : "Export arrangement to WAV",
                Filter = mp3 ? "MP3 audio (*.mp3)|*.mp3" : "WAV audio (*.wav)|*.wav",
                FileName = $"JollyDrumMachine.{(mp3 ? "mp3" : "wav") }"
            };
            if (dialog.ShowDialog(this) != true)
                return;

            PatternFile?[] snapshots = [.. Patterns.Select(pattern => pattern == null ? null : PatternDataService.ClonePattern(pattern))];
            snapshots[ActivePatternIndex] = _viewModel.CapturePattern();
            List<PatternFile> renderPatterns = [.. snapshots.Select(pattern => PatternDataService.ClonePattern(pattern ?? new PatternFile()))];
            List<ArrangementEntry> entries = [.. Arrangement.Select(entry => new ArrangementEntry { PatternIndex = entry.PatternIndex, Repeats = entry.Repeats })];
            double swing = _viewModel.Swing / 100.0;
            bool humanise = _viewModel.Humanise;
            float masterVolume = (float)_viewModel.MasterVolume;
            _exportCancellation?.Dispose();
            _exportCancellation = new CancellationTokenSource();
            CancellationToken token = _exportCancellation.Token;
            _viewModel.Status = "Rendering arrangement offline…";
            IProgress<double> progress = new Progress<double>(value => _viewModel.Status = $"Rendering arrangement… {value:P0}");

            try
            {
                await _exportService.ExportAsync(
                    renderPatterns,
                    entries,
                    dialog.FileName,
                    mp3,
                    masterVolume,
                    swing,
                    humanise,
                    token,
                    progress,
                    message => _viewModel.Status = message);

                _viewModel.Status = $"Exported arrangement to {Path.GetFileName(dialog.FileName)}.";
            }
            catch (OperationCanceledException)
            {
                _viewModel.Status = "Export cancelled.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Error);
                _viewModel.Status = "Arrangement export failed.";
            }
            finally
            {
                _exportCancellation?.Dispose();
                _exportCancellation = null;
            }
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Start a new project? Unsaved changes will be lost.", "New project", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            StopPlayback();
            _viewModel.ResetProjectState();
            SetStepCount(16, false);
            TempoSlider.Value = 120;
            RebuildTrackPanel();
            Patterns[0] = _viewModel.CapturePattern();
            _viewModel.CurrentProjectPath = null;
            _viewModel.IsDirty = false;
            _viewModel.ResetHistory(CaptureProjectSnapshot());
            Title = "JollyDrumMachine - New Project";
            UpdatePatternSlotButtons();
            _viewModel.Status = "New project created.";
        }

        private void OpenRecentProject_Click(object sender, RoutedEventArgs e)
        {
            string? path = ProjectFileService.LoadRecentProjectPath(_settingsFolder);
            if (string.IsNullOrWhiteSpace(path))
            {
                _viewModel.Status = "No recent project is recorded yet.";
                return;
            }
            if (!File.Exists(path))
            {
                _viewModel.Status = "The recent project file could not be found.";
                return;
            }
            LoadProjectFromPath(path);
        }

        private void LoadProjectFromPath(string path)
        {
            try
            {
                PatternProject project = ProjectFileService.LoadProject(path);
                StopPlayback();
                PatternFile activePattern = _viewModel.LoadProjectData(project);
                ApplyPattern(activePattern);
                UpdatePatternSlotButtons();
                _viewModel.CurrentProjectPath = path;
                _viewModel.IsDirty = false;
                _viewModel.ResetHistory(CaptureProjectSnapshot());
                ProjectFileService.SaveRecentProjectPath(_settingsFolder, path);
                Title = $"JollyDrumMachine - {Path.GetFileNameWithoutExtension(path)}";
                _viewModel.Status = $"Loaded project {Path.GetFileName(path)}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Load failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool _shutdownStarted;

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_allowClose && !_shutdownStarted && _viewModel.IsDirty)
            {
                MessageBoxResult result = MessageBox.Show(this, "This project has unsaved changes. Save before closing?", "Unsaved changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    SavePattern_Click(this, new RoutedEventArgs());
                    if (_viewModel.IsDirty)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            if (_allowClose)
            {
                return;
            }

            e.Cancel = true;

            if (_shutdownStarted)
            {
                return;
            }

            _shutdownStarted = true;

            _uiTimer.Stop();
            _uiTimer.Tick -= UiTimer_Tick;

            _lifetime.Cancel();
            _playbackCancellation?.Cancel();

            _ = CompleteShutdownAsync();
        }

        private async Task CompleteShutdownAsync()
        {
            if (_playbackTask != null)
            {
                try
                {
                    await _playbackTask;
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _viewModel.Status = $"Playback stopped with an error: {ex.Message}";
                }
            }


            _playback.Dispose();

            _playbackCancellation?.Dispose();
            _lifetime.Dispose();

            await Dispatcher.InvokeAsync(() =>
            {
                _allowClose = true;
                Close();
            });
        }

        private static bool IsSupportedAudio(string path) => new[]
        {
            ".wav",
            ".mp3",
            ".aiff",
            ".aif",
            ".wma",
            ".mp4"
        }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

        private static SolidColorBrush Brush(string colour) => new((Color)ColorConverter.ConvertFromString(colour));

        private sealed record SampleItem(string Name, string Path, string Key)
        {
            public override string ToString() => Name;
        }
    }
}