#if !SILVERLIGHT && !SMNOSERVER
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
            private bool _isRunning = false;
            private bool _run = true;
            private readonly object lockObject = new object();
            private Socket listener = null;
            private bool _rejectNewConnections = false;
            //  EVENTS
            internal event EventHandler<PolicyServerErrorEventArgs> Error;
            internal event EventHandler<PolicyServerIsRunningChangedArgs> IsRunningChanged;

            //  PROPERTIES
            internal bool IsRunning { get { lock (lockObject) { return _isRunning; } } private set { lock (lockObject) { _isRunning = value; } } }
            private bool RunListener { get { lock (lockObject) { return _run; } } set { lock (lockObject) { _run = value; } } }
            internal bool RejectNewConnections { get { lock (lockObject) { return _rejectNewConnections; } } set { lock (lockObject) { _rejectNewConnections = value; } } }

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


            internal void RejectNewConnectionss()
            {
                if (listener == null || IsRunning == false) return;
                listener.Accept();
            }

            internal void Start(IPAddress Address, int port, GetSocketCallBack callback, int maximumConnections)
            {
                //  RUN ON ANOTHER THREAD
                _rejectNewConnections = false;
                new Thread(new ThreadStart(delegate { Listen(Address, port, callback, maximumConnections); })).Start();
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

            private void Listen(IPAddress Address, int port, GetSocketCallBack callback, int maximumConnections)
            {
                RunListener = true;
                IPEndPoint localEP = new IPEndPoint(Address, port);

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
                    if (sex.ErrorCode != 10004 && Error != null) IsRunningChanged(this, new PolicyServerIsRunningChangedArgs { IsRunning = false });
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    IsRunningChanged?.Invoke(this, new PolicyServerIsRunningChangedArgs { IsRunning = false });
                    Error?.Invoke(this, new PolicyServerErrorEventArgs { Error = ex });
                }
                try { listener.Close(); }
                catch { IsRunning = false; }
            }


        }
    }
}
#endif