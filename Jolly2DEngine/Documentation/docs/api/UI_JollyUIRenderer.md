# `UI/JollyUIRenderer.cs`

Public API declarations from `UI/JollyUIRenderer.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyUIRenderer
```

```csharp
public IReadOnlyList<JollySprite> Sprites => _sprites;
```

```csharp
public JollyUIRenderer(JollyUIManager uiManager, Func<JollyTexture, JollySprite> spriteFactory, Func<byte, byte, byte, byte, JollyTexture> textureFactory, Func<string, string, float, JollyColor, bool, bool, JollyTexture> textTextureFactory)
```

```csharp
public void Update()
```

```csharp
public void Clear()
```
