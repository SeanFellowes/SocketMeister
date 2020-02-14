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
            /// Event raised when when there is a change to the clients connected to the socket server
            /// </summary>
            public event EventHandler<ClientsChangedEventArgs> ClientsChanged;

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

                int clientCount = 0;
                lock (classLock) 
                { 
                    list.Add(client);
                    clientCount = list.Count;
                }
                NotifyClientConnected(client, clientCount);
            }

            public void Disconnect(Client client)
            {
                if (client == null) return;

                int clientCount = 0;
                lock (classLock)
                {
                    list.Remove(client);
                    clientCount = list.Count;
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

                NotifyClientDisconnected(client, clientCount);
            }

            public void DisconnectAll()
            {
                List<Client> items = ToList();
                foreach (Client item in items)
                {
                    Disconnect(item);
                }
            }

            private void NotifyClientConnected(Client client, int ClientCount)
            {
                if (ClientConnected != null || ClientsChanged != null)
                {
                    //  RAISE EVENT IN THE BACKGROUND AND ERRORS ARE IGNORED
                    new Thread(new ThreadStart(delegate
                    {
                        ClientConnected?.Invoke(null, new ClientConnectedEventArgs(client));
                        ClientsChanged?.Invoke(null, new ClientsChangedEventArgs(ClientCount));
                    }
                    )).Start();
                }
            }

            private void NotifyClientDisconnected(Client client, int ClientCount)
            {
                if (ClientConnected != null || ClientsChanged != null)
                {
                    //  RAISE EVENT IN THE BACKGROUND AND ERRORS ARE IGNORED
                    new Thread(new ThreadStart(delegate
                    {
                        ClientDisconnected?.Invoke(null, new ClientDisconnectedEventArgs(client));
                        ClientsChanged?.Invoke(null, new ClientsChangedEventArgs(ClientCount));
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