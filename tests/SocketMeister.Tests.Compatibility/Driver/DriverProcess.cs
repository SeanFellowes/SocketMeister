using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketMeister.Tests.Compatibility.Driver;

public static class DriverProcess
{
    public static async Task<(int ExitCode, string StdOut, string StdErr)> RunAsync(string exePath, string args, TimeSpan timeout)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        proc.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
        proc.Exited += (_, __) => tcs.TrySetResult(proc.ExitCode);

        if (!proc.Start()) throw new InvalidOperationException("Failed to start driver process.");
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        using var cts = new CancellationTokenSource(timeout);
        await using var _ = cts.Token.Register(() => tcs.TrySetCanceled());

        int exit;
        try { exit = await tcs.Task.ConfigureAwait(false); }
        catch (TaskCanceledException)
        {
            try { if (!proc.HasExited) proc.Kill(true); } catch { }
            throw new TimeoutException("Driver process timed out.");
        }

        return (exit, stdout.ToString(), stderr.ToString());
    }
}

