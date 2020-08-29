using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace SocketMeister.Testing
{

    internal class HarnessClientControllerCollection : IEnumerable<HarnessClientController>
    {
        private readonly Dictionary<int, HarnessClientController> _dictClientId = new Dictionary<int, HarnessClientController>();
        private readonly List<HarnessClientController> _listClient = new List<HarnessClientController>();
        private static readonly object _lock = new object();

        public HarnessClientControllerCollection()
        {
        }

        public HarnessClientController this[int ClientId]
        {
            get
            {
                HarnessClientController client;
                if (_dictClientId.TryGetValue(ClientId, out client) == true) return client;
                else return null;
            }
        }

        public void Add(HarnessClientController Item)
        {
            if (_dictClientId.ContainsKey(Item.ClientId)) throw new ArgumentException("ClientId " + Item.ClientId + " already exists in collection.", nameof(Item));
            _dictClientId.Add(Item.ClientId, Item);
            _listClient.Add(Item);
        }

        IEnumerator<HarnessClientController> IEnumerable<HarnessClientController>.GetEnumerator()
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
            List<HarnessClientController> items;
            lock (_lock) 
            { 
                items = _listClient.ToList(); 
            }

            foreach(HarnessClientController item in items)
            {
                item.Disconnect();
            }
            
        }


     }


}


