using System;
using System.Globalization;
using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Core engine for processing socket communication. Handles the serialization of messages for transmission 
    /// and the reconstruction of received messages from byte streams.
    /// </summary>
    internal sealed partial class MessageEngine
    {
        /// <summary>
        /// Length of the message header in bytes.
        /// </summary>
        public const int HEADERLENGTH = 11;

        private bool _messageIsCompressed = false;
        private int _messageLength = 0;
        private int _messageLengthUncompressed = 0;
        private MessageType _messageType = MessageType.Unknown;
        private readonly byte[] _headerBuffer = new byte[HEADERLENGTH];
        private int _headerBufferPtr = 0;
        private bool _headerReceived = false;
        private byte[] _receiveBuffer = new byte[1024];
        private int _receiveBufferPtr = 0;
        private long _statMessageNumber = 1;
        private byte[] _uncompressedBuffer = new byte[1024];

        /// <summary>
        /// Processes bytes from the socket receive buffer into a message. Continues accumulating data
        /// until a complete message (including header and content) has been received.
        /// </summary>
        /// <param name="socketBytesRead">Number of bytes read from the socket</param>
        /// <param name="socketReceiveBuffer">Buffer containing received socket data</param>
        /// <param name="socketReceiveBufferPtr">Current position in the socket receive buffer</param>
        /// <returns>True if a complete message has been received; otherwise, false</returns>
        internal bool AddBytesFromSocketReceiveBuffer(int socketBytesRead, byte[] socketReceiveBuffer, ref int socketReceiveBufferPtr)
        {
            try
            {
                // Process message header if not yet received
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

                // Process message body after header is received
                if (_headerReceived)
                {
                    int bytesRequired = _messageLength - _receiveBufferPtr;
                    int bytesToCopy = Math.Min(bytesRequired, socketBytesRead - socketReceiveBufferPtr);

                    Buffer.BlockCopy(socketReceiveBuffer, socketReceiveBufferPtr, _receiveBuffer, _receiveBufferPtr, bytesToCopy);
                    _receiveBufferPtr += bytesToCopy;
                    socketReceiveBufferPtr += bytesToCopy;

                    if (_receiveBufferPtr == _messageLength)
                    {
                        Reset();
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
        /// Generates a byte array for transmission over a socket, including header and message content.
        /// Optionally compresses the message content if size threshold is met.
        /// </summary>
        /// <param name="SendObject">Message object to be sent</param>
        /// <param name="Compress">Whether to enable compression for large messages</param>
        /// <returns>Byte array containing the complete message ready for transmission</returns>
        public static byte[] GenerateSendBytes(IMessage SendObject, bool Compress)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                // Write header with placeholder values
                writer.Write(Convert.ToInt16(SendObject.MessageType, CultureInfo.InvariantCulture));
                writer.Write(false);   // Initial compression flag
                writer.Write(0);       // Placeholder for compressed length
                writer.Write(0);       // Placeholder for uncompressed length

                int messageBodyStartPosition = (int)memoryStream.Position;

                // Serialize message content
                SendObject.AppendBytes(writer);
                int messageBodyLength = (int)(memoryStream.Position - messageBodyStartPosition);

                byte[] finalBodyBytes;
                int finalBodyLength;

                // Apply compression for messages larger than 1KB if compression is enabled
                if (Compress && messageBodyLength > 1024)
                {
                    byte[] uncompressedBytes = new byte[messageBodyLength];
                    memoryStream.Position = messageBodyStartPosition;
                    memoryStream.Read(uncompressedBytes, 0, messageBodyLength);

                    byte[] compressedBytes = CLZF2.Compress(uncompressedBytes);

                    memoryStream.SetLength(messageBodyStartPosition);
                    memoryStream.Position = messageBodyStartPosition;
                    memoryStream.Write(compressedBytes, 0, compressedBytes.Length);

                    finalBodyBytes = compressedBytes;
                    finalBodyLength = compressedBytes.Length;

                    // Update header for compressed data
                    memoryStream.Position = 2;
                    writer.Write(true);
                    writer.Write(finalBodyLength);
                    writer.Write(messageBodyLength);
                }
                else
                {
                    finalBodyBytes = null;
                    finalBodyLength = messageBodyLength;

                    // Update header for uncompressed data
                    memoryStream.Position = 3;
                    writer.Write(finalBodyLength);
                    writer.Write(finalBodyLength);
                }

                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Gets the message type of the current message.
        /// </summary>
        internal MessageType MessageType => _messageType;

        /// <summary>
        /// Gets the length of the current message in bytes (possibly compressed).
        /// </summary>
        internal int MessageLength => _messageLength;

        /// <summary>
        /// Gets the uncompressed length of the current message in bytes.
        /// </summary>
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

        /// <summary>
        /// Extracts a MessageResponseV1 from the current message buffer.
        /// </summary>
        internal MessageResponseV1 GetMessageResponseV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new MessageResponseV1(reader);
            }
        }

        /// <summary>
        /// Extracts a TokenChangesResponseV1 from the current message buffer.
        /// </summary>
        internal TokenChangesResponseV1 GetSubscriptionChangesResponseV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new TokenChangesResponseV1(reader);
            }
        }

        /// <summary>
        /// Extracts a BroadcastV1 from the current message buffer.
        /// </summary>
        internal BroadcastV1 GetBroadcastV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new BroadcastV1(reader);
            }
        }

        /// <summary>
        /// Extracts a ClientDisconnectingNotificationV1 from the current message buffer.
        /// </summary>
        internal ClientDisconnectingNotificationV1 GetClientDisconnectingNotificationV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new ClientDisconnectingNotificationV1(reader);
            }
        }

        /// <summary>
        /// Extracts a Handshake1 message from the current message buffer.
        /// </summary>
        internal Handshake1 GetHandshake1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new Handshake1(reader);
            }
        }

        /// <summary>
        /// Extracts a Handshake2 message from the current message buffer.
        /// </summary>
        internal Handshake2 GetHandshake2()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new Handshake2(reader);
            }
        }

        /// <summary>
        /// Extracts a Handshake2Ack message from the current message buffer.
        /// </summary>
        internal Handshake2Ack GetHandshake2Ack()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new Handshake2Ack(reader);
            }
        }

        /// <summary>
        /// Extracts a TokenChangesRequestV1 from the current message buffer.
        /// </summary>
        internal TokenChangesRequestV1 GetSubscriptionChangesNotificationV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new TokenChangesRequestV1(reader);
            }
        }

        /// <summary>
        /// Extracts a MessageV1 from the current message buffer.
        /// </summary>
        internal MessageV1 GetMessageV1()
        {
            using (var stream = new MemoryStream(GetBuffer()))
            using (var reader = new BinaryReader(stream))
            {
                return new MessageV1(reader);
            }
        }

        /// <summary>
        /// Returns the appropriate buffer containing the message data. If the message is compressed,
        /// returns the uncompressed data; otherwise, returns the receive buffer directly.
        /// </summary>
        private byte[] GetBuffer()
        {
            if (!_messageIsCompressed) return _receiveBuffer;

            if (_uncompressedBuffer.Length < _messageLengthUncompressed)
                Array.Resize(ref _uncompressedBuffer, _messageLengthUncompressed);

            CLZF2.Decompress(_receiveBuffer, ref _uncompressedBuffer, _messageLength);
            return _uncompressedBuffer;
        }

        /// <summary>
        /// Resets the message engine state to prepare for receiving the next message.
        /// </summary>
        public void Reset()
        {
            _headerBufferPtr = 0;
            _receiveBufferPtr = 0;
            _headerReceived = false;
        }

        /// <summary>
        /// Parses the message header to extract message type, compression status, and size information.
        /// </summary>
        private void ParseHeader()
        {
            _messageType = (MessageType)BitConverter.ToInt16(_headerBuffer, 0);
            _messageIsCompressed = BitConverter.ToBoolean(_headerBuffer, 2);
            _messageLength = BitConverter.ToInt32(_headerBuffer, 3);
            _messageLengthUncompressed = BitConverter.ToInt32(_headerBuffer, 7);

            if (_messageLength > _receiveBuffer.Length)
                Array.Resize(ref _receiveBuffer, _messageLength);
        }

        /// <summary>
        /// Creates a detailed exception message including the current message number.
        /// </summary>
        private Exception BuildDetailedException(Exception ex)
        {
            return new Exception($"Error processing message {_statMessageNumber}: {ex.Message}");
        }
    }
}