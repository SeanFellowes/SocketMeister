# SocketMeister

[![NuGet version](https://img.shields.io/nuget/v/SocketMeister.svg)](https://www.nuget.org/packages/SocketMeister)
[![Docs](https://github.com/SeanFellowes/SocketMeister/actions/workflows/docs.yml/badge.svg?branch=main)](https://github.com/SeanFellowes/SocketMeister/actions/workflows/docs.yml)
[![Tests (Windows)](https://github.com/SeanFellowes/SocketMeister/actions/workflows/windows-ci.yml/badge.svg)](https://github.com/SeanFellowes/SocketMeister/actions/workflows/windows-ci.yml)

**SocketMeister** is a high-performance, fault-tolerant TCP/IP socket library for .NET clients & servers.

## 🚀 Quick start
Get started in minutes:  
👉 https://seanfellowes.github.io/SocketMeister/getting-started.html

## ⭐ Key features
- Auto-reconnect & failover across multiple endpoints  
- Request/response and publish/subscribe messaging  
- Optional compression to save bandwidth  
- Multithreaded server with thousands of concurrent clients  

## 📖 Learn more
- Full docs & tutorials: https://seanfellowes.github.io/SocketMeister/  
- API reference: https://seanfellowes.github.io/SocketMeister/api/index.html  
- Samples & guides: https://seanfellowes.github.io/SocketMeister/samples/index.html  
- Upgrading to v11? https://seanfellowes.github.io/SocketMeister/upgrading-to-version11.html  

## 📊 Telemetry (new in 11.1.0)
- Lightweight runtime telemetry on client and server.  
- Access via `SocketClient.Telemetry` / `SocketServer.Telemetry` or `GetSnapshot()`.  
- Metrics: current/peak connections, uptime, total messages/failures, throughput, bitrate, compression ratio, reconnects, protocol errors.  
- Designed for high throughput: atomic counters, periodic aggregation (default 5s), zero allocations per message.  
- Configurable via `TelemetryEnabled` and `TelemetryUpdateIntervalSeconds` (1–10s). Enabled by default.  

👉 Detailed telemetry guide: https://seanfellowes.github.io/SocketMeister/guides/telemetry.html  

---

If embedding sources: use [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) for easier debugging.