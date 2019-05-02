#if !SILVERLIGHT && !SMNOSERVER
using System;
using System.Net.Sockets;
using SocketMeister.Messages;

namespace SocketMeister
{
#if SMISPUBLIC
    public partial class SocketServer
#else
    internal partial class SocketServer
#endif
    {
        /// <summary>
        /// Remote client which has connected to the socket server
        /// </summary>
        public class Client
        {
            private readonly Guid clientId = Guid.NewGuid();
            private readonly Socket clientSocket;
            private readonly DateTime connectTimestamp = DateTime.Now;
            private readonly MessageEngine receivedEnvelope;
            private readonly SocketServer socketServer;

            internal Client(SocketServer Server, Socket ClientSocket)
            {
                socketServer = Server;
                clientSocket = ClientSocket;
                receivedEnvelope = new MessageEngine();
            }

            /// <summary>
            /// Unique GUID assigned to each client
            /// </summary>
            public Guid ClientId { get { return clientId; } }

            /// <summary>
            /// Socket which the client is transmitting data on.
            /// </summary>
            internal Socket ClientSocket { get { return clientSocket; } }

            /// <summary>
            /// Date and time which the client connected.
            /// </summary>
            public DateTime ConnectTimestamp { get { return connectTimestamp; } }

            /// <summary>
            /// Byte array to directly receive data from the socket. 
            /// </summary>
            internal byte[] ReceiveBuffer = new byte[SocketClient.SEND_RECEIVE_BUFFER_SIZE];

            /// <summary>
            /// Class which processes raw data directly from the socket and converts into usable messages.
            /// </summary>
            internal MessageEngine ReceiveEnvelope { get { return receivedEnvelope; } }

            /// <summary>
            /// Send a message to this client
            /// </summary>
            /// <param name="parameters">Parameters to send to the client.</param>
            /// <param name="timeoutMilliseconds">Number of milliseconds to attempt to send the message before throwing a TimeoutException.</param>
            public void SendMessage(object[] parameters, int timeoutMilliseconds = 60000)
            {
                Message message = new Message(parameters, timeoutMilliseconds);
                socketServer.SendMessage(this, message, true);
            }
        }
    }
}
#endif