using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    internal class Test001_Connect : TestBase, ITest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Test001_Connect() : base (1, "Connects blah blah blah")
        {
            base.Parent = this;
            base.ExecuteTest += Execute;
        }

        private void Execute(object sender, EventArgs e)
        {
            try
            {
                for (int r = 0; r < 20; r++)
                {
                    PercentComplete = Convert.ToInt32(((r + 1.0) / 20) * 100.0);
                    RaiseTraceEventRaised("All gooewfon ewoifm oweiAll gooewfon ewoifm oweimf weofim ewofim  we ewf wef wef " + r.ToString(), SeverityType.Information, 1);

                    //  IS THIS TO BE STOPPED?
                    if (Status == TestStatus.Stopping)
                    {
                        Status = TestStatus.Stopped;
                        RaiseTraceEventRaised("Test was stopped before completing", SeverityType.Information, 1);
                        return;
                    }
                }

                Status = TestStatus.Successful;
                RaiseTraceEventRaised("Test completed successfully", SeverityType.Information, 1);

            }
            catch (Exception ex)
            {
                Status = TestStatus.Failed;
                RaiseTraceEventRaised(ex, 1);
            }
        }

    }
}
