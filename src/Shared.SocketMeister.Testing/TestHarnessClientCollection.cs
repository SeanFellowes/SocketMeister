#if TESTHARNESS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace SocketMeister.Testing
{

    internal class TestHarnessClientCollection : IEnumerable<TestHarnessClient>
    {
        private readonly Dictionary<int, TestHarnessClient> _dictClientId = new Dictionary<int, TestHarnessClient>();
        private readonly List<TestHarnessClient> _listClient = new List<TestHarnessClient>();
        private static readonly object _lock = new object();

        public TestHarnessClientCollection()
        {
        }

        public TestHarnessClient this[int ClientId]
        {
            get
            {
                TestHarnessClient client;
                if (_dictClientId.TryGetValue(ClientId, out client) == true) return client;
                else return null;
            }
        }

        IEnumerator<TestHarnessClient> IEnumerable<TestHarnessClient>.GetEnumerator()
        {
            return _listClient.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _listClient.GetEnumerator();
        }



        /// <summary>
        /// Adds a client to the list and connects it to the test harness control TCP port (Port 4505). Opens an instance of the WinForms client app for each client.
        /// </summary>
        /// <returns>The connected (to the test harness control port) client.</returns>
        public TestHarnessClient AddClient()
        {
            TestHarnessClient newClient = new TestHarnessClient(this);
            _listClient.Add(newClient);
            _dictClientId.Add(newClient.ClientId, newClient);
            try
            {
                newClient.Connect();
            }
            catch
            {
                _listClient.Remove(newClient);
                _dictClientId.Remove(newClient.ClientId);
                throw;
            }

            return newClient;
        }

        /// <summary>
        /// Adds multiple test harness clients (opens an instance of the WinForms client app for each client)
        /// </summary>
        /// <param name="NumberOfClients">Number of test harness clients to run</param>
        /// <returns>List of TestHarnessClient objects</returns>
        public List<TestHarnessClient> AddClients(int NumberOfClients)
        {
            List<TestHarnessClient> rVal = new List<TestHarnessClient>();
            for (int ctr = 1; ctr <= NumberOfClients; ctr++)
            {
                rVal.Add(AddClient());
            }
            return rVal;
        }



        /// <summary>
        /// Disconnects all clients
        /// </summary>
        public void DisconnectClients()
        {
            List<TestHarnessClient> items;
            lock (_lock) 
            { 
                items = _listClient.ToList(); 
            }

            foreach(TestHarnessClient item in items)
            {
                item.Disconnect();
            }
            
        }


     }


}


#endif
