Please visit the [SocketMeister project website on GitHub](https://github.com/SeanFellowes/SocketMeister) for detailed documentation and C# examples to get you quickly up an running.

SocketMeister is a socket library for solutions requiring persistent, fault tolerant, multiple endpoint TCP/IP connectivity. It is easy to use, high throughput and multithreaded. SocketMeister can scale from a single socket server to a farm of server instances.

The SocketMeister `SocketClient` provides fault tolerance by automatically reconnecting in the event of connection failure and in environment where multiple servers are deployed (For example, for redundancy and/or high workload), SocketMeister Client will automatically round-robin until a server is found. 

The SocketMeister `SocketServer` provides a simple framework whereby messages and requests (messages expecting a response) are presented in thread independent events. Your code site cleanly in the event handler, where it can read an array of parameters sent by the client and execute your business logic. In the case of a request, your code can provide a response, which is automatically returned to the client. SocketMeister supports binary arrays, strings and most simple data types. It you enable optional compression, data is seamlessly compressed and decompressed using a high throughput compression algorithm.

## SocketMeister NuGet Packages

SocketMeister `SocketClient` works with many .NET framework versions, from aging .NET 3.5 and Silverlight 5 to .NET 5 and almost everything in-between. 

SocketMeister `SocketServer` is included in DLLs compiled with .NET framework 4.0 and above. 

There are 3 NuGet packages available:

1. The [SocketMeister](https://www.nuget.org/packages/SocketMeister/) NuGet package contains the DLL for numerous .NET Frameworks including.NET 3.5, .NET 4.0, .NET 4.5, .NET 4.6, .NET 4.7.2, .NET 4.8, .NET Standard 2.0, .NET Core 3.1, .NET 5 and .NET 6. The SocketMeister `SocketClient` works in all included frameworks. The SocketMeister `SocketServer` is not available in .NET 3.5.  

2. The [SocketMeister.Silverlight]() NuGet package contains the DLL for Silverlight 5 projects. The SocketMeister `SocketServer` class is not avaialable in Silverlight.

3. The [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) NuGet package contains the C# source files for SocketMeister, enabling you to embed SocketMeister in your own project EXE or DLL. This eliminates the need to ship the SocketMeister DLL with your product. Simply add the [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) NuGet package to your Visual Studio project and a folder containing the code will be added to the project.

