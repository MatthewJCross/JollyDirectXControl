# `Diagnostics/JollyDiagnostics.cs`

Public API declarations from `Diagnostics/JollyDiagnostics.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyDiagnostics
```

```csharp
public void BeginFrame()
```

```csharp
public void EndFrame()
```

```csharp
public JollyDiagnosticScope MeasureUpdate()
```

```csharp
public JollyDiagnosticScope MeasureRender()
```

```csharp
public void SetEntityCount(int count)
```

```csharp
public void SetSpriteCount(int count)
```

```csharp
public void SetTextureCount(int count)
```

```csharp
public void SetRenderBatchCount(int count)
```

```csharp
public void SetRenderDrawCallCount(int count)
```

```csharp
public void SetRenderMaxBatchSize(int count)
```

```csharp
public void SetConnectedClients(int count)
```

```csharp
public void RecordNetworkMessageSent(int byteCount)
```

```csharp
public void RecordNetworkMessageReceived(int byteCount)
```

```csharp
public void SetNetworkLatency(double milliseconds)
```

```csharp
public JollyProfile RegisterProfile(string name)
```

```csharp
public JollyProfileScope Profile(JollyProfile? profile)
```

```csharp
public JollyDiagnosticsSnapshot GetSnapshot()
```

```csharp
public void Reset()
```
