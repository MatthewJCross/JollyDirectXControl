using Jolly2DEngine.Audio;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Rendering.DirectX;
using System.IO;

namespace Jolly2DEngine.Assets;

public sealed class JollyAssetManager : IDisposable
{
    public const string TexturesFolder = "Textures";
    public const string AudioFolder = "Audio";
    public const string FontsFolder = "Fonts";
    public const string MapsFolder = "Maps";
    public const string AnimationsFolder = "Animations";

    private readonly DirectXJollyRenderer _renderer;
    private readonly JollyAudioManager? _audio;
    private readonly object _lock = new();
    private readonly Dictionary<string, TextureEntry> _textures = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _sounds = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public JollyAssetManager(DirectXJollyRenderer renderer, JollyAudioManager? audio = null, string? rootPath = null)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _renderer = renderer;
        _audio = audio;
        RootPath = Path.GetFullPath(rootPath ?? Path.Combine(AppContext.BaseDirectory, "Assets"));
    }

    public string RootPath { get; }

    public string TexturesPath => Path.Combine(RootPath, TexturesFolder);
    public string AudioPath => Path.Combine(RootPath, AudioFolder);
    public string FontsPath => Path.Combine(RootPath, FontsFolder);
    public string MapsPath => Path.Combine(RootPath, MapsFolder);
    public string AnimationsPath => Path.Combine(RootPath, AnimationsFolder);

    public JollyTexture LoadTexture(string assetName)
    {
        ThrowIfDisposed();
        string key = NormalizeAssetName(assetName);

        lock (_lock)
        {
            if (_textures.TryGetValue(key, out TextureEntry? entry))
            {
                entry.ReferenceCount++;
                return entry.Texture;
            }
        }

        string path = ResolveAssetPath(TexturesFolder, key);
        JollyTexture texture = _renderer.LoadTexture(path);

        lock (_lock)
        {
            if (_textures.TryGetValue(key, out TextureEntry? existing))
            {
                existing.ReferenceCount++;
                return existing.Texture;
            }

            _textures.Add(key, new TextureEntry(texture));
            return texture;
        }
    }

    public async Task<JollyTexture> LoadTextureAsync(string assetName, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        string key = NormalizeAssetName(assetName);

        lock (_lock)
        {
            if (_textures.TryGetValue(key, out TextureEntry? entry))
            {
                entry.ReferenceCount++;
                return entry.Texture;
            }
        }

        string path = ResolveAssetPath(TexturesFolder, key);
        JollyTexture texture = await Task.Run(() => _renderer.LoadTexture(path), cancellationToken).ConfigureAwait(false);

        lock (_lock)
        {
            if (_textures.TryGetValue(key, out TextureEntry? existing))
            {
                existing.ReferenceCount++;
                return existing.Texture;
            }

            _textures.Add(key, new TextureEntry(texture));
            return texture;
        }
    }

    public JollyAssetHandle<JollyTexture> AcquireTexture(string assetName)
    {
        string key = NormalizeAssetName(assetName);
        JollyTexture texture = LoadTexture(key);
        return new JollyAssetHandle<JollyTexture>(texture, () => ReleaseTexture(key));
    }

    public void ReleaseTexture(string assetName)
    {
        ThrowIfDisposed();
        string key = NormalizeAssetName(assetName);

        lock (_lock)
        {
            if (!_textures.TryGetValue(key, out TextureEntry? entry))
            {
                return;
            }

            entry.ReferenceCount--;

            if (entry.ReferenceCount <= 0)
            {
                _textures.Remove(key);
                _renderer.UnloadTexture(entry.Texture);
            }
        }
    }

    public bool IsTextureLoaded(string assetName)
    {
        string key = NormalizeAssetName(assetName);
        lock (_lock)
        {
            return _textures.ContainsKey(key);
        }
    }

    public void LoadSound(string assetName)
    {
        ThrowIfDisposed();

        if (_audio == null)
        {
            throw new InvalidOperationException("A JollyAudioManager is required to load audio assets.");
        }

        string key = NormalizeAssetName(assetName);

        lock (_lock)
        {
            if (_sounds.ContainsKey(key))
            {
                return;
            }
        }

        string path = ResolveAudioPath(key);
        _audio.LoadSound(key, path);

        lock (_lock)
        {
            _sounds[key] = path;
        }
    }

    private string ResolveAudioPath(string assetName)
    {
        string normalized = NormalizeAssetName(assetName);

        if (Path.HasExtension(normalized))
        {
            return ResolveAssetPath(AudioFolder, normalized);
        }

        string[] extensions =
        [
            ".wav",
            ".mp3",
            ".ogg"
        ];

        foreach (string extension in extensions)
        {
            string path = ResolveAssetPath(AudioFolder, normalized + extension);

            if (File.Exists(path))
            {
                return path;
            }
        }

        return ResolveAssetPath(AudioFolder, normalized);
    }

    public void PlaySound(string assetName)
    {
        ThrowIfDisposed();
        if (_audio == null)
        {
            throw new InvalidOperationException("A JollyAudioManager is required to play audio assets.");
        }

        string key = NormalizeAssetName(assetName);
        LoadSound(key);
        _audio.PlaySound(key);
    }

    public void PlayMusic(string assetName, bool loop = true)
    {
        ThrowIfDisposed();

        if (_audio == null)
        {
            throw new InvalidOperationException("A JollyAudioManager is required to play music assets.");
        }

        string key = NormalizeAssetName(assetName);
        LoadSound(key);

        lock (_lock)
        {
            if (_sounds.TryGetValue(key, out string? path))
            {
                _audio.PlayMusic(path, loop);
                return;
            }
        }

        string resolvedPath = ResolveAssetPath(AudioFolder, key);
        _audio.PlayMusic(resolvedPath, loop);
    }

    public void UnloadSound(string assetName)
    {
        ThrowIfDisposed();
        if (_audio == null)
        {
            return;
        }

        string key = NormalizeAssetName(assetName);
        _audio.UnloadSound(key);

        lock (_lock)
        {
            _sounds.Remove(key);
        }
    }

    public string GetTexturePath(string assetName) => ResolveAssetPath(TexturesFolder, NormalizeAssetName(assetName));
    public string GetAudioPath(string assetName) => ResolveAssetPath(AudioFolder, NormalizeAssetName(assetName));
    public string GetFontPath(string assetName) => ResolveAssetPath(FontsFolder, NormalizeAssetName(assetName));
    public string GetMapPath(string assetName) => ResolveAssetPath(MapsFolder, NormalizeAssetName(assetName));
    public string GetAnimationPath(string assetName) => ResolveAssetPath(AnimationsFolder, NormalizeAssetName(assetName));

    public bool AssetExists(string category, string assetName)
    {
        string path = ResolveAssetPath(category, NormalizeAssetName(assetName));
        return File.Exists(path);
    }

    public void Validate(params JollyAssetRequirement[] requirements)
    {
        ArgumentNullException.ThrowIfNull(requirements);

        List<string> missing = new();

        foreach (JollyAssetRequirement requirement in requirements)
        {
            ArgumentNullException.ThrowIfNull(requirement);
            string path = ResolveAssetPath(requirement.Category, NormalizeAssetName(requirement.Name));

            if (!File.Exists(path))
            {
                missing.Add($"{requirement.Category}/{requirement.Name}");
            }
        }

        if (missing.Count > 0)
        {
            throw new FileNotFoundException($"Missing Jolly2D assets:{Environment.NewLine} - {string.Join(Environment.NewLine + " - ", missing)}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            foreach (TextureEntry entry in _textures.Values)
            {
                _renderer.UnloadTexture(entry.Texture);
            }

            _textures.Clear();
            _sounds.Clear();
        }

        _disposed = true;
    }

    private string ResolveAssetPath(string category, string assetName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        string categoryPath = Path.GetFullPath(Path.Combine(RootPath, category));
        string fullPath = Path.GetFullPath(Path.Combine(categoryPath, assetName));

        if (!fullPath.StartsWith(categoryPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Asset name must remain inside its asset category folder.", nameof(assetName));
        }

        return fullPath;
    }

    private static string NormalizeAssetName(string assetName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetName);
        string normalized = assetName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

        if (normalized.Length == 0 || normalized.EndsWith(Path.DirectorySeparatorChar))
        {
            throw new ArgumentException("Asset name must identify a file.", nameof(assetName));
        }

        return normalized;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private sealed class TextureEntry
    {
        public TextureEntry(JollyTexture texture)
        {
            Texture = texture;
            ReferenceCount = 1;
        }

        public JollyTexture Texture { get; }
        public int ReferenceCount { get; set; }
    }
}

public sealed record JollyAssetRequirement(string Category, string Name);
