#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable IDE0017 // Simplify object initialization

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketMeister.Messages;

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
        /// If a poll response has not been received from the server after a number of seconds, the socketet client will be disconnected.
        /// </summary>
        private const int DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS = 45;

        /// <summary>
        /// When a shutdown occurs, particularly because of network failure or server shutdown, delay attempting to reconnect to that server, giving the server some time to complete it's shutdown process.
        /// </summary>
        private const int DONT_RECONNECT_DELAY_AFTER_SHUTDOWN = 15;

        /// <summary>
        /// The frequency, in seconds, that this client will poll the server, to ensure the socket is alive.
        /// </summary>
        private const int POLLING_FREQUENCY = 15;

        private SocketAsyncEventArgs _asyncEventArgsConnect = null;
        private SocketAsyncEventArgs _asyncEventArgsPolling = null;
        private SocketAsyncEventArgs _asyncEventArgsReceive = null;
        private SocketAsyncEventArgs _asyncEventArgsSendSubscriptionChanges = null;
        private readonly ManualResetEvent _autoResetConnectEvent = new ManualResetEvent(false);
        //private readonly ManualResetEvent _autoResetPollEvent = new ManualResetEvent(false);
        //private readonly ManualResetEvent _autoResetSendSubscriptionChangesEvent = new ManualResetEvent(false);
        private readonly TokenCollection _subscriptions;
        private ConnectionStatuses _connectionStatus = ConnectionStatuses.Disconnected;
#pragma warning disable CA2213 // Disposable fields should be disposed
        private SocketEndPoint _currentEndPoint = null;
#pragma warning restore CA2213 // Disposable fields should be disposed
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0052 // Remove unread private members
        private bool _enableCompression;
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore IDE0044 // Add readonly modifier
        private readonly List<SocketEndPoint> _endPoints = null;
        private bool _isBackgroundConnectRunning;
        private bool _isBackgroundPollingRunning;
        private bool _isStopAllRequested = false;
        private bool _isStopBackgroundOperationsRequested = false;
        private DateTime _lastPollResponse = DateTime.Now;
        private readonly object _lock = new object();
        private DateTime _nextPollRequest;
        private DateTime _nextSendSubscriptions;
        private readonly OpenRequestMessageCollection _openRequests = new OpenRequestMessageCollection();
        private int _requestsInProgress = 0;
        private readonly Random _randomizer = new Random();
        private MessageEngine _receiveEngine;
        private readonly SocketAsyncEventArgsPool _sendEventArgsPool;

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Event raised when an exception occurs
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;

        /// <summary>
        /// Event raised whenever a message is received from the server.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raised when a request message is received from the server. A response can be provided which will be returned to the server.
        /// </summary>
        public event EventHandler<RequestReceivedEventArgs> RequestReceived;

        /// <summary>
        /// Event raised when a the server stops
        /// </summary>
        public event EventHandler<EventArgs> ServerStopping;


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
                //_autoResetPollEvent.Close();
                //_autoResetSendSubscriptionChangesEvent.Close();

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
                    try { ep.CloseSocket(); }
                    catch { }
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
                if (ConnectionStatusChanged != null)
                {
                    if (value != ConnectionStatuses.Connected) ConnectionStatusChanged(this, new ConnectionStatusChangedEventArgs(value, "", 0));
                    else ConnectionStatusChanged(this, new ConnectionStatusChangedEventArgs(value, CurrentEndPoint.IPAddress, CurrentEndPoint.Port));
                }
            }
        }

        private SocketEndPoint CurrentEndPoint
        {
            get { lock (_lock) { return _currentEndPoint; } }
            set { lock (_lock) { _currentEndPoint = value; } }
        }

        /// <summary>
        /// Get a list of subscription names
        /// </summary>
        /// <returns>List of subscription names</returns>
        public List<string> GetSubscriptions()
        {
            List<string> rVal = new List<string>();
            lock(_lock)
            {
                List<Token> tokens = _subscriptions.ToList();
                foreach(Token t in tokens)
                {
                    rVal.Add(t.Name);
                }
            }
            return rVal;
        }

        private bool IsBackgroundConnectRunning { get { lock (_lock) { return _isBackgroundConnectRunning; } } set { lock (_lock) { _isBackgroundConnectRunning = value; } } }

        private bool IsBackgroundPollingRunning { get { lock (_lock) { return _isBackgroundPollingRunning; } } set { lock (_lock) { _isBackgroundPollingRunning = value; } } }

        private bool IsStopAllRequested { get { lock (_lock) { return _isStopAllRequested; } } set { lock (_lock) { _isStopAllRequested = value; } } }

        private bool IsStopBackgroundOperationsRequested { get { lock (_lock) { return _isStopBackgroundOperationsRequested; } } set { lock (_lock) { _isStopBackgroundOperationsRequested = value; } } }

        /// <summary>
        /// The last time a polling response was received from the socket server.
        /// </summary>
        private DateTime LastPollResponse { get { lock (_lock) { return _lastPollResponse; } } set { lock (_lock) { _lastPollResponse = value; } } }

        /// <summary>
        /// The next time this socket client should attempt to poll the socket server.
        /// </summary>
        private DateTime NextPollRequest { get { lock (_lock) { return _nextPollRequest; } } set { lock (_lock) { _nextPollRequest = value; } } }

        private DateTime NextSendSubscriptions { get { lock (_lock) { return _nextSendSubscriptions; } } set { lock (_lock) { _nextSendSubscriptions = value; } } }


        #region Socket async connect


        /// <summary>
        /// Disconnect the socket. Note: This is performed in the background.
        /// </summary>
        private void DisconnectSocket()
        {
            if (ConnectionStatus == ConnectionStatuses.Disconnecting || ConnectionStatus == ConnectionStatuses.Disconnected) return;

            //  INITIATE SHUTDOWN
            SocketEndPoint disconnectingEndPoint = CurrentEndPoint;
            disconnectingEndPoint.DontReconnectUntil = DateTime.Now.AddSeconds(DONT_RECONNECT_DELAY_AFTER_SHUTDOWN);
            ConnectionStatus = ConnectionStatuses.Disconnecting;

            Thread bgDisconnect = new Thread(
            new ThreadStart(delegate
            {
                //  STOP BACKGROUND OPERATIONS
                IsStopBackgroundOperationsRequested = true;
                //_autoResetPollEvent.Set();
                //_autoResetSendSubscriptionChangesEvent.Set();

                //  CLOSE OPEN REQUESTS
                _openRequests.ResetToUnsent();

                if (disconnectingEndPoint.Socket.Connected == true)
                {
                    //  ATTEMPT TO SEND A DISCONNECT MESSAGE TO THE SERVER.
                    byte[] sendBytes = MessageEngine.GenerateSendBytes(new ClientDisconnectMessage(), false);
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

                    //  DON'T RECONNECT TO THIS SERVER FOR SOME NUMBER OF SECONDS
                    disconnectingEndPoint.DontReconnectUntil = DateTime.Now.AddSeconds(DONT_RECONNECT_DELAY_AFTER_SHUTDOWN);
                }

                //  ENSURE BACKGROUND POLLING HAS STOPPED
                while (IsBackgroundPollingRunning == true) { Thread.Sleep(5); }

                //  SHUTDOWN THE ENDPOINT
                disconnectingEndPoint.CloseSocket();

                //  CLEANUP
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

                //  CLOSE OPEN REQUESTS AGAIN!!! UNDER LOAD THE CLIENT CAN SUBMIT A REQUEST (BECAUSE OF CROSS THREADING)
                _openRequests.ResetToUnsent();

                //  FINALIZE AND RE-ATTEMPT CONNECTION IS WE ARE NOT STOPPING
                ConnectionStatus = ConnectionStatuses.Disconnected;
                if (IsStopAllRequested == false) BgConnectToServer();

            }));
            bgDisconnect.IsBackground = true;
            bgDisconnect.Start();
        }




        /// <summary>
        /// Background process which creates a connection with one of the servers specified
        /// </summary>
        private void BgConnectToServer()
        {
            lock (_lock)
            {
                if (_isBackgroundConnectRunning == true) return;
                _isBackgroundConnectRunning = true;
            }
            ConnectionStatus = ConnectionStatuses.Connecting;

            Thread bgConnect = new Thread(new ThreadStart(delegate
            {
                while (IsStopAllRequested == false)
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

                        if (CurrentEndPoint.DontReconnectUntil < DateTime.Now)
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
                        }
                    }
                    catch { }
                    Thread.Sleep(500);
                }
                IsBackgroundConnectRunning = false;
            }));
            bgConnect.IsBackground = true;
            bgConnect.Start();
        }


        /// <summary>
        /// Background process which performas various operations
        /// </summary>
        private void ExecuteBackgroundOperations()
        {
            Thread bgPolling = new Thread(new ThreadStart(delegate
            {
                lock (_lock)
                {
                    if (_isBackgroundPollingRunning == true) return;
                    _isBackgroundPollingRunning = true;
                    _isStopBackgroundOperationsRequested = false;
                    _lastPollResponse = DateTime.Now;
                    _nextPollRequest = DateTime.Now;
                    _nextSendSubscriptions = DateTime.Now;
                }

                while (IsStopBackgroundOperationsRequested == false)
                {
                    //  POLLING
                    if (DateTime.Now > NextPollRequest && CanExecuteBackgroundOperation())
                    {
                        try
                        {
                            NextPollRequest = DateTime.Now.AddSeconds(POLLING_FREQUENCY);
                            byte[] sendBytes = MessageEngine.GenerateSendBytes(new PollRequest(), false);
                            _asyncEventArgsPolling.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                            _asyncEventArgsPolling.SetBuffer(sendBytes, 0, sendBytes.Length);
                            if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsPolling)) ProcessSendPollRequest(null, _asyncEventArgsPolling);
                            //_autoResetPollEvent.Reset();
                            //_autoResetPollEvent.WaitOne();
                        }
                        catch { }
                    }

                    if (CanExecuteBackgroundOperation() && LastPollResponse < (DateTime.Now.AddSeconds(-DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS)))
                    {
                        NotifyExceptionRaised(new Exception("Disconnecting: Server failed to reply to polling after " + DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS + " seconds."));
                        DisconnectSocket();
                        break;
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
                                byte[] sendBytes = MessageEngine.GenerateSendBytes(new SubscriptionRequest(changesBytes), false);
                                _asyncEventArgsSendSubscriptionChanges.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                                _asyncEventArgsSendSubscriptionChanges.SetBuffer(sendBytes, 0, sendBytes.Length);
                                if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsSendSubscriptionChanges)) ProcessSendSubscriptionChanges(null, _asyncEventArgsSendSubscriptionChanges);
                                //_autoResetSendSubscriptionChangesEvent.Reset();
                                //_autoResetSendSubscriptionChangesEvent.WaitOne();
                            }
                        }
                        catch 
                        {
                            NextSendSubscriptions = DateTime.Now.AddSeconds(15);
                        }
                    }


                    Thread.Sleep(200);
                }
                IsBackgroundPollingRunning = false;
            }));
            bgPolling.IsBackground = true;
            bgPolling.Start();
        }



        /// <summary>
        /// Connect operation has completed
        /// </summary>
        /// <param name="sender">Sending Socket</param>
        /// <param name="e">Socket Arguments</param>
        private void ProcessConnect(object sender, SocketAsyncEventArgs e)
        {
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
            IsStopAllRequested = true;

            //  ENSURE BACKGROUND CONNECT HAS STOPPED
            _autoResetConnectEvent.Set();
            while (IsBackgroundConnectRunning == true) { Thread.Sleep(5); }

            //  SHUTDOWN SOCKET
            DisconnectSocket();

            //  WAIT UNTIL DISCONNECT HAS FINISHED
            while (ConnectionStatus != ConnectionStatuses.Disconnected) { Thread.Sleep(50); }
        }


#pragma warning disable IDE0051 // Remove unused private members
        private void DelaySending()
#pragma warning restore IDE0051 // Remove unused private members
        {
            int inProgress = 0;
            lock (_lock) { inProgress = _openRequests.Count; }
            if (inProgress > 1)
            {
                Thread.Sleep(inProgress * 250);
            }
        }


        private void SendResponse(ResponseMessage Message, RequestMessage Request)
        {
            byte[] sendBytes = MessageEngine.GenerateSendBytes(Message, false);

            SocketAsyncEventArgs sendEventArgs;
            while (true == true)
            {
                if (Request.IsTimeout)
                {
                    //  TIMEOUT: NO POINT SENDING THE RESPONSE BECAUSE IT WILL HAVE ALSO TIMED OUT AT THE OTHER END
                    NotifyExceptionRaised(new TimeoutException("Request " + Request.RequestId + ", timed out after " + Request.TimeoutMilliseconds + " milliseconds."));
                    return;
                }
                else if (IsStopAllRequested)
                {
                    //  SHUTDOWN: CLIENT IS SHUTTING DOWN. A SHUTDOWN MESSAGE SHOULD HAVE ALREADY BEEN SENT SO EXIT
                    SendResponseQuickly(new ResponseMessage(Request.RequestId, RequestResult.Stopping));
                    return;
                }

                if (Message.Status == MessageStatus.Unsent && CanSendReceive() == true)
                {
                    sendEventArgs = _sendEventArgsPool.Pop();
                    if (sendEventArgs != null)
                    {
                        Message.Status = MessageStatus.InProgress;
                        sendEventArgs.UserToken = Message;
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


        private void SendResponseQuickly(ResponseMessage Message)
        {
            byte[] sendBytes = MessageEngine.GenerateSendBytes(Message, false);

            if (_connectionStatus != ConnectionStatuses.Connected || !_currentEndPoint.Socket.Connected) return;

            SocketAsyncEventArgs sendEventArgs = _sendEventArgsPool.Pop();
            if (sendEventArgs == null) return;

            sendEventArgs.UserToken = Message;
            sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
            Message.Status = MessageStatus.InProgress;
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
        /// Send a request to the server and wait for a response. 
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the request</param>
        /// <param name="TimeoutMilliseconds">Maximum number of milliseconds to wait for a response from the server</param>
        /// <param name="IsLongPolling">If the request is long polling on the server mark this as true and the request will be cancelled instantly when a disconnect occurs</param>
        /// <returns>Nullable array of bytes which was returned from the socket server</returns>
        public byte[] SendRequest(object[] Parameters, int TimeoutMilliseconds = 60000, bool IsLongPolling = false)
        {
            if (IsStopAllRequested) throw new Exception("Request cannot be sent. The socket client is stopped or stopping");
            if (Parameters == null) throw new ArgumentException("Request parameters cannot be null.", nameof(Parameters));
            if (Parameters.Length == 0) throw new ArgumentException("At least 1 request parameter is required.", nameof(Parameters));
            DateTime startTime = DateTime.Now;
            DateTime maxWait = startTime.AddMilliseconds(TimeoutMilliseconds);
            while (ConnectionStatus != ConnectionStatuses.Connected && IsStopAllRequested == false)
            {
                Thread.Sleep(200);
                if (IsStopAllRequested) throw new Exception("Request cannot be sent. The socket client is stopped or stopping");
                if (DateTime.Now > maxWait) throw new TimeoutException();
            }
            //DelaySending();
            int remainingMilliseconds = TimeoutMilliseconds - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
            return SendReceive(new RequestMessage(Parameters, remainingMilliseconds, IsLongPolling));
        }


        private byte[] SendReceive(RequestMessage Request)
        {
            if (IsStopAllRequested == true) return null;

            DateTime startTime = DateTime.Now;
            _openRequests.Add(Request);

            byte[] sendBytes = MessageEngine.GenerateSendBytes(Request, false);

            SocketAsyncEventArgs sendEventArgs;

            while (Request.TrySendReceive == true && IsStopAllRequested == false)
            {
                try
                {
                    if (Request.Status == MessageStatus.Unsent && CanSendReceive() == true)
                    {
                        sendEventArgs = _sendEventArgsPool.Pop();
                        if (sendEventArgs != null)
                        {
                            Request.Status = MessageStatus.InProgress;
                            sendEventArgs.UserToken = Request;
                            sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                            int remainingMilliseconds = Request.TimeoutMilliseconds - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);

                            if (remainingMilliseconds > 0)
                            {
                                sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;

                                if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs)) ProcessSend(null, sendEventArgs);

                                //  WAIT FOR RESPONSE
                                while (Request.WaitForResponse)
                                {
                                    Thread.Sleep(5);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }
                if (Request.TrySendReceive == true) Thread.Sleep(200);
            }

            _openRequests.Remove(Request);

            if (Request.Response != null)
            {
                if (Request.Response.Error != null) throw new Exception(Request.Response.Error);
                else return Request.Response.ResponseData;
            }
            else throw new TimeoutException("SendReceive() timed out after " + Request.TimeoutMilliseconds + " milliseconds");
        }


        private void ProcessSendPollRequest(object sender, SocketAsyncEventArgs e)
        {
            //_autoResetPollEvent.Set();
        }

        private void ProcessSendSubscriptionChanges(object sender, SocketAsyncEventArgs e)
        {
            //_autoResetSendSubscriptionChangesEvent.Set();
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
                    message.Status = MessageStatus.Unsent;
                    NotifyExceptionRaised(new Exception("Disconnecting: Connection was reset."));
                    DisconnectSocket();
                }
                else if (result != SocketError.Success)
                {
                    message.Status = MessageStatus.Unsent;
                    NotifyExceptionRaised(new Exception("Disconnecting: Send did not generate a success. Socket operation returned error code " + (int)e.SocketError));
                    DisconnectSocket();
                }
            }
            catch (Exception ex)
            {
                message.Status = MessageStatus.Unsent;
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
            if (ConnectionStatus == ConnectionStatuses.Disconnecting || ConnectionStatus == ConnectionStatuses.Disconnected)  return;

            if (e.BytesTransferred == 0 || e.SocketError != SocketError.Success)
            {
                NotifyExceptionRaised(new Exception("Disconnecting: ProcessReceive received socket error code " + (int)e.SocketError));
                DisconnectSocket();
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
                        if (_receiveEngine.MessageType == MessageTypes.ResponseMessage)
                        {
                            //  SyncEndPointSubscriptionsWithServer() IS WAITING. COMPLETE THE SYNCRONOUS OPERATION SO IT CAN CONTINUE
                            ResponseMessage response = _receiveEngine.GetResponseMessage();

                            //  CHECK TO SEE IS THE MESSAGE IS IN THE LIST OF OPEN SendReceive ITEMS.
                            RequestMessage foundOpenRequest = _openRequests[response.RequestId];
                            if (foundOpenRequest != null)
                            {
                                if (response.RequestResultCode == RequestResult.Stopping)
                                {
                                    //  SOCKET SERVER IS SHUTTING DOWN. WE WILL RETRY
                                    foundOpenRequest.Status = MessageStatus.Unsent;
                                }
                                else
                                {
                                    foundOpenRequest.Response = response;
                                    foundOpenRequest.Status = MessageStatus.Finished;
                                }
                            }
                        }
                        else if (_receiveEngine.MessageType == MessageTypes.Message)
                        {
                            NotifyMessageReceived(_receiveEngine.GetMessage());
                        }
                        else if (_receiveEngine.MessageType == MessageTypes.RequestMessageV1)
                        {
                            RequestMessage request = _receiveEngine.GetRequestMessage(1);
                            lock (_lock) { _requestsInProgress += 1; }
                            ThreadPool.QueueUserWorkItem(BgProcessRequestMessage, request);
                        }
                        else if (_receiveEngine.MessageType == MessageTypes.RequestMessageV2)
                        {
                            RequestMessage request = _receiveEngine.GetRequestMessage(2);
                            lock (_lock) { _requestsInProgress += 1; }
                            ThreadPool.QueueUserWorkItem(BgProcessRequestMessage, request);
                        }
                        else if (_receiveEngine.MessageType == MessageTypes.ServerStoppingMessage)
                        {
                            NotifyServerStopping();
                            DisconnectSocket();
                        }
                        else if (_receiveEngine.MessageType == MessageTypes.PollResponse)
                        {
                            LastPollResponse = DateTime.Now;
                        }

                    }
                }

                //  KEEP RECEIVING
                if (CanSendReceive() == true && !CurrentEndPoint.Socket.ReceiveAsync(e))
                {
                    ProcessReceive(null, e);
                }
            }
            catch (ObjectDisposedException ee)
            {
                //  IF A LARGE CHUNK OF DATA WAS BEING RECEIVED WHEN THE CONNECTION WAS LOST, THE Disconnect() ROUTINE
                //  MAY ALREADY HAVE BEEN RUN (WHICH DISPOSES OBJECTS). IF THIS IS THE CASE, SIMPLY EXIT
                NotifyExceptionRaised(new Exception("Disconnecting: ObjectDisposedException running ProcessReceive: " + ee.Message));
                DisconnectSocket();
            }
            catch (Exception ex)
            {
                NotifyExceptionRaised(new Exception("Disconnecting: Error running ProcessReceive: " + ex.Message));
                DisconnectSocket();
            }
        }


        private void BgProcessRequestMessage(object state)
        {
            RequestMessage request = (RequestMessage)state;
            try
            {
                if (RequestReceived == null)
                {
                    //  THERE IS NO CODE LISTENING TO RequestReceive EVENTS. CANNOT PROCESS THIS REQUEST
                    SendResponseQuickly(new ResponseMessage(request.RequestId, RequestResult.NoRequestProcessor));
                }
                else if (IsStopAllRequested)
                {
                    //  SHUTDOWN: CLIENT IS SHUTTING DOWN. A SHUTDOWN MESSAGE SHOULD HAVE ALREADY BEEN SENT SO EXIT
                    SendResponseQuickly(new ResponseMessage(request.RequestId, RequestResult.Stopping));
                }
                else if (request.IsTimeout)
                {
                    //  IF TIMEOUT, NO POINT SENDING THE RESPONSE
                    NotifyExceptionRaised(new TimeoutException("Request " + request.RequestId + ", timed out after " + request.TimeoutMilliseconds + " milliseconds."));
                }
                else
                {
                    //  DESERIALIZE THE REQUEST FROM THE CLIENT
                    //  WE HAVE A MESSAGE IN FULL. UNPACK, (RESETS COUNTERS) AND RAISE AN EVENT
                    RequestReceivedEventArgs args = new RequestReceivedEventArgs(request.Parameters);
                    RequestReceived(this, args);

                    //  SEND RESPONSE
                    ResponseMessage response = new ResponseMessage(request.RequestId, args.Response);
                    SendResponse(response, request);
                }
            }
            catch (Exception ex)
            {
                NotifyExceptionRaised(ex);
                SendResponseQuickly(new ResponseMessage(request.RequestId, ex));
            }
            finally
            {
                lock (_lock) { _requestsInProgress -= 1; }
            }
        }




        #endregion

        #region Shared


        private bool CanExecuteBackgroundOperation()
        {
            lock (_lock)
            {
                if (_isStopAllRequested == true || _isStopBackgroundOperationsRequested == true) return false;
                if (_connectionStatus != ConnectionStatuses.Connected) return false;
                return _currentEndPoint.Socket.Connected;
            }
        }


        private bool CanSendReceive()
        {
            lock (_lock)
            {
                if (_isStopAllRequested == true) return false;
                if (_connectionStatus != ConnectionStatuses.Connected) return false;
                return _currentEndPoint.Socket.Connected;
            }
        }


        private void NotifyExceptionRaised(Exception ex)
        {
            if (ExceptionRaised != null)
            {
                //  RAISE EVENT IN THE BACKGROUND
                new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        ExceptionRaised(this, new ExceptionEventArgs(ex, 1234));
                    }
                    catch
                    {
                    }
                }
                )).Start();
            }
        }


        private void NotifyMessageReceived(Messages.Message Message)
        {
            if (MessageReceived != null)
            {
                //  RAISE EVENT IN THE BACKGROUND
                new Thread(new ThreadStart(delegate
                {
                    try 
                    { 
                        MessageReceived(this, new MessageReceivedEventArgs(Message.Parameters)); 
                    }
                    catch (Exception ex) 
                    { 
                        NotifyExceptionRaised(ex); 
                    }
                }
                )).Start();

            }
        }

        private void NotifyServerStopping()
        {
            if (ServerStopping != null)
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
            }
        }



        #endregion
    }

#pragma warning restore IDE0017 // Simplify object initialization
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1303 // Do not pass literals as localized parameters

}
