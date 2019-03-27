using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using SocketMeister.Messages;

namespace SocketMeister
{
    public partial class SocketServer
    {
        /// <summary>
        /// Remote client which has connected to the socket server
        /// </summary>
        public class Client
        {
            private readonly Guid _clientId = Guid.NewGuid();
            private Socket _clientSocket;
            private readonly DateTime _connectTimestamp = DateTime.Now;
            private readonly object _lock = new object();
            private MessageEngine _receivedEnvelope;
            private readonly SocketServer _socketServer;

            internal Client(SocketServer Server, Socket ClientSocket, bool EnableCompression)
            {
                _socketServer = Server;
                _clientSocket = ClientSocket;
                _receivedEnvelope = new MessageEngine(EnableCompression);
            }

            /// <summary>
            /// Unique GUID assigned to each client
            /// </summary>
            public Guid ClientId { get { return _clientId; } }

            /// <summary>
            /// Socket which the client is transmitting data on.
            /// </summary>
            internal Socket ClientSocket { get { return _clientSocket; } }

            /// <summary>
            /// Date and time which the client connected.
            /// </summary>
            public DateTime ConnectTimestamp { get { return _connectTimestamp; } }

            /// <summary>
            /// Byte array to directly receive data from the socket. 
            /// </summary>
            internal byte[] ReceiveBuffer = new byte[SocketClient.SEND_RECEIVE_BUFFER_SIZE];

            /// <summary>
            /// Class which processes raw data directly from the socket and converts into usable messages.
            /// </summary>
            internal MessageEngine ReceiveEnvelope { get { return _receivedEnvelope; } }

            /// <summary>
            /// Send a message to this client
            /// </summary>
            /// <param name="Parameters">Parameters to send to the client.</param>
            /// <param name="TimeoutMilliseconds">Number of milliseconds to attempt to send the message before throwing a TimeoutException.</param>
            public void SendMessage(object[] Parameters, int TimeoutMilliseconds = 60000)
            {
                Messages.Message message = new Messages.Message(Parameters, TimeoutMilliseconds);
                _socketServer.SendMessage(this, message, true);
            }
        }
    }
}
