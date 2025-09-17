#if !NET35

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SocketMeister
{
    /// <summary>
    /// A dictionary-based collection of tokens. The data in this collection is read-only.
    /// </summary>
    internal class TokenCollectionReadOnly : IDisposable
    {
        private readonly ConcurrentDictionary<string, Token> _tokenDictionary = new ConcurrentDictionary<string, Token>(StringComparer.OrdinalIgnoreCase);
        private bool _disposed = false;

        /// <summary>
        /// Provides indexed access to a token by its name. Returns the token or null if not found.
        /// </summary>
        /// <param name="Name">The name of the token to retrieve.</param>
        /// <returns>The token if found, or null if not found.</returns>
        public Token this[string Name]
        {
            get
            {
                return _tokenDictionary.TryGetValue(Name, out var token) ? token : null;
            }
        }

        /// <summary>
        /// Gets the number of tokens in the collection.
        /// </summary>
        public int Count => _tokenDictionary.Count;

        /// <summary>
        /// Imports token changes from a byte array. This method is used to update the collection with changes from the client.
        /// </summary>
        /// <param name="changeBytes">A byte array containing the serialized token changes.</param>
        /// <returns>A list of <see cref="TokenChange"/> objects representing the processed changes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="changeBytes"/> is null.</exception>
        internal List<TokenChange> ImportTokenChanges(byte[] changeBytes)
        {
            if (changeBytes == null)
            {
                throw new ArgumentNullException(nameof(changeBytes));
            }

            var processedTokens = new List<TokenChange>();

            using (var stream = new MemoryStream(changeBytes))
            using (var reader = new BinaryReader(stream))
            {
                int itemCount = reader.ReadInt32();

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    string name = reader.ReadString();  // _tokenDictionary is case-insensitive, so case does not matter.
                    int changeId = reader.ReadInt32();
                    TokenAction action = (TokenAction)reader.ReadInt16();

                    // Add the processed token change to the list.
                    processedTokens.Add(new TokenChange(changeId, action, name, null));

                    // Try to find the token in the collection.
                    _tokenDictionary.TryGetValue(name, out var existingToken);

                    if (action == TokenAction.Delete || action == TokenAction.Unknown)
                    {
                        if (existingToken != null) _tokenDictionary.TryRemove(name, out _);
                    }
                    else
                    {
                        if (existingToken == null)
                        {
                            var newToken = new Token(reader);
                            _tokenDictionary.TryAdd(name, newToken);
                        }
                        else
                        {
                            existingToken.Deserialize(reader);
                        }
                    }
                }
            }

            return processedTokens;
        }

        /// <summary>
        /// Initializes the collection with tokens from a byte array created using the Serialize method.
        /// </summary>
        /// <param name="tokenBytes">A byte array containing the serialized tokens.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tokenBytes"/> is null.</exception>
        internal void Initialize(byte[] tokenBytes)
        {
            if (tokenBytes == null) throw new ArgumentNullException(nameof(tokenBytes));

            using (var stream = new MemoryStream(tokenBytes))
            using (var reader = new BinaryReader(stream))
            {
                int itemCount = reader.ReadInt32();

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var newToken = new Token(reader);
                    _tokenDictionary.TryAdd(newToken.Name, newToken);
                }
            }
        }

        /// <summary>
        /// Returns a list of all token names in the collection.
        /// </summary>
        /// <returns>A list of strings containing the token names.</returns>
        public List<string> ToListOfNames()
        {
            // Take a snapshot of the current values.
            var tokensSnapshot = _tokenDictionary.Values.ToArray();

            // Create a new list based on the snapshot.
            var result = new List<string>(tokensSnapshot.Length);
            foreach (var token in tokensSnapshot)
            {
                result.Add(token.Name);
            }

            return result;
        }

        /// <summary>
        /// Releases the resources used by the <see cref="TokenCollectionReadOnly"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Free managed resources.
                _tokenDictionary.Clear();
            }

            // Free unmanaged resources (if any).
            _disposed = true;
        }

        /// <summary>
        /// Releases the resources used by the <see cref="TokenCollectionReadOnly"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer to release resources.
        /// </summary>
        ~TokenCollectionReadOnly()
        {
            Dispose(false);
        }
    }
}

#endif