#if !SMNOSERVER && !NET35
using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

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
        public class Client : IDisposable
        {
            /// <summary>
            /// Internal Message: Test case of sending a message type to the client which doesn't exist on the client.
            /// </summary>
            //internal class ServerOnlyTestMessage : MessageBase, IMessage
            //{
            //    public ServerOnlyTestMessage() : base(MessageType.PollingRequestV1, messageId: 0) { }

            //    public void AppendBytes(BinaryWriter Writer)
            //    {
            //    }
            //}

            private readonly Socket _clientSocket;
            private readonly string _clientId;
            private int _clientSocketMeisterVersion;
            private readonly bool _compressSentData;
            private readonly DateTime _connectTimestamp = DateTime.UtcNow;
            private bool _disposed;
            private string _friendlyName = string.Empty;
            private readonly object _friendlyNameLock = new object();
            private readonly MessageEngine _receiveEngine;
            private readonly AsyncCallback _sendCallback;
            private readonly SocketServer _socketServer;
            private readonly TokenCollectionReadOnly _subscriptions = new TokenCollectionReadOnly();
            private readonly UnrespondedMessageCollection _unrespondedMessages = new UnrespondedMessageCollection();

            internal Client(SocketServer Server, Socket ClientSocket, bool CompressSentData)
            {
                _socketServer = Server;
                _clientId = Guid.NewGuid().ToString();
                _clientSocket = ClientSocket;
                _compressSentData = CompressSentData;
                _receiveEngine = new MessageEngine(CompressSentData);
                _sendCallback = SendCallback; // Cache the delegate
            }

            /// <summary>
            /// Dispose of the class
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (_disposed) return;

                if (disposing)
                {
                    // Dispose managed resources
                    _unrespondedMessages.Clear(); // Explicitly clear any remaining references
                    _clientSocket.Dispose();
                    _subscriptions.Dispose();
                }

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
            /// GUID assigned to the client by the server, when it connects.
            /// </summary>
            public string ClientId => _clientId;


            /// <summary>
            /// Socket which the client is transmitting data on.
            /// </summary>
            internal Socket ClientSocket => _clientSocket;

            /// <summary>
            /// The version of SocketMeister used by the client.
            /// </summary>
            public int ClientSocketMeisterVersion
            {
                get { return _clientSocketMeisterVersion; }
                set { _clientSocketMeisterVersion = value; }
            }

            /// <summary>
            /// Date and time which the client connected.
            /// </summary>
            public DateTime ConnectTimestamp => _connectTimestamp;

            /// <summary>
            /// A friendly name for the client. You application can set this to help identify the client in logs and error handling.
            /// </summary>
            public string FriendlyName
            {
                get { lock (_friendlyNameLock) { return _friendlyName; } }
                set { lock (_friendlyNameLock) { _friendlyName = value; } }
            }


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
            /// Messages sent to the client in which there has been no response
            /// </summary>
            internal UnrespondedMessageCollection UnrespondedMessages
            {
                get { return _unrespondedMessages; }
            }


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
                return new TokenChangesResponseV1(_subscriptions.ImportTokenChanges(request.ChangeBytes));
            }

            /// <summary>
            /// During handshake the server sends the client its subscriptions. This method imports the subscriptions into the client
            /// </summary>
            /// <param name="subscriptionBytes">byte array containing the token information</param>
            internal void ImportSubscriptions(byte[] subscriptionBytes)
            {
                if (subscriptionBytes != null) _subscriptions.Initialize(subscriptionBytes);
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
                    _socketServer.NotifyLogRaised(ex, 5008);
                }
                catch (Exception ex)
                {
                    _socketServer.ConnectedClients.Disconnect(this);
                    _socketServer.NotifyLogRaised(ex, 5008);
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
                    _socketServer.NotifyLogRaised(ex, 5008);
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
