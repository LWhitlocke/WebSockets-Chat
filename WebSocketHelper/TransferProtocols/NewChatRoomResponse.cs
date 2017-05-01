namespace WebSocketHelper.TransferProtocols
{
    public class NewChatRoomResponse
    {
        public string ResponseType
        {
            get { return "NewChatRoomResponse"; }
        }

        public int ChatRoomId { get; set; }
    }
}
