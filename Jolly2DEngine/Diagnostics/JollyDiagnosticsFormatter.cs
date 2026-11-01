using System.Globalization;
using System.Text;

namespace Jolly2DEngine.Diagnostics
{
    public static class JollyDiagnosticsFormatter
    {
        public static string Format(JollyDiagnosticsSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            StringBuilder builder = new();

            builder.Append("FPS: ");
            builder.Append(snapshot.FPS.ToString("0.0", CultureInfo.InvariantCulture));
            builder.AppendLine();

            builder.Append("Frame: ");
            builder.Append(snapshot.FrameTimeMilliseconds.ToString("0.00", CultureInfo.InvariantCulture));
            builder.AppendLine(" ms");

            builder.Append("Update: ");
            builder.Append(snapshot.UpdateTimeMilliseconds.ToString("0.00", CultureInfo.InvariantCulture));
            builder.AppendLine(" ms");

            builder.Append("Render: ");
            builder.Append(snapshot.RenderTimeMilliseconds.ToString("0.00", CultureInfo.InvariantCulture));
            builder.AppendLine(" ms");

            builder.Append("Entities: ");
            builder.Append(snapshot.EntityCount);
            builder.AppendLine();

            builder.Append("Sprites: ");
            builder.Append(snapshot.SpriteCount);
            builder.AppendLine();

            builder.Append("Textures: ");
            builder.Append(snapshot.TextureCount);
            builder.AppendLine();

            builder.Append("Batches: ");
            builder.Append(snapshot.RenderBatchCount);
            builder.AppendLine();

            builder.Append("Draw Calls: ");
            builder.Append(snapshot.RenderDrawCallCount);
            builder.AppendLine();

            builder.Append("Max Batch: ");
            builder.Append(snapshot.RenderMaxBatchSize);
            builder.AppendLine();

            builder.Append("Clients: ");
            builder.Append(snapshot.ConnectedClients);
            builder.AppendLine();

            builder.Append("Network Sent: ");
            builder.Append(snapshot.NetworkMessagesSent);
            builder.Append(" messages / ");
            builder.Append(FormatBytes(snapshot.NetworkBytesSent));
            builder.AppendLine();

            builder.Append("Network Received: ");
            builder.Append(snapshot.NetworkMessagesReceived);
            builder.Append(" messages / ");
            builder.Append(FormatBytes(snapshot.NetworkBytesReceived));
            builder.AppendLine();

            if (snapshot.NetworkLatencyMilliseconds > 0.0)
            {
                builder.Append("Network Latency: ");
                builder.Append(snapshot.NetworkLatencyMilliseconds.ToString("0.00", CultureInfo.InvariantCulture));
                builder.AppendLine(" ms");
            }

            if (snapshot.Profiles.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Profiles:");

                foreach (KeyValuePair<string, JollyProfileSnapshot> pair in snapshot.Profiles)
                {
                    JollyProfileSnapshot profile = pair.Value;

                    builder.Append(profile.Name);
                    builder.Append(": ");
                    builder.Append(profile.AverageMilliseconds.ToString("0.000", CultureInfo.InvariantCulture));
                    builder.Append(" ms avg, ");
                    builder.Append(profile.MaximumMilliseconds.ToString("0.000", CultureInfo.InvariantCulture));
                    builder.Append(" ms max, ");
                    builder.Append(profile.CallCount);
                    builder.AppendLine(" calls");
                }
            }

            return builder.ToString();
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return bytes.ToString(CultureInfo.InvariantCulture) + " B";
            }

            if (bytes < 1024 * 1024)
            {
                return (bytes / 1024.0).ToString("0.0", CultureInfo.InvariantCulture) + " KB";
            }

            if (bytes < 1024L * 1024L * 1024L)
            {
                return (bytes / (1024.0 * 1024.0)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
            }

            return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("0.0", CultureInfo.InvariantCulture) + " GB";
        }
    }
}
