using SocketMeister.Messages;
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
    public class Logger : IDisposable
    {
        private bool _disposed = false;
        private readonly object _lock = new object();
        private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();
        private bool _stopPermanently;
        private readonly object _stopPermanentlyLock = new object();

        /// <summary>
        /// Raised when a log entry has been added.
        /// </summary>
        public event EventHandler<LogEventArgs> LogRaised;

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


        public void Log(LogEntry logEntry)
        {
            _logQueue.Enqueue(logEntry);
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
                    LogRaised?.Invoke(this, new LogEventArgs(entry.Message, entry.Severity, entry.EventType));
                }
            }
        }


        /// <summary>
        /// Stop the logger
        /// </summary>
        public void Stop()
        {
            StopPermanently = true;
            //  Pause to allow the background thread to stop
            Thread.Sleep(500);
        }
    }

    /// <summary>
    /// Log entry details
    /// </summary>
    public class LogEntry
    {
        private readonly LogEventType _eventType;
        private readonly long _messageId;
        private readonly string _message;
        private readonly Severity _severity = Severity.Information;
        private readonly DateTime _timeStamp = DateTime.UtcNow;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception</param>
        public LogEntry(Exception exception)
            : this (exception, 0) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="messageId">SocketMeister message id this relates to</param>
        public LogEntry(Exception exception, long messageId)
        {
            _eventType = LogEventType.Exception;
            _message = exception.ToString();
            _messageId = messageId;
            _severity = Severity.Error;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="severity">Severity</param>
        /// <param name="eventType">Log event type</param>
        public LogEntry(string message, Severity severity, LogEventType eventType)
        : this(message, severity, eventType, 0) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="severity">Severity</param>
        /// <param name="eventType">Log event type</param>
        /// <param name="messageId">SocketMeister message id this relates to</param>
        public LogEntry(string message, Severity severity, LogEventType eventType, long messageId)
        {
            _eventType = eventType;
            _message = message;
            _messageId = messageId;
            _severity = severity;
        }

        public DateTime Timestamp => _timeStamp;
        public string Message => _message;
        public Severity Severity => _severity;
        public LogEventType EventType => _eventType;
        public long MessageId => _messageId;
    }

}
