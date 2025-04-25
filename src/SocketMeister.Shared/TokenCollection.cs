using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SocketMeister
{
    /// <summary>
    /// A dictionary-based collection of tokens. The data in the collection is updatable.
    /// </summary>
    internal class TokenCollection
    {
        private bool _changed;
        private readonly Dictionary<string, Token> _dictTokens = new Dictionary<string, Token>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TokenChange> _dictTokenChanges = new Dictionary<string, TokenChange>();
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCollection"/> class.
        /// </summary>
        public TokenCollection()
        {
        }

        /// <summary>
        /// Provides indexed access to a token by its name. Returns the token or null if not found.
        /// </summary>
        /// <param name="Name">The name of the token to retrieve.</param>
        /// <returns>The token if found, or null if not found.</returns>
        public Token this[string Name]
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Name cannot be null or empty.", nameof(Name));

                lock (_lock)
                {
                    Token fnd;
                    _dictTokens.TryGetValue(Name.ToUpper(CultureInfo.InvariantCulture), out fnd);
                    return fnd;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether changes have been made to the token collection.
        /// </summary>
        public bool Changed
        {
            get { lock (_lock) { return _changed; } }
        }

        /// <summary>
        /// Gets the number of tokens in the token collection.
        /// </summary>
        public int Count { get { lock (_lock) { return _dictTokens.Count; } } }

        /// <summary>
        /// Adds a token to the token collection. Throws an <see cref="ArgumentException"/> if a token with the same name (case-insensitive) already exists.
        /// </summary>
        /// <param name="Token">The token to add.</param>
        internal void Add(Token Token)
        {
            if (Token == null) throw new ArgumentException("Token cannot be null.", nameof(Token));

            lock (_lock)
            {
                _dictTokens.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), Token);
                Token.Changed += Token_Changed;
                AddTokenChange(TokenAction.Add, Token);
            }
        }

        /// <summary>
        /// Adds a token change to the collection. If the token already exists, it will be replaced.
        /// This method must be called within a lock.
        /// </summary>
        /// <param name="Action">The action to perform (Add, Modify, or Delete).</param>
        /// <param name="Token">The token affected by the change.</param>
        /// <exception cref="ArgumentException">Thrown if the token is null.</exception>
        private void AddTokenChange(TokenAction Action, Token Token)
        {
            if (Token == null) throw new ArgumentException("Token cannot be null.", nameof(Token));

            TokenChange foundTokenChange;
            _dictTokenChanges.TryGetValue(Token.Name.ToUpper(CultureInfo.InvariantCulture), out foundTokenChange);

            // If the token has already been changed, remove it from the list.
            _dictTokenChanges.Remove(Token.Name.ToUpper(CultureInfo.InvariantCulture));

            // Add the token to the list.
            if (Action == TokenAction.Delete)
                _dictTokenChanges.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new TokenChange(Action, Token.Name, null));
            else
                _dictTokenChanges.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new TokenChange(Action, Token.Name, Token));

            _changed = true;
        }

        /// <summary>
        /// Serializes all tokens in the collection.
        /// </summary>
        /// <returns>A byte array containing the serialized tokens.</returns>
        internal byte[] SerializeTokens()
        {
            List<Token> alltokens = ToList();

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(alltokens.Count);

                foreach (Token t in alltokens)
                {
                    t.Serialize(writer); // Serialize the token.
                }

                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    return reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }

        /// <summary>
        /// Serializes the changes made to the tokens. If there are no changes, returns null.
        /// </summary>
        /// <returns>A byte array containing the serialized token changes, or null if there are no changes.</returns>
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
                        writer.Write(kvp.Key);                  // Token name.
                        writer.Write(kvp.Value.ChangeId);       // Change ID.
                        writer.Write((short)kvp.Value.Action);  // Action.

                        if (kvp.Value.Action == TokenAction.Add || kvp.Value.Action == TokenAction.Modify)
                        {
                            kvp.Value.Token.Serialize(writer);  // Serialize the token.
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
        /// Removes a token from the collection.
        /// </summary>
        /// <param name="Name">The name of the token to remove (case-insensitive).</param>
        /// <returns>The removed token, or null if no token was removed.</returns>
        public Token Remove(string Name)
        {
            if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Token name cannot be null or empty.", nameof(Name));

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
        /// Removes a token change from the collection if the token name exists and the ChangeId matches the current ChangeId of the change.
        /// If there are no more changes, the changed flag is set to false.
        /// </summary>
        /// <param name="TokenName">The name of the token.</param>
        /// <param name="ChangeId">The change identifier.</param>
        public void RemoveChange(string TokenName, int ChangeId)
        {
            lock (_lock)
            {
                TokenChange fnd;
                _dictTokenChanges.TryGetValue(TokenName.ToUpper(), out fnd);
                if (fnd != null && fnd.ChangeId == ChangeId) _dictTokenChanges.Remove(TokenName.ToUpper());

                // If there are no more changes, set the changed flag to false.
                if (_dictTokenChanges.Count == 0) _changed = false;
            }
        }

        /// <summary>
        /// Returns a list of all tokens in the collection.
        /// </summary>
        /// <returns>A list of all tokens.</returns>
        public List<Token> ToList()
        {
            lock (_lock)
            {
                return _dictTokens.Values.ToList();
            }
        }

        /// <summary>
        /// Returns a list of all token names in the collection.
        /// </summary>
        /// <returns>A list of strings containing the token names.</returns>
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

            // AddTokenChange must be called within a lock.
            lock (_lock)
            {
                AddTokenChange(TokenAction.Modify, t);
            }
        }
    }
}
