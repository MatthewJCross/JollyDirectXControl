# `Save/JollySceneSaveState.cs`

Public API declarations from `Save/JollySceneSaveState.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollySceneSaveState
```

```csharp
public string SceneName { get; set; } = string.Empty;
```

```csharp
public List<JollyGameObjectSaveState> Objects { get; set; } = new();
```

```csharp
public static JollySceneSaveState Capture(JollyScene scene)
```

```csharp
public int ApplyTo(JollyScene scene)
```
