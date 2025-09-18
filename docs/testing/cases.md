# Test Cases (Initial)

Legend
- Scope: Client, Server, Both
- TFMs: net8.0, net472, net35 (driver)

- I-00 Harness Smoke: start server, connect client, echo send/receive — Both — net8.0, net472
- K-35 Driver Wireup: .NET 3.5 driver starts and prints version — Client — driver orchestrated by net8.0
- K-35-1 Driver Echo: driver connects to server and echoes payload — Both — driver orchestrated by net8.0

More detailed scenarios (failover, pub/sub, timeouts, performance) will be added incrementally following this numbering once the harness is validated.

