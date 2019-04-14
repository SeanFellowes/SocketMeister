using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Server
{
    public partial class FormMain : Form
    {
        private readonly BindingList<SocketMeister.LogEntry> _gridItems;

        public FormMain()
        {
            InitializeComponent();


            ControlServer.TraceEventRaised += TraceEventRaised;
            ControlPolicyServer.TraceEventRaised += TraceEventRaised;
            TestServer1.TraceEventRaised += TraceEventRaised;
            TestServer2.TraceEventRaised += TraceEventRaised;
            TestServer3.TraceEventRaised += TraceEventRaised;
            TestServer4.TraceEventRaised += TraceEventRaised;

            dGrid.AutoGenerateColumns = true;
            _gridItems = new BindingList<SocketMeister.LogEntry>
            {
                AllowNew = true,
                AllowRemove = true,
                RaiseListChangedEvents = true,
                AllowEdit = false
            };
            dGrid.DataSource = _gridItems;

            //  START CONTROL ITEMS
            ControlServer.Start();
            ControlPolicyServer.Start();
        }

        private void TraceEventRaised(object sender, SocketMeister.TraceEventArgs e)
        {
            Test.Server.Server server = (Server)sender;
            InsertListboxItem(server.Port.ToString(), e);
        }


        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            this.Height = 900;
            this.Width = 1500;

        }

        private void InsertListboxItem(string source, SocketMeister.TraceEventArgs args)
        {
            if (dGrid.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { InsertListboxItem(source, args); }));
            }
            else
            {
                SocketMeister.LogEntry logEntry = new SocketMeister.LogEntry(source, args.Message, args.Severity, args.EventId);
                if (_gridItems.Count == 0) _gridItems.Add(logEntry);
                else _gridItems.Insert(0, logEntry);
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ControlServer.Status == SocketMeister.ServiceStatus.Started) ControlServer.Stop();
            if (ControlPolicyServer.Status == SocketMeister.ServiceStatus.Started) ControlPolicyServer.Stop();
            Application.Exit();

        }
    }
}
