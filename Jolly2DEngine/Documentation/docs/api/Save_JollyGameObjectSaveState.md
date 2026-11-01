# `Save/JollyGameObjectSaveState.cs`

Public API declarations from `Save/JollyGameObjectSaveState.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyGameObjectSaveState
```

```csharp
public string Id { get; set; } = string.Empty;
```

```csharp
public string Name { get; set; } = string.Empty;
```

```csharp
public string Tag { get; set; } = string.Empty;
```

```csharp
public bool Active { get; set; } public JollyVector2 Position { get; set; } public JollyVector2 Scale { get; set; } public float Rotation { get; set; } public bool Visible { get; set; } public JollyVector2 Velocity { get; set; } public JollyVector2 Acceleration { get; set; } public JollyVector2 Gravity { get; set; } public float Drag { get; set; } public bool UsePhysics { get; set; } public bool UseGravity { get; set; } public bool IsGrounded { get; set; } public bool WasGrounded { get; set; } public float GroundFriction { get; set; } public float AirFriction { get; set; } public float Restitution { get; set; } public JollyColliderSaveState? Collider { get; set; }  public static JollyGameObjectSaveState Capture(JollyGameObject gameObject)
```

```csharp
public void ApplyTo(JollyGameObject gameObject)
```

```csharp
public sealed class JollyColliderSaveState
```

```csharp
public bool Enabled { get; set; } public bool IsTrigger { get; set; } public JollyCollisionLayer Layer { get; set; } public JollyCollisionLayer Mask { get; set; } public JollyColliderShape Shape { get; set; } public float Width { get; set; } public float Height { get; set; } public float Radius { get; set; } public byte AlphaThreshold { get; set; }  public static JollyColliderSaveState Capture(JollyCollider collider)
```

```csharp
public void ApplyTo(JollyCollider collider)
```
