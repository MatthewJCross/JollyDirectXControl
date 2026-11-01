# `Networking/JollyNetworkClient.cs`

Public API declarations from `Networking/JollyNetworkClient.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyNetworkClient : IDisposable
```

```csharp
public bool IsConnected => _connection?.IsConnected == true;
```

```csharp
public string Host { get; private set; } = string.Empty;
```

```csharp
public int Port { get; private set; }  public event Action? Connected;
```

```csharp
public event Action? Disconnected;
```

```csharp
public event Action<JollyNetworkMessage>? MessageReceived;
```

```csharp
public void Connect(string host, int port)
```

```csharp
public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
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
