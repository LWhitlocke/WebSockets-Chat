using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using WebSocketHelper.TransferProtocols;

namespace WebSocketsServer
{
    //Useful references
    //http://stackoverflow.com/questions/8125507/how-can-i-send-and-receive-websocket-messages-on-the-server-side
    //http://www.html5rocks.com/en/tutorials/websockets/basics/
    //http://tools.ietf.org/html/rfc6455#section-5.2
    //https://developer.mozilla.org/en-US/docs/WebSockets/Writing_WebSocket_server

    internal class Client
    {
        private readonly MessageHandler _messageHandler;
        private readonly User _user;
        private readonly Server _server;

        public Client(User user, Server server)
        {
            _messageHandler = new MessageHandler();
            _server = server;
            _user = user;
            HandleNewlyConnectedUser();
        }

        private void HandleNewlyConnectedUser()
        {
            Console.WriteLine("A client connected.");

            using (var stream = _user.Client.GetStream())
            {
                while (_user.Client.Connected)
                {
                    //Wait until we receive more data from the client to work with
                    try
                    {
                        while (!stream.DataAvailable)
                        {
                        }
                    }
                    catch (ObjectDisposedException ex)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                    Console.WriteLine(_user.Client.Available.ToString());

                    var convertedByteStream = ConvertByteStreamToString(stream);
                    if (convertedByteStream.Equals(string.Empty)) continue;

                    var serializer = new JavaScriptSerializer();
                    string responseType;

                    try
                    {
                        var requestObjects = (Dictionary<string, object>)serializer.Deserialize<object>(convertedByteStream);
                        responseType = requestObjects.FirstOrDefault(r => r.Key == "ResponseType").Value.ToString();
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Error processing request from user.");
                        var temp = _user.Client.Available;
                        Console.WriteLine("Awaiting " + temp + " data from client in error.");
                        Console.WriteLine("");
                        continue;
                    }

                    //Below is a prime candidate for strategy pattern
                    switch (responseType)
                    {
                        case ("NewChatRoomRequest"):
                            {
                                var newChatRoomResponse = serializer.Deserialize<NewChatRoomRequest>(convertedByteStream);

                                var chatRoom = _server.SetupNewChatRoom();
                                chatRoom.Users.Add(_user);

                                var returnResponse = new NewChatRoomResponse()
                                {
                                    ChatRoomId = chatRoom.ChatRoomId
                                };

                                var encodedResponse = _messageHandler.ObjectToByteArray(returnResponse);
                                _server.ChatRooms[Server.BaseChatRoomId].SendToSpecificUser(encodedResponse, _user);

                                break;
                            }
                        case ("JoinChatRoomRequest"):
                            {
                                var joinChatRoomRequest = serializer.Deserialize<JoinChatRoomRequest>(convertedByteStream);

                                if (!_server.ChatRooms[joinChatRoomRequest.ChatRoomToJoinId].Users.Contains(_user))
                                {
                                    _server.ChatRooms[joinChatRoomRequest.ChatRoomToJoinId].Users.Add(_user);

                                    var sr = new SystemResponse
                                    {
                                        Message = _user.UserName + " has joined the chat."
                                    };

                                    var encodedResponse = _messageHandler.ObjectToByteArray(sr);
                                    _server.ChatRooms[joinChatRoomRequest.ChatRoomToJoinId].SendToAllUsers(encodedResponse);
                                }
                                else
                                {
                                    var sr = new SystemResponse
                                    {
                                        Message = _user.UserName + " is already a member of this room."
                                    };

                                    var encodedResponse = _messageHandler.ObjectToByteArray(sr);
                                    _server.ChatRooms[joinChatRoomRequest.ChatRoomToJoinId].SendToSpecificUser(encodedResponse, _user);
                                }

                                break;
                            }
                        case ("UserListResponse"):
                            {
                                var userListResponse = serializer.Deserialize<UserListResponse>(convertedByteStream);

                                break;
                            }
                        case ("SystemResponse"):
                            {
                                var systemResponse = serializer.Deserialize<SystemResponse>(convertedByteStream);

                                break;
                            }
                        case ("Ping"):
                            {
                                var ping = serializer.Deserialize<Ping>(convertedByteStream);
                                ProcessPingRequest(ping);
                                break;
                            }
                        case ("MessageResponse"):
                            {
                                var request = serializer.Deserialize<MessageResponse>(convertedByteStream);
                                ProcessMessageRequest(request);
                                break;
                            }
                        case ("NewUser"):
                            {
                                var request = serializer.Deserialize<NewUser>(convertedByteStream);
                                ProcessNewUserRequest(request);
                                break;
                            }
                    }
                }
            }
        }

        private string ConvertByteStreamToString(NetworkStream stream)
        {
            var bytes = GetFullByteSteamFromClient(stream);

            var data = Encoding.UTF8.GetString(bytes);

            if (new Regex("^GET").IsMatch(data))
            {
                Handshake(data, stream);
                return string.Empty;
            }

            var decodedBytes = MessageHandler.DecodeMessage(bytes);
            var legableOutput = Encoding.UTF8.GetString(decodedBytes);

            MessageHandler.ReplaceUnsafeCharactersWithEscapedSet(ref legableOutput);

            var fullLegableOutput = CheckExpectedMessageLengthVsRecievedMessageLength(legableOutput);
            return fullLegableOutput;
        }

        private string CheckExpectedMessageLengthVsRecievedMessageLength(string legableOutput)
        {
            var returnValue = legableOutput;

            if (legableOutput.IndexOf("|-|", StringComparison.Ordinal) <= 0) return returnValue;
            var positionOfTieFigher = legableOutput.IndexOf("|-|", StringComparison.Ordinal);
            int actualMessageLength;
            int.TryParse(legableOutput.Substring(0, positionOfTieFigher), out actualMessageLength);

            var startIndex = actualMessageLength.ToString().Length + 3;
            var processedMessageLength = legableOutput.Length - startIndex;

            Console.WriteLine("if " + processedMessageLength + " is less than " + actualMessageLength);

            if (processedMessageLength < actualMessageLength)
            {
                AwaitFurtherDataFromClientAndAppendToExistingArray();
            }

            returnValue = legableOutput.Substring(startIndex, processedMessageLength);
            return returnValue;
        }

        private void AwaitFurtherDataFromClientAndAppendToExistingArray()
        {
            //we need more data
            var temp = _user.Client.Available;
            Console.WriteLine("Awaiting " + temp + " data from client in specified location.");

            while (temp == 0)
            {
                temp = _user.Client.Available;
                Console.WriteLine("Pending further data...");
                Thread.Sleep(250);
            }

            //Not yet implemented
        }

        private byte[] GetFullByteSteamFromClient(Stream stream)
        {
            const int byteReadingChunk = 2048;
            var buffer = new byte[byteReadingChunk]; //2048 read in chunks of 2KB
            var availableDataLength = _user.Client.Available;
            var bytes = new byte[availableDataLength];
            var initalDataAvailable = availableDataLength;
            var remainingDataAvailable = initalDataAvailable;
            var totalDataInserted = 0;

            while (stream.Read(buffer, 0, buffer.Length) > 0)
            {
                var insertByteAmount = remainingDataAvailable > byteReadingChunk ? byteReadingChunk : remainingDataAvailable;

                var offSet = totalDataInserted;

                if ((offSet + insertByteAmount) > bytes.Length)
                {
                    Array.Resize(ref bytes, (offSet + insertByteAmount));
                }

                bytes = CombineMaintainingArray0Size(bytes, buffer, offSet, insertByteAmount);

                remainingDataAvailable -= insertByteAmount;
                totalDataInserted += insertByteAmount;

                if (remainingDataAvailable > 0)
                {
                    continue;
                }

                //Check if there has become any more data available while processing
                if (_user.Client.Available > 0)
                {
                    var newAvailableData = _user.Client.Available;
                    remainingDataAvailable = newAvailableData;
                    continue;
                }

                break;
            }
            return bytes;
        }

        private static byte[] CombineMaintainingArray0Size(byte[] array, byte[] buffer, int offset, int insertByteAmount)
        {
            byte[] ret = array;

            Buffer.BlockCopy(buffer, 0, ret, offset, insertByteAmount);

            return ret;
        }

        private void ProcessNewUserRequest(NewUser request)
        {
            if (!request.RequestName) return;

            //This method doesn't account for the eventuality of duplicate usernames being assigned.
            var rnd = new Random((int)DateTime.Now.Ticks);
            var username = "User" + rnd.Next(0, 999999);
            _user.UserName = username;

            //Send response to user who requested this
            var returnResponse = new NewUser
            {
                Name = username,
                RequestName = true,
                ChatRoomId = Server.BaseChatRoomId
            };
            var encodedResponse = _messageHandler.ObjectToByteArray(returnResponse);
            _server.ChatRooms[Server.BaseChatRoomId].SendToSpecificUser(encodedResponse, _user);

            //Send response to rest of the room
            returnResponse = new NewUser
            {
                Name = username,
                NewMember = true
            };
            encodedResponse = _messageHandler.ObjectToByteArray(returnResponse);
            _server.ChatRooms[Server.BaseChatRoomId].SendToAllUsers(encodedResponse);
        }

        private void ProcessMessageRequest(MessageResponse request)
        {
            if (request.Name == null || request.Message == null) return;

            var mResponse = new MessageResponse
            {
                Name = request.Name,
                Message = request.Message,
                ChatRoomId = request.ChatRoomId
            };
            var encodedResponse = _messageHandler.ObjectToByteArray(mResponse);

            var chatRoom = _server.ChatRooms.FirstOrDefault(x => x.ChatRoomId == mResponse.ChatRoomId);
            chatRoom?.SendToAllUsers(encodedResponse);
        }

        private void ProcessPingRequest(Ping request)
        {
            byte[] encodedResponse;

            switch (request.RequestType)
            {
                case "Ping":
                    var ping = new Ping { RequestType = "Ping" };
                    encodedResponse = _messageHandler.ObjectToByteArray(ping);
                    _server.ChatRooms[Server.BaseChatRoomId].SendToSpecificUser(encodedResponse, _user);
                    break;
                case "Kill":
                    //Remove user from list
                    _server.ChatRooms[Server.BaseChatRoomId].Users.Remove(_user);
                    _user.Client.Close();

                    //Send response to rest of room informing of user disconnect.
                    //If we implement a listen to replace CheckAllActiveUsersInRoom()
                    //We can remove this call from here

                    var sr = new SystemResponse
                    {
                        Message = _user.UserName + " has left the chat."
                    };
                    encodedResponse = _messageHandler.ObjectToByteArray(sr);
                    _server.ChatRooms[Server.BaseChatRoomId].SendToAllUsers(encodedResponse);
                    break;
            }
        }

        private static void Handshake(string data, Stream stream)
        {
            Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                                                     + "Connection: Upgrade" + Environment.NewLine
                                                     + "Upgrade: websocket" + Environment.NewLine
                                                     + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                                                         SHA1.Create().ComputeHash(
                                                             Encoding.UTF8.GetBytes(
                                                                 new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups
                                                                     [1]
                                                                     .Value.Trim() +
                                                                 "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                                                 )
                                                             )
                                                         ) + Environment.NewLine
                                                     + Environment.NewLine);

            stream.Write(response, 0, response.Length);
        }
    }
}