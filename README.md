# SocketMeister

[![NuGet version](https://img.shields.io/nuget/v/SocketMeister.svg)](https://www.nuget.org/packages/SocketMeister)

**SocketMeister** is a high-performance, fault-tolerant TCP/IP socket library for .NET clients & servers.

## 🚀 Quick start

```powershell
Install-Package SocketMeister

using SocketMeister;

var server = new SocketServer("0.0.0.0", 5000);
server.MessageReceived += (_, e) => Console.WriteLine($"Got: {e.Message}");
server.Start();


⭐ Key features
Auto-reconnect & failover across multiple endpoints

Request/response and publish/subscribe messaging patterns

Optional compression to save bandwidth

Multithreaded server with thousands of concurrent clients

📖 Learn more
Full docs & tutorials: seanfellowes.github.io/SocketMeister/

API reference: https://seanfellowes.github.io/SocketMeister/api/index.html

Samples & advanced guides: see the docs/ folder on GitHub

Note: If you’re embedding sources directly, grab SocketMeister.Sources from NuGet for easier debugging.