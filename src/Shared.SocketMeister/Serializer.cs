﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister
{
    /// <summary>
    /// Serialization routines for SocketMeister
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Data types which are supported for parameters sent with messages.
        /// </summary>
        public enum ParameterType
        {
            BoolParam = 0,
            DateTimeParam = 1,
            DoubleParam = 2,
            Int16Param = 3,
            Int32Param = 4,
            Int64Param = 5,
            UInt16Param = 6,
            UInt32Param = 7,
            UInt64Param = 8,
            StringParam = 9,
            ByteParam = 10,
            ByteArrayParam = 11,
            Null = 99
        }

        /// <summary>
        /// Deserialize a byte array which was serialized using 'byte[] SerializeParameters(object[] Parameters)' from this class.
        /// </summary>
        /// <param name="Data">Binary array</param>
        /// <returns>A list of parameters</returns>
        public static object[] DeserializeParameters(byte[] Data)
        {
            if (Data == null) throw new ArgumentNullException(nameof(Data));

            using (MemoryStream stream = new MemoryStream(Data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return Serializer.DeserializeParameters(reader);
                }
            }
        }

        /// <summary>
        /// Deserialize objects embedded with other data. Must have been serialized using 'void SerializeParameters(BinaryWriter BinaryWriter, object[] Parameters)' from this class
        /// </summary>
        /// <param name="Reader">Open BinaryReader which is queued exactly to the point where the serialized parameters are encoded.</param>
        /// <returns>A list of parameters</returns>
        public static object[] DeserializeParameters(BinaryReader Reader)
        {
            if (Reader == null) throw new ArgumentNullException(nameof(Reader));

            int paramCount = Reader.ReadInt32();
            object[] parameters = new object[paramCount];
            for (int ptr = 0; ptr < paramCount; ptr++)
            {
                Serializer.ParameterType ParamType = (Serializer.ParameterType)Reader.ReadInt16();

                if (ParamType == Serializer.ParameterType.Null) parameters[ptr] = null;
                else if (ParamType == Serializer.ParameterType.BoolParam) parameters[ptr] = Reader.ReadBoolean();
                else if (ParamType == Serializer.ParameterType.Int16Param) parameters[ptr] = Reader.ReadInt16();
                else if (ParamType == Serializer.ParameterType.Int32Param) parameters[ptr] = Reader.ReadInt32();
                else if (ParamType == Serializer.ParameterType.Int64Param) parameters[ptr] = Reader.ReadInt64();
                else if (ParamType == Serializer.ParameterType.UInt16Param) parameters[ptr] = Reader.ReadUInt16();
                else if (ParamType == Serializer.ParameterType.UInt32Param) parameters[ptr] = Reader.ReadUInt32();
                else if (ParamType == Serializer.ParameterType.UInt64Param) parameters[ptr] = Reader.ReadUInt64();
                else if (ParamType == Serializer.ParameterType.StringParam) parameters[ptr] = Reader.ReadString();
                else if (ParamType == Serializer.ParameterType.DateTimeParam) parameters[ptr] = new DateTime(Reader.ReadInt64());
                else if (ParamType == Serializer.ParameterType.DoubleParam) parameters[ptr] = Reader.ReadDouble();
                else if (ParamType == Serializer.ParameterType.ByteParam) parameters[ptr] = Reader.ReadByte();
                else if (ParamType == Serializer.ParameterType.ByteArrayParam) parameters[ptr] = Reader.ReadBytes(Reader.ReadInt32());
                else throw new NotSupportedException("Cannot deserialize parameter[" + ptr + "] because there is no deserialize code for type " + ParamType.ToString());
            }
            return parameters;
        }

        public static byte[] SerializeParameters(object[] Parameters)
        {
            if (Parameters == null) throw new ArgumentNullException(nameof(Parameters));

            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                SerializeParameters(writer, Parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    return reader.ReadBytes((Convert.ToInt32(reader.BaseStream.Length)));
                }
            }
        }


        public static void SerializeParameters(BinaryWriter Writer, object[] Parameters)
        {
            if (Writer == null) throw new ArgumentNullException(nameof(Writer));
            if (Parameters == null) throw new ArgumentNullException(nameof(Parameters));

            Writer.Write(Parameters.Length);

            for (int ptr = 0; ptr < Parameters.Length; ptr++)
            {
                if (Parameters[ptr] == null)
                {
                    Writer.Write((short)ParameterType.Null);
                    continue;
                }

                Type ParamType = Parameters[ptr].GetType();

                if (ParamType == typeof(bool))
                {
                    Writer.Write((short)ParameterType.BoolParam);
                    Writer.Write((bool)Parameters[ptr]);
                }
                else if (ParamType == typeof(DateTime))
                {
                    Writer.Write((short)ParameterType.DateTimeParam);
                    Writer.Write(((DateTime)Parameters[ptr]).Ticks);
                }
                else if (ParamType == typeof(double))
                {
                    Writer.Write((short)ParameterType.DoubleParam);
                    Writer.Write((double)Parameters[ptr]);
                }
                else if (ParamType == typeof(short))
                {
                    Writer.Write((short)ParameterType.Int16Param);
                    Writer.Write((short)Parameters[ptr]);
                }
                else if (ParamType == typeof(int))
                {
                    Writer.Write((short)ParameterType.Int32Param);
                    Writer.Write((int)Parameters[ptr]);
                }
                else if (ParamType == typeof(long))
                {
                    Writer.Write((short)ParameterType.Int64Param);
                    Writer.Write((long)Parameters[ptr]);
                }
                else if (ParamType == typeof(ushort))
                {
                    Writer.Write((short)ParameterType.UInt16Param);
                    Writer.Write((ushort)Parameters[ptr]);
                }
                else if (ParamType == typeof(uint))
                {
                    Writer.Write((short)ParameterType.UInt32Param);
                    Writer.Write((uint)Parameters[ptr]);
                }
                else if (ParamType == typeof(ulong))
                {
                    Writer.Write((short)ParameterType.UInt64Param);
                    Writer.Write((ulong)Parameters[ptr]);
                }
                else if (ParamType == typeof(string))
                {
                    Writer.Write((short)ParameterType.StringParam);
                    Writer.Write((string)Parameters[ptr]);
                }
                else if (ParamType == typeof(byte))
                {
                    Writer.Write((short)ParameterType.ByteParam);
                    Writer.Write((byte)Parameters[ptr]);
                }
                else if (ParamType == typeof(byte[]))
                {
                    //  PREFIX THE DATA WITH AN int OF THE LENGTH, FOLLOWED BY THE DATA (WE NEED THE PREFIX TO DESERIALIZE)
                    Writer.Write((short)ParameterType.ByteArrayParam);
                    byte[] ToWrite = (byte[])Parameters[ptr];
                    Writer.Write((int)ToWrite.Length);
                    Writer.Write(ToWrite);
                }
                else
                {
                    throw new ArgumentException("Request parameter[" + ptr + "] is an unsupported type (" + ParamType.Name + ").", nameof(Parameters));
                }
            }
        }


    }
}