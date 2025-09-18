using System;
using System.Collections.Generic;
using System.Text;
using SocketMeister;

namespace SocketMeister.Tests.Client35.Driver
{
    internal static class Program
    {
        // Lightweight entry points to exercise basic client functions from .NET 3.5 without a test runner.
        // Output is line-oriented JSON for orchestration by the Compatibility tests.
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("{\"ok\":true,\"cmd\":\"noop\"}");
                    return 0;
                }

                var cmd = args[0].ToLowerInvariant();
                if (cmd == "version")
                {
                    Console.WriteLine("{\"ok\":true,\"cmd\":\"version\",\"driver\":\"net35\"}");
                    return 0;
                }
                else if (cmd == "connect-echo")
                {
                    // Args: connect-echo <host> <port> <message>
                    var host = args.Length > 1 ? args[1] : "127.0.0.1";
                    var port = args.Length > 2 ? int.Parse(args[2]) : 5000;
                    var message = args.Length > 3 ? args[3] : "ping";

                    var endpoints = new List<SocketEndPoint> { new SocketEndPoint(host, port) };
                    var client = new SocketClient(endpoints, false, "Driver35");
                    try
                    {
                        client.Start();

                        var reply = client.SendMessage(new object[] { message }, 5000, false, null);
                        var text = reply != null ? Encoding.UTF8.GetString(reply) : null;
                        var ok = text == message;
                        Console.WriteLine("{\"ok\":" + (ok ? "true" : "false") + ",\"cmd\":\"connect-echo\",\"reply\":\"" + (text ?? "") + "\"}");
                        return ok ? 0 : 2;
                    }
                    finally
                    {
                        try { client.Stop(); } catch { }
                        try { (client as IDisposable).Dispose(); } catch { }
                    }
                }
                else
                {
                    Console.WriteLine("{\"ok\":false,\"error\":\"unknown-command\"}");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.GetType().FullName + ": " + ex.Message;
                Console.WriteLine("{\"ok\":false,\"error\":\"" + msg.Replace("\"", "'") + "\"}");
                return 1;
            }
        }
    }
}

