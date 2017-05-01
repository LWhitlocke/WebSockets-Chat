using System;

namespace WebSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter your name.");
            var userName = Console.ReadLine();

            var newClient = new Client(userName);
            newClient.OpenTcpConnectionWithServer();

            Console.WriteLine("Press any key to terminate connection");
            Console.ReadLine();

            newClient.DisconnectTcpConnection();

            Console.WriteLine("Press any key to close application");
            Console.ReadLine();
        }
    }
}
