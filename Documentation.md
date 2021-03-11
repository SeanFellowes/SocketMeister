# SocketMeister

## Introduction

SocketMeister is an easy to use, high throughput, multithreaded, TCP Socket client and server for .NET. SocketMeister **Client** provides fault tolerance by automatically reconnecting in the event of connection failure and in environment where multiple servers are deployed (i.e. for redundancy and/or high workload), SocketMeister **Client** will automatically round-robin until a server is found. SocketMeister **Server** provides a simple framework whereby messages and requests (messages expecting a response) are presented in thread independent events. Your code site cleanly in the event handler, where it can read an array of parameters sent by the client and execute your business logic. In the case of a request, your code can provide a response, which is automatically returned to the client. SocketMeister supports binary arrays, strings and most simple data types. It you enable optional compression, data is seamlessly compressed and decompressed using a high throughput compression algorithm.

## The Flavours of SocketMeister

SocketMeister Client works with most .NET framework versions, from .NET 3.5 and Silverlight 5 to .NET 5 and almost everything in-between. SocketMeister Server can be used with .NET 4.0 and above. There are 3 NuGet packages available:

1. The [SocketMeister](https://www.nuget.org/packages/SocketMeister/) NuGet package contains the DLL for many versions of .NET except Silverlight 5. Frameworks include .NET 3.5, .NET 4.0, .NET 4.5, .NET 4.6, .NET 4.7.2, .NET Standard 2.0, .NET Core 3.1 and .NET 5. SocketMeister Server is not available in .NET 3.5.

2. The [SocketMeister.Silverlight]() NuGet package contains the DLL for Silverlight 5 projects. SocketMeister server is not included as it wouldn't work in Silverlight.

3. The [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) NuGet package contains the C# source files for SocketMeister, enabling you to embed SocketMeister in your own project EXE or DLL. This eliminates the need to ship the SocketMeister DLL with your product. Simply add the [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) NuGet package to your Visual Studio project and a folder containing the code will be added to the project.


## SocketServer Class.

Making a start with SocketServer requires only a few lines of code. The following will start the socket server on port 4505 and process messages and requests from clients. 

```
using System;
using SocketMeister;

namespace MyApplication
{
	public class Server
	{
		//  Instatiate SocketServer on port 4505 with compression enabled
		private readonly SocketServer _socketServer = new SocketServer(4505, true);

		public Server()
		{
			//  Listen to MessageReceived and RequestReceived events
			_socketServer.MessageReceived += SocketServer_MessageReceived;
			_socketServer.RequestReceived += SocketServer_RequestReceived;

			//  Start the socket server
			_socketServer.Start();
		}

		private void SocketServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
		{
			//  Example request
			if (Convert.ToString(e.Parameters[0]) == "GetTimezoneDisplayName")
			{
				//  Response is a binary array
				e.Response = System.Text.Encoding.ASCII.GetBytes(TimeZoneInfo.Local.DisplayName); 
			}
		}

		private void SocketServer_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
		{
			//  Example message
			if (Convert.ToString(e.Parameters[0]) == "DoSomething")
			{
				//  Do something here
			}
		}
		
		public void Stop()
		{
			//  Stop the socket server before exiting the application
			_socketServer.Stop();
		}
	}
}
```
