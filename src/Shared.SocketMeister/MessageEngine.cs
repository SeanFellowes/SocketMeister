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
                    var sb = new StringBuilder(1024);
                    sb.AppendLine("\nReceive History");
                    foreach (var m in _items)
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
        private readonly byte[] _headerBuffer = new byte[HEADERLENGTH];
        private int _headerBufferPtr = 0;
        private bool _headerReceived = false;
        private byte[] _receiveBuffer = new byte[1024];
        private int _receiveBufferPtr = 0;
        private long _statMessageNumber = 1;
        private bool _statMessageReceived;
        private int _statSocketBufferLength;
        private int _statSocketBytesRead;
        private byte[] _uncompressedBuffer = new byte[1024];

        internal MessageEngine(bool EnableCompression)
        {
            _enableCompression = EnableCompression;
        }

        internal bool AddBytesFromSocketReceiveBuffer(int socketBytesRead, byte[] socketReceiveBuffer, ref int socketReceiveBufferPtr)
        {
            try
            {
                // Handle header
                if (!_headerReceived)
                {
                    int bytesRequired = HEADERLENGTH - _headerBufferPtr;
                    int bytesToCopy = Math.Min(bytesRequired, socketBytesRead - socketReceiveBufferPtr);

                    Buffer.BlockCopy(socketReceiveBuffer, socketReceiveBufferPtr, _headerBuffer, _headerBufferPtr, bytesToCopy);
                    _headerBufferPtr += bytesToCopy;
                    socketReceiveBufferPtr += bytesToCopy;

                    if (_headerBufferPtr == HEADERLENGTH)
                    {
                        _headerReceived = true;
                        ParseHeader();
                    }
                }

                // Handle message body
                if (_headerReceived)
                {
                    int bytesRequired = _messageLength - _receiveBufferPtr;
                    int bytesToCopy = Math.Min(bytesRequired, socketBytesRead - socketReceiveBufferPtr);

                    Buffer.BlockCopy(socketReceiveBuffer, socketReceiveBufferPtr, _receiveBuffer, _receiveBufferPtr, bytesToCopy);
                    _receiveBufferPtr += bytesToCopy;
                    socketReceiveBufferPtr += bytesToCopy;

                    if (_receiveBufferPtr == _messageLength)
                    {
                        _statMessageReceived = true;
                        Reset();
                        AddParseAttemptDetails();
                        _statMessageNumber++;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw BuildDetailedException(ex);
            }

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
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                // Write the initial header with placeholder values
                writer.Write(Convert.ToInt16(SendObject.MessageType, CultureInfo.InvariantCulture)); // Message type (2 bytes)
                writer.Write(false);   // Compression flag (1 byte)
                writer.Write(0);       // Placeholder for compressed length (4 bytes)
                writer.Write(0);       // Placeholder for uncompressed length (4 bytes)

                // Mark the start of the message body
                int messageBodyStartPosition = (int)memoryStream.Position;

                // Write the message body
                SendObject.AppendBytes(writer);
                int messageBodyLength = (int)(memoryStream.Position - messageBodyStartPosition);

                // Decide whether to compress
                byte[] finalBodyBytes;
                int finalBodyLength;

                // Compression threshold: Avoid compressing small messages to reduce CPU overhead
                if (Compress && messageBodyLength > 1024)
                {
                    // Extract uncompressed bytes
                    byte[] uncompressedBytes = new byte[messageBodyLength];
                    memoryStream.Position = messageBodyStartPosition;
                    memoryStream.Read(uncompressedBytes, 0, messageBodyLength);

                    // Compress the bytes
                    byte[] compressedBytes = CLZF2.Compress(uncompressedBytes);

                    // Replace message body with compressed data
                    memoryStream.SetLength(messageBodyStartPosition); // Truncate stream to remove uncompressed data
                    memoryStream.Position = messageBodyStartPosition;
                    memoryStream.Write(compressedBytes, 0, compressedBytes.Length);

                    finalBodyBytes = compressedBytes;
                    finalBodyLength = compressedBytes.Length;

                    // Update header for compressed data
                    memoryStream.Position = 2; // After MessageType (2 bytes)
                    writer.Write(true);             // Compression flag (set to true)
                    writer.Write(finalBodyLength);  // Compressed length
                    writer.Write(messageBodyLength); // Uncompressed length
                }
                else
                {
                    // No compression: Use the existing uncompressed data
                    finalBodyBytes = null; // Not used
                    finalBodyLength = messageBodyLength;

                    // Update header for uncompressed data
                    memoryStream.Position = 3;          // After MessageType + Compression flag
                    writer.Write(finalBodyLength);      // Compressed length (same as uncompressed)
                    writer.Write(finalBodyLength);      // Uncompressed length
                }

                // Return the full byte array
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        //public static byte[] GenerateSendBytes(IMessage SendObject, bool Compress)
        //{
        //    using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
        //    {
        //        // Initial Header (placeholder values)
        //        writer.Write(Convert.ToInt16(SendObject.MessageType, CultureInfo.InvariantCulture));
        //        writer.Write(false);  // Compression flag
        //        writer.Write(0);      // Placeholder for compressed length
        //        writer.Write(0);      // Placeholder for uncompressed length

        //        // Write user bytes
        //        int messageBodyStartPosition = Convert.ToInt32(writer.BaseStream.Position);
        //        SendObject.AppendBytes(writer);
        //        int messageBodyLength = Convert.ToInt32(writer.BaseStream.Position - messageBodyStartPosition);

        //        // Decide whether to compress
        //        if (Compress && messageBodyLength > 1024)
        //        {
        //            byte[] uncompressedBytes = new byte[messageBodyLength];
        //            writer.BaseStream.Position = messageBodyStartPosition;
        //            writer.BaseStream.Read(uncompressedBytes, 0, messageBodyLength);

        //            byte[] compressedBytes = CLZF2.Compress(uncompressedBytes);

        //            // The messageBodyLength is now the compressed message length
        //            messageBodyLength = compressedBytes.Length;

        //            // Write compressed data over uncompressed data
        //            writer.BaseStream.Position = messageBodyStartPosition;
        //            writer.Write(compressedBytes);

        //            // Update Header
        //            writer.BaseStream.Position = 2; // 2 bytes for MessageType
        //            writer.Write(true);  // Compression flag
        //            writer.Write(compressedBytes.Length);
        //            writer.Write(messageBodyLength);
        //        }
        //        else
        //        {
        //            // Update Header for uncompressed data
        //            writer.BaseStream.Position = 3;         // 2 bytes for MessageType + 1 byte for compressed flag
        //            writer.Write(messageBodyLength);        // Compressed length (0 for uncompressed)
        //            writer.Write(messageBodyLength);
        //        }

        //        // Return byte array
        //        using (BinaryReader reader = new BinaryReader(writer.BaseStream))
        //        {
        //            reader.BaseStream.Position = 0;

        //            int totalLength = messageBodyStartPosition + messageBodyLength;
        //            return reader.ReadBytes(totalLength);
        //        }
        //    }
        //}







        /// <summary>
        /// The type of message. 
        /// </summary>
        internal MessageEngineMessageType MessageType => _messageType;

        internal int MessageLength => _messageLength;

        internal int MessageLengthUncompressed => _messageLengthUncompressed;


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


        internal MessageResponseV1 GetMessageResponseV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new MessageResponseV1(reader);
            }
        }

        internal TokenChangesResponseV1 GetSubscriptionChangesResponseV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new TokenChangesResponseV1(reader);
            }
        }

        internal BroadcastV1 GetBroadcastV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new BroadcastV1(reader);
            }
        }

        internal TokenChangesRequestV1 GetSubscriptionChangesNotificationV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new TokenChangesRequestV1(reader);
            }
        }

        internal MessageV1 GetMessageV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new MessageV1(reader);
            }
        }

        private byte[] GetBuffer()
        {
            if (!_messageIsCompressed) return _receiveBuffer;

            if (_uncompressedBuffer.Length < _messageLengthUncompressed)
                Array.Resize(ref _uncompressedBuffer, _messageLengthUncompressed);

            CLZF2.Decompress(_receiveBuffer, ref _uncompressedBuffer, _messageLength);
            return _uncompressedBuffer;
        }

        public void Reset()
        {
            _headerBufferPtr = 0;
            _receiveBufferPtr = 0;
            _headerReceived = false;
        }


        private void ParseHeader()
        {
            _messageType = (MessageEngineMessageType)BitConverter.ToInt16(_headerBuffer, 0);
            _messageIsCompressed = BitConverter.ToBoolean(_headerBuffer, 2);
            _messageLength = BitConverter.ToInt32(_headerBuffer, 3);
            _messageLengthUncompressed = BitConverter.ToInt32(_headerBuffer, 7);

            if (!Enum.IsDefined(typeof(MessageEngineMessageType), _messageType))
                throw new Exception("Invalid Message Type");

            if (_messageLength > _receiveBuffer.Length)
                Array.Resize(ref _receiveBuffer, _messageLength);
        }

        private Exception BuildDetailedException(Exception ex)
        {
            return new Exception($"Error processing message {_statMessageNumber}: {ex.Message}");
        }


        private void AddParseAttemptDetails()
        {
            _history.Add(new Parse(_statMessageNumber, _messageType.ToString(), _messageLength, _messageLengthUncompressed)
            {
                MessageReceived = _statMessageReceived
            });
        }
    }
}