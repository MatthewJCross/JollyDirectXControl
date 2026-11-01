# `Rendering/Camera/CameraState.cs`

Public API declarations from `Rendering/Camera/CameraState.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public readonly struct CameraState
```

```csharp
public Vector2 Position { get; }  public float Zoom { get; }  public float Rotation { get; }  public Vector2 ViewportSize { get; }  public CameraState( Vector2 position, float zoom, float rotation, Vector2 viewportSize)
```

```csharp
public Matrix3x2 GetWorldToScreenMatrix()
```

```csharp
public Vector2 WorldToScreen(Vector2 worldPosition)
```

```csharp
public Vector2 ScreenToWorld(Vector2 screenPosition)
```
