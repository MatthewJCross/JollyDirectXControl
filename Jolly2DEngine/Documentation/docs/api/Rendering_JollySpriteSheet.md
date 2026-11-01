# `Rendering/JollySpriteSheet.cs`

Describes a regular grid of fixed-size frames within a `JollyTexture`.

```csharp
public enum JollySpriteSheetOrder { Horizontal, Vertical }
public readonly struct JollySpriteSheetFrame
public int Index { get; }
public int Column { get; }
public int Row { get; }
public int X { get; }
public int Y { get; }
public int Width { get; }
public int Height { get; }
public sealed class JollySpriteSheet
public JollyTexture Texture { get; }
public int FrameWidth { get; }
public int FrameHeight { get; }
public int Columns { get; }
public int Rows { get; }
public int FrameCount { get; }
public JollySpriteSheetOrder Order { get; }
public JollySpriteSheet(JollyTexture texture, int frameWidth, int frameHeight, JollySpriteSheetOrder order = JollySpriteSheetOrder.Horizontal)
public JollySpriteSheetFrame GetFrame(int index)
```

Horizontal order advances across columns before moving down rows. Vertical order advances down rows before moving to the next column. Texture dimensions are divided by frame dimensions; any remainder at the right/bottom edge is not included in the frame grid.
