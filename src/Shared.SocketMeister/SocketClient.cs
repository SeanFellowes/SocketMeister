using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketMeister.Base;
using SocketMeister.Messages;


namespace SocketMeister
{
    /// <summary>
    /// Asynchronous, persistent TCP/IP socket client supporting multiple destinations
    /// </summary>
    public partial class SocketClient : IDisposable
    {
        /// <summary>
        /// The number of simultaneous send operations which can take place. Value should be between 2 and 15
        /// </summary>
        private readonly static int CLIENT_SEND_EVENT_ARGS_POOL_SIZE = 10;

        /// <summary>
        /// If a poll response has not been received from the server after a number of seconds, the socketet client will be disconnected.
        /// </summary>
        private readonly static int DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS = 30;

        /// <summary>
        /// When a shutdown occurs, particularly because of network failure or server shutdown, delay attempting to reconnect to that server, giving the server some time to complete it's shutdown process.
        /// </summary>
        private readonly static int DONT_RECONNECT_DELAY_AFTER_SHUTDOWN = 15;

        /// <summary>
        /// The frequency, in seconds, that this client will poll the server, to ensure the socket is alive.
        /// </summary>
        private readonly static int POLLING_FREQUENCY = 10;

        /// <summary>
        /// The buffer size to use for sending and receiving data. Note: This value is also used by the 'SocketServer' class.
        /// </summary>
        internal readonly static int SEND_RECEIVE_BUFFER_SIZE = 65536;

        private SocketAsyncEventArgs _asyncEventArgsConnect = null;
        private SocketAsyncEventArgs _asyncEventArgsPolling = null;
        private SocketAsyncEventArgs _asyncEventArgsReceive = null;
        private readonly ManualResetEvent _autoResetConnectEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _autoResetPollEvent = new ManualResetEvent(false);
        private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;
        private SocketEndPoint _currentEndPoint = null;
        private bool _enableCompression;
        private readonly List<SocketEndPoint> _endPoints = null;
        private bool _isBackgroundConnectRunning;
        private bool _isBackgroundPollingRunning;
        private bool _isStopAllRequested = false;
        private bool _isStopPollingRequested = false;
        private DateTime _lastPollResponse = DateTime.Now;
        private readonly object _lock = new object();
        private DateTime _nextPollRequest;
        private readonly OpenRequestMessages _openRequests = new OpenRequestMessages();
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
        internal event EventHandler<MessageReceivedEventArgs> MessageReceived;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="EndPoints">Collection of endpoints that are available to connect to</param>
        /// <param name="EnableCompression">Whether compression will be applied to data.</param>
        public SocketClient(List<SocketEndPoint> EndPoints, bool EnableCompression)
        {
            if (EndPoints == null) throw new ArgumentNullException("EndPoints");
            else if (EndPoints.Count == 0) throw new ArgumentException("EndPoints");

            _enableCompression = EnableCompression;
            _receiveEngine = new MessageEngine(EnableCompression);

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
            _sendEventArgsPool = new SocketAsyncEventArgsPool(CLIENT_SEND_EVENT_ARGS_POOL_SIZE);
            for (int i = 0; i < CLIENT_SEND_EVENT_ARGS_POOL_SIZE; i++)
            {
                SocketAsyncEventArgs eArgs = new SocketAsyncEventArgs();
                eArgs.SetBuffer(new byte[SEND_RECEIVE_BUFFER_SIZE], 0, SEND_RECEIVE_BUFFER_SIZE);
                eArgs.Completed += ProcessSend;
                _sendEventArgsPool.Push(eArgs);
            }

            _asyncEventArgsConnect = new SocketAsyncEventArgs();
            _asyncEventArgsConnect.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessConnect);

            _asyncEventArgsPolling = new SocketAsyncEventArgs();
            _asyncEventArgsPolling.SetBuffer(new byte[SEND_RECEIVE_BUFFER_SIZE], 0, SEND_RECEIVE_BUFFER_SIZE);
            _asyncEventArgsPolling.Completed += ProcessSendPollRequest;

            bgConnectToServer();
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
                _autoResetPollEvent.Close();
                if (_asyncEventArgsConnect != null) _asyncEventArgsConnect.Dispose();
                _asyncEventArgsConnect = null;
                if (_asyncEventArgsPolling != null) _asyncEventArgsPolling.Dispose();
                _asyncEventArgsPolling = null;
                if (_asyncEventArgsReceive != null) _asyncEventArgsReceive.Dispose();
                _asyncEventArgsReceive = null;
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
        /// The connection status of the socket client
        /// </summary>
        public ConnectionStatus ConnectionStatus
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
                    if (value != ConnectionStatus.Connected) ConnectionStatusChanged(this, new ConnectionStatusChangedEventArgs(value, "", 0));
                    else ConnectionStatusChanged(this, new ConnectionStatusChangedEventArgs(value, CurrentEndPoint.IPAddress, CurrentEndPoint.Port));
                }
            }
        }

        private SocketEndPoint CurrentEndPoint
        {
            get { lock (_lock) { return _currentEndPoint; } }
            set { lock (_lock) { _currentEndPoint = value; } }
        }

        private bool IsBackgroundConnectRunning { get { lock (_lock) { return _isBackgroundConnectRunning; } } set { lock (_lock) { _isBackgroundConnectRunning = value; } } }

        private bool IsBackgroundPollingRunning { get { lock (_lock) { return _isBackgroundPollingRunning; } } set { lock (_lock) { _isBackgroundPollingRunning = value; } } }

        private bool IsStopAllRequested { get { lock (_lock) { return _isStopAllRequested; } } set { lock (_lock) { _isStopAllRequested = value; } } }

        private bool IsStopPollingRequested { get { lock (_lock) { return _isStopPollingRequested; } } set { lock (_lock) { _isStopPollingRequested = value; } } }

        /// <summary>
        /// The last time a polling response was received from the socket server.
        /// </summary>
        private DateTime LastPollResponse { get { lock (_lock) { return _lastPollResponse; } } set { lock (_lock) { _lastPollResponse = value; } } }

        /// <summary>
        /// The next time this socket client should attempt to poll the socket server.
        /// </summary>
        private DateTime NextPollRequest { get { lock (_lock) { return _nextPollRequest; } } set { lock (_lock) { _nextPollRequest = value; } } }




        #region Socket async connect


        /// <summary>
        /// Disconnect the socket. Note: This is performed in the background.
        /// </summary>
        private void DisconnectSocket()
        {
            if (ConnectionStatus == ConnectionStatus.Disconnecting || ConnectionStatus == ConnectionStatus.Disconnected) return;

            //  INITIATE SHUTDOWN
            SocketEndPoint disconnectingEndPoint = CurrentEndPoint;
            disconnectingEndPoint.DontReconnectUntil = DateTime.Now.AddSeconds(DONT_RECONNECT_DELAY_AFTER_SHUTDOWN);
            ConnectionStatus = ConnectionStatus.Disconnecting;

            Thread bgDisconnect = new Thread(
            new ThreadStart(delegate
            {
                //  STOP POLLING
                IsStopPollingRequested = true;
                _autoResetPollEvent.Set();

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
                ConnectionStatus = ConnectionStatus.Disconnected;
                if (IsStopAllRequested == false) bgConnectToServer();

            }));
            bgDisconnect.IsBackground = true;
            bgDisconnect.Start();
        }




        /// <summary>
        /// Background process which creates a connection with one of the servers specified
        /// </summary>
        private void bgConnectToServer()
        {
            lock (_lock)
            {
                if (_isBackgroundConnectRunning == true) return;
                _isBackgroundConnectRunning = true;
            }
            ConnectionStatus = ConnectionStatus.Connecting;

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

                            if (ConnectionStatus == ConnectionStatus.Connected)
                            {
                                bgPollServer();
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
        /// Background process which polls the server to determine if the socket is alive
        /// </summary>
        private void bgPollServer()
        {
            Thread bgPolling = new Thread(new ThreadStart(delegate
            {
                lock (_lock)
                {
                    if (_isBackgroundPollingRunning == true) return;
                    _isBackgroundPollingRunning = true;
                    _isStopPollingRequested = false;
                    _lastPollResponse = DateTime.Now;
                    _nextPollRequest = DateTime.Now;
                }

                while (IsStopPollingRequested == false)
                {
                    if (DateTime.Now > NextPollRequest && CanPoll())
                    {
                        try
                        {
                            NextPollRequest = DateTime.Now.AddSeconds(POLLING_FREQUENCY);
                            byte[] sendBytes = MessageEngine.GenerateSendBytes(new PollRequest(), false);
                            _asyncEventArgsPolling.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                            _asyncEventArgsPolling.SetBuffer(sendBytes, 0, sendBytes.Length);
                            if (!CurrentEndPoint.Socket.SendAsync(_asyncEventArgsPolling)) ProcessSendPollRequest(null, _asyncEventArgsPolling);
                            _autoResetPollEvent.Reset();
                            _autoResetPollEvent.WaitOne();
                        }
                        catch { }
                    }

                    if (LastPollResponse < (DateTime.Now.AddSeconds(-DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS)))
                    {
                        NotifyExceptionRaised(new Exception("Disconnecting: Server failed to reply to polling after " + DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS + " seconds."));
                        DisconnectSocket();
                        break;
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
                    _asyncEventArgsReceive.SetBuffer(new byte[SEND_RECEIVE_BUFFER_SIZE], 0, SEND_RECEIVE_BUFFER_SIZE);
                    _asyncEventArgsReceive.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessReceive);
                    if (!CurrentEndPoint.Socket.ReceiveAsync(_asyncEventArgsReceive)) ProcessReceive(null, _asyncEventArgsReceive);
                    //  CONNECTED
                    ConnectionStatus = ConnectionStatus.Connected;
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
            while (ConnectionStatus != ConnectionStatus.Disconnected) { Thread.Sleep(50); }
        }


        private void DelaySending()
        {
            int inProgress = 0;
            lock (_lock) { inProgress = _openRequests.Count; }
            if (inProgress > 1)
            {
                Thread.Sleep(inProgress * 250);
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
            if (Parameters == null) throw new ArgumentException("Request parameters cannot be null.", "Parameters");
            if (Parameters.Length == 0) throw new ArgumentException("At least 1 request parameter is required.", "Parameters");
            DateTime startTime = DateTime.Now;
            DateTime maxWait = startTime.AddMilliseconds(TimeoutMilliseconds);
            while(ConnectionStatus != ConnectionStatus.Connected && IsStopAllRequested == false)
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

            DateTime nowTs = DateTime.Now;
            _openRequests.Add(Request);

            byte[] sendBytes = MessageEngine.GenerateSendBytes(Request, false);

            SocketAsyncEventArgs sendEventArgs = null;

            while (true == true)
            {
                try
                {
                    if (IsStopAllRequested == true) return null;

                    if (Request.SendReceiveStatus == SendReceiveStatus.Unsent && CanSendReceive() == true)
                    {
                        sendEventArgs = _sendEventArgsPool.Pop();
                        if (sendEventArgs != null)
                        {
                            sendEventArgs.UserToken = Request;
                            sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                            Request.SendReceiveStatus = SendReceiveStatus.InProgress;
                            int maxWait = Convert.ToInt32(Request.TimeoutMilliseconds - (DateTime.Now - nowTs).TotalMilliseconds);

                            if (maxWait > 0)
                            {
                                sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;

                                if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs)) ProcessSend(null, sendEventArgs);

                                //  WAIT FOR RESPONSE
                                while (Request.SendReceiveStatus == SendReceiveStatus.InProgress)
                                {
                                    Thread.Sleep(5);
                                }
                            }
                        }
                    }

                    if (Request.SendReceiveStatus == SendReceiveStatus.ResponseReceived || Request.SendReceiveStatus == SendReceiveStatus.Timeout) break;
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }
                Thread.Sleep(200);
            }

            _openRequests.Remove(Request);

            if (Request.SendReceiveStatus == SendReceiveStatus.ResponseReceived)
            {
                if (Request.Response.Error != null) throw new Exception(Request.Response.Error);
                else return Request.Response.ResponseData;
            }
            else throw new TimeoutException();
        }


        private void ProcessSendPollRequest(object sender, SocketAsyncEventArgs e)
        {
            _autoResetPollEvent.Set();
        }


        //  CALLED AFTER SendAsync COMPLETES
        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            IMessage tokenSent = (IMessage)e.UserToken;
            SocketError result = e.SocketError;
            RecycleSocketAsyncEventArgs(e);

            try
            {
                if (result == SocketError.ConnectionReset)
                {
                    NotifyExceptionRaised(new Exception("Disconnecting: Connection was reset."));
                    DisconnectSocket();
                }
                else if (result != SocketError.Success)
                {
                    NotifyExceptionRaised(new Exception("Disconnecting: Send did not generate a success. Socket operation returned error code " + (int)e.SocketError));
                    DisconnectSocket();
                }
                else
                {
                    tokenSent.SendReceiveStatus = SendReceiveStatus.InProgress;
                }
            }
            catch (Exception ex)
            {
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
            //if (e.BytesTransferred == 0 || e.SocketError != SocketError.Success)
            //{
            //    if (e.BytesTransferred == 0) NotifyExceptionRaised(new Exception("Disconnecting: ProcessReceive returned 0 bytes."));
            //    else NotifyExceptionRaised(new Exception("Disconnecting: ProcessReceive received socket error code " + (int)e.SocketError));
            //    DisconnectSocket();
            //    return;
            //}
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
                            RequestMessage foundOpenRequest = _openRequests.Find(response.RequestId);
                            if (foundOpenRequest != null)
                            {
                                if (response.ServerIsStopping == true)
                                {
                                    foundOpenRequest.SendReceiveStatus = SendReceiveStatus.Unsent;
                                }
                                else
                                {
                                    foundOpenRequest.Response = response;
                                    foundOpenRequest.SendReceiveStatus = SendReceiveStatus.ResponseReceived;
                                }
                            }
                        }
                        else if (_receiveEngine.MessageType == MessageTypes.Message)
                        {
                            NotifyMessageReceived(_receiveEngine.GetMessage());
                        }
                        else if (_receiveEngine.MessageType == MessageTypes.ServerStoppingMessage)
                        {
                            NotifyExceptionRaised(new Exception("Disconnecting: Server is stopping."));
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



        #endregion

        #region Shared


        private bool CanPoll()
        {
            lock (_lock)
            {
                if (_isStopAllRequested == true || _isStopPollingRequested == true) return false;
                if (_connectionStatus != ConnectionStatus.Connected) return false;
                return _currentEndPoint.Socket.Connected;
            }
        }


        private bool CanSendReceive()
        {
            lock (_lock)
            {
                if (_isStopAllRequested == true) return false;
                if (_connectionStatus != ConnectionStatus.Connected) return false;
                return _currentEndPoint.Socket.Connected;
            }
        }


        private void NotifyExceptionRaised(Exception ex)
        {
            try { ExceptionRaised?.Invoke(this, new ExceptionEventArgs(ex, 1234)); }
            catch { }
        }


        private void NotifyMessageReceived(Messages.Message Message)
        {
            if (MessageReceived != null)
            {
                //  RAISE EVENT IN THE BACKGROUND
                new Thread(new ThreadStart(delegate
                {
                    try { MessageReceived(this, new MessageReceivedEventArgs(Message.Parameters)); }
                    catch (Exception ex) { NotifyExceptionRaised(ex); }
                }
                )).Start();

            }
        }


        #endregion
    }
}
