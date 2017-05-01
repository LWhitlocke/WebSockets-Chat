using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WebSocketsServer
{
    public static class Program
    {
        private static Server _server;
        private static TcpListener _serverTcpListener;

        private static void Main()
        {
            _server = new Server();
            try
            {
                _server.SetupNewChatRoom();
                _serverTcpListener = CreateNewTcpListener();

                _serverTcpListener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured setting up the chat room.");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("");
                Console.WriteLine("Server Location: " + _server.ServerLocation);
                Console.WriteLine("Port: " + _server.Port);
                Console.WriteLine("");
                Console.WriteLine("Press any key to continue.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Server has started on {0}:{1}.{2}Waiting for a connection...", _server.ServerLocation, _server.Port, Environment.NewLine);

            _server.StartListeningForNewUsersThread();

            Console.WriteLine("Client list updater thread started.");

            while (true)
            {
                var client = _serverTcpListener.AcceptTcpClient();
                var newUser = new User(client);
                var clientThread = new Thread(() => new Client(newUser, _server));

                _server.ChatRooms[0].Users.Add(newUser);
                clientThread.Start();
            }
        }

        private static TcpListener CreateNewTcpListener()
        {
            return new TcpListener(IPAddress.Parse(_server.ServerLocation), _server.Port);
        }
    }
}