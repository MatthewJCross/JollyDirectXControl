# `Physics/JollyCollider.cs`

Public API declarations from `Physics/JollyCollider.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyCollider
```

```csharp
public JollySprite Sprite { get; } public bool Enabled { get; set; } = true;
```

```csharp
public bool IsTrigger { get; set; } public JollyCollisionLayer Layer { get; set; } = JollyCollisionLayer.Layer0;
```

```csharp
public JollyCollisionLayer Mask { get; set; } = JollyCollisionLayer.All;
```

```csharp
public JollyColliderShape Shape { get; set; } = JollyColliderShape.Rectangle;
```

```csharp
public float Width { get; set; } public float Height { get; set; } public float Radius { get; set; } public byte AlphaThreshold { get; set; } = 1;
```

```csharp
public JollyCollisionMode Mode
```

```csharp
public JollyCollider(JollySprite sprite)
```

```csharp
public bool CanCollideWith(JollyCollider other)
```

```csharp
public bool CollidesWith(JollyCollider other)
```

```csharp
public bool Check(JollyCollider other)
```
