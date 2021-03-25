using System;
using System.ComponentModel;

namespace SocketMeister.Test
{
    internal class LogEntry : INotifyPropertyChanged
    {
        readonly string eventId = "";
        readonly string message = "";
        readonly string severity = "";
        readonly string source = "";
        readonly string stackTrace = "";
        readonly DateTime timeStamp;

        public string TimeStamp { get { return timeStamp.ToString("HH:mm:ss fff"); } }
        public string Source { get { return source; } }
        public string StackTrace { get { return stackTrace; } }
        public string EventId { get { return eventId; } }
        public string Message { get { return message; } }
        public string Severity { get { return severity; } }

        public LogEntry() { }

        public LogEntry(string source, string message, SeverityType severity, int eventId, string stacktrace = null)
        {
            timeStamp = DateTime.Now;
            this.source = source;
            this.message = message;
            stackTrace = stacktrace;
            if (eventId != 0) this.eventId = eventId.ToString();
            if (severity == SeverityType.Error) this.severity = "Error";
            else if (severity == SeverityType.Information) this.severity = "Information";
            else if (severity == SeverityType.Warning) this.severity = "Warning";
            NotifyPropertyChanged("TimeStamp");
            NotifyPropertyChanged("Source");
            NotifyPropertyChanged("EventId");
            NotifyPropertyChanged("Message");
            NotifyPropertyChanged("Severity");
            if (stackTrace != null) NotifyPropertyChanged("StackTrace");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
