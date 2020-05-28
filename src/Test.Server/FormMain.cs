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

namespace SocketMeister.Test
{
    public partial class FormMain : Form
    {
        private enum Executing
        {
            Stopped = 0,
            SingleTest = 1,
            AllTests = 2,
            StoppingAllTests = 20
        }
        private const int rowHeight = 18;
        private const int executeButtonWidth = 90;
        private const int spacer = 0;

        private readonly TestHarness allTests = new TestHarness();
        private ITest currentTest = null;
        private int currentTestPtr = 0;
        private int errors = 0;
        private Executing executeMode = Executing.Stopped;
        private readonly BindingList<LogEntry> gridItems;
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

            //  REGISTER FOR EVENTS FROM TESTS
            foreach (ITest test in allTests)
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

            lblTests.Text = "Tests: " + allTests.Count;

            //  START CONTROL ITEMS
            ControlServer.Start();
            ControlPolicyServer.Start();

            Setup();
        }

        private void Test_PercentCompleteChanged(object sender, TestPercentCompleteChangedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { Test_PercentCompleteChanged(sender, e); }));
            else
            {
                ITest test = (ITest)sender;
                int testIndex = allTests.IndexOf(test);
                if (testIndex < 0) return;
                lCol4[testIndex].Text = e.PercentComplete.ToString() + " %";
            }
        }

        private void Test_TestStatusChanged(object sender, TestStatusChangedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { Test_TestStatusChanged(sender, e); }));
            else
            {
                ITest test = (ITest)sender;
                int testIndex = allTests.IndexOf(test);
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
                    if (executeMode == Executing.AllTests) executeNextTest = true;
                    else if (executeMode == Executing.StoppingAllTests) allTestsComplete = true;
                }
                else if (e.Status == TestStatus.Stopped)
                {
                    statusColor = Color.DarkRed;
                    statusText = "Stopped";
                    executeThisTestEnabled = true;
                    if (executeMode == Executing.AllTests) executeNextTest = true;
                    else if (executeMode == Executing.StoppingAllTests) allTestsComplete = true;
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
                    if (executeMode == Executing.AllTests) executeNextTest = true;
                    else if (executeMode == Executing.StoppingAllTests) allTestsComplete = true;
                }
                else if (e.Status == TestStatus.Successful)
                {
                    statusColor = Color.DarkSlateBlue;
                    statusText = "Successful";
                    executeThisTestEnabled = true;
                    if (executeMode == Executing.AllTests) executeNextTest = true;
                    else if (executeMode == Executing.StoppingAllTests) allTestsComplete = true;
                }
                else
                {
                    throw new Exception("Unknown TestStatus " + e.Status.ToString());
                }


                for (int index = 0; index < allTests.Count; index++)
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
                    if (currentTestPtr < allTests.Count)
                    {
                        currentTest = (ITest)lCol5[currentTestPtr].Tag;
                        currentTest.Start();
                    }
                    else
                    {
                        allTestsComplete = true;
                    }
                }
                
                if (allTestsComplete == true)
                {
                    executeMode = Executing.Stopped;
                    currentTest = null;
                    for (int index = 0; index < allTests.Count; index++)
                    {
                        lCol5[index].Visible = true;
                    }
                    btnExecuteAllTests.Text = "Execute All Tests";
                    btnExecuteAllTests.Enabled = true;
                }
            }
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

        private void Test_TraceEventRaised(object sender, TraceEventArgs e)
        {
            ITest test = (ITest)sender;
            InsertListboxItem("Test " + test.Id.ToString(), e);
        }



        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Width = Convert.ToInt32(Screen.PrimaryScreen.WorkingArea.Width * .5);

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
                    ITest test = (ITest)((Button)sender).Tag;

                    if (executeMode == Executing.AllTests)
                    {
                        test.Start();
                    }
                    else if (executeMode == Executing.Stopped)
                    {
                        test.Start();
                    }
                    else if (executeMode == Executing.SingleTest)
                    {
                        test.Stop();
                    }
                })).Start();
        }

        private void SetTestNotification(ITest Test, TestStatus Value)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetTestNotification(Test, Value); }));
            else
            {
                int index = allTests.IndexOf(Test);
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
            if (executeMode == Executing.Stopped)
            {
                foreach (ITest test in allTests)
                {
                    test.Reset();
                }
                for (int index = 0; index < allTests.Count; index++)
                {
                    lCol5[index].Visible = false;
                }

                executeMode = Executing.AllTests;
                currentTestPtr = 0;
                btnExecuteAllTests.Text = "Stop";
                ExecuteButton_Click(lCol5[currentTestPtr], new EventArgs());
            }
            else if (executeMode == Executing.AllTests)
            {
                executeMode = Executing.StoppingAllTests;
                if (currentTest != null) currentTest.Stop();
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
