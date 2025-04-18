﻿using System;
using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// SocketServer sends broadcasts to clients.
    /// </summary>
    internal partial class BroadcastV1 : MessageBase, IMessage
    {
        //  BROADCAST VARIABLES
        private readonly byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;
        private readonly string _name;

        /// <summary>
        /// Broadcast constructor
        /// </summary>
        /// <param name="Name">Optional Name/Tag/Identifier for the broadcast.</param>
        /// <param name="Parameters">Array of parameters to send with the broadcast. There must be at least 1 parameter.</param>
        public BroadcastV1(string Name, object[] Parameters) : base(MessageType.BroadcastV1, messageId: 0)
        {
            _parameters = Parameters;
            _name = Name;

            //  SERIALIZE BROADCAST
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                if (string.IsNullOrEmpty(_name))
                {
                    writer.Write(false);
                }
                else
                {
                    writer.Write(true);
                    writer.Write(_name);
                }
                Serializer.SerializeParameters(writer, Parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    _parameterBytes = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }


        internal BroadcastV1(BinaryReader bR) : base(MessageType.BroadcastV1, messageId: 0)
        {
            if (bR.ReadBoolean() == true) _name = bR.ReadString();
            else _name = null;

            _parameters = Serializer.DeserializeParameters(bR);
        }


        /// <summary>
        /// Parameters provided with this broadcast
        /// </summary>
        public object[] Parameters => _parameters;

        /// <summary>
        /// Optional Name/Tag/Identifier given to the broadcast
        /// </summary>
        public string Name => _name;

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }
    }
}