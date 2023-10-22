using System.Net;
using System.Text;
using Core;

Console.WriteLine("0 = SERVER | 1 = CLIENT");
uint res = uint.Parse(Console.ReadLine() ?? "0");

const int serverPort = 25565;

const int messagePacket = 2;

switch (res)
{
    case 0:
        // TODO(calco): This shuold be IPendpoint to int
        Server server = new Server(serverPort);
        server.OnStartedCallback = port =>
        {
            Console.WriteLine($"Started server on port {port}.");
            Console.WriteLine("Awaiting messages...");
        };
        server.OnClosedCallback = () =>
        {
            Console.WriteLine($"Stopped server.");
        };

        server.OnClientConnectedCallback = (endPoint, id) =>
        {
            Console.WriteLine($"{endPoint} connected with id {id}.");
        };
        server.OnClientDisconnectedCallback = (endPoint, id) =>
        {
            Console.WriteLine($"{endPoint} [{id}] disconnected.");
        };

        server.AddPacketHandler(messagePacket, (data, ip, id) =>
        {
            string message = new Packet(data).ReadString();
            Console.WriteLine($"[{id}]: {message}");

            Packet p = new Packet();
            p.WriteByte(messagePacket);
            p.WriteString(message);

            server.BroadcastBytes(p.ToByteArray(), ip);
        });

        server.Start();

        while (true)
        {
            string message = Console.ReadLine() ?? "";
            if (message == "stop")
                break;
        }
        server.Close();

        break;
    case 1:
        Client client = new Client(0);
        client.OnStartedCallback = (port) =>
        {
            Console.WriteLine($"Started client on port {port}.");
        };
        client.OnClosedCallback = () =>
        {
            Console.WriteLine($"Stopped client.");
        };

        client.OnConnectedCallback = (id) =>
        {
            Console.WriteLine($"Connected to server with ID: {id}");
        };
        client.OnDisconnectedCallback = _ =>
        {
            Console.WriteLine($"Disconnected from server.");
            client.Close();
        };

        client.AddPacketHandler(messagePacket, (data, ip, id) =>
        {
            string message = (new Packet(data)).ReadString();
            Console.WriteLine($"[{id}]: {message}");
        });

        client.Connect("127.0.0.1", serverPort);

        while (true)
        {
            Console.WriteLine($"ENTER MESSAGE: ...");
            string message = Console.ReadLine() ?? "";
            if (message == "disconnect")
            {
                client.Disconnect();
                break;
            }

            Packet p = new Packet();
            p.WriteByte(messagePacket);
            p.WriteString(message);
            client.SendBytes(p.ToByteArray());
        }

        break;
    default:
        Console.WriteLine("You non. 1 or 0!!! <:agnry:/>");
        break;
}

Console.ReadKey(false);