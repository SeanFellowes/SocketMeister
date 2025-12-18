# Changelog

All notable changes to this project will be documented in this file.
The format is based on Keep a Changelog and this project adheres to Semantic Versioning.

## 11.2.5 - 2025-12-10

### Added
- Target framework monikers updated to include .NET 10.

## 11.2.4 - 2025-12-18

### Fixed
- SocketServer: Fixed a race condition when multiple threads in the host application concurrently broadcast messages. Implemented thread synchronization to ensure thread-safe broadcast operations.

## 11.2.3 - 2025-11-19

### Fixed
- SocketMeister.Sources nuget package: Mark as a development dependency to avoid being included as a runtime dependency.

## 11.2.2 - 2025-11-01

### Fixed
- SocketClient: Fixed message resend after reconnect (If message has not timed out).

## 11.2.1 - 2025-10-30

### Fixed
- SocketClient: Fixed a race condition causing message failure under heavy load.

## 11.2.0

Added
- SocketClient dynamic endpoint management:
  - New `SocketClient.EndPoints` property returns a snapshot array of configured endpoints.
  - New methods: `SetEndPoints(IEnumerable<SocketEndPoint>)`, `AddEndPoint(SocketEndPoint)`, `AddEndPoint(string,int)`, and `RemoveEndPoint(string,int)`.
  - Behavior: newly supplied endpoints are made immediately eligible (backoff cleared). If the current endpoint is removed, the client disconnects and reconnects to an eligible endpoint.

Changed
- Connection robustness on slow hosts:
  - Client handshake now uses independent windows for Handshake1 and Handshake2Ack (each ~30s) to avoid near‑deadline timeouts.
  - Server sends Handshake1 for a short window (~5s) with small intervals to reduce first‑message loss.
- Server endpoint reporting:
  - `SocketServer.EndPoint` now reflects the actual bound port after `Bind()` (including when starting with port 0).
- XML docs improvements:
  - Clarified `SocketEndPoint` summary and properties (Description, IPAddress, IPEndPoint, Port, Socket).
  - Documented `SocketClient.EndPoints` semantics (snapshot/copy; use Set/Add/Remove to change).

Test Harness (internal)
- Refactored GitHub CI tests for reliability and speed:
  - Bind‑only port reservation to prevent accidental early client connects.
  - Event‑driven reconnect waiters and small post‑start settles across server‑starting tests.
  - Additional connection attempt diagnostics printed during CI.
  These changes do not affect public APIs; they improve CI stability notably on Windows Server 2022 runners.


## 11.1.0
Added
- Runtime telemetry for client and server: lock-free counters + periodic aggregation.
- Metrics: CurrentConnections, MaxConnections, ProcessUptimeSeconds, SessionUptimeSeconds, TotalMessages, TotalFailures, AvgMessageThroughput (msg/s), AvgBitrate (bit/s), CompressionRatio, CompressionSavingsBytes, Reconnects, ProtocolErrors.
- Configuration: `TelemetryEnabled` and `TelemetryUpdateIntervalSeconds` (default 5s, range 1–10s).

Notes
- Non-breaking, no wire/protocol changes, no new dependencies.
- Minimal overhead (<1% CPU target), zero allocations per message.
- Compatible with .NET 3.5 and .NET 4.7.2.

## [11.0.1] - 2025-09-20

### Deprecated
- Long polling is deprecated. The `IsLongPolling` parameter had no internal effect and existing constructors that include it are now obsolete. Use the new constructors without `IsLongPolling`.

## [11.0.0] - 2025-09-19

### Breaking
- SocketClient no longer auto-starts on construction. After creating an instance and attaching event handlers, call `Start()` to begin connecting.
- `ConnectionStatusChanged` and `CurrentEndPointChanged` now use typed event args: `ConnectionStatusChangedEventArgs` and `CurrentEndPointChangedEventArgs`.
- `ClientDisconnectReason` is now a public enum (was internal) when `SMISPUBLIC` is defined.
- SocketServer constructor no longer binds/listens; call `Start()` explicitly. `StatusChanged` now uses typed `ServerStatusChangedEventArgs`.
- SocketServer raises `ClientConnected` only after handshake completes (post-Handshake2/Handshake2Ack), ensuring accurate connection semantics.

### Added
- `SocketClient.Start()` to explicitly start the background worker and connection logic (idempotent; throws if the client has been stopped).
- `SocketClient.IsRunning` to indicate if the background worker is running.
- `SocketClient.ServerVersion` to expose the server’s SocketMeister version once known.
- `SocketServerOptions` with `BindAddress`, `Port`, `Backlog`, and `CompressSentData`.
- `SocketServer.IsRunning` convenience property.

### Changed
- `SocketClient.ExceptionRaised` is no longer marked obsolete. Use it for an error-only channel; use `LogRaised` for full telemetry.
- XML documentation on all `SocketClient` constructors now notes the requirement to call `Start()` in v11.
- SocketServer supports restart (call `Stop()` then `Start()` on the same instance). Listener socket and thread are created in `Start()` and disposed in `Stop()`.
- Handshake performance: removed fixed delay before sending Handshake1; the server now retries Handshake1 a few times until Handshake2 is received.

### Migration
1. Replace auto-start assumptions with an explicit `client.Start()` after subscribing to events.
2. Update handlers for:
   - `ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)`
   - `CurrentEndPointChanged(object sender, CurrentEndPointChangedEventArgs e)`
3. If you previously filtered `LogRaised` for only errors, consider subscribing to `ExceptionRaised` instead.
4. For SocketServer:
   - Call `server.Start()` explicitly after constructing and subscribing to events.
   - Update `StatusChanged` handlers to `(object sender, ServerStatusChangedEventArgs e)` and use `e.OldStatus`, `e.NewStatus`, `e.EndPoint`.
   - If you relied on `ClientConnected` during accept, note it is now raised only after the handshake completes.

Notes:
- .NET 3.5 remains supported. `Start()` uses the existing background thread model on .NET 3.5.
- Event ordering remains as before: the final Connected status is raised after the handshake completes.

## [10.3.1] - 2025-09-10

### Fixed
- Logger could crash due to null log entry being added. Null log entries are now ignored.

## [10.3.0] - 2025-08-22

### Added
- Optional friendly message names in SocketClient logging when sending messages.

## [10.2.8] - 2025-08-21

### Fixed
- SocketClient now raises the Connected status after the handshake completes, rather than before.

## [10.2.7] - 2025-05-03

### Fixed
- (.NET 3.5 only) Occasional crash during disconnect.

## [10.2.6] - 2025-04-27

### Fixed
- SocketMeister.Sources folder layout and naming for Visual Studio.
- SocketMeister.Sources now works with both legacy and modern .NET projects.

## [10.2.5] - 2025-04-26

### Fixed
- (.NET 3.5) Reconnect after multiple failovers to another SocketServer.

### Changed
- Marked obsolete (later reversed in 11.0.0): `SocketClient.ExceptionRaised` and `ExceptionEventArgs`.
- `Token` class visibility changed to internal.

## [10.2.4] - 2025-04-19

### Fixed
- NuGet Sources package layout so .cs files appear correctly in SDK-style projects.

## [10.2.3] - 2025-04-14

### Fixed
- Race where a very fast client closed immediately after connect, causing an unexpected disconnect.

## [10.2.2] - 2025-04-02

### Fixed
- Access modifiers of logging helpers wrapped with SMISPUBLIC to avoid duplicate types when embedding Sources in multiple assemblies.

## [10.2.0] - 2025-03-31

### Changed
- SocketServer: replaced `TraceEventRaised` with `LogRaised` to align with SocketClient (introduced in 10.1.0).
- Removed several unused test projects.

## [10.1.0] - 2025-03-31

### Added
- Single-threaded logging pipeline and `LogRaised` event on SocketClient.

### Changed
- Internal refactor to centralize token handling.

## [10.0.3] - 2025-03-27

### Added
- Extra log events for `TraceEventRaised`.

### Fixed
- Robust socket recreation when `ObjectDisposedException` occurs.

## [10.0.2] - 2025-03-27

### Fixed
- Client could stop reconnecting if the socket was reset before the handshake completed.

## [10.0.1] - 2025-03-26

### Fixed
- Minor bugs discovered during rollout of 10.0.0.

## [10.0.0] - 2025-03-26

### Breaking
- Major internal rewrite. Message framing, handshake sequence and many private members overhauled for throughput and resilience.
- Older clients (≤ 4.x) will not connect to a 10.x server.

## [4.1.5] - 2025-03-11

### Fixed
- Client now waits until server receive buffer is allocated before reporting Connected.

## [4.1.2] - 2024-12-30

### Fixed
- Null payloads are now accepted by SocketClient.
- Corrected message-timeout handling on both client and server.

## [4.1.1] - 2024-12-28

### Added
- Response logging in server test harness.

## [4.1.0] - 2024-12-26

### Added
- Target framework monikers updated to include .NET 9.

## [4.0.6] - 2023-12-31

### Removed
- Legacy FxCop analyzer; added TFM support up to .NET 8.

## [4.0.1] - 2021-03-26

### Fixed / Changed
- First maintenance release on v4 line – general cleanup and compiler-warning passes.

## [4.0.0] - 2021-03-15

### Breaking
- First major API tidying since inception: renamed many types/members for clarity, dropped obsolete properties/events, unified exception model.

## [2.2.1] - 2021-02-24

### Fixed
- .NET Core 3.1 assemblies could not see SocketServer/SocketClient due to incorrect visibility.

## [2.2.0] - 2021-02-23

### Fixed
- Large-payload transfer timeout; improved reconnection latency when server restarts.

## [2.1.2] - 2021-02-17

### Fixed
- Infinite-loop edge case during disconnect.

## [2.0.7] - 2021-01-20

### Added / Changed
- Mini test server/client utilities; incremental server robustness tweaks.

## [2.0.6] - 2021-01-15

### Fixed
- Thread-pool exhaustion under very high load by introducing dedicated worker threads.

## [2.0.5] - 2020-10-07

### Fixed
- Handshake reliability and compiler warnings for Silverlight builds.

## Earlier 1.x releases (2015–2020)

The 1.x line predates public NuGet distribution and consisted mainly of exploratory internal releases. They are no longer supported.

