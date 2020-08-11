using System;
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

        private readonly TestHarness _testHarness;

        public TestBase (TestHarness TestHarness, int Id, string Description)
        {
            _testHarness = TestHarness;
            _id = Id;
            _description = Description;
        }

        //  Test Harness
        public TestHarness TestHarness {  get { return _testHarness; } }


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

    }
}
