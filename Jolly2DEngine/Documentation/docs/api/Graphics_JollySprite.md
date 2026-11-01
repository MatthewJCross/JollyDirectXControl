# `Graphics/JollySprite.cs`

Public API declarations from `Graphics/JollySprite.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollySprite
```

```csharp
public event Action<JollySprite, string>? AnimationCompleted;
```

```csharp
public JollyCollisionMode CollisionMode { get; set; } = JollyCollisionMode.None;
```

```csharp
public byte CollisionAlphaThreshold { get; set; } = 1;
```

```csharp
public string Name { get; set; } = string.Empty;
```

```csharp
public JollyTexture Texture { get; }  public JollyVector2 Position
```

```csharp
public JollyVector2 Size
```

```csharp
public JollyVector2 Scale
```

```csharp
public float X
```

```csharp
public float Y
```

```csharp
public float Width
```

```csharp
public float Height
```

```csharp
public float Rotation
```

```csharp
public float Opacity
```

```csharp
public JollyColor Color { get; set; } = JollyColor.White;
```

```csharp
public bool Visible
```

```csharp
public int Layer
```

```csharp
public bool FlipHorizontal
```

```csharp
public bool FlipVertical
```

```csharp
public float SourceX
```

```csharp
public float SourceY
```

```csharp
public float SourceWidth
```

```csharp
public float SourceHeight
```

```csharp
public bool AnimationPlaying => _animationPlaying;
```

```csharp
public int AnimationFrame => _animationFrame;
```

```csharp
public float AnimationFPS => _animationFPS;
```

```csharp
public int AnimationFrameCount => _currentAnimation?.FrameCount ?? _animationSheet?.FrameCount ?? 0;
```

```csharp
public bool AnimationLoop
```

```csharp
public JollySpriteSheet? AnimationSheet => _animationSheet;
```

```csharp
public string? CurrentAnimation => _currentAnimation?.Name;
```

```csharp
public bool IsAnimationComplete
```

```csharp
public IReadOnlyDictionary<string, JollyAnimation> Animations => _animations;
```

```csharp
public void AddAnimation(JollyAnimation animation)
```

```csharp
public bool RemoveAnimation(string name)
```

```csharp
public bool HasAnimation(string name)
```

```csharp
public JollyAnimation? GetAnimation(string name)
```

```csharp
public void Play(string animationName)
```

```csharp
public void SetAnimation(JollySpriteSheet sheet, float framesPerSecond, bool loop = true)
```

```csharp
public void Play()
```

```csharp
public void Pause()
```

```csharp
public void Stop()
```

```csharp
public void SetAnimationFrame(int frame)
```

```csharp
public void SetFrame(JollySpriteSheetFrame frame, JollySpriteSheet sheet)
```
