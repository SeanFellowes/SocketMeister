using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SocketMeister
{
    /// <summary>
    /// Dictionary based collection of tokens. Data is readonly.
    /// </summary>
#if SMISPUBLIC
    public class TokenCollectionReadOnly : IDisposable
#else
    internal class TokenCollectionReadOnly : IDisposable
#endif
    {
        private readonly Dictionary<string, Token> _dict = new Dictionary<string, Token>();
        private readonly object _lock = new object();
        private bool _disposed = false;

        /// <summary>
        /// Raised when a token value was added after synchronization with the master collection. Token is the 'Sender' in the event.
        /// </summary>
        public event EventHandler<EventArgs> TokenAdded;

        /// <summary>
        /// Raised when a token value was deleted after synchronization with the master collection. Token is the 'Sender' in the event.
        /// </summary>
        public event EventHandler<EventArgs> TokenChanged;

        /// <summary>
        /// Raised when a token value was changed after synchronization with the master collection. Token is the 'Sender' in the event.
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
                string key = Name.ToUpper(CultureInfo.InvariantCulture);

                lock (_lock)
                {
                    return _dict.TryGetValue(key, out var token) ? token : null;
                }
            }
        }

        /// <summary>
        /// Number of tokens in the token collection
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock) { return _dict.Count; }
            }
        }

        internal TokenChangesResponseV1 ImportTokenChangesV1(byte[] changeBytes)
        {
            if (changeBytes == null) throw new ArgumentNullException(nameof(changeBytes));

            var tokenChanges = new List<TokenChange>();

            using (var stream = new MemoryStream(changeBytes))
            using (var reader = new BinaryReader(stream))
            {
                int itemCount = reader.ReadInt32();

                lock (_lock)
                {
                    for (int i = 0; i < itemCount; i++)
                    {
                        string name = reader.ReadString().ToUpper(CultureInfo.InvariantCulture);
                        int changeId = reader.ReadInt32();
                        TokenAction action = (TokenAction)reader.ReadInt16();

                        _dict.TryGetValue(name, out var existingToken);

                        if (action == TokenAction.Delete || action == TokenAction.Unknown)
                        {
                            if (existingToken != null)
                            {
                                _dict.Remove(name);
                                existingToken.Changed -= Token_Changed;
                                TokenDeleted?.Invoke(existingToken, EventArgs.Empty);
                            }
                            tokenChanges.Add(new TokenChange(changeId, action, name, null));
                        }
                        else
                        {
                            if (existingToken == null)
                            {
                                var newToken = new Token(reader);
                                _dict.Add(name, newToken);
                                newToken.Changed += Token_Changed;
                                tokenChanges.Add(new TokenChange(changeId, action, name, newToken));
                                TokenAdded?.Invoke(newToken, EventArgs.Empty);
                            }
                            else
                            {
                                existingToken.Deserialize(reader);
                                tokenChanges.Add(new TokenChange(changeId, action, name, existingToken));
                            }
                        }
                    }
                }
            }

            return new TokenChangesResponseV1(tokenChanges);
        }

        /// <summary>
        /// Returns a list of all the tokens in the collection
        /// </summary>
        /// <returns></returns>
        public List<Token> ToList()
        {
            lock (_lock)
            {
                return new List<Token>(_dict.Values);
            }
        }

        /// <summary>
        /// Returns a string list of names
        /// </summary>
        /// <returns>List of names</returns>
        public List<string> ToListOfNames()
        {
            lock (_lock)
            {
                var result = new List<string>(_dict.Count);
                foreach (var token in _dict.Values)
                {
                    result.Add(token.Name);
                }
                return result;
            }
        }

        private void Token_Changed(object sender, EventArgs e)
        {
            TokenChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// IDisposable implementation to clean up resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                foreach (var token in _dict.Values)
                {
                    token.Changed -= Token_Changed; // Unsubscribe from events
                }

                _dict.Clear(); // Clear the dictionary
                TokenAdded = null;
                TokenChanged = null;
                TokenDeleted = null;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// IDisposable implementation to clean up resources.
        /// </summary>
        ~TokenCollectionReadOnly()
        {
            Dispose();
        }
    }
}
