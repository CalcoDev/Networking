using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Core;

/*
 *
 *int clientId = -1;

        Console.WriteLine("Started CLIENT.\n");

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, serverPort);

        // int receivePort = ((IPEndPoint)socket.LocalEndPoint!).Port;
        // UdpClient listener = new UdpClient(receivePort);
        // IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, receivePort);

        byte[] buffer;
        // byte[] buffer = {connectPacket};
        // socket.SendTo(buffer, serverEndPoint);

        // DateTime startTime = DateTime.Now;
        // while (true)
        // {
            // buffer = listener.Receive(ref receiveEndPoint);
            // if (buffer[0] == connectPacket)
            // {
            //     clientId = buffer[1];
            //     break;
            // }

        //     DateTime now = DateTime.Now;
        //     if ((now - startTime).Seconds >= 10)
        //     {
        //         Console.WriteLine("Tried to connect to server but failed! Aborting...");
        //         Environment.Exit(0);
        //     }
        // }

        // Console.WriteLine($"Successfully connected to server, with ID {clientId}");
        //
        while (true)
        {
            Console.WriteLine("Please enter a message to send: ...");
            string message = Console.ReadLine() ?? string.Empty;
            buffer = Encoding.ASCII.GetBytes(message);
            socket.SendTo(buffer, serverEndPoint);
        }
 *
 */

public class Client
{
    // Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
    //     ProtocolType.Udp);
    // IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
    // IPEndPoint serverEndPoint = new IPEndPoint(ipAddress, serverPort);

    private readonly UdpClient _udpClient;
    private readonly IPEndPoint _sendEndPoint;

    public bool Active { get; private set; } = false;

    public Client(string ipAddress, int port)
    {
        IPEndPoint selfEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
        _sendEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        _udpClient = new UdpClient(selfEndPoint);
    }

    public void Start()
    {
        Console.WriteLine("Client began ... existing.");
        Active = true;
    }

    public void Send(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        _udpClient.Send(data, _sendEndPoint);
    }

    public void Close()
    {
        Console.WriteLine("Shutting off client!");

        Active = false;
        _udpClient.Close();
    }
}