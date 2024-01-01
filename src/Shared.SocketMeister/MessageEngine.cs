#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1805 // Do not initialize unnecessarily
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1825 // Avoid zero-length array allocations.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// This is the core engine for creating bytes to send down a socket and to receive bytes from a socket.
    /// </summary>
    internal sealed partial class MessageEngine
    {
        internal class Parse
        {
            public Parse(long MessageNumber, string MessageType, int MessageLength, int MessageLengthUncompressed)
            {
                this.MessageLength = MessageLength;
                this.MessageLengthUncompressed = MessageLengthUncompressed;
                this.MessageNumber = MessageNumber;
                this.MessageType = MessageType;
                ReceivedDateTime = DateTime.Now;
            }

            public long MessageNumber { get; set; }
            public string MessageType { get; set; }
            public int MessageLength { get; set; }
            public int MessageLengthUncompressed { get; set; }
            public bool MessageReceived { get; set; }
            public int SocketBufferLength { get; set; }
            public int SocketBytesRead { get; set; }
            public DateTime ReceivedDateTime { get; set; }
        }


        internal class ParseHistory
        {
            private readonly Queue<Parse> _items = new Queue<Parse>(20);

            public void Add(Parse message)
            {
                if (_items.Count >= 20) _items.Dequeue();
                _items.Enqueue(message);
            }

            public string Text
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(Environment.NewLine + "Receive History");
                    foreach (Parse m in _items)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "# {0} {1:HH:mm:ss.fff}, MessageType: {2}, MessageLength: {3} ({4} uncompressed), SocketBytesRead: {5}(BufSize: {6}), MsgComplete: {7}",
                            m.MessageNumber, m.ReceivedDateTime, m.MessageType, m.MessageLength, m.MessageLengthUncompressed, m.SocketBytesRead, m.SocketBufferLength, m.MessageReceived ? "Y" : "N")
                          .AppendLine();
                    }
                    return sb.ToString();
                }
            }
        }



        public const int HEADERLENGTH = 11;

        private readonly bool _enableCompression;
        private readonly ParseHistory _history = new ParseHistory();
        private bool _messageIsCompressed = false;
        private int _messageLength = 0;
        private int _messageLengthUncompressed = 0;
        private MessageEngineMessageType _messageType = MessageEngineMessageType.Unknown;
        private readonly byte[] _headerBuffer = new byte[11];
        private int _headerBufferPtr = 0;
        private bool _headerReceived = false;
        private byte[] _receiveBuffer = new byte[0];
        private int _receiveBufferPtr = 0;
        private long _statMessageNumber = 1;
        private bool _statMessageReceived;
        private int _statSocketBufferLength;
        private int _statSocketBytesRead;
        private byte[] _uncompressedBuffer = new byte[0];

        internal MessageEngine(bool EnableCompression)
        {
            _enableCompression = EnableCompression;
        }

        internal bool AddBytesFromSocketReceiveBuffer(int SocketBytesRead, byte[] SocketReceiveBuffer, ref int SocketReceiveBufferPtr)
        {
            _statSocketBufferLength = SocketReceiveBuffer.Length;
            _statSocketBytesRead = SocketBytesRead;
            _statMessageReceived = false;

            if (_headerReceived == false)
            {
                int bytesRequired = 0;
                int bytesPossible = 0;
                try
                {
                    bytesRequired = HEADERLENGTH - _headerBufferPtr;
                    bytesPossible = bytesRequired;
                    if ((SocketBytesRead - SocketReceiveBufferPtr) < bytesRequired) bytesPossible = (SocketBytesRead - SocketReceiveBufferPtr);
                    Buffer.BlockCopy(SocketReceiveBuffer, SocketReceiveBufferPtr, _headerBuffer, _headerBufferPtr, bytesPossible);
                    _headerBufferPtr += bytesPossible;
                    SocketReceiveBufferPtr += bytesPossible;

                    if (_headerBufferPtr == HEADERLENGTH)
                    {
                        //  SETUP TO RECEIVE THE ENTIRE MESSAGE
                        _headerReceived = true;

                        //  SETUP MESSAGE BODY BYTE ARRAY FOR THE EXPECTED BYTES
                        _messageType = (MessageEngineMessageType)Convert.ToInt16(_headerBuffer[0] | (_headerBuffer[1] << 8));
                        _messageIsCompressed = Convert.ToBoolean(_headerBuffer[2]);
                        _messageLength = _headerBuffer[3] | (_headerBuffer[4] << 8) | (_headerBuffer[5] << 16) | (_headerBuffer[6] << 24);
                        _messageLengthUncompressed = _headerBuffer[7] | (_headerBuffer[8] << 8) | (_headerBuffer[9] << 16) | (_headerBuffer[10] << 24);

                        if (Enum.IsDefined(typeof(MessageEngineMessageType), _messageType) == false)
                        {
                            throw new Exception("Invalid Message Type");
                        }

                        //  ONLY EXTEND THE MESSAGE BUFFER IF IT IS TOO SMALL FOR THE INCOMING MESSAGE.
                        //  THIS PROVIDES IMPROVED SPEED AND MUCH LESS WORK FOR THE GARBAGE COLLECTOR (BECAUSE WE ARE REUSING THE BUFFER)
                        if (_messageLength > _receiveBuffer.Length) _receiveBuffer = new byte[_messageLength];
                    }
                }
                catch (Exception ex)
                {
                    StringBuilder msgBuilder = new StringBuilder();
                    msgBuilder.Append("Error processing Message Body. _messageLength: ");
                    msgBuilder.Append(_messageLength);
                    msgBuilder.Append("(");
                    msgBuilder.Append(_messageLengthUncompressed);
                    msgBuilder.Append(" uncompressed), ReceivedByesCount: ");
                    msgBuilder.Append(SocketBytesRead);
                    msgBuilder.Append(", SocketReceiveBuffer.Length: ");
                    msgBuilder.Append(SocketReceiveBuffer.Length);
                    msgBuilder.Append(", SocketReceiveBufferPtr: ");
                    msgBuilder.Append(SocketReceiveBufferPtr);
                    msgBuilder.Append(", _receiveBuffer.Length: ");
                    msgBuilder.Append(_receiveBuffer.Length);
                    msgBuilder.Append(", _receiveBufferPtr: ");
                    msgBuilder.Append(_receiveBufferPtr);
                    msgBuilder.Append(", bytesRequired: ");
                    msgBuilder.Append(bytesRequired);
                    msgBuilder.Append(", BytesPossible: ");
                    msgBuilder.Append(bytesPossible);
                    msgBuilder.Append(", _messageType: ");
                    msgBuilder.Append(_messageType.ToString());
                    msgBuilder.Append(", _messageIsCompressed: ");
                    msgBuilder.Append(_messageIsCompressed.ToString(CultureInfo.InvariantCulture));
                    msgBuilder.Append(", MessageCount: ");
                    msgBuilder.Append(_statMessageNumber);
                    msgBuilder.Append(_history.Text);
                    msgBuilder.Append(Environment.NewLine);
                    msgBuilder.Append(Environment.NewLine);
                    msgBuilder.Append(ex.Message);

                    throw new Exception(msgBuilder.ToString());
                }
            }
            if (_headerReceived == true && (SocketReceiveBufferPtr < SocketBytesRead || _messageLength == 0))
            {
                int bytesRequired = 0;
                int bytesPossible = 0;
                try
                {
                    bytesRequired = _messageLength - _receiveBufferPtr;
                    bytesPossible = bytesRequired;
                    if ((SocketBytesRead - SocketReceiveBufferPtr) < bytesRequired) bytesPossible = (SocketBytesRead - SocketReceiveBufferPtr);
                    Buffer.BlockCopy(SocketReceiveBuffer, SocketReceiveBufferPtr, _receiveBuffer, _receiveBufferPtr, bytesPossible);
                    _receiveBufferPtr += bytesPossible;
                    SocketReceiveBufferPtr += bytesPossible;

                    if (_receiveBufferPtr == _messageLength)
                    {
                        _statMessageReceived = true;
                        Reset();
                        AddParseAttemptDetails();
                        _statMessageNumber++;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    StringBuilder msgBuilder = new StringBuilder();
                    msgBuilder.Append("Error processing Message Body. _messageLength: ");
                    msgBuilder.Append(_messageLength);
                    msgBuilder.Append("(");
                    msgBuilder.Append(_messageLengthUncompressed);
                    msgBuilder.Append(" uncompressed), ReceivedByesCount: ");
                    msgBuilder.Append(SocketBytesRead);
                    msgBuilder.Append(", SocketReceiveBuffer.Length: ");
                    msgBuilder.Append(SocketReceiveBuffer.Length);
                    msgBuilder.Append(", SocketReceiveBufferPtr: ");
                    msgBuilder.Append(SocketReceiveBufferPtr);
                    msgBuilder.Append(", _receiveBuffer.Length: ");
                    msgBuilder.Append(_receiveBuffer.Length);
                    msgBuilder.Append(", _receiveBufferPtr: ");
                    msgBuilder.Append(_receiveBufferPtr);
                    msgBuilder.Append(", bytesRequired: ");
                    msgBuilder.Append(bytesRequired);
                    msgBuilder.Append(", BytesPossible: ");
                    msgBuilder.Append(bytesPossible);
                    msgBuilder.Append(", _messageType: ");
                    msgBuilder.Append(_messageType.ToString());
                    msgBuilder.Append(", _messageIsCompressed: ");
                    msgBuilder.Append(_messageIsCompressed.ToString(CultureInfo.InvariantCulture));
                    msgBuilder.Append(", MessageCount: ");
                    msgBuilder.Append(_statMessageNumber);
                    msgBuilder.Append(_history.Text);
                    msgBuilder.Append(Environment.NewLine);
                    msgBuilder.Append(Environment.NewLine);
                    msgBuilder.Append(ex.Message);

                    throw new Exception(msgBuilder.ToString());
                }

            }
            AddParseAttemptDetails();
            return false;
        }


        /// <summary>
        /// Generate send bytes for a transmit operation.
        /// </summary>
        /// <param name = "SendObject" > Object to be sent</param>
        /// <param name = "Compress" > Compress the data</param>
        /// <returns>Byte array of the data to be sent</returns>
        public static byte[] GenerateSendBytes(IMessage SendObject, bool Compress)
        {
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                // Initial Header (placeholder values)
                writer.Write(Convert.ToInt16(SendObject.MessageType, CultureInfo.InvariantCulture));
                writer.Write(false);  // Compression flag
                writer.Write(0);      // Placeholder for compressed length
                writer.Write(0);      // Placeholder for uncompressed length

                // Write user bytes
                int messageBodyStartPosition = Convert.ToInt32(writer.BaseStream.Position);
                SendObject.AppendBytes(writer);
                int messageBodyLength = Convert.ToInt32(writer.BaseStream.Position - messageBodyStartPosition);

                // Decide whether to compress
                if (Compress && messageBodyLength > 1024)
                {
                    byte[] uncompressedBytes = new byte[messageBodyLength];
                    writer.BaseStream.Position = messageBodyStartPosition;
                    writer.BaseStream.Read(uncompressedBytes, 0, messageBodyLength);

                    byte[] compressedBytes = CLZF2.Compress(uncompressedBytes);

                    // The messageBodyLength is now the compressed message length
                    messageBodyLength = compressedBytes.Length;

                    // Write compressed data over uncompressed data
                    writer.BaseStream.Position = messageBodyStartPosition;
                    writer.Write(compressedBytes);

                    // Update Header
                    writer.BaseStream.Position = 2; // 2 bytes for MessageType
                    writer.Write(true);  // Compression flag
                    writer.Write(compressedBytes.Length);
                    writer.Write(messageBodyLength);
                }
                else
                {
                    // Update Header for uncompressed data
                    writer.BaseStream.Position = 3;         // 2 bytes for MessageType + 1 byte for compressed flag
                    writer.Write(messageBodyLength);        // Compressed length (0 for uncompressed)
                    writer.Write(messageBodyLength);
                }

                // Return byte array
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;

                    int totalLength = messageBodyStartPosition + messageBodyLength;
                    return reader.ReadBytes(totalLength);
                }
            }
        }



        //    public static byte[] GenerateSendBytes(IMessage SendObject, bool Compress)
        //{
        //    using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
        //    {
        //        //  WRITE HEADER
        //        writer.Write(Convert.ToInt16(SendObject.MessageType, CultureInfo.InvariantCulture));

        //        if (Compress == false)
        //        {
        //            //  WRITE HEADER
        //            writer.Write(false);    //  NOT COMPRESSED
        //            writer.Write(0);   //  WILL BE USED TO WRITE _compressedLength
        //            writer.Write(0);   //  WILL BE USED TO WRITE _uncompressedLength

        //            //  WRITE USER BYTES
        //            long position1 = writer.BaseStream.Position;
        //            SendObject.AppendBytes(writer);
        //            int messageLength = Convert.ToInt32(writer.BaseStream.Position - position1);
        //            int messageLengthUncompressed = messageLength;

        //            //  GO BACK AND FINISH HEADER
        //            writer.BaseStream.Position = 3;
        //            writer.Write(messageLength);
        //            writer.Write(messageLengthUncompressed);
        //        }
        //        else
        //        {
        //            byte[] uncompressedBytes = null;
        //            using (BinaryWriter writer2 = new BinaryWriter(new MemoryStream()))
        //            {
        //                SendObject.AppendBytes(writer2);

        //                using (BinaryReader reader2 = new BinaryReader(writer2.BaseStream))
        //                {
        //                    reader2.BaseStream.Position = 0;
        //                    uncompressedBytes = reader2.ReadBytes(Convert.ToInt32(reader2.BaseStream.Length));
        //                }
        //            }

        //            if (uncompressedBytes != null && uncompressedBytes.Length > 1024)
        //            {
        //                writer.Write(true);     //  COMPRESSED
        //                byte[] compressedBytes = CLZF2.Compress(uncompressedBytes);
        //                writer.Write(compressedBytes.Length);
        //                writer.Write(uncompressedBytes.Length);
        //                writer.Write(compressedBytes);
        //            }
        //            else
        //            {
        //                writer.Write(false);     //  UNCOMPRESSED
        //                writer.Write(0);   //  WILL BE USED TO WRITE _compressedLength
        //                writer.Write(0);   //  WILL BE USED TO WRITE _uncompressedLength

        //                //  WRITE USER BYTES
        //                long position1 = writer.BaseStream.Position;
        //                SendObject.AppendBytes(writer);
        //                int messageLength2 = Convert.ToInt32(writer.BaseStream.Position - position1);
        //                int messageLengthUncompressed2 = messageLength2;

        //                //  GO BACK AND FINISH HEADER
        //                writer.BaseStream.Position = 3;
        //                writer.Write(messageLength2);
        //                writer.Write(messageLengthUncompressed2);
        //            }


        //        }
        //        using (BinaryReader reader = new BinaryReader(writer.BaseStream))
        //        {
        //            reader.BaseStream.Position = 0;
        //            return reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));

        //        }
        //    }
        //}









    /// <summary>
    /// The type of message. 
    /// </summary>
    internal MessageEngineMessageType MessageType => _messageType;

        internal int MessageLength => _messageLength;

        internal int MessageLengthUncompressed => _messageLengthUncompressed;


        private void AddParseAttemptDetails()
        {
            Parse p = new Parse(_statMessageNumber, _messageType.ToString(), _messageLength, _messageLengthUncompressed)
            {
                MessageReceived = _statMessageReceived,
                SocketBytesRead = _statSocketBytesRead,
                SocketBufferLength = _statSocketBufferLength
            };
            _history.Add(p);
        }


        internal ServerStoppingNotificationV1 GetServerStoppingNotificationV1()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new ServerStoppingNotificationV1(reader);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }


        internal MessageV1 GetMessageV1()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new MessageV1(reader);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        internal BroadcastV1 GetBroadcastV1()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new BroadcastV1(reader);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }



        internal TokenChangesRequestV1 GetSubscriptionChangesNotificationV1()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new TokenChangesRequestV1(reader);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }


        internal TokenChangesResponseV1 GetSubscriptionChangesResponseV1()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new TokenChangesResponseV1(reader);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }



        internal MessageResponseV1 GetMessageResponseV1()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new MessageResponseV1(reader);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private byte[] GetBuffer()
        {
            if (_messageIsCompressed == false) return _receiveBuffer;
            //  UNCOMPRESS
            if (_uncompressedBuffer.Length < _messageLengthUncompressed) _uncompressedBuffer = new byte[_messageLengthUncompressed];
            CLZF2.Decompress(_receiveBuffer, ref _uncompressedBuffer, _messageLength);
            return _uncompressedBuffer;
        }

        /// <summary>
        /// Resets the envelope for a fresh receive operation. Warning: should not be done in the middle of a receive operation.
        /// </summary>
        public void Reset()
        {
            _headerBufferPtr = 0;
            _receiveBufferPtr = 0;
            _headerReceived = false;
        }


    }
}

#pragma warning restore CA1825 // Avoid zero-length array allocations.
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression

