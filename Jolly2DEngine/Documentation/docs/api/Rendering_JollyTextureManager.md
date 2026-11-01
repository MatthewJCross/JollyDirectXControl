# `Rendering/JollyTextureManager.cs`

Caches loaded textures by normalized full file path.

```csharp
public sealed class JollyTextureManager
public int Count { get; }
public JollyTextureManager(JollyRenderer renderer)
public JollyTexture Load(string filePath)
public bool TryGet(string filePath, out JollyTexture? texture)
public bool Contains(string filePath)
public void Clear()
```

Path comparisons are case-insensitive. `Load` returns an existing cached texture for a path already loaded. `Clear` clears the dictionary; this class does not expose an individual unload operation or dispose textures. The source namespace is `JollyDirectXTest.Rendering`, not `Jolly2DEngine.Rendering`; treat this helper as source/project-specific until the namespace is confirmed as intentional.
