namespace WebSocketHelper.TransferProtocols
{
    public class SystemResponse
    {
        public string ResponseType
        {
            get { return "SystemResponse"; }
        }

        public string Message { get; set; }
    }
}