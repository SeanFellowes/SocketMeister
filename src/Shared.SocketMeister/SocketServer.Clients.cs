#if !SILVERLIGHT && !SMNOSERVER
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
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
            private readonly object classLock = new object();
            private readonly List<Client> list = new List<Client>();

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
            public event EventHandler<TraceEventArgs> TraceEventRaised;

            /// <summary>
            /// Total number of syschronous and asynchronous clients connected
            /// </summary>
            public int Count { get { lock (classLock) { return list.Count; } } }

            public void Add(Client client)
            {
                if (client == null) return;
                lock (classLock) { list.Add(client); }
                NotifyClientConnected(client);
            }

            [SuppressMessage("Microsoft.Performance", "CA1031:DoNotCatchGeneralExceptionTypes", MessageId = "ExceptionEventRaised")]
            public void Disconnect(Client client)
            {
                if (client == null) return;
                lock (classLock)
                {
                    list.Remove(client);
                }

                try
                {
                    client.ClientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }

                try
                {
                    client.ClientSocket.Close();
                }
                catch (Exception ex)
                {
                    NotifyExceptionRaised(ex);
                }

                NotifyClientDisconnected(client);
            }

            public void DisconnectAll()
            {
                List<Client> items = ToList();
                foreach (Client item in items)
                {
                    Disconnect(item);
                }
            }

            private void NotifyClientConnected(Client client)
            {
                if (ClientConnected != null)
                {
                    //  RAISE EVENT IN THE BACKGROUND AND ERRORS ARE IGNORED
                    new Thread(new ThreadStart(delegate
                    {
                        ClientConnected(null, new ClientConnectedEventArgs(client)); 
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
                        ClientDisconnected(null, new ClientDisconnectedEventArgs(client)); 
                    }
                    )).Start();
                }
            }

            private void NotifyExceptionRaised(Exception error)
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
                lock (classLock)
                {
                    List<Client> rVal = new List<Client>(list.Count);
                    foreach (Client cl in list)
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