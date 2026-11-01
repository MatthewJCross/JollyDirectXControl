# `Particles/JollyParticleEmitter.cs`

Public API declarations from `Particles/JollyParticleEmitter.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyParticleEmitter
```

```csharp
public Vector2 Position { get; set; } public bool Enabled { get; set; } = true;
```

```csharp
public float ParticlesPerSecond { get; set; } = 20.0f;
```

```csharp
public float MinimumLife { get; set; } = 0.5f;
```

```csharp
public float MaximumLife { get; set; } = 1.0f;
```

```csharp
public float MinimumSize { get; set; } = 4.0f;
```

```csharp
public float MaximumSize { get; set; } = 8.0f;
```

```csharp
public float MinimumSpeed { get; set; } = 20.0f;
```

```csharp
public float MaximumSpeed { get; set; } = 60.0f;
```

```csharp
public float Direction { get; set; } public float Spread { get; set; } = MathF.PI * 2.0f;
```

```csharp
public Vector2 Gravity { get; set; } public float Drag { get; set; } public float MinimumRotationSpeed { get; set; } public float MaximumRotationSpeed { get; set; } public float Opacity { get; set; } = 1.0f;
```

```csharp
public JollyColor Color { get; internal set; } = JollyColor.White;
```

```csharp
public int Layer { get; set; } public JollyParticleEmitterShape Shape { get; set; } = JollyParticleEmitterShape.Point;
```

```csharp
public float ShapeWidth { get; set; } = 32.0f;
```

```csharp
public float ShapeHeight { get; set; } = 32.0f;
```

```csharp
public float ShapeRadius { get; set; } = 16.0f;
```

```csharp
public int Update(float deltaSeconds, List<JollyParticle> particles)
```

```csharp
public void Emit(int count, List<JollyParticle> particles)
```

```csharp
public void Reset()
```

```csharp
public void Burst(int count, List<JollyParticle> particles)
```

```csharp
public void ApplyPreset(JollyParticlePreset preset)
```
