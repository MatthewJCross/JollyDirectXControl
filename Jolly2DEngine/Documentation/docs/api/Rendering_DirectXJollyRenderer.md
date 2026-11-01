# `Rendering/DirectXJollyRenderer.cs`

High-level engine renderer backed by `DirectXControl` and `DirectXRenderer`.

```csharp
public sealed class DirectXJollyRenderer : JollyRenderer
public Camera2D Camera { get; }
public bool LineGlowEnabled { get; set; }
public float LineGlowWidth { get; set; }
public float LineGlowStrength { get; set; }
public float LineGlowFalloff { get; set; }
public JollyDiagnostics? Diagnostics { get; set; }
public System.Windows.Media.Color ClearColor { get; set; }
public DirectXJollyRenderer(DirectXControl control)
public override JollyTexture LoadTexture(string filePath)
public JollyTexture CreateSolidTexture(byte red, byte green, byte blue, byte alpha = 255)
public JollyTexture CreateCircleTexture(byte red, byte green, byte blue, byte alpha = 255, int size = 32)
public override JollySprite CreateSprite(JollyTexture texture)
public void SubmitFrame(JollyRenderFrame frame)
public JollyTexture CreateTextTexture(string text, string fontFamily, float fontSize, JollyColor color, bool bold = false, bool italic = false, bool ultraBold = false)
public static float MeasureTextWidth(string text, string fontFamily, float fontSize, bool bold = false, bool ultraBold = false, bool italic = false)
public JollyTexture CreateTextureFromBgraPixels(int width, int height, byte[] pixels)
public void UpdateTextureFromBgraPixels(JollyTexture texture, byte[] pixels)
public void Resize(float width, float height)
```

`CreateTextTexture` rasterises WPF formatted text. `ultraBold` selects the black font weight; `bold` selects bold when ultra-bold is false. Pixel methods require exactly `width * height * 4` bytes in BGRA order.
