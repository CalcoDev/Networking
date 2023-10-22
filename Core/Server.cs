using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Core;

/*
 *
 *int clientId = 0;

        UdpClient listener = new UdpClient(serverPort);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);

        // Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw,
        //     ProtocolType.Udp);
        // IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

        try
        {
            Console.WriteLine("Started SERVER.\nWaiting for receive message.");
            while (true)
            {
                Console.WriteLine("Polling for input!");
                byte[] bytes = listener.Receive(ref endPoint);
                Console.WriteLine($"Received message from {endPoint}:");

                Task receiveTask = listener.ReceiveAsync();

                // IPEndPoint socketEndPoint = new IPEndPoint(ipAddress, endPoint.Port);

                // if (bytes[0] == connectPacket)
                // {
                //     int id = clientId;
                //     clientId += 1;
                //     Console.WriteLine($"Client {endPoint} connected to server and assigned ID {id}");
                //     byte[] buffer = { connectPacket, (byte)id };
                //     socket.SendTo(buffer, socketEndPoint);
                // }
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            listener.Close();
        }
 *
 */

public class Server
{
    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _receiveEndPoint;

    public bool Active { get; private set; } = false;

    public Server(int port)
    {
        _receiveEndPoint = new IPEndPoint(IPAddress.Any, port);
        _udpClient = new UdpClient(_receiveEndPoint);
    }

    public void Start()
    {
        Console.WriteLine(
            $"Server began listening to messages on port: {_receiveEndPoint.Port}");

        Active = true;
        _udpClient.BeginReceive(UdpClientReceiveCallback, null);
    }

    private void UdpClientReceiveCallback(IAsyncResult ar)
    {
        if (!Active)
            return;

        IPEndPoint? endPoint = null;
        byte[] data = _udpClient.EndReceive(ar, ref endPoint);

        Console.WriteLine($"Received data:\n{Encoding.ASCII.GetString(data)}");

        _udpClient.BeginReceive(UdpClientReceiveCallback, null);
    }

    public void Close()
    {
        Console.WriteLine("Shutting off server!");

        Active = false;
        _udpClient.Close();
    }
}
