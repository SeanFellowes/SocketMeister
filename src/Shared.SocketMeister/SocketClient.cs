#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1805 // Do not initialize unnecessarily


using SocketMeister.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
#if ! NET35 // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
using System.Threading.Tasks;
#endif

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

        /// <summary>
        /// The frequency, in seconds, that the client will send subscriptions to the server
        /// </summary>
        private const int SEND_SUBSCRIPTIONS_FREQUENCY = 60;

        private SocketAsyncEventArgs _asyncEventArgsConnect = null;
        private SocketAsyncEventArgs _asyncEventArgsPolling = null;
        private SocketAsyncEventArgs _asyncEventArgsReceive = null;
        private SocketAsyncEventArgs _asyncEventArgsSendSubscriptionChanges = null;
        private readonly TokenCollection _subscriptions = new TokenCollection();
        private ConnectionStatuses _connectionStatus = ConnectionStatuses.Disconnected;
        private readonly ReaderWriterLockSlim _connectionStatusLock = new ReaderWriterLockSlim();
        private SocketEndPoint _currentEndPoint = null;
        private readonly ReaderWriterLockSlim _currentEndPointLock = new ReaderWriterLockSlim();
        private readonly bool _enableCompression;
        private readonly List<SocketEndPoint> _endPoints = null;
        private bool _isBackgroundWorkerRunning;
        private DateTime? _lastMessageFromServer = null;
        private DateTime _lastPollResponse = DateTime.Now;
        private readonly object _lock = new object();
        private readonly byte[] _pollingBuffer = new byte[Constants.SEND_RECEIVE_BUFFER_SIZE];
        private readonly Random _randomizer = new Random();
        private readonly MessageEngine _receiveEngine;
        private readonly SocketAsyncEventArgsPool _sendEventArgsPool;
        private volatile bool _stopClientPermanently;
        private bool _triggerSendSubscriptions;
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
        /// Constructor
        /// </summary>
        /// <param name="EndPoints">Collection of endpoints that are available to connect to</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        public SocketClient(List<SocketEndPoint> EndPoints, bool EnableCompression)
        {
            if (EndPoints == null) throw new ArgumentNullException(nameof(EndPoints));
            else if (EndPoints.Count == 0) throw new ArgumentException("EndPoints must contain at least 1 value", nameof(EndPoints));
            _endPoints = EndPoints;

            _enableCompression = EnableCompression;
            _receiveEngine = new MessageEngine(EnableCompression);

            //  STATIC BUFFERS
            _pollingBuffer = MessageEngine.GenerateSendBytes(new PollingRequestV1(), false);

            //  SETUP ENDPOINTS AND CHOOSE THE ENDPOINT TO START WITH
            if (_endPoints.Count == 1)
            {
                _currentEndPoint = _endPoints[0];   // Safe to use direct access in class constructor
            }
            else
            {
                _currentEndPoint = _endPoints[_randomizer.Next(_endPoints.Count)];   // Safe to use direct access in class constructor
            }
            _currentEndPoint.DontReconnectUntil = DateTime.Now.AddYears(-1);   // Safe to use direct access in class constructor

            //  Setup a pool of SocketAsyncEventArgs for sending messages
            _sendEventArgsPool = new SocketAsyncEventArgsPool(Constants.SocketAsyncEventArgsPoolSize);
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
        /// <param name="IPAddress1">IP Address to of the SocketMeister server to connect to</param>
        /// <param name="Port1">TCP port the server is listening on</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        public SocketClient(string IPAddress1, int Port1, bool EnableCompression)
            : this(new List<SocketEndPoint> { new SocketEndPoint(IPAddress1, Port1) }, EnableCompression) { }


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
                EnableCompression)
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

                _asyncEventArgsSendSubscriptionChanges?.Dispose();
                _asyncEventArgsSendSubscriptionChanges = null;

                foreach (SocketEndPoint ep in _endPoints)
                {
                    ep.Dispose();
                }

                _endPoints.Clear();
            }
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
        /// The connection status of the socket client
        /// </summary>
        public ConnectionStatuses ConnectionStatus
        {
            get
            {
                if (_connectionStatusLock == null)
                {
                    return ConnectionStatuses.Disconnected;
                }

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
            private set
            {
                bool lockAcquired = false;
                try
                {
                    if (ConnectionStatus == value) return;
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

                try
                {
                    ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
                }
                catch 
                {
                    Debug.Write("Calling program error processing ConnectionStatusChanged event");
                }
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

        private bool TriggerSendSubscriptions { get { lock (_lock) { return _triggerSendSubscriptions; } } set { lock (_lock) { _triggerSendSubscriptions = value; } } }

        private bool StopClientPermanently { get { lock (_lock) { return _stopClientPermanently; } } set { lock (_lock) { _stopClientPermanently = value; } } }

        /// <summary>
        /// The number of subscriptions for this client
        /// </summary>
        public int SubscriptionCount => _subscriptions.Count;

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
            return _subscriptions.ToListOfStrings();
        }



        private void PerformDisconnect(bool SocketHasErrored)
        {
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                if (ConnectionStatus == ConnectionStatuses.Disconnecting || ConnectionStatus == ConnectionStatuses.Disconnected) return;

                //  Initiate Shutdown
                ConnectionStatus = ConnectionStatuses.Disconnecting;

                //  Stop raising events when messages are received
                try
                {
                    if (_asyncEventArgsReceive != null)
                    {
                        _asyncEventArgsReceive.Completed -= new EventHandler<SocketAsyncEventArgs>(ProcessSend);
                        _asyncEventArgsReceive.Dispose();
                        _asyncEventArgsReceive = null;
                    }
                }
                catch { }


                //  CLOSE UNRESPONDED MESSAGES
                _unrespondedMessages.ResetToUnsent();

                if (SocketHasErrored == false)
                {
                    //  TRY AND SEND A DISCONNECTING MESSAGE TO THE SERVER
                    try
                    {
                        byte[] sendBytes = MessageEngine.GenerateSendBytes(new ClientDisconnectingNotificationV1(), false);
                        CurrentEndPoint.Socket.Send(sendBytes);
                    }
                    catch { }
                }

                if (StopClientPermanently == true)
                {
                    //  WAIT UP TO 15 SECONDS FOR BACKGROUND THREADS TO STOP
                    DateTime maxWait = DateTime.Now.AddSeconds(15);
                    while ((IsBackgroundWorkerRunning == true) && DateTime.Now < maxWait) { Thread.Sleep(100); }
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

                ConnectionStatus = ConnectionStatuses.Disconnected;

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
                Debug.WriteLine(ex);
            }
            finally
            {
                timer.Stop();
                Debug.WriteLine("PerformDisconnect() took " + timer.ElapsedMilliseconds + " milliseconds to execute.");
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
                    TriggerSendSubscriptions = true;
                    LastPollResponse = DateTime.Now;

                    //  FLAG ALL SUBSCRIPTIONS (Tokens) FOR SENDING TO THE SERVER
                    _subscriptions.FlagAllAfterSocketConnect();

                    while (!StopClientPermanently)
                    {
                        switch (ConnectionStatus)
                        {
                            case ConnectionStatuses.Disconnected:
                            case ConnectionStatuses.Connecting:
                                BgPerformAttemptConnect();
                                break;
                            case ConnectionStatuses.Connected:
                                BgPerformPolling(pollingTimer);
                                BgPerformSubscriptionSends(sendSubscriptionsTimer);
                                break;
                        }
                        Thread.Sleep(200);
                    }
                }
                catch (Exception e) { Debug.WriteLine(e); }
                finally
                {
                    IsBackgroundWorkerRunning = false;
                    Debug.WriteLine("SocketClient Background Worker Exited");
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
                if (DateTime.Now < CurrentEndPoint.DontReconnectUntil.AddMilliseconds(-1500)) ConnectionStatus = ConnectionStatuses.Connecting;

                //  Exit is it's not time to actually attempt to reconnect
                if (DateTime.Now < CurrentEndPoint.DontReconnectUntil) return;

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

                if (ConnectionStatus != ConnectionStatuses.Connected)
                {
                    //  IMPORTANT!!! DON'T TRY THIS CONNECTION FOR AT LEAST 3 SECONDS
                    CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(3000);
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

                if (LastPollResponse < (DateTime.Now.AddSeconds(-DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS)))
                {
                    NotifyExceptionRaised(new Exception("Disconnecting: Server failed to reply to polling after " + DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS + " seconds."));
                    PerformDisconnect(SocketHasErrored: true);
                    return;
                }

#if NET35   // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
                PollingTimer.Reset();
                PollingTimer.Start();
#else
                PollingTimer.Restart();
#endif
                _asyncEventArgsPolling.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                _asyncEventArgsPolling.SetBuffer(_pollingBuffer, 0, _pollingBuffer.Length);
                if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsPolling)) ProcessSendPollRequest(null, _asyncEventArgsPolling);
            }
            catch { }
        }


        private void BgPerformSubscriptionSends(Stopwatch sendSubscriptionsTimer)
        {
            try
            {
                //  SEND SUBSCRIPTION CHANGES
                if ((sendSubscriptionsTimer.Elapsed.TotalSeconds > SEND_SUBSCRIPTIONS_FREQUENCY || TriggerSendSubscriptions == true))
                {
#if NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
                    sendSubscriptionsTimer.Reset();
                    sendSubscriptionsTimer.Start();
#else
                    sendSubscriptionsTimer.Restart();
#endif
                    TriggerSendSubscriptions = false;
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
            catch { }
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
                    _asyncEventArgsReceive = new SocketAsyncEventArgs();
                    _asyncEventArgsReceive.SetBuffer(new byte[Constants.SEND_RECEIVE_BUFFER_SIZE], 0, Constants.SEND_RECEIVE_BUFFER_SIZE);
                    _asyncEventArgsReceive.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessReceive);
                    if (!CurrentEndPoint.Socket.ReceiveAsync(_asyncEventArgsReceive)) ProcessReceive(null, _asyncEventArgsReceive);
                    //  CONNECTED
                    ConnectionStatus = ConnectionStatuses.Connected;
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }
            }
            else if (e.SocketError == SocketError.TimedOut)
            {
                //  NOTE: WHEN FAILING OVER UNDER HIGH LOAD, SocketError.TimedOut OCCURS FOR UP TO 120 SECONDS (WORSE CASE)
                //  BEFORE CONNECTION SUCCESSFULLY COMPLETES. IT'S A BIT ANNOYING BUT I HAVE FOUND NO WORK AROUND.
                Debug.WriteLine("Socket Timeout");
            }
            else if (e.SocketError == SocketError.AddressAlreadyInUse)
            {
                Debug.WriteLine("Socket Already in use");
            }
            else
            {
                Debug.WriteLine("Undefined Socket Error: " + e.SocketError.ToString());
            }

            //  RESET
            try
            {
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(pauseReconnect + _randomizer.Next(4000));
            }
            catch (Exception ex)
            {
                if (!StopClientPermanently)
                {
                    NotifyExceptionRaised(ex);
                }
            }
        }


        /// <summary>
        /// Stops the client.
        /// </summary>
        public void Stop()
        {
            StopClientPermanently = true;
            PerformDisconnect(SocketHasErrored: false);
        }


        private void SendResponse(MessageResponseV1 messageResponse, MessageV1 message)
        {
            byte[] sendBytes = MessageEngine.GenerateSendBytes(messageResponse, false);

            SocketAsyncEventArgs sendEventArgs;
            while (true == true)
            {
                if (message.IsTimeout)
                {
                    //  TIMEOUT: NO POINT SENDING THE RESPONSE BECAUSE IT WILL HAVE ALSO TIMED OUT AT THE OTHER END
                    NotifyExceptionRaised(new TimeoutException("Message " + message.MessageId + ", timed out after " + message.TimeoutMilliseconds + " milliseconds."));
                    return;
                }
                else if (StopClientPermanently)
                {
                    //  SHUTDOWN: CLIENT IS SHUTTING DOWN. A SHUTDOWN MESSAGE SHOULD HAVE ALREADY BEEN SENT SO EXIT
                    SendResponseQuickly(new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.Stopping));
                    return;
                }

                if (messageResponse.Status == MessageEngineDeliveryStatus.Unsent && CanSendReceive() == true)
                {
                    sendEventArgs = _sendEventArgsPool.Pop();
                    if (sendEventArgs != null)
                    {
                        messageResponse.Status = MessageEngineDeliveryStatus.InProgress;
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
                            NotifyExceptionRaised(ex);
                        }
                    }
                }
                Thread.Sleep(200);
            }

        }


        private void SendResponseQuickly(MessageResponseV1 Message)
        {
            byte[] sendBytes = MessageEngine.GenerateSendBytes(Message, false);

            if (ConnectionStatus != ConnectionStatuses.Connected || !CurrentEndPoint.Socket.Connected) return;

            SocketAsyncEventArgs sendEventArgs = _sendEventArgsPool.Pop();
            if (sendEventArgs == null) return;

            sendEventArgs.UserToken = Message;
            sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
            Message.Status = MessageEngineDeliveryStatus.InProgress;
            sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
            try
            {
                if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs)) ProcessSend(null, sendEventArgs);
            }
            catch (Exception ex)
            {
                NotifyExceptionRaised(ex);
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
            DateTime startTime = DateTime.Now;
            DateTime maxWait = startTime.AddMilliseconds(TimeoutMilliseconds);
            while (ConnectionStatus != ConnectionStatuses.Connected && StopClientPermanently == false)
            {
                Thread.Sleep(200);
                if (StopClientPermanently) throw new Exception("Message cannot be sent. The socket client is stopped or stopping");
                if (DateTime.Now > maxWait) throw new TimeoutException();
            }
            //DelaySending();
            int remainingMilliseconds = TimeoutMilliseconds - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
            return SendReceive(new MessageV1(Parameters, remainingMilliseconds, IsLongPolling));
        }


        private byte[] SendReceive(MessageV1 message)
        {
            if (_stopClientPermanently) return null;

            DateTime startTime = DateTime.Now;
            _unrespondedMessages.Add(message);

            // Generate the sendBytes once outside the loop
            byte[] sendBytes = MessageEngine.GenerateSendBytes(message, false);

#if NET35
            // Legacy .NET 3.5 approach using Thread.Sleep
            while (message.TrySendReceive && !_stopClientPermanently)
            {
                try
                {
                    if (message.Status == MessageEngineDeliveryStatus.Unsent && CanSendReceive())
                    {
                        var sendEventArgs = _sendEventArgsPool.Pop();
                        if (sendEventArgs != null)
                        {
                            message.Status = MessageEngineDeliveryStatus.InProgress;
                            sendEventArgs.UserToken = message;
                            sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                            sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;

                            if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs))
                                ProcessSend(null, sendEventArgs);

                            // Wait for response or timeout
                            int remainingMilliseconds = message.TimeoutMilliseconds - (int)(DateTime.Now - startTime).TotalMilliseconds;
                            if (remainingMilliseconds <= 0) break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }

                Thread.Sleep(10); // Poll for response
            }
#else
            // Improved implementation for .NET 4.0+
            using (var messageWaitHandle = new ManualResetEventSlim(false))
            {
                message.ResponseReceivedEvent = messageWaitHandle;

                while (message.TrySendReceive && !_stopClientPermanently)
                {
                    try
                    {
                        if (message.Status == MessageEngineDeliveryStatus.Unsent && CanSendReceive())
                        {
                            var sendEventArgs = _sendEventArgsPool.Pop();
                            if (sendEventArgs != null)
                            {
                                message.Status = MessageEngineDeliveryStatus.InProgress;
                                sendEventArgs.UserToken = message;
                                sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                                sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;

                                if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs))
                                    ProcessSend(null, sendEventArgs);

                                // Wait for response or timeout
                                int remainingMilliseconds = message.TimeoutMilliseconds - (int)(DateTime.Now - startTime).TotalMilliseconds;
                                if (!messageWaitHandle.Wait(remainingMilliseconds)) break; // Timeout
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NotifyExceptionRaised(ex);
                    }
                }
            }
#endif

            _unrespondedMessages.Remove(message);

            if (message.Response?.Error != null) throw new Exception(message.Response.Error);
            return message.Response?.ResponseData ?? throw new TimeoutException($"SendReceive() timed out after {message.TimeoutMilliseconds} ms");
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
                    message.Status = MessageEngineDeliveryStatus.Unsent;
                    NotifyExceptionRaised(new Exception("Disconnecting: Connection was reset."));
                    PerformDisconnect(SocketHasErrored: true);
                }
                else if (result != SocketError.Success)
                {
                    message.Status = MessageEngineDeliveryStatus.Unsent;
                    NotifyExceptionRaised(new Exception("Disconnecting: Send did not generate a success. Socket operation returned error code " + (int)e.SocketError));
                    PerformDisconnect(SocketHasErrored: true);
                }
            }
            catch (Exception ex)
            {
                message.Status = MessageEngineDeliveryStatus.Unsent;
                NotifyExceptionRaised(ex);
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
            if (ConnectionStatus == ConnectionStatuses.Disconnected) return;

            if (e.BytesTransferred == 0)
            {
                //  A GRACEFUL CLOSE SHOULD OCCUR LIKE THIS (* IS THIS STEP/S)
                //  1.   The client socket calls Shutdown(SocketShutdown.Send)) but should keep receiving
                //  2.   On the server, EndReceive returns 0 bytes read(the client signals there is no more data from its side)
                //  3.   The server A) sends its last data B) calls Shutdown(SocketShutdown.Send)) C) calls Close on the socket, optionally with a timeout to allow the data to be read from the client
                //  4. * The client A) reads the remaining data from the server and then receives 0 bytes(the server signals there is no more data from its side) B) calls Close on the socket

                PerformDisconnect(SocketHasErrored: true);
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                NotifyExceptionRaised(new Exception("Disconnecting: ProcessReceive received socket error code " + (int)e.SocketError));
                PerformDisconnect(SocketHasErrored: true);
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
                        LastMessageFromServer = DateTime.Now;

                        if (_receiveEngine.MessageType == MessageEngineMessageType.MessageResponseV1)
                        {
                            //  SyncEndPointSubscriptionsWithServer() IS WAITING. COMPLETE THE SYNCRONOUS OPERATION SO IT CAN CONTINUE
                            MessageResponseV1 response = _receiveEngine.GetMessageResponseV1();

                            //  CHECK TO SEE IS THE MESSAGE IS IN THE LIST OF OPEN SendReceive ITEMS.
                            MessageV1 foundUnrespondedMessage = _unrespondedMessages[response.MessageId];
                            if (foundUnrespondedMessage != null)
                            {
                                if (response.ProcessingResult == MessageEngineDeliveryResult.Stopping)
                                {
                                    //  SOCKET SERVER IS SHUTTING DOWN. WE WILL RETRY
                                    foundUnrespondedMessage.Status = MessageEngineDeliveryStatus.Unsent;
                                }
                                else
                                {
                                    foundUnrespondedMessage.Response = response;
                                    foundUnrespondedMessage.Status = MessageEngineDeliveryStatus.ResponseReceived;
                                }
                            }
                        }

                        else if (_receiveEngine.MessageType == MessageEngineMessageType.MessageV1)
                        {
                            MessageV1 message = _receiveEngine.GetMessageV1();
#if NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
                            new Thread(new ThreadStart(delegate
                            {
                                ThreadPool.QueueUserWorkItem(BgProcessMessage, message);
                            }))
                            { IsBackground = true }.Start();
#else
                            Task.Run(() => BgProcessMessage(message));
#endif
                        }

                        else if (_receiveEngine.MessageType == MessageEngineMessageType.ServerStoppingNotificationV1)
                        {
                            //  DON'T RECONNECT TO THIS SERVER FOR SOME NUMBER OF SECONDS
                            if (_endPoints.Count > 1)
                            {
                                CurrentEndPoint.DontReconnectUntil = DateTime.Now;
                            }
                            else
                            {
                                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddSeconds(RECONNECT_DELAY_AFTER_SERVER_SHUTDOWN_AND_ONE_ENDPOINT);
                            }

                            NotifyServerStopping();
                            PerformDisconnect(SocketHasErrored: false);
                        }
                        else if (_receiveEngine.MessageType == MessageEngineMessageType.PollingResponseV1)
                        {
                            LastPollResponse = DateTime.Now;
                        }

                        else if (_receiveEngine.MessageType == MessageEngineMessageType.SubscriptionChangesResponseV1)
                        {
                            TokenChangesResponseV1 response = _receiveEngine.GetSubscriptionChangesResponseV1();
                            _subscriptions.ImportTokenChangesResponseV1(response);
                            TriggerSendSubscriptions = true;
                        }

                        else if (_receiveEngine.MessageType == MessageEngineMessageType.BroadcastV1)
                        {
                            NotifyBroadcastReceived(_receiveEngine.GetBroadcastV1());
                        }
                    }
                }

                //  KEEP RECEIVING
                if (ConnectionStatus == ConnectionStatuses.Disconnecting && !CurrentEndPoint.Socket.ReceiveAsync(e)) ProcessReceive(null, e);
                else if (StopClientPermanently == false && ConnectionStatus == ConnectionStatuses.Connected && !CurrentEndPoint.Socket.ReceiveAsync(e)) ProcessReceive(null, e);
            }
            catch (ObjectDisposedException ee)
            {
                //  IF A LARGE CHUNK OF DATA WAS BEING RECEIVED WHEN THE CONNECTION WAS LOST, THE Disconnect() ROUTINE
                //  MAY ALREADY HAVE BEEN RUN (WHICH DISPOSES OBJECTS). IF THIS IS THE CASE, SIMPLY EXIT
                NotifyExceptionRaised(new Exception("Disconnecting: ObjectDisposedException running ProcessReceive: " + ee.Message));
                PerformDisconnect(SocketHasErrored: true);
            }
            catch (Exception ex)
            {
                NotifyExceptionRaised(new Exception("Disconnecting: Error running ProcessReceive: " + ex.Message));
                PerformDisconnect(SocketHasErrored: true);
            }
        }


        private void BgProcessMessage(object state)
        {
            MessageV1 message = (MessageV1)state;
            try
            {
                if (MessageReceived == null)
                {
                    //  THERE IS NO CODE LISTENING TO MessageReceived EVENTS. CANNOT PROCESS THIS MESSAGE
                    SendResponseQuickly(new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.NoMessageReceivedEventListener));
                }
                else if (StopClientPermanently)
                {
                    //  SHUTDOWN: CLIENT IS SHUTTING DOWN. A SHUTDOWN MESSAGE SHOULD HAVE ALREADY BEEN SENT SO EXIT
                    SendResponseQuickly(new MessageResponseV1(message.MessageId, MessageEngineDeliveryResult.Stopping));
                }
                else if (message.IsTimeout)
                {
                    //  IF TIMEOUT, NO POINT SENDING THE RESPONSE
                    NotifyExceptionRaised(new TimeoutException("Message " + message.MessageId + ", timed out after " + message.TimeoutMilliseconds + " milliseconds."));
                }
                else
                {
                    //  DESERIALIZE THE MESSAGE FROM THE CLIENT
                    //  WE HAVE A MESSAGE IN FULL. UNPACK, (RESETS COUNTERS) AND RAISE AN EVENT
                    MessageReceivedEventArgs args = new MessageReceivedEventArgs(message.Parameters);
                    MessageReceived(this, args);

                    //  SEND RESPONSE
                    MessageResponseV1 response = new MessageResponseV1(message.MessageId, args.Response);
                    SendResponse(response, message);
                }
            }
            catch (Exception ex)
            {
                NotifyExceptionRaised(ex);
                SendResponseQuickly(new MessageResponseV1(message.MessageId, ex));
            }
        }


        private bool CanSendReceive()
        {
            return !_stopClientPermanently && ConnectionStatus == ConnectionStatuses.Connected && CurrentEndPoint.Socket.Connected;
        }


        private void NotifyExceptionRaised(Exception ex)
        {
            if (ExceptionRaised == null) return;

#if NET35  // Compiler directive for code which will be compiled to .NET 3.5. This provides a less efficient but workable solution to the desired functional requirements.
            new Thread(new ThreadStart(delegate
            {
                NotifyExceptionRaisedInThread(ex);
            }))
            { IsBackground = true }.Start();
#else
            Task.Run(() =>
            {
                NotifyExceptionRaisedInThread(ex);
            });
#endif
        }

        private void NotifyExceptionRaisedInThread(Exception ex)
        {
            try
            {
                ExceptionRaised?.Invoke(this, new ExceptionEventArgs(ex, 1234));
            }
            catch { }
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
                    NotifyExceptionRaisedInThread(ex);
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
                    NotifyExceptionRaisedInThread(ex);
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
                    NotifyExceptionRaisedInThread(ex);
                }
            }))
            { IsBackground = true }.Start();
#else
            Task.Run(() =>
            {
                try
                {
                    ServerStopping(this, null);
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaisedInThread(ex);
                }
            });
#endif
        }
    }

}

#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression