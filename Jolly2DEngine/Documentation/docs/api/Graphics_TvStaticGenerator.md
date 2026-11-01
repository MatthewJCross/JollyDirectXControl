# `Graphics/TvStaticGenerator.cs`

Generates animated television static as a reusable DirectX texture and sprite.

```csharp
public sealed class TvStaticGenerator
public int Width { get; }
public int Height { get; }
public int FramesPerSecond { get; }
public TvStaticColourMode ColourMode { get; set; }
public JollyTexture Texture { get; }
public JollySprite Sprite { get; }
public TvStaticGenerator(DirectXJollyRenderer renderer, int width, int height, int framesPerSecond = 25, int? randomSeed = null, TvStaticColourMode colourMode = TvStaticColourMode.Monochrome)
public bool Update(float deltaSeconds)
public void AddOverlay(JollyTexture texture, int x, int y, float opacity = 1.0f)
public JollyTexture AddText(string text, string fontFamily, float fontSize, JollyColor color, int x, int y, bool bold = false, bool italic = false, bool ultraBold = false, float opacity = 1.0f)
public void ClearOverlays()
```

The frame rate is limited to 1–120 FPS. `Update` returns true only when a new frame is generated. A fixed seed supports repeatable noise. Overlays are composited on each generated frame.
