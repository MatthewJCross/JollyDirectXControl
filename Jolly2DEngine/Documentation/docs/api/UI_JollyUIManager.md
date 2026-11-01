# `UI/JollyUIManager.cs`

Public API declarations from `UI/JollyUIManager.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyUIManager
```

```csharp
public IReadOnlyList<JollyUIElement> Elements => _elements;
```

```csharp
public System.Numerics.Vector2 MousePosition { get; private set; } public JollyUIElement? HoveredElement => _hoveredElement;
```

```csharp
public JollyUIElement? PressedElement => _pressedElement; public float ScreenWidth { get; private set; }
```

```csharp
public float ScreenHeight { get; private set; }  public void SetScreenSize(float width, float height)
```

```csharp
public void Add(JollyUIElement element)
```

```csharp
public bool Remove(JollyUIElement element)
```

```csharp
public void Clear()
```

```csharp
public JollyUIElement? HitTest(Vector2 point)
```

```csharp
public void MouseMove(float x, float y)
```

```csharp
public void MouseDown()
```

```csharp
public void MouseUp()
```

```csharp
public JollyUIAnimation AnimatePosition(JollyUIElement element, Vector2 target, float duration)
```

```csharp
public JollyUIAnimation AnimateSize(JollyUIElement element, Vector2 target, float duration)
```

```csharp
public JollyUIAnimation AnimateOpacity(JollyUIElement element, float target, float duration)
```

```csharp
public void Update(float deltaSeconds)
```

```csharp
public void UpdateLayout()
```
