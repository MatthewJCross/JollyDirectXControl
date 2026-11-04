using JollyDrumMachine.Models;
using System.IO;
using System.Text.Json;

namespace JollyDrumMachine.Services
{
    /// <summary>Owns project/pattern JSON persistence, keeping file format concerns out of the window.</summary>
    public sealed class ProjectFileService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static ProjectDocument LoadDocument(string path)
        {
            string json = File.ReadAllText(path);
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException("The selected file is not a valid JSON object.");

            if (document.RootElement.TryGetProperty(nameof(PatternProject.Patterns), out _))
            {
                PatternProject project = JsonSerializer.Deserialize<PatternProject>(json)
                    ?? throw new InvalidDataException("The selected project is empty or invalid.");
                if (project.Patterns == null || project.Patterns.Count == 0)
                    throw new InvalidDataException("The selected project does not contain any patterns.");
                return new ProjectDocument(project, null);
            }

            PatternFile pattern = JsonSerializer.Deserialize<PatternFile>(json)
                ?? throw new InvalidDataException("The selected pattern is empty or invalid.");
            return new ProjectDocument(null, pattern);
        }

        public static PatternProject LoadProject(string path)
        {
            ProjectDocument document = LoadDocument(path);
            return document.Project ?? throw new InvalidDataException("The selected file is not a complete project.");
        }

        public static PatternFile LoadPattern(string path)
        {
            ProjectDocument document = LoadDocument(path);
            return document.Pattern ?? throw new InvalidDataException("The selected file is a project, not a single pattern.");
        }

        public static void SaveProject(string path, PatternProject project) => SaveJson(path, project);

        public static void SavePattern(string path, PatternFile pattern) => SaveJson(path, pattern);

        public static void SaveRecentProjectPath(string settingsFolder, string path)
        {
            Directory.CreateDirectory(settingsFolder);
            File.WriteAllText(Path.Combine(settingsFolder, "recent-project.txt"), path);
        }

        public static string? LoadRecentProjectPath(string settingsFolder)
        {
            string path = Path.Combine(settingsFolder, "recent-project.txt");
            return File.Exists(path) ? File.ReadAllText(path).Trim() : null;
        }

        private static void SaveJson<T>(string path, T value)
        {
            string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions));
        }
    }

    public sealed record ProjectDocument(PatternProject? Project, PatternFile? Pattern);
}
