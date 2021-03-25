using System;
using System.Threading;
using System.Windows.Forms;

namespace Test.Client.WinForms
{
    public partial class TestClientMainForm : Form
    {
        private SocketMeister.Testing.ClientController client = null;

        public TestClientMainForm()
        {
            InitializeComponent();
        }

        private void TestClientMainForm_Load(object sender, EventArgs e)
        {
            try
            {
                Text = "SocketMeister Test Client (" + Program.ClientId.ToString() + ")";
                Guid guid = Guid.NewGuid();
                client = new SocketMeister.Testing.ClientController(Program.ClientId);
                client.ControlBusConnectionFailed += Harness_ControlConnectionFailed;
            }
            catch
            {
                Close();
                Application.Exit();
            }
        }

        private void Harness_ControlConnectionFailed(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                Close();
            });
            Thread.Sleep(500);
            Environment.Exit(2);
        }

    }
}
