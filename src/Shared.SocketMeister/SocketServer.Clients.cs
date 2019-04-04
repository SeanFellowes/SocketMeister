#if !SILVERLIGHT && !SMNOSERVER
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketMeister
{
#if SMISPUBLIC
    public partial class SocketServer
#else
    internal partial class SocketServer
#endif
    {
        internal class Clients
        {
            private readonly List<Client> _list = new List<Client>();
            private readonly object _lock = new object();

            /// <summary>
            /// Event raised when a client connects to the socket server (Raised in a seperate thread)
            /// </summary>
            public event EventHandler<ClientConnectedEventArgs> ClientConnected;

            /// <summary>
            /// Event raised when a client disconnects from the socket server (Raised in a seperate thread)
            /// </summary>
            public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

            /// <summary>
            /// Raised when an exception occurs.
            /// </summary>
            public event EventHandler<ExceptionEventArgs> ExceptionRaised;

            /// <summary>
            /// Total number of syschronous and asynchronous clients connected
            /// </summary>
            public int Count { get { lock (_lock) { return _list.Count; } } }

            public void Add(Client Client)
            {
                if (Client == null) return;
                lock (_lock) { _list.Add(Client); }
                NotifyClientConnected(Client);
            }

            [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
            public void Disconnect(Client Client)
            {
                if (Client == null) return;
                lock (_lock)
                {
                    _list.Remove(Client);
                }

                try
                {
                    Client.ClientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }

                try
                {
                    Client.ClientSocket.Close();
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }

                NotifyClientDisconnected(Client);
            }

            public void DisconnectAll()
            {
                List<Client> items = ToList();
                foreach (Client item in items)
                {
                    Disconnect(item);
                }
            }

            private void NotifyClientConnected(Client Client)
            {
                if (ClientConnected != null)
                {
                    //  RAISE EVENT IN THE BACKGROUND AND ERRORS ARE IGNORED
                    new Thread(new ThreadStart(delegate
                    {
                        ClientConnected(null, new ClientConnectedEventArgs(Client)); 
                    }
                    )).Start();
                }
            }

            private void NotifyClientDisconnected(Client Client)
            {
                if (ClientConnected != null)
                {
                    //  RAISE EVENT IN THE BACKGROUND AND ERRORS ARE IGNORED
                    new Thread(new ThreadStart(delegate
                    {
                        ClientDisconnected(null, new ClientDisconnectedEventArgs(Client)); 
                    }
                    )).Start();
                }
            }

            private void NotifyExceptionRaised(Exception Error)
            {
                if (ExceptionRaised != null)
                {
                    string msg = Error.Message;
                    if (Error.StackTrace != null) msg += Environment.NewLine + Environment.NewLine + Error.StackTrace;
                    if (Error.InnerException != null) msg += Environment.NewLine + Environment.NewLine + "Inner Exception: " + Error.InnerException.Message;
                    //  RAISE EVENT IN THE BACKGROUND
                    new Thread(new ThreadStart(delegate
                    {
                        ExceptionRaised?.Invoke(this, new ExceptionEventArgs(Error, 5008));
                    }
                    )).Start();

                }
            }



            /// <summary>
            /// Returns a list of clients which are connected to the socket server
            /// </summary>
            /// <returns>List of clients</returns>
            public List<Client> ToList()
            {
                lock (_lock)
                {
                    List<Client> rVal = new List<Client>(_list.Count);
                    foreach (Client cl in _list)
                    {
                        rVal.Add(cl);
                    }
                    return rVal;
                }
            }
        }
    }
}
#endif