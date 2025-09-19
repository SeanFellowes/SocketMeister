#if SOCKETMEISTER_TELEMETRY
using System;
using System.Diagnostics;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// Lightweight, thread-safe runtime telemetry view for SocketMeister. Provides a live read-only view and
    /// the ability to produce immutable snapshots for consistent multi-field reads.
    /// </summary>
    public sealed class SocketTelemetry : IDisposable
    {
        // --- Configuration (defaults; tunable internally in later commits) ---
        private const int DefaultUpdateIntervalSeconds = 5;   // cadence target: < 10s
        private const int DefaultEwmaWindowSeconds = 15;      // smoothing window

        // --- Lifecycle ---
        private volatile bool _disposed;
        private Timer _timer; // System.Threading.Timer
        private int _timerGate; // 0 = idle, 1 = ticking (non-reentrant guard)
        private volatile bool _enabled = true; // runtime switch

        // --- Monotonic clock for cadence ---
        private readonly Stopwatch _stopwatch;
        private long _lastSampleTicks; // Stopwatch ticks of last sample
        private long _lastTotalMessages;
        private long _lastTotalCompressedBytes;

        // --- Uptime origins (UTC ticks) ---
        private long _processStartUtcTicks;
        private long _sessionStartUtcTicks;

        // --- Atomic counters (long) ---
        private long _currentConnections;
        private long _maxConnections;
        private long _totalMessages;
        private long _totalFailures;
        private long _totalCompressedBodyBytes;
        private long _totalUncompressedBodyBytes;
        private long _compressionSavingsBytes;
        private long _reconnects;
        private long _protocolErrors;

        // --- Aggregated rates (double) ---
        private double _avgMessageThroughput;       // messages/sec
        private double _avgBitrateBitsPerSecond;    // bits/sec

        private int _updateIntervalSeconds = DefaultUpdateIntervalSeconds;
        private int _ewmaWindowSeconds = DefaultEwmaWindowSeconds;

        // --- Construction ---
        public SocketTelemetry()
        {
            _stopwatch = Stopwatch.StartNew();
            long nowUtcTicks = DateTime.UtcNow.Ticks;
            _processStartUtcTicks = nowUtcTicks;
            _sessionStartUtcTicks = nowUtcTicks;

            // Initialize the sampler baseline lazily on first tick
            _lastSampleTicks = 0;
            _lastTotalMessages = 0;
            _lastTotalCompressedBytes = 0;

            // Start low-frequency timer
            _timer = new Timer(TimerCallback, null, _updateIntervalSeconds * 1000, _updateIntervalSeconds * 1000);
        }

        // --- Public read-only properties ---
        /// <summary>Number of active connections. Client: 0/1. Server: 0..N.</summary>
        public long CurrentConnections { get { return Interlocked.Read(ref _currentConnections); } }

        /// <summary>Peak observed concurrent connections since uptime origin.</summary>
        public long MaxConnections { get { return Interlocked.Read(ref _maxConnections); } }

        /// <summary>Seconds since process-level start (server start; client first-ever successful connection).</summary>
        public long ProcessUptimeSeconds { get { return TicksToSeconds(DateTime.UtcNow.Ticks - Interlocked.Read(ref _processStartUtcTicks)); } }

        /// <summary>Seconds since current active session began (client reconnect resets; server restart resets).</summary>
        public long SessionUptimeSeconds { get { return TicksToSeconds(DateTime.UtcNow.Ticks - Interlocked.Read(ref _sessionStartUtcTicks)); } }

        /// <summary>Total successful messages (send + receive) since session start.</summary>
        public long TotalMessages { get { return Interlocked.Read(ref _totalMessages); } }

        /// <summary>Total failed send attempts since session start.</summary>
        public long TotalFailures { get { return Interlocked.Read(ref _totalFailures); } }

        /// <summary>Rolling average messages per second.</summary>
        public double AvgMessageThroughput { get { return AtomicReadDouble(ref _avgMessageThroughput); } }

        /// <summary>Rolling average bitrate in bits per second (decimal units when displayed).</summary>
        public double AvgBitrateBitsPerSecond { get { return AtomicReadDouble(ref _avgBitrateBitsPerSecond); } }

        /// <summary>Observed compression ratio derived from total compressed/uncompressed body bytes.</summary>
        public double CompressionRatio
        {
            get
            {
                long uncompressed = Interlocked.Read(ref _totalUncompressedBodyBytes);
                if (uncompressed <= 0) return 0d;
                long compressed = Interlocked.Read(ref _totalCompressedBodyBytes);
                return (double)compressed / (double)uncompressed;
            }
        }

        /// <summary>Total bytes saved by compression since session start.</summary>
        public long CompressionSavingsBytes { get { return Interlocked.Read(ref _compressionSavingsBytes); } }

        /// <summary>Total compressed body bytes observed (send + receive) since session start.</summary>
        public long TotalCompressedBodyBytes { get { return Interlocked.Read(ref _totalCompressedBodyBytes); } }

        /// <summary>Total uncompressed body bytes observed (send + receive) since session start.</summary>
        public long TotalUncompressedBodyBytes { get { return Interlocked.Read(ref _totalUncompressedBodyBytes); } }

        /// <summary>Number of successful reconnects (client) or accepted handshakes (server) since session start.</summary>
        public long Reconnects { get { return Interlocked.Read(ref _reconnects); } }

        /// <summary>Number of protocol/framing errors observed since session start.</summary>
        public long ProtocolErrors { get { return Interlocked.Read(ref _protocolErrors); } }

        /// <summary>
        /// Produces an immutable snapshot representing a consistent point-in-time view of telemetry values.
        /// </summary>
        public SocketTelemetrySnapshot GetSnapshot()
        {
            long currConn = Interlocked.Read(ref _currentConnections);
            long maxConn = Interlocked.Read(ref _maxConnections);
            long totalMsg = Interlocked.Read(ref _totalMessages);
            long totalFail = Interlocked.Read(ref _totalFailures);
            long compBytes = Interlocked.Read(ref _totalCompressedBodyBytes);
            long uncompBytes = Interlocked.Read(ref _totalUncompressedBodyBytes);
            long savings = Interlocked.Read(ref _compressionSavingsBytes);
            long reconn = Interlocked.Read(ref _reconnects);
            long protErr = Interlocked.Read(ref _protocolErrors);

            double avgMsg = AtomicReadDouble(ref _avgMessageThroughput);
            double avgBit = AtomicReadDouble(ref _avgBitrateBitsPerSecond);

            long procUp = TicksToSeconds(DateTime.UtcNow.Ticks - Interlocked.Read(ref _processStartUtcTicks));
            long sessUp = TicksToSeconds(DateTime.UtcNow.Ticks - Interlocked.Read(ref _sessionStartUtcTicks));

            double ratio = uncompBytes > 0 ? (double)compBytes / (double)uncompBytes : 0d;

            return new SocketTelemetrySnapshot(
                currConn,
                maxConn,
                procUp,
                sessUp,
                totalMsg,
                totalFail,
                avgMsg,
                avgBit,
                compBytes,
                uncompBytes,
                savings,
                ratio,
                reconn,
                protErr);
        }

        /// <summary>
        /// Resets all counters and uptime origins. Intended for internal/testing scenarios; discouraged for production consumers.
        /// </summary>
        internal void Reset()
        {
            Interlocked.Exchange(ref _currentConnections, 0);
            Interlocked.Exchange(ref _maxConnections, 0);
            Interlocked.Exchange(ref _totalMessages, 0);
            Interlocked.Exchange(ref _totalFailures, 0);
            Interlocked.Exchange(ref _totalCompressedBodyBytes, 0);
            Interlocked.Exchange(ref _totalUncompressedBodyBytes, 0);
            Interlocked.Exchange(ref _compressionSavingsBytes, 0);
            Interlocked.Exchange(ref _reconnects, 0);
            Interlocked.Exchange(ref _protocolErrors, 0);

            Interlocked.Exchange(ref _processStartUtcTicks, DateTime.UtcNow.Ticks);
            Interlocked.Exchange(ref _sessionStartUtcTicks, Interlocked.Read(ref _processStartUtcTicks));

            // Reset aggregator baseline
            _lastSampleTicks = 0;
            _lastTotalMessages = 0;
            _lastTotalCompressedBytes = 0;
            AtomicWriteDouble(ref _avgMessageThroughput, 0d);
            AtomicWriteDouble(ref _avgBitrateBitsPerSecond, 0d);
        }

        // --- IDisposable ---
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            var t = _timer;
            if (t != null)
            {
                try { t.Change(Timeout.Infinite, Timeout.Infinite); }
                catch { }
                try { t.Dispose(); } catch { }
            }
            _timer = null;
        }

        // --- Internal update API (wired in Commit 3) ---
        internal void MarkProcessStartNow()
        {
            Interlocked.Exchange(ref _processStartUtcTicks, DateTime.UtcNow.Ticks);
        }

        internal void MarkSessionStartNow()
        {
            Interlocked.Exchange(ref _sessionStartUtcTicks, DateTime.UtcNow.Ticks);
        }

        internal void IncrementCurrentConnections()
        {
            if (!_enabled) return;
            long newVal = Interlocked.Increment(ref _currentConnections);
            UpdateMax(ref _maxConnections, newVal);
        }

        internal void DecrementCurrentConnections()
        {
            if (!_enabled) return;
            Interlocked.Decrement(ref _currentConnections);
        }

        internal void AddSendSuccess(long compressedBytes, long uncompressedBytes)
        {
            if (!_enabled) return;
            Interlocked.Increment(ref _totalMessages);
            InterlockedAdd(ref _totalCompressedBodyBytes, compressedBytes);
            InterlockedAdd(ref _totalUncompressedBodyBytes, uncompressedBytes);
            long deltaSave = uncompressedBytes - compressedBytes;
            if (deltaSave > 0) InterlockedAdd(ref _compressionSavingsBytes, deltaSave);
        }

        internal void AddReceiveSuccess(long compressedBytes, long uncompressedBytes)
        {
            if (!_enabled) return;
            Interlocked.Increment(ref _totalMessages);
            InterlockedAdd(ref _totalCompressedBodyBytes, compressedBytes);
            InterlockedAdd(ref _totalUncompressedBodyBytes, uncompressedBytes);
            long deltaSave = uncompressedBytes - compressedBytes;
            if (deltaSave > 0) InterlockedAdd(ref _compressionSavingsBytes, deltaSave);
        }

        internal void AddSendFailure()
        {
            if (!_enabled) return;
            Interlocked.Increment(ref _totalFailures);
        }

        internal void AddProtocolError()
        {
            if (!_enabled) return;
            Interlocked.Increment(ref _protocolErrors);
        }

        internal void AddReconnect()
        {
            if (!_enabled) return;
            Interlocked.Increment(ref _reconnects);
        }

        internal void SetUpdateIntervalSeconds(int seconds)
        {
            if (seconds < 1) seconds = 1;
            if (seconds > 10) seconds = 10;
            _updateIntervalSeconds = seconds;
            var t = _timer;
            if (t != null)
            {
                try { t.Change(_updateIntervalSeconds * 1000, _updateIntervalSeconds * 1000); } catch { }
            }
        }

        internal void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            var t = _timer;
            if (!enabled)
            {
                if (t != null)
                {
                    try { t.Change(Timeout.Infinite, Timeout.Infinite); } catch { }
                    try { t.Dispose(); } catch { }
                }
                _timer = null;
            }
            else
            {
                if (t == null)
                {
                    _lastSampleTicks = 0; // restart sampling
                    _timer = new Timer(TimerCallback, null, _updateIntervalSeconds * 1000, _updateIntervalSeconds * 1000);
                }
            }
        }

        // --- Timer callback ---
        private void TimerCallback(object state)
        {
            if (_disposed) return;
            if (!_enabled) return;
            if (Interlocked.Exchange(ref _timerGate, 1) == 1) return; // non-reentrant
            try
            {
                SampleNow();
            }
            finally
            {
                Interlocked.Exchange(ref _timerGate, 0);
            }
        }

        private void SampleNow()
        {
            long nowTicks = _stopwatch.ElapsedTicks;
            if (_lastSampleTicks == 0)
            {
                _lastSampleTicks = nowTicks;
                _lastTotalMessages = Interlocked.Read(ref _totalMessages);
                _lastTotalCompressedBytes = Interlocked.Read(ref _totalCompressedBodyBytes);
                return;
            }

            long deltaTicks = nowTicks - _lastSampleTicks;
            if (deltaTicks <= 0) return;
            double seconds = (double)deltaTicks / (double)Stopwatch.Frequency;
            if (seconds <= 0d) return;

            long totalMsg = Interlocked.Read(ref _totalMessages);
            long totalBytes = Interlocked.Read(ref _totalCompressedBodyBytes);

            long deltaMsg = totalMsg - _lastTotalMessages; if (deltaMsg < 0) deltaMsg = 0;
            long deltaBytes = totalBytes - _lastTotalCompressedBytes; if (deltaBytes < 0) deltaBytes = 0;

            double instMsgRate = seconds > 0d ? (double)deltaMsg / seconds : 0d;
            double instBitrate = seconds > 0d ? ((double)deltaBytes * 8d) / seconds : 0d;

            // EWMA smoothing
            double alpha = _ewmaWindowSeconds > 0 ? Math.Min(1d, ((double)_updateIntervalSeconds) / (double)_ewmaWindowSeconds) : 1d;
            double newMsgRate = (1d - alpha) * AtomicReadDouble(ref _avgMessageThroughput) + alpha * instMsgRate;
            double newBitrate = (1d - alpha) * AtomicReadDouble(ref _avgBitrateBitsPerSecond) + alpha * instBitrate;

            AtomicWriteDouble(ref _avgMessageThroughput, newMsgRate);
            AtomicWriteDouble(ref _avgBitrateBitsPerSecond, newBitrate);

            _lastSampleTicks = nowTicks;
            _lastTotalMessages = totalMsg;
            _lastTotalCompressedBytes = totalBytes;
        }

        // --- Helpers ---
        private static long TicksToSeconds(long ticks)
        {
            if (ticks <= 0) return 0;
            return ticks / TimeSpan.TicksPerSecond;
        }

        private static double AtomicReadDouble(ref double location)
        {
            // Non-mutating atomic read using CompareExchange
            return Interlocked.CompareExchange(ref location, 0d, 0d);
        }

        private static void AtomicWriteDouble(ref double location, double value)
        {
            Interlocked.Exchange(ref location, value);
        }

        private static void UpdateMax(ref long target, long candidate)
        {
            long current = Interlocked.Read(ref target);
            while (candidate > current)
            {
                long prev = Interlocked.CompareExchange(ref target, candidate, current);
                if (prev == current) return;
                current = prev;
            }
        }

        private static long InterlockedAdd(ref long location, long value)
        {
            // Portable add for .NET 3.5: CAS loop
            long initialValue, computed;
            do
            {
                initialValue = Interlocked.Read(ref location);
                computed = initialValue + value;
            } while (Interlocked.CompareExchange(ref location, computed, initialValue) != initialValue);
            return computed;
        }
    }
}
#endif
