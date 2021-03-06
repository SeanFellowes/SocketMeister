#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CA1805 // Do not initialize unnecessarily
#pragma warning disable CA1031 // Do not catch general exception types

#if !SILVERLIGHT && !SMNOSERVER && !NET35 && !NET20
using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace SocketMeister
{
#if SMISPUBLIC
    public partial class PolicyServer : IDisposable
#else
    internal partial class PolicyServer : IDisposable
#endif
    {
        internal delegate void GetSocketCallBack(Socket sock);

        internal class Listener : IDisposable
        {
            private bool isRunning = false;
            private bool run = true;
            private readonly object classLock = new object();
            private Socket listener = null;
            private bool rejectNewConnections = false;

            //  EVENTS
            internal event EventHandler<TraceEventArgs> TraceEventRaised;
            internal event EventHandler<PolicyServerIsRunningChangedArgs> IsRunningChanged;

            //  PROPERTIES
            internal bool IsRunning { get { lock (classLock) { return isRunning; } } private set { lock (classLock) { isRunning = value; } } }
            private bool RunListener { get { lock (classLock) { return run; } } set { lock (classLock) { run = value; } } }
            internal bool RejectNewConnections { get { lock (classLock) { return rejectNewConnections; } } set { lock (classLock) { rejectNewConnections = value; } } }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (listener != null) listener.Close();
                    listener = null;
                }
            }


            internal void Start(IPAddress address, int port, GetSocketCallBack callback, int maximumConnections)
            {
                //  RUN ON ANOTHER THREAD
                rejectNewConnections = false;
                new Thread(new ThreadStart(delegate { Listen(address, port, callback, maximumConnections); })).Start();
            }

            internal void Stop()
            {
                if (listener == null || IsRunning == false) return;
                RunListener = false;
                listener.Close();
                while (IsRunning == true) { Thread.Sleep(200); }
                try
                {
#if !NET20 && !NET35
                    listener.Dispose();
#else
                    listener.Close();
#endif
                    listener = null;
                }
                catch { }
            }

            private void Listen(IPAddress address, int port, GetSocketCallBack callback, int maximumConnections)
            {
                RunListener = true;
                IPEndPoint localEP = new IPEndPoint(address, port);

                try
                {
                    listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    listener.Bind(localEP);
                    IsRunning = true;
                    IsRunningChanged?.Invoke(this, new PolicyServerIsRunningChangedArgs { IsRunning = true });
                    while (RunListener == true)
                    {
                        if (RejectNewConnections == false)
                        {
                            if (RunListener == true)
                            {
                                listener.Listen(maximumConnections);
                                Socket socket = listener.Accept();

                                // Return connected socket through callback function.
                                if (callback != null && RejectNewConnections == false)
                                    callback(socket);
                                else
                                {
                                    socket.Close();
                                    socket = null;
                                }
                            }
                        }
                        else Thread.Sleep(250);
                    }
                    IsRunning = false;
                    IsRunningChanged?.Invoke(this, new PolicyServerIsRunningChangedArgs { IsRunning = false });
                }
                catch (SocketException sex)
                {
                    IsRunning = false;
                    IsRunningChanged?.Invoke(this, new PolicyServerIsRunningChangedArgs { IsRunning = false });
                    if (sex.ErrorCode != 10004 && TraceEventRaised != null) IsRunningChanged(this, new PolicyServerIsRunningChangedArgs { IsRunning = false });
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    IsRunningChanged?.Invoke(this, new PolicyServerIsRunningChangedArgs { IsRunning = false });
                    TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1234));
                }
                try { if (listener != null) listener.Close(); }
                catch { IsRunning = false; }
            }


        }
    }
}
#endif


#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression
