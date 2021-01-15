using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using SocketMeister1;

namespace SocketMeister1.Server.TestApp
{
    public class GridItem : INotifyPropertyChanged
    {
        DateTime _timeStamp = DateTime.Now;
        string _object = "";
        string _message = "";
        string _clientID = "";
        string _endPointID = "";
        string _messageID = "";
        string _messageType = "";

        public string TimeStamp { get { return _timeStamp.ToString("HH:mm:ss fff"); } }
        public String Object { get { return _object; } }
        public string ClientID { get { return _clientID; } }
        public string EndPointID { get { return _endPointID; } }
        public string Message { get { return _message; } }
        public string MessageID { get { return _messageID; } }
        public string MessageType { get { return _messageType; } }

        public GridItem() { }

        public GridItem(string source, string message, int ClientID, int EndPointID, int MessageID, SocketMessageTypes MessageType)
        {
            _object = source;
            _message = message;
            if (MessageID != 0) _messageID = MessageID.ToString();
            if (ClientID != 0) _clientID = ClientID.ToString();
            if (EndPointID != 0) _endPointID = EndPointID.ToString();
            if (MessageType == SocketMessageTypes.Broadcast) _messageType = "Broadcast";
            else if (MessageType == SocketMessageTypes.Response) _messageType = "Response";
            else if (MessageType == SocketMessageTypes.Request) _messageType = "Request";
            this.NotifyPropertyChanged("TimeStamp");
            this.NotifyPropertyChanged("Source");
            this.NotifyPropertyChanged("ClientID");
            this.NotifyPropertyChanged("EndPointID");
            this.NotifyPropertyChanged("MessageID");
            this.NotifyPropertyChanged("MessageType");
            this.NotifyPropertyChanged("Message");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

}
