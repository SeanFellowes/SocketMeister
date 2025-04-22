# Multi‑Endpoint Failover

Demonstrates how to configure a client with multiple server endpoints for automatic load‑balancing and failover.

## Scenario
You have two server instances and want your client to round‑robin between them, automatically failing over if one goes down.

## Code Example
```csharp
using SocketMeister;

// Define two endpoints
var endpoints = new[]
{
    new HostPort("server1.example.com", 5000),
    new HostPort("server2.example.com", 5000)
};

var client = new SocketClient(endpoints);
client.Connect();

// Send 100 messages, cycling through endpoints
for (int i = 0; i < 100; i++)
{
    client.Send($"Message {i}");
    Console.WriteLine($"Sent Message {i}");
}

client.Disconnect();
