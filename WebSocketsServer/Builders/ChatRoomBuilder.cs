using WebSocketsServer.Interfaces;

namespace WebSocketsServer.Builders
{
    public class ChatRoomBuilder
    {
        private readonly Server _server;

        public ChatRoomBuilder(Server server)
        {
            _server = server;
        }

        public IChatRoom CreateChatRoom()
        {
            return new ChatRoom(_server);
        }
    }
}
