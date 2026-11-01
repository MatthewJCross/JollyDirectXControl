# `World/JollyTileMap.cs`

Public API declarations from `World/JollyTileMap.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyTileMap
```

```csharp
public event Action<JollyGameObject, int, int, int>? HazardContact;
```

```csharp
public JollyTexture? Texture { get; private set; } public int TextureColumns { get; private set; } public int TextureRows { get; private set; } public int TextureTileCount => TextureColumns * TextureRows;
```

```csharp
public int Width { get; } public int Height { get; } public float TileWidth { get; } public float TileHeight { get; }  public JollyTileMap(int width, int height, float tileWidth, float tileHeight)
```

```csharp
public void NotifyHazardContact(JollyGameObject gameObject, int tileX, int tileY, int tileIndex)
```

```csharp
public bool IsSolid(int x, int y)
```

```csharp
public void SetSolid(int x, int y, bool solid)
```

```csharp
public int GetTile(int x, int y)
```

```csharp
public void SetTile(int x, int y, int tileIndex)
```

```csharp
public Vector2 GetTilePosition(int x, int y)
```

```csharp
public Vector2 WorldToTile(float x, float y)
```

```csharp
public bool IsSolidAtWorldPosition(float x, float y)
```

```csharp
public bool TryGetTileAtWorldPosition(float x, float y, out int tileIndex)
```

```csharp
public void SetTexture(JollyTexture texture)
```

```csharp
public bool TryGetTileSourceRectangle(int tileIndex, out int x, out int y, out int width, out int height)
```

```csharp
public void SetAnimatedTile(int tileIndex, int startFrame, int frameCount, float framesPerSecond, bool loop = true)
```

```csharp
public void RemoveAnimatedTile(int tileIndex)
```

```csharp
public bool IsAnimatedTile(int tileIndex)
```

```csharp
public void SetConveyorTile(int tileIndex, float speed)
```

```csharp
public void RemoveConveyorTile(int tileIndex)
```

```csharp
public bool IsConveyorTile(int tileIndex)
```

```csharp
public float GetConveyorSpeed(int tileIndex)
```

```csharp
public int GetAnimatedTileIndex(int tileIndex, float elapsedSeconds)
```

```csharp
public void SetHazardTile(int tileIndex, bool hazard)
```

```csharp
public bool IsHazardTile(int tileIndex)
```

```csharp
public int StartFrame { get; } public int FrameCount { get; } public float FramesPerSecond { get; } public bool Loop { get; }  public JollyTileAnimation(int startFrame, int frameCount, float framesPerSecond, bool loop)
```
