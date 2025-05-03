Changelog

All notable changes to SocketMeister are documented in this file.The project follows Semantic Versioning and the format is inspired by Keep a Changelog.

[10.2.7] - 2025-05-03

Fixed

.NET 3.5 only, Occasional crash during disconnect.

[10.2.6] - 2025-04-27

Fixed

SocketMeister.Sources folder naming conventions now best practice within Visual Studio.

SocketMeister.Sources now works with both legacy and new .NET

[10.2.5] - 2025-04-26

Fixed

Bug in .NET 3.5 where SocketClient would not reconnect ofter more than one failover 
to another SocketServer.

Changed

Marked obsolete - SocketClient.ExceptionRaised event

Marked obsolete - ExceptionEventArgs, SocketClient.ExceptionRaised event.

Token class now Internal, not Public

[10.2.4] - 2025‑04‑19

Fixed

NuGet Sources package layout/power‑shell target so .cs files appear correctly in SDK‑style projects.

[10.2.3] - 2025‑04‑14

Fixed

Race condition where a very fast client closed the connection immediately after Connect, causing an unexpected disconnect.

[10.2.2] - 2025‑04‑02

Fixed

Access modifiers of logging helpers wrapped in SMISPUBLIC to stop duplicate‑type warnings when the Sources package is embedded in multiple assemblies.

[10.2.0] - 2025‑03‑31

Changed

SocketServer: replaced TraceEventRaised with LogRaised to align with SocketClient (introduced in 10.1.0).

Removed several unused test projects from the repository.

[10.1.0] - 2025‑03‑31

Added

Single‑threaded logging pipeline and LogRaised event on SocketClient.

Changed

Internal refactor to centralise token handling.

[10.0.3] - 2025‑03‑27

Added

Extra log events for TraceEventRaised.

Fixed

Robust socket recreation when ObjectDisposedException occurs.

[10.0.2] - 2025‑03‑27

Fixed

Client could stop attempting to reconnect if the socket was reset before the handshake completed.

[10.0.1] - 2025‑03‑26

Fixed

Minor bugs discovered during rollout of 10.0.0.

[10.0.0] - 2025‑03‑26

Breaking

Major internal rewrite. Message framing, handshake sequence and many private members were overhauled for throughput and resilience.

Older clients (≤4.x) will not connect to a 10.x server.

[4.1.5] - 2025‑03‑11

Fixed

Client now waits until server receive buffer is allocated before reporting Connected.

[4.1.2] - 2024‑12‑30

Fixed

Null payloads are now accepted by SocketClient.

Corrected message‑timeout handling on both client and server.

[4.1.1] - 2024‑12‑28

Added

Response logging in server test harness.

[4.1.0] - 2024‑12‑26

Added

Target‑framework monikers updated to include .NET 9.

[4.0.6] - 2023‑12‑31

Removed

Legacy FxCop analyser; added TFM support up to .NET 8.

[4.0.1] - 2021‑03‑26

Fixed / Changed

First maintenance release on v4 line – general cleanup and compiler‑warning passes.

[4.0.0] - 2021‑03‑15

Breaking

First big API tidy‑up since inception: renamed many types/members for clarity, dropped obsolete properties/events, unified exception model.

[2.2.1] - 2021‑02‑24

Fixed

.NET Core 3.1 assemblies could not see SocketServer/SocketClient due to incorrect visibility.

[2.2.0] - 2021‑02‑23

Fixed

Large‑payload transfer timeout; improved reconnection latency when server restarts.

[2.1.2] - 2021‑02‑17

Fixed

Infinite‑loop edge case during disconnect.

[2.0.7] - 2021‑01‑20

Added / Changed

Mini test server/client utilities; incremental server robustness tweaks.

[2.0.6] - 2021‑01‑15

Fixed

Thread‑pool exhaustion under very high load by introducing dedicated worker threads.

[2.0.5] - 2020‑10‑07

Fixed

Handshake reliability and compiler warnings for Silverlight builds.

Earlier 1.x releases (2015‑2020)

The 1.x line predates public NuGet distribution and consisted mainly of exploratory internal releases. They are no longer supported.

Generated automatically from commit history & NuGet metadata on 2025‑04‑21.