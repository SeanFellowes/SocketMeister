using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
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
        /// The number of simultaneous send operations which can take place. Value should be between 2 and 15
        /// </summary>
        private const int CLIENT_SEND_EVENT_ARGS_POOL_SIZE = 10;

        /// <summary>
        /// If a poll response has not been received from the server after a number of seconds, the socketet client will be disconnected.
        /// </summary>
        private const int DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS = 30;

        /// <summary>
        /// When a shutdown occurs, particularly because of network failure or server shutdown, delay attempting to reconnect to that server, giving the server some time to complete it's shutdown process.
        /// </summary>
        private const int DONT_RECONNECT_DELAY_AFTER_SHUTDOWN = 15;

        /// <summary>
        /// The frequency, in seconds, that this client will poll the server, to ensure the socket is alive.
        /// </summary>
        private const int POLLING_FREQUENCY = 10;

        /// <summary>
        /// The buffer size to use for sending and receiving data. Note: This value is also used by the 'SocketServer' class.
        /// </summary>
        internal const int SEND_RECEIVE_BUFFER_SIZE = 65536;

        private SocketAsyncEventArgs asyncEventArgsConnect = null;
        private SocketAsyncEventArgs asyncEventArgsPolling = null;
        private SocketAsyncEventArgs asyncEventArgsReceive = null;
        private readonly ManualResetEvent autoResetConnectEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent autoResetPollEvent = new ManualResetEvent(false);
        private readonly object classLock = new object();
        private ConnectionStatuses connectionStatus = ConnectionStatuses.Disconnected;
        private SocketEndPoint currentEndPoint = null;
        private readonly List<SocketEndPoint> endPoints = null;
        private bool isBackgroundConnectRunning;
        private bool isBackgroundPollingRunning;
        private bool isStopAllRequested = false;
        private bool isStopPollingRequested = false;
        private DateTime lastPollResponse = DateTime.Now;
        private DateTime nextPollRequest;
        private readonly OpenRequestMessages openRequests = new OpenRequestMessages();
        private readonly Random randomizer = new Random();
        private MessageEngine receiveEngine;
        private readonly SocketAsyncEventArgsPool sendEventArgsPool;

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Trace message raised from this socket client.
        /// </summary>
        public event EventHandler<TraceEventArgs> TraceEventRaised;

        /// <summary>
        /// Event raised whenever a message is received from the server.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="endPoints">Collection of endpoints that are available to connect to</param>
        public SocketClient(List<SocketEndPoint> endPoints)
        {
            if (endPoints == null) throw new ArgumentNullException(nameof(endPoints));
            else if (endPoints.Count == 0) throw new ArgumentException("No end points were provided", nameof(endPoints));

            receiveEngine = new MessageEngine();

            //  SETUP ENDPOINTS AND CHOOSE THE ENDPOINT TO START WITH
            this.endPoints = endPoints;
            if (this.endPoints.Count == 1)
            {
                CurrentEndPoint = this.endPoints[0];
            }
            else
            {
                int loopCnt = randomizer.Next(20);
                int pointer = 0;
                for (int a = 0; a < loopCnt; a++)
                {
                    pointer = randomizer.Next(this.endPoints.Count);
                }
                pointer = randomizer.Next(this.endPoints.Count);
                CurrentEndPoint = this.endPoints[pointer];
                //  ENSURE THIS ENDPOINT IS SELECTED FIRST (Must have the lowest DontReconnectUntil)
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddYears(-1);
            }

            //  PREALLOCATE A POOL OF SocketAsyncEventArgs FOR SENDING
            sendEventArgsPool = new SocketAsyncEventArgsPool(CLIENT_SEND_EVENT_ARGS_POOL_SIZE);
            for (int i = 0; i < CLIENT_SEND_EVENT_ARGS_POOL_SIZE; i++)
            {
                SocketAsyncEventArgs eArgs = new SocketAsyncEventArgs();
                eArgs.SetBuffer(new byte[SEND_RECEIVE_BUFFER_SIZE], 0, SEND_RECEIVE_BUFFER_SIZE);
                eArgs.Completed += ProcessSend;
                sendEventArgsPool.Push(eArgs);
            }

            asyncEventArgsConnect = new SocketAsyncEventArgs();
            asyncEventArgsConnect.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessConnect);

            asyncEventArgsPolling = new SocketAsyncEventArgs();
            asyncEventArgsPolling.SetBuffer(new byte[SEND_RECEIVE_BUFFER_SIZE], 0, SEND_RECEIVE_BUFFER_SIZE);
            asyncEventArgsPolling.Completed += ProcessSendPollRequest;

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
                currentEndPoint.Dispose();
                autoResetConnectEvent.Close();
                autoResetPollEvent.Close();
                if (asyncEventArgsConnect != null) asyncEventArgsConnect.Dispose();
                asyncEventArgsConnect = null;
                if (asyncEventArgsPolling != null) asyncEventArgsPolling.Dispose();
                asyncEventArgsPolling = null;
                if (asyncEventArgsReceive != null) asyncEventArgsReceive.Dispose();
                asyncEventArgsReceive = null;
                receiveEngine = null; ;
                foreach (SocketEndPoint ep in endPoints)
                {
                    ep.CloseSocket(); 
                }
            }
        }


        /// <summary>
        /// The connection status of the socket client
        /// </summary>
        public ConnectionStatuses ConnectionStatus
        {
            get { lock (classLock) { return connectionStatus; } }
            private set
            {
                lock (classLock)
                {
                    if (connectionStatus == value) return;
                    connectionStatus = value;
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
            get { lock (classLock) { return currentEndPoint; } }
            set { lock (classLock) { currentEndPoint = value; } }
        }

        private bool IsBackgroundConnectRunning { get { lock (classLock) { return isBackgroundConnectRunning; } } set { lock (classLock) { isBackgroundConnectRunning = value; } } }

        private bool IsBackgroundPollingRunning { get { lock (classLock) { return isBackgroundPollingRunning; } } set { lock (classLock) { isBackgroundPollingRunning = value; } } }

        private bool IsStopAllRequested { get { lock (classLock) { return isStopAllRequested; } } set { lock (classLock) { isStopAllRequested = value; } } }

        private bool IsStopPollingRequested { get { lock (classLock) { return isStopPollingRequested; } } set { lock (classLock) { isStopPollingRequested = value; } } }

        /// <summary>
        /// The last time a polling response was received from the socket server.
        /// </summary>
        private DateTime LastPollResponse { get { lock (classLock) { return lastPollResponse; } } set { lock (classLock) { lastPollResponse = value; } } }

        /// <summary>
        /// The next time this socket client should attempt to poll the socket server.
        /// </summary>
        private DateTime NextPollRequest { get { lock (classLock) { return nextPollRequest; } } set { lock (classLock) { nextPollRequest = value; } } }




        #region Socket async connect


        /// <summary>
        /// Disconnect the socket. Note: This is performed in the background.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
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
                //  STOP POLLING
                IsStopPollingRequested = true;
                autoResetPollEvent.Set();

                //  CLOSE OPEN REQUESTS
                openRequests.ResetToUnsent();

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
                        catch (Exception ex)
                        {
                            TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1234));
                        }
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
                    if (asyncEventArgsReceive != null)
                    {
                        asyncEventArgsReceive.Completed -= new EventHandler<SocketAsyncEventArgs>(ProcessSend);
                        asyncEventArgsReceive.Dispose();
                        asyncEventArgsReceive = null;
                    }
                }
                catch (Exception ex)
                {
                    TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1234));
                }

                //  CLOSE OPEN REQUESTS AGAIN!!! UNDER LOAD THE CLIENT CAN SUBMIT A REQUEST (BECAUSE OF CROSS THREADING)
                openRequests.ResetToUnsent();

                //  FINALIZE AND RE-ATTEMPT CONNECTION IS WE ARE NOT STOPPING
                ConnectionStatus = ConnectionStatuses.Disconnected;
                if (IsStopAllRequested == false) BgConnectToServer();

            }))
            {
                IsBackground = true
            };
            bgDisconnect.Start();
        }




        /// <summary>
        /// Background process which creates a connection with one of the servers specified
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void BgConnectToServer()
        {
            lock (classLock)
            {
                if (isBackgroundConnectRunning == true) return;
                isBackgroundConnectRunning = true;
            }
            ConnectionStatus = ConnectionStatuses.Connecting;

            Thread bgConnect = new Thread(new ThreadStart(delegate
            {
                while (IsStopAllRequested == false)
                {
                    try
                    {
                        //  CHOOSE THE NEXT ENDPOINT TO TRY
                        if (endPoints.Count > 1)
                        {
                            SocketEndPoint bestEP = endPoints[0];
                            for (int i = 1; i < endPoints.Count; i++)
                            {
                                if (endPoints[i].DontReconnectUntil < bestEP.DontReconnectUntil) bestEP = endPoints[i];
                            }
                            CurrentEndPoint = bestEP;
                        }

                        if (CurrentEndPoint.DontReconnectUntil < DateTime.Now)
                        {
                            //  TRY TO CONNECT
                            asyncEventArgsConnect.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                            if (!CurrentEndPoint.Socket.ConnectAsync(asyncEventArgsConnect)) ProcessConnect(null, asyncEventArgsConnect);
                            autoResetConnectEvent.Reset();
                            autoResetConnectEvent.WaitOne(5000);

                            if (ConnectionStatus == ConnectionStatuses.Connected)
                            {
                                BgPollServer();
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1234));
                    }
                    Thread.Sleep(500);
                }
                IsBackgroundConnectRunning = false;
            }))
            {
                IsBackground = true
            };
            bgConnect.Start();
        }


        /// <summary>
        /// Background process which polls the server to determine if the socket is alive
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void BgPollServer()
        {
            Thread bgPolling = new Thread(new ThreadStart(delegate
            {
                lock (classLock)
                {
                    if (isBackgroundPollingRunning == true) return;
                    isBackgroundPollingRunning = true;
                    isStopPollingRequested = false;
                    lastPollResponse = DateTime.Now;
                    nextPollRequest = DateTime.Now;
                }

                while (IsStopPollingRequested == false)
                {
                    if (DateTime.Now > NextPollRequest && CanPoll())
                    {
                        try
                        {
                            NextPollRequest = DateTime.Now.AddSeconds(POLLING_FREQUENCY);
                            byte[] sendBytes = MessageEngine.GenerateSendBytes(new PollRequest(), false);
                            asyncEventArgsPolling.RemoteEndPoint = CurrentEndPoint.IPEndPoint;
                            asyncEventArgsPolling.SetBuffer(sendBytes, 0, sendBytes.Length);
                            if (!CurrentEndPoint.Socket.SendAsync(asyncEventArgsPolling)) ProcessSendPollRequest(null, asyncEventArgsPolling);
                            autoResetPollEvent.Reset();
                            autoResetPollEvent.WaitOne();
                        }
                        catch (Exception ex)
                        {
                            TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1234));
                        }
                    }

                    if (LastPollResponse < (DateTime.Now.AddSeconds(-DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS)))
                    {
                        TraceEventRaised?.Invoke(this, new TraceEventArgs("Disconnecting: Server failed to reply to polling after " + DISCONNECT_AFTER_NO_POLL_RESPONSE_SECONDS + " seconds.", SeverityType.Warning, 1234));
                        DisconnectSocket();
                        break;
                    }

                    Thread.Sleep(200);
                }
                IsBackgroundPollingRunning = false;
            }))
            {
                IsBackground = true
            };
            bgPolling.Start();
        }



        /// <summary>
        /// Connect operation has completed
        /// </summary>
        /// <param name="sender">Sending Socket</param>
        /// <param name="e">Socket Arguments</param>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void ProcessConnect(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                //  ATTEMPT TO START RECEIVING
                try
                {
                    asyncEventArgsReceive = new SocketAsyncEventArgs();
                    asyncEventArgsReceive.SetBuffer(new byte[SEND_RECEIVE_BUFFER_SIZE], 0, SEND_RECEIVE_BUFFER_SIZE);
                    asyncEventArgsReceive.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessReceive);
                    if (!CurrentEndPoint.Socket.ReceiveAsync(asyncEventArgsReceive)) ProcessReceive(null, asyncEventArgsReceive);
                    //  CONNECTED
                    ConnectionStatus = ConnectionStatuses.Connected;
                    //  DONE
                    autoResetConnectEvent.Set();
                }
                catch (Exception ex)
                {
                    autoResetConnectEvent.Set();
                    TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1234));
                }
            }
            else if (e.SocketError == SocketError.TimedOut)
            {
                //  NOTE: WHEN FAILING OVER UNDER HIGH LOAD, SocketError.TimedOut OCCURS FOR UP TO 120 SECONDS (WORSE CASE)
                //  BEFORE CONNECTION SUCCESSFULLY COMPLETES. IT'S A BIT ANNOYING BUT I HAVE FOUND NO WORK AROUND.
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(2000 + randomizer.Next(4000));
                autoResetConnectEvent.Set();
            }
            else
            {
                CurrentEndPoint.DontReconnectUntil = DateTime.Now.AddMilliseconds(2000 + randomizer.Next(4000));
                autoResetConnectEvent.Set();
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
            autoResetConnectEvent.Set();
            while (IsBackgroundConnectRunning == true) { Thread.Sleep(5); }

            //  SHUTDOWN SOCKET
            DisconnectSocket();

            //  WAIT UNTIL DISCONNECT HAS FINISHED
            while (ConnectionStatus != ConnectionStatuses.Disconnected) { Thread.Sleep(50); }
        }


        /// <summary>
        /// Send a request to the server and wait for a response. 
        /// </summary>
        /// <param name="parameters">Array of parameters to send with the request</param>
        /// <param name="timeoutMilliseconds">Maximum number of milliseconds to wait for a response from the server</param>
        /// <param name="isLongPolling">If the request is long polling on the server mark this as true and the request will be cancelled instantly when a disconnect occurs</param>
        /// <returns>Nullable array of bytes which was returned from the socket server</returns>
        public byte[] SendRequest(object[] parameters, int timeoutMilliseconds = 60000, bool isLongPolling = false)
        {
            if (IsStopAllRequested) throw new Exception("Request cannot be sent. The socket client is stopped or stopping");
            if (parameters == null) throw new ArgumentException("Request parameters cannot be null.", nameof(parameters));
            if (parameters.Length == 0) throw new ArgumentException("At least 1 request parameter is required.", nameof(parameters));
            DateTime startTime = DateTime.Now;
            DateTime maxWait = startTime.AddMilliseconds(timeoutMilliseconds);
            while(ConnectionStatus != ConnectionStatuses.Connected && IsStopAllRequested == false)
            {
                Thread.Sleep(200);
                if (IsStopAllRequested) throw new Exception("Request cannot be sent. The socket client is stopped or stopping");
                if (DateTime.Now > maxWait) throw new TimeoutException();
            }
            //DelaySending();
            int remainingMilliseconds = timeoutMilliseconds - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
            return SendReceive(new RequestMessage(parameters, remainingMilliseconds, isLongPolling));
        }


        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private byte[] SendReceive(RequestMessage request)
        {
            if (IsStopAllRequested == true) return null;

            DateTime nowTs = DateTime.Now;
            openRequests.Add(request);

            byte[] sendBytes = MessageEngine.GenerateSendBytes(request, false);
            while (true == true)
            {
                try
                {
                    if (IsStopAllRequested == true) return null;

                    if (request.SendReceiveStatus == SendReceiveStatus.Unsent && CanSendReceive() == true)
                    {
                        SocketAsyncEventArgs sendEventArgs = sendEventArgsPool.Pop();
                        if (sendEventArgs != null)
                        {
                            sendEventArgs.UserToken = request;
                            sendEventArgs.SetBuffer(sendBytes, 0, sendBytes.Length);
                            request.SendReceiveStatus = SendReceiveStatus.InProgress;
                            int maxWait = Convert.ToInt32(request.TimeoutMilliseconds - (DateTime.Now - nowTs).TotalMilliseconds);

                            if (maxWait > 0)
                            {
                                sendEventArgs.RemoteEndPoint = CurrentEndPoint.IPEndPoint;

                                if (!CurrentEndPoint.Socket.SendAsync(sendEventArgs)) ProcessSend(null, sendEventArgs);

                                //  WAIT FOR RESPONSE
                                while (request.SendReceiveStatus == SendReceiveStatus.InProgress)
                                {
                                    Thread.Sleep(5);
                                }
                            }
                        }
                    }

                    if (request.SendReceiveStatus == SendReceiveStatus.ResponseReceived || request.SendReceiveStatus == SendReceiveStatus.Timeout) break;
                }
                catch (Exception ex)
                {
                    TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1234));
                }
                Thread.Sleep(200);
            }

            openRequests.Remove(request);

            if (request.SendReceiveStatus == SendReceiveStatus.ResponseReceived)
            {
                if (request.Response.Error != null) throw new Exception(request.Response.Error);
                else return request.Response.ResponseData;
            }
            else throw new TimeoutException();
        }


        private void ProcessSendPollRequest(object sender, SocketAsyncEventArgs e)
        {
            autoResetPollEvent.Set();
        }


        //  CALLED AFTER SendAsync COMPLETES
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            IMessage tokenSent = (IMessage)e.UserToken;
            SocketError result = e.SocketError;
            RecycleSocketAsyncEventArgs(e);

            try
            {
                if (result == SocketError.ConnectionReset)
                {
                    TraceEventRaised?.Invoke(this, new TraceEventArgs("Disconnecting: Connection was reset.", SeverityType.Warning, 11000));
                    DisconnectSocket();
                }
                else if (result != SocketError.Success)
                {
                    TraceEventRaised?.Invoke(this, new TraceEventArgs("Disconnecting: Send did not generate a success. Socket operation returned error code " + (int)e.SocketError, SeverityType.Warning, 11000));
                    DisconnectSocket();
                }
                else
                {
                    tokenSent.SendReceiveStatus = SendReceiveStatus.InProgress;
                }
            }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this,  new TraceEventArgs(ex, 11001));
            }
        }

        private void RecycleSocketAsyncEventArgs(SocketAsyncEventArgs e)
        {
            //  DESTROY THE SocketAsyncEventArgs USER TOKEN TO MINIMISE CHANCE OF MEMORY LEAK
            e.UserToken = null;
            //  FREE THE SocketAsyncEventArg SO IT CAN BE REUSED.
            e.SetBuffer(new byte[2], 0, 2);
            sendEventArgsPool.Push(e);
        }



        #endregion

        #region Socket async Receive


        /// <summary>
        /// A block of data has been received through the socket. It may contain part of a message, a message, or multiple messages. Process the incoming bytes and when a full message has been received, process the complete message.
        /// </summary>
        /// <param name="sender">Sending Socket</param>
        /// <param name="e">Socket Arguments</param>
        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
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
                TraceEventRaised?.Invoke(this, new TraceEventArgs("Disconnecting: ProcessReceive received socket error code " + (int)e.SocketError, SeverityType.Warning, 11000));
                DisconnectSocket();
                return;
            }


            try
            {
                int socketReceiveBufferPtr = 0;
                while (socketReceiveBufferPtr < e.BytesTransferred && CanSendReceive())
                {
                    bool haveEntireMessage = receiveEngine.AddBytesFromSocketReceiveBuffer(e.BytesTransferred, e.Buffer, ref socketReceiveBufferPtr);
                    if (haveEntireMessage == true)
                    {
                        if (receiveEngine.MessageType == MessageTypes.ResponseMessage)
                        {
                            //  SyncEndPointSubscriptionsWithServer() IS WAITING. COMPLETE THE SYNCRONOUS OPERATION SO IT CAN CONTINUE
                            ResponseMessage response = receiveEngine.GetResponseMessage();

                            //  CHECK TO SEE IS THE MESSAGE IS IN THE LIST OF OPEN SendReceive ITEMS.
                            RequestMessage foundOpenRequest = openRequests.Find(response.RequestId);
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
                        else if (receiveEngine.MessageType == MessageTypes.Message)
                        {
                            NotifyMessageReceived(receiveEngine.GetMessage());
                        }
                        else if (receiveEngine.MessageType == MessageTypes.ServerStoppingMessage)
                        {
                            TraceEventRaised?.Invoke(this, new TraceEventArgs("Disconnecting: Server is stopping.", SeverityType.Warning, 11000));
                            DisconnectSocket();
                        }
                        else if (receiveEngine.MessageType == MessageTypes.PollResponse)
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
                TraceEventRaised?.Invoke(this, new TraceEventArgs("Disconnecting: ObjectDisposedException running ProcessReceive: " + ee.Message, SeverityType.Error, 11000));
                DisconnectSocket();
            }
            catch (Exception ex)
            {
                TraceEventRaised?.Invoke(this, new TraceEventArgs("Disconnecting: Error running ProcessReceive: " + ex.Message, SeverityType.Error, 11000));
                DisconnectSocket();
            }
        }



#endregion

#region Shared


        private bool CanPoll()
        {
            lock (classLock)
            {
                if (isStopAllRequested == true || isStopPollingRequested == true) return false;
                if (connectionStatus != ConnectionStatuses.Connected) return false;
                return currentEndPoint.Socket.Connected;
            }
        }


        private bool CanSendReceive()
        {
            lock (classLock)
            {
                if (isStopAllRequested == true) return false;
                if (connectionStatus != ConnectionStatuses.Connected) return false;
                return currentEndPoint.Socket.Connected;
            }
        }



        [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
        private void NotifyMessageReceived(Messages.Message message)
        {
            if (MessageReceived != null)
            {
                //  RAISE EVENT IN THE BACKGROUND
                new Thread(new ThreadStart(delegate
                {
                    try { MessageReceived(this, new MessageReceivedEventArgs(message.Parameters)); }
                    catch (Exception ex)
                    {
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 11000));
                    }
                }
                )).Start();

            }
        }


#endregion
    }
}
