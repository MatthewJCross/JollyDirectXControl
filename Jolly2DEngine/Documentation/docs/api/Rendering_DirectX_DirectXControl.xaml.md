# `Rendering/DirectX/DirectXControl.xaml.cs`

Public API declarations from `Rendering/DirectX/DirectXControl.xaml.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public partial class DirectXControl : System.Windows.Controls.UserControl//, System.ComponentModel.INotifyPropertyChanged
```

```csharp
public event Action<double, double>? MouseMoved;
```

```csharp
public event Action? MouseButtonDown;
```

```csharp
public event Action? MouseButtonUp;
```

```csharp
public event Action<float, float>? RenderSizeChanged;
```

```csharp
public bool LineGlowEnabled
```

```csharp
public float LineGlowWidth
```

```csharp
public float LineGlowStrength
```

```csharp
public float LineGlowFalloff
```

```csharp
public JollyDiagnostics? Diagnostics { get; set; }  public DirectXControl()
```

```csharp
public DirectXStretchMode StretchMode
```

```csharp
public DirectXImageFilter ImageFilter
```

```csharp
public double MaxFPS
```

```csharp
public bool VSync
```

```csharp
public bool ClearEveryFrame
```

```csharp
public System.Windows.Media.Color ClearColor
```

```csharp
public static readonly System.Windows.DependencyProperty FPSProperty = System.Windows.DependencyProperty.Register(nameof(FPS), typeof(double), typeof(DirectXControl), new System.Windows.PropertyMetadata(0.0));
```

```csharp
public double FPS
```

```csharp
public static readonly System.Windows.DependencyProperty FrameTimeProperty = System.Windows.DependencyProperty.Register(nameof(FrameTime), typeof(double), typeof(DirectXControl), new System.Windows.PropertyMetadata(0.0));
```

```csharp
public double FrameTime
```

```csharp
public bool IsDirectXReady => _renderer != null && _renderer.IsInitialized;
```

```csharp
public event EventHandler<System.Windows.Point>? MouseScreenPositionChanged;
```

```csharp
public async Task WaitForReadyAsync()
```

```csharp
public void LoadImage(string filePath)
```

```csharp
public void SetImage(BitmapSource source)
```

```csharp
public void SetImage(byte[] pixels, int width, int height)
```

```csharp
public void SetImage(DirectXImage image)
```

```csharp
public void ClearScreen()
```

```csharp
public void Clear()
```

```csharp
public DirectXTexture CreateTexture(string filePath)
```

```csharp
public DirectXTexture CreateTexture(DirectXImage image)
```

```csharp
public DirectXJollyRenderer CreateJollyRenderer()
```
