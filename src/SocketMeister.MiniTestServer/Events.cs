using System;

namespace SocketMeister
{
    public class LogEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly Severity _severity;
        private readonly string _server;
        private readonly string _source;
        private readonly string _text;
        private readonly DateTime _timeStamp = DateTime.UtcNow;


        public LogEventArgs(Severity Severity, string Server, string Source, string Text)
        {
            _severity = Severity;
            _server = Server;
            _source = Source;
            _text = Text;
            _exception = null;
        }

        public LogEventArgs(Exception Exception, string Server, string Source)
        {
            _severity = Severity.Error;
            _server = Server;
            _source = Source;
            _text = Exception.Message;
            _exception = Exception;
        }

        public Exception Exception => _exception;
        public Severity Severity => _severity;
        public string Server => _server;
        public string Source => _source;
        public string Text => _text;
        public string TimeStamp => _timeStamp.ToString("HH:mm:ss fff");


    }

}
