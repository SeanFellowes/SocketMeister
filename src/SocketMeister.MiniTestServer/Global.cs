using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketMeister
{
    internal static class Global
    {
        private static SocketServer _server = null;


        internal static SocketServer Server
        {
            get { return _server; }
            set { _server = value; }
        }

    }
}
