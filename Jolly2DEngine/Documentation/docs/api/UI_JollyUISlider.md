# `UI/JollyUISlider.cs`

Public API declarations from `UI/JollyUISlider.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public enum JollyUISliderOrientation
```

```csharp
public sealed class JollyUISlider : JollyUIElement
```

```csharp
public JollyUISliderOrientation Orientation { get; set; } = JollyUISliderOrientation.Horizontal;
```

```csharp
public float Minimum
```

```csharp
public float Maximum
```

```csharp
public float Value
```

```csharp
public float Percentage
```

```csharp
public JollyColor TrackColor { get; set; } = JollyColor.FromBytes(50, 50, 50, 255);
```

```csharp
public JollyColor FillColor { get; set; } = JollyColor.FromBytes(80, 160, 240, 255);
```

```csharp
public JollyColor HandleColor { get; set; } = JollyColor.White;
```

```csharp
public float TrackThickness { get; set; } = 8.0f;
```

```csharp
public float HandleSize { get; set; } = 20.0f;
```

```csharp
public JollyTexture? TrackTexture { get; set; }  public JollyTexture? FillTexture { get; set; }  public JollyTexture? HandleTexture { get; set; }  public bool IsDragging
```

```csharp
public event Action<JollyUISlider, float>? ValueChanged;
```
