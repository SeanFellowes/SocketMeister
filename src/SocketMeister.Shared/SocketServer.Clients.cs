#if !NET35
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
            private readonly ConcurrentDictionary<string, Client> _clientDictionary = new ConcurrentDictionary<string, Client>();
            private readonly Logger _logger;

            /// <summary>
            /// Event raised when a client connects to the socket server. This event is raised in a separate thread.
            /// </summary>
            public event EventHandler<ClientEventArgs> ClientConnected;

            /// <summary>
            /// Event raised when a client disconnects from the socket server. This event is raised in a separate thread.
            /// </summary>
            public event EventHandler<ClientEventArgs> ClientDisconnected;

            public Clients(Logger logger)
            {
                _logger = logger;
            }

            /// <summary>
            /// Gets the total number of synchronous and asynchronous clients connected.
            /// </summary>
            public int Count => _clientDictionary.Count;

            /// <summary>
            /// Adds a client to the collection and notifies that the client has connected.
            /// </summary>
            /// <param name="client">The client to add.</param>
            public void Add(Client client)
            {
                if (client == null) return;
                _clientDictionary.TryAdd(client.ClientId, client);
                NotifyClientConnected(client);
            }

            /// <summary>
            /// Disconnects a client and removes it from the collection.
            /// </summary>
            /// <param name="client">The client to disconnect.</param>
            public void Disconnect(Client client)
            {
                if (client == null) return;

                // Abort any outbound messages where a response has not been received
                _ = _clientDictionary.TryRemove(client.ClientId, out Client deletedClient);

                try { client.ClientSocket.Shutdown(SocketShutdown.Both); }
                catch (Exception ex) { _logger.Log(new LogEntry(ex)); }

                try { client.ClientSocket.Close(); }
                catch (Exception ex) { _logger.Log(new LogEntry(ex)); }

                NotifyClientDisconnected(client);
            }

            /// <summary>
            /// Removes a client from the collection without disconnecting its socket.
            /// </summary>
            /// <param name="client">The client to remove.</param>
            public void Remove(Client client)
            {
                if (client == null) return;
                _clientDictionary.TryRemove(client.ClientId, out Client deletedClient);
                NotifyClientDisconnected(client);
            }

            /// <summary>
            /// Disconnects all clients in the collection.
            /// </summary>
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
                        _logger.Log(new LogEntry(ex));
                    }
                });
            }

            /// <summary>
            /// Notifies that a client has connected by raising the <see cref="ClientConnected"/> event.
            /// </summary>
            /// <param name="client">The client that connected.</param>
            private void NotifyClientConnected(Client client)
            {
                Task.Run(() => ClientConnected?.Invoke(null, new ClientEventArgs(client)));
            }

            /// <summary>
            /// Notifies that a client has disconnected by raising the <see cref="ClientDisconnected"/> event.
            /// </summary>
            /// <param name="client">The client that disconnected.</param>
            private void NotifyClientDisconnected(Client client)
            {
                Task.Run(() => ClientDisconnected?.Invoke(null, new ClientEventArgs(client)));
            }

            /// <summary>
            /// Returns a list of clients currently connected to the socket server.
            /// </summary>
            /// <returns>A list of connected clients.</returns>
            public List<Client> ToList()
            {
                return _clientDictionary.Values.ToList();
            }

            /// <summary>
            /// Gets a list of clients that have subscriptions to a specific subscription name.
            /// </summary>
            /// <param name="subscriptionName">The name of the subscription to search for.</param>
            /// <returns>A list of clients with the specified subscription.</returns>
            internal List<Client> GetClientsWithSubscriptions(string subscriptionName)
            {
                List<Client> result = new List<Client>();
                ICollection<Client> clients = _clientDictionary.Values;

                foreach (var client in clients)
                {
                    if (client != null && client.SubscriptionCount == 0) continue;

                    if (client.DoesSubscriptionExist(subscriptionName))
                        result.Add(client);
                }

                return result;
            }
        }
    }
}
#endif

