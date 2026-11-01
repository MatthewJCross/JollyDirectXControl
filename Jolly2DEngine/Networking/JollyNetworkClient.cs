using System.Net;
using System.Net.Sockets;

namespace Jolly2DEngine.Networking;

public sealed class JollyNetworkClient : IDisposable
{
    private JollyNetworkConnection? _connection;
    private bool _disposed;

    public bool IsConnected => _connection?.IsConnected == true;

    public string Host { get; private set; } = string.Empty;

    public int Port { get; private set; }

    public event Action? Connected;

    public event Action? Disconnected;

    public event Action<JollyNetworkMessage>? MessageReceived;

    public void Connect(string host, int port)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        if (IsConnected)
        {
            throw new InvalidOperationException("The network client is already connected.");
        }

        if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        TcpClient client = new();
        client.Connect(host, port);

        JollyNetworkConnection connection = new(client);
        connection.MessageReceived += OnMessageReceived;
        connection.Disconnected += OnDisconnected;
        _connection = connection;
        Host = host;
        Port = port;
        connection.StartReceiving();
        Connected?.Invoke();
    }

    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        if (IsConnected)
        {
            throw new InvalidOperationException("The network client is already connected.");
        }

        if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        TcpClient client = new();
        await client.ConnectAsync(host, port, cancellationToken);

        JollyNetworkConnection connection = new(client);
        connection.MessageReceived += OnMessageReceived;
        connection.Disconnected += OnDisconnected;
        _connection = connection;
        Host = host;
        Port = port;
        connection.StartReceiving();
        Connected?.Invoke();
    }

    public void Send(JollyNetworkMessage message)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("The network client is not connected.");
        }

        _connection.Send(message);
    }

    public void SendText(string text)
    {
        Send(JollyNetworkMessage.FromText(text));
    }

    public void Disconnect()
    {
        JollyNetworkConnection? connection = _connection;
        _connection = null;

        if (connection == null)
        {
            return;
        }

        connection.MessageReceived -= OnMessageReceived;
        connection.Disconnected -= OnDisconnected;
        connection.Disconnect();
        Disconnected?.Invoke();
    }

    private void OnMessageReceived(JollyNetworkConnection connection, JollyNetworkMessage message)
    {
        MessageReceived?.Invoke(message);
    }

    private void OnDisconnected(JollyNetworkConnection connection)
    {
        if (ReferenceEquals(_connection, connection))
        {
            _connection = null;
            Disconnected?.Invoke();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Disconnect();
        GC.SuppressFinalize(this);
    }
}
