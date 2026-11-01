# `World/JollyTileMapRenderer.cs`

Public API declarations from `World/JollyTileMapRenderer.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyTileMapRenderer
```

```csharp
public IReadOnlyList<JollySprite> Sprites => _sprites;
```

```csharp
public int Layer { get; set; } public float X { get; set; } public float Y { get; set; }  public JollyTileMapRenderer(JollyTileMap tileMap, Func<JollyTexture, JollySprite> spriteFactory)
```

```csharp
public void Rebuild()
```

```csharp
public void Update(Camera2D camera, float deltaSeconds)
```
