using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    internal class TestPercentCompleteChangedEventArgs : EventArgs
    {
        private readonly int percentComplete;

        internal TestPercentCompleteChangedEventArgs(int PercentComplete)
        {
            if (PercentComplete < 0) percentComplete = 0;
            else if (PercentComplete > 100) percentComplete = 100;
            else percentComplete = PercentComplete;
        }

        public int PercentComplete
        {
            get { return percentComplete; }
        }

    }


    internal class TestStatusChangedEventArgs : EventArgs
    {
        private readonly TestStatus status;

        internal TestStatusChangedEventArgs(TestStatus Status)
        {
            status = Status;
        }

        public TestStatus Status
        {
            get { return status; }
        }

    }



    /// <summary>
    /// Information provided when a SocketClient connection to a socket server changes status
    /// </summary>
    public class TestHarnessClientConnectionStatusChangedEventArgs : EventArgs
    {
        private readonly object classLock = new object();
        private string iPAddress = "";
        private ushort port = 0;
        private TestHarnessClientConnectionStatus status = TestHarnessClientConnectionStatus.Disconnected;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="status">The status of the socket</param>
        internal TestHarnessClientConnectionStatusChangedEventArgs(TestHarnessClientConnectionStatus status)
        {
            this.status = status;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="status">The status of the socket</param>
        /// <param name="iPAddress">Destination TCP/IP Port.</param>
        /// <param name="port"></param>
        internal TestHarnessClientConnectionStatusChangedEventArgs(TestHarnessClientConnectionStatus status, string iPAddress, ushort port)
        {
            this.status = status;
            this.iPAddress = iPAddress;
            this.port = port;
        }

        /// <summary>
        /// If connected, the IP Address which the socket is connected to.
        /// </summary>
        public string IPAddress
        {
            get { lock (classLock) { return iPAddress; } }
            set { lock (classLock) { iPAddress = value; } }
        }

        /// <summary>
        /// The port which the socket is connected to.
        /// </summary>
        public ushort Port
        {
            get { lock (classLock) { return port; } }
            set { lock (classLock) { port = value; } }
        }

        /// <summary>
        /// The connection status to the remote socket server.
        /// </summary>
        public TestHarnessClientConnectionStatus Status
        {
            get { lock (classLock) { return status; } }
            set { lock (classLock) { status = value; } }
        }

        /// <summary>
        /// Description of the connection status.
        /// </summary>
        public string StatusDescription
        {
            get { lock (classLock) { return GetStatusDescription(); } }
        }
        private string GetStatusDescription()
        {
            if (status == TestHarnessClientConnectionStatus.Connected) return "Connected";
            else if (status == TestHarnessClientConnectionStatus.Connecting) return "Connecting";
            else if (status == TestHarnessClientConnectionStatus.Disconnected) return "Disconnected";
            else if (status == TestHarnessClientConnectionStatus.Disconnecting) return "Disconnecting";
            else return "Unknown";
        }
    }


}
