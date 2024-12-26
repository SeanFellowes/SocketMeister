Please visit the [SocketMeister project website on GitHub](https://github.com/SeanFellowes/SocketMeister) for detailed documentation and C# examples to get you quickly up an running.

SocketMeister is a socket library for solutions requiring persistent, fault tolerant, multiple endpoint TCP/IP connectivity. It is easy to use, high throughput and multithreaded. SocketMeister can scale from a single socket server to a farm of server instances.

The SocketMeister `SocketClient` provides fault tolerance by automatically reconnecting in the event of connection failure and in environment where multiple servers are deployed (For example, for redundancy and/or high workload), SocketMeister Client will automatically round-robin until a server is found. 

The SocketMeister `SocketServer` provides a simple framework whereby messages and requests (messages expecting a response) are presented in thread independent events. Your code sits cleanly in the event handler, where it can read an array of parameters sent by the client and execute your business logic. In the case of a request, your code can provide a response, which is automatically returned to the client. SocketMeister supports binary arrays, strings and most simple data types. It you enable optional compression, data is seamlessly compressed and decompressed using a high throughput compression algorithm.

## SocketMeister NuGet Packages

SocketMeister `SocketClient` works with many .NET framework versions, from .NET 3.5 to .NET 9 

SocketMeister `SocketServer` is included in DLLs compiled with .NET framework 4.0 and above. 

There are 2 active NuGet packages available:

1. The [SocketMeister](https://www.nuget.org/packages/SocketMeister/) NuGet package contains a single DLL for .NET Frameworks from .NET 3.5 to .NET 9. The SocketMeister `SocketClient` in included in all frameworks but the `SocketServer` component is not available in .NET 3.5.  

2. The [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) NuGet package contains the C# source files for SocketMeister, enabling you to embed SocketMeister in your own project EXE or DLL. This eliminates the need to ship the SocketMeister DLL with your product. Simply add the [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) NuGet package to your Visual Studio project and a folder containing the code will be added to the project.

## SocketMeister for Silverlight is no longer maintained.

The [SocketMeister.Silverlight](https://www.nuget.org/packages/SocketMeister.Silverlight/) version is no longer maintained since version 4.0.4 but is compatible with later versions of the SocketMeister `SocketServer` class in later versions of major version 4.

