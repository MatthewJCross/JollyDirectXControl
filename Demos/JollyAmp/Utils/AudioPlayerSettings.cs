using System.IO;
using System.Text.Json;

namespace JollyAmp
{
    public sealed class AudioPlayerSettings
    {
        public string? LastAudioFolder { get; set; }

        private static readonly string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JollyAmp");

        private static readonly string SettingsPath = Path.Combine(SettingsFolder, "settings.json");

        public static AudioPlayerSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);

                    return JsonSerializer.Deserialize<AudioPlayerSettings>(json) ?? new();
                }
            }
            catch
            {
                // Use default settings if the file is invalid.
            }

            return new AudioPlayerSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsFolder);

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Settings failure should not prevent playback.
            }
        }
    }
}
