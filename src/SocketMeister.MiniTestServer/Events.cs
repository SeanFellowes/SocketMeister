using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketMeister
{
    public class LogEventArgs : EventArgs
    {
        private readonly Exception _exception;
        private readonly SeverityType _severity;
        private readonly string _source;
        private readonly string _text;
        private readonly DateTime _timeStamp = DateTime.Now;


        public LogEventArgs(SeverityType Severity, string Source, string Text)
        {
            _severity = Severity;
            _source = Source;
            _text = Text;
            _exception = null;
        }

        public LogEventArgs(Exception Exception, string Source)
        {
            _severity = SeverityType.Error;
            _source = Source;
            _text = Exception.Message;
            _exception = Exception;
        }

        public Exception Exception {  get { return _exception; } }
        public SeverityType Severity { get { return _severity; } }
        public string Source { get { return _source; } }
        public string Text { get { return _text; } }
        public string TimeStamp { get { return _timeStamp.ToString("HH:mm:ss fff"); } }


    }

}
