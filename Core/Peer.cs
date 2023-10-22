using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Core;

public abstract class Peer
{
    private readonly UdpClient _udpClient;

    protected readonly IPEndPoint ReceiveEndPoint;
    protected IPEndPoint SendEndPoint;

    public bool Active { get; private set; } = false;

    public Action<int>? OnStartedCallback;
    public Action? OnClosedCallback;
    public Action<byte[], IPEndPoint, MessageType>? OnDataReceivedCallback;

    protected readonly Dictionary<byte, Action<byte[], IPEndPoint, int>> PacketHandlers;

    public Peer(int receivePort)
    {
        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, receivePort));
        ReceiveEndPoint = (IPEndPoint)_udpClient.Client.LocalEndPoint!;

        // TODO(calco): Figure out a better default.
        // SendEndPoint = new IPEndPoint(IPAddress.Any, receivePort);

        OnStartedCallback = null;
        OnClosedCallback = null;
        OnDataReceivedCallback = null;

        PacketHandlers = new Dictionary<byte, Action<byte[], IPEndPoint, int>>();
        OnDataReceivedCallback = HandleReceivedData;
    }

    public void AddPacketHandler(byte packet,
        Action<byte[], IPEndPoint, int> handler)
    {
        PacketHandlers.Add(packet, handler);
    }


    public virtual void SetSendEndPoint(IPEndPoint ipEndPoint)
    {
        SendEndPoint = ipEndPoint;
    }

    public virtual void SetSendEndPoint(string ipAddress, int port)
    {
        SendEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
    }

    public virtual void Start()
    {
        Active = true;
        OnStartedCallback?.Invoke(ReceiveEndPoint.Port);

        _udpClient.BeginReceive(UdpClientReceiveCallback, null);
    }

    public virtual void Close()
    {
        Active = false;
        OnClosedCallback?.Invoke();

        _udpClient.Close();
    }

    public void SendBytes(byte[] data)
    {
        _udpClient.Send(data, SendEndPoint);
    }

    private void UdpClientReceiveCallback(IAsyncResult ar)
    {
        if (!Active)
            return;

        IPEndPoint endPoint = null!;
        byte[] data = _udpClient.EndReceive(ar, ref endPoint!);

        OnDataReceivedCallback?.Invoke(data, endPoint, MessageType.Udp);

        if (this is Server || data[0] != (byte)CorePackets.Disconnect)
            _udpClient.BeginReceive(UdpClientReceiveCallback, null);
    }

    protected abstract void HandleReceivedData(byte[] data, IPEndPoint sender,
        MessageType type);
}