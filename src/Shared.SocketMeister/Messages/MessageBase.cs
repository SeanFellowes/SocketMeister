using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SocketMeister.Messages
{
    internal class MessageBase
    {
        private AutoResetEvent _sendReceiveCompleteEvent = null;
        private readonly object _lock = new object();
        private readonly MessageTypes _messageType;
        private SendReceiveStatus _sendReceiveStatus = SendReceiveStatus.Unsent;
        private readonly DateTime _timeout;
        private readonly int _timeoutMilliseconds;

        internal MessageBase(MessageTypes MessageType)
        {
            _messageType = MessageType;
        }


        internal MessageBase(MessageTypes MessageType, int TimeoutMilliseconds)
        {
            _messageType = MessageType;
            _timeoutMilliseconds = TimeoutMilliseconds;
            _timeout = DateTime.Now.AddMilliseconds(TimeoutMilliseconds);
        }


        /// <summary>s
        /// Cross threading locking mechanism while waiting for an asynchronous socket send/receive to complete
        /// </summary>
        public AutoResetEvent SendReceiveCompleteEvent
        {
            get { lock (_lock) { return _sendReceiveCompleteEvent; } }
            set { lock (_lock) { _sendReceiveCompleteEvent = value; } }
        }


        internal static object[] DeserializeParameters(BinaryReader bR)
        {
            int paramCount = bR.ReadInt32();
            object[] parameters = new object[paramCount];
            for (int ptr = 0; ptr < paramCount; ptr++)
            {
                ParameterTypes ParamType = (ParameterTypes)bR.ReadInt16();

                if (ParamType == ParameterTypes.Null) parameters[ptr] = null;
                else if (ParamType == ParameterTypes.BoolParam) parameters[ptr] = bR.ReadBoolean();
                else if (ParamType == ParameterTypes.Int16Param) parameters[ptr] = bR.ReadInt16();
                else if (ParamType == ParameterTypes.Int32Param) parameters[ptr] = bR.ReadInt32();
                else if (ParamType == ParameterTypes.Int64Param) parameters[ptr] = bR.ReadInt64();
                else if (ParamType == ParameterTypes.UInt16Param) parameters[ptr] = bR.ReadUInt16();
                else if (ParamType == ParameterTypes.UInt32Param) parameters[ptr] = bR.ReadUInt32();
                else if (ParamType == ParameterTypes.UInt64Param) parameters[ptr] = bR.ReadUInt64();
                else if (ParamType == ParameterTypes.StringParam) parameters[ptr] = bR.ReadString();
                else if (ParamType == ParameterTypes.DateTimeParam) parameters[ptr] = new DateTime(bR.ReadInt64());
                else if (ParamType == ParameterTypes.DoubleParam) parameters[ptr] = bR.ReadDouble();
                else if (ParamType == ParameterTypes.ByteParam) parameters[ptr] = bR.ReadByte();
                else if (ParamType == ParameterTypes.ByteArrayParam) parameters[ptr] = bR.ReadBytes(bR.ReadInt32());
#if !SILVERLIGHT
                else throw new InvalidDataException("Cannot deserialize parameter " + ptr + ". There is no deserialize code for type " + ParamType.ToString());
#else
                else throw new Exception("Cannot deserialize parameter " + ptr + ". There is no deserialize code for type " + ParamType.ToString());
#endif
            }
            return parameters;
        }

        public MessageTypes MessageType { get { return _messageType; } }

        public SendReceiveStatus SendReceiveStatus
        {
            get
            {
                lock (_lock)
                {
                    if (_sendReceiveStatus == SendReceiveStatus.ResponseReceived) return SendReceiveStatus.ResponseReceived;
                    else if (DateTime.Now >= _timeout) return SendReceiveStatus.Timeout;
                    else return _sendReceiveStatus;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_sendReceiveStatus != SendReceiveStatus.ResponseReceived) _sendReceiveStatus = value;
                }
            }
        }


        /// <summary>
        /// Number of milliseconds to wait before a timeout will occur.
        /// </summary>
        public int TimeoutMilliseconds { get { return _timeoutMilliseconds; } }


        internal static void SerializeParameters(BinaryWriter bWriter, object[] Parameters)
        {
            bWriter.Write(Parameters.Length);

            for (int ptr = 0; ptr < Parameters.Length; ptr++)
            {
                if (Parameters[ptr] == null)
                {
                    bWriter.Write((Int16)ParameterTypes.Null);
                    continue;
                }

                Type ParamType = Parameters[ptr].GetType();

                if (ParamType == typeof(bool))
                {
                    bWriter.Write((Int16)ParameterTypes.BoolParam);
                    bWriter.Write((bool)Parameters[ptr]);
                }
                //else if (ParamType == typeof(Nullable))
                //{
                //    bWriter.Write((Int16)ParameterTypes.DateTimeParam);
                //    bWriter.Write(((DateTime)Parameters[ptr]).Ticks);
                //}
                else if (ParamType == typeof(DateTime))
                {
                    bWriter.Write((Int16)ParameterTypes.DateTimeParam);
                    bWriter.Write(((DateTime)Parameters[ptr]).Ticks);
                }
                else if (ParamType == typeof(Double))
                {
                    bWriter.Write((Int16)ParameterTypes.DoubleParam);
                    bWriter.Write((Double)Parameters[ptr]);
                }
                else if (ParamType == typeof(Int16))
                {
                    bWriter.Write((Int16)ParameterTypes.Int16Param);
                    bWriter.Write((Int16)Parameters[ptr]);
                }
                else if (ParamType == typeof(Int32))
                {
                    bWriter.Write((Int16)ParameterTypes.Int32Param);
                    bWriter.Write((Int32)Parameters[ptr]);
                }
                else if (ParamType == typeof(Int64))
                {
                    bWriter.Write((Int16)ParameterTypes.Int64Param);
                    bWriter.Write((Int64)Parameters[ptr]);
                }
                else if (ParamType == typeof(UInt16))
                {
                    bWriter.Write((Int16)ParameterTypes.UInt16Param);
                    bWriter.Write((UInt16)Parameters[ptr]);
                }
                else if (ParamType == typeof(UInt32))
                {
                    bWriter.Write((Int16)ParameterTypes.UInt32Param);
                    bWriter.Write((UInt32)Parameters[ptr]);
                }
                else if (ParamType == typeof(UInt64))
                {
                    bWriter.Write((Int16)ParameterTypes.UInt64Param);
                    bWriter.Write((UInt64)Parameters[ptr]);
                }
                else if (ParamType == typeof(string))
                {
                    bWriter.Write((Int16)ParameterTypes.StringParam);
                    bWriter.Write((string)Parameters[ptr]);
                }
                else if (ParamType == typeof(Byte))
                {
                    bWriter.Write((Int16)ParameterTypes.ByteParam);
                    bWriter.Write((Byte)Parameters[ptr]);
                }
                else if (ParamType == typeof(Byte[]))
                {
                    //  PREFIX THE DATA WITH AN int OF THE LENGTH, FOLLOWED BY THE DATA (WE NEED THE PREFIX TO DESERIALIZE)
                    bWriter.Write((Int16)ParameterTypes.ByteArrayParam);
                    Byte[] ToWrite = (Byte[])Parameters[ptr];
                    bWriter.Write((Int32)ToWrite.Length);
                    bWriter.Write(ToWrite);
                }
                else
                {
                    throw new ArgumentException("Request parameter " + (ptr + 1) + " is an unsupported type (" + ParamType.Name + ").", "Parameters");
                }
            }
        }

    }
}
