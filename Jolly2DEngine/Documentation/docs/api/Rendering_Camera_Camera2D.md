# `Rendering/Camera/Camera2D.cs`

Public API declarations from `Rendering/Camera/Camera2D.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class Camera2D
```

```csharp
public Vector2 Position
```

```csharp
public float X
```

```csharp
public float Y
```

```csharp
public float Zoom
```

```csharp
public float Rotation
```

```csharp
public float FollowSpeed
```

```csharp
public Vector2 ViewportSize { get; private set; } public bool BoundsEnabled => _boundsEnabled;
```

```csharp
public float BoundsLeft => _boundsLeft;
```

```csharp
public float BoundsTop => _boundsTop;
```

```csharp
public float BoundsRight => _boundsRight;
```

```csharp
public float BoundsBottom => _boundsBottom;
```

```csharp
public bool IsShaking => _shakeTime > 0.0f;
```

```csharp
public Vector2 ShakeOffset => _shakeOffset;
```

```csharp
public void SetViewport(float width, float height)
```

```csharp
public void SetBounds(float left, float top, float right, float bottom)
```

```csharp
public void ClearBounds()
```

```csharp
public void Follow(Vector2 targetPosition)
```

```csharp
public void FollowSmooth(Vector2 targetPosition, float deltaSeconds)
```

```csharp
public Vector2 WorldToScreen(Vector2 worldPosition)
```

```csharp
public Vector2 ScreenToWorld(Vector2 screenPosition)
```

```csharp
public Matrix3x2 GetWorldToScreenMatrix()
```

```csharp
public CameraState CaptureState()
```

```csharp
public void Shake(float intensity, float duration)
```

```csharp
public void UpdateShake(float deltaSeconds)
```
