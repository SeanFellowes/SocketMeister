using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using SocketMeister.Testing;

namespace SocketMeister.Testing.ControlBus
{
    /// <summary>
    /// Enhanced ClientCOntroller, with additional properties required for use on the Test Harness
    /// </summary>
    internal class HarnessClientController : ClientController, IDisposable
    {
        private readonly ControlBusCommands _commands = null;
        private bool _disposed = false;
        private bool _disposeCalled = false;
        private SocketServer.Client _controlBuslistenerClient = null;
        private static readonly object _lockMaxClientId = new object();
        private static int _maxClientId = 0;



        public HarnessClientController(int ControlBusClientId) : base(ControlBusClientId)
        {
            _commands = new ControlBusCommands();
        }

        public new void Dispose()
        {
            _disposeCalled = true;
            base.Dispose(true);
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected new virtual void Dispose(bool disposing)
        {
            if (_disposed == true || _disposeCalled == true) return;
            if (disposing)
            {
                _disposeCalled = true;
                _controlBuslistenerClient = null;
                base.Dispose(disposing);
                _disposed = true;
            }
        }


        public ControlBusClientType ClientType { get { return  ControlBusClientType.ClientController; } }

        /// <summary>
        /// Socketmeister client (from the server perspective)
        /// </summary>
        public SocketServer.Client ControlBusListenerClient
        {
            get { lock (Lock) { return _controlBuslistenerClient; } }
            set
            {
                lock (Lock)
                {
                    _controlBuslistenerClient = value;
                }
                _commands.ControlBusListenerClient = value;
            }
        }

        public ControlBusCommands Commands { get { return _commands; } }





        /// <summary>
        /// Creates a new GUI Client Controller running in it's own application
        /// </summary>
        public HarnessClientController() : base(NextClientId())
        { 
        }



        /// <summary>
        /// Adds multiple test harness clients (opens an instance of the WinForms client app for each client)
        /// </summary>
        /// <param name="NumberOfClients">Number of test harness clients to run</param>
        /// <returns>List of TestHarnessClient objects</returns>
        public HarnessClientControllerCollection AddClients(int NumberOfClients)
        {
            HarnessClientControllerCollection rVal = new HarnessClientControllerCollection();
            for (int ctr = 1; ctr <= NumberOfClients; ctr++)
            {
                rVal.Add(new HarnessClientController());
            }
            return rVal;
        }

        public static int NextClientId()
        {
            lock (_lockMaxClientId)
            {
                _maxClientId++;
                return _maxClientId;
            }
        }



        /// <summary>
        /// Launches and instance of the test application and waits for it to connect a socket back so the harness can control it
        /// </summary>
        /// <param name="MaxWaitMilliseconds"></param>
        public void LaunchClientApplication(int MaxWaitMilliseconds = 5000)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"SocketMeister.Test.Client.WinForms.exe";
            process.StartInfo.Arguments = ClientId.ToString();
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            DateTime maxWait = DateTime.Now.AddMilliseconds(MaxWaitMilliseconds);
            while (DateTime.Now < maxWait)
            {
                if (process.HasExited == true)
                {
                    if (process.ExitCode == 1) throw new ApplicationException("Client failed to start. Missing ClientId from process arguments.");
                    else if (process.ExitCode == 3) throw new ApplicationException("Client failed to start. ClientId must be numeric. This is the first parameter");
                    else if (process.ExitCode == 2) throw new ApplicationException("Client failed to start. Couldn't connect to control port " + Constants.ControlBusPort + " (Used to sent test instructions and results between test clients and the test server).");
                    else throw new ApplicationException("Client failed to start. Unknown reason.");
                }

                //  CHECK TO SEE IF THE CLIENT HAS CONNECTED
                if (_controlBuslistenerClient != null)
                {
                    //  CONNECTED
                    return;
                }

                Thread.Sleep(250);
            }
            throw new ApplicationException("Client did not connect within " + MaxWaitMilliseconds + " milliseconds");
        }


        /// <summary>
        /// Sends a message 
        /// </summary>
        public void Disconnect()
        {
            object[] parms = new object[2];
            parms[0] = ControlBus.ControlMessage.ExitClient;
            ControlBusListenerClient.SendMessage(parms);

            //  Wait zzzz miniseconds for the client to send a ClientDisconnecting message.
        }





        public class ControlBusCommands
        {
            private SocketServer.Client _controlBuslistenerClient = null;
            private readonly object _lock = new object();

            public ControlBusCommands()
            {
            }

            public SocketServer.Client ControlBusListenerClient
            {
                get
                {
                    lock (_lock)
                    {
                        if (_controlBuslistenerClient == null)
                            throw new NullReferenceException("Function failed. Property " + nameof(ControlBusListenerClient) + " is null.");
                        return _controlBuslistenerClient;
                    }
                }
                set { lock (_lock) { _controlBuslistenerClient = value; } }
            }


            public byte[] ExecuteMethod(string ClassName, string StaticMethodName, object[] Parameters = null)
            {
                if (Parameters != null)
                {
                    object[] parms = new object[4];
                    parms[0] = ControlMessage.ExecuteMethod;
                    parms[1] = ClassName;
                    parms[2] = StaticMethodName;
                    parms[3] = Serializer.SerializeParameters(Parameters);
                    return ControlBusListenerClient.SendRequest(parms);
                }
                else
                {
                    object[] parms = new object[3];
                    parms[0] = ControlMessage.ExecuteMethod;
                    parms[1] = ClassName;
                    parms[2] = StaticMethodName;
                    return ControlBusListenerClient.SendRequest(parms);
                }
            }

            public void SocketClientStart(List<SocketEndPoint> EndPoints, bool EnableCompression)
            {
                if (EndPoints == null || EndPoints.Count == 0) throw new ArgumentNullException( nameof(EndPoints), "Null or empty list");
                object[] parms = new object[2];
                parms[0] = ControlMessage.SocketClientStart;

                //  SERIALIZE EndPoints
                using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
                {
                    writer.Write(EndPoints.Count);
                    foreach (SocketEndPoint ep in EndPoints)
                    {
                        writer.Write(ep.IPAddress);
                        writer.Write(ep.Port);
                    }
                    writer.Write(EnableCompression);
                    using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                    {
                        reader.BaseStream.Position = 0;
                        parms[1] = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                    }
                }

                ControlBusListenerClient.SendRequest(parms);
            }


            public void SocketClientStop()
            {
                object[] parms = new object[1];
                parms[0] = ControlMessage.SocketClientStop;
                ControlBusListenerClient.SendRequest(parms);
            }









        }


    }
}
