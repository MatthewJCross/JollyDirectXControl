# `Graphics/SpriteFontRenderer.cs`

Creates text from fixed-size character cells in a sprite-font texture atlas.

```csharp
public sealed class SpriteFontRenderer
public float CharacterSpacing { get; set; }
public float LineSpacing { get; set; } = 16.0f;
public float Scale { get; set; } = 1.0f;
public IEnumerable<JollySprite> Sprites { get; }
public SpriteFontRenderer(JollyRenderer renderer, JollyTexture texture, int columns, int characterWidth = 14, int characterHeight = 16, int firstCharacter = 32)
public void Clear()
public void DrawText(string text, float x, float y)
```

Each supported character creates a sprite. Newlines advance by `LineSpacing`, carriage returns are ignored, and unsupported glyphs use `?` when present in the atlas. `Sprites` enumerates all generated character sprites until `Clear` is called.

The source file imports `Jolly2DEngine.Core.Graphics`; verify that namespace in the consuming build if this class is used independently.
