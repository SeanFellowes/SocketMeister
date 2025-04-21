# SocketMeister

SocketMeister is a socket library for solutions requiring persistent, fault‑tolerant, multiple‑endpoint TCP/IP connectivity. It is easy to use, resilient, high throughput and multithreaded. citeturn0search1turn0search6

## Key Features

- **Automatic reconnection & fault tolerance** (SocketClient)
- **Round‑robin load balancing** across multiple servers
- **Event‑driven server** (SocketServer) with thread‑safe message and request handling
- **Optional high‑performance compression**/decompression
- **Scalable architecture**: single server to a farm of instances
- **Supports multiple data types**: binary arrays, strings, primitives

## Requirements

- **.NET Framework 3.5 to .NET 9** (including .NET Core, .NET 5/6/7)
- **SocketMeister.Silverlight** for Silverlight 5 (client only, no server support) ([github.com](https://github.com/SeanFellowes/SocketMeister/blob/master/NuGetDocumentation.md))

## Installation

Install the main SocketMeister package via NuGet: citeturn0search0

```powershell
Install-Package SocketMeister -Version 10.2.4
```

Or include the C# sources directly using:

```powershell
Install-Package SocketMeister.Sources -Version 10.2.4
```

## Quick Start

### Basic Server

```csharp
using SocketMeister;

var server = new SocketServer("0.0.0.0", 5000);
server.MessageReceived += (sender, args) =>
{
    Console.WriteLine($"Received: {args.Message}");
    if (args.IsRequest)
        args.Response = $"Echo: {args.Message}";
};
server.Start();
Console.WriteLine("Server listening on port 5000.");
Console.ReadLine();
```

### Basic Client

```csharp
using SocketMeister;

var client = new SocketClient("127.0.0.1", 5000);
client.Connect();
client.Send("Hello, Server!");
var response = client.Receive();
Console.WriteLine($"Server replied: {response}");
```

For more examples and the full API reference, see the [API documentation](api/index.html) and the [Getting Started guide](getting-started.md).

