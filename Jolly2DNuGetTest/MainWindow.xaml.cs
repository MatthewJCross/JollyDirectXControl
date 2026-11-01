using Jolly2DEngine.Networking;
using System.Windows;

namespace Jolly2DNuGetTest;

public partial class MainWindow : Window
{
    private readonly JollyNetworkServer _server = new();
    private readonly JollyNetworkClient _client = new();
    private int _messageNumber;

    public MainWindow()
    {
        InitializeComponent();
        HookNetworkEvents();
        UpdateNetworkStatus();
    }

    private void HookNetworkEvents()
    {
        _server.ClientConnected += OnServerClientConnected;
        _server.ClientDisconnected += OnServerClientDisconnected;
        _server.MessageReceived += OnServerMessageReceived;
        _client.Connected += OnClientConnected;
        _client.Disconnected += OnClientDisconnected;
        _client.MessageReceived += OnClientMessageReceived;
    }

    private void StartServerButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_server.IsRunning)
            {
                AddLog("SERVER: already running");
                return;
            }

            _server.Start(0);
            AddLog($"SERVER: listening on 127.0.0.1:{_server.Port}");
            UpdateNetworkStatus();
        }
        catch (Exception ex)
        {
            AddLog($"SERVER ERROR: {ex.Message}");
        }
    }

    private void ConnectClientButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_server.IsRunning)
            {
                AddLog("CLIENT: start the server first");
                return;
            }

            if (_client.IsConnected)
            {
                AddLog("CLIENT: already connected");
                return;
            }

            _client.Connect("127.0.0.1", _server.Port);
        }
        catch (Exception ex)
        {
            AddLog($"CLIENT ERROR: {ex.Message}");
        }
    }

    private void SendTestButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_client.IsConnected)
        {
            AddLog("CLIENT: not connected");
            return;
        }

        try
        {
            int number = Interlocked.Increment(ref _messageNumber);
            string message = $"Hello from Jolly2D client #{number}";
            _client.SendText(message);
            AddLog($"CLIENT -> SERVER: {message}");
        }
        catch (Exception ex)
        {
            AddLog($"SEND ERROR: {ex.Message}");
        }
    }

    private void BroadcastButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_server.IsRunning)
        {
            AddLog("SERVER: not running");
            return;
        }

        try
        {
            string message = "Broadcast from Jolly2D server";
            _server.BroadcastText(message);
            AddLog($"SERVER -> ALL: {message}");
        }
        catch (Exception ex)
        {
            AddLog($"BROADCAST ERROR: {ex.Message}");
        }
    }

    private void DisconnectClientButton_Click(object sender, RoutedEventArgs e)
    {
        _client.Disconnect();
        AddLog("CLIENT: disconnected");
        UpdateNetworkStatus();
    }

    private void StopServerButton_Click(object sender, RoutedEventArgs e)
    {
        _server.Stop();
        AddLog("SERVER: stopped");
        UpdateNetworkStatus();
    }

    private void OnServerClientConnected(JollyNetworkConnection connection)
    {
        RunOnUi(() =>
        {
            AddLog($"SERVER: client connected ({connection.RemoteEndPoint})");
            UpdateNetworkStatus();
        });
    }

    private void OnServerClientDisconnected(JollyNetworkConnection connection)
    {
        RunOnUi(() =>
        {
            AddLog("SERVER: client disconnected");
            UpdateNetworkStatus();
        });
    }

    private void OnServerMessageReceived(JollyNetworkConnection connection, JollyNetworkMessage message)
    {
        RunOnUi(() => AddLog($"SERVER <- CLIENT: {message.Text}"));

        try
        {
            connection.SendText($"Echo: {message.Text}");
        }
        catch (Exception ex)
        {
            RunOnUi(() => AddLog($"SERVER ECHO ERROR: {ex.Message}"));
        }
    }

    private void OnClientConnected()
    {
        RunOnUi(() =>
        {
            AddLog("CLIENT: connected");
            UpdateNetworkStatus();
        });
    }

    private void OnClientDisconnected()
    {
        RunOnUi(() =>
        {
            AddLog("CLIENT: disconnected by server");
            UpdateNetworkStatus();
        });
    }

    private void OnClientMessageReceived(JollyNetworkMessage message)
    {
        RunOnUi(() => AddLog($"CLIENT <- SERVER: {message.Text}"));
    }

    private void UpdateNetworkStatus()
    {
        string server = _server.IsRunning ? "running" : "stopped";
        string client = _client.IsConnected ? "connected" : "disconnected";
        string port = _server.IsRunning ? _server.Port.ToString() : "-";
        NetworkStatusText.Text = $"Server: {server}\nClient: {client}\nPort: {port}";
    }

    private void AddLog(string message)
    {
        MessageLogText.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
        MessageLogText.ScrollToEnd();
    }

    private void RunOnUi(Action action)
    {
        if (Dispatcher.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.BeginInvoke(action);
    }

    protected override void OnClosed(EventArgs e)
    {
        _client.Dispose();
        _server.Dispose();
        base.OnClosed(e);
    }
}
