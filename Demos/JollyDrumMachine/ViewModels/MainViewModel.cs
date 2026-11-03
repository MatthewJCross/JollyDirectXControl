using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JollyDrumMachine.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private double _tempo = 120.0;
        private double _masterVolume = 0.85;
        private int _currentStep = -1;
        private bool _isPlaying;
        private string _status = "Ready · Drag an instrument into the sequencer or add your own WAV/MP3 sample.";

        public double Tempo
        {
            get => _tempo;
            set
            {
                double clamped = Math.Clamp(value, 40.0, 240.0);
                if (Math.Abs(_tempo - clamped) < 0.001)
                    return;
                _tempo = clamped;
                OnPropertyChanged();
            }
        }

        public double MasterVolume
        {
            get => _masterVolume;
            set
            {
                double clamped = Math.Clamp(value, 0.0, 1.0);
                if (Math.Abs(_masterVolume - clamped) < 0.001) 
                    return;
                _masterVolume = clamped;
                OnPropertyChanged();
            }
        }

        public int CurrentStep
        {
            get => _currentStep;
            set 
            { 
                if (_currentStep == value) 
                    return; _currentStep = value; 
                OnPropertyChanged(); 
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set 
            { 
                if (_isPlaying == value) 
                    return; 
                _isPlaying = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(PlaybackStatus)); 
                OnPropertyChanged(nameof(PlayButtonText)); 
            }
        }

        public string PlaybackStatus => IsPlaying ? "PLAYING" : "STOPPED";
        public string PlayButtonText => IsPlaying ? "▶  PLAYING" : "▶  PLAY";

        public string Status
        {
            get => _status;
            set 
            { 
                if (_status == value) 
                    return; 
                _status = value; 
                OnPropertyChanged(); 
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}