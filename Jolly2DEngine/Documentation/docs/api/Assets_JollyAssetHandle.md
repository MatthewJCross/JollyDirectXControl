# `Assets/JollyAssetHandle.cs`

Public API declarations from `Assets/JollyAssetHandle.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyAssetHandle<T> : IDisposable where T : class
```

```csharp
public T Asset { get; }  public bool IsReleased => _release == null;
```

```csharp
public void Dispose()
```
