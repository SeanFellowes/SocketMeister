SocketMeister Test Harness

Projects
- tests/SocketMeister.Tests.Common: shared helpers (port allocator, event sink)
- tests/SocketMeister.Tests.Integration: xUnit integration smoke tests (net8.0, net472)
- tests/SocketMeister.Tests.Compatibility: orchestrates the .NET 3.5 client driver
- tests/SocketMeister.Tests.Client35.Driver: .NET 3.5 console driver (SDK-style)

Run
- dotnet build tests/SocketMeister.Tests.Client35.Driver (optional for 3.5 scenarios)
- dotnet test tests/SocketMeister.Tests.Integration -f net8.0
- dotnet test tests/SocketMeister.Tests.Integration -f net472
- dotnet test tests/SocketMeister.Tests.Compatibility -f net8.0

Notes
- Integration tests use ephemeral ports on 127.0.0.1.
- The 3.5 driver builds with Microsoft.NETFramework.ReferenceAssemblies; running requires .NET 3.5 runtime installed.

