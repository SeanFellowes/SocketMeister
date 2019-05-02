using System;
using System.IO;
using System.Threading;

namespace SocketMeister.Messages
{
    internal class MessageBase : IDisposable
    {
        private readonly object classLock = new object();
        private bool disposed = false;
        private AutoResetEvent sendReceiveCompleteEvent = null;
        private readonly MessageTypes messageType;
        private SendReceiveStatus sendReceiveStatus = SendReceiveStatus.Unsent;
        private readonly DateTime timeout;
        private readonly int timeoutMilliseconds;

        internal MessageBase(MessageTypes MessageType)
        {
            messageType = MessageType;
        }


        internal MessageBase(MessageTypes messageType, int timeoutMilliseconds)
        {
            this.messageType = messageType;
            this.timeoutMilliseconds = timeoutMilliseconds;
            timeout = DateTime.Now.AddMilliseconds(timeoutMilliseconds);
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
#if !NET20 && !NET35
                sendReceiveCompleteEvent.Dispose();
#endif
            }

            disposed = true;
        }


        /// <summary>s
        /// Cross threading locking mechanism while waiting for an asynchronous socket send/receive to complete
        /// </summary>
        public AutoResetEvent SendReceiveCompleteEvent
        {
            get { lock (classLock) { return sendReceiveCompleteEvent; } }
            set { lock (classLock) { sendReceiveCompleteEvent = value; } }
        }


        internal static object[] DeserializeParameters(BinaryReader reader)
        {
            int paramCount = reader.ReadInt32();
            object[] parameters = new object[paramCount];
            for (int ptr = 0; ptr < paramCount; ptr++)
            {
                ParameterTypes ParamType = (ParameterTypes)reader.ReadInt16();

                if (ParamType == ParameterTypes.Null) parameters[ptr] = null;
                else if (ParamType == ParameterTypes.BoolParam) parameters[ptr] = reader.ReadBoolean();
                else if (ParamType == ParameterTypes.Int16Param) parameters[ptr] = reader.ReadInt16();
                else if (ParamType == ParameterTypes.Int32Param) parameters[ptr] = reader.ReadInt32();
                else if (ParamType == ParameterTypes.Int64Param) parameters[ptr] = reader.ReadInt64();
                else if (ParamType == ParameterTypes.UInt16Param) parameters[ptr] = reader.ReadUInt16();
                else if (ParamType == ParameterTypes.UInt32Param) parameters[ptr] = reader.ReadUInt32();
                else if (ParamType == ParameterTypes.UInt64Param) parameters[ptr] = reader.ReadUInt64();
                else if (ParamType == ParameterTypes.StringParam) parameters[ptr] = reader.ReadString();
                else if (ParamType == ParameterTypes.DateTimeParam) parameters[ptr] = new DateTime(reader.ReadInt64());
                else if (ParamType == ParameterTypes.DoubleParam) parameters[ptr] = reader.ReadDouble();
                else if (ParamType == ParameterTypes.ByteParam) parameters[ptr] = reader.ReadByte();
                else if (ParamType == ParameterTypes.ByteArrayParam) parameters[ptr] = reader.ReadBytes(reader.ReadInt32());
#if !SILVERLIGHT
                else throw new InvalidDataException("Cannot deserialize parameter " + ptr + ". There is no deserialize code for type " + ParamType.ToString());
#else
                else throw new Exception("Cannot deserialize parameter " + ptr + ". There is no deserialize code for type " + ParamType.ToString());
#endif
            }
            return parameters;
        }

        public MessageTypes MessageType { get { return messageType; } }

        public SendReceiveStatus SendReceiveStatus
        {
            get
            {
                lock (classLock)
                {
                    if (sendReceiveStatus == SendReceiveStatus.ResponseReceived) return SendReceiveStatus.ResponseReceived;
                    else if (DateTime.Now >= timeout) return SendReceiveStatus.Timeout;
                    else return sendReceiveStatus;
                }
            }
            set
            {
                lock (classLock)
                {
                    if (sendReceiveStatus != SendReceiveStatus.ResponseReceived) sendReceiveStatus = value;
                }
            }
        }


        /// <summary>
        /// Number of milliseconds to wait before a timeout will occur.
        /// </summary>
        public int TimeoutMilliseconds { get { return timeoutMilliseconds; } }


        internal static void SerializeParameters(BinaryWriter writer, object[] parameters)
        {
            writer.Write(parameters.Length);

            for (int ptr = 0; ptr < parameters.Length; ptr++)
            {
                if (parameters[ptr] == null)
                {
                    writer.Write((Int16)ParameterTypes.Null);
                    continue;
                }

                Type ParamType = parameters[ptr].GetType();

                if (ParamType == typeof(bool))
                {
                    writer.Write((Int16)ParameterTypes.BoolParam);
                    writer.Write((bool)parameters[ptr]);
                }
                //else if (ParamType == typeof(Nullable))
                //{
                //    bWriter.Write((Int16)ParameterTypes.DateTimeParam);
                //    bWriter.Write(((DateTime)Parameters[ptr]).Ticks);
                //}
                else if (ParamType == typeof(DateTime))
                {
                    writer.Write((Int16)ParameterTypes.DateTimeParam);
                    writer.Write(((DateTime)parameters[ptr]).Ticks);
                }
                else if (ParamType == typeof(Double))
                {
                    writer.Write((Int16)ParameterTypes.DoubleParam);
                    writer.Write((Double)parameters[ptr]);
                }
                else if (ParamType == typeof(Int16))
                {
                    writer.Write((Int16)ParameterTypes.Int16Param);
                    writer.Write((Int16)parameters[ptr]);
                }
                else if (ParamType == typeof(Int32))
                {
                    writer.Write((Int16)ParameterTypes.Int32Param);
                    writer.Write((Int32)parameters[ptr]);
                }
                else if (ParamType == typeof(Int64))
                {
                    writer.Write((Int16)ParameterTypes.Int64Param);
                    writer.Write((Int64)parameters[ptr]);
                }
                else if (ParamType == typeof(UInt16))
                {
                    writer.Write((Int16)ParameterTypes.UInt16Param);
                    writer.Write((UInt16)parameters[ptr]);
                }
                else if (ParamType == typeof(UInt32))
                {
                    writer.Write((Int16)ParameterTypes.UInt32Param);
                    writer.Write((UInt32)parameters[ptr]);
                }
                else if (ParamType == typeof(UInt64))
                {
                    writer.Write((Int16)ParameterTypes.UInt64Param);
                    writer.Write((UInt64)parameters[ptr]);
                }
                else if (ParamType == typeof(string))
                {
                    writer.Write((Int16)ParameterTypes.StringParam);
                    writer.Write((string)parameters[ptr]);
                }
                else if (ParamType == typeof(Byte))
                {
                    writer.Write((Int16)ParameterTypes.ByteParam);
                    writer.Write((Byte)parameters[ptr]);
                }
                else if (ParamType == typeof(Byte[]))
                {
                    //  PREFIX THE DATA WITH AN int OF THE LENGTH, FOLLOWED BY THE DATA (WE NEED THE PREFIX TO DESERIALIZE)
                    writer.Write((Int16)ParameterTypes.ByteArrayParam);
                    Byte[] ToWrite = (Byte[])parameters[ptr];
                    writer.Write((Int32)ToWrite.Length);
                    writer.Write(ToWrite);
                }
                else
                {
                    throw new ArgumentException("Request parameter " + (ptr + 1) + " is an unsupported type (" + ParamType.Name + ").", nameof(parameters));
                }
            }
        }

    }
}
