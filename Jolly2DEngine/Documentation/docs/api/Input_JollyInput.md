# `Input/JollyInput.cs`

Public API declarations from `Input/JollyInput.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyInput
```

```csharp
public JollyKeyboard Keyboard => _keyboard;
```

```csharp
public JollyMouse Mouse => _mouse;
```

```csharp
public IReadOnlyList<JollyGamepad> Gamepads => _gamepads;
```

```csharp
public void Attach(Window window)
```

```csharp
public void Detach(Window window)
```

```csharp
public bool IsKeyDown(Key key)
```

```csharp
public bool IsKeyPressed(Key key)
```

```csharp
public bool IsKeyReleased(Key key)
```

```csharp
public double MouseX => _mouse.X;
```

```csharp
public double MouseY => _mouse.Y;
```

```csharp
public System.Windows.Point MousePosition => _mouse.Position;
```

```csharp
public int MouseWheel => _mouse.Wheel;
```

```csharp
public bool IsMouseButtonDown(JollyMouseButton button)
```

```csharp
public bool IsMouseButtonPressed(JollyMouseButton button)
```

```csharp
public bool IsMouseButtonReleased(JollyMouseButton button)
```

```csharp
public Vector2 GetMouseWorldPosition(Camera2D camera)
```
