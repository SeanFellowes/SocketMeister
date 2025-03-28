using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
#if !NET35
using System.Collections.Concurrent;
using System.Threading.Tasks;
#endif

namespace SocketMeister
{
    class SocketClientLogger
    {
#if NET35
        public SocketClientLogger()
        {
        }
#else

        private readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly TimeSpan _flushInterval = TimeSpan.FromMilliseconds(250);

        public SocketClientLogger()
        {
            // Start the background task that processes log entries.
            Task.Run(ProcessLogQueue, _cts.Token);
        }

        public void Log(string message, LogLevel level)
        {
            _logQueue.Enqueue(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Message = message,
                Level = level
            });
        }

        private async Task ProcessLogQueue()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(_flushInterval, _cts.Token);
                ProcessBatch();
            }
        }

        private void ProcessBatch()
        {
            var batch = new System.Collections.Generic.List<LogEntry>();
            while (_logQueue.TryDequeue(out var entry))
            {
                batch.Add(entry);
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

        public void Dispose()
        {
            _cts.Cancel();
        }
#endif

    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogLevel Level { get; set; }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
