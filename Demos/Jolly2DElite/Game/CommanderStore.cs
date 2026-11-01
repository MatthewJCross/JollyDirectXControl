using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Jolly2DElite.Game;

public sealed class CommanderStore
{
    public const int CommanderDataSize = 256;
    private const string SaveFolderName = "Commanders";
    private readonly string _dataFolder;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public CommanderStore(string dataFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataFolder);

        _dataFolder = dataFolder;
    }

    public string SaveDirectory => Path.Combine(_dataFolder, SaveFolderName);

    public List<CommanderSave> LoadAll()
    {
        Directory.CreateDirectory(SaveDirectory);

        List<CommanderSave> saves = [];

        foreach (string filePath in Directory.EnumerateFiles(SaveDirectory, "*.json"))
        {
            CommanderSave? save = TryReadSave(filePath);

            if (save is not null)
            {
                saves.Add(save);
            }
        }

        return saves.OrderByDescending(save => save.Saved).ToList();
    }

    public void EnsureSampleSaves()
    {
        if (LoadAll().Count > 0)
        {
            return;
        }

        (string Name, uint Cash, int Fuel, int Kills, int LegalStatus)[] samples =
        [
            ("JAMESON", 100, 70, 0, 0),
            ("NOVA", 2500, 55, 18, 0),
            ("COBRA", 12750, 70, 85, 0),
            ("VIPER", 85000, 65, 600, 5),
            ("ELITE", 250000, 70, 6500, 50)
        ];

        DateTimeOffset savedAt = DateTimeOffset.Now;

        foreach (var sample in samples)
        {
            byte[] data = CreateSampleCommanderData(sample.Cash, sample.Fuel, sample.Kills, sample.LegalStatus);
            Save(sample.Name, "Sample Commander", data, savedAt);
            savedAt = savedAt.AddSeconds(-1);
        }
    }

    private static byte[] CreateSampleCommanderData(uint cash, int fuel, int kills, int legalStatus)
    {
        byte[] data = new byte[CommanderDataSize];

        data[1] = (byte)DefaultCommander.SystemX;
        data[2] = (byte)DefaultCommander.SystemY;
        DefaultCommander.GalaxySeeds.CopyTo(data.AsSpan(3, 6));

        data[9] = (byte)(cash >> 24);
        data[10] = (byte)(cash >> 16);
        data[11] = (byte)(cash >> 8);
        data[12] = (byte)cash;

        data[13] = (byte)fuel;
        data[15] = 0;

        data[16] = (byte)DefaultCommander.FrontLaser;
        data[22] = (byte)DefaultCommander.CargoCapacity;
        data[51] = (byte)DefaultCommander.Missiles;
        data[52] = (byte)legalStatus;

        data[71] = (byte)kills;
        data[72] = (byte)(kills >> 8);

        return data;
    }

    public CommanderSave Save(string commander, string label, byte[] data, DateTimeOffset savedAt, CommanderSave? existingSave = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commander);
        ArgumentNullException.ThrowIfNull(data);

        if (!IsValidCommanderData(data))
        {
            throw new ArgumentException("The commander data is not a valid 256-byte commander file.", nameof(data));
        }

        Directory.CreateDirectory(SaveDirectory);

        string destination = existingSave?.Path ?? CreateSavePath(commander, savedAt);
        string temporaryPath = destination + ".tmp";

        SaveDocument document = new()
        {
            Version = 1,
            Commander = commander,
            Label = label,
            Saved = savedAt,
            Data = Convert.ToHexString(data)
        };

        string json = JsonSerializer.Serialize(document, SerializerOptions);

        try
        {
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return new CommanderSave(destination, commander, label, savedAt, data);
    }

    internal CommanderSave Save(Commander commander, string label, DateTimeOffset savedAt, CommanderSave? existingSave = null)
    {
        ArgumentNullException.ThrowIfNull(commander);

        byte[] data = commander.CreateSaveData();

        return Save(commander.Name, label, data, savedAt, existingSave);
    }

    public static void Delete(CommanderSave save)
    {
        ArgumentNullException.ThrowIfNull(save);
        File.Delete(save.Path);
    }

    public static bool IsValidCommanderData(byte[] data)
    {
        return data.Length == CommanderDataSize && (data[0] & 0x80) == 0;
    }

    public static List<(string Commander, List<CommanderSave> Saves)> GroupByCommander(IEnumerable<CommanderSave> saves)
    {
        ArgumentNullException.ThrowIfNull(saves);
        return saves.GroupBy(save => save.Commander, StringComparer.Ordinal).Select(group => (Commander: group.Key, Saves: group.OrderByDescending(save => save.Saved).ToList())).OrderByDescending(group => group.Saves[0].Saved).ToList();
    }

    public static string SystemNameAt(ReadOnlySpan<byte> galaxySeeds, int x, int y)
    {
        if (galaxySeeds.Length < 6)
        {
            throw new ArgumentException("Six galaxy seed bytes are required.", nameof(galaxySeeds));
        }

        int[] seeds = galaxySeeds[..6].ToArray().Select(seed => (int)seed).ToArray();
        for (int systemIndex = 0; systemIndex < 256; systemIndex++)
        {
            if (seeds[3] == x && seeds[1] == y)
            {
                return BuildSystemName(seeds);
            }

            for (int step = 0; step < 4; step++)
            {
                AdvanceSeeds(seeds);
            }
        }

        return string.Empty;
    }

    private static string BuildSystemName(int[] seeds)
    {
        int pairCount = (seeds[0] & 0x40) != 0 ? 4 : 3;
        System.Text.StringBuilder name = new();

        for (int i = 0; i < pairCount; i++)
        {
            int pairIndex = seeds[5] & 31;

            if (pairIndex != 0)
            {
                name.Append(SystemNames.LetterPairs[pairIndex]);
            }

            AdvanceSeeds(seeds);
        }

        return name.ToString();
    }

    private static void AdvanceSeeds(int[] seeds)
    {
        int lowSum = seeds[0] + seeds[2];
        int highSum = seeds[1] + seeds[3] + (lowSum > 255 ? 1 : 0);

        int low = lowSum & 0xFF;
        int high = highSum & 0xFF;

        seeds[0] = seeds[2];
        seeds[1] = seeds[3];
        seeds[3] = seeds[5];
        seeds[2] = seeds[4];

        int nextLow = low + seeds[2];

        seeds[4] = nextLow & 0xFF;
        seeds[5] = (high + seeds[3] + (nextLow > 255 ? 1 : 0)) & 0xFF;
    }

    private CommanderSave? TryReadSave(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            SaveDocument? document = JsonSerializer.Deserialize<SaveDocument>(json, SerializerOptions);

            if (document?.Commander is not { Length: > 0 } commander ||
                string.IsNullOrWhiteSpace(document.Data))
            {
                return null;
            }

            byte[] data = Convert.FromHexString(document.Data);

            if (!IsValidCommanderData(data))
            {
                return null;
            }

            return new CommanderSave(filePath, commander, document.Label ?? string.Empty, document.Saved, data);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or FormatException)
        {
            return null;
        }
    }

    private string CreateSavePath(string commander, DateTimeOffset savedAt)
    {
        char[] invalidCharacters = Path.GetInvalidFileNameChars();

        string safeName = new(commander.Select(character => invalidCharacters.Contains(character) || character == '.' ? '_' : character).ToArray());

        string timestamp = savedAt.ToLocalTime().ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        string baseName = $"{safeName}-{timestamp}";
        string candidate = Path.Combine(SaveDirectory, baseName + ".json");

        for (int suffix = 2; File.Exists(candidate); suffix++)
        {
            candidate = Path.Combine(SaveDirectory, $"{baseName}-{suffix}.json");
        }

        return candidate;
    }

    private sealed class SaveDocument
    {
        public int Version { get; set; }
        public string? Commander { get; set; }
        public string? Label { get; set; }
        public DateTimeOffset Saved { get; set; }
        public string? Data { get; set; }
    }
}
