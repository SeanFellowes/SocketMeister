using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Client.WinForms
{
    public partial class Form1 : Form
    {
        SocketMeister.TestHarnessClient harness = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = "SocketMeister Test Client (" + Program.ClientId.ToString() + ")";
                Guid guid = Guid.NewGuid();
                harness = new SocketMeister.TestHarnessClient(Program.ClientId);
                harness.ControlConnectionFailed += Harness_ControlConnectionFailed;
            }
            catch
            {
                this.Close();
                Application.Exit();
            }

        }

        private void Harness_ControlConnectionFailed(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate {
                this.Close();
            });
            System.Threading.Thread.Sleep(500);
            Environment.Exit(2);
        }
    }
}
