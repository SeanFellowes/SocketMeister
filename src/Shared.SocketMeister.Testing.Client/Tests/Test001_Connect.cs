using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
#if TESTHARNESS
using System.Net.Http.Headers;
#endif

namespace SocketMeister.Testing.Tests
{
    internal partial class Test001 : TestBase, ITest
    {
        private const string TestDescription = "1 Client, Connect, Valid Operations, Disconnect";

#if TESTHARNESS
        public Test001(TestHarness TestHarness, int Id) : base(TestHarness, Id, TestDescription)
        {
            base.Parent = this;
            base.ExecuteTest += Execute;
        }
#elif TESTCLIENT
        public Test001(int Id) : base(Id, TestDescription) 
        {
            base.Parent = this;   
        }
#endif


#if TESTHARNESS
        private void Execute(object sender, EventArgs e)
        {
            try
            {
                Client ClientId01 = base.TestHarness.Clients.AddClient();

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
                base.TestHarness.Clients.DisconnectClients();
            }
        }
#endif


    }
}
