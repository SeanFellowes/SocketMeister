# Architecture Overview

This document delves into the internal design of SocketMeister, outlining how it achieves high throughput, resilience, and flexibility.

## 1. Core Components

- **SocketClient**  
  - Manages outbound TCP connections to one or more endpoints.  
  - Implements automatic reconnection with exponential backoff.  
  - Supports round‑robin and failover across multiple servers.

- **SocketServer**  
  - Listens on a specified IP and port (IPv4/IPv6).  
  - Accepts concurrent client connections using a thread‑safe queue.  
  - Raises `MessageReceived` and `RequestReceived` events for application logic.

- **SocketSession** (internal)  
  - Encapsulates a single TCP socket and associated state.  
  - Handles send/receive loops on dedicated threads.


## 2. Threading & Concurrency

- **Dedicated I/O Threads**  
  - Each active session runs its own send/receive loops to avoid blocking.  
  - Background worker threads manage connection monitoring and reconnection.

- **Synchronization**  
  - Internal queues and event dispatch use thread‑safe collections (\`ConcurrentQueue<T>\`).  
  - Minimal locking ensures high concurrency with low contention.


## 3. Connection Management

- **Automatic Reconnection**  
  - On disconnect, the client enters a retry loop with configurable backoff intervals.  
  - Callbacks allow applications to monitor connection state.

- **Load Balancing & Failover**  
  - When multiple endpoints are provided, each send selects the next active session.  
  - If a session fails, traffic shifts seamlessly to remaining endpoints.


## 4. Message Framing & Protocol

- **Length-Prefixed Frames**  
  - Messages are sent with a 4‑byte length header, ensuring correct reassembly.  
  - Supports streaming of large payloads without partial message delivery.

- **Request/Response Pattern**  
  - `SendRequest` and `SendResponse` methods handle synchronous round‑trip calls.  
  - Timeouts and cancellation tokens ensure callers aren’t blocked indefinitely.


## 5. Optional Compression

- **Built-in GZip Compression**  
  - Toggleable per session or per message.  
  - Ideal for text-heavy or large binary payloads to reduce bandwidth.


## 6. Extensibility & Customization

- **Custom Serializers**  
  - Pass your own encoder/decoder delegates for JSON, Protobuf, or proprietary formats.  

- **Pluggable Logging**  
  - Expose events for raw byte sends/receives to integrate with your logging infrastructure.


## 7. Performance & Scaling

- **Benchmarks**  
  - Achieves thousands of messages per second with minimal GC pressure.  
  - Pooling of buffer arrays reduces memory churn.

- **Deployment Scenarios**  
  - Single instance apps to scaled-out microservices farms behind load balancers.  
  
---

For detailed APIs and configuration options, see the [API reference](/api/index.html). For code examples, check out the [Getting Started guide](getting-started.md) and our [samples folder](/docs/samples).

