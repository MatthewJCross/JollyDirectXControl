using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JollyDrumMachine.Models
{
    public sealed class DrumInstrument : INotifyPropertyChanged
    {
        private string _name;
        private float _volume = 0.85f;
        private float _pan;
        private float _tune;
        private float _attack;
        private float _decay = 10000.0f;
        private bool _muted;
        private bool _solo;

        public DrumInstrument(string name, string samplePath, string sampleKey)
        {
            _name = name;
            SamplePath = samplePath;
            SampleKey = sampleKey;
            Steps = new bool[16];
            StepVelocities = Enumerable.Repeat(100, 16).ToArray();
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string SamplePath { get; set; }

        public string SampleKey { get; set; }

        public bool[] Steps { get; set; }

        public int[] StepVelocities { get; set; }

        public float Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, Math.Clamp(value, 0.0f, 1.0f));
        }

        // -1 = full left, 0 = centre, +1 = full right.
        public float Pan
        {
            get => _pan;
            set => SetProperty(ref _pan, Math.Clamp(value, -1.0f, 1.0f));
        }

        // Semitones, with a range of -24 to +24.
        public float Tune
        {
            get => _tune;
            set => SetProperty(ref _tune, Math.Clamp(value, -24.0f, 24.0f));
        }

        // Attack time in milliseconds.
        public float Attack
        {
            get => _attack;
            set => SetProperty(ref _attack, Math.Clamp(value, 0.0f, 1000.0f));
        }

        // Decay time in milliseconds.
        public float Decay
        {
            get => _decay;
            set => SetProperty(ref _decay, Math.Clamp(value, 1.0f, 10000.0f));
        }

        public bool Muted
        {
            get => _muted;
            set => SetProperty(ref _muted, value);
        }

        public bool Solo
        {
            get => _solo;
            set => SetProperty(ref _solo, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
