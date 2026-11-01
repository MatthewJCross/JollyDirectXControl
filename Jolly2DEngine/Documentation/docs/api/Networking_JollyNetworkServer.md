# `Networking/JollyNetworkServer.cs`

Public API declarations from `Networking/JollyNetworkServer.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyNetworkServer : IDisposable
```

```csharp
public bool IsRunning => _listener != null;
```

```csharp
public int Port { get; private set; }  public IReadOnlyList<JollyNetworkConnection> Clients
```

```csharp
public event Action<JollyNetworkConnection>? ClientConnected;
```

```csharp
public event Action<JollyNetworkConnection>? ClientDisconnected;
```

```csharp
public event Action<JollyNetworkConnection, JollyNetworkMessage>? MessageReceived;
```

```csharp
public void Start(int port)
```

```csharp
public void Stop()
```

```csharp
public void Broadcast(JollyNetworkMessage message)
```

```csharp
public void BroadcastText(string text)
```

```csharp
public void Dispose()
```
