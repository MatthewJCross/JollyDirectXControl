# `Graphics/JollyRenderFrame.cs`

Public API declarations from `Graphics/JollyRenderFrame.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyRenderFrame
```

```csharp
public IReadOnlyList<JollySpriteSnapshot> Sprites => _sprites;
```

```csharp
public IReadOnlyList<JollyLineSnapshot> Lines => _lines;
```

```csharp
public JollyRenderFrame(IEnumerable<JollySprite> sprites, IEnumerable<JollyLineSnapshot>? lines = null)
```
