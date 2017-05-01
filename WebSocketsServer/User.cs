using System;
using System.Net.Sockets;

namespace WebSocketsServer
{
    public class User
    {
        public string UserName;
        public TcpClient Client { get; private set; }
        public Boolean IsUserValid { get; set; } //

        public User(TcpClient tcpConnection)
        {
            Client = tcpConnection;
            IsUserValid = true;
        }

        /// <summary>
        ///     Current not in use
        /// </summary>
        public ChatRoom AssociatedRoom { get; set; }
    }
}