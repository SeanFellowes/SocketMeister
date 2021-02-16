using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketMeister.Messages
{
    /// <summary>
    /// SocketServer sends subscription related messages to clients.
    /// </summary>
    internal partial class SubscriptionMessageV1 : MessageBase, IMessage
    {
        //  REQUEST VARIABLES
        private readonly byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;
        private readonly string _subscriptionName;

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="SubscriptionName">Name of the subscription that message applies to.</param>
        /// <param name="Parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        public SubscriptionMessageV1(string SubscriptionName, object[] Parameters) : base(MessageTypes.SubscriptionMessageV1)
        {
            _parameters = Parameters;
            _subscriptionName = SubscriptionName;

            //  SERIALIZE REQUEST MESSAGE
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(_subscriptionName);
                Serializer.SerializeParameters(writer, Parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    _parameterBytes = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }


        internal SubscriptionMessageV1(BinaryReader bR) : base(MessageTypes.SubscriptionMessageV1)
        {
            _subscriptionName = bR.ReadString();
            _parameters = Serializer.DeserializeParameters(bR);
        }


        /// <summary>
        /// Parameters provided with this request
        /// </summary>
        public object[] Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Name of the subscription that this message applies to.
        /// </summary>
        public string SubscriptionName
        {
            get { return _subscriptionName; }
        }

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }
    }
}
