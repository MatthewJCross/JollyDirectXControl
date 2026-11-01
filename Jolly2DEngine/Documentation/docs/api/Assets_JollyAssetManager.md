# `Assets/JollyAssetManager.cs`

Public API declarations from `Assets/JollyAssetManager.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyAssetManager : IDisposable
```

```csharp
public const string TexturesFolder = "Textures";
```

```csharp
public const string AudioFolder = "Audio";
```

```csharp
public const string FontsFolder = "Fonts";
```

```csharp
public const string MapsFolder = "Maps";
```

```csharp
public const string AnimationsFolder = "Animations";
```

```csharp
public JollyAssetManager(DirectXJollyRenderer renderer, JollyAudioManager? audio = null, string? rootPath = null)
```

```csharp
public string RootPath { get; }  public string TexturesPath => Path.Combine(RootPath, TexturesFolder);
```

```csharp
public string AudioPath => Path.Combine(RootPath, AudioFolder);
```

```csharp
public string FontsPath => Path.Combine(RootPath, FontsFolder);
```

```csharp
public string MapsPath => Path.Combine(RootPath, MapsFolder);
```

```csharp
public string AnimationsPath => Path.Combine(RootPath, AnimationsFolder);
```

```csharp
public JollyTexture LoadTexture(string assetName)
```

```csharp
public async Task<JollyTexture> LoadTextureAsync(string assetName, CancellationToken cancellationToken = default)
```

```csharp
public JollyAssetHandle<JollyTexture> AcquireTexture(string assetName)
```

```csharp
public void ReleaseTexture(string assetName)
```

```csharp
public bool IsTextureLoaded(string assetName)
```

```csharp
public void LoadSound(string assetName)
```

```csharp
public void PlaySound(string assetName)
```

```csharp
public void PlayMusic(string assetName, bool loop = true)
```

```csharp
public void UnloadSound(string assetName)
```

```csharp
public string GetTexturePath(string assetName) => ResolveAssetPath(TexturesFolder, NormalizeAssetName(assetName));
```

```csharp
public string GetAudioPath(string assetName) => ResolveAssetPath(AudioFolder, NormalizeAssetName(assetName));
```

```csharp
public string GetFontPath(string assetName) => ResolveAssetPath(FontsFolder, NormalizeAssetName(assetName));
```

```csharp
public string GetMapPath(string assetName) => ResolveAssetPath(MapsFolder, NormalizeAssetName(assetName));
```

```csharp
public string GetAnimationPath(string assetName) => ResolveAssetPath(AnimationsFolder, NormalizeAssetName(assetName));
```

```csharp
public bool AssetExists(string category, string assetName)
```

```csharp
public void Validate(params JollyAssetRequirement[] requirements)
```

```csharp
public void Dispose()
```

```csharp
public TextureEntry(JollyTexture texture)
```

```csharp
public JollyTexture Texture { get; } public int ReferenceCount { get; set; } } }  public sealed record JollyAssetRequirement(string Category, string Name);
```
