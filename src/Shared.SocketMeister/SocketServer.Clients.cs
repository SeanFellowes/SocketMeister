#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1001 // Types that own disposable fields should be disposable

#if !SILVERLIGHT && !SMNOSERVER && !NET35 && !NET20
using System;
using System.Collections.Generic;
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
            public event EventHandler<ClientEventArgs> ClientConnected;

            /// <summary>
            /// Event raised when a client disconnects from the socket server (Raised in a seperate thread)
            /// </summary>
            public event EventHandler<ClientEventArgs> ClientDisconnected;

            /// <summary>
            /// Raised when an exception occurs.
            /// </summary>
            public event EventHandler<TraceEventArgs> TraceEventRaised;

            /// <summary>
            /// Total number of syschronous and asynchronous clients connected
            /// </summary>
            public int Count
            {
                get { lock (_lock) { return _list.Count; } }
            }

            public void Add(Client Client)
            {
                if (Client == null) return;

                lock (_lock)
                {
                    _list.Add(Client);
                }
                NotifyClientConnected(Client);
            }

            public void Disconnect(Client Client)
            {
                if (Client == null) return;

                lock (_lock)
                {
                    _list.Remove(Client);
                }

                try { Client.ClientSocket.Shutdown(SocketShutdown.Both); }
                catch (Exception ex)
                {
                    NotifyTraceEventRaised(ex);
                }
                try { Client.ClientSocket.Close(); }
                catch (Exception ex)
                {
                    NotifyTraceEventRaised(ex);
                }

                NotifyClientDisconnected(Client);
            }

            /// <summary>
            /// Remove a client from the list
            /// </summary>
            /// <param name="Client">Client to remove</param>
            public void Remove(Client Client)
            {
                if (Client == null) return;

                lock (_lock)
                {
                    _list.Remove(Client);
                }

                NotifyClientDisconnected(Client);
            }


            public void DisconnectAll()
            {
                List<Client> clients = ToList();
                foreach (Client client in clients)
                {
                    Disconnect(client);
                }
            }

            private void NotifyClientConnected(Client client)
            {
                if (ClientConnected != null)
                {
                    //  RAISE EVENT IN THE BACKGROUND AND ERRORS ARE IGNORED
                    new Thread(new ThreadStart(delegate
                    {
                        ClientConnected?.Invoke(null, new ClientEventArgs(client));
                    }
                    )).Start();
                }
            }

            private void NotifyClientDisconnected(Client client)
            {
                if (ClientConnected != null)
                {
                    //  RAISE EVENT IN THE BACKGROUND AND ERRORS ARE IGNORED
                    new Thread(new ThreadStart(delegate
                    {
                        ClientDisconnected?.Invoke(null, new ClientEventArgs(client));
                    }
                    )).Start();
                }
            }

            private void NotifyTraceEventRaised(Exception error)
            {
                if (TraceEventRaised != null)
                {
                    string msg = error.Message;
                    if (error.StackTrace != null) msg += Environment.NewLine + Environment.NewLine + error.StackTrace;
                    if (error.InnerException != null) msg += Environment.NewLine + Environment.NewLine + "Inner Exception: " + error.InnerException.Message;
                    //  RAISE EVENT IN THE BACKGROUND
                    new Thread(new ThreadStart(delegate
                    {
                        TraceEventRaised?.Invoke(this, new TraceEventArgs(error, 5008));
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
                List<Client> rVal = new List<Client>();
                lock (_lock)
                {
                    foreach (Client cl in _list)
                    {
                        rVal.Add(cl);
                    }
                }
                return rVal;
            }
        }
    }
}
#endif

#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression
