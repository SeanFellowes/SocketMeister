#if !SMNOSERVER && !NET35
using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

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
        public class Client : ClientBase, IDisposable
        {
            private readonly Socket _clientSocket;
            private readonly bool _compressSentData;
            private readonly DateTime _connectTimestamp = DateTime.UtcNow;
            private bool _disposed;
            private readonly MessageEngine _receiveEngine;
            private readonly AsyncCallback _sendCallback;
            private readonly SocketServer _socketServer;
            private readonly TokenCollectionReadOnly _subscriptions = new TokenCollectionReadOnly();

            internal Client(SocketServer Server, Socket ClientSocket, bool CompressSentData) : base(isServerImplimentation: true)
            {
                _socketServer = Server;
                _clientSocket = ClientSocket;
                _compressSentData = CompressSentData;
                _receiveEngine = new MessageEngine(CompressSentData);
                _sendCallback = SendCallback; // Cache the delegate
            }

            /// <summary>
            /// Dispose of the class
            /// </summary>
            public new void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private new void Dispose(bool disposing)
            {
                if (_disposed) return;

                if (disposing)
                {
                    // Dispose managed resources
                    _clientSocket.Dispose();
                    _subscriptions.Dispose();
                }

                // Unmanaged resources would be cleaned here (none in this case)
                base.Dispose(disposing);
                _disposed = true;
            }

            /// <summary>
            /// Finalizer
            /// </summary>
            ~Client()
            {
                Dispose(false);
            }

            /// <summary>
            /// Socket which the client is transmitting data on.
            /// </summary>
            internal Socket ClientSocket => _clientSocket;

            /// <summary>
            /// Date and time which the client connected.
            /// </summary>
            public DateTime ConnectTimestamp => _connectTimestamp;

            /// <summary>
            /// Byte array to directly receive data from the socket. 
            /// </summary>
            internal byte[] ReceiveBuffer = new byte[Constants.SEND_RECEIVE_BUFFER_SIZE];

            /// <summary>
            /// Class which processes raw data directly from the socket and converts into usable messages.
            /// </summary>
            internal MessageEngine ReceiveEngine => _receiveEngine;

            /// <summary>
            /// The number of subscriptions for this client
            /// </summary>
            public int SubscriptionCount => _subscriptions.Count;

            /// <summary>
            /// Whether a subscription exists. 
            /// </summary>
            /// <param name="SubscriptionName">Name of the subscription (Case insensitive).</param>
            /// <returns>True if exists, false if the subscription does not exist</returns>
            public bool DoesSubscriptionExist(string SubscriptionName)
            {
                return !string.IsNullOrEmpty(SubscriptionName) && _subscriptions[SubscriptionName] != null;
            }

            /// <summary>
            /// Get a list of subscription names
            /// </summary>
            /// <returns>List of subscription names</returns>
            public List<string> GetSubscriptions()
            {
               return _subscriptions.ToListOfNames();
            }

            /// <summary>
            /// When a response is received for a sent message the received respons needs to attached to the original message. SocketServer class calls this
            /// </summary>
            /// <param name="ResponseMessage"></param>
            internal void SetMessageResponseInUnrespondedMessages(MessageResponseV1 ResponseMessage)
            {
                UnrespondedMessages.FindMessageAndSetResponse(ResponseMessage); //  Locking performed inside the class so not required here
            }

            internal TokenChangesResponseV1 ImportSubscriptionChanges(TokenChangesRequestV1 request)
            {
                return _subscriptions.ImportTokenChangesV1(request.ChangeBytes);
            }

            internal void SendIMessage(IMessage Message, bool Async = true)
            {
                if (_clientSocket?.Connected != true || !_clientSocket.Poll(200000, SelectMode.SelectWrite))
                    return;

                try
                {
                    byte[] sendBytes = MessageEngine.GenerateSendBytes(Message, _compressSentData);
                    _socketServer.IncrementSentTotals(sendBytes.Length);

                    if (Async == true)
                    {
                        ClientSocket.BeginSend(sendBytes, 0, sendBytes.Length, 0, _sendCallback, this);
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
            /// Send a message to the client and wait for a response. 
            /// </summary>
            /// <param name="Parameters">Array of parameters to send with the message</param>
            /// <param name="TimeoutMilliseconds">Maximum number of milliseconds to wait for a response from the server</param>
            /// <param name="IsLongPolling">If the message is long polling on the server mark this as true and the message will be cancelled instantly when a disconnect occurs</param>
            /// <returns>Nullable array of bytes which was returned from the socket server</returns>
            public byte[] SendMessage(object[] Parameters, int TimeoutMilliseconds = 60000, bool IsLongPolling = false)
            {
                if (Parameters == null || Parameters.Length == 0)
                    throw new ArgumentException("Message parameters cannot be null or empty.", nameof(Parameters));

                // Wait for the server to start or timeout
                if (_socketServer.Status == SocketServerStatus.Starting)
                {
                    bool serverStarted = _socketServer.ServerStarted.Wait(TimeoutMilliseconds);
                    if (!serverStarted)
                    {
                        throw new TimeoutException($"The server did not finish starting within the timeout of {TimeoutMilliseconds} milliseconds. Please check the server logs for potential issues.");
                    }
                }

                // Validate server state again after waiting
                if (_socketServer.Status != SocketServerStatus.Started)
                    throw new InvalidOperationException("The socket server is not in the 'Started' state.");

                // Create and initialize the message
                var message = new MessageV1(Parameters, TimeoutMilliseconds, IsLongPolling);

                try
                {
                    UnrespondedMessages.Add(message);   //  Locking performed inside the class so not required here

                    // Generate bytes and prepare to send
                    byte[] sendBytes = MessageEngine.GenerateSendBytes(message, false);

                    DateTime startTime = DateTime.UtcNow;
                    if (_socketServer.Status != SocketServerStatus.Started)
                        return null;

                    SendIMessage(message, true); // Attempt to send the message
                    message.SetStatusInProgress();

                    // Wait for a response. 
                    message.WaitForResponseOrTimeout();

                    if (message.Response != null)
                    {
                        if (message.Response.Error != null)
                            throw message.Response.Error;

                        return message.Response.ResponseData;
                    }

                    if (message.Error != null)
                        throw message.Error;

                    throw new Exception("There was no message response");
                }
                finally
                {
                    // Clean up message and associated resources
                    UnrespondedMessages.Remove(message);   //  Locking performed inside the class so not required here
                }
            }
        }
    }
}
#endif
