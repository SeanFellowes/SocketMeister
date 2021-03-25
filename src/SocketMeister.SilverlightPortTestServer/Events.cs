using System;

namespace SocketMeister
{
    public class LogEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly SeverityType _severity;
        private readonly string _server;
        private readonly string _source;
        private readonly string _text;
        private readonly DateTime _timeStamp = DateTime.Now;


        public LogEventArgs(SeverityType Severity, string Server, string Source, string Text)
        {
            _severity = Severity;
            _server = Server;
            _source = Source;
            _text = Text;
            _exception = null;
        }

        public LogEventArgs(Exception Exception, string Server, string Source)
        {
            _severity = SeverityType.Error;
            _server = Server;
            _source = Source;
            _text = Exception.Message;
            _exception = Exception;
        }

        public Exception Exception => _exception;
        public SeverityType Severity => _severity;
        public string Server => _server;
        public string Source => _source;
        public string Text => _text;
        public string TimeStamp => _timeStamp.ToString("HH:mm:ss fff");


    }

}
