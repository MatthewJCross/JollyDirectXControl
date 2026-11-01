# `Graphics/JollyTextureComposer.cs`

Builds a CPU-side BGRA canvas and uploads it as a `JollyTexture`. Coordinates are top-left pixel coordinates. Texture sources must be backed by the same DirectX renderer.

```csharp
public sealed class JollyTextureComposer
public int Width { get; }
public int Height { get; }
public JollyTextureComposer(DirectXJollyRenderer renderer, int width, int height, JollyColor? background = null)
public void Fill(JollyColor color)
public void DrawTexture(JollyTexture texture, int x, int y, float opacity = 1.0f)
public void DrawTexture(JollyTexture texture, int x, int y, int width, int height, float opacity = 1.0f)
public void DrawText(string text, string fontFamily, float fontSize, JollyColor color, int x, int y, bool bold = false, bool italic = false, bool ultraBold = false, float opacity = 1.0f)
public JollyTexture CreateTexture()
public void UpdateTexture(JollyTexture texture)
```

`opacity` must be in the inclusive range 0–1. Drawing clips to the canvas. `UpdateTexture` requires a destination with the same dimensions.
