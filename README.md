# SocketMeister

SocketMeister is a socket library for solutions requiring persistent, multiple endpoint TCP/IP connectivity. It is easy to use, resilient, high throughput and multithreaded.

**SocketClient** provides fault tolerance by automatically reconnecting in the event of connection failure. In an environment where multiple servers are deployed (i.e. for redundancy and/or high workload), SocketMeister **Client** will automatically round-robin to find a server it can connect to. 

**SocketServer** provides a simple framework whereby messages and requests (messages expecting a response) are presented to your code in thread independent events. Your code site cleanly in the event handler, where it can read an array of parameters sent by the client and execute your business logic. In the case of a request, your code can provide a response, which is automatically returned to the client. SocketMeister supports most data types, including binary arrays. It you enable optional compression, data is seamlessly compressed and decompressed using a high throughput compression algorithm.

## The Flavours of SocketMeister

SocketMeister Client works with most .NET framework versions from .NET 3.5 to .NET 9. SocketMeister Server can be used with .NET 4.0 and above. There are 2 NuGet packages available:

1. The [SocketMeister](https://www.nuget.org/packages/SocketMeister/) NuGet package contains the DLL for .NET 3.5 to .NET 9. The SocketMeister **SocketServer** class is not available in .NET 3.5.

2. The [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) NuGet package contains the C# source files for SocketMeister, enabling you to embed SocketMeister in your own project. This eliminates the need to ship the SocketMeister DLL with your product. Simply add the [SocketMeister.Sources](https://www.nuget.org/packages/SocketMeister.Sources/) NuGet package to your Visual Studio project and a folder containing the code will be added to the project.

## Silverlight no longer supported

The [SocketMeister.Silverlight]() NuGet package is no longer maintained and doesn't work with SocketMeister 10 or above.

## SocketMeister Version 10
SocketMeister Version 10 is a significant upgrade on earlier versions (1.x.x, 2.x.x, 3.x.x and 4.x.x). Although the public interface hasn't changed, previous versions of SocketMeister do not work with Version 10. If you upgrade to version 10 you will need to upgrade and rollout all software using SocketClient and SocketServer. Older clients will fail to connect to SocketServer 10.

SocketMeister 10 introduces robust handshaking and version awareness between clients and servers. Thich will allow for future functionality to be introduced without breaking backward compatibility.

## Simple Example

The following example shows how easy it is to setup a basic server and client. It achieves the following:

1. Start a socket server application and socket client application.
2. Send a request from the client to the server when a button is clicked.
3. Process the request on the server and return a response to the client.
4. Display the response on the client.

### Simple SocketServer Application.

Making a start with SocketServer requires only a few lines of code. The following will start the socket server on port 4505 and process an example request from clients. 

```
using System;
using System.Windows.Forms;
using SocketMeister;

namespace SocketMeisterDemo
{
    public partial class SimpleServer : Form
    {
        //  Instatiate SocketServer on port 4505 with compression enabled
        private readonly SocketServer _socketServer = new SocketServer(4505, true);

        public SimpleServer()
        {
            InitializeComponent();

            //  Listen to RequestReceived events
            _socketServer.RequestReceived += SocketServer_RequestReceived;

            //  Start the socket server
            _socketServer.Start();
        }
		private void SocketServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
		{
			//  Example request
			if (Convert.ToString(e.Parameters[0]) == "GetTimezoneDisplayName")
			{
				//  Response is a binary array. Convert string to binary array.
				e.Response = System.Text.Encoding.ASCII.GetBytes(TimeZoneInfo.Local.DisplayName);
			}
		}

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
			//  Stop the socket server before exiting the application
			_socketServer.Stop();
		}
	}
}

```

### Simple SocketClient Application.

The following code is from a WinForm containing one button called BtnGetTimezoneDisplayName. It automatically connects to the SocketServer running on the local machine. Pressing the button sends a request to the server, then displays a string response. 

```
using System;
using System.Windows.Forms;
using SocketMeister;

namespace SocketMeisterDemo
{
    public partial class SimpleClient : Form
    {
        //  Start socket client, pointed at 127.0.0.1:4505 with compression enabled (127.0.0.1 is localhost)
        SocketClient _socketClient = new SocketClient("127.0.0.1", 4505, true);

        public SimpleClient()
        {
            InitializeComponent();
        }

        private void BtnGetTimezoneDisplayName_Click(object sender, EventArgs e)
        {
            object[] parms = new object[] { "GetTimezoneDisplayName" };
            byte[] response =_socketClient.SendRequest(parms);

            //  Response is a binary array. Convert it to a string and display the message.
            MessageBox.Show(System.Text.Encoding.ASCII.GetString(response));
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //  Disconnect from the socket server
            _socketClient.Stop();
        }
    }
}
```

### Broadcasting a message from the server to all clients

The `SocketServer.BroadcastMessage()` method is used to send a message to all connected clients. In this example we are sending a string, but in more complex situations you could send many parameters. Supported data types for each parameter includes strings, most numeric data types and byte arrays.

On the server application we add the following code to a button click event. This sends a string to all clients:

        private void btnSendMsgToAllClients_Click(object sender, EventArgs e)
        {
            //  Send a string to all connected clients
            object[] parms = new object[] { "This is a broadcast message to all clients" };
            _socketServer.BroadcastMessage(parms);
        }

On the client application we must register to receive `SocketClient.MessageReceived` events and some code to display the message:

        public SimpleClient()
        {
            InitializeComponent();
    
            //  Register to receive messages from the server.
            _socketClient.MessageReceived += SocketClient_MessageReceived;
        }
    
        private void SocketClient_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            //  The message is a string contained in the first parameter sent. Display it.
            MessageBox.Show(Convert.ToString(e.Parameters[0]));
        }

### Broadcasting a message from the server to subscribers.

SocketMeister clients can register and unregister to receive specific message types from the server. *Subscriptions* are simply a list of names maintained by the SocketClient, which are automatically synchronised with the SocketServer. As subscription names are added and removed on the SocketClient, these are immediately reflected to the server. If a TCP connection is broken, as soon as the client reconnects, or connects with another server, it automatically sends the list during the connection process. This provides powerful functionality which you can leverage in your applications. Perhaps your server has to deal with different client applications or you want to turn certain message types on or off depending on what is happening at the client.

On the client side, we will add the subscription "Customer Master Data Changes". Let's extend the example from above

        public SimpleClient()
        {
            InitializeComponent();
    
            //  Register to receive messages from the server.
            _socketClient.MessageReceived += SocketClient_MessageReceived;
    
            //  Subscribe to 'Customer Master Data Changes'. (Note: names are case insensitive)
            _socketClient.AddSubscription("Customer Master Data Changes");
        }

On the server example, we have added a button labelled *"Send Msg to All 'Customer Master Data Changes' Subscribers"*

        private void btnSendMsgToAllCustomerDataChangeSubscribers_Click(object sender, EventArgs e)
        {
            //  Send a string to all connected clients
            object[] parms = new object[] { "This is a broadcast message to all clients subscribing to 'Customer Master Data Changes'" };
            _socketServer.BroadcastMessageToSubscribers("Customer Master Data Changes", parms);
        }

We already have coded the client to display messages when receiving the event `SocketClient_MessageReceived`. 

If we wanted to remove the subscription, at the socket client we would add the following line of code.

```
            _socketClient.RemoveSubscription("Customer Master Data Changes");
```

### Messaging a specific client from the server

From the server you may want to send messages and requests to specific clients.



