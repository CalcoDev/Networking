using Core;

Console.WriteLine("Hello, World!");

Console.WriteLine("Please enter 0 for SERVER, 1 for CLIENT");
uint res = uint.Parse(Console.ReadLine() ?? "0");

const int serverPort = 25565;

const int connectPacket = 1;
const int disconnectPacket = 2;
const int messagePacket = 3;

switch (res)
{
    case 0:
        Server server = new Server(serverPort);
        server.Start();

        Console.WriteLine("Write stop to stop the server.");
        while (true)
        {
            string message = Console.ReadLine() ?? "";
            if (message == "stop")
                break;
        }
        server.Close();

        break;
    case 1:
        Client client = new Client("127.0.0.1", serverPort);
        client.Start();

        while (true)
        {
            Console.WriteLine("Enter a message:");
            string message = Console.ReadLine() ?? "";
            if (message == "stop")
                break;
            client.Send(message);
        }
        client.Close();

        break;
    default:
        Console.WriteLine("You non. 1 or 0!!! <:agnry:/>");
        break;
}

Console.ReadKey(false);