# `Scene/JollySceneManager.cs`

Public API declarations from `Scene/JollySceneManager.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollySceneManager
```

```csharp
public JollyScene? CurrentScene { get; private set; } public JollyScene? PreviousScene { get; private set; } public int EntityCount => CurrentScene?.EntityCount ?? 0;
```

```csharp
public bool IsChangingScene => _pendingScenes.Count > 0;
```

```csharp
public event Action<JollyScene?, JollyScene?>? SceneChanged;
```

```csharp
public void ChangeScene(JollyScene scene)
```

```csharp
public JollyGameObject? Find(string name)
```

```csharp
public T? Find<T>(string name) where T : JollyGameObject
```

```csharp
public IReadOnlyList<T> FindAll<T>() where T : JollyGameObject
```

```csharp
public void Update(JollyGameTime gameTime)
```

```csharp
public void Draw()
```

```csharp
public void Shutdown()
```
