using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
#if TESTHARNESS
#endif

namespace SocketMeister.Testing.Tests
{
    internal partial class Test001Harness : TestOnHarnessBase, ITestOnHarness
    {
        private readonly HarnessController _harnessController;
        public Test001Harness(HarnessController HarnessController) : base (HarnessController, Test001Base.Id, Test001Base.Description)  
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
                _harnessController.FixedServer1.Commands.StartSocketServer(2345);

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


    }
}
