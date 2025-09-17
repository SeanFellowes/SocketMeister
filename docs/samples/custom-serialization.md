# Custom Serialization

Demonstrates sending complex objects using your preferred serializer (e.g., JSON), carried as strings or byte arrays.

## Scenario
You want to send DTOs rather than only primitive types.

## Code Example
```csharp
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SocketMeister;

public class MyDto { public int Id { get; set; } public string Name { get; set; } }

// Server
var server = new SocketServer(port: 5003, CompressSentData: false);
server.MessageReceived += (s, e) =>
{
    // Expect JSON string in parameter[0]
    string json = (string)e.Parameters[0];
    var dto = JsonConvert.DeserializeObject<MyDto>(json);

    // Process and return a JSON reply
    dto.Name = dto.Name + "_ACK";
    string replyJson = JsonConvert.SerializeObject(dto);
    e.Response = Encoding.UTF8.GetBytes(replyJson);
};
server.Start();

// Client
var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("localhost", 5003) }, EnableCompression: false, friendlyName: "CustomSerClient");
client.Start();

var outgoing = new MyDto { Id = 42, Name = "Foobar" };
string payload = JsonConvert.SerializeObject(outgoing);

byte[] bytes = client.SendMessage(new object[] { payload }, TimeoutMilliseconds: 5000);
string replyJson = Encoding.UTF8.GetString(bytes);
var responseDto = JsonConvert.DeserializeObject<MyDto>(replyJson);

Console.WriteLine($"Received DTO with Name={responseDto.Name}");

client.Stop();
server.Stop();
```

Notes:
- SocketMeister serializes message parameters (e.g., string, int, byte[]) automatically. For custom object graphs, serialize to string or byte[] yourself.
- Consider enabling compression for large JSON payloads.