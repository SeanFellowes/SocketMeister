using System;
using System.Diagnostics;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// Lightweight, thread-safe runtime telemetry view for SocketMeister. Provides a live read-only view and
    /// the ability to produce immutable snapshots for consistent multi-field reads.
    /// </summary>
#if SMISPUBLIC
    public sealed class SocketTelemetry : IDisposable
#else
    internal sealed class SocketTelemetry : IDisposable
#endif
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

        /// <summary>
        /// Initializes a new telemetry instance with default update interval and EWMA window.
        /// Starts a low-frequency, non-reentrant timer to periodically sample counters and update rolling averages.
        /// </summary>
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

        /// <summary>Total compressed body bytes observed (send + receive) since session start.</summary>
        public long TotalCompressedBodyBytes { get { return Interlocked.Read(ref _totalCompressedBodyBytes); } }

        /// <summary>Total uncompressed body bytes observed (send + receive) since session start.</summary>
        public long TotalUncompressedBodyBytes { get { return Interlocked.Read(ref _totalUncompressedBodyBytes); } }

        /// <summary>Total bytes saved by compression: sum of (uncompressed - compressed) across send/receive.</summary>
        public long CompressionSavingsBytes { get { return Interlocked.Read(ref _compressionSavingsBytes); } }

        /// <summary>Number of successful reconnects (client) or accepted handshakes (server) since session start.</summary>
        public long Reconnects { get { return Interlocked.Read(ref _reconnects); } }

        /// <summary>Number of protocol/framing errors observed in receive processing since session start.</summary>
        public long ProtocolErrors { get { return Interlocked.Read(ref _protocolErrors); } }

        /// <summary>
        /// Enables or disables telemetry collection for this instance at runtime. Default: true.
        /// Disabling stops the background timer and makes updates no-ops.
        /// </summary>
        /// <param name="enabled">True to enable background sampling; false to stop and dispose the timer.</param>
        public void SetEnabled(bool enabled)
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

        /// <summary>Sets the telemetry aggregation update interval in seconds.</summary>
        /// <param name="seconds">Desired interval in seconds. Clamped to the range [1, 10].</param>
        public void SetUpdateIntervalSeconds(int seconds)
        {
            if (seconds < 1) seconds = 1;
            if (seconds > 10) seconds = 10;
            _updateIntervalSeconds = seconds;
            var t = _timer;
            if (t != null)
            {
                try { t.Change(seconds * 1000, seconds * 1000); } catch { }
            }
        }

        /// <summary>Sets the process-level uptime origin to now (UTC).</summary>
        public void MarkProcessStartNow() => Interlocked.Exchange(ref _processStartUtcTicks, DateTime.UtcNow.Ticks);

        /// <summary>Sets the session-level uptime origin to now (UTC).</summary>
        public void MarkSessionStartNow() => Interlocked.Exchange(ref _sessionStartUtcTicks, DateTime.UtcNow.Ticks);

        /// <summary>
        /// Increments the number of current connections by one and updates the observed maximum if applicable.
        /// </summary>
        public void IncrementCurrentConnections()
        {
            long newVal = InterlockedAdd(ref _currentConnections, 1);
            UpdateMax(ref _maxConnections, newVal);
        }

        /// <summary>Decrements the number of current connections by one.</summary>
        public void DecrementCurrentConnections()
        {
            InterlockedAdd(ref _currentConnections, -1);
        }

        /// <summary>
        /// Records a successful send operation.
        /// </summary>
        /// <param name="compressedBytes">Size of the compressed payload in bytes.</param>
        /// <param name="uncompressedBytes">Original uncompressed payload size in bytes.</param>
        public void AddSendSuccess(int compressedBytes, int uncompressedBytes)
        {
            Interlocked.Increment(ref _totalMessages);
            InterlockedAdd(ref _totalCompressedBodyBytes, compressedBytes);
            InterlockedAdd(ref _totalUncompressedBodyBytes, uncompressedBytes);
            long saved = (long)uncompressedBytes - (long)compressedBytes; if (saved < 0) saved = 0;
            InterlockedAdd(ref _compressionSavingsBytes, saved);
        }

        /// <summary>Records a failed send attempt.</summary>
        public void AddSendFailure()
        {
            Interlocked.Increment(ref _totalFailures);
        }

        /// <summary>
        /// Records a successful receive operation.
        /// </summary>
        /// <param name="compressedBytes">Size of the compressed payload in bytes.</param>
        /// <param name="uncompressedBytes">Original uncompressed payload size in bytes.</param>
        public void AddReceiveSuccess(int compressedBytes, int uncompressedBytes)
        {
            Interlocked.Increment(ref _totalMessages);
            InterlockedAdd(ref _totalCompressedBodyBytes, compressedBytes);
            InterlockedAdd(ref _totalUncompressedBodyBytes, uncompressedBytes);
            long saved = (long)uncompressedBytes - (long)compressedBytes; if (saved < 0) saved = 0;
            InterlockedAdd(ref _compressionSavingsBytes, saved);
        }

        /// <summary>Increments the reconnect counter.</summary>
        public void AddReconnect()
        {
            Interlocked.Increment(ref _reconnects);
        }

        /// <summary>Increments the protocol error counter.</summary>
        public void AddProtocolError()
        {
            Interlocked.Increment(ref _protocolErrors);
        }

        /// <summary>
        /// Creates an immutable snapshot of current telemetry values for this instance.
        /// Prefer live reads for low-overhead sampling; use a snapshot for multi-field consistency.
        /// </summary>
        /// <returns>A snapshot of counters, aggregations, and uptimes at the moment of invocation.</returns>
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

        /// <summary>
        /// Disposes the telemetry instance, stopping and releasing the background timer.
        /// Safe to call multiple times.
        /// </summary>
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

        /// <summary>
        /// Sets the process-level uptime origin to now (UTC).
        /// </summary>
        public void MarkProcessStart()
        {
            Interlocked.Exchange(ref _processStartUtcTicks, DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Sets the session-level uptime origin to now (UTC).
        /// </summary>
        public void MarkSessionStart()
        {
            Interlocked.Exchange(ref _sessionStartUtcTicks, DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Timer callback that samples counters and updates EWMA-based rolling averages.
        /// Non-reentrant via an interlocked gate.
        /// </summary>
        /// <param name="state">Unused.</param>
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

        /// <summary>
        /// Samples totals using a monotonic clock to compute instantaneous rates and updates the EWMA.
        /// </summary>
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

        /// <summary>
        /// Converts .NET ticks to seconds, clamping non-positive values to zero.
        /// </summary>
        private static long TicksToSeconds(long ticks)
        {
            if (ticks <= 0) return 0;
            return ticks / TimeSpan.TicksPerSecond;
        }

        /// <summary>
        /// Atomically reads a double value using CompareExchange.
        /// </summary>
        private static double AtomicReadDouble(ref double location)
        {
            // Non-mutating atomic read using CompareExchange
            return Interlocked.CompareExchange(ref location, 0d, 0d);
        }

        /// <summary>
        /// Atomically writes a double value using Exchange.
        /// </summary>
        private static void AtomicWriteDouble(ref double location, double value)
        {
            Interlocked.Exchange(ref location, value);
        }

        /// <summary>
        /// Updates a maximum value with an interlocked compare/exchange loop.
        /// </summary>
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

        /// <summary>
        /// Adds a value to a 64-bit integer using a portable CAS loop (for .NET 3.5 compatibility).
        /// </summary>
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

