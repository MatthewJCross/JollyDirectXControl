# `Rendering/DirectX/JollyPhysicsDebugRenderer.cs`

Public API declarations from `Rendering/DirectX/JollyPhysicsDebugRenderer.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyPhysicsDebugRenderer
```

```csharp
public bool Enabled { get; set; } = true;
```

```csharp
public bool TileMapEnabled { get; set; } = true;
```

```csharp
public JollyPhysicsDebugRenderer(DirectXJollyRenderer renderer)
```

```csharp
public IReadOnlyList<JollySprite> Sprites => _sprites;
```

```csharp
public void Update(IReadOnlyList<JollyGameObject> objects)
```

```csharp
public void Update(IReadOnlyList<JollyGameObject> objects, JollyTileMap? tileMap, float tileMapX, float tileMapY)
```
