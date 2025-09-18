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

Performance scenarios (throughput/latency, compression effects) will be added separately as opt-in benchmarks.
