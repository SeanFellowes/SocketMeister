using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Client.WinForms
{
    static class Program
    {
        static public int ClientId;
        static public string HarnessControllerIPAddress = "127.0.0.1";
        static public int HarnessControllerPort = 4505;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
            //  IN DEBUG, SET THE CLIENT ID TO int.MaxValue. THE HARNESS WILL WAIT (A BIT) FOR THIS TO 
            //  CONNECT. IMPORTANT: THIS SETUP ALLOWS DEBUGGING OF BOTH THIS CLIENT AND THE HARNESS.
            ClientId = int.MaxValue;
            Application.Run(new Form1());
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
                    Application.Run(new Form1());
                }
            }
#endif
        }
    }
}
