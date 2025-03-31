#if !SMNOSERVER && !NET35

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SocketMeister
{
    /// <summary>
    /// Dictionary-based collection of tokens. Data is readonly.
    /// </summary>
#if SMISPUBLIC
    public class TokenCollectionReadOnly : IDisposable
#else
    internal class TokenCollectionReadOnly : IDisposable
#endif
    {
        private readonly ConcurrentDictionary<string, Token> _tokenDictionary = new ConcurrentDictionary<string, Token>(StringComparer.OrdinalIgnoreCase);
        private bool _disposed = false;

        /// <summary>
        /// Indexed search returning the token or null, for a given token name.
        /// </summary>
        /// <param name="Name">Token requested.</param>
        /// <returns>Found token or null if not found</returns>
        public Token this[string Name]
        {
            get
            {
                return _tokenDictionary.TryGetValue(Name, out var token) ? token : null;
            }
        }

        /// <summary>
        /// Number of tokens in the token collection
        /// </summary>
        public int Count => _tokenDictionary.Count;

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
                    string name = reader.ReadString();  // _tokenDictionary is case insensitive so we don't care about case 
                    int changeId = reader.ReadInt32();
                    TokenAction action = (TokenAction)reader.ReadInt16();

                    // We will return a list of processed tokens to the client.
                    // This will allow the client to know what tokens have been processed by the server.
                    processedTokens.Add(new TokenChange(changeId, action, name, null));

                    //  Try and find the token from the client in this list
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
        /// Imports tokens from a byte array created using the Serialize method.
        /// </summary>
        /// <param name="tokenBytes">byte array</param>
        /// <exception cref="ArgumentNullException"></exception>
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
        /// Returns a string list of names
        /// </summary>
        /// <returns>List of names</returns>
        public List<string> ToListOfNames()
        {
            // Take a snapshot of the current values
            var tokensSnapshot = _tokenDictionary.Values.ToArray();

            // Create a new list based on the snapshot
            var result = new List<string>(tokensSnapshot.Length);
            foreach (var token in tokensSnapshot)
            {
                result.Add(token.Name);
            }

            return result;
        }

        /// <summary>
        /// IDisposable implementation to clean up resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Free managed resources
                _tokenDictionary.Clear();
            }

            // Free unmanaged resources (if any)
            _disposed = true;
        }

        /// <summary>
        /// IDisposable implementation to clean up resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// IDisposable implementation to clean up resources.
        /// </summary>
        ~TokenCollectionReadOnly()
        {
            Dispose(false);
        }
    }
}

#endif