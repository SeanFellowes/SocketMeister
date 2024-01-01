#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0090 // Use 'new(...)'


using System;
using System.IO;

namespace SocketMeister
{
    /// <summary>
    /// Serialization routines for SocketMeister
    /// </summary>
#if SMISPUBLIC
    public static class Serializer
#else
    internal static class Serializer
#endif
    {
        /// <summary>
        /// Data types which are supported for parameters sent with messages.
        /// </summary>
        public enum ParameterType
        {
            /// <summary>
            /// Boolean
            /// </summary>
            BoolParam = 0,
            /// <summary>
            /// DateTime
            /// </summary>
            DateTimeParam = 1,
            /// <summary>
            /// Double Type
            /// </summary>
            DoubleParam = 2,
            /// <summary>
            /// Int16
            /// </summary>
            Int16Param = 3,
            /// <summary>
            /// Int32
            /// </summary>
            Int32Param = 4,
            /// <summary>
            /// Int64
            /// </summary>
            Int64Param = 5,
            /// <summary>
            /// Unsigned Int16
            /// </summary>
            UInt16Param = 6,
            /// <summary>
            /// Unsigned Int32
            /// </summary>
            UInt32Param = 7,
            /// <summary>
            /// Unsigned Int64
            /// </summary>
            UInt64Param = 8,
            /// <summary>
            /// String
            /// </summary>
            StringParam = 9,
            /// <summary>
            /// Byte
            /// </summary>
            ByteParam = 10,
            /// <summary>
            /// Byte Array
            /// </summary>
            ByteArrayParam = 11,
            /// <summary>
            /// Null
            /// </summary>
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
                    return DeserializeParameters(reader);
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
                ParameterType ParamType = (Serializer.ParameterType)Reader.ReadInt16();

                if (ParamType == ParameterType.Null) parameters[ptr] = null;
                else if (ParamType == ParameterType.BoolParam) parameters[ptr] = Reader.ReadBoolean();
                else if (ParamType == ParameterType.Int16Param) parameters[ptr] = Reader.ReadInt16();
                else if (ParamType == ParameterType.Int32Param) parameters[ptr] = Reader.ReadInt32();
                else if (ParamType == ParameterType.Int64Param) parameters[ptr] = Reader.ReadInt64();
                else if (ParamType == ParameterType.UInt16Param) parameters[ptr] = Reader.ReadUInt16();
                else if (ParamType == ParameterType.UInt32Param) parameters[ptr] = Reader.ReadUInt32();
                else if (ParamType == ParameterType.UInt64Param) parameters[ptr] = Reader.ReadUInt64();
                else if (ParamType == ParameterType.StringParam) parameters[ptr] = Reader.ReadString();
                else if (ParamType == ParameterType.DateTimeParam) parameters[ptr] = new DateTime(Reader.ReadInt64());
                else if (ParamType == ParameterType.DoubleParam) parameters[ptr] = Reader.ReadDouble();
                else if (ParamType == ParameterType.ByteParam) parameters[ptr] = Reader.ReadByte();
                else if (ParamType == ParameterType.ByteArrayParam) parameters[ptr] = Reader.ReadBytes(Reader.ReadInt32());
                else throw new NotSupportedException("Cannot deserialize parameter[" + ptr + "] because there is no deserialize code for type " + ParamType.ToString());
            }
            return parameters;
        }

        /// <summary>
        /// Serializes an array of parameters 
        /// </summary>
        /// <param name="Parameters">Array of objects. Only simple values permitted</param>
        /// <returns>Byte array</returns>
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

        /// <summary>
        /// Serializes an array of parameters 
        /// </summary>
        /// <param name="Writer">Binary Writer. Serialization will be appended to the BinaryWriter.</param>
        /// <param name="Parameters">Array of objects. Only simple values permitted</param>
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
                    Writer.Write(ToWrite.Length);
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

#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression

