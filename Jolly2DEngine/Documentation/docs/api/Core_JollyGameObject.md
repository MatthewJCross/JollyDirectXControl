# `Core/JollyGameObject.cs`

Public API declarations from `Core/JollyGameObject.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public abstract class JollyGameObject
```

```csharp
public string SaveId { get; set; } = Guid.NewGuid().ToString("N");
```

```csharp
public string Name { get; set; } = string.Empty;
```

```csharp
public string Tag { get; set; } = string.Empty;
```

```csharp
public bool Active { get; set; } = true;
```

```csharp
public JollyVector2 Position { get; set; } public JollyVector2 Scale { get; set; } = JollyVector2.One;
```

```csharp
public float Rotation { get; set; } public bool Visible { get; set; } = true;
```

```csharp
public JollyVector2 Velocity { get; set; } public JollyVector2 Acceleration { get; set; } public JollyVector2 Gravity { get; set; } public float Drag { get; set; } public bool UsePhysics { get; set; } public bool UseGravity { get; set; } public bool IsGrounded { get; set; } public bool WasGrounded { get; set; } public float GroundFriction { get; set; } = 12.0f;
```

```csharp
public float AirFriction { get; set; } = 0.0f;
```

```csharp
public float Restitution { get; set; } = 0.0f;
```

```csharp
public JollyCollider? Collider { get; private set; } private JollyCharacterController? _characterController;
```

```csharp
public JollyCharacterController? CharacterController => _characterController;
```

```csharp
public bool IsDestroyed { get; private set; } internal JollyScene? Scene { get; set; }  public virtual void Initialize()
```

```csharp
public virtual void Update(JollyGameTime gameTime)
```

```csharp
public virtual void Draw()
```

```csharp
public virtual void Shutdown()
```

```csharp
public void SetCollider(JollyCollider collider)
```

```csharp
public void RemoveCollider()
```

```csharp
public virtual void OnCollisionEnter(JollyGameObject other)
```

```csharp
public virtual void OnCollisionStay(JollyGameObject other)
```

```csharp
public virtual void OnCollisionExit(JollyGameObject other)
```

```csharp
public virtual void OnTriggerEnter(JollyGameObject other)
```

```csharp
public virtual void OnTriggerStay(JollyGameObject other)
```

```csharp
public virtual void OnTriggerExit(JollyGameObject other)
```

```csharp
public JollyCharacterController AddCharacterController()
```

```csharp
public void Destroy()
```
