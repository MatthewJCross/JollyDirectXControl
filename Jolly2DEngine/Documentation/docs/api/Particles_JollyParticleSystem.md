# `Particles/JollyParticleSystem.cs`

Public API declarations from `Particles/JollyParticleSystem.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyParticleSystem
```

```csharp
public float Drag { get; internal set; }  public IReadOnlyList<JollyParticle> Particles => _particles;
```

```csharp
public IReadOnlyList<JollyParticleEmitter> Emitters => _emitters;
```

```csharp
public int ParticleCount => _particles.Count;
```

```csharp
public void AddEmitter(JollyParticleEmitter emitter)
```

```csharp
public void RemoveEmitter(JollyParticleEmitter emitter)
```

```csharp
public void Update(float deltaSeconds)
```

```csharp
public void Clear()
```

```csharp
public void Emit(JollyParticleEmitter emitter, int count)
```
