namespace JollyDrumMachine.Models
{
    public sealed class PatternFile
    {
        public int Tempo { get; set; } = 120;

        public int StepsPerBar { get; set; } = 16;

        public List<PatternInstrument> Instruments { get; set; } = [];
    }

    public sealed class PatternInstrument
    {
        public string Name { get; set; } = "";

        public string SamplePath { get; set; } = "";

        public float Volume { get; set; } = 0.85f;

        public float Pan { get; set; }

        public float Tune { get; set; }

        public float Attack { get; set; }

        public float Decay { get; set; } = 10000.0f;

        public bool Muted { get; set; }

        public bool Solo { get; set; }

        public bool[] Steps { get; set; } = new bool[16];

        public int[] StepVelocities { get; set; } = [.. Enumerable.Repeat(100, 16)];
    }
}
