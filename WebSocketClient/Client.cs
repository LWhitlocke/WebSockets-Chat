using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using WebSocketHelper.TransferProtocols;

namespace WebSocketClient
{
    public class Client
    {
        private string UserName { get; set; }
        private TcpClient tcpClient { get; set; }

        public Client (string userName)
        {
            UserName = userName;
        }

        public void OpenTcpConnectionWithServer()
        {
            var serverAddress = IPAddress.Parse("127.0.0.2");
            tcpClient = new TcpClient();
            tcpClient.Connect(serverAddress, 8080);

            Console.WriteLine(UserName + " connected to the server.");
        }

        public void DisconnectTcpConnection()
        {
            var killCommand = new Ping {RequestType = "Kill"};
            var serializer = new JavaScriptSerializer();
            var serialisedObject = serializer.Serialize(killCommand);
            var sendBytes = Encoding.UTF8.GetBytes(serialisedObject);
            var stream = tcpClient.GetStream();

            stream.Write(sendBytes, 0, sendBytes.Length);
            Thread.Sleep(1000);

            tcpClient.Close();
        }

        public void Send()
        {

        }

        public void Recieve()
        {

        }
    }
}