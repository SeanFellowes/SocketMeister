using System;
using System.Windows.Forms;

namespace Test.Client.WinForms
{
    internal static class Program
    {
        public static int ClientId;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
            //  IN DEBUG, SET THE CLIENT ID TO int.MaxValue. THE HARNESS WILL WAIT (A BIT) FOR THIS TO 
            //  CONNECT. IMPORTANT: THIS SETUP ALLOWS DEBUGGING OF BOTH THIS CLIENT AND THE HARNESS.
            ClientId = int.MaxValue;
            Application.Run(new TestClientMainForm());
#else
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                Environment.Exit(1);
            }
            else
            {
                if (Int32.TryParse(args[1], out ClientId) == false)
                {
                    Environment.Exit(3);
                }
                else
                {
                    Application.Run(new TestClientMainForm());
                }
            }
#endif
        }
    }
}
