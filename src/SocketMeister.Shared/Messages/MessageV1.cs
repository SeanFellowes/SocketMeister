using System;
using System.IO;

namespace SocketMeister.Messages
{
#if !NET35
    internal partial class MessageV1 : MessageBase
    {
        private SocketServer.Client _remoteClient = null;

        /// <summary>
        /// The remote client which sent this Message (value null on SocketClient)
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (Lock) { return _remoteClient; } }
            set { lock (Lock) { _remoteClient = value; } }
        }
    }
#endif

    /// <summary>
    /// A message sent from a socket client to a socket server. A response is expected and will cause problems if it is not sent.
    /// </summary>
    internal partial class MessageV1 : MessageBase, IMessage
    {
        //  MESSAGE VARIABLES
        private readonly bool _booleanForFutureUse = false; // previously _isLongPolling; kept for binary compatibility (reserved for future use)
        private byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;

        /// <summary>
        /// Creates a message with the provided parameters and timeout.
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the message. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        /// <param name="FriendlyMessageName">Optional friendly name for the message, used in logging.</param>
        public MessageV1(object[] Parameters, int TimeoutMilliseconds, string FriendlyMessageName = null)
            : base(MessageType.MessageV1, 0, FriendlyMessageName)
        {
            base.TimeoutMilliseconds = TimeoutMilliseconds;
            _parameters = Parameters;
            _booleanForFutureUse = false; // reserved bit

            BuildPayload();
        }

        /// <summary>
        /// Obsolete. The boolean value is reserved for future use and not functionally used.
        /// Use <see cref="MessageV1(object[], int, string)"/> instead.
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the message. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        /// <param name="IsLongPolling">Reserved for future use. The value is serialized for backward/forward compatibility but not used.</param>
        /// <param name="FriendlyMessageName">Optional friendly name for the message, used in logging.</param>
        [Obsolete("Use MessageV1(object[] parameters, int timeoutMilliseconds, string friendlyMessageName = null) instead. The boolean parameter is reserved for future use and not used.")]
        public MessageV1(object[] Parameters, int TimeoutMilliseconds, bool IsLongPolling, string FriendlyMessageName = null)
            : this(Parameters, TimeoutMilliseconds, FriendlyMessageName)
        {
            // Maintain serialization compatibility by setting the reserved bit
            _booleanForFutureUse = IsLongPolling;
            BuildPayload();
        }

        internal MessageV1(BinaryReader bR)
            : base(MessageType.MessageV1, bR.ReadInt64(), nameof(MessageV1))
        {
            base.TimeoutMilliseconds = bR.ReadInt32();
            _booleanForFutureUse = bR.ReadBoolean(); // reserved for future use
            _parameters = Serializer.DeserializeParameters(bR);
        }

        /// <summary>
        /// Parameters provided with this message.
        /// </summary>
        public object[] Parameters => _parameters;

        /// <summary>
        /// Reserved boolean, kept for binary compatibility. Currently not used by the library.
        /// </summary>
        public bool BooleanForFutureUse => _booleanForFutureUse;

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }

        // Serializes the message fields into the internal byte buffer used for sending.
        private void BuildPayload()
        {
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(MessageId);
                writer.Write(base.TimeoutMilliseconds);
                writer.Write(_booleanForFutureUse);
                Serializer.SerializeParameters(writer, _parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    _parameterBytes = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }
    }
}