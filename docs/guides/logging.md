# Logging

SocketMeister exposes two primary logging surfaces on the client (and a logging event on the server):

- `LogRaised` (rich telemetry): information, warnings, errors, internals, and message flow.
- `ExceptionRaised` (errors only): a narrow channel that emits only exceptions.

## Client

```csharp
client.LogRaised += (s, e) =>
{
    Console.WriteLine($"[{e.LogEntry.Severity}] {e.LogEntry.EventType}: {e.LogEntry.Message}");
};

client.ExceptionRaised += (s, e) =>
{
    Console.WriteLine("ERROR: " + e.Exception);
};
```

Tips:
- Subscribe to both for development; in production, many apps listen only to `ExceptionRaised`.
- Events are raised from background threads; keep handlers non-blocking.

## Server

```csharp
server.LogRaised += (s, e) =>
{
    Console.WriteLine($"[Server {e.LogEntry.Severity}] {e.LogEntry.Message}");
};
```

## Connection telemetry

The client raises typed connection events you can log directly:

```csharp
client.ConnectionStatusChanged += (s, e) =>
{
    Console.WriteLine($"Status: {e.OldStatus} -> {e.NewStatus} (EP: {e.EndPoint?.Description}, Reason: {e.Reason})");
};

client.CurrentEndPointChanged += (s, e) =>
{
    Console.WriteLine($"Endpoint changed: {e.OldEndPoint?.Description} -> {e.NewEndPoint.Description}");
};
```