using System;

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
