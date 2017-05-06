using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using WebSocketHelper.TransferProtocols;
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

        private readonly MessageHandler _messageHandler;

        public Server()
        {
            ServerLocation = ConfigurationManager.AppSettings["location"];
            int parsedPort;
            int.TryParse(ConfigurationManager.AppSettings["port"], out parsedPort);
            Port = parsedPort;
            _chatRoomBuilder = new ChatRoomBuilder(this);
            _messageHandler = new MessageHandler();
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

        public void StartListeningProcesses()
        {
            var updateClientsThread = new Thread(RefreshUserListForAllClients);
            updateClientsThread.Start();
            var updateClientRoomsThread = new Thread(RefreshChatRoomListForAllClients);
            updateClientRoomsThread.Start();
        }

        public void SendToSpecificUser(byte[] decodedMessageBytesArray, User user)
        {
            //Ensure this client is still connected.
            if (!user.Client.Connected) return;
            var client = user.Client;

            //Can't put the below NetworkStream into a using block as this will close the stream
            var stream = client.GetStream();

            var value = decodedMessageBytesArray.Length;
            var returningBytes = new byte[value];

            _messageHandler.BuildReturningBytesResponseForStream(ref returningBytes, decodedMessageBytesArray);

            try
            {
                stream.Write(returningBytes, 0, returningBytes.Length);
            }
            catch (IOException ex)
            {
                user.Client.Close();
                user.IsUserValid = false;
            }
        }

        public void RemoveInvalidUsers(List<User> users, int chatRoomId)
        {
            while (users.Any(u => u.IsUserValid == false))
            {
                var invalidUser = users.First(u => u.IsUserValid == false);
                users.Remove(invalidUser);

                var sr = new SystemResponse
                {
                    Message = invalidUser.UserName + " has left the chat."
                };
                var encodedResponse = _messageHandler.ObjectToByteArray(sr);
                ChatRooms[chatRoomId].SendToAllUsers(encodedResponse);
            }
        }








        //These would be better being replaced with a listener of some kind
        //But I can't work out how to get it to work yet...
        //This will do for now.
        //Observer pattern
        private void RefreshUserListForAllClients()
        {
            const long sleepTime = (TimeSpan.TicksPerSecond);
            var periodToSleep = new TimeSpan(sleepTime);

            while (true)
            {
                foreach (var chatRoom in ChatRooms)
                {
                    chatRoom.CheckAllActiveUsersInRoom();
                }

                Thread.Sleep(periodToSleep);
            }
        }

        private void RefreshChatRoomListForAllClients()
        {
            const long sleepTime = (TimeSpan.TicksPerSecond);
            var periodToSleep = new TimeSpan(sleepTime);

            while (true)
            {
                var ulResponse = new ChatRoomListResponse()
                {
                    ChatRoomIds = ChatRooms.Select(room => room.ChatRoomId).ToList()
                };

                var userListBytes = _messageHandler.ObjectToByteArray(ulResponse);

                var allUsersPerRooms = ChatRooms.Select(x => x.Users).ToList();
                var allUsers = new List<User>();
                foreach (var allUsersPerRoom in allUsersPerRooms)
                {
                    allUsers.AddRange(allUsersPerRoom);
                }
                allUsers = allUsers.Distinct().ToList();

                foreach (var user in allUsers)
                {
                    SendToSpecificUser(userListBytes, user);
                }

                Thread.Sleep(periodToSleep);
            }
        }
    }
}
