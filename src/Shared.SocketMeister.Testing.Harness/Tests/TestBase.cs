using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace SocketMeister.Testing.Tests
{
    internal partial class TestBase
    {

        private static int _maxClientId = 0;
        private static readonly object _lockMaxClientId = new object();

        /// <summary>
        /// Raised when a test creates a client
        /// </summary>
        public event EventHandler<HarnessClientEventArgs> ClientCreated;

        /// <summary>
        /// Raised when an an attempt to establish a socket for control messages between a client and server failed.   
        /// </summary>
        public event EventHandler<HarnessClientEventArgs> ClientConnectFailed;



        /// <summary>
        /// Adds a client to the list and connects it to the test harness control TCP port (Port 4505). Opens an instance of the WinForms client app for each client.
        /// </summary>
        /// <returns>The connected (to the test harness control port) client.</returns>
        public HarnessClient AddClient()
        {
            int nextClientId = 0;
            lock (_lockMaxClientId)
            {
                _maxClientId++;
                nextClientId = _maxClientId;
            }
            HarnessClient newClient = new HarnessClient(nextClientId);
            if (ClientCreated != null) ClientCreated(this, new HarnessClientEventArgs(newClient));
            try
            {
                newClient.LaunchClientApplication();
            }
            catch
            {
                if (ClientConnectFailed != null) ClientConnectFailed(this, new HarnessClientEventArgs(newClient));
                throw;
            }

            return newClient;
        }

        /// <summary>
        /// Adds multiple test harness clients (opens an instance of the WinForms client app for each client)
        /// </summary>
        /// <param name="NumberOfClients">Number of test harness clients to run</param>
        /// <returns>List of TestHarnessClient objects</returns>
        public List<HarnessClient> AddClients(int NumberOfClients)
        {
            List<HarnessClient> rVal = new List<HarnessClient>();
            for (int ctr = 1; ctr <= NumberOfClients; ctr++)
            {
                rVal.Add(AddClient());
            }
            return rVal;
        }

    }
}
