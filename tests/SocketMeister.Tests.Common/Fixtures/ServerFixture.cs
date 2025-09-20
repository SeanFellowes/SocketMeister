using System;
using System.Text;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Common.Fixtures;

public sealed class ServerFixture : IAsyncLifetime
{
    public SocketServer Server { get; private set; } = null!;
    public int Port { get; private set; }

    public async Task InitializeAsync()
    {
        Server = new SocketServer(0, CompressSentData: false);

        // Simple echo handler for harness validation
        Server.MessageReceived += (s, e) =>
        {
            if (e.Parameters?.Length > 0)
            {
                if (e.Parameters[0] is string str)
                    e.Response = Encoding.UTF8.GetBytes(str);
                else
                    e.Response = BitConverter.GetBytes(e.Parameters.Length);
            }
        };

        Server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(Server);
        Port = ServerTestHelpers.GetBoundPort(Server);
    }

    public Task DisposeAsync()
    {
        try { Server.Stop(); } catch { }
        (Server as IDisposable)?.Dispose();
        return Task.CompletedTask;
    }
}
