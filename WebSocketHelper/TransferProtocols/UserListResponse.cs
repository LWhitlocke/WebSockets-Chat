using System;
using System.Collections.Generic;

namespace WebSocketHelper.TransferProtocols
{
    public class UserListResponse
    {
        public string ResponseType
        {
            get { return "UserListResponse"; }
        }

        public IList<string> UsersList { get; set; }
        public DateTime UsersListGenerated { get; set; }
    }
}