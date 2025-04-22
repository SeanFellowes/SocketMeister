# Custom Serialization

Shows how to plug in your own serializers (e.g. JSON or Protobuf).

## Scenario
You need to send complex objects rather than plain strings.

## Code Example
```csharp
using SocketMeister;
using Newtonsoft.Json;

// Create client with JSON serializer callbacks
var client = new SocketClient(
    host: "localhost",
    port: 5003,
    serializer: obj => JsonConvert.SerializeObject(obj),
    deserializer: str => JsonConvert.DeserializeObject<MyDto>(str)
);

client.Connect();

// Send a DTO
client.Send(new MyDto { Id = 42, Name = "Foobar" });
var responseDto = (MyDto)client.Receive();

Console.WriteLine($"Received DTO with Name={responseDto.Name}");