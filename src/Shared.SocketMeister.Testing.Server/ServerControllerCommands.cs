using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Commands executed on the server, via the ControlBus 
    /// </summary>
    internal static class ServerControllerCommands
    {

        internal static byte[] ClientToServerSendRequestEcho01(ServerController Controller, int MessageId, int MessageLength, int TimeoutMilliseconds = 60000)
        {
            OpenTransaction oMsg = Controller.OpenMessages.Add(MessageId);
            DateTime maxWait = DateTime.Now.AddMilliseconds(TimeoutMilliseconds);
            while (true)
            {
                if (oMsg.IsTransacted == true)
                {
                    object[] p = (object[])oMsg.UserToken;
                    byte[] sentMsg = (byte[])p[3];
                    if (sentMsg.Length != MessageLength) throw new ApplicationException("Expected message length " + MessageLength);
                    else return sentMsg;
                }
                else if (DateTime.Now > maxWait) throw new TimeoutException();
                else Thread.Sleep(25);
            }
        }
    }

    internal class OpenTransactions
    {
        private readonly List<OpenTransaction> _items = new List<OpenTransaction>();
        private readonly object _lock = new object();

        public OpenTransactions()
        {

        }

        public OpenTransaction this[int TransactionId]
        {
            get
            {
                lock (_lock)
                {
                    foreach (OpenTransaction trans in _items)
                    {
                        if (trans.TransactionId == TransactionId) return trans;
                    }
                }
                return null;
            }
        }


        public OpenTransaction Add(int MessageId)
        {
            OpenTransaction rVal = new OpenTransaction(MessageId);
            lock (_lock) { _items.Add(rVal); }
            return rVal;
        }

        public void Clear()
        {
            lock (_lock) { _items.Clear(); }
        }


        public void Remove(OpenTransaction Item)
        {
            lock(_lock) { _items.Remove(Item); }
        }
    }


    internal class OpenTransaction
    {
        private bool _isTransacted;
        private object _userToken = null;
        private readonly object _lock = new object();

        public OpenTransaction(int TransactionId)
        {
            this.TransactionId = TransactionId;
            this.TimeStamp = DateTime.Now;
        }


        public bool IsTransacted
        {
            get { lock (_lock) { return _isTransacted; } }
            set { lock(_lock) { _isTransacted = value; } }
        }

        public int TransactionId { get; }

        public DateTime TimeStamp { get; }

        public object UserToken
        {
            get { lock (_lock) { return _userToken; } }
            set { lock (_lock) { _userToken = value; } }
        }


    }
}
