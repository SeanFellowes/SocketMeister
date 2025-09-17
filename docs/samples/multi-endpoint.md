# Multi-Endpoint Failover

Configure a client with multiple server endpoints for automatic failover (and quick switch-over) if one goes down.

## Scenario
You have two server instances; the client maintains a single active connection and will fail over when needed.

## Code Example
```csharp
using System;
using System.Collections.Generic;
using SocketMeister;

// Define endpoints
var endpoints = new List<SocketEndPoint>
{
    new SocketEndPoint("server1.example.com", 5000),
    new SocketEndPoint("server2.example.com", 5000)
};

var client = new SocketClient(endpoints, EnableCompression: false, friendlyName: "MultiEPClient");

client.ConnectionStatusChanged += (s, e) =>
{
    Console.WriteLine($"Status: {e.OldStatus} -> {e.NewStatus} (Reason: {e.Reason})");
};

client.CurrentEndPointChanged += (s, e) =>
{
    Console.WriteLine($"Endpoint changed: {e.OldEndPoint?.Description} -> {e.NewEndPoint.Description}");
};

client.Start();

// Send messages as usual (to the current active endpoint)
for (int i = 0; i < 10; i++)
{
    var response = client.SendMessage(new object[] { $"Message {i}" }, TimeoutMilliseconds: 5000);
    Console.WriteLine($"Reply length: {response?.Length ?? 0}");
}

client.Stop();
```

Notes:
- SocketClient uses the best available endpoint as `CurrentEndPoint` and automatically reconnects/fails over when needed.
- Messages are sent over the currently connected endpoint (no per-message round-robin).