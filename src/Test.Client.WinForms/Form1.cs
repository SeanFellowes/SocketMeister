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
        SocketMeister.TestClientHarness harness = null;
        public Form1()
        {
            InitializeComponent();


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Guid guid = Guid.NewGuid();
                harness = new SocketMeister.TestClientHarness(guid);
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
            Application.Exit();
        }
    }
}
