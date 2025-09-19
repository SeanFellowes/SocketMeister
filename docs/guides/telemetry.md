# Telemetry

SocketMeister 11.1.0 adds lightweight, thread-safe runtime telemetry for both client and server instances.

Goals:
- Diagnose bottlenecks and support capacity planning.
- Keep overhead low: atomic counters, no hot-path allocations, periodic aggregation.

How it works:
- Atomic counters on send/receive and connection events.
- A background sampler (default 5s) computes rolling averages using an EWMA (~15s window).
- Compression efficiency is observed by comparing compressed and uncompressed body lengths where those lengths already exist in the pipeline.

Metrics (selection):
- CurrentConnections, MaxConnections
- ProcessUptimeSeconds, SessionUptimeSeconds
- TotalMessages (sent + received), TotalFailures (send failures)
- AvgMessageThroughput (messages/sec)
- AvgBitrate (bits/sec; decimal units)
- CompressionRatio and CompressionSavingsBytes
- Reconnects, ProtocolErrors

Defaults & configuration:
- Enabled by default; disable per instance with `TelemetryEnabled`.
- Aggregation interval: `TelemetryUpdateIntervalSeconds` (1–10s, default 5s).
- Telemetry does not change the wire protocol and adds no dependencies.

Access patterns:
- For a live view, read `SocketClient.Telemetry` or `SocketServer.Telemetry`.
- For a consistent, point-in-time view, call `GetSnapshot()` on the client/server.

FAQ:
- Does telemetry change the wire protocol? No.
- What’s the overhead? Typically well under 1% CPU with zero per-message allocations.
- Can I disable telemetry? Yes, per instance via `TelemetryEnabled`.
- Are latency percentiles available? Not in 11.1.0; intentionally omitted to keep overhead minimal.

