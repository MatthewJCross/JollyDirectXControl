# `Physics/JollyCharacterController.cs`

Public API declarations from `Physics/JollyCharacterController.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyCharacterController
```

```csharp
public JollyGameObject GameObject => _gameObject;
```

```csharp
public float MoveAcceleration { get; set; } = 2200.0f;
```

```csharp
public float MaxMoveSpeed { get; set; } = 300.0f;
```

```csharp
public float GroundDeceleration { get; set; } = 2600.0f;
```

```csharp
public float AirDeceleration { get; set; } = 800.0f;
```

```csharp
public float JumpVelocity { get; set; } = 450.0f;
```

```csharp
public float JumpGravity { get; set; } = 980.0f;
```

```csharp
public float FallGravity { get; set; } = 1400.0f;
```

```csharp
public float JumpCutGravity { get; set; } = 1800.0f;
```

```csharp
public float MaxFallSpeed { get; set; } = 900.0f;
```

```csharp
public float CoyoteTime { get; set; } = 0.10f;
```

```csharp
public float JumpBufferTime { get; set; } = 0.10f;
```

```csharp
public bool JumpHeld { get; private set; }  public bool IsGrounded => _gameObject.IsGrounded;
```

```csharp
public JollyCharacterController(JollyGameObject gameObject)
```

```csharp
public void Reset()
```

```csharp
public void Move(float direction)
```

```csharp
public void Stop()
```

```csharp
public void SetJumpHeld(bool held)
```

```csharp
public void RequestJump()
```

```csharp
public void Update(JollyGameTime gameTime)
```
