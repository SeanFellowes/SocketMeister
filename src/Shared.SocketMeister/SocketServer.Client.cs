#if !SILVERLIGHT && !SMNOSERVER
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketMeister.Messages;

namespace SocketMeister
{
#if SMISPUBLIC
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public partial class SocketServer
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
#else
    internal partial class SocketServer
#endif
    {
        /// <summary>
        /// Remote client which has connected to the socket server
        /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible
        public class Client
#pragma warning restore CA1034 // Nested types should not be visible
        {
            private readonly Guid _clientId = Guid.NewGuid();
            private readonly Socket _clientSocket;
            private readonly DateTime _connectTimestamp = DateTime.Now;
            private readonly OpenRequestMessageCollection _openRequests = new OpenRequestMessageCollection();
            private readonly MessageEngine _receivedEnvelope;
            private readonly SocketServer _socketServer;
            private readonly TokenCollectionReadOnly _subscriptions = new TokenCollectionReadOnly();

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
            internal byte[] ReceiveBuffer = new byte[Constants.SEND_RECEIVE_BUFFER_SIZE];

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
                Message message = new Message(Parameters, TimeoutMilliseconds);
                _socketServer.SendMessage(this, message, true);
            }

            internal void ProcessResponseMessage(ResponseMessage Message)
            {
                RequestMessage request = _openRequests[Message.RequestId];
                if (request == null) return;
                request.Response = Message;
                //ThreadPool.QueueUserWorkItem(unused =>
                //{
                //    RequestMessage request = _openRequests[Message.RequestId];
                //    if (request == null) return;
                //    request.Response = Message;
                //});
            }

            internal TokenChangesResponseV1 ImportSubscriptionChanges(TokenChangesRequestV1 request)
            {
                return _subscriptions.ImportTokenChangesV1(request.ChangeBytes);
            }

            /// <summary>
            /// Send a request to the server and wait for a response. 
            /// </summary>
            /// <param name="Parameters">Array of parameters to send with the request</param>
            /// <param name="TimeoutMilliseconds">Maximum number of milliseconds to wait for a response from the server</param>
            /// <param name="IsLongPolling">If the request is long polling on the server mark this as true and the request will be cancelled instantly when a disconnect occurs</param>
            /// <returns>Nullable array of bytes which was returned from the socket server</returns>
            public byte[] SendRequest(object[] Parameters, int TimeoutMilliseconds = 60000, bool IsLongPolling = false)
            {
                if (Parameters == null) throw new ArgumentException("Request parameters cannot be null.", nameof(Parameters));
                if (Parameters.Length == 0) throw new ArgumentException("At least 1 request parameter is required.", nameof(Parameters));
                DateTime startTime = DateTime.Now;
                DateTime maxWait = startTime.AddMilliseconds(TimeoutMilliseconds);
                while (_socketServer.ListenerState == SocketServerStatus.Starting)
                {
                    Thread.Sleep(200);
                    if (DateTime.Now > maxWait) throw new TimeoutException();
                }
                if (_socketServer.ListenerState != SocketServerStatus.Started) throw new Exception("Request cannot be sent. The socket server is not running");
                int remainingMilliseconds = TimeoutMilliseconds - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
                return SendReceive(new RequestMessage(Parameters, remainingMilliseconds, IsLongPolling));
            }


            private byte[] SendReceive(RequestMessage Request)
            {
                if (_socketServer.ListenerState != SocketServerStatus.Started) return null;

                DateTime nowTs = DateTime.Now;
                _openRequests.Add(Request);

                byte[] sendBytes = MessageEngine.GenerateSendBytes(Request, false);

                while (Request.TrySendReceive == true)
                {
                    if (_socketServer.ListenerState != SocketServerStatus.Started) return null;

                    if (Request.Status == MessageStatus.Unsent)
                    {
                        _socketServer.SendMessage(this, Request, true);
                        Request.Status = MessageStatus.InProgress;

                        //  WAIT FOR RESPONSE
                        while (Request.WaitForResponse)
                        {
                            Thread.Sleep(5);
                        }
                    }

                    if (Request.TrySendReceive == true) Thread.Sleep(200);
                }

                _openRequests.Remove(Request);

                if (Request.Response != null)
                {
                    if (Request.Response.Error != null) throw new Exception(Request.Response.Error);
                    else return Request.Response.ResponseData;
                }
                else throw new TimeoutException("SendReceive() timed out after " + Request.TimeoutMilliseconds + " milliseconds");
            }





        }
    }
}
#endif