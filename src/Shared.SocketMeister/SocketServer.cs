#if !SILVERLIGHT && !SMNOSERVER
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketMeister.Messages;


namespace SocketMeister
{
    /// <summary>
    /// TCP/IP socket server which listens for client connections and raises events when messages are received
    /// </summary>
#if SMISPUBLIC
    public partial class SocketServer : IDisposable
#else
    internal partial class SocketServer : IDisposable
#endif
    {
        /// <summary>
        /// The maximum number of milliseconds to wait for clients to disconnect whien stopping the socket server
        /// </summary>
        private const int MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING = 20000;

        private readonly ManualResetEvent _allDone = new ManualResetEvent(false);
        private readonly Clients _connectedClients = new Clients();
        private bool _disposed = false;
        private readonly bool _enableCompression;
        private readonly string _endPoint;
        private readonly Socket _listener = null;
        private ServiceStatus _status;
        private readonly object _lock = new object();
        private int _requestsInProgress = 0;
        private readonly Thread _threadListener;

        /// <summary>
        /// Event raised when a client connects to the socket server (Raised in a seperate thread)
        /// </summary>
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the socket server (Raised in a seperate thread)
        /// </summary>
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        public event EventHandler<TraceEventArgs> TraceEventRaised;

        /// <summary>
        /// Reaised when the status of the socket listener changes.
        /// </summary>
        public event EventHandler<ServerStatusEventArgs> ListenerStateChanged;

        /// <summary>
        /// Raised when a message is received from a client.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raised when a request message is received from a client. A response can be provided which will be returned to the client.
        /// </summary>
        public event EventHandler<RequestReceivedEventArgs> RequestReceived;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Port">Port that this socket server will listen on</param>
        /// <param name="EnableCompression">Enable compression on message data</param>
        public SocketServer(int Port, bool EnableCompression)
        {
            _enableCompression = EnableCompression;

            //  SETUP BACKGROUND PROCESS TO FOR LISTENING
            _threadListener = new Thread(new ThreadStart(BgListen))
            {
                IsBackground = true
            };

            //  CONNECT TO ALL INTERFACES (I.P. 0.0.0.0 IS ALL)
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEP = new IPEndPoint(ipAddress, Port);

            //  LOCAL IP ADDRESS AND PORT (USED FOR DIAGNOSTIC MESSAGES)
            _endPoint = GetLocalIPAddress().ToString() + ":" + Port.ToString(CultureInfo.InvariantCulture);

            // Create a TCP/IP socket.  
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(localEP);

            //  REGISTER FOR EVENTS
            _connectedClients.ClientDisconnected += ConnectedClients_ClientDisconnected;
            _connectedClients.ClientConnected += ConnectedClients_ClientConnected;
            _connectedClients.TraceEventRaised += ConnectedClients_ExceptionRaised;
        }


        /// <summary>
        /// Dispose this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                //  NOTE: If you application uses .NET 2.0 or .NET 3.5. add NET20 or NET35 as a conditional compilation symbol, in your project's Build properties
#if !NET20 && !NET35
                _allDone.Dispose();
                _listener.Dispose();
#else
                _allDone.Close();
                _listener.Close();
#endif
            }
            _disposed = true;
        }


        /// <summary>
        /// The IP Address and Port that this socket server is using to communicate (e.g. 10.200.50.25:6000).
        /// </summary>
        public string EndPoint { get { return _endPoint; } }

        /// <summary>
        /// Status of the socket listener
        /// </summary>
        public ServiceStatus Status
        {
            get { lock (_lock) { return _status; } }
            set
            {
                lock (_lock)
                {
                    if (_status == value) return;
                    _status = value;
                }
                ListenerStateChanged?.Invoke(null, new ServerStatusEventArgs(value));
            }
        }

        /// <summary>
        /// The number of client requests currently being executed.
        /// </summary>
        public int RequestsInProgress { get { lock (_lock) { return _requestsInProgress; } } }


        #region Public Methods

        /// <summary>
        /// Send a message to all connected clients. Exceptions will not halt this process, but generate 'ExceptionRaised' events. 
        /// </summary>
        /// <param name="Parameters">Parameters to send with the message</param>
        /// <param name="TimeoutMilliseconds">Number of milliseconds to wait before timing out</param>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        public void BroadcastMessage(object[] Parameters, int TimeoutMilliseconds = 60000)
        {
            Messages.Message message = new Messages.Message(Parameters, TimeoutMilliseconds);
            List<Client> clients = _connectedClients.ToList();
            foreach (Client client in clients)
            {
                try
                {
                    SendMessage(client, message, true);
                }
                catch (Exception ex)
                {
                    TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
                }
            }
        }

        /// <summary>
        /// Number of clients connected to the socket server.
        /// </summary>
        public int ClientCount { get { return _connectedClients.Count; } }

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
            _threadListener.Start();
        }


        /// <summary>
        /// Sends a message to all clients to disconnect, waits for in progress requests to finish, then stops the socket server. 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        public void Stop()
        {
            if (Status != ServiceStatus.Started) throw new Exception("Socket server is stopped, or in the process of starting or stopping.");

            Status = ServiceStatus.Stopping;
            _allDone.Set();

            List<Client> toProcess = _connectedClients.ToList();

            //  SEND SYNCRONOUS DISCONNECT MESSAGE TO CLIENTS
            foreach (Client remoteClient in toProcess)
            {
                SendDisconnectMessage(remoteClient.ClientSocket);
            }

            //  WAIT FOR CLIENTS TO DISCONNECT
            DateTime maxWaitClientDisconnect = DateTime.Now.AddMilliseconds(MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING);
            while (true == true)
            {
                if (_connectedClients.Count == 0) break;
                if (DateTime.Now > maxWaitClientDisconnect)
                {
                    TraceEventRaised?.Invoke(this, new TraceEventArgs(new Exception("There were " + _connectedClients.Count + " client/s still connected after an attempt to close them. They will now be force closed"), 5013));
                    break;
                }
                Thread.Sleep(200);
            }

            //  STOP RECEIVING
            try { _listener.Shutdown(SocketShutdown.Receive); }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }

            //  CLOSE CONNECTED CLIENTS
            _connectedClients.DisconnectAll();

            //  CLOSE LISTENER
            try { _listener.Shutdown(SocketShutdown.Send); }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
            try { _listener.Close(); }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }

            Status = ServiceStatus.Stopped;
        }

        #endregion






        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            _allDone.Set();

            if (Status == ServiceStatus.Stopped || Status == ServiceStatus.Stopping)
            {
                //  ACCEPT THE CONNECTION BUT DISCONNECT THE CLIENT
                Thread bgReceive = new Thread(
                new ThreadStart(delegate
                {
                    Socket listener = (Socket)ar.AsyncState;
                    Socket handler = null;
                    try { handler = listener.EndAccept(ar); }
                    catch { return; }

                    ////  SEND DISCONNECT MESSAGE
                    //SendDisconnectMessage(handler);

                    //  SHUTDOWN THE SOCKET
                    try
                    {
                        handler.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception ex)
                    {
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
                    }
                    try
                    {
                        handler.Close();
                    }
                    catch (Exception ex)
                    {
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
                    }
                    return;

                }))
                {
                    IsBackground = true
                };
                bgReceive.Start();
            }
            else
            {
                //  RECEIVE DATA ON A DEDICATED BACKGROUND THREAD
                Thread bgReceive = new Thread(
                new ThreadStart(delegate
                {
                    // Get the socket that handles the client request. 
                    Socket listener = (Socket)ar.AsyncState;
                    Socket handler = null;
                    try { handler = listener.EndAccept(ar); }
                    catch (Exception ex)
                    {
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
                        return;
                    }
                    handler.SendTimeout = 30000;
                    // Create the state object.  
                    Client remoteClient = new Client(this, handler);
                    _connectedClients.Add(remoteClient);
                    handler.BeginReceive(remoteClient.ReceiveBuffer, 0, SocketClient.SEND_RECEIVE_BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), remoteClient);
                }))
                {
                    IsBackground = true
                };
                bgReceive.Start();
            }

        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void ReadCallback(IAsyncResult ar)
        {
            Client remoteClient = null;

            try
            {
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                remoteClient = (Client)ar.AsyncState;
                MessageEngine receiveEnvelope = remoteClient.ReceiveEnvelope;

                // Read data from the client socket.  
                if (remoteClient.ClientSocket.Connected == false) return;
                int receivedBytesCount = remoteClient.ClientSocket.EndReceive(ar);
                int receiveBufferPtr = 0;

                while (receiveBufferPtr < receivedBytesCount)
                {
                    if (receiveEnvelope.AddBytesFromSocketReceiveBuffer(receivedBytesCount, remoteClient.ReceiveBuffer, ref receiveBufferPtr) == true)
                    {
                        if (receiveEnvelope.MessageType == MessageTypes.RequestMessage)
                        {
                            RequestMessage request = receiveEnvelope.GetRequestMessage();
                            request.RemoteClient = remoteClient;
                            if (Status == ServiceStatus.Stopping)
                            {
                                ResponseMessage response = new ResponseMessage(request.RequestId, new Exception("Server is stopping"));
                                SendMessage(request.RemoteClient, response, false);
                            }
                            else
                            {
                                lock (_lock) { _requestsInProgress += 1; }
                                ThreadPool.QueueUserWorkItem(BgProcessRequestMessage, request);
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageTypes.Message)
                        {
                            if (Status == ServiceStatus.Started)
                            {
                                Message message = receiveEnvelope.GetMessage();
                                message.RemoteClient = remoteClient;
                                ThreadPool.QueueUserWorkItem(BgProcessMessage, message);
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageTypes.ClientDisconnectMessage)
                        {
                            try
                            {
                                _connectedClients.Disconnect(remoteClient);
                            }
                            catch (Exception ex)
                            {
                                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageTypes.PollRequest)
                        {
                            if (Status == ServiceStatus.Started)
                            {
                                lock (_lock) { _requestsInProgress += 1; }
                                new Thread(new ThreadStart(delegate
                                {
                                    BgProcessPollRequest(remoteClient);
                                }
                                )).Start();
                            }
                        }
                    }

                }
                if (remoteClient != null && remoteClient.ClientSocket != null && remoteClient.ClientSocket.Connected == true)
                {
                    remoteClient.ClientSocket.BeginReceive(remoteClient.ReceiveBuffer, 0, SocketClient.SEND_RECEIVE_BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), remoteClient);
                }
            }
            catch (SocketException ex)
            {
                _connectedClients.Disconnect(remoteClient);
                //  CONNECTION RESET EVENTS ARE NORMAL. WE DON'T WANT EVENT LOGS FULL OF THESE DISCONNECT MESSAGES
                if (ex.SocketErrorCode != SocketError.ConnectionReset) TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
            catch (Exception ex)
            {
                _connectedClients.Disconnect(remoteClient);
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void BgListen()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                Status = ServiceStatus.Starting;
                _listener.Listen(500);
                Status = ServiceStatus.Started;

                while (Status != ServiceStatus.Stopped)
                {
                    // Set the event to nonsignaled state.  
                    _allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    _listener.BeginAccept(new AsyncCallback(AcceptCallback), _listener);

                    // Wait until a connection is made before continuing.  
                    _allDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Stopped;
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void BgProcessMessage(object state)
        {
            Messages.Message request = (Messages.Message)state;
            try
            {
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(request.RemoteClient, request.Parameters));
            }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void BgProcessPollRequest(object state)
        {
            Client remoteClient = (Client)state;
            try
            {
                //  SEND POLL RESPONSE
                SendMessage(remoteClient, new PollResponse(), false);
            }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
            finally
            {
                lock (_lock) { _requestsInProgress -= 1; }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void BgProcessRequestMessage(object state)
        {
            RequestMessage request = (RequestMessage)state;
            try
            {
                //  DESERIALIZE THE REQUEST FROM THE CLIENT
                //  WE HAVE A MESSAGE IN FULL. UNPACK, (RESETS COUNTERS) AND RAISE AN EVENT
                RequestReceivedEventArgs args = new RequestReceivedEventArgs(request.RemoteClient, request.Parameters);
                RequestReceived(this, args);

                //  SEND RESPONSE
                ResponseMessage response = new ResponseMessage(request.RequestId, args.Response);
                SendMessage(request.RemoteClient, response, false);
            }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
                ResponseMessage response = new ResponseMessage(request.RequestId, ex);
                SendMessage(request.RemoteClient, response, false);
            }
            finally
            {
                lock (_lock) { _requestsInProgress -= 1; }
            }
        }

        private static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void SendCallback(IAsyncResult ar)
        {
            Client remoteClient = null;
            try
            {
                // Retrieve the socket from the state object.  
                remoteClient = (Client)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = remoteClient.ClientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                _connectedClients.Disconnect(remoteClient);
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void SendDisconnectMessage(Socket Socket)
        {
            try
            {
                if (Socket == null || Socket.Connected == false) return;
                byte[] sendBytes = MessageEngine.GenerateSendBytes(new ServerStoppingMessage(MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING), _enableCompression);
                Socket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        internal void SendMessage(Client RemoteClient, IMessage Message, bool Async = true)
        {
            if (RemoteClient == null || RemoteClient.ClientSocket == null ||
                RemoteClient.ClientSocket.Connected == false || RemoteClient.ClientSocket.Poll(200000, SelectMode.SelectWrite) == false) return;

            try
            {
                byte[] sendBytes = MessageEngine.GenerateSendBytes(Message, _enableCompression);
                if (Async == true)
                {
                    RemoteClient.ClientSocket.BeginSend(sendBytes, 0, sendBytes.Length, 0, new AsyncCallback(SendCallback), RemoteClient);
                }
                else
                {
                    RemoteClient.ClientSocket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
                }
            }
            catch (ObjectDisposedException ex)
            {
                _connectedClients.Disconnect(RemoteClient);
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
            catch (Exception ex)
            {
                _connectedClients.Disconnect(RemoteClient);
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 5008));
            }
        }

        private void ConnectedClients_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        private void ConnectedClients_ExceptionRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(this, e);
        }

        private void ConnectedClients_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            ClientDisconnected?.Invoke(this, e);
        }
    }
}
#endif