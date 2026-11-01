using System.Text;

namespace Jolly2DEngine.Networking;

public sealed class JollyNetworkMessage
{
    public JollyNetworkMessage(byte[] payload)
    {
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    public byte[] Payload { get; }

    public string Text => Encoding.UTF8.GetString(Payload);

    public static JollyNetworkMessage FromText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new JollyNetworkMessage(Encoding.UTF8.GetBytes(text));
    }
}
