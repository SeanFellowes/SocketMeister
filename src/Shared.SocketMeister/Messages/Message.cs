﻿using System;
using System.IO;
using System.Net.Sockets;


namespace SocketMeister.Messages
{
    /// <summary>
    /// A basic message
    /// </summary>
    internal partial class Message : MessageBase, IMessage
    {
        //  REQUEST VARIABLES
        private readonly byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;

        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly object _lock = new object();

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        public Message(object[] Parameters, int TimeoutMilliseconds) : base(MessageTypes.Message, TimeoutMilliseconds)
        {
            _parameters = Parameters;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bWriter = new BinaryWriter(stream))
                {
                    SerializeParameters(bWriter, Parameters);
                }
                _parameterBytes = stream.ToArray();
            }
        }


        internal Message(BinaryReader bR) : base(MessageTypes.Message)
        {
            _parameters = DeserializeParameters(bR);
        }


        /// <summary>
        /// Parameters provided with this request
        /// </summary>
        public object[] Parameters
        {
            get { return _parameters; }
        }

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }
    }
}