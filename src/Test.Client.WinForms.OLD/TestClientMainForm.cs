using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketMeister;
using SocketMeister.Testing;

namespace Test.Client.WinForms
{
    public partial class Form1 : Form
    {
        private TestClient client = null;

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
                client = new SocketMeister.Testing.TestClient(Program.ClientId, Program.HarnessControllerIPAddress, Program.HarnessControllerPort);
                client.ControlConnectionFailed += Harness_ControlConnectionFailed;
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
            Thread.Sleep(500);
            Environment.Exit(2);
        }
    }
}
