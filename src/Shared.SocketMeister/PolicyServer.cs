#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1303 // Do not pass literals as localized parameters

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
        /// <summary>
        /// The port that the policy server is listening on.
        /// </summary>
        public const int ServicePort = 943;
        private const string POLICY_REQUEST = "<policy-file-request/>";

        private readonly object classLock = new object();
        private Listener listener = null;
        private byte[] policyBytes;
        private bool run = true;
        private SocketServerStatus status = SocketServerStatus.Stopped;

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
        public event EventHandler<SocketServer.SocketServerStatusChangedEventArgs> ListenerStateChanged;

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

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (listener != null) listener.Dispose();
                listener = null;
                policyBytes = null;
            }
        }



        //  PRIVATE / INTERNAL PROPERTIES
        private bool Run { get { lock (classLock) { return run; } } set { lock (classLock) run = value; } }


        //  PUBLIC PROPERTIES

        /// <summary>
        /// The IP address being used.
        /// </summary>
        public IPAddress IPAddress { get; private set; }

        /// <summary>
        /// Port number which the policy server is listening on.
        /// </summary>
        public static int Port {  get { return ServicePort; } }

        /// <summary>
        /// The current status of the listener socket. Statuses include Stopped, Starting, Started and Stopping
        /// </summary>
        public SocketServerStatus ListenerState
        {
            get { lock (classLock) { return status; } }
            private set
            {
                lock (classLock) 
                { 
                    if (status == value) return; 
                    status = value; 
                }
                try 
                {
                    ListenerStateChanged?.Invoke(this, new SocketServer.SocketServerStatusChangedEventArgs(value));
                }
                catch { }
            }
        }


        //  *********************
        //  ** PUBLIC METHODS ***
        //  *********************
        //private static IPAddress GetLocalIPAddress()
        //{
        //    var host = Dns.GetHostEntry(Dns.GetHostName());
        //    foreach (IPAddress ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == AddressFamily.InterNetwork)
        //        {
        //            return ip;
        //        }
        //    }
        //    throw new Exception("No network adapters with an IPv4 address in the system!");
        //}


        /// <summary>
        /// Get the local IP4 address
        /// </summary>
        /// <param name="type">NetworkInterfaceType</param>
        /// <returns>IP Address string</returns>
        public static string GetLocalIPv4(NetworkInterfaceType type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == type && item.OperationalStatus == OperationalStatus.Up)
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
        /// Start the socket server. This is a background operation. Your code can (optionally) poll the 'SocketServer.SocketServerStatus' property or catch the 'SocketServer.SocketServerStatusChanged' Event.
        /// </summary>
        public void Start()
        {
            if (this.ListenerState != SocketServerStatus.Stopped) throw new Exception("Cannot start Socket Policy Server as is is not stopped.");
            this.ListenerState = SocketServerStatus.Starting;

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
                        policyBytes = new byte[filestream.Length];
                        filestream.Read(policyBytes, 0, (int)filestream.Length);
                        filestream.Close();

                        // Initialize policy socket listener
                        listener = new Listener();
                        listener.TraceEventRaised += SocketListener_Error;
                        listener.IsRunningChanged += SocketListener_ListenerStatusChanged;
                        listener.Start(IPAddress, ServicePort, PolicyClient_Connected, 1000);

                        //  REGISTER AS STARTED AND START THE MESSAGE SENDER (NOTE: WILL NOT RUN UNLESS SocketServer.SocketServerStatus = STARTED
                        this.ListenerState = SocketServerStatus.Started;

                        TraceEventRaised?.Invoke(this, new TraceEventArgs("Policy server started on port " + ServicePort.ToString(CultureInfo.InvariantCulture),  SeverityType.Information, 10023));
                    }
                    catch (Exception ex)
                    {
                        this.ListenerState = SocketServerStatus.Stopped;
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 52800));
                    }
                })).Start();
        }


        /// <summary>
        /// Stop the socket server. This is a background operation. Your code can (optionally) poll the 'SocketServer.SocketServerStatus' property or catch the 'SocketServer.SocketServerStatusChanged' Event.
        /// </summary>
        public void Stop()
        {
            if (ListenerState == SocketServerStatus.Stopped) return;

            try
            {
                //  STOP THE SERVICE. CHILD THREADS WILL STOP AS SOON AS THE SocketServer.SocketServerStatus IS NOT "Started"
                ListenerState = SocketServerStatus.Stopping;
                Run = false;

                //  LISTENERS TO REJECT NEW CLIENT CONNECTIONS
                listener.RejectNewConnections = true;

                //  UNREGISTER LISTENER EVENTS
                listener.TraceEventRaised -= SocketListener_Error;

                //  STOP THE LISTENERS
                listener.Stop();
                DateTime timeout = DateTime.Now.AddSeconds(30);
                while (this.ListenerState != SocketServerStatus.Stopped && DateTime.Now < timeout)
                {
                    Thread.Sleep(100);
                }


                //  UNREGISTER REMAINING EVENTS
                listener.IsRunningChanged -= SocketListener_ListenerStatusChanged;

                //  IT SHOULD ALREADY BE STOPPED, BUT IF THE STOP TIMED OUT IT WONT HAVE
                this.ListenerState = SocketServerStatus.Stopped;
            }
            catch (Exception ex)
            {
                ListenerState = SocketServerStatus.Stopped;
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 52801));
            }
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
            if (listener == null) return;
            if (listener.IsRunning != true) ListenerState = SocketServerStatus.Stopped;
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
                            sock.Send(policyBytes, 0, policyBytes.Length, SocketFlags.None);
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
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1303 // Do not pass literals as localized parameters

