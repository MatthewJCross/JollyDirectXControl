using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JollyDrumMachine.Models
{
    public sealed class DrumInstrument : INotifyPropertyChanged
    {
        private string _name;
        private float _volume = 0.85f;
        private bool _muted;

        public DrumInstrument(string name, string samplePath, string sampleKey)
        {
            _name = name;
            SamplePath = samplePath;
            SampleKey = sampleKey;
            Steps = new bool[16];
        }

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string SamplePath { get; set; }
        public string SampleKey { get; set; }
        public bool[] Steps { get; set; }
        public float Volume { get => _volume; set { _volume = Math.Clamp(value, 0.0f, 1.0f); OnPropertyChanged(); } }
        public bool Muted { get => _muted; set { _muted = value; OnPropertyChanged(); } }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}