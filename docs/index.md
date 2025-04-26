# SocketMeister

SocketMeister is a high-performance, fault-tolerant, multithreaded TCP/IP socket library for .NET. It provides both client and server implementations with built-in features for automatic reconnection, round‑robin endpoint load balancing, seamless compression, and flexible messaging patterns.

## Key Features

- **Automatic reconnection & failover**: `SocketClient` transparently reconnects on network errors and cycles through multiple endpoints to find a live server.
- **Event‑driven messaging**: Support for one-way messages and request/response patterns via `MessageReceived` and `RequestReceived` events.
- **Broadcast & Publish/Subscribe**: `SocketServer` can broadcast to all clients or to named subscriber groups; clients manage subscriptions by name.
- **High‑performance compression**: Optional payload compression/decompression for reduced bandwidth usage.
- **Scalable, multithreaded server**: `SocketServer` handles thousands of concurrent connections with thread‑safe event dispatch.
- **Flexible data types**: Exchange strings, byte arrays, numeric primitives, and custom serializable objects out of the box.

## Requirements

- **.NET support**: .NET Framework 3.5 through .NET 9 (incl. .NET Core 3.1, .NET 5/6/7/8).
- **Server compatibility**: `SocketServer` requires .NET 4.5 or later.

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
using SocketMeister;

var server = new SocketServer(port: 5000, enableCompression: true);

// Handle incoming one-way messages
event EventHandler<SocketServer.MessageReceivedEventArgs> msgHandler = (s, e) =>
{
    Console.WriteLine($"Received from {e.ClientId}: {e.Parameters[0]}");
};
server.MessageReceived += msgHandler;

// Handle request/response
event EventHandler<SocketServer.RequestReceivedEventArgs> reqHandler = (s, e) =>
{
    var cmd = e.Parameters[0] as string;
    e.Response = System.Text.Encoding.UTF8.GetBytes($"ACK: {cmd}");
};
server.RequestReceived += reqHandler;

server.Start();
Console.WriteLine("Server listening on port 5000.");
```

### Client Example

```csharp
using SocketMeister;

// Connect to multiple endpoints with compression enabled
var client = new SocketClient(
    endpoints: new[] { ("127.0.0.1", 5000), ("server2", 5000) },
    enableCompression: true
);

client.ConnectionStatusChanged += (s, a) =>
    Console.WriteLine($"Client status: {client.ConnectionStatus}");

client.Connect();

// Send a one-way message
client.Send(new object[] { "Hello, Server!" });

// Send a request and wait for reply
var reply = client.SendRequest(new object[] { "Ping" });
Console.WriteLine($"Server replied: {System.Text.Encoding.UTF8.GetString(reply)}");
```

## Next Steps

- Browse the [Full API Reference](/api/index.html) for all classes, methods, and events.
- Follow the [Getting Started Guide](getting-started.md) for tutorials and deeper examples.
- Explore advanced scenarios: [Pub/Sub Messaging](tutorials/pubsub.md), [Custom Serializers](tutorials/custom-serialization.md).

Happy coding! Feel free to open issues or drop us a ⭐ on GitHub if SocketMeister helps power your project.

