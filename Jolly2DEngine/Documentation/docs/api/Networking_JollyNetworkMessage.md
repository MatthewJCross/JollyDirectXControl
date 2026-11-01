# `Networking/JollyNetworkMessage.cs`

Public API declarations from `Networking/JollyNetworkMessage.cs`.

> This page is generated from the source so the signatures stay tied to the implementation. Behavioural notes and examples are in the subsystem guides and `API_REFERENCE.md`.

```csharp
public sealed class JollyNetworkMessage
```

```csharp
public JollyNetworkMessage(byte[] payload)
```

```csharp
public byte[] Payload { get; }  public string Text => Encoding.UTF8.GetString(Payload);
```

```csharp
public static JollyNetworkMessage FromText(string text)
```
