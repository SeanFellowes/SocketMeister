#if !SMNOSERVER && !NET35
using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace SocketMeister
{
    /// <summary>
    /// TCP/IP socket server that listens for client connections and raises events when messages are received.
    /// </summary>
#if SMISPUBLIC
    public partial class SocketServer : IDisposable
#else
        internal partial class SocketServer
#endif
    {
        private ManualResetEventSlim AllDone { get; set; } = new ManualResetEventSlim(false);
        private ManualResetEventSlim ServerStarted { get; set; } = new ManualResetEventSlim(false);

        private readonly Clients _connectedClients;
        private readonly bool _compressSentData;
        private bool _disposed;
        private readonly string _endPoint;
        private readonly Socket _listener = null;
        private SocketServerStatus _status;
        private readonly IPEndPoint _localEndPoint = null;
        private readonly object _lock = new object();
        private readonly object _lockTotals = new object();
        private bool _stopSocketServer;
        private readonly Thread _threadListener = null;
        private long _totalBytesReceived;
        private long _totalBytesSent;
        private int _totalMessagesSent;
        private int _totalMessagesReceived;

        /// <summary>
        /// Event raised when a client connects to the socket server. Raised on a separate thread.
        /// </summary>
        /// <seealso cref="ClientEventArgs"/>
        public event EventHandler<ClientEventArgs> ClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the socket server. Raised on a separate thread.
        /// </summary>
        /// <seealso cref="ClientEventArgs"/>
        public event EventHandler<ClientEventArgs> ClientDisconnected;

        /// <summary>
        /// Event raised when a log event occurs.
        /// </summary>
        /// <seealso cref="LogEventArgs"/>
        public event EventHandler<LogEventArgs> LogRaised;

        /// <summary>
        /// Event raised when a message is received from a client. An optional response can be provided, which will be returned to the client. Raised on a separate thread.
        /// </summary>
        /// <seealso cref="MessageReceivedEventArgs"/>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Event raised when the status of the socket listener changes. Raised on a separate thread.
        /// </summary>
        public event EventHandler<EventArgs> StatusChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Port">Port that this socket server will listen on.</param>
        /// <param name="CompressSentData">Enable compression on message data.</param>
        /// <seealso cref="Start"/>
        /// <seealso cref="Stop"/>
        public SocketServer(int Port, bool CompressSentData)
        {
            _compressSentData = CompressSentData;
            _status = SocketServerStatus.Starting;
            _connectedClients = new Clients(Logger);

            // Logger
            Logger.LogRaised += Logger_LogRaised;

            try
            {
                // Connect to all interfaces (IP 0.0.0.0 means all interfaces).
                IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
                _localEndPoint = new IPEndPoint(ipAddress, Port);

                // Local IP address and port (used for diagnostic messages).
                _endPoint = GetLocalIPAddress().ToString() + ":" + Port.ToString(CultureInfo.InvariantCulture);

                // Create a TCP/IP socket.
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listener.Bind(_localEndPoint);

                // WARNING – DO NOT USE SocketOptionName.ReceiveTimeout OR SocketOptionName.SendTimeout. TRIED THIS AND IT COMPLETELY BROKE BIG DATA TRANSFERS.
                _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, 0);

                // Register for events.
                _connectedClients.ClientDisconnected += ConnectedClients_ClientDisconnected;
                _connectedClients.ClientConnected += ConnectedClients_ClientConnected;

                // Setup background process for listening.
                _threadListener = new Thread(new ThreadStart(BgListen))
                {
                    IsBackground = true
                };
            }
            catch
            {
                _status = SocketServerStatus.Stopped;
                throw;
            }
        }

        /// <summary>
        /// Disposes of the class.
        /// </summary>
        /// <seealso cref="Dispose(bool)"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the class.
        /// </summary>
        /// <param name="disposing">Whether this is called from Dispose (true) or finalizer (false).</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return; // Prevent double dispose
            _disposed = true;

            if (disposing)
            {
                Stop(); // Safely stop the server.
                _listener?.Dispose(); // Clean up socket.
                AllDone?.Dispose();
                ServerStarted?.Dispose();
            }
        }

        /// <summary>
        /// Disposes of the class.
        /// </summary>
        ~SocketServer()
        {
            Dispose(false);
        }

        internal Clients ConnectedClients => _connectedClients;

        /// <summary>
        /// The IP address and port that this socket server is using to communicate (e.g. 10.200.50.25:6000).
        /// </summary>
        public string EndPoint => _endPoint;

        /// <summary>
        /// Central logging for the socket server. This is a simple logger that logs messages to the console and raises log events to calling code.
        /// </summary>
        /// <seealso cref="Logger"/>
        internal Logger Logger { get; } = new Logger();

        /// <summary>
        /// Current status of the SocketServer.
        /// </summary>
        /// <seealso cref="SocketServerStatus"/>
        public SocketServerStatus Status
        {
            get { lock (_lock) { return _status; } }
            private set
            {
                lock (_lock)
                {
                    if (_status == value) return;
                    _status = value;
                }
                StatusChanged?.Invoke(null, new EventArgs());
            }
        }

        /// <summary>
        /// The total number of bytes that have been received through the socket server since it started.
        /// </summary>
        public long TotalBytesReceived
        {
            get { lock (_lockTotals) { return _totalBytesReceived; } }
        }

        /// <summary>
        /// The total number of bytes that have been sent through the socket server since it started.
        /// </summary>
        public long TotalBytesSent => Interlocked.Read(ref _totalBytesSent);

        /// <summary>
        /// The total number of messages that have been sent through the socket server since it started.
        /// </summary>
        public int TotalMessagesSent => Interlocked.CompareExchange(ref _totalMessagesSent, 0, 0);

        /// <summary>
        /// The total number of messages that have been received through the socket server since it started.
        /// </summary>
        public int TotalMessagesReceived
        {
            get { lock (_lockTotals) { return _totalMessagesReceived; } }
        }

        /// <summary>
        /// Indicates whether the socket service is in the process of stopping.
        /// </summary>
        private bool StopSocketServer { get { lock (_lock) { return _stopSocketServer; } } set { lock (_lock) { _stopSocketServer = value; } } }

        #region Public Methods

        /// <summary>
        /// Sends a message to all connected clients. Exceptions will not halt this process but will generate 'ExceptionRaised' events.
        /// </summary>
        /// <param name="Name">Optional name/tag/identifier for the broadcast.</param>
        /// <param name="Parameters">Parameters to send with the message.</param>
        /// <seealso cref="BroadcastToSubscribers(string, object[])"/>
        public void Broadcast(string Name, object[] Parameters)
        {
            BroadcastV1 message = new BroadcastV1(Name, Parameters);
            List<Client> clients = _connectedClients.ToList();
            foreach (Client client in clients)
            {
                try
                {
                    client.SendIMessage(message, true);
                }
                catch (Exception ex)
                {
                    Logger.Log(new LogEntry(ex));
                }
            }
        }

        /// <summary>
        /// Sends a message to all clients subscribing to a subscription name. Exceptions will not halt this process but will generate 'ExceptionRaised' events.
        /// </summary>
        /// <param name="Name">Optional name/tag/identifier for the broadcast.</param>
        /// <param name="Parameters">Parameters to send with the message.</param>
        /// <seealso cref="Broadcast(string, object[])"/>
        public void BroadcastToSubscribers(string Name, object[] Parameters)
        {
            if (string.IsNullOrEmpty(Name) == true) throw new ArgumentNullException(nameof(Name));

            BroadcastV1 message = null;
            List<Client> clients = _connectedClients.GetClientsWithSubscriptions(Name);
            foreach (Client client in clients)
            {
                message = new BroadcastV1(Name, Parameters);
                try
                {
                    client.SendIMessage(message, true);
                }
                catch (Exception ex)
                {
                    Logger.Log(new LogEntry(ex));
                }
            }
        }

        /// <summary>
        /// Gets the number of clients connected to the socket server.
        /// </summary>
        /// <seealso cref="GetClients"/>
        public int ClientCount => _connectedClients.Count;

        /// <summary>
        /// Determines whether there are any clients subscribing to a subscription name.
        /// </summary>
        /// <param name="SubscriptionName">Name of the subscription (case-insensitive).</param>
        /// <returns>True if at least one client is subscribed to the given name; otherwise, false.</returns>
        public bool DoSubscribersExist(string SubscriptionName)
        {
            if (string.IsNullOrEmpty(SubscriptionName) == true) throw new ArgumentNullException(nameof(SubscriptionName));
            List<Client> clients = _connectedClients.ToList();
            foreach (Client client in clients)
            {
                if (client.DoesSubscriptionExist(SubscriptionName) == true) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of clients that are connected to the socket server.
        /// </summary>
        /// <returns>List of clients.</returns>
        /// <seealso cref="Client"/>
        public List<Client> GetClients()
        {
            return _connectedClients.ToList();
        }

        /// <summary>
        /// Starts the socket server, which begins listening for incoming connections.
        /// </summary>
        /// <seealso cref="Stop"/>
        public void Start()
        {
            StopSocketServer = false;
            lock (_lockTotals)
            {
                _totalBytesReceived = 0;
                _totalBytesSent = 0;
                _totalMessagesSent = 0;
                _totalMessagesReceived = 0;
            }
            _threadListener.Start();
        }

        /// <summary>
        /// Sends a message to all clients to disconnect, waits for in-progress messages to finish, and then stops the socket server.
        /// </summary>
        /// <seealso cref="Start"/>
        public void Stop()
        {
            try
            {
                if (Status != SocketServerStatus.Started)
                    throw new InvalidOperationException("Socket server is not currently running.");

                Status = SocketServerStatus.Stopping;
                AllDone.Set();

                List<Client> toProcess = _connectedClients.ToList();

                // Send ServerStoppingNotification concurrently.
                Parallel.ForEach(toProcess, remoteClient =>
                {
                    try
                    {
                        SendServerStoppingMessage(remoteClient);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(new LogEntry(ex));
                    }
                });

                // Wait for clients to disconnect with a timeout.
                DateTime maxWait = DateTime.UtcNow.AddMilliseconds(Constants.MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING);
                while (_connectedClients.Count > 0 && DateTime.UtcNow < maxWait)
                {
                    Thread.Sleep(50); // Reduced sleep interval for responsiveness.
                }

                _connectedClients.DisconnectAll(); // Force disconnect remaining clients.
                _listener?.Close();
            }
            catch (Exception ex)
            {
                Logger.Log(new LogEntry(ex));
            }
            finally
            {
                Status = SocketServerStatus.Stopped;
                StopSocketServer = true;
                Logger.Stop();
                _threadListener.Join(1000); // Wait for the listener thread to finish.
            }
        }

        #endregion

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            AllDone.Set();

            if (Status == SocketServerStatus.Stopped)
            {
                return;
            }
            else if (Status == SocketServerStatus.Stopping)
            {
                // Accept the connection but disconnect the client.
                Task.Run(() =>
                {
                    Socket listener = (Socket)ar.AsyncState;
                    Socket handler = null;
                    try { handler = listener.EndAccept(ar); }
                    catch { return; }

                    // Shutdown the client's socket.
                    try { handler.Shutdown(SocketShutdown.Both); }
                    catch { }
                    try { handler.Close(); }
                    catch { }
                    return;
                });
            }
            else
            {
                // Receive data on a dedicated background task.
                Task.Run(() =>
                {
                    // Get the socket that handles the client.
                    Socket listener = (Socket)ar.AsyncState;
                    Socket handler = null;
                    try { handler = listener.EndAccept(ar); }
                    catch { return; }
                    handler.SendTimeout = 30000;

                    // Create the state object.
                    Client remoteClient = new Client(this, handler, _compressSentData);
                    _connectedClients.Add(remoteClient);
                    handler.BeginReceive(remoteClient.ReceiveBuffer, 0, Constants.SEND_RECEIVE_BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), remoteClient);

                    // Send SocketServerHandshake1 to the client to indicate that the server is ready to receive data.
                    try
                    {
                        // Pause so the client can establish its receive buffer and so the server can complete setting up the buffer.
                        // *** Look for a better solution, e.g. retry on Handshake1 (will require a Handshake1Ack) ***
                        Thread.Sleep(1000);

                        // Send Handshake 1.
                        byte[] sendBytes = MessageEngine.GenerateSendBytes(new Handshake1(Constants.SOCKET_MEISTER_VERSION, remoteClient.ClientId.ToString()), _compressSentData);
                        remoteClient.ClientSocket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
                    }
                    catch (ObjectDisposedException)
                    {
                        _connectedClients.Disconnect(remoteClient);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(new LogEntry(ex));
                    }
                });
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            Client remoteClient = null;

            try
            {
                // Retrieve the state object and the handler socket from the asynchronous state object.
                remoteClient = (Client)ar.AsyncState;
                MessageEngine receiveEnvelope = remoteClient.ReceiveEngine;

                // Read data from the client socket.
                if (remoteClient.ClientSocket.Connected == false) return;
                int receivedBytesCount = remoteClient.ClientSocket.EndReceive(ar);
                int receiveBufferPtr = 0;

                // If we received 0 bytes, the client is shutting down.
                if (receivedBytesCount == 0)
                {
                    // A graceful close should occur as follows:
                    // 1. The client socket calls Shutdown(SocketShutdown.Send) but continues receiving.
                    // 2. On the server, EndReceive returns 0 bytes read (the client signals there is no more data).
                    // 3. The server sends its last data, calls Shutdown(SocketShutdown.Send), and then calls Close on the socket (optionally with a timeout to allow the data to be read).
                    // 4. The client reads the remaining data, then receives 0 bytes (the server signals there is no more data) and calls Close on the socket.
                    _connectedClients.Remove(remoteClient);
                    remoteClient.ClientSocket.Shutdown(SocketShutdown.Send);
                    remoteClient.ClientSocket.Close(15);
                    return;
                }

                while (receiveBufferPtr < receivedBytesCount)
                {
                    if (receiveEnvelope.AddBytesFromSocketReceiveBuffer(receivedBytesCount, remoteClient.ReceiveBuffer, ref receiveBufferPtr) == true)
                    {
                        lock (_lockTotals)
                        {
                            if (_totalBytesReceived > (long.MaxValue * 0.9)) _totalBytesReceived = 0;
                            _totalBytesReceived += receiveEnvelope.MessageLength;
                        }

                        if (receiveEnvelope.MessageType == MessageType.MessageV1)
                        {
                            lock (_lockTotals)
                            {
                                if (_totalMessagesReceived == int.MaxValue) _totalMessagesReceived = 0;
                                _totalMessagesReceived++;
                            }

                            MessageV1 message = receiveEnvelope.GetMessageV1();
                            message.RemoteClient = remoteClient;

                            if (Status == SocketServerStatus.Stopping)
                            {
                                MessageResponseV1 response = new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.Stopping);
                                message.RemoteClient.SendIMessage(response, false);
                            }
                            else
                            {
                                // Offload CPU-bound work to a thread pool thread.
                                Task.Run(() => BgProcessMessage(message));
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageType.MessageResponseV1)
                        {
                            if (Status == SocketServerStatus.Started)
                            {
                                // Process ResponseMessage. NOTE: Method is executed on a thread pool thread.
                                remoteClient.SetMessageResponseInUnrespondedMessages(receiveEnvelope.GetMessageResponseV1());
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageType.ClientDisconnectingNotificationV1)
                        {
                            try
                            {
                                _connectedClients.Disconnect(remoteClient);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(new LogEntry(ex));
                            }
                            ClientDisconnectingNotificationV1 msg = receiveEnvelope.GetClientDisconnectingNotificationV1();
                            Logger.Log(new LogEntry(msg.ClientMessage, Severity.Information, LogEventType.ConnectionEvent));
                        }
                        else if (receiveEnvelope.MessageType == MessageType.PollingRequestV1)
                        {
                            if (Status == SocketServerStatus.Started)
                            {
                                Task.Run(() => BgProcessPollRequest(remoteClient));
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageType.Handshake2)
                        {
                            Handshake2 msg = receiveEnvelope.GetHandshake2();
                            if (Status == SocketServerStatus.Started)
                            {
                                Task.Run(() => BgProcessHandshake2(remoteClient, msg));
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageType.TokenChangesRequestV1)
                        {
                            if (Status == SocketServerStatus.Started)
                            {
                                TokenChangesRequestV1 request = receiveEnvelope.GetSubscriptionChangesNotificationV1();
                                Task.Run(() => BgProcessSubscriptionChanges(remoteClient, request));
                            }
                        }
                        else
                        {
                            string msg = "ReadCallback() ";
                            if (!Enum.IsDefined(typeof(MessageType), receiveEnvelope.MessageType))
                            {
                                msg += "encountered an undefined MessageType, " + (int)receiveEnvelope.MessageType;
                            }
                            else
                            {
                                msg += "has no message handling code for the MessageType, " + receiveEnvelope.MessageType.ToString();
                            }

                            // Unknown message received. The client may be a newer version of SocketMeister. Raise a log message and ignore.
                            string logMsg = $"ReadCallback() encountered an unknown message type ({receiveEnvelope.MessageType.ToString()}). The server may be running a newer version of SocketMeister.";
                            Logger.Log(new LogEntry(logMsg, Severity.Warning, LogEventType.Exception));
                        }
                    }
                }
                if (remoteClient != null && remoteClient.ClientSocket != null && remoteClient.ClientSocket.Connected == true)
                {
                    remoteClient.ClientSocket.BeginReceive(remoteClient.ReceiveBuffer, 0, Constants.SEND_RECEIVE_BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), remoteClient);
                }
            }
            catch (SocketException ex)
            {
                _connectedClients.Disconnect(remoteClient);
                // Connection reset events are normal; do not flood the logs with these disconnect messages.
                if (ex.SocketErrorCode != SocketError.ConnectionReset)
                {
                    Logger.Log(new LogEntry(ex));
                }
            }
            catch (Exception ex)
            {
                _connectedClients.Disconnect(remoteClient);
                Logger.Log(new LogEntry(ex));
            }
        }

        private void BgListen()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                _listener.Listen(500);
                Status = SocketServerStatus.Started;

                // Signal that the socket server is ready for incoming connections.
                ServerStarted.Set();

                while (Status != SocketServerStatus.Stopped && Status != SocketServerStatus.Stopping)
                {
                    // Set the event to nonsignaled state.
                    AllDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    _listener.BeginAccept(new AsyncCallback(AcceptCallback), _listener);

                    // Wait until a connection is made before continuing.
                    AllDone.Wait();
                }
            }
            catch (Exception ex)
            {
                Status = SocketServerStatus.Stopped;
                Logger.Log(new LogEntry(ex));
            }
        }

        private void BgProcessPollRequest(Client remoteClient)
        {
            try
            {
                // Send poll response.
                remoteClient.SendIMessage(new PollingResponseV1(), false);
            }
            catch (Exception ex)
            {
                Logger.Log(new LogEntry(ex));
            }
        }

        private void BgProcessHandshake2(Client remoteClient, Handshake2 message)
        {
            try
            {
                remoteClient.ClientSocketMeisterVersion = message.ClientSocketMeisterVersion;
                remoteClient.FriendlyName = message.FriendlyName;
                remoteClient.ImportSubscriptions(message.ChangeBytes);

                bool serverSupportsClientVersion = true;
                if (message.ClientSocketMeisterVersion < Constants.MINIMUM_CLIENT_VERSION_SUPPORTED_BY_SERVER)
                {
                    serverSupportsClientVersion = false;
                }
                // Send Handshake 2 ACK.
                remoteClient.SendIMessage(new Handshake2Ack(serverSupportsClientVersion), serverSupportsClientVersion);
            }
            catch (Exception ex)
            {
                Logger.Log(new LogEntry(ex));
            }
        }

        private void BgProcessSubscriptionChanges(Client remoteClient, TokenChangesRequestV1 request)
        {
            try
            {
                // Import subscriptions and send subscription response.
                remoteClient.SendIMessage(remoteClient.ImportSubscriptionChanges(request), false);
            }
            catch (Exception ex)
            {
                Logger.Log(new LogEntry(ex));
            }
        }

        private void BgProcessMessage(MessageV1 message)
        {
            try
            {
                // Deserialize the message. We have a complete message, so unpack, reset counters, and raise an event.
                MessageReceivedEventArgs args = new MessageReceivedEventArgs(message.RemoteClient, message.Parameters);

                if (MessageReceived == null)
                {
                    Exception ex = new Exception("There is no process on the server listening to 'MessageReceived' events from the socket server.");
                    MessageResponseV1 noListener = new MessageResponseV1(message.MessageId, ex);
                    message.RemoteClient.SendIMessage(noListener, false);
                }
                else
                {
                    MessageReceived(this, args);

                    // Send response.
                    MessageResponseV1 response = new MessageResponseV1(message.MessageId, args.Response);
                    message.RemoteClient.SendIMessage(response, false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new LogEntry(ex));
                MessageResponseV1 response = new MessageResponseV1(message.MessageId, ex);
                message.RemoteClient.SendIMessage(response, false);
            }
        }

        private static IPAddress GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void SendServerStoppingMessage(Client RemoteClient)
        {
            try
            {
                if (RemoteClient.ClientSocket == null || RemoteClient.ClientSocket.Connected == false)
                    return;
                byte[] sendBytes = MessageEngine.GenerateSendBytes(new ServerStoppingNotificationV1(Constants.MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING), _compressSentData);
                RemoteClient.ClientSocket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                Logger.Log(new LogEntry(ex));
            }
        }

        internal void IncrementSentTotals(int BytesSent)
        {
            Interlocked.Add(ref _totalBytesSent, BytesSent);
            Interlocked.Increment(ref _totalMessagesSent);
        }

        private void ConnectedClients_ClientConnected(object sender, ClientEventArgs e)
        {
            // Raise event in the background using a Task. This ensures the event does not block the main thread.
            Task.Run(() =>
            {
                try
                {
                    ClientConnected?.Invoke(this, e);
                }
                catch
                {
                    // Ignore any errors that occurred in event subscribers while processing the event.
                }
            });
        }

        private void ConnectedClients_ClientDisconnected(object sender, ClientEventArgs e)
        {
            // Raise event in the background using a Task.
            Task.Run(() =>
            {
                try
                {
                    ClientDisconnected?.Invoke(this, e);
                }
                catch
                {
                    // Ignore any errors that occurred in event subscribers while processing the event.
                }
            });
        }

        private void Logger_LogRaised(object sender, LogEventArgs e)
        {
            LogRaised?.Invoke(this, e);
        }
    }
}
#endif
