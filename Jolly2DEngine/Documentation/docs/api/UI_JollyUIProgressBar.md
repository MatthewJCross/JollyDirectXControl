# `UI/JollyUIProgressBar.cs`

Public API declarations from `UI/JollyUIProgressBar.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public enum JollyUIProgressBarOrientation
```

```csharp
public sealed class JollyUIProgressBar : JollyUIElement
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
public JollyUIProgressBarOrientation Orientation { get; set; } = JollyUIProgressBarOrientation.Horizontal;
```

```csharp
public JollyColor BackgroundColor
```

```csharp
public JollyColor FillColor
```

```csharp
public JollyTexture? BackgroundTexture { get; set; }  public JollyTexture? FillTexture { get; set; }  private bool _showText;
```

```csharp
public bool ShowText
```

```csharp
public string Text
```

```csharp
public JollyColor TextColor
```

```csharp
public float FontSize
```

```csharp
public bool Bold
```

```csharp
public bool TextDirty { get; internal set; } = true;
```
