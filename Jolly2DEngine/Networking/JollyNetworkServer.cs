using System.Net;
using System.Net.Sockets;

namespace Jolly2DEngine.Networking;

public sealed class JollyNetworkServer : IDisposable
{
    private readonly object _clientsLock = new();
    private readonly List<JollyNetworkConnection> _clients = new();
    private TcpListener? _listener;
    private CancellationTokenSource? _acceptCancellation;
    private bool _disposed;

    public bool IsRunning => _listener != null;

    public int Port { get; private set; }

    public IReadOnlyList<JollyNetworkConnection> Clients
    {
        get
        {
            lock (_clientsLock)
            {
                return _clients.ToArray();
            }
        }
    }

    public event Action<JollyNetworkConnection>? ClientConnected;

    public event Action<JollyNetworkConnection>? ClientDisconnected;

    public event Action<JollyNetworkConnection, JollyNetworkMessage>? MessageReceived;

    public void Start(int port)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (IsRunning)
        {
            throw new InvalidOperationException("The network server is already running.");
        }

        if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        TcpListener listener = new(IPAddress.Any, port);
        listener.Start();
        _listener = listener;
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        _acceptCancellation = new CancellationTokenSource();
        _ = Task.Run(AcceptLoopAsync);
    }

    public void Stop()
    {
        TcpListener? listener = _listener;
        _listener = null;
        Port = 0;

        _acceptCancellation?.Cancel();
        _acceptCancellation?.Dispose();
        _acceptCancellation = null;

        try
        {
            listener?.Stop();
        }
        catch
        {
        }

        JollyNetworkConnection[] clients;

        lock (_clientsLock)
        {
            clients = _clients.ToArray();
            _clients.Clear();
        }

        foreach (JollyNetworkConnection client in clients)
        {
            UnhookClient(client);
            client.Disconnect();
        }
    }

    public void Broadcast(JollyNetworkMessage message)
    {
        foreach (JollyNetworkConnection client in Clients)
        {
            if (!client.IsConnected)
            {
                continue;
            }

            try
            {
                client.Send(message);
            }
            catch
            {
                client.Disconnect();
            }
        }
    }

    public void BroadcastText(string text)
    {
        Broadcast(JollyNetworkMessage.FromText(text));
    }

    private async Task AcceptLoopAsync()
    {
        TcpListener? listener = _listener;
        CancellationToken cancellationToken = _acceptCancellation?.Token ?? CancellationToken.None;

        if (listener == null)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                JollyNetworkConnection connection = new(client);
                HookClient(connection);

                lock (_clientsLock)
                {
                    _clients.Add(connection);
                }

                ClientConnected?.Invoke(connection);
                connection.StartReceiving();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private void HookClient(JollyNetworkConnection client)
    {
        client.MessageReceived += OnClientMessageReceived;
        client.Disconnected += OnClientDisconnected;
    }

    private void UnhookClient(JollyNetworkConnection client)
    {
        client.MessageReceived -= OnClientMessageReceived;
        client.Disconnected -= OnClientDisconnected;
    }

    private void OnClientMessageReceived(JollyNetworkConnection client, JollyNetworkMessage message)
    {
        MessageReceived?.Invoke(client, message);
    }

    private void OnClientDisconnected(JollyNetworkConnection client)
    {
        lock (_clientsLock)
        {
            _clients.Remove(client);
        }

        UnhookClient(client);
        ClientDisconnected?.Invoke(client);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
        GC.SuppressFinalize(this);
    }
}
