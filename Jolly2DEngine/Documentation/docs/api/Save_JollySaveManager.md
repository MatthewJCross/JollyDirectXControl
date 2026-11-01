# `Save/JollySaveManager.cs`

Public API declarations from `Save/JollySaveManager.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollySaveManager
```

```csharp
public const int CurrentFormatVersion = 1;
```

```csharp
public string SaveDirectory { get; }  public JollySaveManager(string? saveDirectory = null)
```

```csharp
public void SaveGame<T>(string slot, T state, int gameVersion = 1)
```

```csharp
public T LoadGame<T>(string slot, int? expectedGameVersion = null)
```

```csharp
public bool TryLoadGame<T>(string slot, out T? state, int? expectedGameVersion = null)
```

```csharp
public bool Exists(string slot)
```

```csharp
public bool HasBackup(string slot)
```

```csharp
public bool RestoreBackup(string slot)
```

```csharp
public void Delete(string slot)
```

```csharp
public IReadOnlyList<string> GetSlots()
```

```csharp
public int FormatVersion { get; set; } public int GameVersion { get; set; } public DateTimeOffset SavedUtc { get; set; } public T? Payload { get; set; } } }  public sealed class JollySaveException : Exception
```

```csharp
public JollySaveException(string message) : base(message)
```

```csharp
public JollySaveException(string message, Exception innerException) : base(message, innerException)
```
