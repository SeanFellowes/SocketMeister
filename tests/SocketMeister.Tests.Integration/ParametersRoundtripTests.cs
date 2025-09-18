using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ParametersRoundtripTests
{
    private static string Canonical(object[] p)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < p.Length; i++)
        {
            object v = p[i];
            if (v == null) { sb.Append("Null"); }
            else if (v is bool b) { sb.Append("Bool:").Append(b ? "true" : "false"); }
            else if (v is short i16) { sb.Append("Int16:").Append(i16.ToString(CultureInfo.InvariantCulture)); }
            else if (v is int i32) { sb.Append("Int32:").Append(i32.ToString(CultureInfo.InvariantCulture)); }
            else if (v is long i64) { sb.Append("Int64:").Append(i64.ToString(CultureInfo.InvariantCulture)); }
            else if (v is ushort u16) { sb.Append("UInt16:").Append(u16.ToString(CultureInfo.InvariantCulture)); }
            else if (v is uint u32) { sb.Append("UInt32:").Append(u32.ToString(CultureInfo.InvariantCulture)); }
            else if (v is ulong u64) { sb.Append("UInt64:").Append(u64.ToString(CultureInfo.InvariantCulture)); }
            else if (v is double dbl) { sb.Append("Double:").Append(dbl.ToString("R", CultureInfo.InvariantCulture)); }
            else if (v is float flt) { sb.Append("Single:").Append(flt.ToString("R", CultureInfo.InvariantCulture)); }
            else if (v is decimal dec) { sb.Append("Decimal:").Append(dec.ToString(CultureInfo.InvariantCulture)); }
            else if (v is string s) { sb.Append("String:").Append(s.Replace("|","\\|").Replace(";","\\;")); }
            else if (v is byte by) { sb.Append("Byte:").Append(by.ToString(CultureInfo.InvariantCulture)); }
            else if (v is sbyte sby) { sb.Append("SByte:").Append(sby.ToString(CultureInfo.InvariantCulture)); }
            else if (v is char ch) { sb.Append("Char:").Append(((int)ch).ToString(CultureInfo.InvariantCulture)); }
            else if (v is byte[] buf) { sb.Append("ByteArray:").Append(Convert.ToBase64String(buf)); }
            else if (v is DateTime dt) { sb.Append("DateTime:").Append(dt.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture)); }
            else if (v is DateTimeOffset dto) { sb.Append("DateTimeOffset:").Append(dto.UtcDateTime.Ticks.ToString(CultureInfo.InvariantCulture)).Append("@").Append(((int)dto.Offset.TotalMinutes).ToString(CultureInfo.InvariantCulture)); }
            else if (v is TimeSpan ts) { sb.Append("TimeSpan:").Append(ts.Ticks.ToString(CultureInfo.InvariantCulture)); }
            else if (v is Guid gd) { sb.Append("Guid:").Append(gd.ToString("N")); }
            else { sb.Append("Unsupported:").Append(v.GetType().FullName); }
            if (i < p.Length - 1) sb.Append(";");
        }
        return sb.ToString();
    }

    private static object[] BuildAllTypes()
    {
        return new object[]
        {
            true, false, (short)-123, (int)123456, (long)-9999999999L,
            (ushort)65000, (uint)4000000000, (ulong)9000000000000000000UL,
            123.456d, 78.9f, 12.34m,
            "hello", (byte)0x2A, (sbyte)-5, 'Z',
            new byte[] { 1,2,3,4,5 },
            new DateTime(2020,1,2,3,4,5, DateTimeKind.Utc),
            new DateTimeOffset(new DateTime(2021,2,3,4,5,6, DateTimeKind.Unspecified), TimeSpan.FromHours(2)),
            TimeSpan.FromMinutes(123),
            Guid.NewGuid(),
            null
        };
    }

    [Trait("Category","Parameters")]
    [Fact]
    public async Task ClientToServer_AllSupportedTypes_EchoAsCanonical()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.MessageReceived += (s, e) => { var str = Canonical(e.Parameters); e.Response = Encoding.UTF8.GetBytes(str); };
        server.Start();
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "ParamsClient");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(20000));
            Assert.True(tcs.Task.IsCompleted, "Client did not connect");

            var arr = BuildAllTypes();
            var reply = client.SendMessage(arr, 10000);
            var text = Encoding.UTF8.GetString(reply);
            Assert.Equal(Canonical(arr), text);

            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","Parameters")]
    [Fact]
    public async Task ServerToClient_AllSupportedTypes_RoundtripCanonical()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);

        SocketServer.Client remote = null;
        var connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        server.ClientConnected += (s, e) => { remote = e.Client; connected.TrySetResult(true); };
        server.Start();
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "ParamsClient2");
            // Client will echo server-sent params as canonical string in Response
            client.MessageReceived += (s, e) => { e.Response = Encoding.UTF8.GetBytes(Canonical(e.Parameters)); };
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(20000));
            Assert.True(tcs.Task.IsCompleted, "Client did not connect");
            await Task.WhenAny(connected.Task, Task.Delay(5000));
            Assert.True(connected.Task.IsCompleted, "Server did not capture client");
            Assert.NotNull(remote);

            var arr = BuildAllTypes();
            var resp = remote!.SendMessage(arr, 10000, false);
            Assert.NotNull(resp);
            var str = Encoding.UTF8.GetString(resp);
            Assert.Equal(Canonical(arr), str);

            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}
