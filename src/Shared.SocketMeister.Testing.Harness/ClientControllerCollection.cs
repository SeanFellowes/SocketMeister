using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace SocketMeister.Testing
{

    internal class ClientControllerCollection : IEnumerable<ClientController>
    {
        private readonly Dictionary<int, ClientController> _dictClientId = new Dictionary<int, ClientController>();
        private readonly List<ClientController> _listClient = new List<ClientController>();
        private static readonly object _lock = new object();

        public ClientControllerCollection()
        {
        }

        public ClientController this[int ClientId]
        {
            get
            {
                ClientController client;
                if (_dictClientId.TryGetValue(ClientId, out client) == true) return client;
                else return null;
            }
        }

        IEnumerator<ClientController> IEnumerable<ClientController>.GetEnumerator()
        {
            return _listClient.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _listClient.GetEnumerator();
        }






        /// <summary>
        /// Disconnects all clients
        /// </summary>
        public void DisconnectClients()
        {
            List<ClientController> items;
            lock (_lock) 
            { 
                items = _listClient.ToList(); 
            }

            foreach(ClientController item in items)
            {
                item.Disconnect();
            }
            
        }


     }


}


