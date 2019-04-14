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
using System.Globalization;

namespace SocketMeister
{
    /// <summary>
    /// Lightweight TCP server which listens on port 943 for policy file requests from Silverlight clients. Policy file describes which ports between 4502-4534 can be used by Silverlight.
    /// </summary>
#if SMISPUBLIC
     public partial class PolicyServer : IDisposable
#else
    internal partial class PolicyServer : IDisposable
#endif
    {
        public const int ServicePort = 943;
        private const string POLICY_REQUEST = "<policy-file-request/>";

        private Listener _listener = null;
        private byte[] _policybytes;
        private bool _run = true;
        private ServiceStatus _status = ServiceStatus.Stopped;

        //  THREADSAFE LOCKS
        private readonly object _lockClass = new object();

        /// <summary>
        /// Trace message raised from within SocketMeister.
        /// </summary>
        public event EventHandler<TraceEventArgs> TraceEventRaised;

        /// <summary>
        /// A request for the policy file was received.
        /// </summary>
        public event EventHandler<PolicyRequestReceivedEventArgs> PolicyRequestReceived;

        /// <summary>
        /// The status of the socket server changed. Statuses include stating, started, stopping and stopped.
        /// </summary>
        public event EventHandler<ServerStatusEventArgs> SocketServiceStatusChanged;

        /// <summary>
        /// An unknown request was received. Port 943 is expecting ONLY policy requests.
        /// </summary>
        public event EventHandler<PolicyRequestReceivedEventArgs> UnknownRequestReceived;

        /// <summary>
        /// PolicyServer Constructor. The policy server will connect to all network interfaces on port 943
        /// </summary>
        public PolicyServer()
        {
            //  CONNECT TO ALL INTERFACES (I.P. 0.0.0.0 IS ALL)
            IPAddress = IPAddress.Parse("0.0.0.0");
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
                if (_listener != null) _listener.Dispose();
                _listener = null;
                _policybytes = null;
            }
        }



        //  PRIVATE / INTERNAL PROPERTIES
        private bool Run { get { lock (_lockClass) { return _run; } } set { lock (_lockClass) _run = value; } }


        //  PUBLIC PROPERTIES

        /// <summary>
        /// The IP address being used.
        /// </summary>
        public IPAddress IPAddress { get; private set; }


        /// <summary>
        /// The current status of the sockted server. Statuses include Stopped, Starting, Started and Stopping
        /// </summary>
        public ServiceStatus Status
        {
            get { lock (_lockClass) { return _status; } }
            private set
            {
                lock (_lockClass) { _status = value; }
                if (SocketServiceStatusChanged != null)
                {
                    try { SocketServiceStatusChanged(this, new ServerStatusEventArgs(value)); }
                    catch { }
                }
            }
        }


        //  *********************
        //  ** PUBLIC METHODS ***
        //  *********************
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
            if (this.Status != ServiceStatus.Stopped) throw new Exception("Cannot start Socket Policy Server as is is not stopped.");
            this.Status = ServiceStatus.Starting;

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
                        _listener = new Listener();
                        _listener.TraceEventRaised += SocketListener_Error;
                        _listener.IsRunningChanged += SocketListener_ListenerStatusChanged;
                        _listener.Start(IPAddress, ServicePort, PolicyClient_Connected, 1000);

                        //  REGISTER AS STARTED AND START THE MESSAGE SENDER (NOTE: WILL NOT RUN UNLESS ServiceStatus = STARTED
                        this.Status = ServiceStatus.Started;

                        TraceEventRaised?.Invoke(this, new TraceEventArgs("Policy server started on port " + ServicePort.ToString(), SeverityType.Information, 10023));
                    }
                    catch (Exception ex)
                    {
                        this.Status = ServiceStatus.Stopped;
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 52800));
                    }
                })).Start();
        }


        /// <summary>
        /// Stop the socket server. This is a background operation. Your code can (optionally) poll the 'ServiceStatus' property or catch the 'SocketServiceStatusChanged' Event.
        /// </summary>
        public void Stop()
        {
            if (Status == ServiceStatus.Stopped) return;

            //  STOP IN A THREAD (ALLOW THE PARENT TO DO WHAT IT NEEDS TO DO)
            new Thread( new ThreadStart(delegate {
                try
                {
                    //  STOP THE SERVICE. CHILD THREADS WILL STOP AS SOON AS THE ServiceStatus IS NOT "Started"
                    Status = ServiceStatus.Stopping;
                    Run = false;

                    //  LISTENERS TO REJECT NEW CLIENT CONNECTIONS
                    _listener.RejectNewConnections = true;

                    //  UNREGISTER LISTENER EVENTS
                    _listener.TraceEventRaised -= SocketListener_Error;

                    //  STOP THE LISTENERS
                    _listener.Stop();
                    DateTime timeout = DateTime.Now.AddSeconds(30);
                    while (this.Status != ServiceStatus.Stopped && DateTime.Now < timeout)
                    {
                        Thread.Sleep(100);
                    }


                    //  UNREGISTER REMAINING EVENTS
                    _listener.IsRunningChanged -= SocketListener_ListenerStatusChanged;

                    //  IT SHOULD ALREADY BE STOPPED, BUT IF THE STOP TIMED OUT IT WONT HAVE
                    this.Status = ServiceStatus.Stopped;
                }
                catch (Exception ex)
                {
                    this.Status = ServiceStatus.Stopped;
                    TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 52801));
                }
            })).Start();
        }




        //  ************************
        //  *** LISTERNER EVENTS ***
        //  ************************


        private void SocketListener_Error(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(this, e);
        }

        private void SocketListener_ListenerStatusChanged(object sender, PolicyServerIsRunningChangedArgs e)
        {
            //  IF BOTH THE POLICY AND CLIENT LISTENERS ARE RUNNING THEN RETURN A STATUS OF RUNNING
            if (_listener == null) return;
            if (_listener.IsRunning != true) Status = ServiceStatus.Stopped;
        }

        //  POLICY FILE LISTENER (PORT 943) - WHEN A CLIENT CONNECTS, SEND BACK THE POLICY FILE
        private void PolicyClient_Connected(Socket sock)
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
                            if (PolicyRequestReceived != null)
                            {
                                new Thread(new ThreadStart(delegate
                                {
                                    try { PolicyRequestReceived(this, new PolicyRequestReceivedEventArgs() { EndPoint = endPoint }); }
                                    catch { }
                                })).Start();
                            }
                        }
                        else
                        {
                            if (UnknownRequestReceived != null)
                            {
                                new Thread(new ThreadStart(delegate
                                {
                                    try { UnknownRequestReceived(this, new PolicyRequestReceivedEventArgs() { EndPoint = endPoint }); }
                                    catch { }
                                })).Start();
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 52810));
                    }
                    finally
                    {
                        sock.Close();
                    }
                })).Start();
        }





    }
}
#endif