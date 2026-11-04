using JollyDrumMachine.Models;

namespace JollyDrumMachine.Services
{
    /// <summary>Pure conversions between editable tracks and serialisable pattern data.</summary>
    public static class PatternDataService
    {
        public static PatternFile CapturePattern(double tempo, int stepsPerPattern, IEnumerable<DrumInstrument> tracks)
        {
            return new PatternFile
            {
                Tempo = (int)Math.Round(tempo),
                StepsPerBar = stepsPerPattern,
                Instruments = tracks.Select(track => new PatternInstrument
                {
                    Pan = track.Pan,
                    Tune = track.Tune,
                    Attack = track.Attack,
                    Decay = track.Decay,
                    Solo = track.Solo,
                    Name = track.Name,
                    SamplePath = track.SamplePath,
                    Volume = track.Volume,
                    Muted = track.Muted,
                    Steps = track.Steps.ToArray(),
                    StepVelocities = track.StepVelocities.ToArray()
                }).ToList()
            };
        }

        public static PatternFile ClonePattern(PatternFile pattern)
        {
            return new PatternFile
            {
                Tempo = pattern.Tempo,
                StepsPerBar = pattern.StepsPerBar,
                Instruments = (pattern.Instruments ?? []).Select(item => new PatternInstrument
                {
                    Pan = item.Pan,
                    Tune = item.Tune,
                    Attack = item.Attack,
                    Decay = item.Decay,
                    Solo = item.Solo,
                    Name = item.Name,
                    SamplePath = item.SamplePath,
                    Volume = item.Volume,
                    Muted = item.Muted,
                    Steps = item.Steps?.ToArray() ?? new bool[pattern.StepsPerBar],
                    StepVelocities = item.StepVelocities?.ToArray() ?? Enumerable.Repeat(100, pattern.StepsPerBar).ToArray()
                }).ToList()
            };
        }

        public static bool[] ResizeSteps(bool[]? source, int length)
        {
            bool[] result = new bool[length];
            if (source != null)
                Array.Copy(source, result, Math.Min(source.Length, result.Length));
            return result;
        }

        public static int[] ResizeVelocities(int[]? source, int length)
        {
            int[] result = Enumerable.Repeat(100, length).ToArray();
            if (source != null)
                Array.Copy(source, result, Math.Min(source.Length, result.Length));

            for (int index = 0; index < result.Length; index++)
                result[index] = Math.Clamp(result[index], 1, 127);

            return result;
        }
    }
}
