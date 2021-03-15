#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: COntains information related to changed tokens.
    /// </summary>
    internal class TokenChangesResponseV1 : MessageBase, IMessage
    {
        internal struct ChangeIdentifier
        {
            public ChangeIdentifier(string TokenName, int ChangeId)
            {
                this.TokenNameUppercase = TokenName.ToUpper(CultureInfo.InvariantCulture);
                this.ChangeId = ChangeId;
            }

            public int ChangeId { get; }
            public string TokenNameUppercase { get; }
        }

        private readonly List<ChangeIdentifier> _changes = new List<ChangeIdentifier>();

        public TokenChangesResponseV1(List<TokenChange> Changes) : base(InternalMessageType.SubscriptionChangesResponseV1)
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
        public TokenChangesResponseV1(BinaryReader Reader) : base(InternalMessageType.SubscriptionChangesResponseV1)
        {
            int changeCount = Reader.ReadInt32();
            _changes = new List<ChangeIdentifier>(changeCount);
            for (int ctr = 1; ctr <= changeCount; ctr++)
            {
                _changes.Add(new ChangeIdentifier(Reader.ReadString(), Reader.ReadInt32()));
            }
        }

        public List<ChangeIdentifier> ChangeIdentifiers { get { return _changes; } }

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_changes.Count);
            foreach(ChangeIdentifier i in _changes)
            {
                Writer.Write(i.TokenNameUppercase);
                Writer.Write(i.ChangeId);
            }
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression
