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

        // Запускаем обработку запроса с таймером
        _ = Task.Run(() => HandleRequest(remoteSocket));
    }
}

void HandleRequest(Socket remoteSocket)
{
    byte[] buffer = new byte[256];
    int count = 0;
    string message = string.Empty;

    Timer timer = new Timer(120000); // Таймер на 2 минуты
    timer.AutoReset = false; // Не повторять после истечения времени
    timer.Elapsed += (sender, e) =>
    {
        // Закрываем соединение, если не получено сообщение в течение 2 минут
        remoteSocket.Shutdown(SocketShutdown.Both);
        remoteSocket.Close();
        Console.WriteLine("Connection closed due to inactivity");
    };
    timer.Start();

    do
    {
        count = remoteSocket.Receive(buffer);
        message += Encoding.UTF8.GetString(buffer, 0, count);

        // Сбросить таймер каждый раз, когда получаем данные
        timer.Stop();
        timer.Start();
    } while (remoteSocket.Available > 0);

    Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: {message}");

    string response = "Everything is ok!";
    remoteSocket.Send(Encoding.UTF8.GetBytes(response));
}
