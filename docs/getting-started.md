# Getting Started with SocketMeister

This guide walks you through the essential steps to install, configure, and run your first SocketMeister client and server applications.

## Prerequisites

- **.NET SDK**: .NET Framework 3.5 or higher, .NET Core 3.1, .NET 5/6/7/8
- **IDE or Editor**: Visual Studio 2019+, Visual Studio Code, JetBrains Rider, or equivalent
- **NuGet**: Integrated in your IDE or via `dotnet` CLI

## 1. Install the Package

Add SocketMeister to your project via NuGet:

```powershell
Install-Package SocketMeister -Version 10.2.4
```

Or using the .NET CLI:

```bash
dotnet add package SocketMeister --version 10.2.4
```

## 2. Create the Server

1. In Visual Studio (or your preferred IDE), create a new Console Application project.
2. Add `using SocketMeister;` at the top of `Program.cs`.
3. Replace the `Main` method with:

```csharp
static void Main(string[] args)
{
    // Listen on port 5000 for any IPv4 address
    var server = new SocketServer("0.0.0.0", 5000);

    server.MessageReceived += (sender, e) =>
    {
        Console.WriteLine($"Received from {e.RemoteEndPoint}: {e.Message}");
        if (e.IsRequest)
        {
            // Automatically respond to request messages
            e.Response = $"Echo: {e.Message}";
        }
    };

    server.Start();
    Console.WriteLine("SocketMeister server running on port 5000. Press Enter to exit.");
    Console.ReadLine();
    server.Stop();
}
```

4. **Build and run** the project. You should see the “SocketMeister server running…” message.


## 3. Create the Client

1. In a separate solution or project folder, create another Console Application.
2. Add the same NuGet package and `using SocketMeister;`.
3. Replace the `Main` method with:

```csharp
static void Main(string[] args)
{
    // Connect to localhost on port 5000
    var client = new SocketClient("127.0.0.1", 5000);
    client.Connect();

    Console.WriteLine("Connected to server.");
    var message = "Hello, SocketMeister!";
    client.Send(message);
    Console.WriteLine($"Sent: {message}");

    // Receive and display the response
    var response = client.Receive();
    Console.WriteLine($"Received: {response}");

    client.Disconnect();
}
```

4. **Build and run** the client. Watch the server console echo your message back.


## 4. Explore and Experiment

- **Multiple Endpoints**: Pass multiple host/port pairs to `SocketClient` for round‑robin or failover.
- **Compression**: Enable built‑in compression via `new SocketClient(hosts, port, enableCompression: true)`.
- **Events & Requests**: Use `RequestReceived` and `ResponseReceived` events for richer protocols.


## Next Steps

- Visit the [API reference](/api/index.html) to explore all classes and methods.
- Check out the [architecture overview](architecture.md) for insights into threading, scalability, and fault tolerance.
- Dive into advanced samples under `docs/samples/` to see real‑world scenarios.

Happy coding with SocketMeister!

