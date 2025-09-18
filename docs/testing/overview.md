# Testing Overview

This repository includes an automated, multi-target test harness for SocketMeister.

Goals
- Validate client and server across .NET 4.7.2 and .NET 8.
- Exercise the .NET 3.5 client via a console driver.
- Make tests easy to run locally and in CI.

Projects
- `tests/SocketMeister.Tests.Common`: Shared helpers.
- `tests/SocketMeister.Tests.Integration` (xUnit): end-to-end scenarios (net8.0, net472).
- `tests/SocketMeister.Tests.Compatibility` (xUnit): orchestrates the .NET 3.5 driver.
- `tests/SocketMeister.Tests.Client35.Driver` (net35): console program that performs simple client actions.

How To Run
- `dotnet build tests/SocketMeister.Tests.Client35.Driver` (optional; required for 3.5 tests)
- `dotnet test tests/SocketMeister.Tests.Integration -f net8.0`
- `dotnet test tests/SocketMeister.Tests.Integration -f net472`
- `dotnet test tests/SocketMeister.Tests.Compatibility -f net8.0`

Environment
- Tests bind to `127.0.0.1` using ephemeral ports by default.

