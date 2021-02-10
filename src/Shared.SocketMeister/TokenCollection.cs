using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
        private readonly TokenChanges _changes;
        private readonly Dictionary<string, Token> _dict = new Dictionary<string, Token>();
        private readonly object _lock = new object();

        public TokenCollection()
        {
            _changes = new TokenChanges(this);
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
        /// Token Changes
        /// </summary>
        internal TokenChanges Changes {  get { return _changes; } }

        /// <summary>
        /// Number of tokens in the token collection
        /// </summary>
        public int Count {  get { lock (_lock) { return _dict.Count; } } }

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

                TokenAdded?.Invoke(Token, new EventArgs());
            }
        }

        /// <summary>
        /// Clears all tokens
        /// </summary>
        internal void Clear()
        {
            lock (_lock)
            {
                _dict.Clear();
            }
        }

        internal byte[] GetChangeBytes()
        {
            return _changes.Serialize();
        }

        public void ImportTokenChangesResponseV1(byte[] changeResponseBytes)
        {
            List<TokenChanges.Change> tokenChanges = TokenChanges.DeserializeTokenChanges(changeResponseBytes);

            //  IMPORT CHANGES
            foreach (TokenChanges.Change change in tokenChanges)
            {
                Token fnd = null;
                lock (_lock) { _dict.TryGetValue(change.Token.Name, out fnd); }

                if (change.Action == TokenAction.Delete)
                {
                    if (fnd != null)
                    {
                        lock (_lock) { _dict.Remove(change.Token.Name); }
                        change.Token.Changed -= Token_Changed;
                        if (TokenDeleted != null) TokenDeleted(fnd, new EventArgs());
                    }
                }
                else
                {
                    if (fnd == null)
                    {
                        lock (_lock) { _dict.Add(change.Token.Name, change.Token); }
                        change.Token.Changed += Token_Changed;
                        if (TokenAdded != null) TokenAdded(fnd, new EventArgs());
                    }
                    else
                    {
                        fnd.Value = change.Token.Value;
                    }
                }
            }
        }



        /// <summary>
        /// Removes a token from the dictionary
        /// </summary>
        /// <param name="Name">Name of the token (Case insensitive)</param>
        /// <returns>The token which was removed (Null if nothing removed)</returns>
        internal Token Remove(string Name)
        {
            if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Name cannot be null or empty", nameof(Name));

            lock (_lock)
            {
                Token fnd;
                _dict.TryGetValue(Name.ToUpper(CultureInfo.InvariantCulture), out fnd);
                if (fnd == null) return null;

                fnd.Changed -= Token_Changed;

                TokenDeleted?.Invoke(fnd, new EventArgs());

                _dict.Remove(Name.ToUpper(CultureInfo.InvariantCulture));
                return fnd;
            }
        }

        /// <summary>
        /// Returns a list of all the tokens in the collection
        /// </summary>
        /// <returns></returns>
        public List<Token> ToList()
        {
            List<Token> rVal = new List<Token>();
            lock(_lock)
            {
                foreach (KeyValuePair<string , Token> kvp in _dict)
                {
                    rVal.Add(kvp.Value);
                }
                return rVal;
            }
        }

        private void Token_Changed(object sender, EventArgs e)
        {
            TokenChanged?.Invoke(sender, e);
        }
    }
}
