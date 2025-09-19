using System;

namespace SocketMeister
{
    /// <summary>
    /// Immutable snapshot of runtime telemetry values captured at a point in time.
    /// </summary>
 #if SMISPUBLIC
        public sealed class SocketTelemetrySnapshot
#else
        internal sealed class SocketTelemetrySnapshot
#endif
    {
        /// <summary>Number of active connections at snapshot time. Client: 0/1. Server: 0..N.</summary>
        public long CurrentConnections { get; private set; }

        /// <summary>Peak observed concurrent connections since process/session start (see uptime semantics).</summary>
        public long MaxConnections { get; private set; }

        /// <summary>Seconds since process-level start (server start; client first-ever successful connection).</summary>
        public long ProcessUptimeSeconds { get; private set; }

        /// <summary>Seconds since current active session began (client reconnects reset this; server restart resets).</summary>
        public long SessionUptimeSeconds { get; private set; }

        /// <summary>Total messages successfully sent or received since session start.</summary>
        public long TotalMessages { get; private set; }

        /// <summary>Total failed send attempts since session start.</summary>
        public long TotalFailures { get; private set; }

        /// <summary>Rolling average messages per second.</summary>
        public double AvgMessageThroughput { get; private set; }

        /// <summary>Rolling average bitrate in bits per second (decimal units when displayed).</summary>
        public double AvgBitrateBitsPerSecond { get; private set; }

        /// <summary>Observed compression ratio computed from total compressed and uncompressed body bytes.</summary>
        public double CompressionRatio { get; private set; }

        /// <summary>Total bytes saved by compression: sum of (uncompressed - compressed) across send/receive.</summary>
        public long CompressionSavingsBytes { get; private set; }

        /// <summary>Total compressed body bytes observed (send + receive) since session start.</summary>
        public long TotalCompressedBodyBytes { get; private set; }

        /// <summary>Total uncompressed body bytes observed (send + receive) since session start.</summary>
        public long TotalUncompressedBodyBytes { get; private set; }

        /// <summary>Number of successful reconnects (client) or accepted handshakes (server) since session start.</summary>
        public long Reconnects { get; private set; }

        /// <summary>Number of protocol/framing errors observed in receive processing since session start.</summary>
        public long ProtocolErrors { get; private set; }

        /// <summary>
        /// Constructs an immutable snapshot with explicit values.
        /// </summary>
        /// <param name="currentConnections">Number of active connections at snapshot time.</param>
        /// <param name="maxConnections">Peak observed concurrent connections since uptime origin.</param>
        /// <param name="processUptimeSeconds">Seconds since process-level start.</param>
        /// <param name="sessionUptimeSeconds">Seconds since session-level start.</param>
        /// <param name="totalMessages">Total messages (sent + received) since session start.</param>
        /// <param name="totalFailures">Total failed send attempts since session start.</param>
        /// <param name="avgMessageThroughput">Rolling average messages per second.</param>
        /// <param name="avgBitrateBitsPerSecond">Rolling average bitrate in bits per second.</param>
        /// <param name="totalCompressedBodyBytes">Total compressed body bytes observed since session start.</param>
        /// <param name="totalUncompressedBodyBytes">Total uncompressed body bytes observed since session start.</param>
        /// <param name="compressionSavingsBytes">Total bytes saved by compression.</param>
        /// <param name="compressionRatio">Observed compression ratio.</param>
        /// <param name="reconnects">Successful reconnects (client) or accepted handshakes (server).</param>
        /// <param name="protocolErrors">Protocol/framing errors observed.</param>
        internal SocketTelemetrySnapshot(
            long currentConnections,
            long maxConnections,
            long processUptimeSeconds,
            long sessionUptimeSeconds,
            long totalMessages,
            long totalFailures,
            double avgMessageThroughput,
            double avgBitrateBitsPerSecond,
            long totalCompressedBodyBytes,
            long totalUncompressedBodyBytes,
            long compressionSavingsBytes,
            double compressionRatio,
            long reconnects,
            long protocolErrors)
        {
            CurrentConnections = currentConnections;
            MaxConnections = maxConnections;
            ProcessUptimeSeconds = processUptimeSeconds;
            SessionUptimeSeconds = sessionUptimeSeconds;
            TotalMessages = totalMessages;
            TotalFailures = totalFailures;
            AvgMessageThroughput = avgMessageThroughput;
            AvgBitrateBitsPerSecond = avgBitrateBitsPerSecond;
            TotalCompressedBodyBytes = totalCompressedBodyBytes;
            TotalUncompressedBodyBytes = totalUncompressedBodyBytes;
            CompressionSavingsBytes = compressionSavingsBytes;
            CompressionRatio = compressionRatio;
            Reconnects = reconnects;
            ProtocolErrors = protocolErrors;
        }

        /// <summary>
        /// Creates an empty snapshot with all values set to zero. Intended for default initialization.
        /// </summary>
        public static SocketTelemetrySnapshot Empty()
        {
            return new SocketTelemetrySnapshot(0, 0, 0, 0, 0, 0, 0d, 0d, 0, 0, 0, 0d, 0, 0);
        }
    }
}

