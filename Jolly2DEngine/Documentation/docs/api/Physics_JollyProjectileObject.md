# `Physics/JollyProjectileObject.cs`

Public API declarations from `Physics/JollyProjectileObject.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyProjectileObject : JollySpriteObject
```

```csharp
public float Lifetime { get; set; } = 5.0f;
```

```csharp
public int Damage { get; set; } = 1;
```

```csharp
public bool DestroyOnCollision { get; set; } = true;
```

```csharp
public bool IsExpired { get; private set; }  public JollyProjectileObject(JollySprite sprite) : base(sprite)
```

```csharp
public override void Initialize()
```

```csharp
public override void Update(JollyGameTime gameTime)
```

```csharp
public override void OnCollisionEnter(JollyGameObject other)
```
