using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Hello, World!");

// Mess around with code lol
Console.WriteLine("Please enter 0 for SERVER, 1 for CLIENT");
uint res = uint.Parse(Console.ReadLine() ?? "0");

const int serverPort = 25565;

const int connectPacket = 1;
const int disconnectPacket = 2;
const int messagePacket = 3;

switch (res)
{
    case 0:
    {
        int clientId = 0;

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

        break;
    }
    case 1:
    {
        int clientId = -1;

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

        break;
    }
    default:
        Console.WriteLine("You non. 1 or 0!!! <:agnry:/>");
        break;
}

Console.ReadKey(false);