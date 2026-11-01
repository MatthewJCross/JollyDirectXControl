# `Core/JollyGame.cs`

Public API declarations from `Core/JollyGame.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public abstract class JollyGame : IDisposable
```

```csharp
public JollySceneManager Scenes { get; }  public JollyInput Input { get; }  public JollyGameTime GameTime { get; } = new JollyGameTime();
```

```csharp
public JollyDiagnostics Diagnostics { get; }  public double GameLoopFPS
```

```csharp
public double GameLoopActualFPS => Volatile.Read(ref _gameLoopActualFPS);
```

```csharp
public double GameFrameTime => GameTime.FrameTime;
```

```csharp
public double RenderFPS
```

```csharp
public bool IsRunning => _running;
```

```csharp
public void Start()
```

```csharp
public void Stop()
```
