# `Core/JollyGameTime.cs`

Public API declarations from `Core/JollyGameTime.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyGameTime
```

```csharp
public double TotalSeconds { get; internal set; } public double DeltaSeconds { get; internal set; } public double DeltaMilliseconds => DeltaSeconds * 1000.0;
```

```csharp
public double FPS { get; internal set; } public double FrameTime => DeltaMilliseconds;
```

```csharp
public long FrameCount { get; internal set; } } }
```
