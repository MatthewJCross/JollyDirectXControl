# `Networking/JollyNetworkConnection.cs`

Public API declarations from `Networking/JollyNetworkConnection.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyNetworkConnection : IDisposable
```

```csharp
public string Id { get; } = Guid.NewGuid().ToString("N");
```

```csharp
public EndPoint? RemoteEndPoint { get; }  public bool IsConnected => !_disposed && _client.Connected;
```

```csharp
public event Action<JollyNetworkConnection, JollyNetworkMessage>? MessageReceived;
```

```csharp
public event Action<JollyNetworkConnection>? Disconnected;
```

```csharp
public void Send(JollyNetworkMessage message)
```

```csharp
public void SendText(string text)
```

```csharp
public void Disconnect()
```

```csharp
public void Dispose()
```
