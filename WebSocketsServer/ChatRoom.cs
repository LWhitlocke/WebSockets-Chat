using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebSocketHelper.TransferProtocols;
using WebSocketsServer.Interfaces;

namespace WebSocketsServer
{
    public class ChatRoom : IChatRoom
    {
        public List<User> Users { get; set; }
        public int ChatRoomId { get; set; }
        private Server _server { get; set; }

        private readonly MessageHandler _messageHandler;
        
        public ChatRoom(Server server)
        {
            Users = new List<User>();
            _messageHandler = new MessageHandler();
            _server = server;
            var rnd = new Random();
            ChatRoomId = rnd.Next(1000000000, 2147483647);
        }

        public void SendToAllUsers(byte[] decodedMessageBytesArray)
        {
            foreach (var user in Users)
            {
                SendToSpecificUser(decodedMessageBytesArray, user);
            }

            RemoveInvalidUsers();
        }

        public void SendToAllExceptSpecificUser(byte[] decodedMessageBytesArray, User exceptionUser)
        {
            foreach (var user in Users.Where(user => user.UserName != exceptionUser.UserName))
            {
                SendToSpecificUser(decodedMessageBytesArray, user);
            }

            RemoveInvalidUsers();
        }

        private void RemoveInvalidUsers()
        {
            while (Users.Any(u => u.IsUserValid == false))
            {
                var invalidUser = Users.First(u => u.IsUserValid == false);
                Users.Remove(invalidUser);

                var sr = new SystemResponse
                {
                    Message = invalidUser.UserName + " has left the chat."
                };
                var encodedResponse = _messageHandler.ObjectToByteArray(sr);
                _server.ChatRooms[Server.BaseChatRoomId].SendToAllUsers(encodedResponse);
            }

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

        public void CheckAllActiveUsersInRoom()
        {
            var ulResponse = new UserListResponse
            {
                UsersList = Users.Select(user => user.UserName).ToList(),
                UsersListGenerated = DateTime.Now
            };

            var userListBytes = _messageHandler.ObjectToByteArray(ulResponse);

            SendToAllUsers(userListBytes);
        }
    }
}