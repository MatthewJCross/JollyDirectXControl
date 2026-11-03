using JollyDrumMachine.Models;
using System.Diagnostics;

namespace JollyDrumMachine.Services
{
    /// <summary>Runs the 16-step clock independently of WPF controls and Dispatcher timing.</summary>
    public sealed class DrumSequencerService
    {
        public async Task RunAsync(Func<double> getTempo, Func<DrumInstrument[]> getTracks, Action<DrumInstrument> triggerTrack, Action<int> stepChanged, CancellationToken cancellationToken)
        {
            Stopwatch clock = Stopwatch.StartNew();
            double nextTargetMs = 0;
            double previousTempo = getTempo();
            int step = 0;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    double tempo = Math.Clamp(getTempo(), 40.0, 240.0);
                    double stepDurationMs = 60000.0 / tempo / 4.0;

                    if (Math.Abs(tempo - previousTempo) > 0.001)
                    {
                        nextTargetMs = clock.Elapsed.TotalMilliseconds;
                        previousTempo = tempo;
                    }

                    double waitMs = nextTargetMs - clock.Elapsed.TotalMilliseconds;
                    while (waitMs > 1.0)
                    {
                        await Task.Delay((int)Math.Min(waitMs, 20.0), cancellationToken).ConfigureAwait(false);
                        waitMs = nextTargetMs - clock.Elapsed.TotalMilliseconds;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    int currentStep = step;
                    foreach (DrumInstrument track in getTracks())
                    {
                        if (currentStep < track.Steps.Length && track.Steps[currentStep] && !track.Muted)
                        {
                            try { triggerTrack(track); }
                            catch { /* A single unavailable sample should not stop the sequencer clock. */ }
                        }
                    }

                    stepChanged(currentStep);
                    step = (step + 1) % 16;
                    nextTargetMs += stepDurationMs;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Normal stop/shutdown.
            }
        }
    }
}