using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using WebSocketsServer.Builders;
using WebSocketsServer.Interfaces;

namespace WebSocketsServer
{
    public class Server
    {
        public readonly List<IChatRoom> ChatRooms = new List<IChatRoom>();
        public string ServerLocation { get;}
        public int Port { get; }
        private static ChatRoomBuilder _chatRoomBuilder;
        public const int BaseChatRoomId = 0;

        public Server()
        {
            ServerLocation = ConfigurationManager.AppSettings["location"];
            int parsedPort;
            int.TryParse(ConfigurationManager.AppSettings["port"], out parsedPort);
            Port = parsedPort;
            _chatRoomBuilder = new ChatRoomBuilder(this);
        }

        public ChatRoom SetupNewChatRoom()
        {
            var chatRoom = _chatRoomBuilder.CreateChatRoom() as ChatRoom;
            if (chatRoom == null) throw new Exception("Error creating chatroom");

            var lastChatRoom = ChatRooms.LastOrDefault();
            var newChatRoomId = 0;

            if (lastChatRoom != null)
            {
                var lastChatRoomId = lastChatRoom.ChatRoomId;
                newChatRoomId = lastChatRoomId + 1;

                while (ChatRooms.Any(x => x.ChatRoomId == newChatRoomId))
                {
                    newChatRoomId += 1;
                }
            }

            chatRoom.ChatRoomId = newChatRoomId;
            ChatRooms.Add(chatRoom);

            return chatRoom;
        }

        public void StartListeningForNewUsersThread()
        {
            var updateClientsThread = new Thread(RefreshUserListForAllClients);
            updateClientsThread.Start();
        }

        //This would be better being replaced with a listener of some kind
        //But I can't work out how to get it to work yet...
        //This will do for now.
        //Observer pattern
        private void RefreshUserListForAllClients()
        {
            const long sleepTime = (TimeSpan.TicksPerSecond);
            var periodToSleep = new TimeSpan(sleepTime);

            while (true)
            {
                ChatRooms[0].CheckAllActiveUsersInRoom();

                Thread.Sleep(periodToSleep);
            }
        }
    }
}
