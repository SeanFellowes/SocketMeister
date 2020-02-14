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

        private readonly ManualResetEvent allDone = new ManualResetEvent(false);
        private readonly object classLock = new object();
        private readonly Clients connectedClients = new Clients();
        private bool disposed = false;
        private readonly bool enableCompression;
        private readonly string endPoint;
        private readonly Socket listener = null;
        private readonly int port;
        private int requestsInProgress = 0;
        private ServiceStatus status;
        private readonly Thread threadListener;

        /// <summary>
        /// Event raised when a client connects to the socket server (Raised in a seperate thread)
        /// </summary>
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        /// <summary>
        /// Event raised when when there is a change to the clients connected to the socket server
        /// </summary>
        public event EventHandler<ClientsChangedEventArgs> ClientsChanged;

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


        private void NotifyTraceEventRaised(Exception ex, int ErrorNumber)
        {
            try
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, ErrorNumber));
            }
            catch { }
        }

        private void NotifyTraceEventRaised(TraceEventArgs args)
        {
            try
            {
                TraceEventRaised?.Invoke(this, args);
            }
            catch { }
        }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">Port that this socket server will listen on</param>
        /// <param name="enableCompression">Enable compression on message data</param>
        public SocketServer(int port, bool enableCompression)
        {
            this.port = port;
            this.enableCompression = enableCompression;

            //  SETUP BACKGROUND PROCESS TO FOR LISTENING
            threadListener = new Thread(new ThreadStart(BgListen))
            {
                IsBackground = true
            };

            //  CONNECT TO ALL INTERFACES (I.P. 0.0.0.0 IS ALL)
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            IPEndPoint localEP = new IPEndPoint(ipAddress, port);

            //  LOCAL IP ADDRESS AND PORT (USED FOR DIAGNOSTIC MESSAGES)
            endPoint = GetLocalIPAddress().ToString() + ":" + port.ToString(CultureInfo.InvariantCulture);

            // Create a TCP/IP socket.  
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEP);

            //  REGISTER FOR EVENTS
            connectedClients.ClientDisconnected += ConnectedClients_ClientDisconnected;
            connectedClients.ClientConnected += ConnectedClients_ClientConnected;
            connectedClients.TraceEventRaised += ConnectedClients_ExceptionRaised;
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
            if (disposed) return;
            if (disposing)
            {
                //  NOTE: If you application uses .NET 2.0 or .NET 3.5. add NET20 or NET35 as a conditional compilation symbol, in your project's Build properties
#if !NET20 && !NET35
                allDone.Dispose();
                listener.Dispose();
#else
                allDone.Close();
                listener.Close();
#endif
            }
            disposed = true;
        }


        /// <summary>
        /// The IP Address and Port that this socket server is using to communicate (e.g. 10.200.50.25:6000).
        /// </summary>
        public string EndPoint { get { return endPoint; } }

        /// <summary>
        /// Status of the socket listener
        /// </summary>
        public ServiceStatus Status
        {
            get { lock (classLock) { return status; } }
            set
            {
                lock (classLock)
                {
                    if (status == value) return;
                    status = value;
                }
                ListenerStateChanged?.Invoke(null, new ServerStatusEventArgs(value));
            }
        }

        /// <summary>
        /// The number of client requests currently being executed.
        /// </summary>
        public int RequestsInProgress { get { lock (classLock) { return requestsInProgress; } } }


        #region Public Methods

        /// <summary>
        /// Send a message to all connected clients. Exceptions will not halt this process, but generate 'ExceptionRaised' events. 
        /// </summary>
        /// <param name="parameters">Parameters to send with the message</param>
        /// <param name="timeoutMilliseconds">Number of milliseconds to wait before timing out</param>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        public void BroadcastMessage(object[] parameters, int timeoutMilliseconds = 60000)
        {
            Messages.Message message = new Messages.Message(parameters, timeoutMilliseconds);
            List<Client> clients = connectedClients.ToList();
            foreach (Client client in clients)
            {
                try
                {
                    SendMessage(client, message, true);
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
        public int ClientCount { get { return connectedClients.Count; } }

        /// <summary>
        /// Returns a list of clients which are connected to the socket server
        /// </summary>
        /// <returns>List of clients</returns>
        public List<Client> GetClients()
        {
            return connectedClients.ToList();
        }

        /// <summary>
        /// Starts the socket server. It will begin listening for incoming connections.
        /// </summary>
        public void Start()
        {
            threadListener.Start();
        }


        /// <summary>
        /// Sends a message to all clients to disconnect, waits for in progress requests to finish, then stops the socket server. 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        public void Stop()
        {
            if (Status != ServiceStatus.Started) throw new Exception("Socket server is stopped, or in the process of starting or stopping.");

            Status = ServiceStatus.Stopping;
            allDone.Set();

            List<Client> toProcess = connectedClients.ToList();

            //  SEND SYNCRONOUS DISCONNECT MESSAGE TO CLIENTS
            foreach (Client remoteClient in toProcess)
            {
                SendDisconnectMessage(remoteClient.ClientSocket);
            }

            //  WAIT FOR CLIENTS TO DISCONNECT
            DateTime maxWaitClientDisconnect = DateTime.Now.AddMilliseconds(MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING);
            while (true == true)
            {
                if (connectedClients.Count == 0) break;
                if (DateTime.Now > maxWaitClientDisconnect)
                {
                    NotifyTraceEventRaised(new Exception("There were " + connectedClients.Count + " client/s still connected after an attempt to close them. They will now be force closed"), 5013);
                    break;
                }
                Thread.Sleep(200);
            }

            //  STOP RECEIVING
            try { listener.Shutdown(SocketShutdown.Receive); }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }

            //  CLOSE CONNECTED CLIENTS
            connectedClients.DisconnectAll();


            //  CLOSE LISTENER
            try { listener.Shutdown(SocketShutdown.Send); }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }
            try { listener.Close(); }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }
#if ! NET35
            try { listener.Dispose(); }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }
#endif

            Status = ServiceStatus.Stopped;
        }

        #endregion






        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

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
                        NotifyTraceEventRaised(ex, 5008);
                    }
                    try
                    {
                        handler.Close();
                    }
                    catch (Exception ex)
                    {
                        NotifyTraceEventRaised(ex, 5008);
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
                        NotifyTraceEventRaised(ex, 5008);
                        return;
                    }
                    handler.SendTimeout = 30000;
                    // Create the state object.  
                    Client remoteClient = new Client(this, handler);
                    connectedClients.Add(remoteClient);
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
                                lock (classLock) { requestsInProgress += 1; }
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
                                connectedClients.Disconnect(remoteClient);
                            }
                            catch (Exception ex)
                            {
                                NotifyTraceEventRaised(ex, 5008);
                            }
                        }
                        else if (receiveEnvelope.MessageType == MessageTypes.PollRequest)
                        {
                            if (Status == ServiceStatus.Started)
                            {
                                lock (classLock) { requestsInProgress += 1; }
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
                connectedClients.Disconnect(remoteClient);
                //  CONNECTION RESET EVENTS ARE NORMAL. WE DON'T WANT EVENT LOGS FULL OF THESE DISCONNECT MESSAGES
                if (ex.SocketErrorCode != SocketError.ConnectionReset) NotifyTraceEventRaised(ex, 5008);
            }
            catch (Exception ex)
            {
                connectedClients.Disconnect(remoteClient);
                NotifyTraceEventRaised(ex, 5008);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void BgListen()
        {
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                Status = ServiceStatus.Starting;
                listener.Listen(500);
                Status = ServiceStatus.Started;
                NotifyTraceEventRaised(new TraceEventArgs("Socket server started on port " + port.ToString(), SeverityType.Information, 10023));

                while (Status != ServiceStatus.Stopped)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Stopped;
                NotifyTraceEventRaised(ex, 5008);
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
                NotifyTraceEventRaised(ex, 5008);
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
                NotifyTraceEventRaised(ex, 5008);
            }
            finally
            {
                lock (classLock) { requestsInProgress -= 1; }
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
                NotifyTraceEventRaised(ex, 5008);
                ResponseMessage response = new ResponseMessage(request.RequestId, ex);
                SendMessage(request.RemoteClient, response, false);
            }
            finally
            {
                lock (classLock) { requestsInProgress -= 1; }
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
                connectedClients.Disconnect(remoteClient);
                NotifyTraceEventRaised(ex, 5008);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void SendDisconnectMessage(Socket socket)
        {
            try
            {
                if (socket == null || socket.Connected == false) return;
                byte[] sendBytes = MessageEngine.GenerateSendBytes(new ServerStoppingMessage(MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING), enableCompression);
                socket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex, 5008);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        internal void SendMessage(Client remoteClient, IMessage message, bool async = true)
        {
            if (remoteClient == null || remoteClient.ClientSocket == null ||
                remoteClient.ClientSocket.Connected == false || remoteClient.ClientSocket.Poll(200000, SelectMode.SelectWrite) == false) return;

            try
            {
                byte[] sendBytes = MessageEngine.GenerateSendBytes(message, enableCompression);
                if (async == true)
                {
                    remoteClient.ClientSocket.BeginSend(sendBytes, 0, sendBytes.Length, 0, new AsyncCallback(SendCallback), remoteClient);
                }
                else
                {
                    remoteClient.ClientSocket.Send(sendBytes, sendBytes.Length, SocketFlags.None);
                }
            }
            catch (ObjectDisposedException ex)
            {
                connectedClients.Disconnect(remoteClient);
                NotifyTraceEventRaised(ex, 5008);
            }
            catch (Exception ex)
            {
                connectedClients.Disconnect(remoteClient);
                NotifyTraceEventRaised(ex, 5008);
            }
        }

        private void ConnectedClients_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        private void ConnectedClients_ExceptionRaised(object sender, TraceEventArgs e)
        {
            NotifyTraceEventRaised(e);
        }

        private void ConnectedClients_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            ClientDisconnected?.Invoke(this, e);
        }
    }
}
#endif