//using System;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Reflection;
//using System.Threading;

//namespace SocketMeister
//{
//    /// <summary>
//    /// Base class for SocketClient and SocketServer.Client
//    /// </summary>
//#if SMISPUBLIC
//    public abstract class ClientBase : IDisposable
//#else
//    internal abstract class ClientBase : IDisposable
//#endif
//    {
//        private bool _disposed = false;
//        //private readonly UnrespondedMessageCollection _unrespondedMessages = new UnrespondedMessageCollection();

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        public ClientBase()
//        {
//        }

//        /// <summary>
//        /// Disposes of the resources used by the class.
//        /// </summary>
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        /// <summary>
//        /// Releases resources.
//        /// </summary>
//        /// <param name="disposing">True if managed resources should be released.</param>
//        protected virtual void Dispose(bool disposing)
//        {
//            if (!_disposed)
//            {
//                if (disposing)
//                {
//                    // Dispose managed resources here if any
//                }
//                //_unrespondedMessages.Clear(); // Explicitly clear any remaining references

//                // Release unmanaged resources if any
//                _disposed = true;
//            }
//        }

//        /// <summary>
//        /// Disposes of the resources used by the class.
//        /// </summary>
//        ~ClientBase()
//        {
//            Dispose(false);
//        }



//        //internal UnrespondedMessageCollection UnrespondedMessages
//        //{
//        //    get { return _unrespondedMessages; }
//        //}


//    }
//}
