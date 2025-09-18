# Upgrading to Version 11

This guide summarizes the breaking changes and how to migrate client and server code to SocketMeister v11.

## Overview

Version 11 focuses on lifecycle correctness, explicit startup, and clearer events:

- Client no longer auto-starts; you call `Start()` after subscribing to events.
- Typed client connection events (`ConnectionStatusChangedEventArgs`, `CurrentEndPointChangedEventArgs`).
- Server constructor is side‑effect free; call `Start()` to bind and listen.
- Server `StatusChanged` is typed (`ServerStatusChangedEventArgs`).
- Server raises `ClientConnected` only after handshake completes.
- Server supports restart (Stop → Start) within the same instance.

## Client Migration

1) Replace auto‑start with explicit Start:

```csharp
var endpoints = new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", 5000) };
var client = new SocketClient(endpoints, EnableCompression: false, friendlyName: "AppClient");

// Subscribe first
client.ConnectionStatusChanged += (s, e) =>
    Console.WriteLine($"Status: {e.OldStatus} -> {e.NewStatus} (Reason: {e.Reason})");
client.CurrentEndPointChanged += (s, e) =>
    Console.WriteLine($"EP: {e.OldEndPoint?.Description} -> {e.NewEndPoint.Description}");

client.Start();
```

2) Update event handler signatures:

```csharp
// Old: EventArgs
// client.ConnectionStatusChanged += (s, e) => { /* ... */ };
// client.CurrentEndPointChanged += (s, e) => { /* ... */ };

// New: typed args
client.ConnectionStatusChanged += (s, e) => { /* use e.OldStatus, e.NewStatus, e.Reason */ };
client.CurrentEndPointChanged += (s, e) => { /* use e.OldEndPoint, e.NewEndPoint */ };
```

3) Use `ExceptionRaised` for error‑only notifications and `LogRaised` for full telemetry.

## Server Migration

1) Explicit Start and typed `StatusChanged`:

```csharp
var server = new SocketServer(new SocketServerOptions
{
    BindAddress = System.Net.IPAddress.Any,
    Port = 5000,
    Backlog = 500,
    CompressSentData = false
});

server.StatusChanged += (s, e) =>
    Console.WriteLine($"Server: {e.OldStatus} -> {e.NewStatus} @ {e.EndPoint}");

server.Start();
```

2) `ClientConnected` fires after handshake completes.

If you relied on `ClientConnected` during accept, move any logic that required a “ready” client to the `ClientConnected` handler (now raised post-handshake). This simplifies downstream logic.

3) Restart support

You can now call `server.Stop()` then `server.Start()` on the same instance. This replaces patterns that previously created a new server instance to restart.

## Handshake Changes

- The server no longer uses a fixed delay before sending Handshake1; it retries Handshake1 a few times if Handshake2 has not been received yet.
- The client continues to report `Connecting` until the handshake completes.

## Quick Checklist

- [ ] Add `client.Start()` after constructing and subscribing to events.
- [ ] Update client event handlers to typed args.
- [ ] Subscribe to `server.StatusChanged` and handle typed args.
- [ ] Ensure any logic triggered by `ClientConnected` assumes a post‑handshake, ready client.
- [ ] Remove any helper code used to restart servers; use `Stop()` → `Start()` instead.

If you have questions or need help with a specific migration scenario, open an issue or start a discussion.

