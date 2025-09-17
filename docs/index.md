# SocketMeister

SocketMeister is a high-performance, fault-tolerant TCP/IP socket library for .NET with a client and server implementation. It provides automatic reconnection and failover, optional compression, and a simple request/response model.

## Key Features

- Automatic reconnection and failover: `SocketClient` maintains a single active connection and automatically fails over across multiple endpoints.
- Event-driven messaging: Server raises `MessageReceived`, and the handler can set `e.Response` (byte[]) for request/response semantics.
- Built-in compression: Optional payload compression/decompression for reduced bandwidth usage.
- Scalable server: `SocketServer` handles many concurrent clients with thread-safe event dispatch.
- Flexible data types: Exchange strings, byte arrays, and numeric primitives. For complex objects, serialize to string or byte[] (e.g., JSON).

## Requirements

- .NET Framework 3.5+ and modern .NET (Core/5+/6+/7+/8+).

## Installation

```powershell
Install-Package SocketMeister -Version <latest>
```

Or embed sources directly:

```powershell
Install-Package SocketMeister.Sources -Version <latest>
```

## Quick Start

### Server Example

```csharp
using System;
using System.Text;
using SocketMeister;

var server = new SocketServer(port: 5000, CompressSentData: true);

server.MessageReceived += (s, e) =>
{
    Console.WriteLine($"Received {e.Parameters.Length} parameter(s)");
    var payload = Encoding.UTF8.GetBytes("ACK");
    e.Response = payload; // optional
};

server.Start();
Console.WriteLine("Server listening on port 5000.");
```

### Client Example

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using SocketMeister;

// Configure endpoints; client uses the best available endpoint and fails over automatically
var endpoints = new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", 5000) };
var client = new SocketClient(endpoints, EnableCompression: true, friendlyName: "QuickStartClient");

client.ConnectionStatusChanged += (s, e) =>
    Console.WriteLine($"Status: {e.OldStatus} -> {e.NewStatus} (Reason: {e.Reason})");

client.Start();

// Send a request and wait for reply
byte[] reply = client.SendMessage(new object[] { "Ping" }, TimeoutMilliseconds: 5000);
Console.WriteLine($"Server replied: {Encoding.UTF8.GetString(reply)}");

client.Stop();
```

## Next Steps

- Browse the [API reference](/api/index.html) for classes, methods, and events.
- Follow the [Getting Started guide](getting-started.md) for a step-by-step introduction.
- Explore the [samples](samples/index.md) for multi-endpoint, compression, and request/response patterns.

Happy coding!