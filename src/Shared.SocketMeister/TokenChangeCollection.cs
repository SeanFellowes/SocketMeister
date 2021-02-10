using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SocketMeister
{
    /// <summary>
    /// Used internally to track token changes to publish
    /// </summary>
    internal class TokenChangeCollection
    {
        private Dictionary<string, TokenChange> _dictName = new Dictionary<string, TokenChange>();
        private readonly object _lock = new object();
        private readonly TokenCollection _tokenCollection = null;

        private void AddChange(TokenAction Action, Token Token)
        {
            if (Token == null) throw new ArgumentException("Token cannot be null", nameof(Token));

            TokenChange foundTokenChange;
            lock (_lock)
            {
                _dictName.TryGetValue(Token.Name.ToUpper(CultureInfo.InvariantCulture), out foundTokenChange);

                //  IF EXISTING RECORD EXISTS, DELETE IT
                _dictName.Remove(Token.Name.ToUpper(CultureInfo.InvariantCulture));

                //  ADD IT BACK IN
                if (Action == TokenAction.Delete)
                    _dictName.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new TokenChange(Action, Token.Name, null));
                else
                    _dictName.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new TokenChange(Action, Token.Name, Token));
            }
        }

        public TokenChangeCollection(TokenCollection tokenCollection)
        {
            _tokenCollection = tokenCollection;
            _tokenCollection.TokenAdded += _tokenCollection_TokenAdded;
            _tokenCollection.TokenChanged += _tokenCollection_TokenChanged;
            _tokenCollection.TokenDeleted += _tokenCollection_TokenDeleted;
        }

        //public static TokenChanges(byte[] Data)
        //{
        //    if (Data == null) throw new ArgumentNullException(nameof(Data));

        //    using (MemoryStream stream = new MemoryStream(Data))
        //    {
        //        using (BinaryReader reader = new BinaryReader(stream))
        //        {
        //            int itemCount = reader.ReadInt32();

        //            for (int i = 0; i < itemCount; i++)
        //            {
        //                string name = reader.ReadString();
        //                int changeId = reader.ReadInt32();
        //                TokenAction action = (TokenAction)reader.ReadInt16();

        //                Token t;
        //                if (reader.ReadBoolean() == false) t = null;
        //                else t = new Token(reader);

        //                _dict.Add(name, new Change(changeId, action, t));
        //            }
        //        }
        //    }
        //}


        internal void ImportTokenChangesResponseV1(TokenChangesResponseV1 Response)
        {
            //  DELETE ANY CHANGES WHICH MATCH
            lock (_lock)
            {
                foreach (TokenChangesResponseV1.ChangeIdentifier i in Response.ChangeIdentifiers)
                {
                    TokenChange fnd;
                    _dictName.TryGetValue(i.TokenName.ToUpper(CultureInfo.InvariantCulture), out fnd);
                    if (fnd != null && fnd.ChangeId == i.ChangeId) _dictName.Remove(i.TokenName);
                }
            }
        }


        public static List<TokenChange> DeserializeTokenChanges(byte[] Data)
        {
            if (Data == null) throw new ArgumentNullException(nameof(Data));

            List<TokenChange> rVal = new List<TokenChange>();
            using (MemoryStream stream = new MemoryStream(Data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int itemCount = reader.ReadInt32();

                    for (int i = 0; i < itemCount; i++)
                    {
                        string name = reader.ReadString();
                        int changeId = reader.ReadInt32();
                        TokenAction action = (TokenAction)reader.ReadInt16();

                        Token t;
                        if (reader.ReadBoolean() == false) t = null;
                        else t = new Token(reader);

                        rVal.Add(new TokenChange(changeId, action, name, t));
                    }
                }
            }
            return rVal;
        }



        /// <summary>
        /// If there are any changes, returns a byte[] array containing the seralized data. If there are no changes, returns null
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            lock (_lock)
            {
                if (_dictName.Count == 0) return null;

                using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
                {
                    writer.Write(_dictName.Count);

                    foreach(KeyValuePair<string, TokenChange> kvp in _dictName)
                    {
                        writer.Write(kvp.Key);                  //  NAME

                        writer.Write(kvp.Value.ChangeId);       //  CHANGE ID
                        writer.Write((short)kvp.Value.Action);  //  ACTION

                        if (kvp.Value.Token == null) writer.Write(false);
                        else
                        {
                            writer.Write(true);
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

        private void _tokenCollection_TokenAdded(object sender, EventArgs e)
        {
            Token t = (Token)sender;
            AddChange(TokenAction.Add, t);
        }

        private void _tokenCollection_TokenChanged(object sender, EventArgs e)
        {
            Token t = (Token)sender;
            AddChange(TokenAction.Modify, t);
        }

        private void _tokenCollection_TokenDeleted(object sender, EventArgs e)
        {
            Token t = (Token)sender;
            AddChange(TokenAction.Delete, t);
        }


    }

}
