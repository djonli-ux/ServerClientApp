
//TODO: Исправить ошибку при повторном отправлении сообщения 

using System.Net;
using System.Net.Sockets;
using System.Text;

const string serverIp = "127.0.0.1";   
const int serverPort = 8080;
bool isWork = true;

Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

try
{
    socket.Connect(serverEP);

    while (isWork)
    {
        string? message = string.Empty;
        do
        {
            Console.Clear();
            Console.Write("Enter your message: ");
            message = Console.ReadLine();
        } while (string.IsNullOrWhiteSpace(message));

        if (message != null)
        {
            socket.Send(Encoding.UTF8.GetBytes(message));

            byte[] buffer = new byte[256];
            int byteCount = 0;
            string response = string.Empty;

            do
            {
                byteCount = socket.Receive(buffer);
                response += Encoding.UTF8.GetString(buffer, 0, byteCount);

            } while (socket.Available > 0);

            Console.WriteLine($"Response from Server: {response}");

            Console.WriteLine("\n\nIf u wanna exit the app press 'ESC' \n\n");

            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (keyInfo.Key == ConsoleKey.Escape)
                isWork = false; 

            if (!isWork)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        } 
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");

}

Console.ReadLine();
