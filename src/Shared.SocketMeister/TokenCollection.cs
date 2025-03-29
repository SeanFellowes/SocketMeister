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
        private bool _changed;
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
        /// Whether changes have been made to the token collection
        /// </summary>
        public bool Changed
        {
            get { lock (_lock) { return _changed; } }
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
        }

        /// <summary>
        /// Adds a token to the collection. If the token already exists, it will be replaced.
        /// This must be called within a lock.
        /// </summary>
        /// <param name="Action">Add/Modify/Delete</param>
        /// <param name="Token">Token affected</param>
        /// <exception cref="ArgumentException">Token must not be null</exception>
        private void AddTokenChange(TokenAction Action, Token Token)
        {
            if (Token == null) throw new ArgumentException("Token cannot be null", nameof(Token));

            TokenChange foundTokenChange;
            _dictTokenChanges.TryGetValue(Token.Name.ToUpper(CultureInfo.InvariantCulture), out foundTokenChange);

            //  If the token has already been changed, remove it from the list
            _dictTokenChanges.Remove(Token.Name.ToUpper(CultureInfo.InvariantCulture));

            //  Add the token to the list
            if (Action == TokenAction.Delete)
                _dictTokenChanges.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new TokenChange(Action, Token.Name, null));
            else
                _dictTokenChanges.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new TokenChange(Action, Token.Name, Token));

            _changed = true;
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
                    AddTokenChange(TokenAction.Delete, fnd);
                }
            }
            return fnd;
        }


        /// <summary>
        /// Removes a token change from the collection of the token name exists and the ChangeId 
        /// matches the current ChangeId of the change. 
        /// If there are no more changes, the changed flag is set to false.
        /// </summary>
        /// <param name="TokenName">Name property of the token</param>
        /// <param name="ChangeId">Change identifyer</param>
        public void RemoveChange(string TokenName, int ChangeId)
        {
            lock (_lock)
            {
                TokenChange fnd;
                _dictTokenChanges.TryGetValue(TokenName.ToUpper(), out fnd);
                if (fnd != null && fnd.ChangeId == ChangeId) _dictTokenChanges.Remove(TokenName.ToUpper());

                //  If there are no more changes, set the changed flag to false
                if (_dictTokenChanges.Count == 0) _changed = false;
            }
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

            //  AddTokenCHange must be called within a lock
            lock (_lock)
            {
                AddTokenChange(TokenAction.Modify, t);
            }
        }
    }
}
