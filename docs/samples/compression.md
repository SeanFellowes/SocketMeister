# Built‑in Compression

Shows how to enable GZip compression for bandwidth‑heavy payloads.

## Scenario
You’re sending large text or binary data and want to reduce network usage.

## Code Example
```csharp
using SocketMeister;
using System.Text;

// Enable compression on both client and server
var server = new SocketServer("0.0.0.0", 5001, enableCompression: true);
var client = new SocketClient("localhost", 5001, enableCompression: true);

server.MessageReceived += (s, e) =>
{
    Console.WriteLine($"Server got: {e.Message}");
    e.Response = e.Message; // echo
};

server.Start();
client.Connect();

string payload = new string('X', 100_000);
client.Send(payload);
Console.WriteLine("Client sent compressed payload.");

var reply = client.Receive();
Console.WriteLine("Client got response of length " + reply.Length);

client.Disconnect();