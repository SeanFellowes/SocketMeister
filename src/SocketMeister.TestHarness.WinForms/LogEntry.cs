using System;
using System.ComponentModel;

namespace SocketMeister.Test
{
    internal class LogEntry : INotifyPropertyChanged
    {
        private readonly string eventId = "";
        private readonly string message = "";
        private readonly string severity = "";
        private readonly string source = "";
        private readonly string stackTrace = "";
        private readonly DateTime timeStamp;

        public string TimeStamp => timeStamp.ToString("HH:mm:ss fff");
        public string Source => source;
        public string StackTrace => stackTrace;
        public string EventId => eventId;
        public string Message => message;
        public string Severity => severity;

        public LogEntry() { }

        public LogEntry(string source, string message, SeverityType severity, int eventId, string stacktrace = null)
        {
            timeStamp = DateTime.UtcNow;
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
