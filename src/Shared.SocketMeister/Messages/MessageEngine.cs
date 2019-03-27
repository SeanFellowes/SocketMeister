

using System;
using System.Collections.Generic;
using System.IO;
using SocketMeister.Messages;

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
                this.ReceivedDateTime = DateTime.Now;
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
            private List<Parse> _items = new List<Parse>();

            public void Add(Parse message)
            {
                if (_items.Count > 19) _items.RemoveAt(0);
                _items.Add(message);
            }

            public string Text
            {
                get
                {
                    string T = Environment.NewLine + Environment.NewLine + "Receive History" + Environment.NewLine;
                    foreach (Parse m in _items)
                    {
                        T += "#" + m.MessageNumber + " " + m.ReceivedDateTime.ToString("HH:mm:ss.fff");
                        T += ", MessageType: " + m.MessageType;
                        T += ", MessageLength: " + m.MessageLength + " (" + m.MessageLengthUncompressed + " uncompressed)";
                        T += ", SocketBytesRead: " + m.SocketBytesRead + "(BufSize: " + m.SocketBufferLength + ")";
                        if (m.MessageReceived == true) T += ", MsgComplete: Y";
                        else T += ", MsgComplete: N";
                        T += Environment.NewLine;
                    }
                    return T;
                }
            }
        }


        public const int HEADERLENGTH = 11;

        private readonly bool _enableCompression;
        private ParseHistory _history = new ParseHistory();
        private bool _messageIsCompressed = false;
        private int _messageLength = 0;
        private int _messageLengthUncompressed = 0;
        private MessageTypes _messageType = MessageTypes.Unknown;
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
                        _messageType = (MessageTypes)Convert.ToInt16(_headerBuffer[0] | (_headerBuffer[1] << 8));
                        _messageIsCompressed = Convert.ToBoolean(_headerBuffer[2]);
                        _messageLength = _headerBuffer[3] | (_headerBuffer[4] << 8) | (_headerBuffer[5] << 16) | (_headerBuffer[6] << 24);
                        _messageLengthUncompressed = _headerBuffer[7] | (_headerBuffer[8] << 8) | (_headerBuffer[9] << 16) | (_headerBuffer[10] << 24);

                        if (Enum.IsDefined(typeof(MessageTypes), _messageType) == false)
                        {
                            throw new Exception("Invalid Message Type");
                        }

                        //  ONLY EXTEND THE MESSAGE BUFFER IF IT IS TOO SMALL FOR THE INCOMING MESSAGE.
                        //  THIS PROVIDES IMPROVED SPEED AND MUCH LESS WORK FOR THE GARBAGE COLLECTOR (BECAUSE WE ARE REUSING THE BUFFER)
                        if (_messageLength > _receiveBuffer.Length) _receiveBuffer = new byte[_messageLength];
                        // _receiveBuffer = new byte[_messageLength];

                        //  WE MAY NEED THIS
                        ////  WRITE THE MESSAGE LENGTH IN THE FIRST 4 BYTES (THIS LOOKS CONVOLUTED BUT ACTUALLY IS THE FASTEST WAY POSSIBLE)
                        //_sendBytes[0] = (byte)(_uncompressedLength >> 24);
                        //_sendBytes[1] = (byte)(_uncompressedLength >> 16);
                        //_sendBytes[2] = (byte)(_uncompressedLength >> 8);
                        //_sendBytes[3] = (byte)_uncompressedLength;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Error processing Message Body. _messageLength: " + _messageLength + "(" + _messageLengthUncompressed + " uncompressed), ReceivedByesCount: " + SocketBytesRead + ", SocketReceiveBuffer.Length: " + SocketReceiveBuffer.Length;
                    msg += ", SocketReceiveBufferPtr: " + SocketReceiveBufferPtr;
                    msg += ", _receiveBuffer.Length: " + _receiveBuffer.Length + ", _receiveBufferPtr: " + _receiveBufferPtr;
                    msg += ", bytesRequired: " + bytesRequired + ", BytesPossible: " + bytesPossible;
                    msg += ", _messageType: " + _messageType.ToString() + ", _messageIsCompressed: " + _messageIsCompressed.ToString();
                    msg += ", MessageCount: " + _statMessageNumber;
                    msg += _history.Text;
                    msg += Environment.NewLine + Environment.NewLine;
                    msg += ex.Message;
                    throw new Exception(msg);
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
                    string msg = "Error processing Message Body. _messageLength: " + _messageLength + "(" + _messageLengthUncompressed + " uncompressed), ReceivedByesCount: " + SocketBytesRead + ", SocketReceiveBuffer.Length: " + SocketReceiveBuffer.Length;
                    msg += ", SocketReceiveBufferPtr: " + SocketReceiveBufferPtr;
                    msg += ", _receiveBuffer.Length: " + _receiveBuffer.Length + ", _receiveBufferPtr: " + _receiveBufferPtr;
                    msg += ", bytesRequired: " + bytesRequired + ", BytesPossible: " + bytesPossible;
                    msg += ", _messageType: " + _messageType.ToString() + ", _messageIsCompressed: " + _messageIsCompressed.ToString();
                    msg += ", MessageCount: " + _statMessageNumber;
                    msg += _history.Text;
                    msg += Environment.NewLine + Environment.NewLine;
                    msg += ex.Message;
                    throw new Exception(msg);
                }

            }
            AddParseAttemptDetails();
            return false;
        }


        /// <summary>
        /// Generate send bytes for a transmit operation.
        /// </summary>
        /// <param name="SendObject">Object to be sent</param>
        /// <param name="Compress">Compress the data</param>
        /// <returns>Byte array of the data to be sent</returns>
        public static byte[] GenerateSendBytes(IMessage SendObject, bool Compress)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    //  WRITE HEADER
                    writer.Write(Convert.ToInt16(SendObject.MessageType));

                    if (Compress == false)
                    {
                        //  WRITE HEADER
                        writer.Write(false);    //  NOT COMPRESSED
                        writer.Write((int)0);   //  WILL BE USED TO WRITE _compressedLength
                        writer.Write((int)0);   //  WILL BE USED TO WRITE _uncompressedLength

                        //  WRITE USER BYTES
                        long position1 = ms.Position;
                        SendObject.AppendBytes(writer);
                        int messageLength = Convert.ToInt32(ms.Position - position1);
                        int messageLengthUncompressed = messageLength;

                        //  GO BACK AND FINISH HEADER
                        ms.Position = 3;
                        writer.Write(messageLength);
                        writer.Write(messageLengthUncompressed);
                    }
                    else
                    {
                        //  WRITE HEADER

                        using (MemoryStream msUncompressed = new MemoryStream())
                        {
                            using (BinaryWriter writer2 = new BinaryWriter(msUncompressed))
                            {
                                SendObject.AppendBytes(writer2);

                                if (writer2.BaseStream.Length > 1024)
                                {
                                    writer.Write(true);     //  COMPRESSED
                                    byte[] uncompressedBytes = msUncompressed.ToArray();
                                    byte[] compressedBytes = SocketMeister.Compression.CLZF2.Compress(msUncompressed.ToArray());
                                    int messageLengthUncompressed = uncompressedBytes.Length;
                                    int messageLength = compressedBytes.Length;

                                    writer.Write(messageLength);
                                    writer.Write(messageLengthUncompressed);
                                    writer.Write(compressedBytes);

                                }
                                else
                                {
                                    writer.Write(false);     //  UNCOMPRESSED
                                    writer.Write((int)0);   //  WILL BE USED TO WRITE _compressedLength
                                    writer.Write((int)0);   //  WILL BE USED TO WRITE _uncompressedLength

                                    //  WRITE USER BYTES
                                    long position1 = ms.Position;
                                    SendObject.AppendBytes(writer);
                                    int messageLength2 = Convert.ToInt32(ms.Position - position1);
                                    int messageLengthUncompressed2 = messageLength2;

                                    //  GO BACK AND FINISH HEADER
                                    ms.Position = 3;
                                    writer.Write(messageLength2);
                                    writer.Write(messageLengthUncompressed2);
                                }

                            }
                        }
                    }
                }
                return ms.ToArray();
            }
        }


        /// <summary>
        /// The type of message. 
        /// </summary>
        internal MessageTypes MessageType
        {
            get { return _messageType; }
        }


        private void AddParseAttemptDetails()
        {
            Parse p = new Parse(_statMessageNumber, _messageType.ToString(), _messageLength, _messageLengthUncompressed);
            p.MessageReceived = _statMessageReceived;
            p.SocketBytesRead = _statSocketBytesRead;
            p.SocketBufferLength = _statSocketBufferLength;
            _history.Add(p);
        }


        internal ServerStoppingMessage GetDisconnectRequest()
        {
            using (MemoryStream ms = new MemoryStream(GetBuffer()))
            {
                using (BinaryReader bR = new BinaryReader(ms))
                {
                    return new ServerStoppingMessage(bR);
                }
            }
        }

        internal Message GetMessage()
        {
            using (MemoryStream ms = new MemoryStream(GetBuffer()))
            {
                using (BinaryReader bR = new BinaryReader(ms))
                {
                    return new Message(bR);
                }
            }
        }


        internal RequestMessage GetRequestMessage()
        {
            using (MemoryStream ms = new MemoryStream(GetBuffer()))
            {
                using (BinaryReader bR = new BinaryReader(ms))
                {
                    return new RequestMessage(bR);
                }
            }
        }


        internal ResponseMessage GetResponseMessage()
        {
            using (MemoryStream ms = new MemoryStream(GetBuffer()))
            {
                using (BinaryReader bR = new BinaryReader(ms))
                {
                    return new ResponseMessage(bR);
                }
            }
        }

        private byte[] GetBuffer()
        {
            if (_messageIsCompressed == false) return _receiveBuffer;
            //  UNCOMPRESS
            if (_uncompressedBuffer.Length < _messageLengthUncompressed) _uncompressedBuffer = new byte[_messageLengthUncompressed];
            SocketMeister.Compression.CLZF2.Decompress(_receiveBuffer, ref _uncompressedBuffer, _messageLength);
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
