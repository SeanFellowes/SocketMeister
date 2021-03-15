#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
#pragma warning disable CA1034 // Nested types should not be visible

#if !SILVERLIGHT && !SMNOSERVER && !NET35 && !NET20
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            private readonly Guid _clientId = Guid.NewGuid();
            private readonly Socket _clientSocket;
            private readonly bool _compressSentData;
            private readonly DateTime _connectTimestamp = DateTime.Now;
            private readonly OpenRequestMessageCollection _openRequests = new OpenRequestMessageCollection();
            private readonly MessageEngine _receivedEnvelope;
            private readonly SocketServer _socketServer;
            private readonly TokenCollectionReadOnly _subscriptions = new TokenCollectionReadOnly();

            internal Client(SocketServer Server, Socket ClientSocket, bool CompressSentData)
            {
                _socketServer = Server;
                _clientSocket = ClientSocket;
                _compressSentData = CompressSentData;
                _receivedEnvelope = new MessageEngine(CompressSentData);
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
            /// The number of subscriptions for this client
            /// </summary>
            public int SubscriptionCount { get { return _subscriptions.Count; } }

            /// <summary>
            /// Whether a subscription exists. 
            /// </summary>
            /// <param name="SubscriptionName">Name of the subscription (Case insensitive).</param>
            /// <returns>True if exists, false if the subscription does not exist</returns>
            public bool DoesSubscriptionExist(string SubscriptionName)
            {
                if (string.IsNullOrEmpty(SubscriptionName) == true) return false;
                else if (_subscriptions[SubscriptionName] != null) return true;
                else return false;
            }

            /// <summary>
            /// Get a list of subscription names
            /// </summary>
            /// <returns>List of subscription names</returns>
            public List<string> GetSubscriptions()
            {
                List<string> rVal = new List<string>();
                List<Token> tokens = _subscriptions.ToList();
                foreach (Token t in tokens)
                {
                    rVal.Add(t.Name);
                }
                return rVal;
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
            /// Send a message to this client
            /// </summary>
            /// <param name="Parameters">Parameters to send to the client.</param>
            /// <param name="TimeoutMilliseconds">Number of milliseconds to attempt to send the message before throwing a TimeoutException.</param>
            public void SendMessage(object[] Parameters, int TimeoutMilliseconds = 60000)
            {
                Message message = new Message(Parameters, TimeoutMilliseconds);
                SendIMessage(message, true);
            }

            internal void SendIMessage(IMessage Message, bool Async = true)
            {
                if (ClientSocket == null ||
                    ClientSocket.Connected == false || ClientSocket.Poll(200000, SelectMode.SelectWrite) == false) return;

                try
                {
                    byte[] sendBytes = MessageEngine.GenerateSendBytes(Message, _compressSentData);
                    _socketServer.IncrementSentTotals(sendBytes.Length);

                    if (Async == true)
                    {
                        ClientSocket.BeginSend(sendBytes, 0, sendBytes.Length, 0, new AsyncCallback(SendCallback), this);
                    }
                    else
                    {
                        ClientSocket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    _socketServer.ConnectedClients.Disconnect(this);
                    _socketServer.NotifyTraceEventRaised(ex, 5008);
                }
                catch (Exception ex)
                {
                    _socketServer.ConnectedClients.Disconnect(this);
                    _socketServer.NotifyTraceEventRaised(ex, 5008);
                }
            }

            private void SendCallback(IAsyncResult ar)
            {
                try
                {
                    // Retrieve the socket from the state object.  
                    Client remoteClient = (Client)ar.AsyncState;

                    // Complete sending the data to the remote device.  
                    int bytesSent = remoteClient.ClientSocket.EndSend(ar);
                }
                catch (Exception ex)
                {
                    _socketServer.ConnectedClients.Disconnect(this);
                    _socketServer.NotifyTraceEventRaised(ex, 5008);
                }
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
                while (_socketServer.Status == SocketServerStatus.Starting)
                {
                    Thread.Sleep(200);
                    if (DateTime.Now > maxWait) throw new TimeoutException();
                }
                if (_socketServer.Status != SocketServerStatus.Started) throw new Exception("Request cannot be sent. The socket server is not running");
                int remainingMilliseconds = TimeoutMilliseconds - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
                return SendReceive(new RequestMessage(Parameters, remainingMilliseconds, IsLongPolling));
            }


            private byte[] SendReceive(RequestMessage Request)
            {
                if (_socketServer.Status != SocketServerStatus.Started) return null;

                DateTime nowTs = DateTime.Now;
                _openRequests.Add(Request);

                byte[] sendBytes = MessageEngine.GenerateSendBytes(Request, false);

                while (Request.TrySendReceive == true)
                {
                    if (_socketServer.Status != SocketServerStatus.Started) return null;

                    if (Request.Status == MessageStatus.Unsent)
                    {
                        SendIMessage(Request, true);
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

#pragma warning restore CA1034 // Nested types should not be visible
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression
