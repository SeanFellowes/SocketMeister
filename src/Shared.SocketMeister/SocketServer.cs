#if !SMNOSERVER && !NET35
using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace SocketMeister
{
    /// <summary>
    /// TCP/IP socket server which listens for client connections and raises events when messages are received
    /// </summary>
#if SMISPUBLIC
    public partial class SocketServer : IDisposable
#else
    internal partial class SocketServer
#endif
    {
        /// <summary>
        /// The maximum number of milliseconds to wait for clients to disconnect whien stopping the socket server
        /// </summary>
        private const int MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING = 30000;

        internal ManualResetEventSlim ServerStarted { get; set; } = new ManualResetEventSlim(false);

        internal ManualResetEventSlim _allDone { get; set; } = new ManualResetEventSlim(false);
        private readonly Clients _connectedClients = new Clients();
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
        /// Event raised when a client connects to the socket server. Raised in a seperate thread
        /// </summary>
        public event EventHandler<ClientEventArgs> ClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the socket server. Raised in a seperate thread
        /// </summary>
        public event EventHandler<ClientEventArgs> ClientDisconnected;

        /// <summary>
        /// Raised when the status of the socket listener changes. Raised in a seperate thread
        /// </summary>
        public event EventHandler<EventArgs> StatusChanged;

        /// <summary>
        /// Raised when a  message is received from a client. An optional response can be provided which will be returned to the client. Raised in a seperate thread.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        public event EventHandler<TraceEventArgs> TraceEventRaised;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Port">Port that this socket server will listen on</param>
        /// <param name="CompressSentData">Enable compression on message data</param>
        public SocketServer(int Port, bool CompressSentData)
        {
            _compressSentData = CompressSentData;
            _status = SocketServerStatus.Starting;
            try
            {

                //  CONNECT TO ALL INTERFACES (I.P. 0.0.0.0 IS ALL)
                IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
                _localEndPoint = new IPEndPoint(ipAddress, Port);

                //  LOCAL IP ADDRESS AND PORT (USED FOR DIAGNOSTIC MESSAGES)
                _endPoint = GetLocalIPAddress().ToString() + ":" + Port.ToString(CultureInfo.InvariantCulture);

                // Create a TCP/IP socket.  
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listener.Bind(_localEndPoint);

                //  WARNING - DO NOT USE SocketOptionName.ReceiveTimeout OR SocketOptionName.SendTimeout. TRIED THIS AND IT COMPLETELY BROKE THIS FOR BIG DATA TRANSFERS
                _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, 0);

                //  REGISTER FOR EVENTS
                _connectedClients.ClientDisconnected += ConnectedClients_ClientDisconnected;
                _connectedClients.ClientConnected += ConnectedClients_ClientConnected;
                _connectedClients.TraceEventRaised += ConnectedClients_ExceptionRaised;

                //  SETUP BACKGROUND PROCESS TO FOR LISTENING
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
        /// Dispose of the class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the class
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return; // Prevent double dispose
            _disposed = true;

            if (disposing)
            {
                Stop(); // Safely stop the server
                _listener?.Dispose(); // Clean up socket
                _allDone?.Dispose();
                ServerStarted?.Dispose();
               // _connectedClients?.Dispose(); // Ensure clients are disposed
            }
        }

        /// <summary>
        /// Dispose of the class
        /// </summary>
        ~SocketServer()
        {
            Dispose(false);
        }



        internal Clients ConnectedClients => _connectedClients;

        /// <summary>
        /// The IP Address and Port that this socket server is using to communicate (e.g. 10.200.50.25:6000).
        /// </summary>
        public string EndPoint => _endPoint;


        /// <summary>
        /// Current status of the SocketServer
        /// </summary>
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
        /// The total number of bytes which have been received through the socket server since it started
        /// </summary>
        public long TotalBytesReceived
        {
            get { lock (_lockTotals) { return _totalBytesReceived; } }
        }

        /// <summary>
        /// The total number of bytes which have been sent through the socket server since it started
        /// </summary>
        public long TotalBytesSent => Interlocked.Read(ref _totalBytesSent);


        /// <summary>
        /// The total number of messages that have been sent through the socket server since it started;
        /// </summary>
        public int TotalMessagesSent => Interlocked.CompareExchange(ref _totalMessagesSent, 0, 0);


        /// <summary>
        /// The total number of messages that have been received through the socket server since it started;
        /// </summary>
        public int TotalMessagesReceived
        {
            get { lock (_lockTotals) { return _totalMessagesReceived; } }
        }


        /// <summary>
        /// Whether the socket service is in the process of stopping.
        /// </summary>
        private bool StopSocketServer { get { lock (_lock) { return _stopSocketServer; } } set { lock (_lock) { _stopSocketServer = value; } } }




#region Public Methods

        /// <summary>
        /// Send a message to all connected clients. Exceptions will not halt this process, but generate 'ExceptionRaised' events. 
        /// </summary>
        /// <param name="Name">Optional Name/Tag/Identifier for the broadcast.</param>
        /// <param name="Parameters">Parameters to send with the message</param>
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
                    NotifyTraceEventRaised(ex, 5008);
                }
            }
        }

        /// <summary>
        /// Send a message to all clients subscribing to a subscription name. Exceptions will not halt this process, but generate 'ExceptionRaised' events. 
        /// </summary>
        /// <param name="Name">Optional Name/Tag/Identifier for the broadcast.</param>
        /// <param name="Parameters">Parameters to send with the message</param>
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
                    NotifyTraceEventRaised(ex, 5008);
                }
            }
        }


        /// <summary>
        /// Number of clients connected to the socket server.
        /// </summary>
        public int ClientCount => _connectedClients.Count;

        /// <summary>
        /// Are there any clients subscribing to a subscription name
        /// </summary>
        /// <param name="SubscriptionName">Name of the subscription (Case insensitive)</param>
        /// <returns>true if there is at least one client subscribing to the SubscriptionName</returns>
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
        /// Returns a list of clients which are connected to the socket server
        /// </summary>
        /// <returns>List of clients</returns>
        public List<Client> GetClients()
        {
            return _connectedClients.ToList();
        }

        /// <summary>
        /// Starts the socket server. It will begin listening for incoming connections.
        /// </summary>
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
        /// Sends a message to all clients to disconnect, waits for in progress messages to finish, then stops the socket server. 
        /// </summary>
        public void Stop()
        {
            if (Status != SocketServerStatus.Started)
                throw new InvalidOperationException("Socket server is not currently running.");

            Status = SocketServerStatus.Stopping;
            _allDone.Set();

            var toProcess = _connectedClients.ToList();

            // Send ServerStoppingNotification concurrently
            Parallel.ForEach(toProcess, remoteClient =>
            {
                try
                {
                    SendServerStoppingMessage(remoteClient);
                }
                catch (Exception ex)
                {
                    NotifyTraceEventRaised(ex, 5013);
                }
            });

            // Wait for clients to disconnect with timeout
            DateTime maxWait = DateTime.UtcNow.AddMilliseconds(MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING);
            while (_connectedClients.Count > 0 && DateTime.UtcNow < maxWait)
            {
                Thread.Sleep(50); // Reduced sleep interval for responsiveness
            }

            _connectedClients.DisconnectAll(); // Force disconnect remaining clients
            _listener?.Close();

            Status = SocketServerStatus.Stopped;
        }

        #endregion



        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            _allDone.Set();

            if (Status == SocketServerStatus.Stopped)
            {
                return;
            }
            else if (Status == SocketServerStatus.Stopping)
            {
                // Accept the connection but disconnect the client
                Task.Run(() =>
                {
                    Socket listener = (Socket)ar.AsyncState;
                    Socket handler = null;
                    try { handler = listener.EndAccept(ar); }
                    catch { return; }

                    // Shutdown the client's socket
                    try { handler.Shutdown(SocketShutdown.Both); }
                    catch { }
                    try { handler.Close(); }
                    catch { }
                    return;
                });

            }
            else
            {
                // Receive data on a dedicated background task
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

                    //  Send SocketServerHandshake1 to the client which will indicate that the server is reasy to receive data.
                    try
                    {
                        Thread.Sleep(1000);
                        byte[] sendBytes = MessageEngine.GenerateSendBytes(new Handshake1(Constants.SOCKET_SERVER_VERSION, remoteClient.ClientId.ToString()), _compressSentData);
                        remoteClient.ClientSocket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
                    }
                    catch (Exception ex)
                    {
                        NotifyTraceEventRaised(ex, 5008);
                    }

                });
            }
        }


        private void ReadCallback(IAsyncResult ar)
        {
            Client remoteClient = null;

            try
            {
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                remoteClient = (Client)ar.AsyncState;
                MessageEngine receiveEnvelope = remoteClient.ReceiveEngine;

                // Read data from the client socket.  
                if (remoteClient.ClientSocket.Connected == false) return;
                int receivedBytesCount = remoteClient.ClientSocket.EndReceive(ar);
                int receiveBufferPtr = 0;

                //  IF WE RECEIVED 0 BYTES, THE CLIENT IS SHUTTING DOWN
                if (receivedBytesCount == 0)
                {
                    //  A GRACEFUL CLOSE SHOULD OCCUR LIKE THIS (* IS THIS STEP/S)
                    //  1.   The client socket calls Shutdown(SocketShutdown.Send)) but should keep receiving
                    //  2. * On the server, EndReceive returns 0 bytes read(the client signals there is no more data from its side)
                    //  3. * The server A) sends its last data B) calls Shutdown(SocketShutdown.Send)) C) calls Close on the socket, optionally with a timeout to allow the data to be read from the client
                    //  4.   The client A) reads the remaining data from the server and then receives 0 bytes(the server signals there is no more data from its side) B) calls Close on the socket
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
                                // Offload CPU-bound work to a thread pool thread
                                Task.Run(() => BgProcessMessage(message));
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageType.MessageResponseV1)
                        {
                            if (Status == SocketServerStatus.Started)
                            {
                                //  PROCESS ResponseMessage. NOTE: METHOD IS EXECUTED IN A ThreadPool THREAD
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
                                NotifyTraceEventRaised(ex, 5008);
                            }
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


                        else if (receiveEnvelope.MessageType == MessageType.SubscriptionChangesNotificationV1)
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

                            //  Unknown message received. The client may be a newer version of SocketMeister. Raise a message and ignore
                            NotifyTraceEventRaised(new Exception("ReadCallback() encountered an Unknown message type received (" + receiveEnvelope.MessageType.ToString() + "). The server may be running a newer version of SocketMeister"), 4665);

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
                //  CONNECTION RESET EVENTS ARE NORMAL. WE DON'T WANT EVENT LOGS FULL OF THESE DISCONNECT MESSAGES
                if (ex.SocketErrorCode != SocketError.ConnectionReset) NotifyTraceEventRaised(ex, 5008);
            }
            catch (Exception ex)
            {
                _connectedClients.Disconnect(remoteClient);
                NotifyTraceEventRaised(ex, 5008);
            }
        }

        private async Task ProcessMessageAsync(Client remoteClient, MessageEngine receiveEnvelope)
        {
            if (receiveEnvelope.MessageType == MessageType.MessageV1)
            {
                var message = receiveEnvelope.GetMessageV1();
                await Task.Run(() => BgProcessMessage(message));
            }
            // Add similar async handling for other message types
        }


        private void BgListen()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                _listener.Listen(500);
                Status = SocketServerStatus.Started;

                // Signal all clear to waiting socket server clients which may try to send data before service startup is complete
                ServerStarted.Set();

                while (Status != SocketServerStatus.Stopped && Status != SocketServerStatus.Stopping)
                {
                    // Set the event to nonsignaled state.  
                    _allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    _listener.BeginAccept(new AsyncCallback(AcceptCallback), _listener);

                    // Wait until a connection is made before continuing.  
                    _allDone.Wait();
                }
            }
            catch (Exception ex)
            {
                Status = SocketServerStatus.Stopped;
                NotifyTraceEventRaised(ex, 5008);
            }
        }


        private void BgProcessPollRequest(Client remoteClient)
        {
            try
            {
                //  SEND POLL RESPONSE
                remoteClient.SendIMessage(new PollingResponseV1(), false);
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }
        }


        private void BgProcessHandshake2(Client remoteClient, Handshake2 message)
        {
            try
            {
                remoteClient.ClientSocketMeisterVersion = message.ClientSocketMeisterVersion;
                remoteClient.FriendlyName = message.FriendlyName;
                remoteClient.SendIMessage(new Handshake2Ack(), false);
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }
        }


        private void BgProcessSubscriptionChanges(Client remoteClient, TokenChangesRequestV1 request)
        {
            try
            {
                //  IMPORTS AND SEND SUBSCRIPTION RESPONSE
                remoteClient.SendIMessage(remoteClient.ImportSubscriptionChanges(request), false);
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }
        }



        private void BgProcessMessage(MessageV1 message)
        {
            try
            {
                //  DESERIALIZE THE MESSAGE
                //  WE HAVE A MESSAGE IN FULL. UNPACK, (RESETS COUNTERS) AND RAISE AN EVENT
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

                    //  SEND RESPONSE
                    MessageResponseV1 response = new MessageResponseV1(message.MessageId, args.Response);
                    message.RemoteClient.SendIMessage(response, false);
                }
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
                MessageResponseV1 response = new MessageResponseV1(message.MessageId, ex);
                message.RemoteClient.SendIMessage(response, false);
            }
        }


        internal static IPAddress GetLocalIPAddress()
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


        internal void NotifyTraceEventRaised(Exception ex, int ErrorNumber)
        {
            NotifyTraceEventRaised(new TraceEventArgs(ex, ErrorNumber));
        }


        private void NotifyTraceEventRaised(TraceEventArgs args)
        {
            // Raise event in the background using a Task
            Task.Run(() =>
            {
                try
                {
                    TraceEventRaised?.Invoke(this, args);
                }
                catch 
                {
                    // Ignore any errors in the event handlers
                }
            });
        }


        private void SendServerStoppingMessage(Client RemoteClient)
        {
            try
            {
                if (RemoteClient.ClientSocket == null || RemoteClient.ClientSocket.Connected == false) return;
                byte[] sendBytes = MessageEngine.GenerateSendBytes(new ServerStoppingNotificationV1(MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING), _compressSentData);
                RemoteClient.ClientSocket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }
        }



        internal void IncrementSentTotals(int BytesSent)
        {
            Interlocked.Add(ref _totalBytesSent, BytesSent);
            Interlocked.Increment(ref _totalMessagesSent);
        }

        private void ConnectedClients_ClientConnected(object sender, ClientEventArgs e)
        {
            // Raise event in the background using a Task
            Task.Run(() =>
            {
                try
                {
                    ClientConnected?.Invoke(this, e);
                }
                catch
                {
                    // Ignore any errors in the event handlers
                }
            });
        }

        private void ConnectedClients_ClientDisconnected(object sender, ClientEventArgs e)
        {
            // Raise event in the background using a Task
            Task.Run(() =>
            {
                try
                {
                    ClientDisconnected?.Invoke(this, e);
                }
                catch
                {
                    // Ignore any errors in the event handlers
                }
            });
        }

        private void ConnectedClients_ExceptionRaised(object sender, TraceEventArgs e)
        {
            NotifyTraceEventRaised(e);
        }


    }
}
#endif
