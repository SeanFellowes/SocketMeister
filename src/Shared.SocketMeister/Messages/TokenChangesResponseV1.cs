using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Contains information related to changed tokens.
    /// </summary>
    internal class TokenChangesResponseV1 : MessageBase, IMessage
    {
        internal readonly struct ChangeIdentifier
        {
            public ChangeIdentifier(string TokenName, int ChangeId)
            {
                TokenNameUppercase = TokenName.ToUpper(CultureInfo.InvariantCulture);
                this.ChangeId = ChangeId;
            }

            public int ChangeId { get; }
            public string TokenNameUppercase { get; }
        }

        private readonly List<ChangeIdentifier> _changes = new List<ChangeIdentifier>();

        public TokenChangesResponseV1(List<TokenChange> Changes) : base(MessageType.TokenChangesResponseV1, messageId: 0)
        {
            _changes = new List<ChangeIdentifier>(Changes.Count);

            //  CREATE A LOCAL COPY OF THE LIST
            foreach (TokenChange i in Changes)
            {
                _changes.Add(new ChangeIdentifier(i.TokenNameUppercase, i.ChangeId));
            }
        }


        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public TokenChangesResponseV1(BinaryReader Reader) : base(MessageType.TokenChangesResponseV1, messageId: 0)
        {
            int changeCount = Reader.ReadInt32();
            _changes = new List<ChangeIdentifier>(changeCount);
            for (int ctr = 1; ctr <= changeCount; ctr++)
            {
                _changes.Add(new ChangeIdentifier(Reader.ReadString(), Reader.ReadInt32()));
            }
        }

        public List<ChangeIdentifier> ChangeIdentifiers => _changes;

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_changes.Count);
            foreach (ChangeIdentifier i in _changes)
            {
                Writer.Write(i.TokenNameUppercase);
                Writer.Write(i.ChangeId);
            }
        }
    }
}

