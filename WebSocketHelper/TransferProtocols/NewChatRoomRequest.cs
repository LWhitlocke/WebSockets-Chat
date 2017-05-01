namespace WebSocketHelper.TransferProtocols
{
    public class NewChatRoomRequest
    {
        public string ResponseType
        {
            get { return "NewChatRoomRequest"; }
        }

        public string Name { get; set; }
    }
}
