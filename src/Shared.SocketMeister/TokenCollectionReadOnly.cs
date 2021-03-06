#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE1005 // Delegate invocation can be simplified.
#pragma warning disable IDE0063 // Use simple 'using' statement

using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SocketMeister
{
    /// <summary>
    /// Dictionary based collection of tokens. Data is readonly.
    /// </summary>
#if SMISPUBLIC
    public class TokenCollectionReadOnly
#else
    internal class TokenCollectionReadOnly
#endif
    {
        private readonly Dictionary<string, Token> _dict = new Dictionary<string, Token>();
        private readonly object _lock = new object();

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

        internal TokenChangesResponseV1 ImportTokenChangesV1(byte[] changeBytes)
        {
            if (changeBytes == null) throw new ArgumentNullException(nameof(changeBytes));

            List<TokenChange> tokenChanges = new List<TokenChange>();
            using (MemoryStream stream = new MemoryStream(changeBytes))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int itemCount = reader.ReadInt32();

                    for (int i = 0; i < itemCount; i++)
                    {
                        string name = reader.ReadString().ToUpper(CultureInfo.InvariantCulture);
                        int changeId = reader.ReadInt32();
                        TokenAction action = (TokenAction)reader.ReadInt16();

                        Token fnd = null;
                        lock (_lock) { _dict.TryGetValue(name, out fnd); }

                        if (action == TokenAction.Delete || action == TokenAction.Unknown)
                        {
                            //  DELETE TOKEN
                            if (fnd != null)
                            {
                                lock (_lock) { _dict.Remove(name); }
                                fnd.Changed -= Token_Changed;
                                if (TokenDeleted != null) TokenDeleted(fnd, new EventArgs());
                            }
                            tokenChanges.Add(new TokenChange(changeId, action, name, null));
                        }
                        else
                        {
                            if (fnd == null)
                            {
                                //  ADD NEW TOKEN
                                Token newToken = new Token(reader);
                                lock (_lock) { _dict.Add(name, newToken); }
                                newToken.Changed += Token_Changed;
                                tokenChanges.Add(new TokenChange(changeId, action, name, newToken));
                                TokenAdded?.Invoke(fnd, new EventArgs());
                            }
                            else
                            {
                                //  UPDATE VALUE
                                fnd.Deserialize(reader);
                                tokenChanges.Add(new TokenChange(changeId, action, name, fnd));
                            }
                        }
                    }
                }
            }

            //  RETURN A SubscriptionResponseV1
            return new TokenChangesResponseV1(tokenChanges);
        }

        /// <summary>
        /// Returns a list of all the tokens in the collection
        /// </summary>
        /// <returns></returns>
        public List<Token> ToList()
        {
            List<Token> rVal = new List<Token>();
            lock (_lock)
            {
                foreach (KeyValuePair<string, Token> kvp in _dict)
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

#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE1005 // Delegate invocation can be simplified.
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression

