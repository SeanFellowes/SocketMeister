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

        private readonly AllTests allTests = new AllTests();
        private const int rowHeight = 18;
        private const int executeButtonWidth = 90;
        private const int spacer = 0;

        private readonly List<Label> lCol1 = new List<Label>();
        private readonly List<Label> lCol2 = new List<Label>();
        private readonly List<Label> lCol3 = new List<Label>();
        private readonly List<Label> lCol4 = new List<Label>();
        private readonly List<Button> lCol5 = new List<Button>();


        public FormMain()
        {
            InitializeComponent();

            ControlServer.TraceEventRaised += TraceEventRaised;
            ControlPolicyServer.TraceEventRaised += TraceEventRaised;
            ControlServer.MessageReceived += ControlServer_MessageReceived;
            ControlServer.RequestReceived += ControlServer_RequestReceived;
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

            Setup();
        }

        private void ControlServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            int r = Convert.ToInt32(e.Parameters[0]);
            if (r == Testing.ControlMessage.ClientConnected)
            {

            }
        }

        private void ControlServer_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
        }

        private void TraceEventRaised(object sender, TraceEventArgs e)
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
                while (gridItems.Count > 1000)
                {
                    gridItems.RemoveAt(gridItems.Count - 1);
                }
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


        private void Setup()
        {
            Font font = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular, GraphicsUnit.Pixel);

            //  CLEAR EXISTING ITEMS
            foreach (Control ctl in lCol1) { pnlTests.Controls.Remove(ctl); }
            foreach (Control ctl in lCol2) { pnlTests.Controls.Remove(ctl); }
            foreach (Control ctl in lCol3) { pnlTests.Controls.Remove(ctl); }
            foreach (Control ctl in lCol4) { pnlTests.Controls.Remove(ctl); }
            lCol1.Clear();
            lCol2.Clear();
            lCol3.Clear();
            lCol4.Clear();

            CH1.Top = 0;
            CH2.Top = 0;
            CH3.Top = 0;
            CH4.Top = 0;

            foreach (ITest test in allTests)
            {
                //  COL 1
                Label lb = new Label();
                lb.Text = test.Id.ToString();

                lb.TextAlign = ContentAlignment.MiddleRight;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lb.Font = font;
                lCol1.Add(lb);
                pnlTests.Controls.Add(lb);

                //  COL 2
                lb = new Label();
                lb.Text = test.Description;
                lb.TextAlign = ContentAlignment.MiddleLeft;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lb.Font = font;
                lCol2.Add(lb);
                pnlTests.Controls.Add(lb);

                //  COL 3
                lb = new Label();
                lb.Text = "";
                lb.TextAlign = ContentAlignment.MiddleLeft;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lb.Font = font;
                lb.Anchor = AnchorStyles.Top;
              
                lCol3.Add(lb);
                pnlTests.Controls.Add(lb);

                //  COL 4
                lb = new Label();
                lb.Text = "";
                lb.TextAlign = ContentAlignment.MiddleLeft;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lb.Font = font;
                lCol4.Add(lb);
                pnlTests.Controls.Add(lb);

                //  COL 5 (BUTTON)
                Button bt = new Button();
                bt.Text = "Execute";
                bt.TextAlign = ContentAlignment.MiddleCenter;
                bt.Font = font;
                bt.Width = 100;
                bt.Height = rowHeight;
                bt.Tag = test;
                lCol5.Add(bt);
                pnlTests.Controls.Add(bt);

                //  SUBSCRIBE TO EVENTS
                test.TestNotification += Test_TestNotification;
            }

            Repos();
        }

        private void Test_TestNotification(object sender, TestNotificationEventArgs e)
        {
            ITest test = (ITest)sender;
            SetTestNotification(test, e);
        }


        private void SetTestNotification(ITest Test, TestNotificationEventArgs Value)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetTestNotification(Test, Value); }));
            else
            {
                int index = allTests.IndexOf(Test);
                if (index < 0) return;
                if (Value.Status == TestNotificationStatuses.Failed)
                {
                    (lCol3[index]).BackColor = Color.DarkRed;
                    (lCol3[index]).Text = "Failed";
                }
                else if (Value.Status == TestNotificationStatuses.InProgress)
                {
                    (lCol3[index]).BackColor = Color.DarkGreen;
                    (lCol3[index]).Text = "In Progress";
                }
                else if (Value.Status == TestNotificationStatuses.NoStarted)
                {
                    (lCol3[index]).BackColor = Color.White;
                    (lCol3[index]).Text = "Not Started";
                }
                else
                {
                    (lCol3[index]).BackColor = Color.DarkSlateBlue;
                    (lCol3[index]).Text = "Successful";
                }
            }
        }



        private void Repos()
        {
            try
            {
                int rows = lCol1.Count;

                //  WORK OUT TOTAL HEIGHT
                int totalHeight = CH1.Height + ((rowHeight + spacer) * lCol1.Count);

                for (int row = 0; row < rows; row++)
                {
                    int top = CH1.Top + CH1.Height + spacer;
                    if (row > 0) top = lCol1[row - 1].Top + lCol1[row - 1].Height + spacer;

                    //  COL 1
                    (lCol1[row]).Height = rowHeight;
                    (lCol1[row]).Top = top;
                    (lCol1[row]).Width = CH1.Width;
                    if (row == 0)
                    {
                        (lCol1[row]).Left = CH1.Left;
                    }
                    else
                    {
                        (lCol1[row]).Left = (lCol1[row - 1]).Left;
                    }

                    //  COL 2
                    (lCol2[row]).Height = rowHeight;
                    (lCol2[row]).Top = top;
                    (lCol2[row]).Width = CH2.Width;
                    if (row == 0)
                    {
                        (lCol2[row]).Left = CH2.Left;
                    }
                    else
                    {
                        (lCol2[row]).Left = (lCol2[row - 1]).Left;
                    }

                    //  COL 3
                    (lCol3[row]).Height = rowHeight;
                    (lCol3[row]).Top = top;
                    (lCol3[row]).Width = CH3.Width;
                    if (row == 0)
                    {
                        (lCol3[row]).Left = CH3.Left;
                    }
                    else
                    {
                        (lCol3[row]).Left = (lCol3[row - 1]).Left;
                    }

                    //  COL 4
                    (lCol4[row]).Height = rowHeight;
                    (lCol4[row]).Top = top;
                    (lCol4[row]).Width = CH4.Width;
                    if (row == 0)
                    {
                        (lCol4[row]).Left = CH4.Left;
                    }
                    else
                    {
                        (lCol4[row]).Left = (lCol4[row - 1]).Left;
                    }

                    //  COL 5
                    (lCol5[row]).Height = rowHeight + 2;
                    (lCol5[row]).Top = top - 1;
                    (lCol5[row]).Width = executeButtonWidth;
                    (lCol5[row]).Left = CH4.Left + CH4.Width + spacer;
                 }
            }
            catch { }
        }

        private void pnlTests_Resize(object sender, EventArgs e)
        {
            try
            {
                //  WORK OUT TOTAL HEIGHT
                int totalHeight = CH1.Height + ((rowHeight + spacer) * lCol1.Count);
                int descriptionWidth = pnlTests.Width - (CH1.Width + CH3.Width + CH4.Width + executeButtonWidth + (spacer * 5) + 10);
                if (descriptionWidth < 100) descriptionWidth = 100;
                else if (totalHeight > pnlTests.Height) descriptionWidth -= 15;

                CH1.Left = 0;
                CH2.Left = CH1.Left + CH1.Width + spacer;
                CH2.Width = descriptionWidth;
                CH3.Left = CH2.Left + CH2.Width + spacer;
                CH4.Left = CH3.Left + CH3.Width + spacer;

                Repos();
            }
            catch { }
        }
    }
}
