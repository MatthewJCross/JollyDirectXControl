# `Particles/JollyParticleRenderer.cs`

Public API declarations from `Particles/JollyParticleRenderer.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyParticleRenderer
```

```csharp
public IReadOnlyList<JollySprite> Sprites => _sprites;
```

```csharp
public JollyParticleRenderer(JollyParticleSystem particleSystem, Func<JollyTexture, JollySprite> spriteFactory, Func<JollyColor, JollyTexture> textureFactory)
```

```csharp
public void Update(JollyTexture texture)
```

```csharp
public void Clear()
```
