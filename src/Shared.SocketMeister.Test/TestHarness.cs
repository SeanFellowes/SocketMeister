//#if TESTHARNESS
//using Microsoft.SqlServer.Server;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Text;

//namespace SocketMeister
//{
//    internal class TestHarness : List<ITest>
//    {
//        private TestHarnessClientCollection _clients = new TestHarnessClientCollection();
//        private static readonly object _lock = new object();
//        private static int _maxClientId = 0;

//        public TestHarness()
//        {
//            //  DYNAMICALLY ADD TESTS. TESTS ARE ANY CLASS NAMED BETWEEN SocketMeister.Test000 and SocketMeister.Test999
//            //  THIS ELIMINATES HAVING TO ADD CODE HERE WHEN A NEW TEST IS CREATED.
//            for (int ctr = 0; ctr <= 999; ctr++ )
//            {
//                string className = typeof(TestHarness).Namespace + ".Test" + ctr.ToString("000", CultureInfo.InvariantCulture);
//                Type t = Type.GetType(className);
//                object[] parms = new object[2];
//                parms[0] = this;
//                parms[1] = ctr;
//                if (t != null) Add((ITest)Activator.CreateInstance(t, parms));
//            }
//        }

//        /// <summary>
//        /// Test Harness Client Collection used during testing.
//        /// </summary>
//        public TestHarnessClientCollection Clients {  get { return _clients; } } 

//        /// <summary>
//        /// Returns the next sequential number to be uised as a ClientId for tests
//        /// </summary>
//        /// <returns></returns>
//        public static int GetNextClientId()
//        {
//            lock (_lock)
//            {
//                _maxClientId++;
//                return _maxClientId;
//            }
//        }
//    }



//}


//#endif