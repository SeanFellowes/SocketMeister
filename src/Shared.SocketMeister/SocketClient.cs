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
    /// Asynchronous, persistent TCP/IP socket client supporting multiple destinations
    /// </summary>
#if SMISPUBLIC
    public partial class SocketClient : IDisposable
#else
    internal partial class SocketClient : IDisposable
#endif
    {
        /// <summary>
        /// If a poll response has not been received from the server after a number of seconds, the socket client will be disconnected.
        /// </summary>
        private const int DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS = 300;

        /// <summary>
        /// When a shutdown occurs, particularly because of network failure or server shutdown, delay attempting to reconnect to that server, giving the server some time to complete it's shutdown process.
        /// When a shutdown occurs, particularly because of network failure or serveDisconnectr shutdown, delay attempting to reconnect to that server, giving the server some time to complete it's shutdown process.
        /// </summary>
        private const int RECONNECT_DELAY_AFTER_SERVER_SHUTDOWN_AND_ONE_ENDPOINT = 30;

        /// <summary>
        /// The frequency, in seconds, that this client will poll the server, to ensure the socket is alive.
        /// </summary>
        private const int POLLING_FREQUENCY = 15;


#if NET35
        // ThreadStatic for .NET 3.5 to give each thread its own Random instance
        [ThreadStatic]
        private static Random _threadStaticRandom;
#else
        // ThreadLocal for .NET 4.0+
        private static ThreadLocal<Random> _threadLocalRandom = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
        private object _cancellationTokenSourceLock = new object();
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<Task, bool> _backgroundTasks = new ConcurrentDictionary<Task, bool>();
#endif
        private SocketAsyncEventArgs _asyncEventArgsConnect;
        private SocketAsyncEventArgs _asyncEventArgsPolling;
        private SocketAsyncEventArgs _asyncEventArgsReceive;
        private SocketAsyncEventArgs _asyncEventArgsSendSubscriptionChanges;
        private string _clientId = "";
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

        private readonly byte[] _pollingBuffer = new byte[Constants.SEND_RECEIVE_BUFFER_SIZE];
        private readonly Random _randomizer = new Random();
        private readonly MessageEngine _receiveEngine;
        private readonly SocketAsyncEventArgsPool _sendEventArgsPool;
        private int _serverSocketMeisterVersion;
        private bool _stopClientPermanently;
        private readonly object _stopClientPermanentlyLock = new object();
        private readonly TokenCollection _subscriptions = new TokenCollection();
        private bool _subscriptionsSendTrigger;
        private readonly UnrespondedMessageCollection _unrespondedMessages = new UnrespondedMessageCollection();

        /// <summary>
        /// Event raised when the status of a socket connection changes
        /// </summary>
        public event EventHandler<EventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Event raised when the current EndPoint channges
        /// </summary>
        public event EventHandler<EventArgs> CurrentEndPointChanged;

        /// <summary>
        /// Event raised when an exception occurs
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;

        /// <summary>
        /// Raised when a  message is received from the server. When processing this event, an optional response can be provided which will be returned to the server.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Event raised when a the server stops
        /// </summary>
        public event EventHandler<EventArgs> ServerStopping;

        /// <summary>
        /// Event raised whenever a broadcast is received from the server.
        /// </summary>
        public event EventHandler<BroadcastReceivedEventArgs> BroadcastReceived;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        public event EventHandler<TraceEventArgs> TraceEventRaised;



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="EndPoints">Collection of endpoints that are available to connect to</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        /// <param name="friendlyName">Friendly name is sent to the SocketServer to be displayed in errors and logging.</param>
        public SocketClient(List<SocketEndPoint> EndPoints, bool EnableCompression, string friendlyName)
        {
            if (EndPoints == null) throw new ArgumentNullException(nameof(EndPoints));
            else if (EndPoints.Count == 0) throw new ArgumentException("EndPoints must contain at least 1 value", nameof(EndPoints));
            _endPoints = EndPoints;

            _enableCompression = EnableCompression;
            if (!string.IsNullOrEmpty(friendlyName)) _friendlyName = friendlyName;
            _receiveEngine = new MessageEngine(EnableCompression);

            _subscriptions.TokenAdded += Subscriptions_AddChangedDeleted;
            _subscriptions.TokenChanged += Subscriptions_AddChangedDeleted;
            _subscriptions.TokenDeleted += Subscriptions_AddChangedDeleted;

            //  STATIC BUFFERS
            _pollingBuffer = MessageEngine.GenerateSendBytes(new PollingRequestV1(), false);

            //  SETUP ENDPOINTS AND CHOOSE THE ENDPOINT TO START WITH
            if (_endPoints.Count == 1)
            {
                _currentEndPoint = _endPoints[0];   // Safe to use direct access in class constructor
            }
            else
            {
                _currentEndPoint = _endPoints[GetThreadSafeRandomNumber(0, _endPoints.Count)];   // Safe to use direct access in class constructor
            }
            _currentEndPoint.DontReconnectUntil = DateTime.UtcNow.AddYears(-1);   // Safe to use direct access in class constructor

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
        /// <param name="FriendlyName">Friendly name is sent to the SocketServer to be displayed in errors and logging.</param>
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
        /// <param name="FriendlyName">Friendly name is sent to the SocketServer to be displayed in errors and logging.</param>
        public SocketClient(string IPAddress1, int Port1, string IPAddress2, int Port2, bool EnableCompression, string FriendlyName)
            : this(
                new List<SocketEndPoint>
                {
                    new SocketEndPoint(IPAddress1, Port1),
                    new SocketEndPoint(IPAddress2, Port2)
                },
                EnableCompression, FriendlyName)
        {
        }


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
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();

                    _connectionStatusLock.Dispose();
                    _currentEndPointLock.Dispose();

                    _asyncEventArgsConnect?.Dispose();
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



        private void Subscriptions_AddChangedDeleted(object sender, EventArgs e)
        {
            SubscriptionsSendTrigger = true;
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
        /// A friendly name for the client which can be. If available, the SocketServer will use this in logging and error handling
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

        private bool IsBackgroundWorkerRunning { get { lock (_lock) { return _isBackgroundWorkerRunning; } } set { lock (_lock) { _isBackgroundWorkerRunning = value; } } }

        /// <summary>
        /// The last time a polling response was received from the socket server.
        /// </summary>
        private DateTime LastPollResponse { get { lock (_lock) { return _lastPollResponse; } } set { lock (_lock) { _lastPollResponse = value; } } }

        private bool HandshakeCompleted { get { lock (_handshakeLock) { return _handshakeCompleted; } } set { lock (_handshakeLock) { _handshakeCompleted = value; } } }

        private bool Handshake1Received { get { lock (_handshakeLock) { return _handshake1Received; } } set { lock (_handshakeLock) { _handshake1Received = value; } } }

        private bool Handshake2AckReceived { get { lock (_handshakeLock) { return _handshake2AckReceived; } } set { lock (_handshakeLock) { _handshake2AckReceived = value; } } }

        private int  ServerSocketMeisterVersion { get { lock (_handshakeLock) { return _serverSocketMeisterVersion; } } set { lock (_handshakeLock) { _serverSocketMeisterVersion = value; } } }



        private bool StopClientPermanently { get { lock (_stopClientPermanentlyLock) { return _stopClientPermanently; } } set { lock (_stopClientPermanentlyLock) { _stopClientPermanently = value; } } }

        /// <summary>
        /// The number of subscriptions for this client
        /// </summary>
        public int SubscriptionCount => _subscriptions.Count;

        private bool SubscriptionsSendTrigger { get { lock (_lock) { return _subscriptionsSendTrigger; } } set { lock (_lock) { _subscriptionsSendTrigger = value; } } }

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

        private void Disconnect(bool SocketHasErrored)
        {
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                if (InternalConnectionStatus == ConnectionStatuses.Disconnecting || InternalConnectionStatus == ConnectionStatuses.Disconnected) return;
                lock (_handshakeLock)
                {
                    _handshakeCompleted = false;
                    _handshake1Received = false;
                    _handshake2AckReceived = false;
                }
                if (SocketHasErrored == false)
                {
                    //  Attempt to send a disconnecting message  to the server
                    try
                    {
                        byte[] sendBytes = MessageEngine.GenerateSendBytes(new ClientDisconnectingNotificationV1(), false);
                        CurrentEndPoint.Socket.Send(sendBytes);
                        SendFastMessage(new ClientDisconnectingNotificationV1());
                    }
                    catch (Exception ex)
                    {
                        NotifyTraceEventRaised(ex);
                    }
                }

                //  Initiate Shutdown
                InternalConnectionStatus = ConnectionStatuses.Disconnecting;

                //  Stop raising events when messages are received
                try
                {
                    if (_asyncEventArgsReceive != null)
                    {
                        _asyncEventArgsReceive.Completed -= new EventHandler<SocketAsyncEventArgs>(ProcessReceive);
                        _asyncEventArgsReceive.Dispose();
                        _asyncEventArgsReceive = null;
                    }
                }
                catch { }


#if !NET35
                // Wait for calling program to progress inbound messages, with a timeout
                CancellationToken.Cancel();  // Signal cancellation
                try
                {
                    Task allTasks = Task.WhenAll(_backgroundTasks.Keys);
                    if (!allTasks.Wait(TimeSpan.FromSeconds(20)))
                    {
                        NotifyTraceEventRaised("Timeout occurred while waiting for background tasks to complete.", SeverityType.Error);
                    }
                }
                catch (AggregateException ex)
                {
                    // Handle any task exceptions here
                    foreach (var innerException in ex.InnerExceptions)
                    {
                        NotifyTraceEventRaised(innerException);
                    }
                }
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
                    catch { }
                }

                //  try another disconnect. It may already be disconnected, so the error will be ignored
                try
                {
                    CurrentEndPoint.Socket.Disconnect(false);
                }
                catch { }

                InternalConnectionStatus = ConnectionStatuses.Disconnected;

                //  Prepare new socket and start reconnect process
                if (StopClientPermanently == false)
                {
#if !NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
                    try { CurrentEndPoint.Socket.Dispose(); }
                    catch { }
#endif
                    CurrentEndPoint.RecreateSocket();
                }
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex);
            }
            finally
            {
                timer.Stop();
                NotifyTraceEventRaised($"{nameof(Disconnect)}() took " + timer.ElapsedMilliseconds + " milliseconds to execute.", SeverityType.Information);
            }
        }


        private void StartBackgroundWorker()
        {
            Thread bgWorker = new Thread(new ThreadStart(delegate
            {
                try
                {
                    IsBackgroundWorkerRunning = true;
                    Stopwatch pollingTimer = Stopwatch.StartNew();
                    Stopwatch sendSubscriptionsTimer = Stopwatch.StartNew();
                    LastPollResponse = DateTime.UtcNow;

                    //  FLAG ALL SUBSCRIPTIONS (Tokens) FOR SENDING TO THE SERVER
                    _subscriptions.FlagAllAfterSocketConnect();

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
                                BgSendSubscriptionChanges(sendSubscriptionsTimer);
                                break;
                        }
                        Thread.Sleep(200);
                    }
                }
                catch (Exception e) { NotifyTraceEventRaised(e); }
                finally
                {
                    IsBackgroundWorkerRunning = false;
                    NotifyTraceEventRaised("SocketClient Background Worker Exited", SeverityType.Information);
                }
            }));
            bgWorker.IsBackground = true;
            bgWorker.Start();
        }



        private void BgPerformAttemptConnect()
        {
            try
            {
                //  This provides visual asthetics that connect operations are in progress
                if (DateTime.UtcNow < CurrentEndPoint.DontReconnectUntil.AddMilliseconds(-1500)) InternalConnectionStatus = ConnectionStatuses.Connecting;

                //  Exit is it's not time to actually attempt to reconnect
                if (DateTime.UtcNow < CurrentEndPoint.DontReconnectUntil) return;

                //  CHOOSE THE NEXT ENDPOINT TO TRY
                if (_endPoints.Count > 1)
                {
                    SocketEndPoint bestEP = _endPoints[0];
                    for (int i = 1; i < _endPoints.Count; i++)
                    {
                        if (_endPoints[i].DontReconnectUntil < bestEP.DontReconnectUntil) bestEP = _endPoints[i];
                    }
                    CurrentEndPoint = bestEP;
                    CurrentEndPointChanged?.Invoke(this, new EventArgs());
                }

                //  TRY TO CONNECT
                _asyncEventArgsConnect.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                if (!CurrentEndPoint.Socket.ConnectAsync(_asyncEventArgsConnect)) ProcessConnect(null, _asyncEventArgsConnect);
                if (StopClientPermanently == true) return;

                if (InternalConnectionStatus != ConnectionStatuses.Connected)
                {
                    //  IMPORTANT!!! DON'T TRY THIS CONNECTION FOR AT LEAST 3 SECONDS
                    CurrentEndPoint.DontReconnectUntil = DateTime.UtcNow.AddMilliseconds(3000);
                }
                else
                {
                }
            }
            catch { }
        }


        private void BgPerformPolling(Stopwatch PollingTimer)
        {
            try
            {
                if (PollingTimer.Elapsed.TotalSeconds < POLLING_FREQUENCY) return;

                if (LastPollResponse < (DateTime.UtcNow.AddSeconds(-DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS)))
                {
                    RestartStopwatch(PollingTimer);
                    NotifyTraceEventRaised(new Exception("Disconnecting: Server failed to reply to polling after " + DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS + " seconds."));
                    Disconnect(SocketHasErrored: true);
                    return;
                }
                RestartStopwatch(PollingTimer);
                _asyncEventArgsPolling.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                _asyncEventArgsPolling.SetBuffer(_pollingBuffer, 0, _pollingBuffer.Length);
                if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsPolling)) ProcessSendPollRequest(null, _asyncEventArgsPolling);
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex);
            }
        }


        private void BgSendSubscriptionChanges(Stopwatch sendSubscriptionsTimer)
        {
            try
            {
                //  SEND SUBSCRIPTION CHANGES
                if (SubscriptionsSendTrigger == true)
                {
                    RestartStopwatch(sendSubscriptionsTimer);
                    SubscriptionsSendTrigger = false;
                    byte[] changesBytes = _subscriptions.GetChangeBytes();
                    if (changesBytes != null)
                    {
                        byte[] sendBytes = MessageEngine.GenerateSendBytes(new TokenChangesRequestV1(changesBytes), false);
                        _asyncEventArgsSendSubscriptionChanges.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                        _asyncEventArgsSendSubscriptionChanges.SetBuffer(sendBytes, 0, sendBytes.Length);
                        if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsSendSubscriptionChanges)) ProcessSendSubscriptionChanges(null, _asyncEventArgsSendSubscriptionChanges);
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(ex);
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
                NotifyTraceEventRaised($"Caller error processing {nameof(RaiseConnectionStatusChanged)} event: {ex}", SeverityType.Error);
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

            double pauseReconnect = 2000;
            if (e.SocketError == SocketError.Success)
            {
                //  ATTEMPT TO START RECEIVING
                try
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

                    // Start a new thread to complete the handshake.
                    // Note: Server will send Handshake1 message.
                    Thread pollingThread = new Thread(new ThreadStart(BgCompleteHandshake));
                    pollingThread.IsBackground = true;
                    pollingThread.Start();

                    //  Internally, we are connected, but ConnectionStatus property must continue to show Connecting until handshake is completed.
                    InternalConnectionStatus = ConnectionStatuses.Connected;

                    //  IF SUBSCRIPTIONS EXIST, SEND THEM
                    if (SubscriptionCount > 0) SubscriptionsSendTrigger = true;
                }
                catch (Exception ex)
                {
                    NotifyTraceEventRaised(ex);
                }
            }
            else if (e.SocketError == SocketError.TimedOut)
            {
                //  NOTE: WHEN FAILING OVER UNDER HIGH LOAD, SocketError.TimedOut OCCURS FOR UP TO 120 SECONDS (WORSE CASE)
                //  BEFORE CONNECTION SUCCESSFULLY COMPLETES. IT'S A BIT ANNOYING BUT I HAVE FOUND NO WORK AROUND.
                NotifyTraceEventRaised(new Exception("Socket Timeout"));
            }
            else if (e.SocketError == SocketError.AddressAlreadyInUse)
            {
                NotifyTraceEventRaised(new Exception("Socket address already in use"));
            }
            else if (e.SocketError == SocketError.ConnectionRefused)
            {
                NotifyTraceEventRaised(new Exception($"Connection refused by server {CurrentEndPoint.Description}"));
            }

            else
            {
                NotifyTraceEventRaised(new Exception("Undefined Socket Error: " + e.SocketError.ToString()));
            }

            //  RESET
            try
            {
                CurrentEndPoint.DontReconnectUntil = DateTime.UtcNow.AddMilliseconds(pauseReconnect + GetThreadSafeRandomNumber(1, 4000));
            }
            catch (Exception ex)
            {
                if (!StopClientPermanently)
                {
                    NotifyTraceEventRaised(ex);
                }
            }
        }

        /// <summary>
        /// Once a connection has been established, wait for the socket server to send a SocketServerHandshake1 message, then
        /// complete handshake activities.
        /// </summary>
        private void BgCompleteHandshake()
        {
            NotifyTraceEventRaised($"Connected to {CurrentEndPoint.Description}. Initiating handshake...", SeverityType.Information);
            int timeoutSeconds = 30;
            DateTime timeout = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            while (DateTime.UtcNow < timeout && !Handshake1Received && !StopClientPermanently && InternalConnectionStatus == ConnectionStatuses.Connected)
            {
                Thread.Sleep(100);
            }
            if (StopClientPermanently || InternalConnectionStatus != ConnectionStatuses.Connected)
            {
                NotifyTraceEventRaised(new Exception("Connection reset before handshake completed."));
                return;
            }
            if (!Handshake1Received)
            {
                NotifyTraceEventRaised(new Exception($"Handshake1 could not be completed within {timeoutSeconds} seconds. Disconnecting."));
                Disconnect(SocketHasErrored: true);
                return;
            }

            //  SEND THE SERVER Handshake2 MESSAGE
            SendFastMessage(new Handshake2(Constants.SocketMeisterVersion, FriendlyName, _subscriptions.Serialize()));

            //  WAIT TO RECEIVE A Handshake2Ack MESSAGE FROM THE SERVER
            while (DateTime.UtcNow < timeout && !Handshake2AckReceived && !StopClientPermanently && InternalConnectionStatus == ConnectionStatuses.Connected)
            {
                Thread.Sleep(100);
            }

            if (StopClientPermanently || InternalConnectionStatus != ConnectionStatuses.Connected)
            {
                NotifyTraceEventRaised(new Exception("Connection reset before handshake completed."));
                return;
            }
            else if (HandshakeCompleted)
            {
                //  No need to change InternalConnectionStatus = Connected but me MUST raise an event so the calling
                //  software knows the connection is fully established.
                RaiseConnectionStatusChanged();
                NotifyTraceEventRaised("Handshake completed. Connection is fully established.", SeverityType.Information);
            }
            else
            {
                NotifyTraceEventRaised(new Exception($"Handshake2 could not be completed within {timeoutSeconds} seconds. Disconnecting."));
                Disconnect(SocketHasErrored: true);
            }
        }


        /// <summary>
        /// Stops the client permenently. There is no option from here to restart without creating a new instance of SocketClient.
        /// </summary>
        public void Stop()
        {
            if (StopClientPermanently == true) return;  //  Don't allow this to run more than once
            StopClientPermanently = true;

            _subscriptions.TokenAdded -= Subscriptions_AddChangedDeleted;
            _subscriptions.TokenChanged -= Subscriptions_AddChangedDeleted;
            _subscriptions.TokenDeleted -= Subscriptions_AddChangedDeleted;

            Disconnect(SocketHasErrored: false);

#if !NET35
            CancellationToken.Dispose();
#endif

        }


        private void SendResponse(MessageResponseV1 messageResponse, MessageV1 message)
        {
            byte[] sendBytes = MessageEngine.GenerateSendBytes(messageResponse, false);

            SocketAsyncEventArgs sendEventArgs;
            while (true)
            {
                if (message.IsTimeout)
                {
                    //  TIMEOUT: NO POINT SENDING THE RESPONSE BECAUSE IT WILL HAVE ALSO TIMED OUT AT THESERVER
                    NotifyTraceEventRaised(new TimeoutException("Message " + message.MessageId + ", timed out at the server after " + message.TimeoutMilliseconds + " milliseconds."));
                    return;
                }
                else if (StopClientPermanently)
                {
                    //  SHUTDOWN: CLIENT IS SHUTTING DOWN. A SHUTDOWN MESSAGE SHOULD HAVE ALREADY BEEN SENT SO EXIT
                    SendFastMessage(new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.Stopping));
                    return;
                }

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
                            NotifyTraceEventRaised(ex);
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
                NotifyTraceEventRaised(ex);
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
            if (StopClientPermanently) throw new Exception("Message cannot be sent. The socket client is stopped or stopping");
            if (Parameters == null) throw new ArgumentException("Message parameters cannot be null.", nameof(Parameters));
            if (Parameters.Length == 0) throw new ArgumentException("At least 1 parameter is required because the message makes no sense.", nameof(Parameters));
            DateTime startTime = DateTime.UtcNow;
            DateTime maxWait = startTime.AddMilliseconds(TimeoutMilliseconds);
            while (ConnectionStatus != ConnectionStatuses.Connected && StopClientPermanently == false)
            {
                Thread.Sleep(200);
                if (StopClientPermanently) throw new Exception("Message cannot be sent. The socket client is stopped or stopping");
                if (DateTime.UtcNow > maxWait) throw new TimeoutException();
            }
            int remainingMilliseconds = TimeoutMilliseconds - Convert.ToInt32((DateTime.UtcNow - startTime).TotalMilliseconds);
            return SendReceive(new MessageV1(Parameters, remainingMilliseconds, IsLongPolling));
        }


        private byte[] SendReceive(MessageV1 message)
        {
            if (StopClientPermanently) return null;

            DateTime startTime = DateTime.UtcNow;
            UnrespondedMessages.Add(message);

            // Generate the sendBytes once outside the loop
            byte[] sendBytes = MessageEngine.GenerateSendBytes(message, false);

            // Improved implementation for .NET 4.0+
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
                        throw;
                    }
                    catch (Exception ex)
                    {
                        NotifyTraceEventRaised(ex);
                    }

#if NET35
                Thread.Sleep(10); // Poll for response
#endif
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                // Clean up message and associated resources
                UnrespondedMessages.Remove(message);
            }

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



        private void ProcessSendPollRequest(object sender, SocketAsyncEventArgs e)
        {
        }

        private void ProcessSendSubscriptionChanges(object sender, SocketAsyncEventArgs e)
        {
        }


        //  CALLED AFTER SendAsync COMPLETES
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
                    NotifyTraceEventRaised(new Exception("Disconnecting: Connection was reset."));
                    Disconnect(SocketHasErrored: true);
                }
                else if (result != SocketError.Success)
                {
                    message.SetStatusUnsent();
                    NotifyTraceEventRaised(new Exception("Disconnecting: Send did not generate a success. Socket operation returned error code " + (int)e.SocketError));
                    Disconnect(SocketHasErrored: true);
                }
            }
            catch (Exception ex)
            {
                message.SetStatusUnsent();
                NotifyTraceEventRaised(ex);
            }
        }

        private void RecycleSocketAsyncEventArgs(SocketAsyncEventArgs e)
        {
            //  DESTROY THE SocketAsyncEventArgs USER TOKEN TO MINIMISE CHANCE OF MEMORY LEAK
            e.UserToken = null;
            //  FREE THE SocketAsyncEventArg SO IT CAN BE REUSED.
            e.SetBuffer(new byte[2], 0, 2);
            _sendEventArgsPool.Push(e);
        }



        /// <summary>
        /// A block of data has been received through the socket. It may contain part of a message, a message, or multiple messages. Process the incoming bytes and when a full message has been received, process the complete message.
        /// </summary>
        /// <param name="sender">Sending Socket</param>
        /// <param name="e">Socket Arguments</param>
        private void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            if (InternalConnectionStatus == ConnectionStatuses.Disconnected) return;

            if (e.BytesTransferred == 0)
            {
                //  A GRACEFUL CLOSE SHOULD OCCUR LIKE THIS (* IS THIS STEP/S)
                //  1.   The client socket calls Shutdown(SocketShutdown.Send)) but should keep receiving
                //  2.   On the server, EndReceive returns 0 bytes read(the client signals there is no more data from its side)
                //  3.   The server A) sends its last data B) calls Shutdown(SocketShutdown.Send)) C) calls Close on the socket, optionally with a timeout to allow the data to be read from the client
                //  4. * The client A) reads the remaining data from the server and then receives 0 bytes(the server signals there is no more data from its side) B) calls Close on the socket

                Disconnect(SocketHasErrored: true);
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                NotifyTraceEventRaised(new Exception("Disconnecting: ProcessReceive received socket error code " + (int)e.SocketError));
                Disconnect(SocketHasErrored: true);
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
                            UnrespondedMessages.FindMessageAndSetResponse(_receiveEngine.GetMessageResponseV1()); ;
                        }

                        else if (_receiveEngine.MessageType == MessageType.MessageV1)
                        {
                            ProcessMessageV1BackgroundLauncher(_receiveEngine.GetMessageV1());
                        }

                        else if (_receiveEngine.MessageType == MessageType.ServerStoppingNotificationV1)
                        {
                            //  Server has notified that it is stopping. 
                            //  Determine the length of time to wait before attempting reconnection
                            if (_endPoints.Count > 1)
                                CurrentEndPoint.DontReconnectUntil = DateTime.UtcNow;
                            else
                                CurrentEndPoint.DontReconnectUntil = DateTime.UtcNow.AddSeconds(RECONNECT_DELAY_AFTER_SERVER_SHUTDOWN_AND_ONE_ENDPOINT);
                            //  Clean up gracefully
                            NotifyServerStopping();
                            Disconnect(SocketHasErrored: false);
                        }
                        else if (_receiveEngine.MessageType == MessageType.PollingResponseV1)
                        {
                            LastPollResponse = DateTime.UtcNow;
                            HandshakeCompleted = true;
                        }

                        else if (_receiveEngine.MessageType == MessageType.Handshake1)
                        {
                            Handshake1Received = true;
                            Handshake1 hs1 = _receiveEngine.GetHandshake1();
                            ServerSocketMeisterVersion = hs1.SocketServerVersion;
                            string msg = $"Handshake1 received from server. Server version {hs1.SocketServerVersion}";
                            if (hs1.SocketServerVersion < Constants.SocketMeisterVersion)
                            {
                                msg += $" is older than this client version ({Constants.SocketMeisterVersion}).";
                            }
                            else if(hs1.SocketServerVersion > Constants.SocketMeisterVersion)
                                msg += $" is newer than this client version ({Constants.SocketMeisterVersion}).";
                            else
                            {
                                msg += " matches the client version.";
                            }
                            NotifyTraceEventRaised(msg, SeverityType.Information);
                            ClientId = hs1.ClientId;
                        }

                        else if (_receiveEngine.MessageType == MessageType.Handshake2Ack)
                        {
                            HandshakeCompleted = true;
                            Handshake2AckReceived = true;
                        }

                        else if (_receiveEngine.MessageType == MessageType.SubscriptionChangesResponseV1)
                        {
                            _subscriptions.ImportTokenChangesResponseV1(_receiveEngine.GetSubscriptionChangesResponseV1());
                            SubscriptionsSendTrigger = true;
                        }

                        else if (_receiveEngine.MessageType == MessageType.BroadcastV1)
                        {
                            NotifyBroadcastReceived(_receiveEngine.GetBroadcastV1());
                        }

                        else
                        {
                            //  An unknown message was received. The server may be a more recent version of SocketMeister.
                            //  Raise an exception and ignore the message. The server will have to factor this into consideration.
                            NotifyTraceEventRaised(new Exception("Unknown message type received (" + _receiveEngine.MessageType.ToString() + "). The server may be running a newer version of SocketMeister"));
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
                NotifyTraceEventRaised(new Exception("Disconnecting: ObjectDisposedException running ProcessReceive: " + ee.Message));
                Disconnect(SocketHasErrored: true);
            }
            catch (Exception ex)
            {
                NotifyTraceEventRaised(new Exception("Disconnecting: Error running ProcessReceive: " + ex.Message));
                Disconnect(SocketHasErrored: true);
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
                NotifyTraceEventRaised($"Error processing message: {ex}", SeverityType.Error);
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
                    SendFastMessage(new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.NoMessageReceivedEventListener));
                }
                else if (StopClientPermanently)
                {
                    //  This SocketClient is shuting down. A shutdown message should have already been sent
                    //  but we'll also send a quick response to the sender for this particular message.
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
                NotifyTraceEventRaised(ex);
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
                    NotifyTraceEventRaised(ex);
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
                    NotifyTraceEventRaised(ex);
                }
            });
#endif
        }


        private void NotifyServerStopping()
        {
            if (ServerStopping == null) return;
            string warningMessage = "Server has notified that it is stopping.";

#if NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    ServerStopping(this, null);
                    NotifyTraceEventRaised(warningMessage, SeverityType.Warning);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {nameof(NotifyServerStopping)}: {ex}");
                }
            }))
            { IsBackground = true }.Start();
#else
            Task.Run(() =>
            {
                try
                {
                    ServerStopping(this, null);
                    NotifyTraceEventRaised(warningMessage, SeverityType.Warning);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in {nameof(NotifyServerStopping)}: {ex}");
                }
            });
#endif
        }

        private void NotifyTraceEventRaised(string message, SeverityType severity)
        {
            string source = FriendlyName;
            if (string.IsNullOrEmpty(source)) source = ClientId;
            NotifyTraceEventRaised(new TraceEventArgs(message, severity, 0, source));
        }

        private void NotifyTraceEventRaised(Exception ex, int ErrorNumber = 0)
        {
            string source = FriendlyName;
            if (string.IsNullOrEmpty(source)) source = ClientId;
            NotifyTraceEventRaised(new TraceEventArgs(ex, ErrorNumber, source));
        }


        private void NotifyTraceEventRaised(TraceEventArgs args, Exception ex = null)
        {
#if NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    Debug.WriteLine(args.Message);
                    if ((ex != null || args.Severity == SeverityType.Error) && ExceptionRaised != null)
                    {
                        if (ex != null)
                            ExceptionRaised(this, new ExceptionEventArgs(ex, args.EventId));
                        else
                            ExceptionRaised(this, new ExceptionEventArgs(new Exception(args.Message), args.EventId));
                    }
                    TraceEventRaised?.Invoke(this, args);
                }
                catch (Exception ex1)
                {
                    Debug.WriteLine($"Error in {nameof(NotifyTraceEventRaised)}: {ex1}");
                }
            }))
            { IsBackground = true }.Start();
#else
            Task.Run(() =>
            {
                try
                {
                    Debug.WriteLine(args.Message);
                    if ((ex != null || args.Severity == SeverityType.Error) && ExceptionRaised != null)
                    {
                        if (ex != null) 
                            ExceptionRaised(this, new ExceptionEventArgs(ex, args.EventId));
                        else 
                            ExceptionRaised(this, new ExceptionEventArgs(new Exception(args.Message), args.EventId));
                    }
                    TraceEventRaised?.Invoke(this, args);
                }
                catch (Exception ex1)
                {
                    Debug.WriteLine($"Error in {nameof(NotifyTraceEventRaised)}: {ex1}");
                }
            });
#endif
        }


#if NET35
        private static Random ThreadSafeRandom
        {
            get
            {
                // Initialize Random for the current thread if not already initialized
                if (_threadStaticRandom == null)
                {
                    _threadStaticRandom = new Random(Guid.NewGuid().GetHashCode());
                }
                return _threadStaticRandom;
            }
        }
#else
        private static Random ThreadSafeRandom
        {
            get
            {
                return _threadLocalRandom.Value;
            }
        }
#endif


        private static int GetThreadSafeRandomNumber(int min, int max)
        {
            return ThreadSafeRandom.Next(min, max);
        }

    }

}

