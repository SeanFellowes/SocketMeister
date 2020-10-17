using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Server sends this back to the client after it processes a ClientHandshake.
    /// </summary>
    internal partial class ClientHandshakeResponse : MessageBase, IMessage
    {
        /// <summary>
        /// Increment this and add deserialization code when changing the serialized format.
        /// </summary>
        private const int SERIALIZER_VERSION = 1;

        private readonly int _socketServerVersion;

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        public ClientHandshakeResponse() : base(MessageTypes.ClientHandshakeResponse, SERIALIZER_VERSION)
        {
            _socketServerVersion = SocketServer.VERSION;
        }

        internal ClientHandshakeResponse(BinaryReader bR, int SerializationVersion) : base(MessageTypes.ClientHandshakeResponse, SerializationVersion)
        {
            if (SerializationVersion == 1)
            {
                _socketServerVersion = bR.ReadInt32();
            }
            else
            {
                throw new PlatformNotSupportedException("Deserializer does not exist for version " + base.SerializerVersion + " of message type " + nameof(ClientHandshake) + " in this version of this assembly (" + Assembly.GetExecutingAssembly().FullName + ")");
            }
        }


        /// <summary>
        /// Version of the client accessing this server. Developer must update this when client functionality changes.
        /// </summary>
        public int SocketServerVersion
        {
            get { return _socketServerVersion; }
        }



        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(base.SerializerVersion);
            Writer.Write(_socketServerVersion);
        }

    }
}
