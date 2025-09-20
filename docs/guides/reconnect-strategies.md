# Reconnect Strategies

SocketMeister clients automatically reconnect and fail over across endpoints. You can observe and influence behavior via events and configuration.

## Start and subscribe first

```csharp
var client = new SocketClient(endpoints, EnableCompression: false, friendlyName: "AppClient");
client.ConnectionStatusChanged += (s, e) =>
{
    Console.WriteLine($"Status: {e.OldStatus} -> {e.NewStatus} (Reason: {e.Reason})");
};
client.CurrentEndPointChanged += (s, e) =>
{
    Console.WriteLine($"EP: {e.OldEndPoint?.Description} -> {e.NewEndPoint.Description}");
};
client.Start();
```

## Backoff and failover

- Each `SocketEndPoint` tracks when it is next eligible to reconnect. The client selects the endpoint with the earliest eligibility.
- Backoff duration varies by `ClientDisconnectReason` (e.g., timeouts vs. incompatibilities) to avoid hot-loops.
- During outages, the client reports `Connecting`, then `Disconnected` if attempts fail, and will retry until stopped.

### Updating endpoints at runtime

In 11.1+, you can adjust endpoints without recreating the client:

```csharp
// Replace all endpoints
client.SetEndPoints(new[]
{
    new SocketEndPoint("10.0.0.5", 6000),
    new SocketEndPoint("10.0.0.6", 6000)
});

// Add/remove specific entries
client.AddEndPoint("10.0.0.7", 6000);
client.RemoveEndPoint("10.0.0.5", 6000);
```

Notes:
- Newly supplied endpoints are made immediately eligible (internal backoff cleared) so the worker considers them promptly.
- If the current endpoint is removed, the client disconnects and reconnects to an eligible endpoint.

## Heartbeats and timeouts

- The client sends periodic poll requests; if the server does not respond within a configured window, it disconnects and retries.
- Message sends use per-message timeouts (e.g., `TimeoutMilliseconds` on `SendMessage`).

## Operational tips

- Always subscribe to connection events before calling `Start()`.
- Use `IsRunning` to check the background worker state.
- Monitor `ServerVersion` after handshake for compatibility diagnostics.
- For tests or blue/green deploys, add a short settle (~200ms) after server start before initiating new client connections on slow hosts.
