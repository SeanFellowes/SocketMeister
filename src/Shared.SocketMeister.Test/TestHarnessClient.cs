using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    internal class TestHarnessClient
    {
        private readonly int _clientId = 0;
        private readonly object _lock = new object();
        private readonly TestHarnessClientCollection _parentCollection;
        private SocketMeister.SocketServer.Client _client = null;

        /// <summary>
        /// Default constructor. Should only be called from TestHarnessClientCollection. Automatically connects to the test harness control port (Port 4505)
        /// </summary>
        /// <param name="ParentCollection"></param>
        public TestHarnessClient(TestHarnessClientCollection ParentCollection)
        {
            _parentCollection = ParentCollection;
            _clientId = TestHarness.GetNextClientId();
        }

        /// <summary>
        /// Socketmeister client (from the server perspective)
        /// </summary>
        public SocketMeister.SocketServer.Client Client
        {
            get { lock (_lock) { return _client; } }
            set { lock (_lock) { _client = value; } }
        }


        public int ClientId {  get { return _clientId; } }

        /// <summary>
        /// TestHarnessClientCollection that this client belongs to
        /// </summary>
        public TestHarnessClientCollection ParentCollection {  get { return _parentCollection; } }

        public void Connect(int MaxWaitMilliseconds = 5000)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"SocketMeister.Test.Client.WinForms.exe";
            process.StartInfo.Arguments = _clientId.ToString();
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            DateTime maxWait = DateTime.Now.AddMilliseconds(MaxWaitMilliseconds);
            while (DateTime.Now < maxWait)
            {
                if (process.HasExited == true)
                {
                    maxWait = DateTime.Now.AddHours(-1);
                    if (process.ExitCode == 1) throw new ApplicationException("Client failed to start. Missing ClientId from process arguments.");
                    else if (process.ExitCode == 3) throw new ApplicationException("Client failed to start. ClientId must be numeric. This is the first parameter");
                    else if (process.ExitCode == 2) throw new ApplicationException("Client failed to start. Couldn't connect to control port 4505 (Used to sent test instructions and results between test clients and the test server).");
                    else throw new ApplicationException("Client failed to start. Unknown reason.");
                }

                //  CHECK TO SEE IF THE CLIENT HAS CONNECTED
                if (_client != null)
                {
                    //  CONNECTED
                    return;
                }

                Thread.Sleep(250);
            }
            throw new ApplicationException("Client did not connect within " + MaxWaitMilliseconds + " milliseconds");
        }

        public void Disconnect()
        {

        }




    }
}
