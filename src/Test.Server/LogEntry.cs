using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using SocketMeister;

namespace SocketMeister.Test
{
    internal class LogEntry : INotifyPropertyChanged
    {
        readonly DateTime _timeStamp;
        readonly string _source = "";
        readonly string _message = "";
        readonly string _eventId = "";
        readonly string _severity = "";

        public string TimeStamp { get { return _timeStamp.ToString("HH:mm:ss fff"); } }
        public string Source { get { return _source; } }
        public string EventId { get { return _eventId; } }
        public string Message { get { return _message; } }
        public string Severity { get { return _severity; } }

        public LogEntry() { }

        public LogEntry(string Source, string Message, SeverityType Severity, int EventId)
        {
            _timeStamp = DateTime.Now;
            _source = Source;
            _message = Message;
            if (EventId != 0) _eventId = EventId.ToString();
            if (Severity == SeverityType.Error) _severity = "Error";
            else if (Severity == SeverityType.Information) _severity = "Information";
            else if (Severity == SeverityType.Warning) _severity = "Warning";
            this.NotifyPropertyChanged("TimeStamp");
            this.NotifyPropertyChanged("Source");
            this.NotifyPropertyChanged("EventId");
            this.NotifyPropertyChanged("Message");
            this.NotifyPropertyChanged("Severity");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
