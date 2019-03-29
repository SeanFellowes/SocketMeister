using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    public partial class SocketServer
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
            /// Total number of syschronous and asynchronous clients connected
            /// </summary>
            public int Count { get { lock (_lock) { return _list.Count; } } }

            public void Add(Client Client)
            {
                if (Client == null) return;
                lock (_lock) { _list.Add(Client); }
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
                catch { }
                try { Client.ClientSocket.Close(); }
                catch { }

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
                        try { ClientConnected(null, new ClientConnectedEventArgs(Client)); }
                        catch { }
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
                        try { ClientDisconnected(null, new ClientDisconnectedEventArgs(Client)); }
                        catch { }
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
