using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    /// <summary>
    /// Used by the test server. Contains a client and fields for tracking tests
    /// </summary>
    internal class TestActor
    {
        private readonly TestClientHarness clientSide;
        private readonly Guid guid = System.Guid.NewGuid();
        private bool controlServerReceivedGuid = false;

        public TestActor()
        {
            clientSide = new TestClientHarness(guid);
        }


        /// <summary>
        /// GUID for this actor
        /// </summary>
        public string Guid {  get { return guid.ToString(); } }

        /// <summary>
        /// Called then ControlServer received a request from the TestClientHarness
        /// </summary>
        public void ControlServerReceivedRequest(SocketServer.RequestReceivedEventArgs e)
        {

        }
    }
}
