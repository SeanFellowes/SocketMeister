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
        readonly string eventId = "";
        readonly string message = "";
        readonly string severity = "";
        readonly string source = "";
        readonly DateTime timeStamp;

        public string TimeStamp { get { return timeStamp.ToString("HH:mm:ss fff"); } }
        public string Source { get { return source; } }
        public string EventId { get { return eventId; } }
        public string Message { get { return message; } }
        public string Severity { get { return severity; } }

        public LogEntry() { }

        public LogEntry(string source, string message, SeverityType severity, int eventId)
        {
            timeStamp = DateTime.Now;
            this.source = source;
            this.message = message;
            if (eventId != 0) this.eventId = eventId.ToString();
            if (severity == SeverityType.Error) this.severity = "Error";
            else if (severity == SeverityType.Information) this.severity = "Information";
            else if (severity == SeverityType.Warning) this.severity = "Warning";
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
