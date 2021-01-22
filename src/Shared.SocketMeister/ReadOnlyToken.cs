//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;

//namespace SocketMeister
//{
//    /// <summary>
//    /// Value which is automatically synchronised between SocketClient and SocketServer. Multiple Tokens are 
//    /// </summary>
//#if SMISPUBLIC
//    public class ReadOnlyToken
//#else
//    internal class Token
//#endif
//    {
//        private readonly bool _isReadOnly;
//        private readonly string _name;
//        private bool _needsSynchronization;
//        private object _value = null;
//        private ValueType _valueType = ValueType.NullValue;
//        private readonly object _lock = new object();

//        /// <summary>
//        /// Raised when the value is changed.
//        /// </summary>
//        public event EventHandler<EventArgs> Changed;


//        internal ReadOnlyToken(BinaryReader Reader)
//        {
//            if (Reader == null) throw new ArgumentNullException(nameof(Reader));

//            _isReadOnly = true;

//            short readVal = Reader.ReadInt16();
//            _valueType = (ValueType)readVal;

//            if (_valueType == ValueType.NullValue) _value = null;
//            else if (_valueType == ValueType.BoolValue) _value = Reader.ReadBoolean();
//            else if (_valueType == ValueType.Int16Value) _value = Reader.ReadInt16();
//            else if (_valueType == ValueType.Int32Value) _value = Reader.ReadInt32();
//            else if (_valueType == ValueType.Int64Value) _value = Reader.ReadInt64();
//            else if (_valueType == ValueType.UInt16Value) _value = Reader.ReadUInt16();
//            else if (_valueType == ValueType.UInt32Value) _value = Reader.ReadUInt32();
//            else if (_valueType == ValueType.UInt64Value) _value = Reader.ReadUInt64();
//            else if (_valueType == ValueType.StringValue) _value = Reader.ReadString();
//            else if (_valueType == ValueType.DateTimeValue) _value = new DateTime(Reader.ReadInt64());
//            else if (_valueType == ValueType.DoubleValue) _value = Reader.ReadDouble();
//            else if (_valueType == ValueType.ByteValue) _value = Reader.ReadByte();
//            else if (_valueType == ValueType.ByteArrayValue) _value = Reader.ReadBytes(Reader.ReadInt32());
//            throw new NotImplementedException("No code for " + nameof(ValueType) + " = " + readVal);
//        }

//        /// <summary>
//        /// Internal setting to track syncronisation between server and client.
//        /// </summary>
//        internal bool NeedsSynchronization
//        {
//            get { lock (_lock) { return _needsSynchronization; } }
//            set { lock (_lock) { _needsSynchronization = value; } }
//        }


//        /// <summary>
//        /// Readonly name of the token
//        /// </summary>
//        public string Name { get { return _name; } }

//        /// <summary>
//        /// Value for the token. Supports null, bool, DateTime, double, short, int, long, ushort, uint, ulong, string, byte, byte[]
//        /// </summary>
//        public object Value
//        {
//            get { lock (_lock) { return _value; } }
//        }

//        /// <summary>
//        /// The type of data stored in the value
//        /// </summary>
//        public ValueType ValueType
//        {
//            get { lock (_lock) { return _valueType; } }
//        }


//        /// <summary>
//        /// Appends the binary data for this token to an open BinaryWriter
//        /// </summary>
//        /// <param name="Writer"></param>
//        public void Serialize(BinaryWriter Writer)
//        {
//            if (Writer == null) throw new ArgumentNullException(nameof(Writer));

//            lock (_lock)
//            {
//                Writer.Write((short)_valueType);

//                if (_valueType == ValueType.NullValue)
//                    return;
//                else if (_valueType == ValueType.BoolValue)
//                    Writer.Write((bool)_value);
//                else if (_valueType == ValueType.DateTimeValue)
//                    Writer.Write(((DateTime)_value).Ticks);
//                else if (_valueType == ValueType.DoubleValue)
//                    Writer.Write((double)_value);
//                else if (_valueType == ValueType.Int16Value)
//                    Writer.Write((short)_value);
//                else if (_valueType == ValueType.Int32Value)
//                    Writer.Write((int)_value);
//                else if (_valueType == ValueType.Int64Value)
//                    Writer.Write((long)_value);
//                else if (_valueType == ValueType.UInt16Value)
//                    Writer.Write((ushort)_value);
//                else if (_valueType == ValueType.UInt32Value)
//                    Writer.Write((uint)_value);
//                else if (_valueType == ValueType.UInt64Value)
//                    Writer.Write((ulong)_value);
//                else if (_valueType == ValueType.StringValue)
//                    Writer.Write((string)_value);
//                else if (_valueType == ValueType.ByteValue)
//                    Writer.Write((byte)_value);
//                else if (_valueType == ValueType.ByteArrayValue)
//                {
//                    //  PREFIX THE DATA WITH AN int OF THE LENGTH, FOLLOWED BY THE DATA (WE NEED THE PREFIX TO DESERIALIZE)
//                    byte[] ToWrite = (byte[])_value;
//                    Writer.Write((int)ToWrite.Length);
//                    Writer.Write(ToWrite);
//                }
//                else
//                    throw new NotImplementedException("No code for " + nameof(ValueType) + " = " + _valueType.ToString());
//            }
//        }


//    }
//}
