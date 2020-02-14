using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketMeister.Test
{
    public partial class FormMain : Form
    {
        private readonly BindingList<LogEntry> gridItems;

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
            gridItems = new BindingList<LogEntry>
            {
                AllowNew = true,
                AllowRemove = true,
                RaiseListChangedEvents = true,
                AllowEdit = false
            };
            dGrid.DataSource = gridItems;

            //  START CONTROL ITEMS
            ControlServer.Start();
            ControlPolicyServer.Start();
        }

        private void TraceEventRaised(object sender, SocketMeister.TraceEventArgs e)
        {
            SocketMeister.Test.Server server = (Server)sender;
            InsertListboxItem(server.Port.ToString(), e);
        }


        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Width = Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Width * .5);

        }

        private void InsertListboxItem(string source, SocketMeister.TraceEventArgs args)
        {
            if (dGrid.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { InsertListboxItem(source, args); }));
            }
            else
            {
                LogEntry logEntry = new LogEntry(source, args.Message, args.Severity, args.EventId);
                if (gridItems.Count == 0) gridItems.Add(logEntry);
                else gridItems.Insert(0, logEntry);
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ControlServer.UnregisterEvents();
            ControlPolicyServer.UnregisterEvents();
            ControlServer.Stop();
            ControlPolicyServer.Stop();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void PanelMain_Resize(object sender, EventArgs e)
        {
            PanelMainTrace.Top = 0;
            PanelMainTrace.Left = 0;
            PanelMainTrace.Height = Convert.ToInt32( PanelMain.Height * 0.3);
            PanelMainTrace.Width = PanelMain.Width;
            PanelMainTests.Left = 0;
            PanelMainTests.Top = PanelMainTrace.Height - 1;
            PanelMainTests.Height = PanelMain.Height - PanelMainTests.Top;
            PanelMainTests.Width = PanelMain.Width;
        }

    }
}
