using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Response when tokens are changed. Contains ChangeIds of token changes.
    /// </summary>
    internal class TokenChangesRequestV1 : MessageBase, IMessage
    {
        private readonly byte[] _changeBytes;

        public TokenChangesRequestV1(byte[] ChangeBytes) : base(MessageEngineMessageType.SubscriptionChangesNotificationV1) 
        {
            _changeBytes = ChangeBytes;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public TokenChangesRequestV1(BinaryReader Reader) : base(MessageEngineMessageType.SubscriptionChangesNotificationV1)
        {
            _changeBytes = Reader.ReadBytes(Reader.ReadInt32());
        }

        public byte[] ChangeBytes {  get { return _changeBytes; } }

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_changeBytes.Length);
            Writer.Write(_changeBytes);
        }
    }
}
