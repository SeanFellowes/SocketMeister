using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Client sends this to server after it connects. It contains information about the client and version of the client software.
    /// </summary>
    internal partial class ClientHandshake : MessageBase, IMessage
    {
        /// <summary>
        /// Increment this and add deserialization code when changing the serialized format.
        /// </summary>
        private const int SERIALIZER_VERSION = 1;

        private readonly int _socketClientVersion;

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        public ClientHandshake() : base(MessageTypes.ClientHandshake, SERIALIZER_VERSION)
        {
            _socketClientVersion = SocketClient.VERSION;
        }


        internal ClientHandshake(BinaryReader bR, int SerializationVersion) : base(MessageTypes.ClientHandshake, SerializationVersion)
        {
            if (SerializationVersion == 1)
            {
                _socketClientVersion = bR.ReadInt32();
            }
            else
            {
                throw new PlatformNotSupportedException("Deserializer does not exist for version " + base.SerializerVersion + " of message type " + nameof(ClientHandshake) + " in this version of this assembly (" + Assembly.GetExecutingAssembly().FullName + ")");
            }

        }


        /// <summary>
        /// Version of the client accessing this server. Developer must update this when client functionality changes.
        /// </summary>
        public int SocketClientVersion
        {
            get { return _socketClientVersion; }
        }



        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(base.SerializerVersion);
            Writer.Write(_socketClientVersion);
        }


    }
}
