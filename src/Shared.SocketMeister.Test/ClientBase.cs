//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SocketMeister
//{
//    internal class ClientBase
//    {
//        private int _clientId;
//        public readonly object Lock = new object();

//        public int ClientId { 
//            get { lock (Lock) { return _clientId; } } 
//            set { lock (Lock) { _clientId = value; } }
//        }


//    }
//}
