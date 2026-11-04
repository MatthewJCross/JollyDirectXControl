using JollyDrumMachine.Models;
using System.Diagnostics;

namespace JollyDrumMachine.Services
{
    /// <summary>Runs the configurable step clock independently of WPF controls and Dispatcher timing.</summary>
    public sealed class DrumSequencerService
    {
        public static async Task RunAsync(Func<double> getTempo, Func<DrumInstrument[]> getTracks, Action<DrumInstrument, int> triggerTrack, Action<int> stepChanged, Func<int> getStepCount, Func<double> getSwing, Func<bool> getHumanise, CancellationToken cancellationToken, int loopCount = 0)
        {
            Stopwatch clock = Stopwatch.StartNew();
            double nextTargetMs = 0;
            double previousTempo = getTempo();
            int step = 0;
            int totalSteps = 0;

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

                    // Humanise the trigger within its scheduled step without moving the
                    // clock target itself, so timing variation cannot accumulate as drift.
                    if (getHumanise())
                    {
                        double timingJitterMs = Random.Shared.NextDouble() * stepDurationMs * 0.03;
                        if (timingJitterMs >= 1.0)
                            await Task.Delay((int)timingJitterMs, cancellationToken).ConfigureAwait(false);
                    }

                    int currentStep = step;
                    foreach (DrumInstrument track in getTracks())
                    {
                        if (currentStep < track.Steps.Length && track.Steps[currentStep] && !track.Muted)
                        {
                            int velocity = track.StepVelocities.Length > currentStep ? track.StepVelocities[currentStep] : 100;
                            if (getHumanise())
                                velocity = Math.Clamp(velocity + Random.Shared.Next(-8, 9), 1, 127);
                            try { triggerTrack(track, velocity); }
                            catch { /* A single unavailable sample should not stop the sequencer clock. */ }
                        }
                    }

                    stepChanged(currentStep);
                    step = (step + 1) % Math.Max(1, getStepCount());
                    totalSteps++;
                    double swingAmount = Math.Clamp(getSwing(), 0, 0.45);
                    nextTargetMs += stepDurationMs * (currentStep % 2 == 0 ? 1.0 + swingAmount : 1.0 - swingAmount);
                    if (loopCount > 0 && totalSteps >= Math.Max(1, getStepCount()) * loopCount)
                        break;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Normal stop/shutdown.
            }
        }
    }
}