using System.Net;
using System.Net.Sockets;

namespace Core;

public class Server : Peer
{
    public Action<IPEndPoint, int, MessageType>? OnClientConnectedCallback;
    public Action<IPEndPoint, int, MessageType>? OnClientDisconnectedCallback;

    private readonly Dictionary<IPEndPoint, int> _clientIds;
    private readonly Dictionary<IPEndPoint, TcpClient> _tcpClients;

    private int _currentClientId;
    private readonly TcpListener _tcpListener;
    private readonly byte[] _tcpDataBuffer;

    public Server(int receivePort) : base(receivePort)
    {
        _clientIds = new Dictionary<IPEndPoint, int>();
        _tcpClients = new Dictionary<IPEndPoint, TcpClient>();
        _tcpListener =
            new TcpListener(IPAddress.Parse("127.0.0.1"), receivePort);
        _tcpDataBuffer = new byte[1024];
    }

    public override void Start()
    {
        base.Start();

        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(TcpClientConnectCallback, null);
    }

    public override void Close()
    {
        base.Close();

        _tcpListener.Stop();
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

    protected override void HandleReceivedData(byte[] data, IPEndPoint sender,
        MessageType type)
    {
        if (data.Length == 0)
            return;

        switch (data[0])
        {
            case (byte)CorePackets.Connect:
            {
                if (_clientIds.TryAdd(sender, _currentClientId))
                    _currentClientId += 1;

                int id = _clientIds[sender];
                OnClientConnectedCallback?.Invoke(sender, id, type);

                byte[] sendBuffer =
                    { (byte)CorePackets.Connect, (byte)id };
                if (type == MessageType.Udp)
                {
                    SetSendEndPoint(sender);
                    SendBytes(sendBuffer);
                }
                else
                    _tcpClients[sender].GetStream().Write(sendBuffer);

                break;
            }
            case (byte)CorePackets.Disconnect:
            {
                OnClientDisconnectedCallback?.Invoke(sender, _clientIds[sender],
                    type);

                // TODO(calco): Should remove, but check if both TCP and UDP.
                // _clientIds.Remove(sender);

                byte[] sendBuffer = { (byte)CorePackets.Disconnect };
                if (type == MessageType.Udp)
                {
                    SetSendEndPoint(sender);
                    SendBytes(sendBuffer);
                }
                else
                {
                    _tcpClients[sender].GetStream().Write(sendBuffer);
                    // _tcpClients.Remove(sender);
                }

                break;
            }
        }

        byte[] b = new byte[data.Length - 1];
        Array.Copy(data, 1, b, 0, b.Length);

        if (PacketHandlers.TryGetValue(data[0], out var f))
            f.Invoke(b, sender, _clientIds[sender]);
    }

    private void TcpClientConnectCallback(IAsyncResult ar)
    {
        if (!Active)
            return;

        TcpClient tcpClient = _tcpListener.EndAcceptTcpClient(ar);

        IPEndPoint sender = (IPEndPoint)tcpClient.Client.RemoteEndPoint!;
        _tcpClients.TryAdd(sender, tcpClient);
        tcpClient.GetStream().BeginRead(_tcpDataBuffer, 0, 1024,
            TcpClientReceiveCallback, sender);

        _tcpListener.BeginAcceptTcpClient(TcpClientConnectCallback, null);
    }

    private void TcpClientReceiveCallback(IAsyncResult ar)
    {
        if (!Active)
            return;

        IPEndPoint sender = (IPEndPoint)ar.AsyncState!;
        NetworkStream networkStream = _tcpClients[sender].GetStream();

        // TODO(calco): Define a fixed max size TCP size send.
        int bytesRead = networkStream.EndRead(ar);
        if (bytesRead >= 0)
        {
            byte[] data = new byte[bytesRead];
            Array.Copy(_tcpDataBuffer, 0, data, 0, bytesRead);
            OnDataReceivedCallback?.Invoke(data,
                sender, MessageType.Tcp);
        }

        if (bytesRead <= 0 || _tcpDataBuffer[0] != (byte)CorePackets.Disconnect)
            networkStream.BeginRead(_tcpDataBuffer, 0, 1024,
                TcpClientReceiveCallback, sender);
    }
}
