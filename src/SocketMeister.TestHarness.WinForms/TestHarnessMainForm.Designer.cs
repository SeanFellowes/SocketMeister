namespace SocketMeister.Test
{
    partial class TestHarnessMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestHarnessMainForm));
            this.pnlControl = new System.Windows.Forms.Panel();
            this.ControlServer = new SocketMeister.Test.SocketServerOverview();
            this.panelControlHeader = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TestServer4 = new SocketMeister.Test.SocketServerOverview();
            this.TestServer3 = new SocketMeister.Test.SocketServerOverview();
            this.TestServer2 = new SocketMeister.Test.SocketServerOverview();
            this.TestServer1 = new SocketMeister.Test.SocketServerOverview();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PanelMainTests = new System.Windows.Forms.Panel();
            this.pnlTests = new System.Windows.Forms.Panel();
            this.CH4 = new System.Windows.Forms.TextBox();
            this.CH3 = new System.Windows.Forms.TextBox();
            this.CH2 = new System.Windows.Forms.TextBox();
            this.CH1 = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnExecuteAllTests = new System.Windows.Forms.Button();
            this.lblTests = new System.Windows.Forms.Label();
            this.PanelMain = new System.Windows.Forms.Panel();
            this.PanelMainTrace = new System.Windows.Forms.Panel();
            this.dGrid = new System.Windows.Forms.DataGridView();
            this.PnlTestsHeader = new System.Windows.Forms.Panel();
            this.lblErrors = new System.Windows.Forms.Label();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.lblTraceLog = new System.Windows.Forms.Label();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnStack = new System.Windows.Forms.DataGridViewButtonColumn();
            this.StackTrace = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlControl.SuspendLayout();
            this.panelControlHeader.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.PanelMainTests.SuspendLayout();
            this.pnlTests.SuspendLayout();
            this.panel3.SuspendLayout();
            this.PanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).BeginInit();
            this.PanelMainTrace.SuspendLayout();
            this.PnlTestsHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControl
            // 
            this.pnlControl.BackColor = System.Drawing.Color.Moccasin;
            this.pnlControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlControl.Controls.Add(this.ControlServer);
            this.pnlControl.Controls.Add(this.panelControlHeader);
            this.pnlControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControl.Location = new System.Drawing.Point(0, 0);
            this.pnlControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlControl.Name = "pnlControl";
            this.pnlControl.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlControl.Size = new System.Drawing.Size(992, 85);
            this.pnlControl.TabIndex = 2;
            // 
            // ControlServer
            // 
            this.ControlServer.Location = new System.Drawing.Point(4, 47);
            this.ControlServer.Margin = new System.Windows.Forms.Padding(2);
            this.ControlServer.Name = "ControlServer";
            this.ControlServer.Port = 4505;
            this.ControlServer.Size = new System.Drawing.Size(350, 31);
            this.ControlServer.SocketServerType = SocketMeister.Test.SocketServerOverview.SocketServerTypes.SocketServer;
            this.ControlServer.TabIndex = 4;
            // 
            // panelControlHeader
            // 
            this.panelControlHeader.Controls.Add(this.label2);
            this.panelControlHeader.Controls.Add(this.label1);
            this.panelControlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlHeader.Location = new System.Drawing.Point(4, 3);
            this.panelControlHeader.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelControlHeader.Name = "panelControlHeader";
            this.panelControlHeader.Padding = new System.Windows.Forms.Padding(7);
            this.panelControlHeader.Size = new System.Drawing.Size(982, 44);
            this.panelControlHeader.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(142, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(833, 30);
            this.label2.TabIndex = 1;
            this.label2.Text = "A special socket for communicating instructions and results between the test harn" +
    "ess and multiple test clients.";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.BurlyWood;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Controller";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.PaleTurquoise;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.TestServer4);
            this.panel1.Controls.Add(this.TestServer3);
            this.panel1.Controls.Add(this.TestServer2);
            this.panel1.Controls.Add(this.TestServer1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 85);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Size = new System.Drawing.Size(992, 128);
            this.panel1.TabIndex = 3;
            // 
            // TestServer4
            // 
            this.TestServer4.BackColor = System.Drawing.Color.Transparent;
            this.TestServer4.Location = new System.Drawing.Point(358, 82);
            this.TestServer4.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer4.Name = "TestServer4";
            this.TestServer4.Port = 4513;
            this.TestServer4.Size = new System.Drawing.Size(350, 36);
            this.TestServer4.SocketServerType = SocketMeister.Test.SocketServerOverview.SocketServerTypes.SocketServer;
            this.TestServer4.TabIndex = 8;
            // 
            // TestServer3
            // 
            this.TestServer3.BackColor = System.Drawing.Color.Honeydew;
            this.TestServer3.Location = new System.Drawing.Point(358, 47);
            this.TestServer3.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer3.Name = "TestServer3";
            this.TestServer3.Port = 4511;
            this.TestServer3.Size = new System.Drawing.Size(350, 36);
            this.TestServer3.SocketServerType = SocketMeister.Test.SocketServerOverview.SocketServerTypes.SocketServer;
            this.TestServer3.TabIndex = 7;
            // 
            // TestServer2
            // 
            this.TestServer2.BackColor = System.Drawing.Color.Honeydew;
            this.TestServer2.Location = new System.Drawing.Point(4, 82);
            this.TestServer2.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer2.Name = "TestServer2";
            this.TestServer2.Port = 4512;
            this.TestServer2.Size = new System.Drawing.Size(350, 36);
            this.TestServer2.SocketServerType = SocketMeister.Test.SocketServerOverview.SocketServerTypes.SocketServer;
            this.TestServer2.TabIndex = 6;
            // 
            // TestServer1
            // 
            this.TestServer1.Location = new System.Drawing.Point(4, 47);
            this.TestServer1.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer1.Name = "TestServer1";
            this.TestServer1.Port = 4510;
            this.TestServer1.Size = new System.Drawing.Size(350, 36);
            this.TestServer1.SocketServerType = SocketMeister.Test.SocketServerOverview.SocketServerTypes.SocketServer;
            this.TestServer1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(4, 3);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(7);
            this.panel2.Size = new System.Drawing.Size(982, 44);
            this.panel2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(142, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(833, 30);
            this.label3.TabIndex = 1;
            this.label3.Text = "Automatically Activated and Deactivated for automated testing";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.LightGreen;
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(7, 7);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(135, 30);
            this.label4.TabIndex = 0;
            this.label4.Text = "Test Services";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PanelMainTests
            // 
            this.PanelMainTests.Controls.Add(this.pnlTests);
            this.PanelMainTests.Controls.Add(this.panel3);
            this.PanelMainTests.Location = new System.Drawing.Point(15, 262);
            this.PanelMainTests.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PanelMainTests.Name = "PanelMainTests";
            this.PanelMainTests.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PanelMainTests.Size = new System.Drawing.Size(932, 397);
            this.PanelMainTests.TabIndex = 31;
            // 
            // pnlTests
            // 
            this.pnlTests.AutoScroll = true;
            this.pnlTests.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlTests.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlTests.Controls.Add(this.CH4);
            this.pnlTests.Controls.Add(this.CH3);
            this.pnlTests.Controls.Add(this.CH2);
            this.pnlTests.Controls.Add(this.CH1);
            this.pnlTests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTests.Location = new System.Drawing.Point(4, 34);
            this.pnlTests.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pnlTests.Name = "pnlTests";
            this.pnlTests.Size = new System.Drawing.Size(924, 360);
            this.pnlTests.TabIndex = 35;
            this.pnlTests.Resize += new System.EventHandler(this.PnlTests_Resize);
            // 
            // CH4
            // 
            this.CH4.BackColor = System.Drawing.Color.Black;
            this.CH4.ForeColor = System.Drawing.Color.White;
            this.CH4.Location = new System.Drawing.Point(429, 5);
            this.CH4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CH4.Name = "CH4";
            this.CH4.Size = new System.Drawing.Size(67, 23);
            this.CH4.TabIndex = 3;
            this.CH4.Text = "%";
            this.CH4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // CH3
            // 
            this.CH3.BackColor = System.Drawing.Color.Black;
            this.CH3.ForeColor = System.Drawing.Color.White;
            this.CH3.Location = new System.Drawing.Point(312, 5);
            this.CH3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CH3.Name = "CH3";
            this.CH3.Size = new System.Drawing.Size(110, 23);
            this.CH3.TabIndex = 2;
            this.CH3.Text = "Status";
            // 
            // CH2
            // 
            this.CH2.BackColor = System.Drawing.Color.Black;
            this.CH2.ForeColor = System.Drawing.Color.White;
            this.CH2.Location = new System.Drawing.Point(72, 5);
            this.CH2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CH2.Name = "CH2";
            this.CH2.Size = new System.Drawing.Size(231, 23);
            this.CH2.TabIndex = 1;
            this.CH2.Text = "Description";
            // 
            // CH1
            // 
            this.CH1.BackColor = System.Drawing.Color.Black;
            this.CH1.ForeColor = System.Drawing.Color.White;
            this.CH1.Location = new System.Drawing.Point(7, 5);
            this.CH1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CH1.Name = "CH1";
            this.CH1.Size = new System.Drawing.Size(48, 23);
            this.CH1.TabIndex = 0;
            this.CH1.Text = "ID";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnExecuteAllTests);
            this.panel3.Controls.Add(this.lblTests);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(4, 3);
            this.panel3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel3.Size = new System.Drawing.Size(924, 31);
            this.panel3.TabIndex = 36;
            // 
            // btnExecuteAllTests
            // 
            this.btnExecuteAllTests.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnExecuteAllTests.Location = new System.Drawing.Point(786, 3);
            this.btnExecuteAllTests.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExecuteAllTests.Name = "btnExecuteAllTests";
            this.btnExecuteAllTests.Size = new System.Drawing.Size(134, 25);
            this.btnExecuteAllTests.TabIndex = 1;
            this.btnExecuteAllTests.Text = "Execute All Tests";
            this.btnExecuteAllTests.UseVisualStyleBackColor = true;
            this.btnExecuteAllTests.Click += new System.EventHandler(this.BtnExecuteAllTests_Click);
            // 
            // lblTests
            // 
            this.lblTests.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTests.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTests.Location = new System.Drawing.Point(4, 3);
            this.lblTests.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTests.Name = "lblTests";
            this.lblTests.Size = new System.Drawing.Size(167, 25);
            this.lblTests.TabIndex = 30;
            this.lblTests.Text = "Tests: 0";
            this.lblTests.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PanelMain
            // 
            this.PanelMain.Controls.Add(this.PanelMainTests);
            this.PanelMain.Controls.Add(this.PanelMainTrace);
            this.PanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelMain.Location = new System.Drawing.Point(0, 213);
            this.PanelMain.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PanelMain.Name = "PanelMain";
            this.PanelMain.Size = new System.Drawing.Size(992, 825);
            this.PanelMain.TabIndex = 33;
            this.PanelMain.Resize += new System.EventHandler(this.PanelMain_Resize);
            // 
            // PanelMainTrace
            // 
            // 
            // dGrid
            // 
            this.dGrid.AllowUserToAddRows = false;
            this.dGrid.AllowUserToDeleteRows = false;
            this.dGrid.AllowUserToResizeRows = false;
            this.dGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.ColumnStack,
            this.StackTrace});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.dGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dGrid.Location = new System.Drawing.Point(4, 34);
            this.dGrid.Margin = new System.Windows.Forms.Padding(4, 1, 4, 1);
            this.dGrid.MultiSelect = false;
            this.dGrid.Name = "dGrid";
            this.dGrid.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dGrid.ShowCellErrors = false;
            this.dGrid.ShowCellToolTips = false;
            this.dGrid.ShowEditingIcon = false;
            this.dGrid.ShowRowErrors = false;
            this.dGrid.Size = new System.Drawing.Size(676, 211);
            this.dGrid.TabIndex = 31;
            this.dGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGrid_CellContentClick);
            this.PanelMainTrace.Controls.Add(this.dGrid);
            this.PanelMainTrace.Controls.Add(this.PnlTestsHeader);
            this.PanelMainTrace.Location = new System.Drawing.Point(68, 7);
            this.PanelMainTrace.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PanelMainTrace.Name = "PanelMainTrace";
            this.PanelMainTrace.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PanelMainTrace.Size = new System.Drawing.Size(684, 248);
            this.PanelMainTrace.TabIndex = 31;
            // 
            // PnlTestsHeader
            // 
            this.PnlTestsHeader.Controls.Add(this.lblErrors);
            this.PnlTestsHeader.Controls.Add(this.btnClearLog);
            this.PnlTestsHeader.Controls.Add(this.lblTraceLog);
            this.PnlTestsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.PnlTestsHeader.Location = new System.Drawing.Point(4, 3);
            this.PnlTestsHeader.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PnlTestsHeader.Name = "PnlTestsHeader";
            this.PnlTestsHeader.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PnlTestsHeader.Size = new System.Drawing.Size(676, 31);
            this.PnlTestsHeader.TabIndex = 32;
            // 
            // lblErrors
            // 
            this.lblErrors.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblErrors.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblErrors.Location = new System.Drawing.Point(151, 3);
            this.lblErrors.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblErrors.Name = "lblErrors";
            this.lblErrors.Size = new System.Drawing.Size(104, 25);
            this.lblErrors.TabIndex = 32;
            this.lblErrors.Text = "Errors: 0";
            this.lblErrors.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClearLog
            // 
            this.btnClearLog.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClearLog.Location = new System.Drawing.Point(538, 3);
            this.btnClearLog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(134, 25);
            this.btnClearLog.TabIndex = 31;
            this.btnClearLog.Text = "Clear Trace Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.BtnClearLog_Click);
            // 
            // lblTraceLog
            // 
            this.lblTraceLog.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTraceLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTraceLog.Location = new System.Drawing.Point(4, 3);
            this.lblTraceLog.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTraceLog.Name = "lblTraceLog";
            this.lblTraceLog.Size = new System.Drawing.Size(147, 25);
            this.lblTraceLog.TabIndex = 30;
            this.lblTraceLog.Text = "Trace Log: 0";
            this.lblTraceLog.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "TimeStamp";
            this.dataGridViewTextBoxColumn1.HeaderText = "TimeStamp";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn1.Width = 85;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Source";
            this.dataGridViewTextBoxColumn2.HeaderText = "Source";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn2.Width = 66;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Severity";
            this.dataGridViewTextBoxColumn3.HeaderText = "Severity";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn3.Width = 70;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "EventId";
            this.dataGridViewTextBoxColumn4.HeaderText = "EventId";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn4.Width = 69;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Message";
            this.dataGridViewTextBoxColumn5.HeaderText = "Message";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewTextBoxColumn5.Width = 75;
            // 
            // ColumnStack
            // 
            this.ColumnStack.HeaderText = "Stack";
            this.ColumnStack.Name = "ColumnStack";
            this.ColumnStack.ReadOnly = true;
            this.ColumnStack.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColumnStack.Text = "Stack";
            this.ColumnStack.Width = 41;
            // 
            // StackTrace
            // 
            this.StackTrace.DataPropertyName = "StackTrace";
            this.StackTrace.HeaderText = "StackTrace";
            this.StackTrace.Name = "StackTrace";
            this.StackTrace.ReadOnly = true;
            this.StackTrace.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.StackTrace.Width = 88;
            // 
            // TestHarnessMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 1038);
            this.Controls.Add(this.PanelMain);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "TestHarnessMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SocketMeister Test Harness";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.pnlControl.ResumeLayout(false);
            this.panelControlHeader.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.PanelMainTests.ResumeLayout(false);
            this.pnlTests.ResumeLayout(false);
            this.pnlTests.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.PanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).EndInit();
            this.PanelMainTrace.ResumeLayout(false);
            this.PnlTestsHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControl;
        private SocketServerOverview ControlServer;
        private System.Windows.Forms.Panel panelControlHeader;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private SocketServerOverview TestServer2;
        private SocketServerOverview TestServer1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private SocketServerOverview TestServer4;
        private SocketServerOverview TestServer3;
        private System.Windows.Forms.Panel PanelMainTests;
        private System.Windows.Forms.Button btnExecuteAllTests;
        private System.Windows.Forms.Label lblTests;
        private System.Windows.Forms.Panel PanelMain;
        private System.Windows.Forms.Panel PanelMainTrace;
        private System.Windows.Forms.Label lblTraceLog;
        private System.Windows.Forms.Panel pnlTests;
        private System.Windows.Forms.TextBox CH4;
        private System.Windows.Forms.TextBox CH3;
        private System.Windows.Forms.TextBox CH2;
        private System.Windows.Forms.TextBox CH1;
        private System.Windows.Forms.Panel PnlTestsHeader;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblErrors;
        private System.Windows.Forms.DataGridView dGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnStack;
        private System.Windows.Forms.DataGridViewTextBoxColumn StackTrace;
    }
}

