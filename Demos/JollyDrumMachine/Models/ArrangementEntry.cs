namespace JollyDrumMachine.Models
{
    public sealed class ArrangementEntry
    {
        public int PatternIndex { get; set; }
        public int Repeats { get; set; } = 1;
        public string Label => $"{(char)('A' + Math.Clamp(PatternIndex, 0, 3))}  × {Repeats}";
        public override string ToString() => Label;
    }
}
