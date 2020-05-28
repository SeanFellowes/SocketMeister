using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace SocketMeister
{

    internal class TestHarnessClientCollection : IEnumerable<TestHarnessClient>
    {
        private readonly Dictionary<int, TestHarnessClient> _dictClientId = new Dictionary<int, TestHarnessClient>();
        private readonly List<TestHarnessClient> _list = new List<TestHarnessClient>();
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
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }


        /// <summary>
        /// Adds a client to the list and connects it to the test harness control TCP port (Port 4505)
        /// </summary>
        /// <returns>The connected (to the test harness control port) client.</returns>
        public TestHarnessClient AddClient()
        {
            TestHarnessClient newClient = new TestHarnessClient(this);
            _list.Add(newClient);
            _dictClientId.Add(newClient.ClientId, newClient);
            try
            {
                newClient.Connect();
            }
            catch
            {
                _list.Remove(newClient);
                _dictClientId.Remove(newClient.ClientId);
                throw;
            }

            return newClient;
        }


        /// <summary>
        /// Disconnects all clients
        /// </summary>
        public void DisconnectAllClients()
        {

        }


     }


}
