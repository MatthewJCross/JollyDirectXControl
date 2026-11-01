# `Core/JollySpriteObject.cs`

Public API declarations from `Core/JollySpriteObject.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public class JollySpriteObject : JollyGameObject
```

```csharp
public JollySprite Sprite { get; } public bool IsColliding => _collisions.Count > 0;
```

```csharp
public bool IsTriggered => _triggers.Count > 0;
```

```csharp
public IReadOnlyCollection<JollyGameObject> Collisions => _collisions;
```

```csharp
public IReadOnlyCollection<JollyGameObject> Triggers => _triggers;
```

```csharp
public JollyCollisionLayer CollisionLayer => Collider?.Layer ?? JollyCollisionLayer.None;
```

```csharp
public JollyCollisionLayer CollisionMask => Collider?.Mask ?? JollyCollisionLayer.None;
```

```csharp
public JollySpriteObject(JollySprite sprite)
```

```csharp
public override void Initialize()
```

```csharp
public override void Update(JollyGameTime gameTime)
```

```csharp
public override void Draw()
```

```csharp
public override void Shutdown()
```

```csharp
public void EnableCollision(JollyCollisionMode mode = JollyCollisionMode.Bounds, byte alphaThreshold = 1)
```

```csharp
public void SetRectangleCollider(float width, float height)
```

```csharp
public void SetCircleCollider(float radius)
```

```csharp
public void SetPixelCollider(byte alphaThreshold = 1)
```

```csharp
public void SetCollisionLayers(JollyCollisionLayer layer, JollyCollisionLayer mask)
```

```csharp
public void SetTrigger(bool isTrigger = true)
```

```csharp
public void DisableCollision()
```

```csharp
public override void OnCollisionEnter(JollyGameObject other)
```

```csharp
public override void OnCollisionStay(JollyGameObject other)
```

```csharp
public override void OnCollisionExit(JollyGameObject other)
```

```csharp
public override void OnTriggerEnter(JollyGameObject other)
```

```csharp
public override void OnTriggerStay(JollyGameObject other)
```

```csharp
public override void OnTriggerExit(JollyGameObject other)
```
