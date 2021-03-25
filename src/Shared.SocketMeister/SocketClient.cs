#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0017 // Simplify object initialization
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1805 // Do not initialize unnecessarily
#pragma warning disable CA2213 // Disposable fields should be disposed


using SocketMeister.Messages;
using System;
using System.Collections.Generic;
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
        private const int DONT_RECONNECT_DELAY_AFTER_SHUTDOWN = 30;

        /// <summary>
        /// The frequency, in seconds, that this client will poll the server, to ensure the socket is alive.
        /// </summary>
        private const int POLLING_FREQUENCY = 15;

        private SocketAsyncEventArgs _asyncEventArgsConnect = null;
        private SocketAsyncEventArgs _asyncEventArgsPolling = null;
        private SocketAsyncEventArgs _asyncEventArgsReceive = null;
        private SocketAsyncEventArgs _asyncEventArgsSendSubscriptionChanges = null;
        private readonly ManualResetEvent _autoResetConnectEvent = new ManualResetEvent(false);
        private readonly TokenCollection _subscriptions;
        private ConnectionStatuses _connectionStatus = ConnectionStatuses.Disconnected;
        private SocketEndPoint _currentEndPoint = null;
        private readonly bool _enableCompression;
        private readonly List<SocketEndPoint> _endPoints = null;
        private bool _isBackgroundConnectThreadRunning;
        private bool _isBackgroundOperationsThreadRunning;
        private DateTime _lastPollResponse = DateTime.Now;
        private readonly object _lock = new object();
        private DateTime? _lastMessageFromServer = null;
        private DateTime _nextPoll;
        private DateTime _nextSendSubscriptions;
        private readonly UnrespondedMessageCollection _unrespondedMessages = new UnrespondedMessageCollection();
        private readonly Random _randomizer = new Random();
        private MessageEngine _receiveEngine;
        private readonly SocketAsyncEventArgsPool _sendEventArgsPool;
        private bool _stopAllBackgroundThreads;
        private bool _stopClientPermanently;

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

            _enableCompression = EnableCompression;
            _receiveEngine = new MessageEngine(EnableCompression);

            //  CREATE SUBSCRIPTION AND SUBSCRIPTION CHANGES
            _subscriptions = new TokenCollection();

            //  SETUP ENDPOINTS AND CHOOSE THE ENDPOINT TO START WITH
            _endPoints = EndPoints;
            if (_endPoints.Count == 1)
            {
                CurrentEndPoint = _endPoints[0];
            }
            else
            {
                int loopCnt = _randomizer.Next(20);
                int pointer = 0;
                for (int a = 0; a < loopCnt; a++)
                {
                    pointer = _randomizer.Next(_endPoints.Count);
                }
                pointer = _randomizer.Next(_endPoints.Count);
                CurrentEndPoint = _endPoints[pointer];
                //  ENSURE THIS ENDPOINT IS SELECTED FIRST (Must have the lowest DontReconnectUntil)
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddYears(-1);
            }

            //  PREALLOCATE A POOL OF SocketAsyncEventArgs FOR SENDING
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

            BgConnectToServer();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="IPAddress1">IP Address to of the SocketMeister server to connect to</param>
        /// <param name="Port1">TCP port the server is listening on</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        public SocketClient(string IPAddress1, int Port1, bool EnableCompression)
        {
            List<SocketEndPoint> EndPoints = new List<SocketEndPoint>();
            EndPoints.Add(new SocketEndPoint(IPAddress1, Port1));

            _enableCompression = EnableCompression;
            _receiveEngine = new MessageEngine(EnableCompression);

            //  CREATE SUBSCRIPTION AND SUBSCRIPTION CHANGES
            _subscriptions = new TokenCollection();

            //  SETUP ENDPOINTS AND CHOOSE THE ENDPOINT TO START WITH
            _endPoints = EndPoints;
            if (_endPoints.Count == 1)
            {
                CurrentEndPoint = _endPoints[0];
            }
            else
            {
                int loopCnt = _randomizer.Next(20);
                int pointer = 0;
                for (int a = 0; a < loopCnt; a++)
                {
                    pointer = _randomizer.Next(_endPoints.Count);
                }
                pointer = _randomizer.Next(_endPoints.Count);
                CurrentEndPoint = _endPoints[pointer];
                //  ENSURE THIS ENDPOINT IS SELECTED FIRST (Must have the lowest DontReconnectUntil)
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddYears(-1);
            }

            //  PREALLOCATE A POOL OF SocketAsyncEventArgs FOR SENDING
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

            BgConnectToServer();
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
        {
            List<SocketEndPoint> EndPoints = new List<SocketEndPoint>();
            EndPoints.Add(new SocketEndPoint(IPAddress1, Port1));
            EndPoints.Add(new SocketEndPoint(IPAddress2, Port2));

            _enableCompression = EnableCompression;
            _receiveEngine = new MessageEngine(EnableCompression);

            //  CREATE SUBSCRIPTION AND SUBSCRIPTION CHANGES
            _subscriptions = new TokenCollection();

            //  SETUP ENDPOINTS AND CHOOSE THE ENDPOINT TO START WITH
            _endPoints = EndPoints;
            if (_endPoints.Count == 1)
            {
                CurrentEndPoint = _endPoints[0];
            }
            else
            {
                int loopCnt = _randomizer.Next(20);
                int pointer = 0;
                for (int a = 0; a < loopCnt; a++)
                {
                    pointer = _randomizer.Next(_endPoints.Count);
                }
                pointer = _randomizer.Next(_endPoints.Count);
                CurrentEndPoint = _endPoints[pointer];
                //  ENSURE THIS ENDPOINT IS SELECTED FIRST (Must have the lowest DontReconnectUntil)
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddYears(-1);
            }

            //  PREALLOCATE A POOL OF SocketAsyncEventArgs FOR SENDING
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

            BgConnectToServer();
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
                _autoResetConnectEvent.Close();

                if (_asyncEventArgsConnect != null) _asyncEventArgsConnect.Dispose();
                _asyncEventArgsConnect = null;

                if (_asyncEventArgsPolling != null) _asyncEventArgsPolling.Dispose();
                _asyncEventArgsPolling = null;

                if (_asyncEventArgsReceive != null) _asyncEventArgsReceive.Dispose();
                _asyncEventArgsReceive = null;

                if (_asyncEventArgsSendSubscriptionChanges != null) _asyncEventArgsSendSubscriptionChanges.Dispose();
                _asyncEventArgsSendSubscriptionChanges = null;

                _receiveEngine = null; ;
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
            get { lock (_lock) { return _connectionStatus; } }
            private set
            {
                lock (_lock)
                {
                    if (_connectionStatus == value) return;
                    _connectionStatus = value;
                }
                ConnectionStatusChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// The current socket endpoint which the client is using
        /// </summary>
        public SocketEndPoint CurrentEndPoint
        {
            get { lock (_lock) { return _currentEndPoint; } }
            private set
            {
                lock (_lock)
                {
                    if (_currentEndPoint == value) return;
                    _currentEndPoint = value;
                }
                CurrentEndPointChanged?.Invoke(this, new EventArgs());
            }
        }


        private bool IsBackgroundConnectRunning { get { lock (_lock) { return _isBackgroundConnectThreadRunning; } } set { lock (_lock) { _isBackgroundConnectThreadRunning = value; } } }

        private bool IsBackgroundOperationsRunning { get { lock (_lock) { return _isBackgroundOperationsThreadRunning; } } set { lock (_lock) { _isBackgroundOperationsThreadRunning = value; } } }

        /// <summary>
        /// The last time a polling response was received from the socket server.
        /// </summary>
        private DateTime LastPollResponse { get { lock (_lock) { return _lastPollResponse; } } set { lock (_lock) { _lastPollResponse = value; } } }

        /// <summary>
        /// The next time this socket client should attempt to poll the socket server.
        /// </summary>
        private DateTime NextPoll { get { lock (_lock) { return _nextPoll; } } set { lock (_lock) { _nextPoll = value; } } }

        private DateTime NextSendSubscriptions { get { lock (_lock) { return _nextSendSubscriptions; } } set { lock (_lock) { _nextSendSubscriptions = value; } } }

        private bool StopAllBackgroundThreads { get { lock (_lock) { return _stopAllBackgroundThreads; } } set { lock (_lock) { _stopAllBackgroundThreads = value; } } }

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
            List<string> rVal = new List<string>();
            List<Token> tokens = _subscriptions.ToList();
            foreach (Token t in tokens)
            {
                rVal.Add(t.Name);
            }
            return rVal;
        }





        #region Socket async connect



        /// <summary>
        /// Disconnect the socket gracefully. 
        /// </summary>
        private void DisconnectSocketGracefully()
        {
            //  INITIATE SHUTDOWN
            ConnectionStatus = ConnectionStatuses.Disconnecting;
            StopAllBackgroundThreads = true;

            //  CLOSE UNRESPONDED MESSAGES
            _unrespondedMessages.ResetToUnsent();

            //  ENSURE BACKGROUND THREADS HAVE STOPPED
            EnsureBackgroundThreadsHaveStopped();

            //  A GRACEFUL CLOSE SHOULD OCCUR LIKE THIS (* IS THIS STEP/S)
            //  1. * The client socket calls Shutdown(SocketShutdown.Send)) but should keep receiving
            //  2.   On the server, EndReceive returns 0 bytes read(the client signals there is no more data from its side)
            //  3.   The server A) sends its last data B) calls Shutdown(SocketShutdown.Send)) C) calls Close on the socket, optionally with a timeout to allow the data to be read from the client
            //  4.   The client A) reads the remaining data from the server and then receives 0 bytes(the server signals there is no more data from its side) B) calls Close on the socket
            try { CurrentEndPoint.Socket.Shutdown(SocketShutdown.Send); }
            catch { }
        }


        /// <summary>
        /// Disconnect the socket. Note: This is performed in the background.
        /// </summary>
        private void CloseSocketWithForce()
        {
            if (ConnectionStatus == ConnectionStatuses.Disconnecting || ConnectionStatus == ConnectionStatuses.Disconnected) return;

            //  INITIATE SHUTDOWN
            SocketEndPoint disconnectingEndPoint = CurrentEndPoint;

            ConnectionStatus = ConnectionStatuses.Disconnecting;

            //  SET BACKGROUND THREADS TO STOP
            StopAllBackgroundThreads = true;

            Thread bgDisconnect = new Thread(
            new ThreadStart(delegate
            {
                //  CLOSE UNRESPONDED MESSAGES
                _unrespondedMessages.ResetToUnsent();

                if (disconnectingEndPoint.Socket.Connected == true)
                {
                    //  ATTEMPT TO SEND A DISCONNECT MESSAGE TO THE SERVER.
                    byte[] sendBytes = MessageEngine.GenerateSendBytes(new ClientDisconnectingNotificationV1(), false);
                    using (SocketAsyncEventArgs sendDisconnectEventArgs = new SocketAsyncEventArgs())
                    {
                        try
                        {
                            sendDisconnectEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                            sendDisconnectEventArgs.RemoteEndPoint = disconnectingEndPoint.IPEndPoint;
                            disconnectingEndPoint.Socket.SendAsync(sendDisconnectEventArgs);
                        }
                        catch { }
                    }
                }

                //  COMPLETE CLOSING ACTIVITIES AND START RECONNECTING IF REQUIRED
                EnsureBackgroundThreadsHaveStopped();
                CompleteSocketClosure(disconnectingEndPoint, false);
            }));

            bgDisconnect.IsBackground = true;
            bgDisconnect.Start();

            EnsureBackgroundThreadsHaveStopped();
        }


        private void EnsureBackgroundThreadsHaveStopped()
        {
            //  SHOULD NOT HAPPEN BUT JUST TO MAKE SURE
            if (StopAllBackgroundThreads == false) StopAllBackgroundThreads = true;

            //  WAIT UP TO 15 SECONDS FOR BACKGROUND THREADS TO STOP
            DateTime maxWait = DateTime.Now.AddSeconds(15);
            while ((IsBackgroundOperationsRunning == true || IsBackgroundConnectRunning == true) && DateTime.Now < maxWait) { Thread.Sleep(100); }
        }


        private void CompleteSocketClosure(SocketEndPoint EndPoint, bool CreateNewSocket)
        {
            //  CLEANUP _asyncEventArgsReceive
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

#if SILVERLIGHT
            try { EndPoint.Socket.Close(); }
            catch { }
#else
            try
            {
                if (StopClientPermanently == true) EndPoint.Socket.Disconnect(false);
                else if (CreateNewSocket == true) EndPoint.Socket.Disconnect(false);
                else EndPoint.Socket.Disconnect(true);
            }
            catch { }
#endif
            if (CreateNewSocket == true)
            {
#if !NET35 && !NET20
                try { EndPoint.Socket.Dispose(); }
                catch { }
#endif
                EndPoint.RecreateSocket();
            }

            //  CLOSE UNRESPONDED MESSAGES (AGAIN IN SOME CASES). UNDER LOAD THE CLIENT CAN SUBMIT A MESSAGE (BECAUSE OF CROSS THREADING)
            _unrespondedMessages.ResetToUnsent();

            //  FINALIZE AND RE-ATTEMPT CONNECTION IS WE ARE NOT STOPPING
            ConnectionStatus = ConnectionStatuses.Disconnected;

            //  RECONNECT
            if (StopClientPermanently == false) BgConnectToServer();
        }



        /// <summary>
        /// Background process which creates a connection with one of the servers specified
        /// </summary>
        private void BgConnectToServer()
        {
            lock (_lock)
            {
                if (_isBackgroundConnectThreadRunning == true) return;
                _isBackgroundConnectThreadRunning = true;
            }
            ConnectionStatus = ConnectionStatuses.Connecting;

            Thread bgConnect = new Thread(new ThreadStart(delegate
            {
                while (StopClientPermanently == false)
                {
                    try
                    {
                        //  CHOOSE THE NEXT ENDPOINT TO TRY
                        if (_endPoints.Count > 1)
                        {
                            SocketEndPoint bestEP = _endPoints[0];
                            for (int i = 1; i < _endPoints.Count; i++)
                            {
                                if (_endPoints[i].DontReconnectUntil < bestEP.DontReconnectUntil) bestEP = _endPoints[i];
                            }
                            CurrentEndPoint = bestEP;
                        }

                        if (DateTime.Now > CurrentEndPoint.DontReconnectUntil)
                        {
                            //  TRY TO CONNECT
                            _asyncEventArgsConnect.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                            if (!CurrentEndPoint.Socket.ConnectAsync(_asyncEventArgsConnect)) ProcessConnect(null, _asyncEventArgsConnect);
                            _autoResetConnectEvent.Reset();
                            _autoResetConnectEvent.WaitOne(5000);

                            if (ConnectionStatus == ConnectionStatuses.Connected)
                            {
                                ExecuteBackgroundOperations();
                                break;
                            }
                            else
                            {
                                //  IMPORTANT!!! DON'T TRY THIS CONNECTION FOR AT LEAST 3 SECONDS
                                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(3000);
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(500);
                }
                IsBackgroundConnectRunning = false;
#if DEBUG
                Console.WriteLine("Exiting BgConnectToServer for client");
#endif
            }));
            bgConnect.IsBackground = true;
            bgConnect.Start();
        }


        /// <summary>
        /// Background process which performas various operations
        /// </summary>
        private void ExecuteBackgroundOperations()
        {
            Thread backgroundOperationsThread = new Thread(new ThreadStart(delegate
            {
                //  INITIALIZE
                lock (_lock)
                {
                    if (_isBackgroundOperationsThreadRunning == true) return;
                    _isBackgroundOperationsThreadRunning = true;
                    _stopAllBackgroundThreads = false;
                    _lastPollResponse = DateTime.Now;
                    _nextPoll = DateTime.Now;
                    _nextSendSubscriptions = DateTime.Now;
                }

                //  FLAG ALL SUBSCRIPTIONS (Tokens) FOR SENDING TO THE SERVER
                _subscriptions.FlagAllAfterSocketConnect();

                while (StopAllBackgroundThreads == false)
                {
                    //  POLLING
                    if (DateTime.Now > NextPoll && CanExecuteBackgroundOperation())
                    {
                        try
                        {
                            NextPoll = DateTime.Now.AddSeconds(POLLING_FREQUENCY);
                            byte[] sendBytes = MessageEngine.GenerateSendBytes(new PollingRequestV1(), false);
                            _asyncEventArgsPolling.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                            _asyncEventArgsPolling.SetBuffer(sendBytes, 0, sendBytes.Length);
                            if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsPolling)) ProcessSendPollRequest(null, _asyncEventArgsPolling);
                        }
                        catch { }
                    }

                    if (CanExecuteBackgroundOperation() && LastPollResponse < (DateTime.Now.AddSeconds(-DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS)))
                    {
                        IsBackgroundOperationsRunning = false;
                        NotifyExceptionRaised(new Exception("Disconnecting: Server failed to reply to polling after " + DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS + " seconds."));
                        CloseSocketWithForce();
                        return;
                    }

                    //  SEND SUBSCRIPTION CHANGES
                    if (DateTime.Now > NextSendSubscriptions && CanExecuteBackgroundOperation())
                    {
                        try
                        {
                            byte[] changesBytes = _subscriptions.GetChangeBytes();
                            if (changesBytes != null)
                            {
                                NextSendSubscriptions = DateTime.Now.AddSeconds(60);
                                byte[] sendBytes = MessageEngine.GenerateSendBytes(new TokenChangesRequestV1(changesBytes), false);
                                _asyncEventArgsSendSubscriptionChanges.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                                _asyncEventArgsSendSubscriptionChanges.SetBuffer(sendBytes, 0, sendBytes.Length);
                                if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsSendSubscriptionChanges)) ProcessSendSubscriptionChanges(null, _asyncEventArgsSendSubscriptionChanges);
                            }
                        }
                        catch
                        {
                            NextSendSubscriptions = DateTime.Now.AddSeconds(15);
                        }
                    }

                    Thread.Sleep(200);
                }
                IsBackgroundOperationsRunning = false;
                Console.WriteLine("Exiting ExecuteBackgroundOperations for client");
            }));
            backgroundOperationsThread.IsBackground = true;
            backgroundOperationsThread.Start();
        }



        /// <summary>
        /// Connect operation has completed
        /// </summary>
        /// <param name="sender">Sending Socket</param>
        /// <param name="e">Socket Arguments</param>
        private void ProcessConnect(object sender, SocketAsyncEventArgs e)
        {
            if (StopClientPermanently == true) return;

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
                    //  DONE
                    _autoResetConnectEvent.Set();
                }
                catch (Exception ex)
                {
                    CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(2000 + _randomizer.Next(4000));
                    _autoResetConnectEvent.Set();
                    NotifyExceptionRaised(ex);
                }
            }
            else if (e.SocketError == SocketError.TimedOut)
            {
                //  NOTE: WHEN FAILING OVER UNDER HIGH LOAD, SocketError.TimedOut OCCURS FOR UP TO 120 SECONDS (WORSE CASE)
                //  BEFORE CONNECTION SUCCESSFULLY COMPLETES. IT'S A BIT ANNOYING BUT I HAVE FOUND NO WORK AROUND.
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(2000 + _randomizer.Next(4000));
                _autoResetConnectEvent.Set();
            }
            else if (e.SocketError == SocketError.AddressAlreadyInUse)
            {
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(2000 + _randomizer.Next(4000));
                _autoResetConnectEvent.Set();
            }
            else
            {
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(2000 + _randomizer.Next(4000));
                _autoResetConnectEvent.Set();
            }
        }




        #endregion


        #region Socket async Send


        /// <summary>
        /// Stops the client.
        /// </summary>
        public void Stop()
        {
            StopClientPermanently = true;

            //  ENSURE BACKGROUND CONNECT HAS STOPPED
            _autoResetConnectEvent.Set();

            //  SHUTDOWN SOCKET
            if (ConnectionStatus == ConnectionStatuses.Connected)
            {
                DisconnectSocketGracefully();
            }
            else
            {
                EnsureBackgroundThreadsHaveStopped();
            }

            if (ConnectionStatus == ConnectionStatuses.Disconnecting)
            {
                //  WAIT UNTIL DISCONNECT HAS FINISHED
                DateTime maxWait = DateTime.Now.AddSeconds(15);
                while (ConnectionStatus != ConnectionStatuses.Disconnected && DateTime.Now < maxWait) { Thread.Sleep(50); }
            }

            if (ConnectionStatus != ConnectionStatuses.Disconnected)
            {
                ConnectionStatus = ConnectionStatuses.Disconnected;
            }
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

            if (_connectionStatus != ConnectionStatuses.Connected || !CurrentEndPoint.Socket.Connected) return;

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
            if (StopClientPermanently == true) return null;

            DateTime startTime = DateTime.Now;
            _unrespondedMessages.Add(message);

            byte[] sendBytes = MessageEngine.GenerateSendBytes(message, false);

            SocketAsyncEventArgs sendEventArgs;

            while (message.TrySendReceive == true && StopClientPermanently == false)
            {
                try
                {
                    if (message.Status == MessageEngineDeliveryStatus.Unsent && CanSendReceive() == true)
                    {
                        sendEventArgs = _sendEventArgsPool.Pop();
                        if (sendEventArgs != null)
                        {
                            message.Status = MessageEngineDeliveryStatus.InProgress;
                            sendEventArgs.UserToken = message;
                            sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                            int remainingMilliseconds = message.TimeoutMilliseconds - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);

                            if (remainingMilliseconds > 0)
                            {
                                sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;

                                if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs)) ProcessSend(null, sendEventArgs);

                                //  WAIT FOR RESPONSE
                                while (message.WaitForResponse)
                                {
                                    Thread.Sleep(10);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }
                if (message.TrySendReceive == true) Thread.Sleep(200);
            }

            _unrespondedMessages.Remove(message);

            if (message.Response != null)
            {
                if (message.Response.Error != null) throw new Exception(message.Response.Error);
                else return message.Response.ResponseData;
            }
            else throw new TimeoutException("SendReceive() timed out after " + message.TimeoutMilliseconds + " milliseconds");
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
                    CloseSocketWithForce();
                }
                else if (result != SocketError.Success)
                {
                    message.Status = MessageEngineDeliveryStatus.Unsent;
                    NotifyExceptionRaised(new Exception("Disconnecting: Send did not generate a success. Socket operation returned error code " + (int)e.SocketError));
                    CloseSocketWithForce();
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



        #endregion

        #region Socket async Receive


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

                //  COMPLETE CLOSING ACTIVITIES AND START RECONNECTING IF REQUIRED
                CompleteSocketClosure(CurrentEndPoint, true);

                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                NotifyExceptionRaised(new Exception("Disconnecting: ProcessReceive received socket error code " + (int)e.SocketError));
                CloseSocketWithForce();
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
                            Thread bgThread = new Thread(new ThreadStart(delegate
                            {
                                ThreadPool.QueueUserWorkItem(BgProcessMessage, message);
                            }));
                            bgThread.IsBackground = true;
                            bgThread.Start();

                        }

                        else if (_receiveEngine.MessageType == MessageEngineMessageType.ServerStoppingNotificationV1)
                        {
                            //  DON'T RECONNECT TO THIS SERVER FOR SOME NUMBER OF SECONDS
                            CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddSeconds(DONT_RECONNECT_DELAY_AFTER_SHUTDOWN);

                            NotifyServerStopping();
                            DisconnectSocketGracefully();
                        }
                        else if (_receiveEngine.MessageType == MessageEngineMessageType.PollingResponseV1)
                        {
                            LastPollResponse = DateTime.Now;
                        }

                        else if (_receiveEngine.MessageType == MessageEngineMessageType.SubscriptionChangesResponseV1)
                        {
                            TokenChangesResponseV1 response = _receiveEngine.GetSubscriptionChangesResponseV1();
                            _subscriptions.ImportTokenChangesResponseV1(response);
                            NextSendSubscriptions = DateTime.Now;
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
                CloseSocketWithForce();
            }
            catch (Exception ex)
            {
                NotifyExceptionRaised(new Exception("Disconnecting: Error running ProcessReceive: " + ex.Message));
                CloseSocketWithForce();
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




        #endregion

        #region Shared


        private bool CanExecuteBackgroundOperation()
        {
            lock (_lock)
            {
                try
                {
                    if (_stopClientPermanently == true || _stopAllBackgroundThreads == true) return false;
                    if (_connectionStatus != ConnectionStatuses.Connected) return false;
                    if (_currentEndPoint == null || _currentEndPoint.Socket == null) return false;
                    return _currentEndPoint.Socket.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }


        private bool CanSendReceive()
        {
            lock (_lock)
            {
                if (_stopClientPermanently == true) return false;
                if (_connectionStatus != ConnectionStatuses.Connected) return false;
                return _currentEndPoint.Socket.Connected;
            }
        }


        private void NotifyExceptionRaised(Exception ex)
        {
            Thread bgThread = new Thread(new ThreadStart(delegate
            {
                try
                {
                    ExceptionRaised(this, new ExceptionEventArgs(ex, 1234));
                }
                catch
                {
                }
            }));
            bgThread.IsBackground = true;
            bgThread.Start();
        }


        private void NotifyBroadcastReceived(Messages.BroadcastV1 Message)
        {
            if (BroadcastReceived != null)
            {
                Thread bgThread = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        BroadcastReceived(this, new BroadcastReceivedEventArgs(Message.Name, Message.Parameters));
                    }
                    catch (Exception ex)
                    {
                        NotifyExceptionRaised(ex);
                    }
                }));
                bgThread.IsBackground = true;
                bgThread.Start();
            }
        }



        private void NotifyServerStopping()
        {
            if (ServerStopping != null)
            {
                Thread bgThread = new Thread(new ThreadStart(delegate
                {
                    //  RAISE EVENT IN THE BACKGROUND
                    new Thread(new ThreadStart(delegate
                    {
                        try
                        {
                            ServerStopping(this, null);
                        }
                        catch (Exception ex)
                        {
                            NotifyExceptionRaised(ex);
                        }
                    }
                    )).Start();
                }));
                bgThread.IsBackground = true;
                bgThread.Start();
            }
        }



        #endregion
    }

}

#pragma warning restore CA2213 // Disposable fields should be disposed
#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression