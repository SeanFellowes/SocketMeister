using SocketMeister.Testing.ControlBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Commands executed on the server, via the ControlBus 
    /// </summary>
    internal static class HarnessControllerCommands
    {
        private readonly static object _lock = new object();
        private static int _lastMessageId = 0;

        internal static async Task ClientToServerSendRequestEcho01(HarnessServerController ServerController, HarnessClientController ClientController, int MessageLength, int TimeoutMilliseconds = 60000)
        {
            int messageId = GetNextMessageId();
            Task tasks = null;
            try
            {
                Task task1 = Task.Run(() =>
                {
                    object[] p = new object[3];
                    p[0] = messageId;
                    p[1] = MessageLength;
                    p[2] = TimeoutMilliseconds;
                    byte[] rVal = ServerController.Commands.ExecuteCommand(nameof(ServerControllerCommands), nameof(ServerControllerCommands.ClientToServerSendRequestEcho01), p);
                });
                Task task2 = Task.Run(() =>
                {
                    object[] p = new object[3];
                    p[0] = messageId;
                    p[1] = MessageLength;
                    p[2] = TimeoutMilliseconds;
                    byte[] rVal = ClientController.Commands.ExecuteCommand(nameof(ClientControllerCommands), nameof(ClientControllerCommands.ClientToServerSendRequestEcho01), p);
                    Thread.Sleep(30000);
                });
                tasks = Task.WhenAll(task1, task2);
                await tasks;
            }
            catch
            {
                AggregateException aggregateException = tasks.Exception;
                foreach (Exception e in aggregateException.InnerExceptions)
                {
                    Console.WriteLine(e.GetType().ToString());
                }
            }




            //int messageId = GetNextMessageId();

            ////  START SOCKET SERVER IN BACHGROUND
            //Thread bgServer = new Thread(new ThreadStart(delegate
            //{
            //    object[] p = new object[3];
            //    p[0] = messageId;
            //    p[1] = MessageLength;
            //    p[2] = TimeoutMilliseconds;
            //    byte[] rVal = Client.Commands.ExecuteMethod(nameof(ServerControllerCommands), nameof(ServerControllerCommands.ClientToServerSendRequestEcho01), p);
            //}));
            //bgServer.IsBackground = true;
            //bgServer.Start();

            //Thread bgClient = new Thread(new ThreadStart(delegate
            //{
            //    byte[] rVal = Client.Commands.ExecuteMethod(nameof(ClientControllerCommands), nameof(ClientControllerCommands.ClientToServerSendRequestEcho01));
            //}));
            //bgClient.IsBackground = true;
            //bgClient.Start();


            //ThreadPool.QueueUserWorkItem(state =>
            //{
            //    try
            //    {
            //        action();
            //    }
            //    catch (Exception ex)
            //    {
            //        OnException(ex);
            //    }
            //});

            //  SEND A 1KB FILE
        }



        private static int GetNextMessageId()
        {
            lock (_lock)
            {
                _lastMessageId++;
                return _lastMessageId;
            }
        }

    }
}
