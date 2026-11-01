# `Rendering/DirectX/DirectXImage.cs`

CPU-side image representation used by the DirectX texture path.

```csharp
public sealed class DirectXImage
public int Width { get; }
public int Height { get; }
public ReadOnlyMemory<byte> Pixels { get; }
public DirectXImage(int width, int height, ReadOnlyMemory<byte> pixels)
public static DirectXImage FromFile(string filePath)
public static DirectXImage FromBitmapSource(BitmapSource source)
```

Construction requires positive dimensions and exactly four bytes per pixel. The constructor's exception text describes RGBA bytes; the renderer's separate pixel-array helper explicitly accepts BGRA. Check channel ordering at conversion boundaries.
