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
    internal class TokenChanges
    {
        internal class Change
        {
            private static int _maxTokenChangeId = 0;
            private static readonly object _lockMaxTokenChangeId = new object();

            public int ChangeId { get; }
            public TokenAction Action { get; }
            public Token Token { get; }

            public Change(TokenAction Action, Token Token = null)
            {
                ChangeId = GetTokenChangeId();
                this.Action = Action;
                this.Token = Token;
            }

            public Change(int ChangeId, TokenAction Action, Token Token = null)
            {
                this.ChangeId = ChangeId;
                this.Action = Action;
                this.Token = Token;
            }

            internal static int GetTokenChangeId()
            {
                lock (_lockMaxTokenChangeId)
                {
                    _maxTokenChangeId++;
                    if (_maxTokenChangeId == int.MaxValue) _maxTokenChangeId = 1;
                    return _maxTokenChangeId;
                }
            }
        }

        private Dictionary<string, Change> _dict = new Dictionary<string, Change>();
        private readonly object _lock = new object();
        private readonly TokenCollection _tokenCollection = null;

        private void AddChange(TokenAction Action, Token Token)
        {
            if (Token == null) throw new ArgumentException("Token cannot be null", nameof(Token));

            //  IF EXISTING RECORD EXISTS, DELETE IT
            _dict.Remove(Token.Name.ToUpper(CultureInfo.InvariantCulture));

            //  ADD IT BACK IN
            if (Action == TokenAction.Delete)
                _dict.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new Change(Action, null));
            else
                _dict.Add(Token.Name.ToUpper(CultureInfo.InvariantCulture), new Change(Action, Token));

        }

        public TokenChanges(TokenCollection tokenCollection)
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

        public static List<Change> DeserializeTokenChanges(byte[] Data)
        {
            if (Data == null) throw new ArgumentNullException(nameof(Data));

            List<Change> rVal = new List<Change>();
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

                        rVal.Add(new Change(changeId, action, t));
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
                if (_dict.Count == 0) return null;

                using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
                {
                    writer.Write(_dict.Count);

                    foreach(KeyValuePair<string, Change> kvp in _dict)
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
