using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jolly2DEngine.Save
{
    public sealed class JollySaveManager
    {
        public const int CurrentFormatVersion = 1;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public string SaveDirectory { get; }

        public JollySaveManager(string? saveDirectory = null)
        {
            SaveDirectory = string.IsNullOrWhiteSpace(saveDirectory)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jolly2D", "Saves")
                : Path.GetFullPath(saveDirectory);

            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
            Directory.CreateDirectory(SaveDirectory);
        }

        public void SaveGame<T>(string slot, T state, int gameVersion = 1)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slot);
            ArgumentNullException.ThrowIfNull(state);

            ArgumentOutOfRangeException.ThrowIfLessThan(gameVersion, 1);

            string path = GetSavePath(slot);
            string temporaryPath = path + ".tmp";
            string backupPath = path + ".bak";

            JollySaveFile<T> saveFile = new()
            {
                FormatVersion = CurrentFormatVersion,
                GameVersion = gameVersion,
                SavedUtc = DateTimeOffset.UtcNow,
                Payload = state
            };

            string json = JsonSerializer.Serialize(saveFile, _jsonOptions);
            Directory.CreateDirectory(SaveDirectory);
            File.WriteAllText(temporaryPath, json);

            try
            {
                if (File.Exists(path))
                {
                    File.Copy(path, backupPath, true);
                }

                File.Move(temporaryPath, path, true);
            }
            catch
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }

                throw;
            }
        }

        public T LoadGame<T>(string slot, int? expectedGameVersion = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slot);

            string path = GetSavePath(slot);

            if (!File.Exists(path))
            {
                throw new JollySaveException($"Save slot '{slot}' does not exist.");
            }

            try
            {
                string json = File.ReadAllText(path);
                JollySaveFile<T>? saveFile = JsonSerializer.Deserialize<JollySaveFile<T>>(json, _jsonOptions);

                if (saveFile == null || saveFile.Payload is null)
                {
                    throw new JollySaveException($"Save slot '{slot}' is empty or invalid.");
                }

                if (saveFile.FormatVersion != CurrentFormatVersion)
                {
                    throw new JollySaveException($"Save slot '{slot}' uses unsupported save format version {saveFile.FormatVersion}.");
                }

                if (expectedGameVersion.HasValue && saveFile.GameVersion != expectedGameVersion.Value)
                {
                    throw new JollySaveException($"Save slot '{slot}' uses game version {saveFile.GameVersion}, expected {expectedGameVersion.Value}.");
                }

                return saveFile.Payload;
            }
            catch (JollySaveException)
            {
                throw;
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                throw new JollySaveException($"Save slot '{slot}' could not be loaded.", ex);
            }
        }

        public bool TryLoadGame<T>(string slot, out T? state, int? expectedGameVersion = null)
        {
            try
            {
                state = LoadGame<T>(slot, expectedGameVersion);
                return true;
            }
            catch (JollySaveException)
            {
                state = default;
                return false;
            }
        }

        public bool Exists(string slot)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slot);
            return File.Exists(GetSavePath(slot));
        }

        public bool HasBackup(string slot)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slot);
            return File.Exists(GetBackupPath(slot));
        }

        public bool RestoreBackup(string slot)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slot);

            string path = GetSavePath(slot);
            string backupPath = GetBackupPath(slot);

            if (!File.Exists(backupPath))
            {
                return false;
            }

            File.Copy(backupPath, path, true);
            return true;
        }

        public void Delete(string slot)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slot);

            string path = GetSavePath(slot);
            string backupPath = GetBackupPath(slot);
            string temporaryPath = path + ".tmp";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        public IReadOnlyList<string> GetSlots()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                return [];
            }

            return Directory.EnumerateFiles(SaveDirectory, "*.json", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray()!;
        }

        private string GetSavePath(string slot)
        {
            string safeSlot = ValidateSlot(slot);
            return Path.Combine(SaveDirectory, safeSlot + ".json");
        }

        private string GetBackupPath(string slot)
        {
            string safeSlot = ValidateSlot(slot);
            return Path.Combine(SaveDirectory, safeSlot + ".json.bak");
        }

        private static string ValidateSlot(string slot)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slot);

            if (slot.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || slot.Contains(Path.DirectorySeparatorChar) || slot.Contains(Path.AltDirectorySeparatorChar))
            {
                throw new ArgumentException("Save slot names may only contain valid file-name characters and cannot contain directory separators.", nameof(slot));
            }

            return slot.Trim();
        }

        private sealed class JollySaveFile<T>
        {
            public int FormatVersion { get; set; }
            public int GameVersion { get; set; }
            public DateTimeOffset SavedUtc { get; set; }
            public T? Payload { get; set; }
        }
    }

    public sealed class JollySaveException : Exception
    {
        public JollySaveException(string message)
            : base(message)
        {
        }

        public JollySaveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
