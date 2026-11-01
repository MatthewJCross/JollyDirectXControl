# `UI/JollyUIButton.cs`

Public API declarations from `UI/JollyUIButton.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyUIButton : JollyUIElement
```

```csharp
public string Text { get; set; } = "Button";
```

```csharp
public JollyColor NormalColor { get; set; } = JollyColor.FromBytes(60, 60, 60, 255);
```

```csharp
public JollyColor HoverColor { get; set; } = JollyColor.FromBytes(80, 80, 80, 255);
```

```csharp
public JollyColor PressedColor { get; set; } = JollyColor.FromBytes(40, 40, 40, 255);
```

```csharp
public JollyColor DisabledColor { get; set; } = JollyColor.FromBytes(35, 35, 35, 255);
```

```csharp
public JollyColor TextColor { get; set; } = JollyColor.White;
```

```csharp
public float FontSize { get; set; } = 18.0f;
```

```csharp
public bool Bold { get; set; } public bool IsHovered { get; internal set; } public bool IsPressed { get; internal set; }  public event Action<JollyUIButton>? Click;
```

```csharp
public JollyColor CurrentColor
```

```csharp
public JollyTexture? NormalTexture { get; set; } public JollyTexture? HoverTexture { get; set; } public JollyTexture? PressedTexture { get; set; } public JollyTexture? DisabledTexture { get; set; } public JollyTexture? CurrentTexture
```

```csharp
public JollyUI9Slice? NormalNineSlice { get; set; } public JollyUI9Slice? HoverNineSlice { get; set; } public JollyUI9Slice? PressedNineSlice { get; set; } public JollyUI9Slice? DisabledNineSlice { get; set; }  public JollyUI9Slice? CurrentNineSlice
```
