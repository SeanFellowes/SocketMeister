using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SocketMeister.Messages
{
    internal partial class MessageBase
    {
        private bool _isAborted = false;
        private readonly object _lock = new object();
        private readonly MessageTypes _messageType;
        private MessageStatus _messageStatus = MessageStatus.Unsent;

        internal MessageBase(MessageTypes MessageType)
        {
            _messageType = MessageType;
        }

        public object Lock {  get { return _lock; } }

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


        public bool IsAborted
        {
            get { lock (_lock) { return _isAborted; } }
            set { lock (_lock) { _isAborted = value; } }
        }


        public MessageTypes MessageType { get { return _messageType; } }


        public MessageStatus Status
        {
            get { lock (_lock) { return _messageStatus; } }
            set { lock (_lock) { _messageStatus = value; } }
        }


        internal static void SerializeParameters(BinaryWriter bWriter, object[] Parameters)
        {
            bWriter.Write(Parameters.Length);

            for (int ptr = 0; ptr < Parameters.Length; ptr++)
            {
                if (Parameters[ptr] == null)
                {
                    bWriter.Write((short)ParameterTypes.Null);
                    continue;
                }

                Type ParamType = Parameters[ptr].GetType();

                if (ParamType == typeof(bool))
                {
                    bWriter.Write((short)ParameterTypes.BoolParam);
                    bWriter.Write((bool)Parameters[ptr]);
                }
                //else if (ParamType == typeof(Nullable))
                //{
                //    bWriter.Write((Int16)ParameterTypes.DateTimeParam);
                //    bWriter.Write(((DateTime)Parameters[ptr]).Ticks);
                //}
                else if (ParamType == typeof(DateTime))
                {
                    bWriter.Write((short)ParameterTypes.DateTimeParam);
                    bWriter.Write(((DateTime)Parameters[ptr]).Ticks);
                }
                else if (ParamType == typeof(double))
                {
                    bWriter.Write((short)ParameterTypes.DoubleParam);
                    bWriter.Write((double)Parameters[ptr]);
                }
                else if (ParamType == typeof(short))
                {
                    bWriter.Write((short)ParameterTypes.Int16Param);
                    bWriter.Write((short)Parameters[ptr]);
                }
                else if (ParamType == typeof(int))
                {
                    bWriter.Write((short)ParameterTypes.Int32Param);
                    bWriter.Write((int)Parameters[ptr]);
                }
                else if (ParamType == typeof(long))
                {
                    bWriter.Write((short)ParameterTypes.Int64Param);
                    bWriter.Write((long)Parameters[ptr]);
                }
                else if (ParamType == typeof(ushort))
                {
                    bWriter.Write((short)ParameterTypes.UInt16Param);
                    bWriter.Write((ushort)Parameters[ptr]);
                }
                else if (ParamType == typeof(uint))
                {
                    bWriter.Write((short)ParameterTypes.UInt32Param);
                    bWriter.Write((uint)Parameters[ptr]);
                }
                else if (ParamType == typeof(ulong))
                {
                    bWriter.Write((short)ParameterTypes.UInt64Param);
                    bWriter.Write((ulong)Parameters[ptr]);
                }
                else if (ParamType == typeof(string))
                {
                    bWriter.Write((short)ParameterTypes.StringParam);
                    bWriter.Write((string)Parameters[ptr]);
                }
                else if (ParamType == typeof(byte))
                {
                    bWriter.Write((short)ParameterTypes.ByteParam);
                    bWriter.Write((byte)Parameters[ptr]);
                }
                else if (ParamType == typeof(byte[]))
                {
                    //  PREFIX THE DATA WITH AN int OF THE LENGTH, FOLLOWED BY THE DATA (WE NEED THE PREFIX TO DESERIALIZE)
                    bWriter.Write((short)ParameterTypes.ByteArrayParam);
                    byte[] ToWrite = (byte[])Parameters[ptr];
                    bWriter.Write((int)ToWrite.Length);
                    bWriter.Write(ToWrite);
                }
                else
                {
                    throw new ArgumentException("Request parameter " + (ptr + 1) + " is an unsupported type (" + ParamType.Name + ").", nameof(Parameters));
                }
            }
        }

    }
}
