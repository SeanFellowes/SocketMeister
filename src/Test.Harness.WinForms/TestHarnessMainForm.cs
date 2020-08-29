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

namespace SocketMeister.Test
{
    public partial class TestHarnessMainForm : Form
    {
        private const int rowHeight = 18;
        private const int executeButtonWidth = 90;
        private const int spacer = 0;

        private readonly HarnessController _harnessController;
        private int currentTestPtr = 0;
        private int errors = 0;
        private readonly BindingList<LogEntry> gridItems;
        private readonly List<Label> lCol1 = new List<Label>();
        private readonly List<Label> lCol2 = new List<Label>();
        private readonly List<Label> lCol3 = new List<Label>();
        private readonly List<Label> lCol4 = new List<Label>();
        private readonly List<Button> lCol5 = new List<Button>();

        public TestHarnessMainForm()
        {
            InitializeComponent();

            _harnessController = new HarnessController();
            _harnessController.PolicyServer.ListenerStateChanged += PolicyServer_ListenerStateChanged;
            _harnessController.FixedServer1.ListenerStateChanged += FixedServer1_ListenerStateChanged;

            _harnessController.ControlBusServer.ListenerStateChanged += ControlBusServer_ListenerStateChanged;
            _harnessController.ControlBusServer.ClientsChanged += ControlBusServer_ClientsChanged;

            //  REGISTER FOR EVENTS FROM TESTS
            foreach (ITestOnHarness test in _harnessController.Tests)
            {
                test.PercentCompleteChanged += Test_PercentCompleteChanged;
                test.StatusChanged += Test_TestStatusChanged;
                test.TraceEventRaised += Test_TraceEventRaised;
            }

            dGrid.AutoGenerateColumns = false;
            gridItems = new BindingList<LogEntry>
            {
                AllowNew = true,
                AllowRemove = true,
                RaiseListChangedEvents = true,
                AllowEdit = false
            };
            dGrid.DataSource = gridItems;

            lblTests.Text = "Tests: " + _harnessController.Tests.Count;

            Setup();

            //  START
            _harnessController.Start();
        }

        private void ControlBusServer_ClientsChanged(object sender, SocketServer.ClientsChangedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { ControlBusServer_ClientsChanged(this, e); }));
            else
            {
                ControlServer.ClientCount = e.Count;
            }
        }

        private void ControlBusServer_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { ControlBusServer_ListenerStateChanged(this, e); }));
            else
            {
                ControlServer.SetListenerState(e.Status);
            }
        }

        private void FixedServer1_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PolicyServer_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { PolicyServer_ListenerStateChanged(sender, e); }));
            else
            {
                ControlPolicyServer.SetListenerState(e.Status);
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Top = 0;
            this.Left = 0;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;

            if (Screen.PrimaryScreen.WorkingArea.Width > 5000)
                this.Width = Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Width * .33);
            else
                this.Width = Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Width * .5);

            //testHarness.Initialize();
        }



        private void Test_PercentCompleteChanged(object sender, TestPercentCompleteChangedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { Test_PercentCompleteChanged(sender, e); }));
            else
            {
                ITestOnHarness test = (ITestOnHarness)sender;
                int testIndex = _harnessController.Tests.IndexOf(test);
                if (testIndex < 0) return;
                lCol4[testIndex].Text = e.PercentComplete.ToString() + " %";
            }
        }

        private void Test_TestStatusChanged(object sender, HarnessTestStatusChangedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { Test_TestStatusChanged(sender, e); }));
            else
            {
                ITestOnHarness test = (ITestOnHarness)sender;
                int testIndex = _harnessController.Tests.IndexOf(test);
                if (testIndex < 0) return;

                bool allTestsComplete = false;
                bool executeThisTestEnabled = false;
                bool executeNextTest = false;
                Color statusColor = Color.Black;
                string statusText = "";

                if (e.Status == TestStatus.Failed)
                {
                    statusColor = Color.DarkRed;
                    statusText = "Failed";
                    executeThisTestEnabled = true;
                    if (_harnessController.ExecuteMode == Executing.AllTests) executeNextTest = true;
                    else if (_harnessController.ExecuteMode == Executing.StoppingAllTests) allTestsComplete = true;
                }
                else if (e.Status == TestStatus.Stopped)
                {
                    statusColor = Color.DarkRed;
                    statusText = "Stopped";
                    executeThisTestEnabled = true;
                    if (_harnessController.ExecuteMode == Executing.AllTests) executeNextTest = true;
                    else if (_harnessController.ExecuteMode == Executing.StoppingAllTests) allTestsComplete = true;
                }
                else if (e.Status == TestStatus.InProgress)
                {
                    statusColor = Color.DarkGreen;
                    statusText = "In Progress";
                    executeThisTestEnabled = false;
                }
                else if (e.Status == TestStatus.Stopping)
                {
                    statusColor = Color.DarkOrange;
                    statusText = "Stopping";
                    executeThisTestEnabled = false;
                }
                else if (e.Status == TestStatus.NotStarted)
                {
                    statusColor = Color.White;
                    statusText = "Not Started";
                    executeThisTestEnabled = true;
                    if (_harnessController.ExecuteMode == Executing.AllTests) executeNextTest = true;
                    else if (_harnessController.ExecuteMode == Executing.StoppingAllTests) allTestsComplete = true;
                }
                else if (e.Status == TestStatus.Successful)
                {
                    statusColor = Color.DarkSlateBlue;
                    statusText = "Successful";
                    executeThisTestEnabled = true;
                    if (_harnessController.ExecuteMode == Executing.AllTests) executeNextTest = true;
                    else if (_harnessController.ExecuteMode == Executing.StoppingAllTests) allTestsComplete = true;
                }
                else
                {
                    throw new Exception("Unknown TestStatus " + e.Status.ToString());
                }


                for (int index = 0; index < _harnessController.Tests.Count; index++)
                {

                    if (index == testIndex)
                    {
                        lCol1[index].BackColor = Color.SlateGray;
                        lCol1[index].ForeColor = Color.White;
                        lCol2[index].BackColor = Color.SlateGray;
                        lCol2[index].ForeColor = Color.White;

                        lCol3[index].ForeColor = Color.White;
                        lCol3[index].BackColor = statusColor;
                        lCol3[index].Text = statusText;

                        lCol4[index].BackColor = Color.SlateGray;
                        lCol4[index].ForeColor = Color.White;

                        if (e.Status == TestStatus.InProgress)
                        {
                            lCol5[index].Enabled = true;
                            lCol5[index].Text = "Stop";
                        }
                        else
                        {
                            lCol5[index].Enabled = executeThisTestEnabled;
                            lCol5[index].Text = "Execute";
                        }
                    }
                    else
                    {
                        lCol1[index].BackColor = Color.White;
                        lCol1[index].ForeColor = Color.Black;
                        lCol2[index].BackColor = Color.White;
                        lCol2[index].ForeColor = Color.Black;
                        lCol4[index].BackColor = Color.White;
                        lCol4[index].ForeColor = Color.Black;

                        lCol5[index].Enabled = executeThisTestEnabled;
                    }
                }

                //  RUN THE NEXT TEST IF APPLICABLE
                if (executeNextTest == true)
                {
                    currentTestPtr++;
                    if (currentTestPtr < _harnessController.Tests.Count)
                    {
                        ITestOnHarness nextTest = (ITestOnHarness)lCol5[currentTestPtr].Tag;
                        nextTest.Start();
                    }
                    else
                    {
                        allTestsComplete = true;
                    }
                }
                
                if (allTestsComplete == true)
                {
                    _harnessController.ExecuteMode = Executing.Stopped;
                    for (int index = 0; index < _harnessController.Tests.Count; index++)
                    {
                        lCol5[index].Visible = true;
                    }
                    btnExecuteAllTests.Text = "Execute All Tests";
                    btnExecuteAllTests.Enabled = true;
                }
            }
        }

        //private void ControlServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        //{
        //    int r = Convert.ToInt32(e.Parameters[0]);
        //    if (r == ControlBus.ControlMessage.HarnessControlBusClientIsConnecting)
        //    {
        //        int ClientId = Convert.ToInt32(e.Parameters[1]);
        //        if (ClientId == int.MaxValue)
        //        {
        //            //  FIXED CLIENT HAS PHONED HOME
        //            _harnessController.FixedHarnessClient.ListenerClient = e.Client;
        //        }
        //        else
        //        {
        //            //  ANOTHER CLIENT HAS PHONED HOME. FIND THE CLIENT
        //            HarnessClientController client = _harnessController.TestClientCollection[ClientId];
        //            if (client != null)
        //            {
        //                //  ASSIGN THE SocketMeister Server Client to the class. When connecting a test harness client, this value is checked for NOT null (Connected).
        //                client.ListenerClient = e.Client;
        //            }
        //        }
        //    }
        //}

        private void ControlServer_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            string ggg = "ertretre";
        }

        private void TraceEventRaised(object sender, TraceEventArgs e)
        {
            Type senderType = sender.GetType();
            if (senderType == typeof(SocketMeister.Test.SocketServerOverview))
            {
                SocketMeister.Test.SocketServerOverview server = (SocketServerOverview)sender;
                InsertListboxItem(server.Port.ToString(), e);
            }
            else if(senderType == typeof(PolicyServer))
            {
                SocketMeister.PolicyServer ps = (PolicyServer)sender;
                InsertListboxItem(ps.Port.ToString(), e);
            }
            else
            {
                throw new ApplicationException("Unknown type of sender");
            }
        }

        private void Test_TraceEventRaised(object sender, TraceEventArgs e)
        {
            ITestOnHarness test = (ITestOnHarness)sender;
            InsertListboxItem("Test " + test.Id.ToString(), e);
        }



        private void InsertListboxItem(string source, TraceEventArgs args)
        {
            if (dGrid.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { InsertListboxItem(source, args); }));
            }
            else
            {
                LogEntry logEntry = new LogEntry(source, args.Message, args.Severity, args.EventId, args.StackTrace);
                //while (gridItems.Count > 1000)
                //{
                //    gridItems.RemoveAt(gridItems.Count - 1);
                //}
                if (gridItems.Count == 0) gridItems.Add(logEntry);
                else gridItems.Insert(0, logEntry);

                lblTraceLog.Text = "Trace Log:" + gridItems.Count;
                lblTraceLog.Refresh();

                if (args.Severity == SeverityType.Error)
                {
                    errors++;
                    lblErrors.Text = "Errors: " + errors.ToString(); ;
                    lblErrors.ForeColor = Color.White;
                    lblErrors.BackColor = Color.DarkRed;
                }

            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //  UNREGISTER 
            _harnessController.PolicyServer.ListenerStateChanged -= PolicyServer_ListenerStateChanged;
            _harnessController.FixedServer1.ListenerStateChanged -= FixedServer1_ListenerStateChanged;

            _harnessController.ControlBusServer.ListenerStateChanged -= ControlBusServer_ListenerStateChanged;
            _harnessController.ControlBusServer.ClientsChanged -= ControlBusServer_ClientsChanged;

            _harnessController.Dispose();
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

            foreach (ITestOnHarness test in _harnessController.Tests)
            {
                //  COL 1 (ID)
                Label lb = new Label
                {
                    Text = test.Id.ToString(),

                    TextAlign = ContentAlignment.MiddleRight,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = font
                };
                lCol1.Add(lb);
                pnlTests.Controls.Add(lb);

                //  COL 2 (Description)
                lb = new Label
                {
                    Text = test.Description,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = font
                };
                lCol2.Add(lb);
                pnlTests.Controls.Add(lb);

                //  COL 3 (Status)
                lb = new Label
                {
                    Text = "",
                    TextAlign = ContentAlignment.MiddleLeft,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = font,
                    Anchor = AnchorStyles.Top
                };

                lCol3.Add(lb);
                pnlTests.Controls.Add(lb);

                //  COL 4 (Percent COmplete)
                lb = new Label
                {
                    Text = "",
                    TextAlign = ContentAlignment.MiddleRight,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = font
                };
                lCol4.Add(lb);
                pnlTests.Controls.Add(lb);

                //  COL 5 (BUTTON)
                Button bt = new Button
                {
                    Text = "Execute",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = font,
                    Width = 100,
                    Height = rowHeight,
                    Tag = test
                };
                lCol5.Add(bt);
                pnlTests.Controls.Add(bt);
                bt.Click += ExecuteButton_Click;

            }

            Repos();
        }

        private void ExecuteButton_Click(object sender, EventArgs e)
        {
            new Thread(
                new ThreadStart(delegate
                {
                    ITestOnHarness test = (ITestOnHarness)((Button)sender).Tag;

                    if (_harnessController.ExecuteMode == Executing.AllTests)
                    {
                        test.Start();
                    }
                    else if (_harnessController.ExecuteMode == Executing.Stopped)
                    {
                        test.Start();
                    }
                    else if (_harnessController.ExecuteMode == Executing.SingleTest)
                    {
                        test.Stop();
                    }
                })).Start();
        }

        private void SetTestNotification(ITestOnHarness Test, TestStatus Value)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetTestNotification(Test, Value); }));
            else
            {
                int index = _harnessController.Tests.IndexOf(Test);
                if (index < 0) return;
                if (Value == TestStatus.Failed)
                {
                    (lCol3[index]).BackColor = Color.DarkRed;
                    (lCol3[index]).Text = "Failed";
                }
                else if (Value == TestStatus.InProgress)
                {
                    (lCol3[index]).BackColor = Color.DarkGreen;
                    (lCol3[index]).Text = "In Progress";
                }
                else if (Value == TestStatus.NotStarted)
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

        private void PnlTests_Resize(object sender, EventArgs e)
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

        private void BtnExecuteAllTests_Click(object sender, EventArgs e)
        {
            if (_harnessController.ExecuteMode == Executing.Stopped)
            {
                foreach (ITestOnHarness test in _harnessController.Tests)
                {
                    test.Reset();
                }
                for (int index = 0; index < _harnessController.Tests.Count; index++)
                {
                    lCol5[index].Visible = false;
                }

                _harnessController.ExecuteMode = Executing.AllTests;
                currentTestPtr = 0;
                btnExecuteAllTests.Text = "Stop";
                ExecuteButton_Click(lCol5[currentTestPtr], new EventArgs());
            }
            else if (_harnessController.ExecuteMode == Executing.AllTests)
            {
                _harnessController.ExecuteMode = Executing.StoppingAllTests;
                if (_harnessController.CurrentTest != null) _harnessController.CurrentTest.Stop();
            }
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            gridItems.Clear();
            errors = 0;
            lblTraceLog.Text = "Trace Log: 0";
            lblErrors.Text = "Errors: 0";
            lblErrors.ForeColor = Color.Black;
            lblErrors.BackColor = Color.Transparent;
        }

        private void DGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {
                LogEntry li = gridItems[e.RowIndex];
                if (li.StackTrace == null) MessageBox.Show("No stack trace", "No stack trace", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else MessageBox.Show(li.StackTrace, "Stack trace", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
