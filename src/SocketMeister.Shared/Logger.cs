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
    /// A simple logger that logs messages to the console and raises log events to the calling code.
    /// This logger is thread-safe. It reduces the number of background threads used for raising
    /// logging events by batching log entries and processing them in a single background thread.
    /// </summary>
    internal partial class Logger : IDisposable
    {
        private bool _disposed = false;
        private readonly object _lock = new object();
        private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();
        private bool _stopPermanently;
        private readonly object _stopPermanentlyLock = new object();

        /// <summary>
        /// Raised when a log entry is added.
        /// </summary>
        public event EventHandler<LogEventArgs> LogRaised;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        public Logger()
        {
            Thread bgWorker = new Thread(new ThreadStart(delegate
            {
                try
                {
                    while (!StopPermanently)
                    {
                        ProcessBatch();
                        // Wait for the flush interval.
                        Thread.Sleep(200);
                    }
                }
                catch (Exception e)
                {
                    // Swallow exceptions to prevent the background thread from crashing.
                    Debug.Write(e.ToString());
                }
            }))
            {
                IsBackground = true
            };
            bgWorker.Start();
        }

        /// <summary>
        /// Disposes of the resources used by the logger.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the resources used by the logger.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose or the finalizer.</param>
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
        /// Finalizer to ensure resources are released.
        /// </summary>
        ~Logger()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the logger should stop permanently.
        /// </summary>
        private bool StopPermanently
        {
            get { lock (_stopPermanentlyLock) { return _stopPermanently; } }
            set { lock (_stopPermanentlyLock) { _stopPermanently = value; } }
        }

        /// <summary>
        /// Adds a log entry to the queue for processing.
        /// </summary>
        /// <param name="logEntry">The log entry to add.</param>
        public void Log(LogEntry logEntry)
        {
            _logQueue.Enqueue(logEntry);
        }

        /// <summary>
        /// Processes a batch of log entries from the queue and raises the <see cref="LogRaised"/> event.
        /// </summary>
        private void ProcessBatch()
        {
            var batch = new List<LogEntry>();

            lock (_lock)
            {
                while (_logQueue.Count > 0)
                {
                    batch.Add(_logQueue.Dequeue());
                }
            }

            if (LogRaised != null && batch.Count > 0)
            {
                // Sort log entries by timestamp to ensure chronological order.
                batch.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

                // Emit the log entries.
                foreach (var entry in batch)
                {
                    try
                    {
                        LogRaised(this, new LogEventArgs(entry));
                    }
                    catch
                    {
                        // Swallow exceptions to prevent the logger from crashing.
                    }
                }
            }
        }

        /// <summary>
        /// Stops the logger and allows the background thread to terminate gracefully.
        /// </summary>
        public void Stop()
        {
            StopPermanently = true;
            // Pause to allow the background thread to stop.
            Thread.Sleep(500);
        }
    }
}
