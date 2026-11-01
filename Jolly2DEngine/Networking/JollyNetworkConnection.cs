using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Jolly2DEngine.Networking;

public sealed class JollyNetworkConnection : IDisposable
{
    private const int MaxMessageSize = 1024 * 1024;
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly Lock _sendLock = new();
    private readonly CancellationTokenSource _receiveCancellation = new();
    private bool _disposed;

    internal JollyNetworkConnection(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        RemoteEndPoint = client.Client.RemoteEndPoint;
    }

    public string Id { get; } = Guid.NewGuid().ToString("N");

    public EndPoint? RemoteEndPoint { get; }

    public bool IsConnected => !_disposed && _client.Connected;

    public event Action<JollyNetworkConnection, JollyNetworkMessage>? MessageReceived;

    public event Action<JollyNetworkConnection>? Disconnected;

    internal void StartReceiving()
    {
        _ = Task.Run(ReceiveLoopAsync);
    }

    public void Send(JollyNetworkMessage message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(message);

        if (message.Payload.Length > MaxMessageSize)
        {
            throw new InvalidOperationException($"Network message exceeds the {MaxMessageSize} byte limit.");
        }

        byte[] length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(message.Payload.Length));

        lock (_sendLock)
        {
            _stream.Write(length, 0, length.Length);
            _stream.Write(message.Payload, 0, message.Payload.Length);
            _stream.Flush();
        }
    }

    public void SendText(string text)
    {
        Send(JollyNetworkMessage.FromText(text));
    }

    public void Disconnect()
    {
        Dispose();
    }

    private async Task ReceiveLoopAsync()
    {
        try
        {
            byte[] lengthBuffer = new byte[4];

            while (!_receiveCancellation.IsCancellationRequested)
            {
                bool gotLength = await ReadExactlyAsync(lengthBuffer, _receiveCancellation.Token);

                if (!gotLength)
                {
                    break;
                }

                int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                if (length < 0 || length > MaxMessageSize)
                {
                    throw new System.IO.InvalidDataException("Invalid network message length.");
                }

                byte[] payload = new byte[length];

                if (!await ReadExactlyAsync(payload, _receiveCancellation.Token))
                {
                    break;
                }

                MessageReceived?.Invoke(this, new JollyNetworkMessage(payload));
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (IOException)
        {
        }
        catch (SocketException)
        {
        }
        finally
        {
            DisposeInternal(true);
        }
    }

    private async Task<bool> ReadExactlyAsync(byte[] buffer, CancellationToken cancellationToken)
    {
        int offset = 0;

        while (offset < buffer.Length)
        {
            int count = await _stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);

            if (count == 0)
            {
                return false;
            }

            offset += count;
        }

        return true;
    }

    public void Dispose()
    {
        DisposeInternal(false);
        GC.SuppressFinalize(this);
    }

    private void DisposeInternal(bool notify)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _receiveCancellation.Cancel();

        try
        {
            _stream.Close();
        }
        catch
        {
        }

        try
        {
            _client.Close();
        }
        catch
        {
        }

        if (notify)
        {
            Disconnected?.Invoke(this);
        }
    }
}
