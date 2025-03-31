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

            if (LogRaised != null && batch.Count > 0)
            {
                // Sort by Timestamp. Given the multithreading nature of SocketMeister, the log entries may not be in order.
                batch.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

                // Emit the log entries. This could be an event invocation or direct logging.
                foreach (var entry in batch)
                {
                    try
                    {
                        LogRaised(this, new LogEventArgs(entry));
                    }
                    catch
                    {
                        //  Swallow exceptions
                    }
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
}
