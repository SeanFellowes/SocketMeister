# SocketMeister

[![NuGet version](https://img.shields.io/nuget/v/SocketMeister.svg)](https://www.nuget.org/packages/SocketMeister)
[![Docs](https://github.com/SeanFellowes/SocketMeister/actions/workflows/docs.yml/badge.svg?branch=main)](https://github.com/SeanFellowes/SocketMeister/actions/workflows/docs.yml)
[![Automated Testing](https://github.com/SeanFellowes/SocketMeister/actions/workflows/docs.yml/badge.svg)](https://github.com/SeanFellowes/SocketMeister/actions/workflows/docs.yml)

**SocketMeister** is a high-performance, fault-tolerant TCP/IP socket library for .NET clients & servers.

## 🚀 Quick start
Check out the getting started guide at https://seanfellowes.github.io/SocketMeister/getting-started.html

## ⭐ Key features
Auto-reconnect & failover across multiple endpoints

Request/response and publish/subscribe messaging patterns

Optional compression to save bandwidth

Multithreaded server with thousands of concurrent clients

## 📖 Learn more
Full docs & tutorials: https://seanfellowes.github.io/SocketMeister/

API reference: https://seanfellowes.github.io/SocketMeister/api/index.html

Samples & advanced guides: https://seanfellowes.github.io/SocketMeister/samples/index.html

Upgrading to v11? See the guide: https://seanfellowes.github.io/SocketMeister/upgrading-to-version11.html


## 📊 Telemetry (new in 11.1.0)
- SocketMeister now exposes lightweight runtime telemetry on both the client and server.
- Access via `SocketClient.Telemetry` and `SocketServer.Telemetry` for a live view, or call `GetSnapshot()` for a point‑in‑time immutable snapshot.
- Metrics include: current/peak connections, process/session uptime, total messages/failures, rolling messages/sec, rolling bitrate (bits/sec), compression ratio and savings, reconnects, and protocol errors.
- Designed for high throughput: atomic counters, periodic aggregation every few seconds (default 5s), zero allocations per message.
- Configuration: enable/disable per instance via `TelemetryEnabled`, adjust cadence via `TelemetryUpdateIntervalSeconds` (1–10s). Telemetry is enabled by default.


Note: If you’re embedding sources directly, grab [SocketMeister.Sources from NuGet](https://www.nuget.org/packages/SocketMeister.Sources/) for easier debugging.

## 🧪 Continuous Integration

- Platform: GitHub Actions on `windows-latest` runners.
- Workflows: `windows-ci.yml` (badge above) runs on every push and pull request.
- Docs: `docs.yml` (badge above) builds DocFX and deploys to GitHub Pages on changes to docs and source.
- Matrix:
  - Integration tests for `net8.0` and `net472`.
  - Compatibility tests driven by a .NET 3.5 console client (orchestrated from `net8.0`).
- How to inspect results:
  - Open the repository’s Actions tab → “Tests (Windows)”.
  - Each job step shows restore, build, and test phases with logs and annotations.
  - Click into a step to see full test output and any warnings.
  - For docs: open Actions → “Build & Deploy Docs”. The “deploy” job publishes to GitHub Pages.

Notes:
- Some scenarios (e.g., handshake timeout) are intentionally thorough and can take ~30–45s each.
- Tests use ephemeral ports and localhost; no external resources are required.
 - Docs site is published at: https://seanfellowes.github.io/SocketMeister/
