using System.Net;

namespace Core;

public class Server : Peer
{
    private readonly Dictionary<IPEndPoint, int> _clientIds;
    private int _currentClientId = 0;

    public Action<IPEndPoint, int>? OnClientConnectedCallback;
    public Action<IPEndPoint, int>? OnClientDisconnectedCallback;

    public Server(int receivePort) : base(receivePort)
    {
        _clientIds = new Dictionary<IPEndPoint, int>();
    }

    public void BroadcastBytes(byte[] data, IPEndPoint exclude)
    {
        foreach (var (ip, _) in _clientIds)
        {
            if (Equals(ip, exclude))
                continue;

            SetSendEndPoint(ip);
            SendBytes(data);
        }
    }

    protected override void HandleReceivedData(byte[] data, IPEndPoint sender)
    {
        switch (data[0])
        {
            case (byte)CorePackets.Connect:
                _clientIds.Add(sender, _currentClientId);
                OnClientConnectedCallback?.Invoke(sender, _currentClientId);

                SetSendEndPoint(sender);
                SendBytes(new []{(byte)CorePackets.Connect, (byte)_currentClientId});

                _currentClientId += 1;
                break;
            case (byte)CorePackets.Disconnect:
                OnClientDisconnectedCallback?.Invoke(sender, _clientIds[sender]);
                _clientIds.Remove(sender);

                SetSendEndPoint(sender);
                SendBytes(new []{(byte)CorePackets.Disconnect});
                break;
        }

        byte[] b = new byte[data.Length - 1];
        Array.Copy(data, 1, b, 0, b.Length);

        if (PacketHandlers.TryGetValue(data[0], out var f))
            f.Invoke(b, sender, _clientIds[sender]);
    }
}
