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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
        }
    }
}
