using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{

    internal class HarnessClientEventArgs : EventArgs
    {
        private readonly HarnessClient _client;

        internal HarnessClientEventArgs(HarnessClient Client)
        {
            _client = Client;
        }

        public HarnessClient Client
        {
            get { return _client; }
        }

    }




}
