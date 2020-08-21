﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

#if TESTHARNESS
using System.Management.Instrumentation;
#endif

namespace SocketMeister.Testing.Tests
{
    internal partial class TestBase
    {



        /// <summary>
        /// Adds a client to the list and connects it to the test harness control TCP port (Port 4505). Opens an instance of the WinForms client app for each client.
        /// </summary>
        /// <returns>The connected (to the test harness control port) client.</returns>
        public TestClient AddClient()
        {
            TestClient newClient = new TestClient();
            if (ClientCreated != null) ClientCreated(this, new ClientEventArgs(newClient));
            try
            {
                newClient.LaunchClientApplication();
            }
            catch
            {
                if (ClientConnectFailed != null) ClientConnectFailed(this, new ClientEventArgs(newClient));
                throw;
            }

            return newClient;
        }

        /// <summary>
        /// Adds multiple test harness clients (opens an instance of the WinForms client app for each client)
        /// </summary>
        /// <param name="NumberOfClients">Number of test harness clients to run</param>
        /// <returns>List of TestHarnessClient objects</returns>
        public List<TestClient> AddClients(int NumberOfClients)
        {
            List<TestClient> rVal = new List<TestClient>();
            for (int ctr = 1; ctr <= NumberOfClients; ctr++)
            {
                rVal.Add(AddClient());
            }
            return rVal;
        }

    }
}