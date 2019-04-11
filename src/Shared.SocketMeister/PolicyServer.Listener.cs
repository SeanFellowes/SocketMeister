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
            private readonly object _lock = new object();
            private Socket _listener = null;
            private bool _rejectNewConnections = false;

            //  EVENTS
            internal event EventHandler<ExceptionEventArgs> ExceptionRaised;
            internal event EventHandler<PolicyServerIsRunningChangedArgs> IsRunningChanged;

            //  PROPERTIES
            internal bool IsRunning { get { lock (_lock) { return _isRunning; } } private set { lock (_lock) { _isRunning = value; } } }
            private bool RunListener { get { lock (_lock) { return _run; } } set { lock (_lock) { _run = value; } } }
            internal bool RejectNewConnections { get { lock (_lock) { return _rejectNewConnections; } } set { lock (_lock) { _rejectNewConnections = value; } } }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_listener != null) _listener.Close();
                    _listener = null;
                }
            }


            internal void Start(IPAddress Address, int port, GetSocketCallBack callback, int maximumConnections)
            {
                //  RUN ON ANOTHER THREAD
                _rejectNewConnections = false;
                new Thread(new ThreadStart(delegate { Listen(Address, port, callback, maximumConnections); })).Start();
            }

            internal void Stop()
            {
                if (_listener == null || IsRunning == false) return;
                RunListener = false;
                _listener.Close();
                while (IsRunning == true) { Thread.Sleep(200); }
                try
                {
#if !NET20 && !NET35
                    _listener.Dispose();
#else
                    _listener.Close();
#endif
                    _listener = null;
                }
                catch { }
            }

            private void Listen(IPAddress Address, int port, GetSocketCallBack callback, int maximumConnections)
            {
                RunListener = true;
                IPEndPoint localEP = new IPEndPoint(Address, port);

                try
                {
                    _listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    _listener.Bind(localEP);
                    IsRunning = true;
                    IsRunningChanged?.Invoke(this, new PolicyServerIsRunningChangedArgs { IsRunning = true });
                    while (RunListener == true)
                    {
                        if (RejectNewConnections == false)
                        {
                            if (RunListener == true)
                            {
                                _listener.Listen(maximumConnections);
                                Socket socket = _listener.Accept();

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
                    if (sex.ErrorCode != 10004 && ExceptionRaised != null) IsRunningChanged(this, new PolicyServerIsRunningChangedArgs { IsRunning = false });
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    IsRunningChanged?.Invoke(this, new PolicyServerIsRunningChangedArgs { IsRunning = false });
                    ExceptionRaised?.Invoke(this, new ExceptionEventArgs(ex, 1234));
                }
                try { _listener.Close(); }
                catch { IsRunning = false; }
            }


        }
    }
}
#endif