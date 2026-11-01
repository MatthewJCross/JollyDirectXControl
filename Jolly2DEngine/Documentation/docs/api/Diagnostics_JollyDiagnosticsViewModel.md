# `Diagnostics/JollyDiagnosticsViewModel.cs`

Public API declarations from `Diagnostics/JollyDiagnosticsViewModel.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyDiagnosticsViewModel : INotifyPropertyChanged
```

```csharp
public event PropertyChangedEventHandler? PropertyChanged;
```

```csharp
public double FPS
```

```csharp
public double FrameTimeMilliseconds
```

```csharp
public double UpdateTimeMilliseconds
```

```csharp
public double RenderTimeMilliseconds
```

```csharp
public long FrameCount
```

```csharp
public long UpdateCount
```

```csharp
public long RenderCount
```

```csharp
public int EntityCount
```

```csharp
public int SpriteCount
```

```csharp
public int TextureCount
```

```csharp
public int RenderBatchCount
```

```csharp
public int RenderDrawCallCount
```

```csharp
public int RenderMaxBatchSize
```

```csharp
public int ConnectedClients
```

```csharp
public long NetworkMessagesSent
```

```csharp
public long NetworkMessagesReceived
```

```csharp
public long NetworkBytesSent
```

```csharp
public long NetworkBytesReceived
```

```csharp
public double NetworkLatencyMilliseconds
```

```csharp
public IReadOnlyDictionary<string, JollyProfileSnapshot> Profiles
```

```csharp
public void Refresh(JollyDiagnostics diagnostics)
```

```csharp
public string GetSummary()
```

```csharp
public string GetRenderSummary()
```

```csharp
public JollyProfileSnapshot? GetProfile(string name)
```

```csharp
public long GetProfileCallCount(string name)
```
