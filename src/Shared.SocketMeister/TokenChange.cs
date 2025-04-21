using System;
using System.Globalization;

namespace SocketMeister
{
    /// <summary>
    /// Represents a change to a token, including the action performed, the token itself, and its name.
    /// </summary>
    internal class TokenChange
    {
        private static int _maxTokenChangeId;
        private static readonly object _lockMaxTokenChangeId = new object();

        public int ChangeId { get; }
        public TokenAction Action { get; }
        public Token Token { get; }
        public string TokenNameUppercase { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="TokenChange"/> class.
        /// </summary>
        /// <param name="Action"></param>
        /// <param name="TokenName"></param>
        /// <param name="Token"></param>
        /// <exception cref="ArgumentNullException"></exception>
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
            TokenNameUppercase = TokenName.ToUpper(CultureInfo.InvariantCulture);
            this.Token = Token;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TokenChange"/> class with a specified change ID.
        /// </summary>
        /// <param name="ChangeId"></param>
        /// <param name="Action"></param>
        /// <param name="TokenName"></param>
        /// <param name="Token"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TokenChange(int ChangeId, TokenAction Action, string TokenName, Token Token)
        {
            if (string.IsNullOrEmpty(TokenName) == true) throw new ArgumentNullException(nameof(TokenName));

            this.ChangeId = ChangeId;
            this.Action = Action;
            TokenNameUppercase = TokenName.ToUpper(CultureInfo.InvariantCulture);
            this.Token = Token;
        }
    }

}
