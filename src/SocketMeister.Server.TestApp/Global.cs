using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketMeister1.Server.TestApp
{
    internal static class Global
    {
        private static SMServer _server;


        internal static SMServer Server
        {
            get { return _server; }
            set { _server = value; }
        }

    }
}
