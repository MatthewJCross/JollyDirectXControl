# `Scene/JollyScene.cs`

Public API declarations from `Scene/JollyScene.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public abstract class JollyScene
```

```csharp
public IReadOnlyList<JollyGameObject> Objects => _objects;
```

```csharp
public int EntityCount => _objects.Count;
```

```csharp
public string Name { get; set; } = string.Empty;
```

```csharp
public bool IsInitialized => _initialized;
```

```csharp
public JollyTileMap? TileMap => _tileMap;
```

```csharp
public void SetTileMap(JollyTileMap tileMap, float x, float y)
```

```csharp
public void ClearTileMap()
```

```csharp
public virtual void OnEnter()
```

```csharp
public virtual void OnExit()
```

```csharp
public virtual void Initialize()
```

```csharp
public void Add(JollyGameObject gameObject)
```

```csharp
public void Spawn(JollyGameObject gameObject)
```

```csharp
public void Remove(JollyGameObject gameObject)
```

```csharp
public void Despawn(JollyGameObject gameObject)
```

```csharp
public virtual void Update(JollyGameTime gameTime)
```

```csharp
public virtual void Draw()
```

```csharp
public IReadOnlyList<JollySprite> GetSprites()
```

```csharp
public JollyGameObject? FindBySaveId(string saveId)
```

```csharp
public JollyGameObject? Find(string name)
```

```csharp
public T? Find<T>(string name) where T : JollyGameObject
```

```csharp
public IReadOnlyList<T> FindAll<T>() where T : JollyGameObject
```

```csharp
public bool Contains(JollyGameObject gameObject)
```

```csharp
public virtual void Shutdown()
```

```csharp
public static void ResolveTileMap(JollyGameObject gameObject, JollyTileMap tileMap, float tileMapX, float tileMapY, float deltaTime)
```

```csharp
public JollyGameObject ObjectA { get; }  public JollyGameObject ObjectB { get; }  public CollisionPair(JollyGameObject objectA, JollyGameObject objectB)
```

```csharp
public bool Equals(CollisionPair other)
```

```csharp
public override bool Equals(object? obj)
```

```csharp
public override int GetHashCode()
```
