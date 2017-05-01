using System.Collections.Generic;

namespace WebSocketsServer.Interfaces
{
    public interface IChatRoom
    {
        int ChatRoomId { get; set; }
        List<User> Users { get; set; }

        void SendToAllUsers(byte[] decodedMessageBytesArray);

        void SendToAllExceptSpecificUser(byte[] decodedMessageBytesArray, User exceptionUser);

        void SendToSpecificUser(byte[] decodedMessageBytesArray, User user);

        void CheckAllActiveUsersInRoom();
    }
}
