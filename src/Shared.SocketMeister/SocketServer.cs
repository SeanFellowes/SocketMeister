#if !SILVERLIGHT && !SMNOSERVER
using System;
using System.Collections.Generic;
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

        private ManualResetEvent _allDone = new ManualResetEvent(false);
        private readonly Clients _connectedClients = new Clients();
        private bool _disposed = false;
        private readonly bool _enableCompression;
        private readonly string _endPoint;
        private readonly Socket _listener = null;
        private SocketServerStatus _listenerState;
        private readonly IPEndPoint _localEndPoint = null;
        private readonly object _lock = new object();
        private int _requestsInProgress = 0;
        private bool _isStopRequested;
        private Thread _threadListener = null;

        /// <summary>
        /// Event raised when a client connects to the socket server (Raised in a seperate thread)
        /// </summary>
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        /// <summary>
        /// Event raised when a client disconnects from the socket server (Raised in a seperate thread)
        /// </summary>
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        /// Raised when an exception occurs.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;

        /// <summary>
        /// Reaised when the status of the socket listener changes.
        /// </summary>
        public event EventHandler<SocketServerStatusChangedEventArgs> ListenerStateChanged;

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
            _localEndPoint = new IPEndPoint(ipAddress, Port);

            //  LOCAL IP ADDRESS AND PORT (USED FOR DIAGNOSTIC MESSAGES)
            _endPoint = GetLocalIPAddress().ToString() + ":" + Port.ToString();

            // Create a TCP/IP socket.  
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(_localEndPoint);

            //  REGISTER FOR EVENTS
            _connectedClients.ClientDisconnected += _connectedClients_ClientDisconnected;
            _connectedClients.ClientConnected += _connectedClients_ClientConnected;
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
        public SocketServerStatus ListenerState
        {
            get { lock (_lock) { return _listenerState; } }
            set
            {
                lock (_lock)
                {
                    if (_listenerState == value) return;
                    _listenerState = value;
                }
                ListenerStateChanged?.Invoke(null, new SocketServerStatusChangedEventArgs { Status = value });
            }
        }

        /// <summary>
        /// The number of client requests currently being executed.
        /// </summary>
        public int RequestsInProgress { get { lock (_lock) { return _requestsInProgress; } } }

        /// <summary>
        /// Whether the socket service is in the process of stopping.
        /// </summary>
        private bool IsStopRequested { get { lock (_lock) { return _isStopRequested; } } set { lock (_lock) { _isStopRequested = value; } } }


#region Public Methods

        /// <summary>
        /// Send a message to all connected clients. Exceptions will not halt this process, but generate 'ExceptionRaised' events. 
        /// </summary>
        /// <param name="Parameters">Parameters to send with the message</param>
        /// <param name="TimeoutMilliseconds">Number of milliseconds to wait before timing out</param>
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
                    NotifyExceptionRaised(ex);
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
            IsStopRequested = false;
            _threadListener.Start();
        }


        /// <summary>
        /// Sends a message to all clients to disconnect, waits for in progress requests to finish, then stops the socket server. 
        /// </summary>
        public void Stop()
        {
            if (ListenerState != SocketServerStatus.Started) throw new Exception("Socket server is stopped, or in the process of starting or stopping.");

            ListenerState = SocketServerStatus.Stopping;
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
                    ExceptionRaised?.Invoke(this, new ExceptionEventArgs(new Exception("There were " + _connectedClients.Count + " client/s still connected after an attempt to close them. They will now be force closed"), 5013));
                    break;
                }
                Thread.Sleep(200);
            }

            //  STOP BACKGROUND THREADS
            IsStopRequested = true;

            //  STOP RECEIVING
            try { _listener.Shutdown(SocketShutdown.Receive); }
            catch { }

            //  CLOSE CONNECTED CLIENTS
            _connectedClients.DisconnectAll();

            //  CLOSE LISTENER
            try { _listener.Shutdown(SocketShutdown.Send); }
            catch { }
            try { _listener.Close(); }
            catch { }

            ListenerState = SocketServerStatus.Stopped;
        }

#endregion






        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            _allDone.Set();

            if (ListenerState == SocketServerStatus.Stopped || ListenerState == SocketServerStatus.Stopping)
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
                    try { handler.Shutdown(SocketShutdown.Both); }
                    catch { }
                    try { handler.Close(); }
                    catch { }
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
                    catch { return; }
                    handler.SendTimeout = 30000;
                    // Create the state object.  
                    Client remoteClient = new Client(this, handler, _enableCompression);
                    _connectedClients.Add(remoteClient);
                    handler.BeginReceive(remoteClient.ReceiveBuffer, 0, SocketClient.SEND_RECEIVE_BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), remoteClient);
                }))
                {
                    IsBackground = true
                };
                bgReceive.Start();
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
                            if (ListenerState == SocketServerStatus.Stopping)
                            {
                                ResponseMessage response = new ResponseMessage(request.RequestId, new Exception("Server is stopping"));
                                SendMessage(request.RemoteClient, response, false);
                            }
                            else
                            {
                                lock (_lock) { _requestsInProgress = _requestsInProgress + 1; }
                                ThreadPool.QueueUserWorkItem(BgProcessRequestMessage, request);
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageTypes.Message)
                        {
                            if (ListenerState == SocketServerStatus.Started)
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
                                NotifyExceptionRaised(ex);
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageTypes.PollRequest)
                        {
                            if (ListenerState == SocketServerStatus.Started)
                            {
                                lock (_lock) { _requestsInProgress = _requestsInProgress + 1; }
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
                if (ex.SocketErrorCode != SocketError.ConnectionReset) NotifyExceptionRaised(ex);
            }
            catch (Exception ex)
            {
                _connectedClients.Disconnect(remoteClient);
                NotifyExceptionRaised(ex);
            }
        }

        private void BgListen()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                ListenerState = SocketServerStatus.Starting;
                _listener.Listen(500);
                ListenerState = SocketServerStatus.Started;

                while (ListenerState != SocketServerStatus.Stopped)
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
                ListenerState = SocketServerStatus.Stopped;
                NotifyExceptionRaised(ex);
            }
        }

        private void BgProcessMessage(object state)
        {
            Messages.Message request = (Messages.Message)state;
            try
            {
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(request.RemoteClient, request.Parameters));
            }
            catch (Exception ex)
            {
                NotifyExceptionRaised(ex);
            }
        }

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
                NotifyExceptionRaised(ex);
            }
            finally
            {
                lock (_lock) { _requestsInProgress = _requestsInProgress - 1; }
            }
        }

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
                NotifyExceptionRaised(ex);
                ResponseMessage response = new ResponseMessage(request.RequestId, ex);
                SendMessage(request.RemoteClient, response, false);
            }
            finally
            {
                lock (_lock) { _requestsInProgress = _requestsInProgress - 1; }
            }
        }

        internal static IPAddress GetLocalIPAddress()
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

        private void NotifyExceptionRaised(Exception Error)
        {
            if (ExceptionRaised != null)
            {
                string msg = Error.Message;
                if (Error.StackTrace != null) msg += Environment.NewLine + Environment.NewLine + Error.StackTrace;
                if (Error.InnerException != null) msg += Environment.NewLine + Environment.NewLine + "Inner Exception: " + Error.InnerException.Message;
                //  RAISE EVENT IN THE BACKGROUND
                new Thread(new ThreadStart(delegate
                {
                    try { ExceptionRaised?.Invoke(this, new ExceptionEventArgs(Error, 5008)); }
                    catch { }
                }
                )).Start();

            }
        }

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
                NotifyExceptionRaised(ex);
            }
        }

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
                NotifyExceptionRaised(ex);
            }
        }

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
                NotifyExceptionRaised(ex);
            }
            catch (Exception ex)
            {
                _connectedClients.Disconnect(RemoteClient);
                NotifyExceptionRaised(ex);
            }
        }

        private void _connectedClients_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        private void _connectedClients_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            ClientDisconnected?.Invoke(this, e);
        }
    }
}
#endif