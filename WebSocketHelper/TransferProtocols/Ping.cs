using System;

namespace WebSocketHelper.TransferProtocols
{
    [Serializable]
    public class Ping
    {
        public string ResponseType
        {
            get { return "Ping"; }
        }

        public string RequestType { get; set; }
    }
}