#if !SMNOSERVER && !NET35
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

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
            private readonly ConcurrentDictionary<Guid, Client> _clientDictionary = new ConcurrentDictionary<Guid, Client>();
            /// <summary>
            /// Event raised when a client connects to the socket server (Raised in a seperate thread)
            /// </summary>
            public event EventHandler<ClientEventArgs> ClientConnected;

            /// <summary>
            /// Event raised when a client disconnects from the socket server (Raised in a seperate thread)
            /// </summary>
            public event EventHandler<ClientEventArgs> ClientDisconnected;

            /// <summary>
            /// Raised when an exception occurs (Raised in a seperate thread)
            /// </summary>
            public event EventHandler<TraceEventArgs> TraceEventRaised;

            /// <summary>
            /// Total number of syschronous and asynchronous clients connected
            /// </summary>
            public int Count
            {
                get { return _clientDictionary.Count; }
            }

            public void Add(Client client)
            {
                if (client == null) return;
                _clientDictionary.TryAdd(client.ClientId, client);
                NotifyClientConnected(client);
            }

            public void Disconnect(Client client)
            {
                if (client == null) return;

                //  Abort any outbound messages where a response has not been received
               

                _ = _clientDictionary.TryRemove(client.ClientId, out Client deletedClient);

                try { client.ClientSocket.Shutdown(SocketShutdown.Both); }
                catch (Exception ex) { NotifyTraceEventRaised(ex); }

                try { client.ClientSocket.Close(); }
                catch (Exception ex) { NotifyTraceEventRaised(ex); }

                NotifyClientDisconnected(client);
            }

            /// <summary>
            /// Remove a client from the list
            /// </summary>
            /// <param name="client">Client to remove</param>
            public void Remove(Client client)
            {
                if (client == null) return;
                _clientDictionary.TryRemove(client.ClientId, out Client deletedClient);
                NotifyClientDisconnected(client);
            }

            public void DisconnectAll()
            {
                Parallel.ForEach(ToList(), client =>
                {
                    try
                    {
                        Disconnect(client);
                    }
                    catch (Exception ex)
                    {
                        NotifyTraceEventRaised(ex);
                    }
                });
            }

            private void NotifyClientConnected(Client client)
            {
                Task.Run(() => ClientConnected?.Invoke(null, new ClientEventArgs(client)));
            }

            private void NotifyClientDisconnected(Client client)
            {
                Task.Run(() => ClientDisconnected?.Invoke(null, new ClientEventArgs(client)));
            }

            private void NotifyTraceEventRaised(Exception error)
            {
                var msg = error.ToString(); // Includes message, stack trace, and inner exception details.
                Task.Run(() => TraceEventRaised?.Invoke(this, new TraceEventArgs(error, 5008)));
            }

            /// <summary>
            /// Returns a list of clients which are connected to the socket server
            /// </summary>
            /// <returns>List of clients</returns>
            public List<Client> ToList()
            {
                return _clientDictionary.Values.ToList();
            }

            internal List<Client> GetClientsWithSubscriptions(string subscriptionName)
            {
                List<Client> rVal = new List<Client>();
                ICollection<Client> clients = _clientDictionary.Values;
                foreach (var client in clients)
                {
                    if (client != null && client.SubscriptionCount == 0) continue;
                    
                    if (client.DoesSubscriptionExist(subscriptionName))
                        rVal.Add(client);
                }
                return rVal;
            }
        }
    }
}
#endif

