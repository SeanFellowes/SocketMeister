#if !SILVERLIGHT && !SMNOSERVER
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// Server side socket processing components. Is the central socket server.
    /// </summary>
#if SMISPUBLIC
     public partial class PolicyServer : IDisposable
#else
    internal partial class PolicyServer : IDisposable
#endif
    {
        private const string POLICY_REQUEST = "<policy-file-request/>";

        private IPAddress _ipAddress = null;
        private Listener _listenerPolicyRequests = null;
        private byte[] _policybytes;
        private int _policyRequestsReceived = 0;
        private bool _run = true;
        public const int ServicePort = 943;
        private SocketServerStatusTypes _serviceStatus = SocketServerStatusTypes.Stopped;
        private int _unknownRequestsReceived = 0;

        //  THREADSAFE LOCKS
        private readonly object _lockClass = new object();

        //  EVENTS
 
        /// <summary>
        /// An error occured in the socket server. Trap these if you want to perform logging.
        /// </summary>
        public event EventHandler<PolicyServerErrorEventArgs> Error;

        /// <summary>
        /// A request for the policy file was received.
        /// </summary>
        public event EventHandler<PolicyRequestReceivedEventArgs> PolicyRequestReceived;

        /// <summary>
        /// The status of the socket server changed. Statuses include stating, started, stopping and stopped.
        /// </summary>
        public event EventHandler<PolicyServerStatusChangedEventArgs> SocketServiceStatusChanged;

        /// <summary>
        /// An unknown request was received. Port 943 is expecting ONLY policy requests.
        /// </summary>
        public event EventHandler<UnknownPolicyRequestReceivedEventArgs> UnknownRequestReceived;

        /// <summary>
        /// Default PolicyServer Constructor. Will automatically use the first IP4 ethernet card found
        /// </summary>
        public PolicyServer()
        {
            _ipAddress = System.Net.IPAddress.Parse(GetLocalIPv4(NetworkInterfaceType.Ethernet));
        }


        /// <summary>
        /// PolicyServer Constructor
        /// </summary>
        /// <param name="IPAddress">The IP address of this server. If using this locally only (e.g. for locally testing the server and client), use "127.0.0.1"</param>
        public PolicyServer(string IPAddress)
        {
            _ipAddress = System.Net.IPAddress.Parse(IPAddress); ;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ipAddress = null;
                if (_listenerPolicyRequests != null) _listenerPolicyRequests.Dispose();
                _listenerPolicyRequests = null;
                _policybytes = null;
            }
        }


        //  PRIVATE / INTERNAL PROPERTIES
        private Listener PolicyListener { get { return _listenerPolicyRequests; } }
        private bool Run { get { lock (_lockClass) { return _run; } } set { lock (_lockClass) _run = value; } }


        //  PUBLIC PROPERTIES

        /// <summary>
        /// The IP address being used.
        /// </summary>
        public IPAddress IPAddress { get { return _ipAddress; } }



        /// <summary>
        /// The current status of the sockted server. Statuses include Stopped, Starting, Started and Stopping
        /// </summary>
        public SocketServerStatusTypes ServiceStatus
        {
            get { lock (_lockClass) { return _serviceStatus; } }
            private set
            {
                lock (_lockClass) { _serviceStatus = value; }
                if (SocketServiceStatusChanged != null)
                {
                    try { SocketServiceStatusChanged(this, new PolicyServerStatusChangedEventArgs { Status = value, IPAddress = _ipAddress }); }
                    catch { }
                }
            }
        }


        //  *********************
        //  ** PUBLIC METHODS ***
        //  *********************


        public static string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Start the socket server. This is a background operation. Your code can (optionally) poll the 'ServiceStatus' property or catch the 'SocketServiceStatusChanged' Event.
        /// </summary>
        public void Start()
        {
            if (this.ServiceStatus != SocketServerStatusTypes.Stopped) throw new Exception("Cannot start Socket Policy Server as is is not stopped.");
            this.ServiceStatus = SocketServerStatusTypes.Starting;

            new Thread(
                new ThreadStart(delegate
                {
                    try
                    {
                        Run = true;

                        //  READ CrossDomainPolicy.xml INTO A BYTE ARRAY
                        string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string xmlFileName = Path.Combine(assemblyFolder, "CrossDomainPolicy.xml");
                        var filestream = new FileStream(xmlFileName, FileMode.Open, FileAccess.Read);
                        _policybytes = new byte[filestream.Length];
                        filestream.Read(_policybytes, 0, (int)filestream.Length);
                        filestream.Close();

                        // Initialize policy socket listener
                        _listenerPolicyRequests = new Listener();
                        _listenerPolicyRequests.Error += socketListener_Error;
                        _listenerPolicyRequests.IsRunningChanged += socketListener_ListenerStatusChanged;
                        _listenerPolicyRequests.Start(_ipAddress, ServicePort, policyClient_Connected, 1000, "Policy Listener");

                        //  REGISTER AS STARTED AND START THE MESSAGE SENDER (NOTE: WILL NOT RUN UNLESS ServiceStatus = STARTED
                        this.ServiceStatus = SocketServerStatusTypes.Started;

                        //  WRITE EVENT
                        //EventLogHelper.WriteInfoToEventLog("Socket policy server started on address " + _ipAddress + ":" + _servicePort + ". Policy port listener on port 943. Maximum client connections = 1000", 1000);
                    }
                    catch (Exception ex)
                    {
                        this.ServiceStatus = SocketServerStatusTypes.Stopped;
                        GenerateError(new Exception("Error starting Socket Service: " + ex.Message), 52800);
                    }
                })).Start();
        }


        /// <summary>
        /// Stop the socket server. This is a background operation. Your code can (optionally) poll the 'ServiceStatus' property or catch the 'SocketServiceStatusChanged' Event.
        /// </summary>
        public void Stop()
        {
            if (ServiceStatus == SocketServerStatusTypes.Stopped) return;

            //  STOP IN A THREAD (ALLOW THE PARENT TO DO WHAT IT NEEDS TO DO)
            new Thread( new ThreadStart(delegate {
                try
                {
                    //  STOP THE SERVICE. CHILD THREADS WILL STOP AS SOON AS THE ServiceStatus IS NOT "Started"
                    ServiceStatus = SocketServerStatusTypes.Stopping;
                    Run = false;

                    //  LISTENERS TO REJECT NEW CLIENT CONNECTIONS
                    _listenerPolicyRequests.RejectNewConnections = true;

                    //  UNREGISTER LISTENER EVENTS
                    _listenerPolicyRequests.Error -= socketListener_Error;

                    //  STOP THE LISTENERS
                    _listenerPolicyRequests.Stop();
                    DateTime timeout = DateTime.Now.AddSeconds(30);
                    while (this.ServiceStatus != SocketServerStatusTypes.Stopped && DateTime.Now < timeout)
                    {
                        Thread.Sleep(100);
                    }


                    //  UNREGISTER REMAINING EVENTS
                    _listenerPolicyRequests.IsRunningChanged -= socketListener_ListenerStatusChanged;

                    //  IT SHOULD ALREADY BE STOPPED, BUT IF THE STOP TIMED OUT IT WONT HAVE
                    this.ServiceStatus = SocketServerStatusTypes.Stopped;
                }
                catch (Exception ex)
                {
                    this.ServiceStatus = SocketServerStatusTypes.Stopped;
                    GenerateError(new Exception("Error Stopping Socket Service: " + ex.Message), 52801);
                }
            })).Start();
        }




        //  ************************
        //  *** LISTERNER EVENTS ***
        //  ************************


        private void socketListener_Error(object sender, PolicyServerErrorEventArgs e)
        {
            GenerateError(e.Error, 52805);
        }

        private void socketListener_ListenerStatusChanged(object sender, PolicyServerIsRunningChangedArgs e)
        {
            //  IF BOTH THE POLICY AND CLIENT LISTENERS ARE RUNNING THEN RETURN A STATUS OF RUNNING
            if (_listenerPolicyRequests == null) return;
            if (_listenerPolicyRequests.IsRunning != true) ServiceStatus = SocketServerStatusTypes.Stopped;
        }

        //  POLICY FILE LISTENER (PORT 943) - WHEN A CLIENT CONNECTS, SEND BACK THE POLICY FILE
        private void policyClient_Connected(Socket sock)
        {
            new Thread(
                new ThreadStart(delegate
                {
                    string endPoint = "Unknown Endpoint";
                    try
                    {
                        if (Run == false) return;
                        if (sock != null)
                            if (sock.RemoteEndPoint != null) endPoint = sock.RemoteEndPoint.ToString();
                        byte[] receivebuffer = new byte[1000];
                        var receivedcount = sock.Receive(receivebuffer);
                        string requeststr = Encoding.UTF8.GetString(receivebuffer, 0, receivedcount);

                        //  IF A POLICY HAS BEEN RECEIVED, RETURN THE POLICY FILE
                        if (requeststr == POLICY_REQUEST)
                        {
                            sock.Send(_policybytes, 0, _policybytes.Length, SocketFlags.None);
                            int cnt = 0;
                            lock (_lockClass)
                            {
                                _policyRequestsReceived = _policyRequestsReceived + 1;
                                if (_policyRequestsReceived == int.MaxValue) _policyRequestsReceived = 1;
                                cnt = _policyRequestsReceived;
                            }
                            if (PolicyRequestReceived != null)
                            {
                                new Thread(new ThreadStart(delegate
                                {
                                    try { PolicyRequestReceived(this, new PolicyRequestReceivedEventArgs() { EndPoint = endPoint, TotalPolicyRequestsReceived = cnt }); }
                                    catch { }
                                })).Start();
                            }
                        }
                        else
                        {
                            int cnt = 0;
                            lock (_lockClass)
                            {
                                _unknownRequestsReceived = _unknownRequestsReceived + 1;
                                if (_unknownRequestsReceived == int.MaxValue) _unknownRequestsReceived = 1;
                                cnt = _unknownRequestsReceived;
                            }
                            if (UnknownRequestReceived != null)
                            {
                                new Thread(new ThreadStart(delegate
                                {
                                    try { UnknownRequestReceived(this, new UnknownPolicyRequestReceivedEventArgs() { EndPoint = endPoint, TotalUnknownRequestsReceived = cnt }); }
                                    catch { }
                                })).Start();
                            }

                        }
                    }
                    catch (Exception ex) { GenerateError(ex, 52810); }
                    finally
                    {
                        sock.Close();
                    }
                })).Start();
        }






        private string DetermineSourceName(object Source)
        {
            if (Source == this) return "Socket Server";
            if (Source == _listenerPolicyRequests) return "Policy Listener";
            return "Unknown";
        }


        internal void GenerateError(Exception ex, int ErrorNumber)
        {
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    //EventLogHelper.WriteErrorToEventLog(ex, ErrorNumber);
                    if (Error != null) Error(this, new PolicyServerErrorEventArgs { Source = DetermineSourceName(this), Error = ex });
                }
                catch { }
            })).Start();
        }



    }
}
#endif