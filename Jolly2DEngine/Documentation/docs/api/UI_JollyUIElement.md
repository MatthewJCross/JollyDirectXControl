# `UI/JollyUIElement.cs`

Public API declarations from `UI/JollyUIElement.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public class JollyUIElement
```

```csharp
public JollyUIElement? Parent { get; private set; } public IReadOnlyList<JollyUIElement> Children => _children;
```

```csharp
public Vector2 Position { get; set; } public Vector2 Size { get; set; } public bool Visible { get; set; } = true;
```

```csharp
public bool Enabled { get; set; } = true;
```

```csharp
public int Layer { get; set; } public float Opacity { get; set; } = 1.0f;
```

```csharp
public JollyUIAnchor Anchor { get; set; } = JollyUIAnchor.TopLeft;
```

```csharp
public float MarginLeft { get; set; } public float MarginTop { get; set; } public float MarginRight { get; set; } public float MarginBottom { get; set; }  public float X
```

```csharp
public float Y
```

```csharp
public float Width
```

```csharp
public float Height
```

```csharp
public float AbsoluteX
```

```csharp
public float AbsoluteY
```

```csharp
public Vector2 AbsolutePosition
```

```csharp
public bool ContainsPoint(Vector2 point)
```

```csharp
public void AddChild(JollyUIElement child)
```

```csharp
public bool RemoveChild(JollyUIElement child)
```

```csharp
public void RemoveAllChildren()
```

```csharp
public void SetMargin(float left, float top, float right, float bottom)
```
