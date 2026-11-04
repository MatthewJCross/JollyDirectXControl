namespace JollyDrumMachine.Models
{
    public sealed class PatternProject
    {
        public int Version { get; set; } = 3;

        public int ActivePatternIndex { get; set; }

        public List<PatternFile> Patterns { get; set; } = [];

        public List<ArrangementEntry> Arrangement { get; set; } = [];

        public double Swing { get; set; }

        public bool Humanise { get; set; }
    }
}
