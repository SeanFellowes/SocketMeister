using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SocketMeister.Tests.Compatibility.Driver;
using SocketMeister.Tests.Common.Fixtures;
using Xunit;

namespace SocketMeister.Tests.Compatibility;

// Orchestrates the .NET 3.5 client driver. Skips if not buildable/runnable.
public class Client35HarnessTests : IClassFixture<ServerFixture>
{
    private readonly ServerFixture _fixture;
    public Client35HarnessTests(ServerFixture fx) { _fixture = fx; }

    private static string? FindDriverExe()
    {
        // Build output path convention (Debug)
        var projRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SocketMeister.Tests.Client35.Driver"));
        var exe = Path.Combine(projRoot, "bin", "Debug", "net35", "SocketMeister.Tests.Client35.Driver.exe");
        return File.Exists(exe) ? exe : null;
    }

    [Trait("Category","Compatibility")]
    [Fact]
    public async Task Driver_Version_Prints()
    {
        var exe = FindDriverExe();
        if (exe == null)
            Assert.True(false, "Client35 driver not built. Build tests/SocketMeister.Tests.Client35.Driver first.");

        var (code, stdout, stderr) = await DriverProcess.RunAsync(exe, "version", TimeSpan.FromSeconds(10));
        Assert.Equal(0, code);
        Assert.Contains("\"driver\":\"net35\"", stdout);
        Assert.True(string.IsNullOrWhiteSpace(stderr));
    }

    [Trait("Category","Compatibility")]
    [Fact]
    public async Task Driver_ConnectEcho_Works()
    {
        var exe = FindDriverExe();
        if (exe == null)
            Assert.True(false, "Client35 driver not built. Build tests/SocketMeister.Tests.Client35.Driver first.");

        var msg = "HarnessPing";
        var args = $"connect-echo 127.0.0.1 {_fixture.Port} {msg}";
        var (code, stdout, stderr) = await DriverProcess.RunAsync(exe, args, TimeSpan.FromSeconds(20));
        Assert.Equal(0, code);
        Assert.Contains("\"ok\":true", stdout);
        Assert.True(string.IsNullOrWhiteSpace(stderr));
    }
}
