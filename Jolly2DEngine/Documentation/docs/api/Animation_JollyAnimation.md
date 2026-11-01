# `Animation/JollyAnimation.cs`

Public API declarations from `Animation/JollyAnimation.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyAnimation
```

```csharp
public string Name { get; }  public JollySpriteSheet Sheet { get; }  public int StartFrame { get; }  public int FrameCount { get; }  public double FPS { get; }  public bool Loop { get; }  public JollyAnimation(string name, JollySpriteSheet sheet, int startFrame, int frameCount, double fps, bool loop = true)
```

```csharp
public JollySpriteSheetFrame GetFrame(int animationFrame)
```
