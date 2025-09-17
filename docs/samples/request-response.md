# Request/Response Pattern

Illustrates synchronous request/reply messaging with timeouts using the v11 API.

## Scenario
Your client sends parameters to the server and waits for a byte[] response.

## Code Example
```csharp
using System;
using System.Collections.Generic;
using System.Text;
using SocketMeister;

// Server
var server = new SocketServer(port: 5002, CompressSentData: false);
server.MessageReceived += (s, e) =>
{
    // Parameters are strongly typed; expect a single string
    string input = (string)e.Parameters[0];
    int result = int.Parse(input) * 2;
    e.Response = Encoding.UTF8.GetBytes(result.ToString());
};
server.Start();

// Client
var endpoints = new List<SocketEndPoint> { new SocketEndPoint("localhost", 5002) };
var client = new SocketClient(endpoints, EnableCompression: false, friendlyName: "ReqRespClient");
client.Start();

string request = "21";
byte[] bytes = client.SendMessage(new object[] { request }, TimeoutMilliseconds: 5000);
string reply = Encoding.UTF8.GetString(bytes);
Console.WriteLine($"Request {request}, got reply {reply}");

client.Stop();
server.Stop();
```