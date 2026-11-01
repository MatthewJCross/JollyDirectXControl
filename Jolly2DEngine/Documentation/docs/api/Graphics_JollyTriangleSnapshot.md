# `Graphics/JollyTriangleSnapshot.cs`

Immutable flat-colour triangle submission value.

```csharp
public readonly struct JollyTriangleSnapshot
public Vector2 A { get; }
public Vector2 B { get; }
public Vector2 C { get; }
public JollyColor Colour { get; }
public JollyTriangleSnapshot(Vector2 a, Vector2 b, Vector2 c, JollyColor colour)
```

Triangles are passed to `JollyRenderFrame` and rendered by the DirectX triangle-list pipeline. The snapshot does not contain per-vertex colours or texture coordinates.
