using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SocketMeister
{
    internal class TokenChange
    {
        private static int _maxTokenChangeId = 0;
        private static readonly object _lockMaxTokenChangeId = new object();

        public int ChangeId { get; }
        public TokenAction Action { get; }
        public Token Token { get; }
        public string TokenNameUppercase { get; }

        public TokenChange(TokenAction Action, string TokenName, Token Token)
        {
            if (string.IsNullOrEmpty(TokenName) == true) throw new ArgumentNullException(nameof(TokenName));

            //  GENERATE NEXT ChangeId
            lock (_lockMaxTokenChangeId)
            {
                _maxTokenChangeId++;
                if (_maxTokenChangeId == int.MaxValue) _maxTokenChangeId = 1;
                ChangeId = _maxTokenChangeId;
            }

            this.Action = Action;
            this.TokenNameUppercase = TokenName.ToUpper(CultureInfo.InvariantCulture);
            this.Token = Token;
        }

        public TokenChange(int ChangeId, TokenAction Action, string TokenName, Token Token)
        {
            if (string.IsNullOrEmpty(TokenName) == true) throw new ArgumentNullException(nameof(TokenName));

            this.ChangeId = ChangeId;
            this.Action = Action;
            this.TokenNameUppercase = TokenName.ToUpper(CultureInfo.InvariantCulture);
            this.Token = Token;
        }

    }

}
