# Built-in Compression

Enable compression on both client and server for bandwidth-heavy payloads.

## Scenario
You're sending large text or binary data and want to reduce network usage.

## Code Example
```csharp
using System;
using System.Collections.Generic;
using System.Text;
using SocketMeister;

// Enable compression on both sides
var server = new SocketServer(port: 5001, CompressSentData: true);
server.MessageReceived += (s, e) =>
{
    // Echo back the first string parameter
    string msg = (string)e.Parameters[0];
    e.Response = Encoding.UTF8.GetBytes(msg);
};
server.Start();

var endpoints = new List<SocketEndPoint> { new SocketEndPoint("localhost", 5001) };
var client = new SocketClient(endpoints, EnableCompression: true, friendlyName: "CompressionClient");
client.Start();

string payload = new string('X', 100_000);
byte[] reply = client.SendMessage(new object[] { payload }, TimeoutMilliseconds: 10000);
Console.WriteLine("Client sent compressed payload and received " + reply.Length + " bytes.");

client.Stop();
server.Stop();
```