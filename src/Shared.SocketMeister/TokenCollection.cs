using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace SocketMeister
{
    /// <summary>
    /// Dictionary based collection of tokens. Data is updatable.
    /// </summary>
#if SMISPUBLIC
    public class TokenCollection
#else
    internal class TokenCollection
#endif
    {
        private readonly TokenChangeCollection _changes;
        private readonly Dictionary<string, Token> _dict = new Dictionary<string, Token>();
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        public TokenCollection()
        {
            _changes = new TokenChangeCollection(this);
        }

        /// <summary>
        /// Raised when a token value has added (Token is the source).
        /// </summary>
        public event EventHandler<EventArgs> TokenAdded;

        /// <summary>
        /// Raised when a token value has changed (Token is the source).
        /// </summary>
        public event EventHandler<EventArgs> TokenChanged;

        /// <summary>
        /// Raised when a token value was deleted (Token is the source).
        /// </summary>
        public event EventHandler<EventArgs> TokenDeleted;

        /// <summary>
        /// Indexed search returning the token or null, for a given token name.
        /// </summary>
        /// <param name="Name">Token requested.</param>
        /// <returns>Found token or null if not found</returns>
        public Token this[string Name]
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Name cannot be null or empty", nameof(Name));

                lock (_lock)
                {
                    Token fnd;
                    _dict.TryGetValue(Name.ToUpper(CultureInfo.InvariantCulture), out fnd);
                    return fnd;
                }
            }
        }


        /// <summary>
        /// Number of tokens in the token collection
        /// </summary>
        public int Count { get { lock (_lock) { return _dict.Count; } } }


        /// <summary>
        /// Add a token to the token collection. Throws ArgumentException if Token.Name (case insensitive) already exists.
        /// </summary>
        /// <param name="Token">Token to add</param>
        internal void Add(Token Token)
        {
            if (Token == null) throw new ArgumentException("Token cannot be null", nameof(Token));

            lock (_lock)
            {
                _dict.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), Token);
                Token.Changed += Token_Changed;
            }
            TokenAdded?.Invoke(Token, new EventArgs());
        }


        internal void Clear()
        {
            List<Token> deletedTokens = null;
            lock (_lock)
            {
                if (TokenDeleted != null)
                {
                    deletedTokens = new List<Token>(_dict.Values);
                }

                foreach (var token in _dict.Values)
                {
                    token.Changed -= Token_Changed;
                }

                _dict.Clear();
            }

            //  if there is an event listener attached, send TokenDeleted events.
            if (deletedTokens != null)
            {
                foreach (Token t in deletedTokens)
                {
                    TokenDeleted?.Invoke(t, new EventArgs());
                }
            }
        }

        /// <summary>
        /// After a socket connects, all tokens are sent to the other side.
        /// </summary>
        internal void FlagAllAfterSocketConnect()
        {
            _changes.FlagAllAfterSocketConnect();
        }

        internal byte[] GetChangeBytes()
        {
            return _changes.Serialize();
        }


        internal void ImportTokenChangesResponseV1(TokenChangesResponseV1 Response)
        {
            _changes.ImportTokenChangesResponseV1(Response);
        }


        /// <summary>
        /// Removes a token from the dictionary
        /// </summary>
        /// <param name="Name">Name of the token (Case insensitive)</param>
        /// <returns>The token which was removed (Null if nothing removed)</returns>
        public Token Remove(string Name)
        {
            if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Name cannot be null or empty", nameof(Name));

            Token fnd = null;
            lock (_lock)
            {
                _dict.TryGetValue(Name.ToUpper(CultureInfo.InvariantCulture), out fnd);
            }

            if (fnd != null)
            {
                fnd.Changed -= Token_Changed;

                lock (_lock)
                {
                    _dict.Remove(Name.ToUpper(CultureInfo.InvariantCulture));
                }
                TokenDeleted?.Invoke(fnd, new EventArgs());
            }
            return fnd;
        }

        /// <summary>
        /// Returns a list of all the tokens in the collection
        /// </summary>
        /// <returns></returns>
        public List<Token> ToList()
        {
            lock (_lock)
            {
                return _dict.Values.ToList();
            }
        }

        /// <summary>
        /// Returns a list of token names
        /// </summary>
        /// <returns>List of strings containing the token names.</returns>
        public List<string> ToListOfStrings()
        {
            lock (_lock)
            {
                return _dict.Values.Select(t => t.Name).ToList();
            }
        }


        private void Token_Changed(object sender, EventArgs e)
        {
            TokenChanged?.Invoke(sender, e);
        }
    }
}
