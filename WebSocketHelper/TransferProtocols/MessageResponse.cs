namespace WebSocketHelper.TransferProtocols
{
    public class MessageResponse
    {
        public string ResponseType
        {
            get { return "MessageResponse"; }
        }

        public string Name { get; set; }
        public string Message { get; set; }
        public int ChatRoomId { get; set; }
    }
}