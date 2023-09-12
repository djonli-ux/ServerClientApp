using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

const string serverIp = "127.0.0.1";
const int port = 8080;

Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);

try
{
    socket.Bind(endPoint);
    socket.Listen();

    Console.WriteLine($"Server started {serverIp}:{port}");

    await RunProcessAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

async Task RunProcessAsync()
{
    while (true)
    {
        Socket remoteSocket = await socket.AcceptAsync();

        if (remoteSocket.RemoteEndPoint is IPEndPoint remoteEP)
        {
            Console.WriteLine($"Connection opened for remote: {remoteEP.Address}:{remoteEP.Port}");
        }

        _ = Task.Run(() => HandleRequest(remoteSocket));
    }
}

void HandleRequest(Socket remoteSocket)
{

    while (true)
    {
        byte[] buffer = new byte[256];
        int count = 0;
        string message = string.Empty;

        do
        {
            count = remoteSocket.Receive(buffer);
            message += Encoding.UTF8.GetString(buffer, 0, count);
            if (message.Contains("quit"))
            {
                Console.WriteLine("\nReceived quit command. Closing the server.\n");
                remoteSocket.Shutdown(SocketShutdown.Both);
                remoteSocket.Close();
                break;
            }

        } while (remoteSocket.Available > 0);

        Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: {message}");

        string response = "Everything is ok!";
        remoteSocket.Send(Encoding.UTF8.GetBytes(response)); 
    }
}