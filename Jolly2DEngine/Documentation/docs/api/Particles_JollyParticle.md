# `Particles/JollyParticle.cs`

Public API declarations from `Particles/JollyParticle.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyParticle
```

```csharp
public System.Numerics.Vector2 Position { get; internal set; } public System.Numerics.Vector2 Velocity { get; internal set; } public System.Numerics.Vector2 Acceleration { get; internal set; } public float Drag { get; internal set; } public float Life { get; internal set; } public float MaximumLife { get; internal set; } public float Size { get; internal set; } public float StartSize { get; internal set; } public float EndSize { get; internal set; } public float Rotation { get; internal set; } public float RotationSpeed { get; internal set; } public float Opacity { get; internal set; } public JollyColor Color { get; internal set; } = JollyColor.White;
```

```csharp
public int Layer { get; internal set; } public bool IsAlive => Life > 0.0f;
```

```csharp
public float NormalizedLife
```

```csharp
public float Age
```
