# Getting Started with SocketMeister

This guide shows how to install the package and build a minimal server and client using the v11 API.

## Prerequisites

- .NET Framework 3.5+ or modern .NET (Core/5+/6+/7+/8+)
- Visual Studio, VS Code, Rider, or equivalent
- NuGet (via IDE or `dotnet` CLI)

## 1) Install the package

PowerShell:

```powershell
Install-Package SocketMeister -Version 11.0.0
```

Or using the .NET CLI:

```bash
dotnet add package SocketMeister --version 11.0.0
```

## 2) Minimal server

```csharp
using System;
using System.Text;
using SocketMeister;

class Program
{
    static void Main()
    {
        // Listen on port 5000, compression disabled
        var server = new SocketServer(port: 5000, CompressSentData: false);

        server.MessageReceived += (s, e) =>
        {
            // Parameters are strongly-typed objects (int, string, byte[], etc.)
            Console.WriteLine($"Server received {e.Parameters.Length} parameter(s).");

            // Optional response: send bytes back to the client
            var payload = Encoding.UTF8.GetBytes("Hello from server!");
            e.Response = payload;
        };

        server.Start();
        Console.WriteLine("Server running on port 5000. Press Enter to exit.");
        Console.ReadLine();
        server.Stop();
    }
}
```

## 3) Minimal client

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using SocketMeister;

class Program
{
    static void Main()
    {
        // Prepare endpoints (supports automatic failover)
        var endpoints = new List<SocketEndPoint>
        {
            new SocketEndPoint("127.0.0.1", 5000)
        };

        var client = new SocketClient(endpoints, EnableCompression: false, friendlyName: "SampleClient");

        // Subscribe before starting to avoid missing early events
        client.ConnectionStatusChanged += (s, e) =>
        {
            Console.WriteLine($"Status: {e.OldStatus} -> {e.NewStatus} (Reason: {e.Reason})");
        };

        client.CurrentEndPointChanged += (s, e) =>
        {
            Console.WriteLine($"Endpoint changed: {e.OldEndPoint?.Description} -> {e.NewEndPoint.Description}");
        };

        client.Start();

        // Send a request and wait for a response (bytes)
        byte[] response = client.SendMessage(new object[] { "Hello from client" }, TimeoutMilliseconds: 5000);
        Console.WriteLine("Client received: " + Encoding.UTF8.GetString(response));

        client.Stop();
    }
}
```

Notes:
- v11 requires calling `Start()` explicitly after constructing and subscribing to events.
- `ExceptionRaised` provides an error-only channel. For full telemetry, subscribe to `LogRaised`.
- `client.ServerVersion` is populated after handshake completes.

## 4) Explore further

- Multiple endpoints and failover: see the sample in `docs/samples/multi-endpoint.md`.
- Compression: enable on both sides for large payloads; see `docs/samples/compression.md`.
- Request/response: see `docs/samples/request-response.md`.

For design details, review the [architecture overview](architecture.md).