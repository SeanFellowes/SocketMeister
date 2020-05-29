//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SocketMeister
//{
//    /// <summary>
//    /// Used by the test server. Contains a client and fields for tracking tests
//    /// </summary>
//    internal class TestActor
//    {
//        private readonly TestClientHarness clientSide;
//        //private readonly Guid guid = System.Guid.NewGuid();
//        //private bool controlServerReceivedGuid = false;

//        public TestActor()
//        {
//            clientSide = new TestClientHarness(12345);
//        }


//        ///// <summary>
//        ///// GUID for this actor
//        ///// </summary>
//        //public int ClientId {  get { return _clie; } }

//        /// <summary>
//        /// Called then ControlServer received a request from the TestClientHarness
//        /// </summary>
//        public void ControlServerReceivedRequest(SocketServer.RequestReceivedEventArgs e)
//        {

//        }
//    }
//}
