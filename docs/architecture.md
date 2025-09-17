# Architecture Overview

This document describes the core design of SocketMeister and how it achieves throughput, resilience, and a simple developer experience.

## 1. Core Components

- **SocketClient**
  - Maintains a single active TCP connection to a selected endpoint.
  - Performs a handshake before reporting `Connected`.
  - Automatic reconnection and failover across multiple endpoints when errors occur.
  - Emits typed events: `ConnectionStatusChangedEventArgs`, `CurrentEndPointChangedEventArgs`.

- **SocketServer**
  - Listens on a specified port and accepts concurrent client connections.
  - Raises `MessageReceived` for application logic; the handler can optionally set `e.Response` (byte[]), enabling request/response.

## 2. Threading & Concurrency

- **Client background worker**
  - A background thread drives connection attempts, polling, subscription sync, and receive processing.
  - On .NET 4.0+ additional background tasks are used to process inbound messages and support graceful shutdown.

- **Server listener and workers**
  - A dedicated listener thread accepts connections and per-connection receive loops process messages.

- **Synchronization**
  - Client uses `ReaderWriterLockSlim` and `lock` for key state; server uses thread-safe patterns around connection lists and message dispatch.

## 3. Connection Management

- **Explicit start**
  - In v11 the client does not auto-start. Construct → subscribe to events → call `Start()`.

- **Status transitions**
  - `ConnectionStatus` reflects `Connecting` until the handshake completes, even if the socket is open.
  - `ConnectionStatusChanged` is raised with `OldStatus`, `NewStatus`, `EndPoint`, and a `ClientDisconnectReason` when applicable.

- **Failover and backoff**
  - When multiple endpoints are configured, the client selects the endpoint with the earliest reconnect eligibility.
  - Backoff durations depend on the disconnect reason (e.g., timeouts vs. incompatibilities).

## 4. Message Framing & Protocol

- **Header + body**
  - Each message has an 11-byte header: message type (Int16), compression flag (bool), compressed length (Int32), uncompressed length (Int32).
  - After the header, the body contains serialized parameters or response content.

- **Compression**
  - Large payloads may be compressed based on size; bodies are decompressed transparently on receipt.

- **Request/Response**
  - Client sends via `SendMessage(object[] parameters, int timeoutMs, bool isLongPolling=false, string friendlyName=null)` and blocks until a response or timeout.
  - Server handles `MessageReceived` and (optionally) sets `e.Response`.

## 5. Subscriptions & Broadcasts

- Clients can add/remove named subscriptions. The server can send broadcasts, and the client raises `BroadcastReceived`.

## 6. Logging & Diagnostics

- **Events**
  - `LogRaised` surfaces structured log entries with severity and event type.
  - `ExceptionRaised` is an error-only channel for consumers who want only failures.

- **Observability**
  - `ServerVersion` is set after handshake and indicates the remote server’s SocketMeister version.

## 7. Performance Considerations

- Buffer reuse via pooled `SocketAsyncEventArgs` objects.
- Message framing and serialization reduce allocations and support large payloads.

---

For code examples, see the [Getting Started](getting-started.md) and [Samples](samples/index.md). For API details, refer to the [reference](/api/index.html).