﻿using System;
using System.IO;

namespace SocketMeister
{
    /// <summary>
    /// A value that is automatically synchronized between a SocketClient and a SocketServer. Multiple tokens can be used.
    /// </summary>
    internal class Token
    {
        private bool _isReadOnly;
        private string _name;
        private object _value;
        private ValueType _valueType = ValueType.NullValue;
        private readonly object _lock = new object();

        /// <summary>
        /// Raised when the value is changed.
        /// </summary>
        public event EventHandler<EventArgs> Changed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="Name">The name of the token.</param>
        public Token(string Name)
        {
            if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Name cannot be null or empty.", nameof(Name));

            _name = Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class with a specified value.
        /// </summary>
        /// <param name="Name">The name of the token.</param>
        /// <param name="Value">The value to assign to the token.</param>
        public Token(string Name, object Value)
        {
            if (string.IsNullOrEmpty(Name)) throw new ArgumentException("Name cannot be null or empty.", nameof(Name));

            _name = Name;
            this.Value = Value;
        }

        internal Token(BinaryReader Reader)
        {
            Deserialize(Reader);
        }

        /// <summary>
        /// Gets the read-only name of the token.
        /// </summary>
        public string Name { get { lock (_lock) { return _name; } } }

        /// <summary>
        /// Gets or sets the value of the token. Supports null, bool, DateTime, double, short, int, long, ushort, uint, ulong, string, byte, and byte[].
        /// </summary>
        public object Value
        {
            get { lock (_lock) { return _value; } }
            set
            {
                lock (_lock)
                {
                    if (_isReadOnly) throw new InvalidOperationException("This token is read-only. Its value is synchronized with a master token.");
                    if (value == null) _valueType = ValueType.NullValue;
                    else
                    {
                        Type ParamType = value.GetType();
                        if (ParamType == typeof(bool)) _valueType = ValueType.BoolValue;
                        else if (ParamType == typeof(DateTime)) _valueType = ValueType.DateTimeValue;
                        else if (ParamType == typeof(double)) _valueType = ValueType.DoubleValue;
                        else if (ParamType == typeof(short)) _valueType = ValueType.Int16Value;
                        else if (ParamType == typeof(int)) _valueType = ValueType.Int32Value;
                        else if (ParamType == typeof(long)) _valueType = ValueType.Int64Value;
                        else if (ParamType == typeof(ushort)) _valueType = ValueType.UInt16Value;
                        else if (ParamType == typeof(uint)) _valueType = ValueType.UInt32Value;
                        else if (ParamType == typeof(ulong)) _valueType = ValueType.UInt64Value;
                        else if (ParamType == typeof(string)) _valueType = ValueType.StringValue;
                        else if (ParamType == typeof(byte)) _valueType = ValueType.ByteValue;
                        else if (ParamType == typeof(byte[])) _valueType = ValueType.ByteArrayValue;
                        else throw new ArgumentException($"Type {ParamType.Name} is unsupported. Supported types are null, bool, DateTime, double, short, int, long, ushort, uint, ulong, string, byte, and byte[].", nameof(Value));
                    }
                    _value = Value;
                }
                Changed?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Gets the type of data stored in the value.
        /// </summary>
        public ValueType ValueType
        {
            get { lock (_lock) { return _valueType; } }
        }

        /// <summary>
        /// Updates the value from the other side using a binary reader.
        /// </summary>
        /// <param name="Reader">The binary reader to read data from.</param>
        internal void Deserialize(BinaryReader Reader)
        {
            if (Reader == null) throw new ArgumentNullException(nameof(Reader));

            lock (_lock)
            {
                _isReadOnly = true;

                _name = Reader.ReadString();
                short readVal = Reader.ReadInt16();

                _valueType = (ValueType)readVal;
                if (_valueType == ValueType.NullValue) _value = null;
                else if (_valueType == ValueType.BoolValue) _value = Reader.ReadBoolean();
                else if (_valueType == ValueType.Int16Value) _value = Reader.ReadInt16();
                else if (_valueType == ValueType.Int32Value) _value = Reader.ReadInt32();
                else if (_valueType == ValueType.Int64Value) _value = Reader.ReadInt64();
                else if (_valueType == ValueType.UInt16Value) _value = Reader.ReadUInt16();
                else if (_valueType == ValueType.UInt32Value) _value = Reader.ReadUInt32();
                else if (_valueType == ValueType.UInt64Value) _value = Reader.ReadUInt64();
                else if (_valueType == ValueType.StringValue) _value = Reader.ReadString();
                else if (_valueType == ValueType.DateTimeValue) _value = new DateTime(Reader.ReadInt64());
                else if (_valueType == ValueType.DoubleValue) _value = Reader.ReadDouble();
                else if (_valueType == ValueType.ByteValue) _value = Reader.ReadByte();
                else if (_valueType == ValueType.ByteArrayValue) _value = Reader.ReadBytes(Reader.ReadInt32());
                else throw new NotImplementedException($"No code for {nameof(ValueType)} = {readVal}.");
            }

            Changed?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Appends the binary data for this token to an open binary writer.
        /// </summary>
        /// <param name="Writer">The binary writer to write data to.</param>
        public void Serialize(BinaryWriter Writer)
        {
            if (Writer == null) throw new ArgumentNullException(nameof(Writer));

            lock (_lock)
            {
                Writer.Write(_name);
                Writer.Write((short)_valueType);

                if (_valueType == ValueType.NullValue)
                    return;
                else if (_valueType == ValueType.BoolValue)
                    Writer.Write((bool)_value);
                else if (_valueType == ValueType.DateTimeValue)
                    Writer.Write(((DateTime)_value).Ticks);
                else if (_valueType == ValueType.DoubleValue)
                    Writer.Write((double)_value);
                else if (_valueType == ValueType.Int16Value)
                    Writer.Write((short)_value);
                else if (_valueType == ValueType.Int32Value)
                    Writer.Write((int)_value);
                else if (_valueType == ValueType.Int64Value)
                    Writer.Write((long)_value);
                else if (_valueType == ValueType.UInt16Value)
                    Writer.Write((ushort)_value);
                else if (_valueType == ValueType.UInt32Value)
                    Writer.Write((uint)_value);
                else if (_valueType == ValueType.UInt64Value)
                    Writer.Write((ulong)_value);
                else if (_valueType == ValueType.StringValue)
                    Writer.Write((string)_value);
                else if (_valueType == ValueType.ByteValue)
                    Writer.Write((byte)_value);
                else if (_valueType == ValueType.ByteArrayValue)
                {
                    // Prefix the data with an int of the length, followed by the data (required for deserialization).
                    byte[] ToWrite = (byte[])_value;
                    Writer.Write(ToWrite.Length);
                    Writer.Write(ToWrite);
                }
                else
                    throw new NotImplementedException($"No code for {nameof(ValueType)} = {_valueType}.");
            }
        }
    }
}
