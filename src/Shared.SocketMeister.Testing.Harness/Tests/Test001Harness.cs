using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#if TESTHARNESS
#endif

namespace SocketMeister.Testing.Tests
{
    internal partial class Test001Harness : TestOnHarnessBase, ITestOnHarness
    {
        private readonly HarnessController _harnessController;
        public Test001Harness(HarnessController HarnessController) : base(HarnessController, Test001Base.Id, Test001Base.Description)
        {
            _harnessController = HarnessController;
            base.Parent = this;
            base.ExecuteTest += Test001Harness_ExecuteTest;
        }

        private void Test001Harness_ExecuteTest(object sender, EventArgs e)
        {
            try
            {
                //  START THE SOCKET SERVER ON FixedServer1 
                RaiseTraceEventRaised("Starting SocketServer on port " + Constants.HarnessFixedServer1Port, SeverityType.Information, 1);
                _harnessController.FixedServer1.Commands.SocketServerStart(Constants.HarnessFixedServer1Port);

                //  CONNECT FixedClient1 TO FixedServer1
                RaiseTraceEventRaised("Starting SocketClient on port " + Constants.HarnessFixedServer1Port, SeverityType.Information, 1);
                List<SocketEndPoint> endPoints = new List<SocketEndPoint>
                {
                    new SocketEndPoint("127.0.0.1", Constants.HarnessFixedServer1Port)
                };
                _harnessController.FixedClient1.Commands.SocketClientStart(endPoints, false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Test001Step001();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed


                //ClientController ClientId01 = base.AddClient();

                //for (int r = 0; r < 20; r++)
                //{
                //    PercentComplete = Convert.ToInt32(((r + 1.0) / 20) * 100.0);
                //    RaiseTraceEventRaised("Loop " + r.ToString(), SeverityType.Information, 1);

                //    //  IS THIS TO BE STOPPED?
                //    if (Status == TestStatus.Stopping)
                //    {
                //        Status = TestStatus.Stopped;
                //        RaiseTraceEventRaised("Test was stopped before completing", SeverityType.Information, 1);
                //        return;
                //    }

                //    Thread.Sleep(1000);
                //}

                //  TEST THROW EXCEPTION (SHOULD APPEAR ON SCREEN)
                //throw new FieldAccessException("Bad things hewflkm welfkm ewlkfm welfkmlm Error regfergergregreg erg reg reg re greg re greg re gtsrh yrthjtyfj tyj jy tyfju ytfj ytj ytj tydj tydj dtyjdcfth dfyjcgjy cfyj cgjy ycjvgukgyukyfutkjg fyuk ftyj fyuk ftyjt dyh t");

                DateTime end = DateTime.Now.AddSeconds(5);
                while (DateTime.Now < end)
                {
                    //    //  IS THIS TO BE STOPPED?
                    if (Status == TestStatus.Stopping)
                    {
                        Status = TestStatus.Stopped;
                        RaiseTraceEventRaised("Test was stopped before completing", SeverityType.Information, 1);
                        return;
                    }
                    Thread.Sleep(1000);
                }


                //  STOP THE FIXED CLIENT
                RaiseTraceEventRaised("Stopping SocketClient", SeverityType.Information, 1);
                _harnessController.FixedClient1.Commands.SocketClientStop();


                //  STOP THE SOCKET SERVER ON FixedServer1 
                RaiseTraceEventRaised("Stopping SocketServer", SeverityType.Information, 1);
                _harnessController.FixedServer1.Commands.SocketServerStop();


                Status = TestStatus.Successful;
                RaiseTraceEventRaised("Test completed successfully", SeverityType.Information, 1);

            }
            catch (Exception ex)
            {
                Status = TestStatus.Failed;
                RaiseTraceEventRaised(ex, 1);
            }
            finally
            {
                //  CLEANUP
                //  SEAN SEAN SEAN - ADD THIS (OR SOMETHING SIMILAR) BACK IN
                //base.TestHarness.Clients.DisconnectClients();
            }
        }


        internal async Task Test001Step001()
        {
            Task task = HarnessControllerCommands.ClientToServerSendRequestEcho01(_harnessController.FixedServer1, _harnessController.FixedClient1, 1024, 10000);
            await task;

            //  SEND A 1KB FILE
            //byte[] rVal = _harnessController.FixedClient1.Commands.ExecuteMethod("Test001Client", "Test001Step001");

        }


    }
}
