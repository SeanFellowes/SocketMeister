using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// A simple logger that logs messages to the console and raises log events to calling code.
    /// This logger is thread safe. It reduces the number of background threads used for raising
    /// logging events by batching log entries and processing them in a single background thread.
    /// </summary>
    class Logger : IDisposable
    {
        private bool _disposed = false;
        private readonly object _lock = new object();
        private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();
        private bool _stopPermanently;
        private readonly object _stopPermanentlyLock = new object();

        /// <summary>
        /// Raised when a log entry has been added.
        /// </summary>
        public event EventHandler<TraceEventArgs> LogRaised;

        public Logger()
        {
            Thread bgWorker = new Thread(new ThreadStart(delegate
            {
                try
                {
                    while (!StopPermanently)
                    {
                        ProcessBatch();
                        //  Wait for the flush interval
                        Thread.Sleep(200);
                    }
                }
                catch (Exception e) 
                {
                    //  Swallow exceptions
                    Debug.Write(e.ToString());
                }
            }));
            bgWorker.IsBackground = true;
            bgWorker.Start();

        }



        /// <summary>
        /// Dispose of the class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the class
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes of the resources used by the class.
        /// </summary>
        ~Logger()
        {
            Dispose(false);
        }


        private bool StopPermanently { get { lock (_stopPermanentlyLock) { return _stopPermanently; } } set { lock (_stopPermanentlyLock) { _stopPermanently = value; } } }

        public void Log(string message, Severity level)
        {
            _logQueue.Enqueue(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Message = message,
                Level = level
            });
        }


        private void ProcessBatch()
        {
            var batch = new List<LogEntry>();

            lock(_lock)
            {
                while (_logQueue.Count > 0)
                {
                    batch.Add(_logQueue.Dequeue());
                }
            }

            if (batch.Count > 0)
            {
                // Optionally, sort by Timestamp if necessary.
                batch.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

                // Emit the log entries. This could be an event invocation or direct logging.
                foreach (var entry in batch)
                {
                    // For example, raise an event or write to a file/console.
                    Console.WriteLine($"[{entry.Timestamp:O}] {entry.Level}: {entry.Message}");
                }
            }
        }


        /// <summary>
        /// Stop the logger
        /// </summary>
        public void Stop()
        {
            StopPermanently = true;
        }



    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public Severity Level { get; set; }
    }

    public enum Severity
    {
        Info,
        Warning,
        Error
    }

    public class LogEventArgs : EventArgs
    {
        private readonly int _eventId;
        private readonly string _message;
        private readonly Severity _severity;
        private readonly string _source;
        private readonly string _stackTrace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the trace event</param>
        /// <param name="severity">Severity of the trace event.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public LogEventArgs(string message, Severity severity, int eventId)
        {
            this._message = message;
            this._severity = severity;
            this._eventId = eventId;
            _source = null;
            _stackTrace = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the trace event</param>
        /// <param name="severity">Severity of the trace event.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        /// <param name="source">Source of the trace event.</param>
        public LogEventArgs(string message, Severity severity, int eventId, string source)
        {
            this._message = message;
            this._severity = severity;
            this._eventId = eventId;
            this._source = source;
            _stackTrace = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception which occured.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public LogEventArgs(Exception exception, int eventId)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _message = exception.Message;
            _severity = Severity.Error;
            this._eventId = eventId;
            _source = null;
            if (exception.StackTrace != null) _stackTrace = exception.StackTrace;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception which occured.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        /// <param name="source">Source of the trace event.</param>
        public LogEventArgs(Exception exception, int eventId, string source)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            _message = exception.Message;
            _severity = Severity.Error;
            this._eventId = eventId;
            this._source = source;
            if (exception.StackTrace != null) _stackTrace = exception.StackTrace;
        }

        /// <summary>
        /// Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).
        /// </summary>
        public int EventId => _eventId;

        /// <summary>
        /// Message describing the trace event
        /// </summary>
        public string Message => _message;

        /// <summary>
        /// Severity of the trace event.
        /// </summary>
        public Severity Severity => _severity;

        /// <summary>
        /// Optional source of the trace event.
        /// </summary>
        public string Source => _source;

        /// <summary>
        /// Optional stack trace information.
        /// </summary>
        public string StackTrace => _stackTrace;
    }

}
