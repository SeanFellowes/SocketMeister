# Test Cases (Initial)

Legend
- Scope: Client, Server, Both
- TFMs: net8.0, net472, net35 (driver)

- I-00 Harness Smoke: start server, connect client, echo send/receive — Both — net8.0, net472
- K-35 Driver Wireup: .NET 3.5 driver starts and prints version — Client — driver orchestrated by net8.0
- K-35-1 Driver Echo: driver connects to server and echoes payload — Both — driver orchestrated by net8.0

# Integration Suite
- I-01 Failover: client reconnects across two endpoints as servers stop/start — Both — net8.0, net472
- I-02 PubSub: three clients subscribe to same topic; all receive broadcast — Both — net8.0, net472
- I-03 PubSub Routing: clients subscribe to different topics; routing is correct and silence for unknown topic — Both — net8.0, net472
- I-04 RemoveSubscription: remove topic stops routing; restart server and remaining subscriptions persist — Both — net8.0, net472
- I-05 Timeout: client times out (2s) when server delays response (3s) — Both — net8.0, net472
- I-06 Server Logging: server raises LogRaised when handler throws; verify event observed — Server — net8.0, net472
- I-07 Server→Client: server sends messages to one and many clients via Client.SendMessage and receives replies — Both — net8.0, net472

# Client Validation
- C-01 SendMessage null/empty parameters throw — Client — net8.0, net472
- C-02 AddSubscription duplicate throws; RemoveSubscription missing returns false — Client — net8.0, net472
- C-03 Start/Stop toggles IsRunning; ServerVersion available post-handshake — Client — net8.0, net472

# Compression
- X-01 Echo with compression on both sides — Both — net8.0, net472
- X-02 Large payload roundtrip with compression — Both — net8.0, net472

# Disconnect & Status
- D-01 Client raises ServerStopping when server stops — Both — net8.0, net472
- S-01 StatusChanged sequence on Start/Stop includes Starting/Started/Stopping/Stopped — Server — net8.0, net472
- S-02 Invalid port throws (options.Port out of range) — Server — net8.0, net472

Performance scenarios (throughput/latency, compression effects) will be added separately as opt-in benchmarks.
# Test Cases (Initial)

Legend
- Scope: Client, Server, Both
- TFMs: net8.0, net472, net35 (driver)

- I-00 Harness Smoke: start server, connect client, echo send/receive — Both — net8.0, net472
- K-35 Driver Wireup: .NET 3.5 driver starts and prints version — Client — driver orchestrated by net8.0
- K-35-1 Driver Echo: driver connects to server and echoes payload — Both — driver orchestrated by net8.0

# Integration Suite
- I-01 Failover: client reconnects across two endpoints as servers stop/start — Both — net8.0, net472
- I-01b Failover (Delayed Start): client has two endpoints; none are up initially; server 2 starts later; client connects then fails back when server 1 starts — Both — net8.0, net472
- I-02 PubSub: three clients subscribe to same topic; all receive broadcast — Both — net8.0, net472
- I-03 PubSub Routing: clients subscribe to different topics; routing is correct and silence for unknown topic — Both — net8.0, net472
- I-04 RemoveSubscription: remove topic stops routing; restart server and remaining subscriptions persist — Both — net8.0, net472
- I-05 Timeout: client times out (2s) when server delays response (3s) — Both — net8.0, net472
- I-06 Server Logging: server raises LogRaised when handler throws; verify event observed — Server — net8.0, net472
- I-07 Server→Client: server sends messages to one and many clients via Client.SendMessage and receives replies — Both — net8.0, net472

# Client Validation
- C-01 SendMessage null/empty parameters throw — Client — net8.0, net472
- C-02 AddSubscription duplicate throws; RemoveSubscription missing returns false — Client — net8.0, net472
- C-03 Start/Stop toggles IsRunning; ServerVersion available post-handshake — Client — net8.0, net472
- C-04 Constructor guards: SocketClient(null/empty EndPoints) throw; SocketEndPoint invalid IP/port throw — Client — net8.0, net472

# Compression
- X-01 Echo with compression on both sides — Both — net8.0, net472
- X-02 Large payload roundtrip with compression — Both — net8.0, net472
- X-03 Mixed compression: server-only compression — Both — net8.0, net472
- X-04 Mixed compression: client-only compression — Both — net8.0, net472

# Disconnect & Status
- D-01 Client raises ServerStopping when server stops — Both — net8.0, net472
- S-01 StatusChanged sequence on Start/Stop includes Starting/Started/Stopping/Stopped — Server — net8.0, net472
- S-02 Invalid port throws (options.Port out of range) — Server — net8.0, net472
- S-03 SocketServer(null options) throws — Server — net8.0, net472

Performance scenarios (throughput/latency, compression effects) will be added separately as opt-in benchmarks.
