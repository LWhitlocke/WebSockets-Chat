using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace WebSocketsServer
{
    public class MessageHandler
    {
        private static readonly Dictionary<string, string> CharactersToEscape = new Dictionary<string, string> 
        {
            {"%","&#37;"}
        };

        public static void ReplaceUnsafeCharactersWithEscapedSet(ref string message)
        {
            foreach (var entry in CharactersToEscape){
                message = message.Replace(entry.Key, entry.Value);
            }
        }

        public byte[] ObjectToByteArray(object request)
        {
            var json = new JavaScriptSerializer().Serialize(request);
            var encoding = new UTF8Encoding();
            var encodedResponse = encoding.GetBytes(json);
            return encodedResponse;
        }

        public static byte[] DecodeMessage(IList<byte> bytes)
        {
            //Short messages need bytes[1] to calculate length of content
            //Medium messages bytes[3] is the length of the content
            //Large messages need bytes???? Maybe bytes[5]

            int initalLengthOfMessageByteValue = bytes[1];

            var lengthOfMessage = CalculateLengthOfMessage(bytes);
            var startKeyIndex = CalculateKeyStartIndex(initalLengthOfMessageByteValue);

            var encoded = new Byte[lengthOfMessage];
            var key = new Byte[4];

            FillEncodedBytes(bytes, lengthOfMessage, encoded);
            FillKeyValue(bytes, startKeyIndex, key);

            var decoded = new Byte[lengthOfMessage];
            for (var i = 0; i < encoded.Length; i++)
            {
                decoded[i] = (Byte) (encoded[i] ^ key[i%4]);
            }
            return decoded;
        }

        public void BuildReturningBytesResponseForStream(ref byte[] returningBytes, byte[] decoded)
        {
            //Return pattern is as below
            //one byte which contains the type of data (and some additional info which is out of scope for a trivial server)
            //one byte which contains the length
            //either two or eight bytes if the length does not fit in the second byte (the second byte is then a code saying how many bytes are used for the length)
            //the actual (raw) data

            var indexStartRawData = -1;
            returningBytes[0] = 129;

            if (decoded.Length <= 125)
            {
                returningBytes[1] = (byte) decoded.Length;
                indexStartRawData = 2;
                Array.Resize(ref returningBytes, (decoded.Length + 2));
            }
            else if (decoded.Length >= 126 && decoded.Length <= 65535)
            {
                //Code copied from site, totally unsure what's going on.
                returningBytes[1] = 126;
                returningBytes[2] = (byte) ((decoded.Length >> 8) & 255);
                returningBytes[3] = (byte) (decoded.Length & 255);
                indexStartRawData = 4;
                Array.Resize(ref returningBytes, (decoded.Length + 4));
            }
            else
            {
                //Code copied from site, totally unsure what's going on.
                // '>>' is "right-shift operator"
                returningBytes[1] = 127;
                returningBytes[2] = (byte) ((decoded.Length >> 56) & 255);
                returningBytes[3] = (byte) ((decoded.Length >> 48) & 255);
                returningBytes[4] = (byte) ((decoded.Length >> 40) & 255);
                returningBytes[5] = (byte) ((decoded.Length >> 32) & 255);
                returningBytes[6] = (byte) ((decoded.Length >> 24) & 255);
                returningBytes[7] = (byte) ((decoded.Length >> 16) & 255);
                returningBytes[8] = (byte) ((decoded.Length >> 8) & 255);
                returningBytes[9] = (byte) (decoded.Length & 255);
                indexStartRawData = 10;
                Array.Resize(ref returningBytes, (decoded.Length + 10));
            }

            //Add the decoded message to the returning response
            decoded.CopyTo(returningBytes, indexStartRawData);
        }

        private static int CalculateLengthOfMessage(IList<byte> bytes)
        {
            //Unsure if case 127 is required anymore...
            var payloadSize = (int)(byte) (bytes[1] & 0x7F);

            switch (payloadSize)
            {
                case 126:
                {
                    //This now works for byte arrays up to 1460 in length
                    payloadSize = (bytes.Count - 8);
                    break;
                }
                case 127:
                {
                    //We have never entered this situation yet...
                    throw new Exception("Case 127 not implemented.");
                    break;
                }
            }

            return payloadSize;
        }

        private static int CalculateKeyStartIndex(int initalLengthOfMessageByteValue)
        {
            var lengthOfMessage = initalLengthOfMessageByteValue - 128;
            var startKeyIndex = 2;

            switch (lengthOfMessage)
            {
                case 126:
                {
                    startKeyIndex = 4;
                    break;
                }
                case 127:
                {
                    startKeyIndex = 10;
                    break;
                }
            }
            return startKeyIndex;
        }

        private static void FillEncodedBytes(IList<byte> bytes, int lengthOfMessage, IList<byte> encoded)
        {
            var encodedByteCount = 0;
            for (var i = (bytes.Count - lengthOfMessage); i < bytes.Count; i++)
            {
                encoded[encodedByteCount++] = bytes[i];
            }
        }

        private static void FillKeyValue(IList<byte> bytes, int startKeyIndex, IList<byte> key)
        {
            var encodedKeysCount = 0;

            for (var i = startKeyIndex; i < (startKeyIndex + 4); i++)
            {
                key[encodedKeysCount++] = bytes[i];
            }
        }
    }
}