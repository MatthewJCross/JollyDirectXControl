using JollyDrumMachine.Models;
using JollyDrumsMachine.Utils;
using JollyDrumMachine.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JollyDrumMachine.ViewModels
{
    /// <summary>
    /// Owns editable application state and user actions. MainWindow is responsible for
    /// WPF-only concerns such as file pickers, dynamic pad rendering and window lifetime.
    /// </summary>
    public sealed class MainViewModel : ViewModelBase
    {
        private double _tempo = 120.0;
        private double _masterVolume = 0.85;
        private double _swing;
        private bool _humanise;
        private int _currentStep = -1;
        private int _activePatternIndex;
        private int _stepsPerPattern = 16;
        private bool _isPlaying;
        private string _status = "Ready · Drag an instrument into the sequencer or add your own WAV/MP3 sample.";
        private string? _currentProjectPath;
        private bool _isDirty;
        private ArrangementEntry? _selectedArrangementEntry;
        private ArrangementEntry? _arrangementClipboard;
        private readonly Stack<string> _undoHistory = new();
        private readonly Stack<string> _redoHistory = new();
        private string? _lastHistorySnapshot;
        private const int MaximumHistoryEntries = 100;

        public MainViewModel()
        {
            ClearPatternCommand = new RelayCommand(ClearPattern, () => !IsPlaying);
            CopyArrangementEntryCommand = new RelayCommand(CopyArrangementEntry, () => SelectedArrangementEntry != null);
            PasteArrangementEntryCommand = new RelayCommand(PasteArrangementEntry, () => _arrangementClipboard != null);
            DuplicateArrangementEntryCommand = new RelayCommand(DuplicateArrangementEntry, () => SelectedArrangementEntry != null);
            RemoveArrangementEntryCommand = new RelayCommand(RemoveArrangementEntry, () => SelectedArrangementEntry != null);
            MoveArrangementUpCommand = new RelayCommand(() => MoveArrangementEntry(-1), () => CanMoveArrangementEntry(-1));
            MoveArrangementDownCommand = new RelayCommand(() => MoveArrangementEntry(1), () => CanMoveArrangementEntry(1));
        }

        public event EventHandler? ProjectChanged;

        public ObservableCollection<DrumInstrument> Tracks { get; } = [];
        public ObservableCollection<ArrangementEntry> Arrangement { get; } = [];
        public PatternFile?[] Patterns { get; } = new PatternFile?[4];

        public ICommand ClearPatternCommand { get; }
        public ICommand CopyArrangementEntryCommand { get; }
        public ICommand PasteArrangementEntryCommand { get; }
        public ICommand DuplicateArrangementEntryCommand { get; }
        public ICommand RemoveArrangementEntryCommand { get; }
        public ICommand MoveArrangementUpCommand { get; }
        public ICommand MoveArrangementDownCommand { get; }

        public double Tempo
        {
            get => _tempo;
            set => SetProperty(ref _tempo, Math.Clamp(value, 40.0, 240.0));
        }

        public double MasterVolume
        {
            get => _masterVolume;
            set => SetProperty(ref _masterVolume, Math.Clamp(value, 0.0, 1.0));
        }

        /// <summary>Swing amount in percent, from straight timing (0) to 45%.</summary>
        public double Swing
        {
            get => _swing;
            set => SetProperty(ref _swing, Math.Clamp(value, 0.0, 45.0));
        }

        public bool Humanise
        {
            get => _humanise;
            set => SetProperty(ref _humanise, value);
        }

        public int ActivePatternIndex
        {
            get => _activePatternIndex;
            set => SetProperty(ref _activePatternIndex, Math.Clamp(value, 0, 3));
        }

        public int StepsPerPattern
        {
            get => _stepsPerPattern;
            set => SetProperty(ref _stepsPerPattern, value is 16 or 32 or 64 ? value : 16);
        }

        public int CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (!SetProperty(ref _isPlaying, value))
                    return;

                OnPropertyChanged(nameof(PlaybackStatus));
                OnPropertyChanged(nameof(PlayButtonText));
                RaiseHistoryState();
                RaiseCommandStates();
            }
        }

        public ArrangementEntry? SelectedArrangementEntry
        {
            get => _selectedArrangementEntry;
            set
            {
                SetProperty(ref _selectedArrangementEntry, value);
                RaiseCommandStates();
            }
        }

        public string? CurrentProjectPath
        {
            get => _currentProjectPath;
            set => SetProperty(ref _currentProjectPath, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public string PlaybackStatus => IsPlaying ? "PLAYING" : "STOPPED";
        public string PlayButtonText => IsPlaying ? "▶  PLAYING" : "▶  PLAY";

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public void SetDefaultPattern()
        {
            foreach (DrumInstrument track in Tracks)
            {
                Array.Fill(track.Steps, false);
                Array.Fill(track.StepVelocities, 100);

                if (track.Name.Contains("Kick", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (int step in new[] { 0, 4, 8, 12 })
                        if (step < track.Steps.Length)
                            track.Steps[step] = true;
                }
                else if (track.Name.Contains("Snare", StringComparison.OrdinalIgnoreCase) || track.Name.Contains("Clap", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (int step in new[] { 4, 12 })
                        if (step < track.Steps.Length)
                            track.Steps[step] = true;
                }
                else if (track.Name.Contains("Hat", StringComparison.OrdinalIgnoreCase))
                {
                    for (int step = 0; step < track.Steps.Length; step += 2)
                        track.Steps[step] = true;
                }
            }
        }

        public void ResetProjectState()
        {
            Arrangement.Clear();
            Array.Clear(Patterns);
            ActivePatternIndex = 0;
            StepsPerPattern = 16;
            Tempo = 120;
            Swing = 0;
            Humanise = false;
            SelectedArrangementEntry = null;
            foreach (DrumInstrument track in Tracks)
            {
                track.Steps = new bool[StepsPerPattern];
                track.StepVelocities = Enumerable.Repeat(100, StepsPerPattern).ToArray();
            }
            SetDefaultPattern();
        }

        public DrumInstrument? AddTrack(string name, string samplePath, string sampleKey)
        {
            if (Tracks.Count >= 24)
            {
                Status = "Track limit reached (24).";
                return null;
            }

            DrumInstrument track = new(name, samplePath, sampleKey)
            {
                Steps = new bool[StepsPerPattern],
                StepVelocities = Enumerable.Repeat(100, StepsPerPattern).ToArray()
            };
            Tracks.Add(track);
            ProjectChanged?.Invoke(this, EventArgs.Empty);
            return track;
        }

        public void RemoveTrack(DrumInstrument track)
        {
            if (Tracks.Remove(track))
                ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ReplaceTrackSample(DrumInstrument track, string name, string samplePath, string sampleKey)
        {
            track.Name = name;
            track.SamplePath = samplePath;
            track.SampleKey = sampleKey;
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyPatternData(PatternFile pattern, Func<PatternInstrument, (string Path, string Key)?> resolveSample)
        {
            StepsPerPattern = pattern.StepsPerBar is 16 or 32 or 64 ? pattern.StepsPerBar : 16;
            Tracks.Clear();

            foreach (PatternInstrument item in pattern.Instruments ?? [])
            {
                (string Path, string Key)? resolved = resolveSample(item);
                if (resolved == null)
                    continue;

                DrumInstrument track = new(item.Name, resolved.Value.Path, resolved.Value.Key)
                {
                    Pan = item.Pan,
                    Tune = item.Tune,
                    Attack = item.Attack,
                    Decay = item.Decay,
                    Solo = item.Solo,
                    Volume = item.Volume,
                    Muted = item.Muted,
                    Steps = PatternDataService.ResizeSteps(item.Steps, StepsPerPattern),
                    StepVelocities = PatternDataService.ResizeVelocities(item.StepVelocities, StepsPerPattern)
                };
                Tracks.Add(track);
            }

            Tempo = Math.Clamp(pattern.Tempo, 40, 240);
        }

        public PatternFile CapturePattern()
        {
            return PatternDataService.CapturePattern(Tempo, StepsPerPattern, Tracks);
        }

        public PatternProject CreateProjectSnapshot()
        {
            Patterns[ActivePatternIndex] = CapturePattern();
            return new PatternProject
            {
                Version = 3,
                ActivePatternIndex = ActivePatternIndex,
                Patterns = Patterns.Select(pattern => PatternDataService.ClonePattern(pattern ?? new PatternFile())).ToList(),
                Arrangement = Arrangement.Select(entry => new ArrangementEntry { PatternIndex = entry.PatternIndex, Repeats = entry.Repeats }).ToList(),
                Swing = Swing,
                Humanise = Humanise
            };
        }

        public PatternFile LoadProjectData(PatternProject project)
        {
            for (int index = 0; index < Patterns.Length; index++)
                Patterns[index] = PatternDataService.ClonePattern(index < project.Patterns.Count ? project.Patterns[index] : new PatternFile());

            ActivePatternIndex = Math.Clamp(project.ActivePatternIndex, 0, Patterns.Length - 1);
            SelectedArrangementEntry = null;
            Arrangement.Clear();
            foreach (ArrangementEntry entry in project.Arrangement ?? [])
                Arrangement.Add(new ArrangementEntry { PatternIndex = Math.Clamp(entry.PatternIndex, 0, 3), Repeats = Math.Clamp(entry.Repeats, 1, 64) });

            Swing = Math.Clamp(project.Swing, 0, 45);
            Humanise = project.Humanise;
            return Patterns[ActivePatternIndex]!;
        }

        public PatternFile SetActivePattern(int index)
        {
            Patterns[ActivePatternIndex] = CapturePattern();
            ActivePatternIndex = index;
            return Patterns[ActivePatternIndex] ?? new PatternFile();
        }

        public async Task PlayArrangementAsync(
            Func<PatternFile, int, Task> activatePatternAsync,
            Func<int, CancellationToken, Task> playPatternAsync,
            CancellationToken cancellationToken)
        {
            if (Arrangement.Count == 0)
            {
                Status = "Add patterns to the arrangement first.";
                return;
            }

            IsPlaying = true;
            try
            {
                for (int index = 0; index < Arrangement.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ArrangementEntry entry = Arrangement[index];
                    PatternFile pattern = Patterns[Math.Clamp(entry.PatternIndex, 0, Patterns.Length - 1)] ?? new PatternFile();
                    await activatePatternAsync(PatternDataService.ClonePattern(pattern), entry.PatternIndex);
                    await playPatternAsync(entry.Repeats, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Stopping an arrangement is an expected transport action.
            }
            catch (Exception ex)
            {
                Status = $"Arrangement playback failed: {ex.Message}";
            }
            finally
            {
                IsPlaying = false;
                CurrentStep = -1;
            }
        }

        public void AddArrangementEntry(int patternIndex, int repeats)
        {
            ArrangementEntry entry = new() { PatternIndex = Math.Clamp(patternIndex, 0, 3), Repeats = Math.Clamp(repeats, 1, 64) };
            Arrangement.Add(entry);
            SelectedArrangementEntry = entry;
            Status = $"Added pattern {(char)('A' + entry.PatternIndex)} to the arrangement.";
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyProjectChanged() => ProjectChanged?.Invoke(this, EventArgs.Empty);

        public void ResetHistory(string snapshot)
        {
            _undoHistory.Clear();
            _redoHistory.Clear();
            _lastHistorySnapshot = snapshot;
            RaiseHistoryState();
        }

        public void SetHistoryBaseline(string snapshot)
        {
            _lastHistorySnapshot = snapshot;
        }

        public void RecordProjectEdit(string snapshot, bool restoringHistory)
        {
            if (restoringHistory)
                return;

            if (_lastHistorySnapshot != null && !string.Equals(snapshot, _lastHistorySnapshot, StringComparison.Ordinal))
            {
                PushHistory(_undoHistory, _lastHistorySnapshot);
                _redoHistory.Clear();
            }

            _lastHistorySnapshot = snapshot;
            RaiseHistoryState();
        }

        public bool TryUndo(string currentSnapshot, out string previousSnapshot)
        {
            previousSnapshot = string.Empty;
            if (IsPlaying || _undoHistory.Count == 0)
                return false;

            PushHistory(_redoHistory, currentSnapshot);
            previousSnapshot = _undoHistory.Pop();
            _lastHistorySnapshot = previousSnapshot;
            RaiseHistoryState();
            return true;
        }

        public bool TryRedo(string currentSnapshot, out string nextSnapshot)
        {
            nextSnapshot = string.Empty;
            if (IsPlaying || _redoHistory.Count == 0)
                return false;

            PushHistory(_undoHistory, currentSnapshot);
            nextSnapshot = _redoHistory.Pop();
            _lastHistorySnapshot = nextSnapshot;
            RaiseHistoryState();
            return true;
        }

        private static void PushHistory(Stack<string> history, string snapshot)
        {
            history.Push(snapshot);
            if (history.Count <= MaximumHistoryEntries)
                return;

            string[] keep = history.Take(MaximumHistoryEntries).Reverse().ToArray();
            history.Clear();
            foreach (string item in keep)
                history.Push(item);
        }

        private void RaiseHistoryState()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }

        public bool CanUndo => !IsPlaying && _undoHistory.Count > 0;
        public bool CanRedo => !IsPlaying && _redoHistory.Count > 0;


        private void ClearPattern()
        {
            foreach (DrumInstrument track in Tracks)
            {
                Array.Fill(track.Steps, false);
                Array.Fill(track.StepVelocities, 100);
            }
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CopyArrangementEntry()
        {
            if (SelectedArrangementEntry == null)
                return;
            _arrangementClipboard = CloneEntry(SelectedArrangementEntry);
            Status = "Arrangement section copied.";
            RaiseCommandStates();
        }

        private void PasteArrangementEntry()
        {
            if (_arrangementClipboard == null)
                return;
            int index = SelectedArrangementEntry == null ? Arrangement.Count : Arrangement.IndexOf(SelectedArrangementEntry) + 1;
            ArrangementEntry pasted = CloneEntry(_arrangementClipboard);
            Arrangement.Insert(index, pasted);
            SelectedArrangementEntry = pasted;
            Status = "Arrangement section pasted.";
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DuplicateArrangementEntry()
        {
            if (SelectedArrangementEntry == null)
                return;
            int index = Arrangement.IndexOf(SelectedArrangementEntry) + 1;
            ArrangementEntry duplicate = CloneEntry(SelectedArrangementEntry);
            Arrangement.Insert(index, duplicate);
            SelectedArrangementEntry = duplicate;
            Status = "Arrangement section duplicated.";
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveArrangementEntry()
        {
            if (SelectedArrangementEntry == null)
                return;
            Arrangement.Remove(SelectedArrangementEntry);
            SelectedArrangementEntry = null;
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveArrangementEntry(int direction)
        {
            if (!CanMoveArrangementEntry(direction) || SelectedArrangementEntry == null)
                return;
            int index = Arrangement.IndexOf(SelectedArrangementEntry);
            int target = index + direction;
            ArrangementEntry entry = SelectedArrangementEntry;
            Arrangement.RemoveAt(index);
            Arrangement.Insert(target, entry);
            SelectedArrangementEntry = entry;
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool CanMoveArrangementEntry(int direction)
        {
            if (SelectedArrangementEntry == null)
                return false;
            int target = Arrangement.IndexOf(SelectedArrangementEntry) + direction;
            return target >= 0 && target < Arrangement.Count;
        }

        private static ArrangementEntry CloneEntry(ArrangementEntry entry) => new() { PatternIndex = entry.PatternIndex, Repeats = entry.Repeats };

        private void RaiseCommandStates()
        {
            ((RelayCommand)ClearPatternCommand).RaiseCanExecuteChanged();
            ((RelayCommand)CopyArrangementEntryCommand).RaiseCanExecuteChanged();
            ((RelayCommand)PasteArrangementEntryCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DuplicateArrangementEntryCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveArrangementEntryCommand).RaiseCanExecuteChanged();
            ((RelayCommand)MoveArrangementUpCommand).RaiseCanExecuteChanged();
            ((RelayCommand)MoveArrangementDownCommand).RaiseCanExecuteChanged();
        }
    }
}
