using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{
    internal class ClientEventArgs : EventArgs
    {
        private readonly ClientController _client;

        internal ClientEventArgs(ClientController Client)
        {
            _client = Client;
        }

        public ClientController Client
        {
            get { return _client; }
        }

    }
}
