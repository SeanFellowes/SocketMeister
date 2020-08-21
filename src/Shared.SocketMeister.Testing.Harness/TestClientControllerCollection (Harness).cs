using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace SocketMeister.Testing
{

    internal class ClientCollection : IEnumerable<TestClient>
    {
        private readonly Dictionary<int, TestClient> _dictClientId = new Dictionary<int, TestClient>();
        private readonly List<TestClient> _listClient = new List<TestClient>();
        private static readonly object _lock = new object();

        public ClientCollection()
        {
        }

        public TestClient this[int ClientId]
        {
            get
            {
                TestClient client;
                if (_dictClientId.TryGetValue(ClientId, out client) == true) return client;
                else return null;
            }
        }

        IEnumerator<TestClient> IEnumerable<TestClient>.GetEnumerator()
        {
            return _listClient.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _listClient.GetEnumerator();
        }



        ///// <summary>
        ///// Adds a client to the list and connects it to the test harness control TCP port (Port 4505). Opens an instance of the WinForms client app for each client.
        ///// </summary>
        ///// <returns>The connected (to the test harness control port) client.</returns>
        //public Client AddClient()
        //{
        //    Client newClient = new Client(this);
        //    _listClient.Add(newClient);
        //    _dictClientId.Add(newClient.ClientId, newClient);
        //    try
        //    {
        //        newClient.Connect();
        //    }
        //    catch
        //    {
        //        _listClient.Remove(newClient);
        //        _dictClientId.Remove(newClient.ClientId);
        //        throw;
        //    }

        //    return newClient;
        //}

        ///// <summary>
        ///// Adds multiple test harness clients (opens an instance of the WinForms client app for each client)
        ///// </summary>
        ///// <param name="NumberOfClients">Number of test harness clients to run</param>
        ///// <returns>List of TestHarnessClient objects</returns>
        //public List<Client> AddClients(int NumberOfClients)
        //{
        //    List<Client> rVal = new List<Client>();
        //    for (int ctr = 1; ctr <= NumberOfClients; ctr++)
        //    {
        //        rVal.Add(AddClient());
        //    }
        //    return rVal;
        //}



        /// <summary>
        /// Disconnects all clients
        /// </summary>
        public void DisconnectClients()
        {
            List<TestClient> items;
            lock (_lock) 
            { 
                items = _listClient.ToList(); 
            }

            foreach(TestClient item in items)
            {
                item.Disconnect();
            }
            
        }


     }


}


