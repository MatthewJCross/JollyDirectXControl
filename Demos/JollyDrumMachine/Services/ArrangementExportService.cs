using JollyDrumMachine.Models;
using NAudio.Wave;
using System.IO;

namespace JollyDrumMachine.Services
{
    /// <summary>Runs offline arrangement rendering and optional MP3 encoding without UI dependencies.</summary>
    public sealed class ArrangementExportService(ArrangementRenderer? renderer = null)
    {
        private readonly ArrangementRenderer _renderer = renderer ?? new ArrangementRenderer();

        public async Task ExportAsync(IReadOnlyList<PatternFile> patterns, IReadOnlyList<ArrangementEntry> arrangement, string destinationPath, bool mp3, float masterVolume, double swing, bool humanise, CancellationToken cancellationToken, IProgress<double>? progress = null, Action<string>? status = null)
        {
            string? temporaryWav = null;

            try
            {
                string renderPath = destinationPath;

                if (mp3)
                {
                    renderPath = temporaryWav = Path.Combine(Path.GetTempPath(), $"JollyDrumMachine_{Guid.NewGuid():N}.wav");
                }

                await _renderer.RenderWavAsync(patterns, arrangement, renderPath, masterVolume, swing, humanise, progress, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (!mp3)
                {
                    return;
                }

                status?.Invoke("Encoding MP3…");

                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using WaveFileReader reader = new(renderPath);
                    MediaFoundationEncoder.EncodeToMp3(reader, destinationPath, 192000);
                    cancellationToken.ThrowIfCancellationRequested();
                }, cancellationToken);
            }
            finally
            {
                if (temporaryWav != null && File.Exists(temporaryWav))
                {
                    File.Delete(temporaryWav);
                }
            }
        }
    }
}
