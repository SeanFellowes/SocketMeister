using SocketMeister.Messages;
using System;
#if !NET35
using System.Collections.Concurrent;
using System.Threading.Tasks;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// Asynchronous, persistent TCP/IP socket client supporting multiple endpoints.
    /// Implements <see cref="System.IDisposable"/>.
    /// For proper cleanup, see <see cref="Dispose()"/> and <see cref="Dispose(bool)"/>.
    /// </summary>
#if SMISPUBLIC
    public partial class SocketClient : IDisposable
#else
    internal partial class SocketClient : IDisposable
#endif
    {
        /// <summary>
        /// Disconnect the client if no poll response is received within this number of seconds.
        /// </summary>
        private const int DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS = 300; 

        /// <summary>
        /// Frequency (in seconds) at which the client polls the server to verify the socket is active.
        /// </summary>
        private const int POLLING_FREQUENCY = 15;

#if !NET35
        // Synchronization for cancellation token source (for .NET 4.0+)
        private object _cancellationTokenSourceLock = new object();
        private CancellationTokenSource _cancellationTokenSource;
        // Tracks background tasks for graceful shutdown
        private readonly ConcurrentDictionary<Task, bool> _backgroundTasks = new ConcurrentDictionary<Task, bool>();
#endif
        private SocketAsyncEventArgs _asyncEventArgsConnect;
        private SocketAsyncEventArgs _asyncEventArgsPolling;
        private SocketAsyncEventArgs _asyncEventArgsReceive;
        private SocketAsyncEventArgs _asyncEventArgsSendSubscriptionChanges;
        private string _clientId = "";
        private bool _connectInProgress;
        private ConnectionStatuses _connectionStatus = ConnectionStatuses.Disconnected;
        private readonly ReaderWriterLockSlim _connectionStatusLock = new ReaderWriterLockSlim();
        private SocketEndPoint _currentEndPoint = null;
        private readonly ReaderWriterLockSlim _currentEndPointLock = new ReaderWriterLockSlim();
        private bool _disposed = false;
        private readonly bool _enableCompression;
        private readonly List<SocketEndPoint> _endPoints = null;
        private readonly string _friendlyName = string.Empty;

        private bool _handshakeCompleted = false;
        private readonly object _handshakeLock = new object();
        private bool _handshake1Received;
        private bool _handshake2AckReceived;

        private bool _isBackgroundWorkerRunning;
        private DateTime? _lastMessageFromServer;
        private DateTime _lastPollResponse = DateTime.UtcNow;
        private readonly object _lock = new object();
        private readonly Logger _logger = new Logger();

        private readonly byte[] _pollingBuffer = new byte[Constants.SEND_RECEIVE_BUFFER_SIZE];
        private readonly MessageEngine _receiveEngine;
        private readonly SocketAsyncEventArgsPool _sendEventArgsPool;
        private int _serverSocketMeisterVersion;
        private bool _serverSupportsThisClientVersion = true;
        private bool _stopClientPermanently;
        private readonly object _stopClientPermanentlyLock = new object();
        private readonly TokenCollection _subscriptions = new TokenCollection();
        private readonly UnrespondedMessageCollection _unrespondedMessages = new UnrespondedMessageCollection();

        /// <summary>
        /// Event raised when the status of a socket connection changes.
        /// </summary>
        public event EventHandler<EventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Event raised when the current endpoint changes.
        /// </summary>
        public event EventHandler<EventArgs> CurrentEndPointChanged;

        /// <summary>
        /// <para>Event raised when an exception occurs.</para>
        /// <para><strong>Obsolete:</strong> This class will be removed in a future release.  
        /// Use the <see cref="LogRaised"/> event instead.</para>
        /// </summary>
        /// <remarks>
        /// Marked obsolete in v10.2.5; scheduled for removal in v11.0.0.
        /// </remarks>
        [Obsolete(
            "ExceptionRaised is deprecated and will be removed in a future release. " +
            "Please use the LogRaised event instead.",
            false  // false = compiler warning; true = compiler error
        )]
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;

        /// <summary>
        /// Raised when a message is received from the server. When processing this event, an optional response can be provided which will be returned to the server.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Event raised when the server stops.
        /// </summary>
        public event EventHandler<EventArgs> ServerStopping;

        /// <summary>
        /// Event raised whenever a broadcast is received from the server.
        /// </summary>
        public event EventHandler<BroadcastReceivedEventArgs> BroadcastReceived;

        /// <summary>
        /// Raised when a log event has been raised.
        /// </summary>
        public event EventHandler<LogEventArgs> LogRaised;



        /// <summary>
        /// Initializes a new instance of the SocketClient class.
        /// </summary>
        /// <param name="EndPoints">List of endpoints available for connection.</param>
        /// <param name="EnableCompression">Whether to apply compression to data.</param>
        /// <param name="friendlyName">Friendly name sent to the server for logging and error reporting purposes. If not provided a GUID is used.</param>
        public SocketClient(List<SocketEndPoint> EndPoints, bool EnableCompression, string friendlyName)
        {
            if (EndPoints == null) throw new ArgumentNullException(nameof(EndPoints));
            else if (EndPoints.Count == 0) throw new ArgumentException("EndPoints must contain at least 1 value", nameof(EndPoints));
            _endPoints = EndPoints;

            _enableCompression = EnableCompression;
            if (!string.IsNullOrEmpty(friendlyName)) _friendlyName = friendlyName;
            _receiveEngine = new MessageEngine();

            //  Logger
            _logger.LogRaised += Logger_LogRaised;

            //  STATIC BUFFERS
            _pollingBuffer = MessageEngine.GenerateSendBytes(new PollingRequestV1(), false);

            //  Set the default endpoint. This may be changed when the client first connects to the server
            _currentEndPoint = _endPoints[0];

            //  Setup a pool of SocketAsyncEventArgs for sending messages
            _sendEventArgsPool = new SocketAsyncEventArgsPool();
            _sendEventArgsPool.Completed += ProcessSend;

            _asyncEventArgsConnect = new SocketAsyncEventArgs();
            _asyncEventArgsConnect.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessConnect);

            _asyncEventArgsPolling = new SocketAsyncEventArgs();
            _asyncEventArgsPolling.SetBuffer(new byte[Constants.SEND_RECEIVE_BUFFER_SIZE], 0, Constants.SEND_RECEIVE_BUFFER_SIZE);
            _asyncEventArgsPolling.Completed += ProcessSendPollRequest;

            _asyncEventArgsSendSubscriptionChanges = new SocketAsyncEventArgs();
            _asyncEventArgsSendSubscriptionChanges.SetBuffer(new byte[Constants.SEND_RECEIVE_BUFFER_SIZE], 0, Constants.SEND_RECEIVE_BUFFER_SIZE);
            _asyncEventArgsSendSubscriptionChanges.Completed += ProcessSendSubscriptionChanges;

            StartBackgroundWorker();
        }

        private void Logger_LogRaised(object sender, LogEventArgs e)
        {
            LogRaised?.Invoke(this, e);
            if (ExceptionRaised != null && !(e.LogEntry.Exception == null))
                    ExceptionRaised(this, new ExceptionEventArgs(e.LogEntry.Exception, 0));
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="EndPoints">Collection of endpoints that are available to connect to</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        public SocketClient(List<SocketEndPoint> EndPoints, bool EnableCompression)
            : this(EndPoints, EnableCompression, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="IPAddress1">IP Address to of the SocketMeister server to connect to</param>
        /// <param name="Port1">TCP port the server is listening on</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        /// <param name="FriendlyName">Friendly name is sent to the SocketServer to be displayed in errors and logging. If not provided a GUID is used.</param>
        public SocketClient(string IPAddress1, int Port1, bool EnableCompression, string FriendlyName)
            : this(new List<SocketEndPoint> { new SocketEndPoint(IPAddress1, Port1) }, EnableCompression, FriendlyName) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="IPAddress1">IP Address to of the SocketMeister server to connect to</param>
        /// <param name="Port1">TCP port the server is listening on</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        public SocketClient(string IPAddress1, int Port1, bool EnableCompression)
            : this(new List<SocketEndPoint> { new SocketEndPoint(IPAddress1, Port1) }, EnableCompression, null) { }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="IPAddress1">IP Address to of the first SocketMeister server to connect to</param>
        /// <param name="Port1">TCP port the first server is listening on</param>
        /// <param name="IPAddress2">IP Address to of the second SocketMeister server to connect to</param>
        /// <param name="Port2">TCP port the second server is listening on</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        /// <param name="FriendlyName">Friendly name is sent to the SocketServer to be displayed in errors and logging. If not provided a GUID is used.</param>
        public SocketClient(string IPAddress1, int Port1, string IPAddress2, int Port2, bool EnableCompression, string FriendlyName)
            : this(
                new List<SocketEndPoint>
                {
                    new SocketEndPoint(IPAddress1, Port1),
                    new SocketEndPoint(IPAddress2, Port2)
                },
                EnableCompression, FriendlyName)
        { }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="IPAddress1">IP Address to of the first SocketMeister server to connect to</param>
        /// <param name="Port1">TCP port the first server is listening on</param>
        /// <param name="IPAddress2">IP Address to of the second SocketMeister server to connect to</param>
        /// <param name="Port2">TCP port the second server is listening on</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        public SocketClient(string IPAddress1, int Port1, string IPAddress2, int Port2, bool EnableCompression)
            : this(
                new List<SocketEndPoint>
                {
                    new SocketEndPoint(IPAddress1, Port1),
                    new SocketEndPoint(IPAddress2, Port2)
                },
                EnableCompression, null)
        {
        }


        /// <summary>
        /// Disposes of the resources used by this instance.
        /// </summary>
        /// <remarks>
        /// This method simply calls <see cref="Dispose(bool)"/> and then suppresses finalization.
        /// For more details, see <see cref="Dispose(bool)"/> and <see cref="System.IDisposable.Dispose"/>.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the class resources.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        /// <remarks>
        /// See also <see cref="Dispose()"/> and <see cref="System.IDisposable.Dispose"/>.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();

                    _connectionStatusLock.Dispose();
                    _currentEndPointLock.Dispose();

                    _asyncEventArgsConnect?.Dispose(); // Fixed disposed typo.
                    _asyncEventArgsConnect = null;

                    _asyncEventArgsPolling?.Dispose();
                    _asyncEventArgsPolling = null;

                    _asyncEventArgsReceive?.Dispose();
                    _asyncEventArgsReceive = null;

                    _sendEventArgsPool?.Dispose();

                    _asyncEventArgsSendSubscriptionChanges?.Dispose();
                    _asyncEventArgsSendSubscriptionChanges = null;

                    _unrespondedMessages.Clear(); // Explicitly clear any remaining references

                    foreach (SocketEndPoint ep in _endPoints)
                    {
                        ep.Dispose();
                    }

                    _endPoints.Clear();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes of the resources used by the class.
        /// </summary>
        ~SocketClient()
        {
            Dispose(false);
        }


        /// <summary>
        /// Adds a subscription name to the list of subscriptions. Throws an error if the name (case insensitive) exists.
        /// </summary>
        /// <param name="SubscriptionName"></param>
        public void AddSubscription(string SubscriptionName)
        {
            Token newSubs = new Token(SubscriptionName);
            _subscriptions.Add(newSubs);
        }

        /// <summary>
        /// The last time any contact was received from the server. This includes polling message/responses, instigated intermittently from the client.
        /// </summary>
        public DateTime? LastMessageFromServer
        {
            get { lock (_lock) { return _lastMessageFromServer; } }
            private set { lock (_lock) { _lastMessageFromServer = value; } }
        }

        /// <summary>
        /// Removes a subscription name from the list of subscriptions
        /// </summary>
        /// <param name="SubscriptionName">Name of the subscription to remove</param>
        /// <returns>True if the subscription was removed</returns>
        public bool RemoveSubscription(string SubscriptionName)
        {
            Token removed = _subscriptions.Remove(SubscriptionName);
            if (removed != null) return true;
            else return false;
        }


        /// <summary>
        /// GUID identifier for the client, set by the server and sent to the client during connection handshaking
        /// </summary>
        public string ClientId
        {
            get
            {
                return _clientId;
            }
            internal set
            {
                _clientId = value;
            }
        }


#if !NET35
        private CancellationTokenSource CancellationToken
        {
            get
            {
                lock (_cancellationTokenSourceLock)
                {
                    return _cancellationTokenSource;
                }
            }
            set
            {
                lock (_cancellationTokenSourceLock)
                {
                    _cancellationTokenSource = value;
                }
            }
        }
#endif

        /// <summary>
        /// Whether the client is currently connecting to a server
        /// </summary>
        private bool ConnectInProgress
        {
            get { lock (_lock) { return _connectInProgress; } }
            set { lock (_lock) { _connectInProgress = value; } }
        }


        /// <summary>
        /// The connection status of the socket client
        /// </summary>
        public ConnectionStatuses ConnectionStatus
        {
            get
            {
                //  If the socket is connected but we haven't completed polling, then we are still connecting.
                ConnectionStatuses conStatus = InternalConnectionStatus;
                if (conStatus == ConnectionStatuses.Connected && HandshakeCompleted == false)
                {
                    return ConnectionStatuses.Connecting;
                }
                return conStatus;
            }
        }


        /// <summary>
        /// A friendly name for the client which can be. If available, the SocketServer will use this in logging and error handling, otherwise a GUID is used.
        /// </summary>
        public string FriendlyName
        {
            get { return _friendlyName; }
        }



        /// <summary>
        /// Used internally. Returns the real connection status.
        /// </summary>
        private ConnectionStatuses InternalConnectionStatus
        {
            get
            {
                if (_connectionStatusLock == null)
                    return ConnectionStatuses.Disconnected;

                bool lockAcquired = false;
                try
                {
                    _connectionStatusLock.EnterReadLock();
                    lockAcquired = true;
                    return _connectionStatus;
                }
                catch (ObjectDisposedException)
                {
                    return ConnectionStatuses.Disconnected;
                }
                finally
                {
                    if (lockAcquired && _connectionStatusLock.IsReadLockHeld)
                    {
                        _connectionStatusLock.ExitReadLock();
                    }
                }
            }
            set
            {
                bool lockAcquired = false;
                try
                {
                    //  IF THE VALUE IS THE SAME, DON'T RAISE THE EVENT
                    if (InternalConnectionStatus == value) return;
                    _connectionStatusLock.EnterWriteLock();
                    lockAcquired = true;
                    _connectionStatus = value;
                }
                finally
                {
                    if (lockAcquired && _connectionStatusLock.IsWriteLockHeld)
                    {
                        _connectionStatusLock.ExitWriteLock();
                    }
                }
                RaiseConnectionStatusChanged();
            }
        }



        /// <summary>
        /// The current socket endpoint which the client is using
        /// </summary>
        public SocketEndPoint CurrentEndPoint
        {
            get
            {
                try
                {
                    _currentEndPointLock.EnterReadLock();
                    return _currentEndPoint;
                }
                finally
                {
                    _currentEndPointLock.ExitReadLock();
                }
            }
            private set
            {
                //  SET _currentEndPoint IN A LOCK
                bool lockAcquired = false;
                try
                {
                    _currentEndPointLock.EnterWriteLock();
                    lockAcquired = true;
                    _currentEndPoint = value;
                }
                finally
                {
                    if (lockAcquired && _currentEndPointLock.IsWriteLockHeld)
                    {
                        _currentEndPointLock.ExitWriteLock();
                    }

                }

            }
        }

        private bool HandshakeCompleted
        {
            get { lock (_handshakeLock) { return _handshakeCompleted; } }
            set
            {
                lock (_handshakeLock)
                {
                    _handshakeCompleted = value;
                    _handshake1Received = value;
                    _handshake2AckReceived = value;
                }
            }
        }

        private bool Handshake1Received { get { lock (_handshakeLock) { return _handshake1Received; } } set { lock (_handshakeLock) { _handshake1Received = value; } } }

        private bool Handshake2AckReceived { get { lock (_handshakeLock) { return _handshake2AckReceived; } } set { lock (_handshakeLock) { _handshake2AckReceived = value; } } }

        private bool IsBackgroundWorkerRunning { get { lock (_lock) { return _isBackgroundWorkerRunning; } } set { lock (_lock) { _isBackgroundWorkerRunning = value; } } }

        /// <summary>
        /// The last time a polling response was received from the socket server.
        /// </summary>
        private DateTime LastPollResponse { get { lock (_lock) { return _lastPollResponse; } } set { lock (_lock) { _lastPollResponse = value; } } }

        private int ServerSocketMeisterVersion { get { lock (_handshakeLock) { return _serverSocketMeisterVersion; } } set { lock (_handshakeLock) { _serverSocketMeisterVersion = value; } } }

        private bool ServerSupportsThisClientVersion { get { lock (_handshakeLock) { return _serverSupportsThisClientVersion; } } set { lock (_handshakeLock) { _serverSupportsThisClientVersion = value; } } }


        private bool StopClientPermanently { get { lock (_stopClientPermanentlyLock) { return _stopClientPermanently; } } set { lock (_stopClientPermanentlyLock) { _stopClientPermanently = value; } } }

        /// <summary>
        /// The number of subscriptions for this client
        /// </summary>
        public int SubscriptionCount => _subscriptions.Count;

        /// <summary>
        /// Messages sent to the server in which there has been no response
        /// </summary>
        internal UnrespondedMessageCollection UnrespondedMessages
        {
            get { return _unrespondedMessages; }
        }


        /// <summary>
        /// Whether a subscription name exists in the list of subscriptions. 
        /// </summary>
        /// <param name="SubscriptionName">Name of the subscription (Case insensitive).</param>
        /// <returns>True if exists, false if the subscription does not exist</returns>
        public bool DoesSubscriptionNameExist(string SubscriptionName)
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
            return _subscriptions.GetNames();
        }

        private void Disconnect(bool SocketHasErrored, ClientDisconnectReason Reason, string Message)
        {
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                if (InternalConnectionStatus == ConnectionStatuses.Disconnecting || InternalConnectionStatus == ConnectionStatuses.Disconnected) return;
                CurrentEndPoint.SetDisconnected(Reason);

                HandshakeCompleted = false;  // sets all handshake flags to false

                if (SocketHasErrored == false)
                {
                    // attempt to send a disconnecting message to the server
                    try
                    {
                        Log("Sending disconnecting message to server", Severity.Information, LogEventType.ConnectionEvent);
                        SendFastMessage(new ClientDisconnectingNotificationV1(Reason, Message));
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                }

                // initiate shutdown
                InternalConnectionStatus = ConnectionStatuses.Disconnecting;

                // stop raising events when messages are received
                try
                {
                    if (_asyncEventArgsReceive != null)
                    {
                        _asyncEventArgsReceive.Completed -= new EventHandler<SocketAsyncEventArgs>(ProcessReceive);
                        _asyncEventArgsReceive.Dispose();
                        _asyncEventArgsReceive = null;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex);
                }


#if !NET35
                // Wait for calling program to progress inbound messages, with a timeout
                try
                {
                    CancellationToken?.Cancel();  // Signal cancellation
                    Task allTasks = Task.WhenAll(_backgroundTasks.Keys);
                    if (!allTasks.Wait(TimeSpan.FromSeconds(20)))
                    {
                        Log("Timeout occurred while waiting for background tasks to complete.", Severity.Warning, LogEventType.ConnectionEvent);
                    }
                }
                catch (AggregateException ex)
                {
                    // Handle any task exceptions here
                    foreach (var innerException in ex.InnerExceptions)
                    {
                        Log(innerException);
                    }
                }
#else
                //  .NET 3.5 Only - Workaround after SocketClient fails to another endpoint more than once,
                if (!StopClientPermanently) CurrentEndPoint.RecreateSocket();
#endif


                //  Deal with unresponded messages
                if (StopClientPermanently == true)
                {
                    UnrespondedMessages.Clear();
                }
                else
                {
                    UnrespondedMessages.ResetAfterDisconnect();
                }


                if (StopClientPermanently == true)
                {
                    //  WAIT UP TO 15 SECONDS FOR BACKGROUND THREADS TO STOP
                    DateTime maxWait = DateTime.UtcNow.AddSeconds(15);
                    while ((IsBackgroundWorkerRunning == true) && DateTime.UtcNow < maxWait) { Thread.Sleep(100); }
                }

                if (!SocketHasErrored && CurrentEndPoint.Socket.Connected)
                {
                    try
                    {
                        CurrentEndPoint.Socket.Shutdown(SocketShutdown.Send);
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                }

                //  try another disconnect. It may already be disconnected, so the error will be ignored
                try
                {
                    CurrentEndPoint.Socket.Disconnect(false);
                }
                catch (Exception ex)
                {
                    Log(ex);
                }

                InternalConnectionStatus = ConnectionStatuses.Disconnected;

                //  Prepare new socket and start reconnect process
                if (StopClientPermanently == false)
                {
#if !NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
                    try { CurrentEndPoint.Socket.Dispose(); }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
            finally
            {
                timer.Stop();
                string dmsg = $"{nameof(Disconnect)}() took " + timer.ElapsedMilliseconds + " milliseconds.";
                Log(dmsg, Severity.Information, LogEventType.ConnectionEvent);
            }
        }


        /// <summary>
        /// This background thread runs until the SocketClient is stopped
        /// </summary>
        private void StartBackgroundWorker()
        {
            Thread bgWorker = new Thread(new ThreadStart(delegate
            {
                try
                {
                    IsBackgroundWorkerRunning = true;
                    Stopwatch pollingTimer = Stopwatch.StartNew();

                    while (!StopClientPermanently)
                    {
                        switch (InternalConnectionStatus)
                        {
                            case ConnectionStatuses.Disconnected:
                            case ConnectionStatuses.Connecting:
                                RestartStopwatch(pollingTimer);
                                BgPerformAttemptConnect();
                                break;
                            case ConnectionStatuses.Connected:
                                if (HandshakeCompleted == false) break;
                                BgPerformPolling(pollingTimer);
                                BgSendSubscriptionChanges();
                                break;
                        }
                        Thread.Sleep(200);
                    }
                }
                catch (Exception e) { Log(e); }
                finally
                {
                    IsBackgroundWorkerRunning = false;
                    Log("SocketClient Background Worker Exited", Severity.Information, LogEventType.ConnectionEvent);
                }
            }));
            bgWorker.IsBackground = true;
            bgWorker.Start();
        }



        /// <summary>
        /// Attempts to re-establish a connection using the endpoint with the shortest wait time.
        /// </summary>
        private void BgPerformAttemptConnect()
        {
            try
            {
                //  Exit if the client is already connecting
                if (ConnectInProgress == true) return;
                ConnectInProgress = true;

                //  This provides visual feedback that a connection attempt is underway
                if (DateTime.UtcNow < CurrentEndPoint.DontReconnectUntil.AddMilliseconds(-1000)) InternalConnectionStatus = ConnectionStatuses.Connecting;

                //  Determine the endpoint which should be current based on the lowest DontReconnectUntil from the collection
                DateTime lowestDontReconnectUntil = _endPoints[0].DontReconnectUntil;
                SocketEndPoint bestEndPoint = _endPoints[0];
                foreach (SocketEndPoint ep in _endPoints)
                {
                    if (ep.DontReconnectUntil < lowestDontReconnectUntil)
                    {
                        lowestDontReconnectUntil = ep.DontReconnectUntil;
                        bestEndPoint = ep;
                    }
                }

                //  Exit is it's not time to actually attempt to reconnect
                if (DateTime.UtcNow < lowestDontReconnectUntil)
                {
                    ConnectInProgress = false;
                    return;
                }

                //  Set the current endpoint to the best endpoint
                if (CurrentEndPoint != bestEndPoint)
                {
                    CurrentEndPoint = bestEndPoint;
                    CurrentEndPointChanged?.Invoke(this, new EventArgs());
                }

                //  Try to connect
                try
                {
                    _asyncEventArgsConnect.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                    if (!CurrentEndPoint.Socket.ConnectAsync(_asyncEventArgsConnect))
                    {
                        ProcessConnect(null, _asyncEventArgsConnect);
                    }
                }
                catch (ObjectDisposedException)
                {
                    //  Recreate the socket if it has been disposed
                    CurrentEndPoint.RecreateSocket();
                    Log("Socket recreated", Severity.Information, LogEventType.ConnectionEvent);
                    ConnectInProgress = false;
                }
                catch (Exception ex)
                {
                    Log(ex);
                    ConnectInProgress = false;
                }

                if (StopClientPermanently == true)
                {
                    ConnectInProgress = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }


        private void BgPerformPolling(Stopwatch PollingTimer)
        {
            try
            {
                if (PollingTimer.Elapsed.TotalSeconds < POLLING_FREQUENCY) return;

                if (LastPollResponse < (DateTime.UtcNow.AddSeconds(-DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS)))
                {
                    RestartStopwatch(PollingTimer);
                    string msg = "Disconnecting: Server failed to reply to polling after " + DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS + " seconds.";
                    Log(new Exception(msg));
                    Disconnect(SocketHasErrored: false, ClientDisconnectReason.PollingTimeout, msg);
                    return;
                }
                Log($"Sending polling request ({_pollingBuffer.Length} bytes)...", Severity.Information, LogEventType.PollingEvent);

                RestartStopwatch(PollingTimer);
                _asyncEventArgsPolling.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                _asyncEventArgsPolling.SetBuffer(_pollingBuffer, 0, _pollingBuffer.Length);
                if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsPolling)) ProcessSendPollRequest(null, _asyncEventArgsPolling);
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }


        private void BgSendSubscriptionChanges()
        {
            try
            {
                //  SEND SUBSCRIPTION CHANGES
                if (_subscriptions.Changed)
                {
                    byte[] changesBytes = _subscriptions.SerializeTokenChanges();
                    if (changesBytes != null)
                    {
                        byte[] sendBytes = MessageEngine.GenerateSendBytes(new TokenChangesRequestV1(changesBytes), false);
                        Log($"Sending subscription changes ({sendBytes.Length} bytes)...", Severity.Information, LogEventType.SubscriptionSyncEvent);

                        _asyncEventArgsSendSubscriptionChanges.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                        _asyncEventArgsSendSubscriptionChanges.SetBuffer(sendBytes, 0, sendBytes.Length);
                        if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsSendSubscriptionChanges)) ProcessSendSubscriptionChanges(null, _asyncEventArgsSendSubscriptionChanges);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }


        private void RaiseConnectionStatusChanged()
        {
            try
            {
                ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Log($"Caller error processing {nameof(RaiseConnectionStatusChanged)} event: {ex}", Severity.Error, LogEventType.Exception);
            }
        }

        /// <summary>
        /// Provides a Stopwatch restart mechanism for .NET 3.5 and everything else through compiler directives.
        /// </summary>
        /// <param name="stopwatch"></param>
        private void RestartStopwatch(Stopwatch stopwatch)
        {
#if NET35  
            stopwatch.Reset();
            stopwatch.Start();
#else
            stopwatch.Restart();
#endif
        }


        /// <summary>
        /// Connect operation has completed
        /// </summary>
        /// <param name="sender">Sending Socket</param>
        /// <param name="e">Socket Arguments</param>
        private void ProcessConnect(object sender, SocketAsyncEventArgs e)
        {
            if (StopClientPermanently == true) return;

            try
            {
                if (e.SocketError == SocketError.Success)
                {
#if !NET35
                    // Instantiate a new CancellationTokenSource
                    CancellationToken = new CancellationTokenSource();
#endif

                    _asyncEventArgsReceive = new SocketAsyncEventArgs();
                    _asyncEventArgsReceive.SetBuffer(new byte[Constants.SEND_RECEIVE_BUFFER_SIZE], 0, Constants.SEND_RECEIVE_BUFFER_SIZE);
                    _asyncEventArgsReceive.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessReceive);
                    if (!CurrentEndPoint.Socket.ReceiveAsync(_asyncEventArgsReceive))
                    {
                        ProcessReceive(null, _asyncEventArgsReceive);
                    }

                    //  Internally, we are connected, but ConnectionStatus property must continue to show Connecting until handshake is completed.
                    InternalConnectionStatus = ConnectionStatuses.Connected;

                    // Start a new thread to complete the handshake.
                    // Note: Server will send Handshake1 message.
                    Thread handshakeThread = new Thread(new ThreadStart(BgCompleteHandshake));
                    handshakeThread.IsBackground = true;
                    handshakeThread.Start();

                    //  Reset the last poll response time
                    LastPollResponse = DateTime.UtcNow;
                }
                else if (e.SocketError == SocketError.IsConnected)
                {
                    //  This happens with the .NET 3.5 client after failing over server endpoints.
                    //  The socket is already connected, so we can just continue.
                    CurrentEndPoint.SetDisconnected(ClientDisconnectReason.SocketError);
                    Disconnect(SocketHasErrored: false, ClientDisconnectReason.SocketError, "Socket is already connected");
                }
                else if (e.SocketError == SocketError.TimedOut)
                {
                    //  NOTE: WHEN FAILING OVER UNDER HIGH LOAD, SocketError.TimedOut OCCURS FOR UP TO 120 SECONDS (WORSE CASE)
                    //  BEFORE CONNECTION SUCCESSFULLY COMPLETES. IT'S A BIT ANNOYING BUT I HAVE FOUND NO WORK AROUND.
                    CurrentEndPoint.SetDisconnected(ClientDisconnectReason.SocketConnectionTimeout);
                    Log($"Connection failed: Socket timeout {CurrentEndPoint.Description}", Severity.Warning, LogEventType.ConnectionEvent);
                }
                else if (e.SocketError == SocketError.AddressAlreadyInUse)
                {
                    CurrentEndPoint.SetDisconnected(ClientDisconnectReason.SocketError);
                    Log(new Exception("Connection failed: Socket address already in use"));
                }
                else if (e.SocketError == SocketError.ConnectionRefused)
                {
                    CurrentEndPoint.SetDisconnected(ClientDisconnectReason.SocketConnectionRefused);
                    Log($"Connection failed: Connection refused by server {CurrentEndPoint.Description}", Severity.Warning, LogEventType.ConnectionEvent);
                }
                else
                {
                    CurrentEndPoint.SetDisconnected(ClientDisconnectReason.SocketError);
                    Log(new Exception("Connection failed: Undefined Socket Error: " + e.SocketError.ToString()));
                }
            }
            catch (Exception ex)
            {
                CurrentEndPoint.SetDisconnected(ClientDisconnectReason.Unknown);
                Log(ex);
            }
            finally
            {
                ConnectInProgress = false;
            }
        }

        /// <summary>
        /// Completes the handshake with the server after an initial connection.
        /// </summary>
        private void BgCompleteHandshake()
        {
            string traceMsg;

            // wait for handshake1 message
            Log($"Connected to {CurrentEndPoint.Description}. Awaiting handshake...", Severity.Information, LogEventType.ConnectionEvent);
            int timeoutSeconds = 30;
            DateTime timeout = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            while (DateTime.UtcNow < timeout && !Handshake1Received && !StopClientPermanently && InternalConnectionStatus == ConnectionStatuses.Connected)
            {
                Thread.Sleep(25);
            }
            if (StopClientPermanently || InternalConnectionStatus != ConnectionStatuses.Connected)
            {
                traceMsg = $"Connection reset before receiving handshake1.";
                Log(new Exception(traceMsg));
                Disconnect(SocketHasErrored: false, ClientDisconnectReason.ConnectionReset, traceMsg);
                return;
            }
            if (!Handshake1Received)
            {
                traceMsg = $"Handshake was not received within {timeoutSeconds} seconds. Disconnecting.";
                Log(new Exception(traceMsg));
                Disconnect(SocketHasErrored: false, ClientDisconnectReason.HandshakeTimeout, traceMsg);
                return;
            }

            // validate handshake1 message 
            traceMsg = $"Handshake received. Server version {ServerSocketMeisterVersion}";
            if (ServerSocketMeisterVersion < Constants.SOCKET_MEISTER_VERSION)
            {
                traceMsg += $" is older than this client version ({Constants.SOCKET_MEISTER_VERSION}).";
            }
            else if (ServerSocketMeisterVersion > Constants.SOCKET_MEISTER_VERSION)
                traceMsg += $" is newer than this client version ({Constants.SOCKET_MEISTER_VERSION}).";
            else
            {
                traceMsg += " matches the client version.";
            }
            Log(traceMsg, Severity.Information, LogEventType.ConnectionEvent);


            //  Establish whether this client supports the server version
            bool isServerVersionSupported = false;
            if (ServerSocketMeisterVersion >= Constants.MINIMUM_SERVER_VERSION_SUPPORTED_BY_CLIENT)
            {
                isServerVersionSupported = true;
            }

            // send handshake2 to the server
            SendFastMessage(new Handshake2(Constants.SOCKET_MEISTER_VERSION, FriendlyName, _subscriptions.SerializeTokens(), isServerVersionSupported));
            Log("Handshake response sent", Severity.Information, LogEventType.ConnectionEvent);

            // wait to receive a handshake2Ack message from the server
            while (DateTime.UtcNow < timeout && !Handshake2AckReceived && !StopClientPermanently && InternalConnectionStatus == ConnectionStatuses.Connected)
            {
                Thread.Sleep(25);
            }

            if (StopClientPermanently)
            {
                return;
            }
            else if (InternalConnectionStatus != ConnectionStatuses.Connected)
            {
                Log(new Exception("Connection reset before handshake completed."));
                Disconnect(SocketHasErrored: false, ClientDisconnectReason.ConnectionReset, traceMsg);
            }
            else if (!HandshakeCompleted)
            {
                traceMsg = $"Handshake could not be completed within {timeoutSeconds} seconds. Disconnecting.";
                Log(new Exception(traceMsg));
                Disconnect(SocketHasErrored: false, ClientDisconnectReason.HandshakeTimeout, traceMsg);
            }
            else if (ServerSocketMeisterVersion < Constants.MINIMUM_SERVER_VERSION_SUPPORTED_BY_CLIENT)
            {
                //  Abort if this client does not support the server version
                traceMsg = $"Disconnecting: Server version {ServerSocketMeisterVersion} is not supported. Minimum required version is {Constants.MINIMUM_SERVER_VERSION_SUPPORTED_BY_CLIENT}";
                Log(new Exception(traceMsg));
                Disconnect(SocketHasErrored: false, ClientDisconnectReason.IncompatibleServerVersion, traceMsg);
            }
            else if (!ServerSupportsThisClientVersion)
            {
                traceMsg = $"Disconnecting: Server (Version {ServerSocketMeisterVersion}) does not support this client (Version {Constants.SOCKET_MEISTER_VERSION}).";
                Log(new Exception(traceMsg));
                Disconnect(SocketHasErrored: false, ClientDisconnectReason.IncompatibleClientVersion, traceMsg);
            }
            else
            {
                //  Success. Connection is fully established.
                //  No need to change InternalConnectionStatus = Connected but me MUST raise an event so the calling
                //  software knows the connection is fully established via the ConnectionStatus property.
                RaiseConnectionStatusChanged();
                Log("Handshake completed. Connection is fully established.", Severity.Information, LogEventType.ConnectionEvent);
            }
        }


        /// <summary>
        /// Stops the client permenently. There is no option from here to restart without creating a new instance of SocketClient.
        /// </summary>
        public void Stop()
        {
            if (StopClientPermanently == true) return;  //  Don't allow this to run more than once
            StopClientPermanently = true;
            _logger.Stop();

            Disconnect(SocketHasErrored: false, ClientDisconnectReason.ClientIsStopping, "Client is stopping (Requested by calling program).");

#if !NET35
            CancellationToken?.Dispose();
#endif

        }


        private void SendResponse(MessageResponseV1 messageResponse, MessageV1 message)
        {
            byte[] sendBytes = MessageEngine.GenerateSendBytes(messageResponse, _enableCompression);

            SocketAsyncEventArgs sendEventArgs;
            while (true)
            {
                if (message.IsTimeout)
                {
                    //  TIMEOUT: NO POINT SENDING THE RESPONSE BECAUSE IT WILL HAVE ALSO TIMED OUT AT THESERVER
                    Log(new TimeoutException("Message " + message.MessageId + ", timed out at the server after " + message.TimeoutMilliseconds + " milliseconds."));
                    return;
                }
                else if (StopClientPermanently)
                {
                    //  SHUTDOWN: CLIENT IS SHUTTING DOWN. A SHUTDOWN MESSAGE SHOULD HAVE ALREADY BEEN SENT SO EXIT
                    Log("Client is stopping. Sending response without response data from user program.", Severity.Information, LogEventType.UserMessage);
                    SendFastMessage(new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.Stopping));
                    return;
                }

                Log($"Sending user response to message {message.MessageId} ({sendBytes.Length} bytes) ", Severity.Information, LogEventType.UserMessage);
                if (messageResponse.SendReceiveStatus == MessageStatus.Unsent && CanSendReceive() == true)
                {
                    sendEventArgs = _sendEventArgsPool.Pop();
                    if (sendEventArgs != null)
                    {
                        messageResponse.SetStatusInProgress();
                        sendEventArgs.UserToken = messageResponse;
                        sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                        sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                        try
                        {
                            if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs)) ProcessSend(null, sendEventArgs);
                            return;
                        }
                        catch (Exception ex)
                        {
                            Log(ex);
                        }
                    }
                }
                Thread.Sleep(200);
            }
        }


        /// <summary>
        /// Used for system messages where the normal checks and balances do not apply.
        /// </summary>
        /// <param name="Message">Message to be sent</param>
        private void SendFastMessage(IMessage Message)
        {
            byte[] sendBytes = MessageEngine.GenerateSendBytes(Message, false);

            if (InternalConnectionStatus != ConnectionStatuses.Connected || !CurrentEndPoint.Socket.Connected) return;

            SocketAsyncEventArgs sendEventArgs = _sendEventArgsPool.Pop();
            if (sendEventArgs == null) return;

            sendEventArgs.UserToken = Message;
            sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
            Message.SetStatusInProgress();
            sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
            try
            {
                if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs)) ProcessSend(null, sendEventArgs);
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }




        /// <summary>
        /// Send a message to the server and wait for a response. 
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the message</param>
        /// <param name="TimeoutMilliseconds">Maximum number of milliseconds to wait for a response from the server</param>
        /// <param name="IsLongPolling">If the message is long polling on the server mark this as true and the message will be cancelled instantly when a disconnect occurs</param>
        /// <returns>Nullable array of bytes which was returned from the socket server</returns>
        public byte[] SendMessage(object[] Parameters, int TimeoutMilliseconds = 60000, bool IsLongPolling = false)
        {
            string msg;

            if (Parameters == null)
            {
                msg = $"{nameof(SendMessage)}() failed: Message parameters cannot be null.";
                ArgumentException ex8 = new ArgumentException(msg, nameof(Parameters));
                Log(ex8);
                throw ex8;
            }
            if (Parameters.Length == 0)
            {
                msg = $"{nameof(SendMessage)}() failed: At least 1 parameter is required.";
                ArgumentException ex7 = new ArgumentException(msg, nameof(Parameters));
                Log(ex7);
                throw ex7;
            }
            DateTime startTime = DateTime.UtcNow;
            DateTime maxWait = startTime.AddMilliseconds(TimeoutMilliseconds);
            while (ConnectionStatus != ConnectionStatuses.Connected && !StopClientPermanently)
            {
                if (StopClientPermanently)
                {
                    msg = $"{nameof(SendMessage)}() failed: The socket client is stopped or stopping.";
                    Exception ex6 = new Exception(msg);
                    Log(ex6);
                    throw ex6;
                }
                if (DateTime.UtcNow > maxWait)
                {
                    msg = $"{nameof(SendMessage)}() failed: Timeout ({TimeoutMilliseconds} ms).";
                    TimeoutException ex5 = new TimeoutException(msg);
                    Log(ex5);
                    throw ex5;
                }
                Thread.Sleep(100);
            }
            int remainingMilliseconds = TimeoutMilliseconds - Convert.ToInt32((DateTime.UtcNow - startTime).TotalMilliseconds);

            //  Create Message
            MessageV1 message = new MessageV1(Parameters, remainingMilliseconds, IsLongPolling);

            //  Add the message to the unresponded messages collection
            UnrespondedMessages.Add(message);

            // Generate the sendBytes
            byte[] sendBytes = MessageEngine.GenerateSendBytes(message, _enableCompression);
            Log($"Parent sending {typeof(MessageV1).Name} ({sendBytes.Length} bytes)...", Severity.Information, LogEventType.UserMessage, message.MessageId);

            try
            {
                while (message.TrySendReceive && !StopClientPermanently)
                {
                    try
                    {
                        if (message.SendReceiveStatus == MessageStatus.Unsent && CanSendReceive())
                        {
                            var sendEventArgs = _sendEventArgsPool.Pop();
                            if (sendEventArgs != null)
                            {
                                message.SetStatusInProgress();
                                sendEventArgs.UserToken = message;
                                sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                                sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;

                                if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs))
                                    ProcessSend(null, sendEventArgs);

#if NET35
                                // Exit if the message has timed out
                                if (message.SendReceiveStatus == MessageStatus.Completed) break;
#else
                                // Wait for a response.
                                message.WaitForResponseOrTimeout();
                                break;
#endif
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        msg = $"{nameof(SendMessage)}() failed: Timeout ({message.TimeoutMilliseconds} ms).";
                        TimeoutException ex4 = new TimeoutException(msg);
                        Log(ex4);
                        throw ex4;
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                        throw;
                    }
#if NET35
                    Thread.Sleep(10); // Poll for response
#endif
                }
            }
            catch (Exception ex)
            {
                msg = $"{nameof(SendMessage)}() failed: {ex.ToString()}";
                Exception ex3 = new Exception(msg);
                Log(ex3);
                throw ex3;
            }
            finally
            {
                // Clean up message and associated resources
                UnrespondedMessages.Remove(message);
            }

            if (message.Response != null)
            {
                if (message.Response.Error != null)
                {
                    msg = $"{nameof(SendMessage)}() failed: {message.Response.Error.ToString()}";
                    Exception ex2 = new Exception(msg);
                    Log(ex2);
                    throw ex2;
                }

                // Success. Return the response data.
                return message.Response.ResponseData;
            }

            if (message.Error != null)
            {
                msg = $"{nameof(SendMessage)}() failed: {message.Error.ToString()}";
                Exception ex1 = new Exception(msg);
                Log(ex1);
                throw ex1;
            }

            msg = $"{nameof(SendMessage)}() failed: There was no message response";
            Exception exception = new Exception(msg);
            Log(exception);
            throw exception;
        }




        private void ProcessSendPollRequest(object sender, SocketAsyncEventArgs e)
        {
        }

        private void ProcessSendSubscriptionChanges(object sender, SocketAsyncEventArgs e)
        {
        }


        // called after SendAsync completes
        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            IMessage message = (IMessage)e.UserToken;
            SocketError result = e.SocketError;
            RecycleSocketAsyncEventArgs(e);

            try
            {
                if (result == SocketError.ConnectionReset)
                {
                    message.SetStatusUnsent();
                    Log(new Exception("Disconnecting: Connection was reset."));
                    Disconnect(SocketHasErrored: true, ClientDisconnectReason.SocketError, "");
                }
                else if (result != SocketError.Success)
                {
                    message.SetStatusUnsent();
                    Log(new Exception("Disconnecting: Send did not generate a success. Socket operation returned error code " + (int)e.SocketError));
                    Disconnect(SocketHasErrored: true, ClientDisconnectReason.SocketError, "");
                }
            }
            catch (Exception ex)
            {
                message.SetStatusUnsent();
                Log(ex);
            }
        }

        private void RecycleSocketAsyncEventArgs(SocketAsyncEventArgs e)
        {
            // destroy the SocketAsyncEventArgs user token to minimize chance of memory leak
            e.UserToken = null;
            // free the SocketAsyncEventArgs so it can be reused.
            e.SetBuffer(new byte[2], 0, 2);
            _sendEventArgsPool.Push(e);
        }



        /// <summary>
        /// Processes received data from the socket. It handles partial messages, full messages, and multiple messages.
        /// </summary>
        /// <param name="sender">Sending Socket</param>
        /// <param name="e">Socket Arguments</param>
        private void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            if (InternalConnectionStatus == ConnectionStatuses.Disconnected) return;

            if (e.BytesTransferred == 0)
            {
                // a graceful close scenario:
                // 1. the client shuts down send operations while continuing to receive any remaining data.
                // 2. the server signals the end of data by eventually transmitting 0 bytes.
                Disconnect(SocketHasErrored: true, ClientDisconnectReason.SocketError, "");
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                Log(new Exception("Disconnecting: ProcessReceive encountered socket error " + (int)e.SocketError));
                Disconnect(SocketHasErrored: true, ClientDisconnectReason.SocketError, "");
                return;
            }

            try
            {
                int socketReceiveBufferPtr = 0;
                while (socketReceiveBufferPtr < e.BytesTransferred && CanSendReceive())
                {
                    bool haveEntireMessage = _receiveEngine.AddBytesFromSocketReceiveBuffer(e.BytesTransferred, e.Buffer, ref socketReceiveBufferPtr);
                    if (haveEntireMessage == true)
                    {
                        LastMessageFromServer = DateTime.UtcNow;

                        if (_receiveEngine.MessageType == MessageType.MessageResponseV1)
                        {
                            //  Attempt to find the message this response belongs to and attach the response to the message.
                            MessageResponseV1 response = _receiveEngine.GetMessageResponseV1();
                            Log($"Received {_receiveEngine.MessageType.ToString()} ({e.BytesTransferred} bytes)", Severity.Information, LogEventType.UserMessage, response.MessageId);
                            UnrespondedMessages.FindMessageAndSetResponse(response); ;
                        }

                        else if (_receiveEngine.MessageType == MessageType.MessageV1)
                        {
                            MessageV1 msg = _receiveEngine.GetMessageV1();
                            Log($"Received user message {msg.MessageId} from server ({e.BytesTransferred} bytes)", Severity.Information, LogEventType.UserMessage, msg.MessageId);
                            ProcessMessageV1BackgroundLauncher(msg);
                        }

                        else if (_receiveEngine.MessageType == MessageType.ServerStoppingNotificationV1)
                        {
                            Log($"Received server stopping notification ", Severity.Warning, LogEventType.UserMessage);
                            NotifyServerStopping();
                            Disconnect(SocketHasErrored: false, ClientDisconnectReason.ServerIsStopping, "Closing because server has notified it is stopping");
                        }
                        else if (_receiveEngine.MessageType == MessageType.PollingResponseV1)
                        {
                            Log($"Received polling response ({e.BytesTransferred} bytes)", Severity.Information, LogEventType.PollingEvent, 0);
                            LastPollResponse = DateTime.UtcNow;
                        }

                        else if (_receiveEngine.MessageType == MessageType.Handshake1)
                        {
                            // Note: Logging and validation is performed in BgCompleteHandshake()
                            Handshake1 hs1 = _receiveEngine.GetHandshake1();
                            ClientId = hs1.ClientId;
                            ServerSocketMeisterVersion = hs1.ServerSocketMeisterVersion;
                            Handshake1Received = true;
                        }

                        else if (_receiveEngine.MessageType == MessageType.Handshake2Ack)
                        {
                            // Note: Logging and validation is performed in BgCompleteHandshake()
                            Handshake2Ack hs1 = _receiveEngine.GetHandshake2Ack();
                            ServerSupportsThisClientVersion = hs1.ServerSupportsClientVersion;
                            HandshakeCompleted = true;   // Sets all handshake flags to true
                        }

                        else if (_receiveEngine.MessageType == MessageType.TokenChangesResponseV1)
                        {
                            TokenChangesResponseV1 tcr1 = _receiveEngine.GetSubscriptionChangesResponseV1();
                            Log($"Received subscription changes response ({e.BytesTransferred} bytes)", Severity.Information, LogEventType.SubscriptionSyncEvent, 0);
                            foreach (TokenChangesResponseV1.ChangeIdentifier i in tcr1.ChangeIdentifiers)
                            {
                                _subscriptions.RemoveChange(i.TokenNameUppercase, i.ChangeId);
                            }
                        }

                        else if (_receiveEngine.MessageType == MessageType.BroadcastV1)
                        {
                            BroadcastV1 bc = _receiveEngine.GetBroadcastV1();
                            Log($"Received user broadcast ({e.BytesTransferred} bytes)", Severity.Information, LogEventType.UserMessage, 0);
                            NotifyBroadcastReceived(bc);
                        }

                        else
                        {
                            //  An unknown message was received. The server may be a more recent version of SocketMeister.
                            //  Raise an exception and ignore the message. The server will have to factor this into consideration.
                            Log(new Exception("Unknown message type received (" + _receiveEngine.MessageType.ToString() + "). The server may be running a newer version of SocketMeister"));
                        }
                    }
                }

                //  KEEP RECEIVING
                if (InternalConnectionStatus == ConnectionStatuses.Disconnecting && !CurrentEndPoint.Socket.ReceiveAsync(e)) ProcessReceive(null, e);
                else if (StopClientPermanently == false && InternalConnectionStatus == ConnectionStatuses.Connected && !CurrentEndPoint.Socket.ReceiveAsync(e)) ProcessReceive(null, e);
            }
            catch (ObjectDisposedException ee)
            {
                //  IF A LARGE CHUNK OF DATA WAS BEING RECEIVED WHEN THE CONNECTION WAS LOST, THE Disconnect() ROUTINE
                //  MAY ALREADY HAVE BEEN RUN (WHICH DISPOSES OBJECTS). IF THIS IS THE CASE, SIMPLY EXIT
                string em1 = "Disconnecting: ObjectDisposedException running ProcessReceive: " + ee.ToString();
                Log(new Exception(em1));
                Disconnect(SocketHasErrored: true, ClientDisconnectReason.SocketError, em1);
            }
            catch (Exception ex)
            {
                string em2 = "Disconnecting: Exception running ProcessReceive: " + ex.ToString();
                Log(new Exception(em2));
                Disconnect(SocketHasErrored: true, ClientDisconnectReason.SocketError, em2);
            }
        }

#if !NET35
#pragma warning disable CS1998  //  The await is intentionally delayed so the task can be added to _background tasks first, so ignore the warming from Visual Studio
        private async void ProcessMessageV1BackgroundLauncher(object state)
        {
            var message = (MessageV1)state;
            var cancellationToken = CancellationToken.Token;
            var task = Task.Run(async () =>
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                ProcessMessageV1(message);
            }, cancellationToken);

            // Add the task to the dictionary and remove it upon completion
            _backgroundTasks.TryAdd(task, true);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log($"Error processing message: {ex}", Severity.Error, LogEventType.Exception);
            }
            finally
            {
                _backgroundTasks.TryRemove(task, out _);
            }
        }
#pragma warning restore CS1998
#else
        private void ProcessMessageV1BackgroundLauncher(object state)
        {
            var message = (MessageV1)state;
            new Thread(new ThreadStart(delegate
            {
                ProcessMessageV1(message);
            }))
            { IsBackground = true }.Start();
        }
#endif

        private void ProcessMessageV1(MessageV1 message)
        {
            try
            {
                if (MessageReceived == null)
                {
                    //  The parent program using this SocketClient is ignoring MessageReceived events so incoming messages are not being processed.
                    Log("Respond to server that the MessageReceived event is not being handled by the parent program. The use message was ignored.", Severity.Warning, LogEventType.UserMessage, message.MessageId);
                    SendFastMessage(new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.NoMessageReceivedEventListener));
                    Log(new Exception("MessageReceived event is not being handled by the parent program."), message.MessageId);
                }
                else if (StopClientPermanently)
                {
                    //  This SocketClient is shuting down. A shutdown message should have already been sent
                    //  but we'll also send a quick response to the sender for this particular message.
                    Log("Client is stopping. Send MessageResponseV1 without response data from user program.", Severity.Warning, LogEventType.UserMessage, message.MessageId);
                    SendFastMessage(new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.Stopping));
                }
                else
                {
                    //  Deserialize the message and raise a MessageReceived event for the parent program to process.
                    MessageReceivedEventArgs args = new MessageReceivedEventArgs(message.Parameters, message.MessageId, DateTime.UtcNow.AddMilliseconds(message.TimeoutMilliseconds));
                    MessageReceived(this, args);

                    //  There could be significant delay waiting for the calling program to process the MessageReceived event.
                    //  Respond to the sender, including optional response data (args.Response) the calling program provided.
                    MessageResponseV1 response = new MessageResponseV1(message.MessageId, args.Response);
                    SendResponse(response, message);
                }
            }
            catch (Exception ex)
            {
                Log(ex);
                SendFastMessage(new MessageResponseV1(message.MessageId, ex));
            }
        }



        private bool CanSendReceive()
        {
            return !StopClientPermanently && InternalConnectionStatus == ConnectionStatuses.Connected && CurrentEndPoint.Socket.Connected;
        }




        private void NotifyBroadcastReceived(Messages.BroadcastV1 Message)
        {
            if (BroadcastReceived == null) return;
#if NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    BroadcastReceived(this, new BroadcastReceivedEventArgs(Message.Name, Message.Parameters));
                }
                catch (Exception ex)
                {
                    Log(ex);
                }
            }))
            { IsBackground = true }.Start();
#else
            Task.Run(() =>
            {
                try
                {
                    BroadcastReceived(this, new BroadcastReceivedEventArgs(Message.Name, Message.Parameters));
                }
                catch (Exception ex)
                {
                    Log(ex);
                }
            });
#endif
        }


        private void NotifyServerStopping()
        {
            if (ServerStopping == null) return;

#if NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    ServerStopping(this, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {nameof(NotifyServerStopping)}: {ex}");
                }
            }))
            { IsBackground = true }.Start();
#else
            Task.Run((Action)(() =>
            {
                try
                {
                    ServerStopping(this, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {nameof(NotifyServerStopping)}: {ex}");
                }
            }));
#endif
        }

        private void Log(string message, Severity severity, LogEventType eventType, long messageId = 0)
        {
            LogEntry log = new LogEntry(message, severity, eventType, messageId);
            _logger.Log(log);
        }

        private void Log(Exception ex, long messageId = 0)
        {
            LogEntry log = new LogEntry(ex,messageId);
            _logger.Log(log);
        }
    }
}

