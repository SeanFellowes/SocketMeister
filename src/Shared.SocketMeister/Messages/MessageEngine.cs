
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// This is the core engine for creating bytes to send down a socket and to receive bytes from a socket.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1825:AvoidZeroLegthArray", MessageId = "UnsupportedInEarlyDotNetVersions")]
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
            private readonly List<Parse> items = new List<Parse>();

            public void Add(Parse message)
            {
                if (items.Count > 19) items.RemoveAt(0);
                items.Add(message);
            }

            public string Text
            {
                get
                {
                    string T = Environment.NewLine + Environment.NewLine + "Receive History" + Environment.NewLine;
                    foreach (Parse m in items)
                    {
                        T += "#" + m.MessageNumber + " " + m.ReceivedDateTime.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
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

        private readonly byte[] headerBuffer = new byte[11];
        private int headerBufferPtr = 0;
        private bool headerReceived = false;
        private readonly ParseHistory history = new ParseHistory();
        private bool messageIsCompressed = false;
        private int messageLength = 0;
        private int messageLengthUncompressed = 0;
        private MessageTypes messageType = MessageTypes.Unknown;
        private byte[] receiveBuffer = new byte[0];
        private int receiveBufferPtr = 0;
        private long statMessageNumber = 1;
        private bool statMessageReceived;
        private int statSocketBufferLength;
        private int statSocketBytesRead;
        private byte[] uncompressedBuffer = new byte[0];

        internal bool AddBytesFromSocketReceiveBuffer(int SocketBytesRead, byte[] SocketReceiveBuffer, ref int SocketReceiveBufferPtr)
        {
            statSocketBufferLength = SocketReceiveBuffer.Length;
            statSocketBytesRead = SocketBytesRead;
            statMessageReceived = false;

            if (headerReceived == false)
            {
                int bytesRequired = 0;
                int bytesPossible = 0;
                try
                {
                    bytesRequired = HEADERLENGTH - headerBufferPtr;
                    bytesPossible = bytesRequired;
                    if ((SocketBytesRead - SocketReceiveBufferPtr) < bytesRequired) bytesPossible = (SocketBytesRead - SocketReceiveBufferPtr);
                    Buffer.BlockCopy(SocketReceiveBuffer, SocketReceiveBufferPtr, headerBuffer, headerBufferPtr, bytesPossible);
                    headerBufferPtr += bytesPossible;
                    SocketReceiveBufferPtr += bytesPossible;

                    if (headerBufferPtr == HEADERLENGTH)
                    {
                        //  SETUP TO RECEIVE THE ENTIRE MESSAGE
                        headerReceived = true;

                        //  SETUP MESSAGE BODY BYTE ARRAY FOR THE EXPECTED BYTES
                        messageType = (MessageTypes)Convert.ToInt16(headerBuffer[0] | (headerBuffer[1] << 8));
                        messageIsCompressed = Convert.ToBoolean(headerBuffer[2]);
                        messageLength = headerBuffer[3] | (headerBuffer[4] << 8) | (headerBuffer[5] << 16) | (headerBuffer[6] << 24);
                        messageLengthUncompressed = headerBuffer[7] | (headerBuffer[8] << 8) | (headerBuffer[9] << 16) | (headerBuffer[10] << 24);

                        if (Enum.IsDefined(typeof(MessageTypes), messageType) == false)
                        {
                            throw new Exception("Invalid Message Type");
                        }

                        //  ONLY EXTEND THE MESSAGE BUFFER IF IT IS TOO SMALL FOR THE INCOMING MESSAGE.
                        //  THIS PROVIDES IMPROVED SPEED AND MUCH LESS WORK FOR THE GARBAGE COLLECTOR (BECAUSE WE ARE REUSING THE BUFFER)
                        if (messageLength > receiveBuffer.Length) receiveBuffer = new byte[messageLength];

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
                    string msg = "Error processing Message Body. messageLength: " + messageLength + "(" + messageLengthUncompressed + " uncompressed), ReceivedByesCount: " + SocketBytesRead + ", SocketReceiveBuffer.Length: " + SocketReceiveBuffer.Length;
                    msg += ", SocketReceiveBufferPtr: " + SocketReceiveBufferPtr;
                    msg += ", receiveBuffer.Length: " + receiveBuffer.Length + ", receiveBufferPtr: " + receiveBufferPtr;
                    msg += ", bytesRequired: " + bytesRequired + ", BytesPossible: " + bytesPossible;
                    msg += ", messageType: " + messageType.ToString() + ", messageIsCompressed: " + messageIsCompressed.ToString(CultureInfo.CurrentCulture);
                    msg += ", MessageCount: " + statMessageNumber;
                    msg += history.Text;
                    msg += Environment.NewLine + Environment.NewLine;
                    msg += ex.Message;
                    throw new Exception(msg);
                }
            }
            if (headerReceived == true && (SocketReceiveBufferPtr < SocketBytesRead || messageLength == 0))
            {
                int bytesRequired = 0;
                int bytesPossible = 0;
                try
                {
                    bytesRequired = messageLength - receiveBufferPtr;
                    bytesPossible = bytesRequired;
                    if ((SocketBytesRead - SocketReceiveBufferPtr) < bytesRequired) bytesPossible = (SocketBytesRead - SocketReceiveBufferPtr);
                    Buffer.BlockCopy(SocketReceiveBuffer, SocketReceiveBufferPtr, receiveBuffer, receiveBufferPtr, bytesPossible);
                    receiveBufferPtr += bytesPossible;
                    SocketReceiveBufferPtr += bytesPossible;

                    if (receiveBufferPtr == messageLength)
                    {
                        statMessageReceived = true;
                        Reset();
                        AddParseAttemptDetails();
                        statMessageNumber++;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Error processing Message Body. messageLength: " + messageLength + "(" + messageLengthUncompressed + " uncompressed), ReceivedByesCount: " + SocketBytesRead + ", SocketReceiveBuffer.Length: " + SocketReceiveBuffer.Length;
                    msg += ", SocketReceiveBufferPtr: " + SocketReceiveBufferPtr;
                    msg += ", receiveBuffer.Length: " + receiveBuffer.Length + ", receiveBufferPtr: " + receiveBufferPtr;
                    msg += ", bytesRequired: " + bytesRequired + ", BytesPossible: " + bytesPossible;
                    msg += ", messageType: " + messageType.ToString() + ", messageIsCompressed: " + messageIsCompressed.ToString(CultureInfo.CurrentCulture);
                    msg += ", MessageCount: " + statMessageNumber;
                    msg += history.Text;
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
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                //  WRITE HEADER
                writer.Write((short)SendObject.MessageType);

                if (Compress == false)
                {
                    //  WRITE HEADER
                    writer.Write(false);    //  NOT COMPRESSED
                    writer.Write((int)0);   //  WILL BE USED TO WRITE compressedLength
                    writer.Write((int)0);   //  WILL BE USED TO WRITE uncompressedLength

                    //  WRITE USER BYTES
                    long position1 = writer.BaseStream.Position;
                    SendObject.AppendBytes(writer);
                    int messageLength = Convert.ToInt32(writer.BaseStream.Position - position1);
                    int messageLengthUncompressed = messageLength;

                    //  GO BACK AND FINISH HEADER
                    writer.BaseStream.Position = 3;
                    writer.Write(messageLength);
                    writer.Write(messageLengthUncompressed);
                }
                else
                {
                    byte[] uncompressedBytes = null;
                    using (BinaryWriter writer2 = new BinaryWriter(new MemoryStream()))
                    {
                        SendObject.AppendBytes(writer2);

                        using (BinaryReader reader2 = new BinaryReader(writer2.BaseStream))
                        {
                            reader2.BaseStream.Position = 0;
                            uncompressedBytes = reader2.ReadBytes(Convert.ToInt32(reader2.BaseStream.Length));

                        }
                    }

                    if (uncompressedBytes != null && uncompressedBytes.Length > 1024)
                    {
                        writer.Write(true);     //  COMPRESSED
                        byte[] compressedBytes = CLZF2.Compress(uncompressedBytes);
                        writer.Write(compressedBytes.Length);
                        writer.Write(uncompressedBytes.Length);
                        writer.Write(compressedBytes);
                    }
                    else
                    {
                        writer.Write(false);     //  UNCOMPRESSED
                        writer.Write((int)0);   //  WILL BE USED TO WRITE compressedLength
                        writer.Write((int)0);   //  WILL BE USED TO WRITE uncompressedLength

                        //  WRITE USER BYTES
                        long position1 = writer.BaseStream.Position;
                        SendObject.AppendBytes(writer);
                        int messageLength2 = Convert.ToInt32(writer.BaseStream.Position - position1);
                        int messageLengthUncompressed2 = messageLength2;

                        //  GO BACK AND FINISH HEADER
                        writer.BaseStream.Position = 3;
                        writer.Write(messageLength2);
                        writer.Write(messageLengthUncompressed2);
                    }


                }
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    return reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));

                }
            }
        }


        /// <summary>
        /// The type of message. 
        /// </summary>
        internal MessageTypes MessageType
        {
            get { return messageType; }
        }


        private void AddParseAttemptDetails()
        {
            Parse p = new Parse(statMessageNumber, messageType.ToString(), messageLength, messageLengthUncompressed)
            {
                MessageReceived = statMessageReceived,
                SocketBytesRead = statSocketBytesRead,
                SocketBufferLength = statSocketBufferLength
            };
            history.Add(p);
        }


        internal ServerStoppingMessage GetDisconnectRequest()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new ServerStoppingMessage(reader);
                }
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
        }

        internal Message GetMessage()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new Message(reader);
                }
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
        }


        internal RequestMessage GetRequestMessage()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new RequestMessage(reader);
                }
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
        }

        internal ResponseMessage GetResponseMessage()
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetBuffer());
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream = null;
                    return new ResponseMessage(reader);
                }
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
        }

        private byte[] GetBuffer()
        {
            if (messageIsCompressed == false) return receiveBuffer;
            //  UNCOMPRESS
            if (uncompressedBuffer.Length < messageLengthUncompressed) uncompressedBuffer = new byte[messageLengthUncompressed];
            CLZF2.Decompress(receiveBuffer, ref uncompressedBuffer, messageLength);
            return uncompressedBuffer;
        }

        /// <summary>
        /// Resets the envelope for a fresh receive operation. Warning: should not be done in the middle of a receive operation.
        /// </summary>
        public void Reset()
        {
            headerBufferPtr = 0;
            receiveBufferPtr = 0;
            headerReceived = false;
        }


    }
}
