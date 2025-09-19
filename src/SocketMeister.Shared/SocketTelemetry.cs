#if SOCKETMEISTER_TELEMETRY
using System;

namespace SocketMeister
{
    /// <summary>
    /// Lightweight, thread-safe runtime telemetry view for SocketMeister. Provides a live read-only view and
    /// the ability to produce immutable snapshots for consistent multi-field reads.
    /// </summary>
    public sealed class SocketTelemetry
    {
        // Commit 1 (Scaffolding): public read-only surface only. No integration or timers.
        // Aggregation and atomic counters will be added in subsequent commits.

        /// <summary>Number of active connections. Client: 0/1. Server: 0..N.</summary>
        public long CurrentConnections { get { return 0L; } }

        /// <summary>Peak observed concurrent connections since uptime origin.</summary>
        public long MaxConnections { get { return 0L; } }

        /// <summary>Seconds since process-level start (server start; client first-ever successful connection).</summary>
        public long ProcessUptimeSeconds { get { return 0L; } }

        /// <summary>Seconds since current active session began (client reconnect resets; server restart resets).</summary>
        public long SessionUptimeSeconds { get { return 0L; } }

        /// <summary>Total successful messages (send + receive) since session start.</summary>
        public long TotalMessages { get { return 0L; } }

        /// <summary>Total failed send attempts since session start.</summary>
        public long TotalFailures { get { return 0L; } }

        /// <summary>Rolling average messages per second.</summary>
        public double AvgMessageThroughput { get { return 0d; } }

        /// <summary>Rolling average bitrate in bits per second (decimal units when displayed).</summary>
        public double AvgBitrateBitsPerSecond { get { return 0d; } }

        /// <summary>Observed compression ratio derived from total compressed/uncompressed body bytes.</summary>
        public double CompressionRatio { get { return 0d; } }

        /// <summary>Total bytes saved by compression since session start.</summary>
        public long CompressionSavingsBytes { get { return 0L; } }

        /// <summary>Total compressed body bytes observed (send + receive) since session start.</summary>
        public long TotalCompressedBodyBytes { get { return 0L; } }

        /// <summary>Total uncompressed body bytes observed (send + receive) since session start.</summary>
        public long TotalUncompressedBodyBytes { get { return 0L; } }

        /// <summary>Number of successful reconnects (client) or accepted handshakes (server) since session start.</summary>
        public long Reconnects { get { return 0L; } }

        /// <summary>Number of protocol/framing errors observed since session start.</summary>
        public long ProtocolErrors { get { return 0L; } }

        /// <summary>
        /// Produces an immutable snapshot representing a consistent point-in-time view of telemetry values.
        /// </summary>
        public SocketTelemetrySnapshot GetSnapshot()
        {
            // In Commit 2, this will compose values atomically. For now, return an empty snapshot.
            return SocketTelemetrySnapshot.Empty();
        }

        /// <summary>
        /// Resets all counters and uptime origins. Intended for internal/testing scenarios; discouraged for production consumers.
        /// </summary>
        internal void Reset()
        {
            // Implemented in Commit 2 as part of the core aggregator.
        }
    }
}
#endif

