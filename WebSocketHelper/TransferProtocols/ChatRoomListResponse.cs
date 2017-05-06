using System.Collections.Generic;

namespace WebSocketHelper.TransferProtocols
{
    public class ChatRoomListResponse
    {
        public string ResponseType
        {
            get { return "ChatRoomListResponse"; }
        }

        public IList<int> ChatRoomIds { get; set; }
    }
}
