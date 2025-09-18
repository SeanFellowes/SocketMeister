#if !NET35
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
        /// Represents a remote client that has connected to the socket server.
        /// </summary>
        public class Client : IDisposable
        {
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
            private volatile bool _handshake2Received;
            private volatile bool _isAddedToServerList;

            internal Client(SocketServer server, Socket clientSocket, bool compressSentData)
            {
                _socketServer = server;
                _clientId = Guid.NewGuid().ToString();
                _clientSocket = clientSocket;
                _compressSentData = compressSentData;
                _receiveEngine = new MessageEngine();
                _sendCallback = SendCallback; // Cache the delegate
            }

            /// <summary>
            /// Disposes of the resources used by the class.
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
                    // Dispose of managed resources
                    _unrespondedMessages.Clear(); // Explicitly clear any remaining references
                    _clientSocket.Dispose();
                    _subscriptions.Dispose();
                }

                _disposed = true;
            }

            /// <summary>
            /// Finalizer to ensure resources are released.
            /// </summary>
            ~Client()
            {
                Dispose(false);
            }

            /// <summary>
            /// Gets the GUID assigned to the client by the server when it connects.
            /// </summary>
            public string ClientId => _clientId;

            /// <summary>
            /// Gets the socket used by the client for data transmission.
            /// </summary>
            internal Socket ClientSocket => _clientSocket;

            /// <summary>
            /// Gets or sets the version of SocketMeister used by the client.
            /// </summary>
            public int ClientSocketMeisterVersion
            {
                get => _clientSocketMeisterVersion;
                set => _clientSocketMeisterVersion = value;
            }

            /// <summary>
            /// Gets the date and time when the client connected.
            /// </summary>
            public DateTime ConnectTimestamp => _connectTimestamp;

            /// <summary>
            /// Gets or sets a friendly name for the client. This can be used to help identify the client in logs and error handling.
            /// </summary>
            public string FriendlyName
            {
                get { lock (_friendlyNameLock) { return _friendlyName; } }
                set { lock (_friendlyNameLock) { _friendlyName = value; } }
            }

            /// <summary>
            /// Byte array used to directly receive data from the socket.
            /// </summary>
            internal byte[] ReceiveBuffer = new byte[Constants.SEND_RECEIVE_BUFFER_SIZE];

            /// <summary>
            /// Gets the class that processes raw data from the socket and converts it into usable messages.
            /// </summary>
            internal MessageEngine ReceiveEngine => _receiveEngine;

            /// <summary>
            /// Gets the number of subscriptions for this client.
            /// </summary>
            public int SubscriptionCount => _subscriptions.Count;

            /// <summary>
            /// Gets the collection of messages sent to the client that have not yet received a response.
            /// </summary>
            internal UnrespondedMessageCollection UnrespondedMessages => _unrespondedMessages;

            internal bool Handshake2Received
            {
                get => _handshake2Received;
                set => _handshake2Received = value;
            }

            internal bool IsAddedToServerList
            {
                get => _isAddedToServerList;
                set => _isAddedToServerList = value;
            }

            /// <summary>
            /// Checks whether a subscription exists.
            /// </summary>
            /// <param name="subscriptionName">The name of the subscription (case-insensitive).</param>
            /// <returns>True if the subscription exists; otherwise, false.</returns>
            public bool DoesSubscriptionExist(string subscriptionName)
            {
                return !string.IsNullOrEmpty(subscriptionName) && _subscriptions[subscriptionName] != null;
            }

            /// <summary>
            /// Gets a list of subscription names.
            /// </summary>
            /// <returns>A list of subscription names.</returns>
            public List<string> GetSubscriptions()
            {
                return _subscriptions.ToListOfNames();
            }

            /// <summary>
            /// Associates a received response with the original message. This method is called by the SocketServer class.
            /// </summary>
            /// <param name="responseMessage">The response message.</param>
            internal void SetMessageResponseInUnrespondedMessages(MessageResponseV1 responseMessage)
            {
                UnrespondedMessages.FindMessageAndSetResponse(responseMessage); // Locking is performed inside the class, so it is not required here.
            }

            internal TokenChangesResponseV1 ImportSubscriptionChanges(TokenChangesRequestV1 request)
            {
                return new TokenChangesResponseV1(_subscriptions.ImportTokenChanges(request.ChangeBytes));
            }

            /// <summary>
            /// Imports subscriptions into the client during the handshake process.
            /// </summary>
            /// <param name="subscriptionBytes">A byte array containing the token information.</param>
            internal void ImportSubscriptions(byte[] subscriptionBytes)
            {
                if (subscriptionBytes != null) _subscriptions.Initialize(subscriptionBytes);
            }

            internal void SendIMessage(IMessage message, bool async = true)
            {
                if (_clientSocket?.Connected != true || !_clientSocket.Poll(200000, SelectMode.SelectWrite))
                    return;

                try
                {
                    byte[] sendBytes = MessageEngine.GenerateSendBytes(message, _compressSentData);
                    _socketServer.IncrementSentTotals(sendBytes.Length);

                    if (async)
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
                    _socketServer.Logger.Log(new LogEntry(ex));
                }
                catch (Exception ex)
                {
                    _socketServer.ConnectedClients.Disconnect(this);
                    _socketServer.Logger.Log(new LogEntry(ex));
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
                    _socketServer.Logger.Log(new LogEntry(ex));
                }
            }

            /// <summary>
            /// Sends a message to the client and waits for a response.
            /// </summary>
            /// <param name="parameters">An array of parameters to send with the message.</param>
            /// <param name="timeoutMilliseconds">The maximum number of milliseconds to wait for a response from the server.</param>
            /// <param name="isLongPolling">Indicates whether the message is long-polling on the server. If true, the message will be canceled instantly when a disconnect occurs.</param>
            /// <returns>A nullable array of bytes returned from the socket server.</returns>
            public byte[] SendMessage(object[] parameters, int timeoutMilliseconds = 60000, bool isLongPolling = false)
            {
                if (parameters == null || parameters.Length == 0)
                    throw new ArgumentException("Message parameters cannot be null or empty.", nameof(parameters));

                // Wait for the server to start or timeout
                if (_socketServer.Status == SocketServerStatus.Starting)
                {
                    bool serverStarted = _socketServer.ServerStarted.Wait(timeoutMilliseconds);
                    if (!serverStarted)
                    {
                        throw new TimeoutException($"The server did not finish starting within the timeout of {timeoutMilliseconds} milliseconds. Please check the server logs for potential issues.");
                    }
                }

                // Validate server state again after waiting
                if (_socketServer.Status != SocketServerStatus.Started)
                    throw new InvalidOperationException("The socket server is not in the 'Started' state.");

                // Create and initialize the message
                var message = new MessageV1(parameters, timeoutMilliseconds, isLongPolling);

                try
                {
                    UnrespondedMessages.Add(message); // Locking is performed inside the class, so it is not required here.

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

                    throw new Exception("No response was received for the message.");
                }
                finally
                {
                    // Clean up the message and associated resources
                    UnrespondedMessages.Remove(message); // Locking is performed inside the class, so it is not required here.
                }
            }
        }
    }
}
#endif
