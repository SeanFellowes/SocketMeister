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
using SocketMeister.Testing;

namespace Test.Client.WinForms
{
    public partial class TestClientMainForm : Form
    {
        private SocketMeister.Testing.Client.ClientController client = null;

        public TestClientMainForm()
        {
            InitializeComponent();
        }

        private void TestClientMainForm_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = "SocketMeister Test Client (" + Program.ClientId.ToString() + ")";
                Guid guid = Guid.NewGuid();
                client = new SocketMeister.Testing.Client.ClientController(Program.ClientId);
                client.ControlBusConnectionFailed += Harness_ControlConnectionFailed;
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
