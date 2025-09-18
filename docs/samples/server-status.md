# Server Status Events

Demonstrates handling server lifecycle changes via the typed `StatusChanged` event.

## Scenario
Log when the server starts, stops, or restarts, including the bound endpoint.

## Code Example
```csharp
using System;
using System.Net;
using SocketMeister;

var options = new SocketServerOptions
{
    BindAddress = IPAddress.Any,
    Port = 5005,
    Backlog = 200,
    CompressSentData = false
};

var server = new SocketServer(options);

// Typed status event: OldStatus/NewStatus + EndPoint
server.StatusChanged += (s, e) =>
{
    Console.WriteLine($"Status: {e.OldStatus} -> {e.NewStatus} @ {e.EndPoint}");
};

server.Start();

Console.WriteLine("Press R to restart, Q to quit...");
while (true)
{
    var key = Console.ReadKey(intercept: true).Key;
    if (key == ConsoleKey.R)
    {
        server.Stop();
        server.Start();
    }
    else if (key == ConsoleKey.Q)
    {
        break;
    }
}

server.Stop();
```

Notes:
- `StatusChanged` is raised with `OldStatus`, `NewStatus`, and the server `EndPoint` string.
- The server supports restart (Stop → Start) on the same instance.
