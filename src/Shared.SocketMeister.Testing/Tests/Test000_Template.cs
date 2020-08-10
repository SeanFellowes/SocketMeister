using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
{
    internal class Test000 : TestBase, ITest
    {
        private const string TestDescription = "Template for other tests";

#if TESTHARNESS
        public Test000(TestHarness TestHarness, int Id) : base(TestHarness, Id, TestDescription)
        {
            base.Parent = this;
            base.ExecuteTest += Execute;
        }
#elif TESTCLIENT
        public Test000(int Id) : base(Id, TestDescription)
        {
            base.Parent = this;
        }
#endif


#if TESTHARNESS
        private void Execute(object sender, EventArgs e)
        {
            try
            {
                for (int r = 0; r < 5; r++)
                {
                    PercentComplete = Convert.ToInt32(((r + 1.0) / 20) * 100.0);
                    RaiseTraceEventRaised("Loop " + r.ToString(), SeverityType.Information, 1);

                    //  IS THIS TO BE STOPPED?
                    if (Status == TestStatus.Stopping)
                    {
                        Status = TestStatus.Stopped;
                        RaiseTraceEventRaised("Test was stopped before completing", SeverityType.Information, 1);
                        return;
                    }

                    Thread.Sleep(500);
                }

                //  TEST THROW EXCEPTION (SHOULD APPEAR ON SCREEN)
                //throw new FieldAccessException("Bad things hewflkm welfkm ewlkfm welfkmlm Error regfergergregreg erg reg reg re greg re greg re gtsrh yrthjtyfj tyj jy tyfju ytfj ytj ytj tydj tydj dtyjdcfth dfyjcgjy cfyj cgjy ycjvgukgyukyfutkjg fyuk ftyj fyuk ftyjt dyh t");

                Status = TestStatus.Successful;
                RaiseTraceEventRaised("Test completed successfully", SeverityType.Information, 1);

            }
            catch (Exception ex)
            {
                Status = TestStatus.Failed;
                RaiseTraceEventRaised(ex, 1);
            }
        }
#endif

    }
}
