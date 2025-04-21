using System;
using System.IO;

namespace SocketMeister
{
    /// <summary>
    /// Provides serialization and deserialization routines for SocketMeister.
    /// </summary>
#if SMISPUBLIC
    public static class Serializer
#else
        internal static class Serializer
#endif
    {
        /// <summary>
        /// Enumerates the data types supported for parameters sent with messages.
        /// </summary>
        public enum ParameterType
        {
            /// <summary>Boolean</summary>
            BoolParam = 0,
            /// <summary>DateTime</summary>
            DateTimeParam = 1,
            /// <summary>Double</summary>
            DoubleParam = 2,
            /// <summary>Int16</summary>
            Int16Param = 3,
            /// <summary>Int32</summary>
            Int32Param = 4,
            /// <summary>Int64</summary>
            Int64Param = 5,
            /// <summary>Unsigned Int16</summary>
            UInt16Param = 6,
            /// <summary>Unsigned Int32</summary>
            UInt32Param = 7,
            /// <summary>Unsigned Int64</summary>
            UInt64Param = 8,
            /// <summary>String</summary>
            StringParam = 9,
            /// <summary>Byte</summary>
            ByteParam = 10,
            /// <summary>Byte array</summary>
            ByteArrayParam = 11,
            /// <summary>Null</summary>
            Null = 99
        }

        /// <summary>
        /// Deserializes a byte array that was serialized using the <see cref="SerializeParameters(object[])"/> method.
        /// </summary>
        /// <param name="Data">The binary array to deserialize.</param>
        /// <returns>An array of deserialized parameters.</returns>
        public static object[] DeserializeParameters(byte[] Data)
        {
            if (Data == null) throw new ArgumentNullException(nameof(Data));

            using (MemoryStream stream = new MemoryStream(Data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return DeserializeParameters(reader);
            }
        }

        /// <summary>
        /// Deserializes parameters embedded within other data. The data must have been serialized using the 
        /// <see cref="SerializeParameters(BinaryWriter, object[])"/> method.
        /// </summary>
        /// <param name="Reader">An open <see cref="BinaryReader"/> positioned at the start of the serialized parameters.</param>
        /// <returns>An array of deserialized parameters.</returns>
        public static object[] DeserializeParameters(BinaryReader Reader)
        {
            if (Reader == null) throw new ArgumentNullException(nameof(Reader));

            int paramCount = Reader.ReadInt32();
            object[] parameters = new object[paramCount];

            for (int ptr = 0; ptr < paramCount; ptr++)
            {
                ParameterType ParamType = (ParameterType)Reader.ReadInt16();

                switch (ParamType)
                {
                    case ParameterType.Null:
                        parameters[ptr] = null;
                        break;
                    case ParameterType.BoolParam:
                        parameters[ptr] = Reader.ReadBoolean();
                        break;
                    case ParameterType.Int16Param:
                        parameters[ptr] = Reader.ReadInt16();
                        break;
                    case ParameterType.Int32Param:
                        parameters[ptr] = Reader.ReadInt32();
                        break;
                    case ParameterType.Int64Param:
                        parameters[ptr] = Reader.ReadInt64();
                        break;
                    case ParameterType.UInt16Param:
                        parameters[ptr] = Reader.ReadUInt16();
                        break;
                    case ParameterType.UInt32Param:
                        parameters[ptr] = Reader.ReadUInt32();
                        break;
                    case ParameterType.UInt64Param:
                        parameters[ptr] = Reader.ReadUInt64();
                        break;
                    case ParameterType.StringParam:
                        parameters[ptr] = Reader.ReadString();
                        break;
                    case ParameterType.DateTimeParam:
                        parameters[ptr] = new DateTime(Reader.ReadInt64());
                        break;
                    case ParameterType.DoubleParam:
                        parameters[ptr] = Reader.ReadDouble();
                        break;
                    case ParameterType.ByteParam:
                        parameters[ptr] = Reader.ReadByte();
                        break;
                    case ParameterType.ByteArrayParam:
                        int length = Reader.ReadInt32();
                        long remainingBytes = Reader.BaseStream.Length - Reader.BaseStream.Position;

                        if (length < 0 || length > remainingBytes)
                            throw new InvalidDataException($"Invalid byte array length: {length}. Remaining bytes: {remainingBytes}.");

                        parameters[ptr] = Reader.ReadBytes(length);
                        break;
                    default:
                        throw new NotSupportedException($"Cannot deserialize parameter[{ptr}] of type {ParamType}.");
                }
            }

            return parameters;
        }

        /// <summary>
        /// Serializes an array of parameters into a byte array.
        /// </summary>
        /// <param name="Parameters">An array of objects to serialize. Only simple types are supported.</param>
        /// <returns>A byte array containing the serialized parameters.</returns>
        public static byte[] SerializeParameters(object[] Parameters)
        {
            if (Parameters == null) throw new ArgumentNullException(nameof(Parameters));

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                SerializeParameters(writer, Parameters);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes an array of parameters and appends the data to a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="Writer">The <see cref="BinaryWriter"/> to which the serialized data will be appended.</param>
        /// <param name="Parameters">An array of objects to serialize. Only simple types are supported.</param>
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
                    // Prefix the data with an integer length, followed by the byte array itself.
                    Writer.Write((short)ParameterType.ByteArrayParam);
                    byte[] ToWrite = (byte[])Parameters[ptr];
                    Writer.Write(ToWrite.Length);
                    Writer.Write(ToWrite);
                }
                else
                {
                    throw new ArgumentException($"Request parameter[{ptr}] is an unsupported type ({ParamType.Name}).", nameof(Parameters));
                }
            }
        }
    }
}
