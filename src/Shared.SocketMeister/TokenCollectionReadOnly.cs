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
            List<TokenChanges.Change> tokenChanges = TokenChanges.DeserializeTokenChanges(changeBytes);

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

            //  RETURN A SubscriptionResponseV1
            List<int> changeIds = new List<int>();
            foreach (TokenChanges.Change change in tokenChanges)
            {
                changeIds.Add(change.ChangeId);
            }
            return new TokenChangesResponseV1(changeIds);
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
