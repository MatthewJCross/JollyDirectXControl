
using JollyAmp.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace JollyAmp
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private string? _lastAudioFolder;
        private readonly AudioPlayerSettings _settings;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _settings = AudioPlayerSettings.Load();
            _lastAudioFolder = _settings.LastAudioFolder;
            
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            StateChanged += MainWindow_StateChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Activate();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _viewModel.Dispose();
        }

        private void OpenAudioFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenAudioFiles();
        }

        private void OpenAudioFiles()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Open audio files",
                Filter = "Audio files|*.mp3;*.wav;*.aiff;*.aif;*.wma;*.aac;*.m4a|All files|*.*",
                Multiselect = true,
                CheckFileExists = true
            };

            string initialFolder = !string.IsNullOrWhiteSpace(_lastAudioFolder) && Directory.Exists(_lastAudioFolder) ? _lastAudioFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            if (Directory.Exists(initialFolder))
            {
                dialog.InitialDirectory = initialFolder;
            }

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            _lastAudioFolder = Path.GetDirectoryName(dialog.FileNames[0]);
            _settings.LastAudioFolder = _lastAudioFolder;
            _settings.Save();

            _viewModel.SetPlaylist(dialog.FileNames, true);
        }

        private void PlaylistList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _viewModel.PlaySelectedTrack();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
            {
                OpenAudioFiles();
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                _viewModel.TogglePlayback();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel.StopPlayback();
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp)
            {
                _viewModel.PlayPreviousTrack();
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                _viewModel.PlayNextTrack();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && PlaylistList.IsKeyboardFocusWithin)
            {
                _viewModel.PlaySelectedTrack();
                e.Handled = true;
            }
        }

        private void PlaybackSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.IsSeeking = true;
            }
        }

        private void PlaybackSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.IsSeeking = false;
                viewModel.PlaybackPosition = PlaybackSlider.Value;
            }
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MaximizeButton.Visibility = Visibility.Visible;
                RestoreButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
    }
}
