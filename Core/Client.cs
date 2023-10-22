using System.Net;

namespace Core;

public class Client : Peer
{
    public int Id;
    public bool Connected { get; private set; } = false;

    public Action<int>? OnConnectedCallback;
    public Action<int>? OnDisconnectedCallback;

    public Client(int receivePort) : base(receivePort)
    {
    }

    public void Connect(string ip, int port)
    {
        SetSendEndPoint(ip, port);
        Start();
        SendBytes(new []{(byte)CorePackets.Connect});
    }

    public void Disconnect()
    {
        SendBytes(new[]{(byte)CorePackets.Disconnect});
    }

    protected override void HandleReceivedData(byte[] data, IPEndPoint sender)
    {
        switch (data[0])
        {
            case (byte)CorePackets.Connect:
                Id = data[1];
                Connected = true;
                OnConnectedCallback?.Invoke(Id);
                break;
            case (byte)CorePackets.Disconnect:
                Connected = false;
                OnDisconnectedCallback?.Invoke(Id);
                break;
        }

        byte[] b = new byte[data.Length - 1];
        Array.Copy(data, 1, b, 0, b.Length);

        if (PacketHandlers.TryGetValue(data[0], out var f))
            f.Invoke(b, sender, Id);
    }
}