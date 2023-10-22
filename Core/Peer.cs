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
    public Action<byte[], IPEndPoint>? OnDataReceivedCallback;

    protected readonly Dictionary<byte, Action<byte[], IPEndPoint, int>> PacketHandlers;

    public Peer(int receivePort)
    {
        ReceiveEndPoint = new IPEndPoint(IPAddress.Any, receivePort);
        _udpClient = new UdpClient(ReceiveEndPoint);

        // TODO(calco): Figure out a better default.
        SendEndPoint = new IPEndPoint(IPAddress.Any, receivePort);

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
        _udpClient.BeginReceive(UdpClientReceiveCallback, null);

        OnStartedCallback?.Invoke(ReceiveEndPoint.Port);
    }

    public virtual void Close()
    {
        Active = false;
        _udpClient.Close();

        OnClosedCallback?.Invoke();
    }

    public void SendBytes(byte[] data)
    {
        _udpClient.Send(data, SendEndPoint);
    }

    public void SendInt(int data)
    {
        byte[] buffer = BitConverter.GetBytes(data);
        _udpClient.Send(buffer, SendEndPoint);
    }

    public void SendFloat(float data)
    {
        byte[] buffer = BitConverter.GetBytes(data);
        _udpClient.Send(buffer, SendEndPoint);
    }

    public void SendString(string data)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        _udpClient.Send(buffer, SendEndPoint);
    }

    private void UdpClientReceiveCallback(IAsyncResult ar)
    {
        if (!Active)
            return;

        IPEndPoint endPoint = null!;
        byte[] data = _udpClient.EndReceive(ar, ref endPoint!);

        OnDataReceivedCallback?.Invoke(data, endPoint);

        if (this is Server || data[0] != (byte)CorePackets.Disconnect)
            _udpClient.BeginReceive(UdpClientReceiveCallback, null);
    }

    protected abstract void HandleReceivedData(byte[] data, IPEndPoint sender);
}