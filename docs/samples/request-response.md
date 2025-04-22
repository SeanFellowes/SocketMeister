# Request/Response Pattern

Illustrates synchronous request/reply messaging with timeouts.

## Scenario
Your client must send a query and wait for a server’s computed response.

## Code Example
```csharp
using SocketMeister;

var server = new SocketServer("0.0.0.0", 5002);
server.RequestReceived += (s, e) =>
{
    // Simulate work
    int result = int.Parse(e.Message) * 2;
    e.Response = result.ToString();
};
server.Start();

var client = new SocketClient("localhost", 5002);
client.Connect();

string request = "21";
string reply = client.SendRequest(request, timeout: TimeSpan.FromSeconds(5));
Console.WriteLine($"Request {request}, got reply {reply}");

client.Disconnect();
server.Stop();