using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: COntains information related to changed tokens.
    /// </summary>
    internal class TokenChangesResponseV1 : MessageBase, IMessage
    {
        private readonly List<int> _changeIdentifiers = new List<int>();

        public TokenChangesResponseV1(List<int> ChangeIdentifiers) : base(MessageTypes.SubscriptionChangesResponseV1) 
        {
            _changeIdentifiers = new List<int>(ChangeIdentifiers.Count);

            //  CREATE A LOCAL COPY OF THE LIST
            foreach (int i in ChangeIdentifiers)
            {
                _changeIdentifiers.Add(i);
            }
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public TokenChangesResponseV1(BinaryReader Reader) : base(MessageTypes.SubscriptionChangesResponseV1)
        {
            int changeCount = Reader.ReadInt32();
            _changeIdentifiers = new List<int>(changeCount);
            for (int ctr = 1; ctr <= changeCount; ctr++)
            {
                _changeIdentifiers.Add(Reader.ReadInt32());
            }
        }

        public List<int> ChangeIdentifiers { get { return _changeIdentifiers; } }

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_changeIdentifiers.Count);
            foreach(int i in _changeIdentifiers)
            {
                Writer.Write(i);
            }
        }
    }
}
