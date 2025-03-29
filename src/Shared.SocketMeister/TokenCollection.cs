using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private readonly Dictionary<string, Token> _dictTokens = new Dictionary<string, Token>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TokenChange> _dictTokenChanges = new Dictionary<string, TokenChange>();
        private readonly object _lock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        public TokenCollection()
        {
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
                    _dictTokens.TryGetValue(Name.ToUpper(CultureInfo.InvariantCulture), out fnd);
                    return fnd;
                }
            }
        }


        /// <summary>
        /// Number of tokens in the token collection
        /// </summary>
        public int Count { get { lock (_lock) { return _dictTokens.Count; } } }


        /// <summary>
        /// Add a token to the token collection. Throws ArgumentException if Token.Name (case insensitive) already exists.
        /// </summary>
        /// <param name="Token">Token to add</param>
        internal void Add(Token Token)
        {
            if (Token == null) throw new ArgumentException("Token cannot be null", nameof(Token));

            lock (_lock)
            {
                _dictTokens.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), Token);
                Token.Changed += Token_Changed;
                AddTokenChange(TokenAction.Add, Token);
            }
            TokenAdded?.Invoke(Token, new EventArgs());
        }

        private void AddTokenChange(TokenAction Action, Token Token)
        {
            if (Token == null) throw new ArgumentException("Token cannot be null", nameof(Token));

            TokenChange foundTokenChange;
            lock (_lock)
            {
                _dictTokenChanges.TryGetValue(Token.Name.ToUpper(CultureInfo.InvariantCulture), out foundTokenChange);

                //  IF EXISTING TokenChange EXISTS, DELETE IT
                _dictTokenChanges.Remove(Token.Name.ToUpper(CultureInfo.InvariantCulture));

                //  ADD IT BACK IN
                if (Action == TokenAction.Delete)
                    _dictTokenChanges.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new TokenChange(Action, Token.Name, null));
                else
                    _dictTokenChanges.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new TokenChange(Action, Token.Name, Token));
            }
        }

        /// <summary>
        /// Serializes all tokens
        /// </summary>
        /// <returns>byte array of the tokens in the collection</returns>
        internal byte[] SerializeTokens()
        {
            List<Token> alltokens = ToList();

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(alltokens.Count);

                foreach (Token t in alltokens)
                {
                    t.Serialize(writer);  //  TOKEN
                }

                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    return reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }


        /// <summary>
        /// Serializes token changes. If there are no changes, returns null.
        /// </summary>
        /// <returns>If there are no changes, returns null</returns>
        public byte[] SerializeTokenChanges()
        {
            lock (_lock)
            {
                if (_dictTokenChanges.Count == 0) return null;

                using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
                {
                    writer.Write(_dictTokenChanges.Count);

                    foreach (KeyValuePair<string, TokenChange> kvp in _dictTokenChanges)
                    {
                        writer.Write(kvp.Key);                  //  NAME

                        writer.Write(kvp.Value.ChangeId);       //  CHANGE ID
                        writer.Write((short)kvp.Value.Action);  //  ACTION

                        if (kvp.Value.Action == TokenAction.Add || kvp.Value.Action == TokenAction.Modify)
                        {
                            kvp.Value.Token.Serialize(writer);  //  TOKEN
                        }
                    }

                    using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                    {
                        reader.BaseStream.Position = 0;
                        return reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                    }
                }
            }
        }


        /// <summary>
        /// Processes a TokenChangesResponseV1 response from the server. 
        /// Delete the token changes contained in the TokenChangesResponseV1 response from the server
        /// because the server has accepted the changes.
        /// </summary>
        /// <param name="Response">THe response returned to the client from the server</param>
        internal void ImportTokenChangesResponseV1(TokenChangesResponseV1 Response)
        {
            lock (_lock)
            {
                foreach (TokenChangesResponseV1.ChangeIdentifier i in Response.ChangeIdentifiers)
                {
                    TokenChange fnd;
                    _dictTokenChanges.TryGetValue(i.TokenNameUppercase, out fnd);
                    if (fnd != null && fnd.ChangeId == i.ChangeId) _dictTokenChanges.Remove(i.TokenNameUppercase);
                }
            }
        }


        /// <summary>
        /// Removes a token from the dictionary
        /// </summary>
        /// <param name="Name">Name of the token (Case insensitive)</param>
        /// <returns>The token which was removed (Null if nothing removed)</returns>
        public Token Remove(string Name)
        {
            if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Token name cannot be null or empty", nameof(Name));

            Token fnd = null;
            lock (_lock)
            {
                _dictTokens.TryGetValue(Name.ToUpper(CultureInfo.InvariantCulture), out fnd);
            }

            if (fnd != null)
            {
                fnd.Changed -= Token_Changed;

                lock (_lock)
                {
                    _dictTokens.Remove(Name.ToUpper(CultureInfo.InvariantCulture));
                }
                AddTokenChange(TokenAction.Delete, fnd);
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
                return _dictTokens.Values.ToList();
            }
        }

        /// <summary>
        /// Returns a list of token names
        /// </summary>
        /// <returns>List of strings containing the token names.</returns>
        public List<string> GetNames()
        {
            lock (_lock)
            {
                return _dictTokens.Values.Select(t => t.Name).ToList();
            }
        }


        private void Token_Changed(object sender, EventArgs e)
        {
            Token t = (Token)sender;
            AddTokenChange(TokenAction.Modify, t);
            TokenChanged?.Invoke(sender, e);
        }
    }
}
