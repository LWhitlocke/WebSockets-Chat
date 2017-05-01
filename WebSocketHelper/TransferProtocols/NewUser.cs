using System;

namespace WebSocketHelper.TransferProtocols
{
    [Serializable]
    public class NewUser
    {
        public string ResponseType
        {
            get { return "NewUser"; }
        }

        /// <summary>
        ///     If true from client then this means we need to generate a new username.
        ///     If true from server this means a new name has been supplied.
        /// </summary>
        public bool RequestName { get; set; }

        public bool NewMember { get; set; }
        public string Name { get; set; }
        public int ChatRoomId { get; set; }
    }
}