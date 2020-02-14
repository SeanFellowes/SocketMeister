namespace SocketMeister.Test
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.pnlControl = new System.Windows.Forms.Panel();
            this.ControlPolicyServer = new SocketMeister.Test.Server();
            this.ControlServer = new SocketMeister.Test.Server();
            this.panelControlHeader = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TestServer4 = new SocketMeister.Test.Server();
            this.TestServer3 = new SocketMeister.Test.Server();
            this.TestServer2 = new SocketMeister.Test.Server();
            this.TestServer1 = new SocketMeister.Test.Server();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PanelMainTests = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label7 = new System.Windows.Forms.Label();
            this.PanelMain = new System.Windows.Forms.Panel();
            this.PanelMainTrace = new System.Windows.Forms.Panel();
            this.dGrid = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label5 = new System.Windows.Forms.Label();
            this.pnlControl.SuspendLayout();
            this.panelControlHeader.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.PanelMainTests.SuspendLayout();
            this.panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.PanelMain.SuspendLayout();
            this.PanelMainTrace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlControl
            // 
            this.pnlControl.BackColor = System.Drawing.Color.Moccasin;
            this.pnlControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlControl.Controls.Add(this.ControlPolicyServer);
            this.pnlControl.Controls.Add(this.ControlServer);
            this.pnlControl.Controls.Add(this.panelControlHeader);
            this.pnlControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControl.Location = new System.Drawing.Point(0, 0);
            this.pnlControl.Name = "pnlControl";
            this.pnlControl.Padding = new System.Windows.Forms.Padding(3);
            this.pnlControl.Size = new System.Drawing.Size(850, 74);
            this.pnlControl.TabIndex = 2;
            // 
            // ControlPolicyServer
            // 
            this.ControlPolicyServer.BackColor = System.Drawing.Color.OldLace;
            this.ControlPolicyServer.Location = new System.Drawing.Point(307, 41);
            this.ControlPolicyServer.Margin = new System.Windows.Forms.Padding(2);
            this.ControlPolicyServer.Name = "ControlPolicyServer";
            this.ControlPolicyServer.Port = 943;
            this.ControlPolicyServer.ServerType = SocketMeister.Test.ServerType.PolicyServer;
            this.ControlPolicyServer.Size = new System.Drawing.Size(300, 27);
            this.ControlPolicyServer.TabIndex = 5;
            // 
            // ControlServer
            // 
            this.ControlServer.Location = new System.Drawing.Point(3, 41);
            this.ControlServer.Margin = new System.Windows.Forms.Padding(2);
            this.ControlServer.Name = "ControlServer";
            this.ControlServer.Port = 4505;
            this.ControlServer.ServerType = SocketMeister.Test.ServerType.SocketServer;
            this.ControlServer.Size = new System.Drawing.Size(300, 27);
            this.ControlServer.TabIndex = 4;
            // 
            // panelControlHeader
            // 
            this.panelControlHeader.Controls.Add(this.label2);
            this.panelControlHeader.Controls.Add(this.label1);
            this.panelControlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControlHeader.Location = new System.Drawing.Point(3, 3);
            this.panelControlHeader.Name = "panelControlHeader";
            this.panelControlHeader.Padding = new System.Windows.Forms.Padding(6);
            this.panelControlHeader.Size = new System.Drawing.Size(842, 38);
            this.panelControlHeader.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(122, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(714, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "Provides connectivity to clients for the purposes of controlling automated tests." +
    "";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.BurlyWood;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Control";
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
            this.panel1.Location = new System.Drawing.Point(0, 74);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(850, 104);
            this.panel1.TabIndex = 3;
            // 
            // TestServer4
            // 
            this.TestServer4.BackColor = System.Drawing.Color.Transparent;
            this.TestServer4.Location = new System.Drawing.Point(307, 71);
            this.TestServer4.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer4.Name = "TestServer4";
            this.TestServer4.Port = 4513;
            this.TestServer4.ServerType = SocketMeister.Test.ServerType.SocketServer;
            this.TestServer4.Size = new System.Drawing.Size(300, 31);
            this.TestServer4.TabIndex = 8;
            // 
            // TestServer3
            // 
            this.TestServer3.BackColor = System.Drawing.Color.Honeydew;
            this.TestServer3.Location = new System.Drawing.Point(307, 41);
            this.TestServer3.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer3.Name = "TestServer3";
            this.TestServer3.Port = 4511;
            this.TestServer3.ServerType = SocketMeister.Test.ServerType.SocketServer;
            this.TestServer3.Size = new System.Drawing.Size(300, 31);
            this.TestServer3.TabIndex = 7;
            // 
            // TestServer2
            // 
            this.TestServer2.BackColor = System.Drawing.Color.Honeydew;
            this.TestServer2.Location = new System.Drawing.Point(3, 71);
            this.TestServer2.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer2.Name = "TestServer2";
            this.TestServer2.Port = 4512;
            this.TestServer2.ServerType = SocketMeister.Test.ServerType.SocketServer;
            this.TestServer2.Size = new System.Drawing.Size(300, 31);
            this.TestServer2.TabIndex = 6;
            // 
            // TestServer1
            // 
            this.TestServer1.Location = new System.Drawing.Point(3, 41);
            this.TestServer1.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer1.Name = "TestServer1";
            this.TestServer1.Port = 4510;
            this.TestServer1.ServerType = SocketMeister.Test.ServerType.SocketServer;
            this.TestServer1.Size = new System.Drawing.Size(300, 31);
            this.TestServer1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(6);
            this.panel2.Size = new System.Drawing.Size(842, 38);
            this.panel2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(122, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(714, 26);
            this.label3.TabIndex = 1;
            this.label3.Text = "Automatically Activated and Deactivated for automated testing";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.LightGreen;
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 26);
            this.label4.TabIndex = 0;
            this.label4.Text = "Test Services";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PanelMainTests
            // 
            this.PanelMainTests.Controls.Add(this.panel7);
            this.PanelMainTests.Controls.Add(this.dataGridView2);
            this.PanelMainTests.Controls.Add(this.label7);
            this.PanelMainTests.Location = new System.Drawing.Point(32, 227);
            this.PanelMainTests.Name = "PanelMainTests";
            this.PanelMainTests.Padding = new System.Windows.Forms.Padding(3);
            this.PanelMainTests.Size = new System.Drawing.Size(586, 175);
            this.PanelMainTests.TabIndex = 31;
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.button2);
            this.panel7.Controls.Add(this.button1);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel7.Location = new System.Drawing.Point(3, 144);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(580, 28);
            this.panel7.TabIndex = 34;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(117, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 21);
            this.button2.TabIndex = 1;
            this.button2.Text = "Execute All Tests";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(108, 21);
            this.button1.TabIndex = 0;
            this.button1.Text = "Execute Test";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            this.dataGridView2.AllowUserToAddRows = false;
            this.dataGridView2.AllowUserToDeleteRows = false;
            this.dataGridView2.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.dataGridViewTextBoxColumn8,
            this.dataGridViewTextBoxColumn9,
            this.dataGridViewTextBoxColumn10});
            this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView2.Location = new System.Drawing.Point(3, 27);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(580, 145);
            this.dataGridView2.TabIndex = 29;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "TimeStamp";
            this.dataGridViewTextBoxColumn6.HeaderText = "TimeStamp";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.Width = 85;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "Source";
            this.dataGridViewTextBoxColumn7.HeaderText = "Source";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.Width = 66;
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.DataPropertyName = "Severity";
            this.dataGridViewTextBoxColumn8.HeaderText = "Severity";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.Width = 70;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.DataPropertyName = "EventId";
            this.dataGridViewTextBoxColumn9.HeaderText = "EventId";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.ReadOnly = true;
            this.dataGridViewTextBoxColumn9.Width = 69;
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.DataPropertyName = "Message";
            this.dataGridViewTextBoxColumn10.HeaderText = "Message";
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            this.dataGridViewTextBoxColumn10.Width = 75;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Top;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(3, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(580, 24);
            this.label7.TabIndex = 30;
            this.label7.Text = "Tests";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PanelMain
            // 
            this.PanelMain.Controls.Add(this.PanelMainTests);
            this.PanelMain.Controls.Add(this.PanelMainTrace);
            this.PanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelMain.Location = new System.Drawing.Point(0, 178);
            this.PanelMain.Name = "PanelMain";
            this.PanelMain.Size = new System.Drawing.Size(850, 469);
            this.PanelMain.TabIndex = 33;
            this.PanelMain.Resize += new System.EventHandler(this.PanelMain_Resize);
            // 
            // PanelMainTrace
            // 
            this.PanelMainTrace.Controls.Add(this.dGrid);
            this.PanelMainTrace.Controls.Add(this.label5);
            this.PanelMainTrace.Location = new System.Drawing.Point(58, 6);
            this.PanelMainTrace.Name = "PanelMainTrace";
            this.PanelMainTrace.Padding = new System.Windows.Forms.Padding(3);
            this.PanelMainTrace.Size = new System.Drawing.Size(586, 215);
            this.PanelMainTrace.TabIndex = 31;
            // 
            // dGrid
            // 
            this.dGrid.AllowUserToAddRows = false;
            this.dGrid.AllowUserToDeleteRows = false;
            this.dGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5});
            this.dGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGrid.Location = new System.Drawing.Point(3, 27);
            this.dGrid.Name = "dGrid";
            this.dGrid.Size = new System.Drawing.Size(580, 185);
            this.dGrid.TabIndex = 31;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "TimeStamp";
            this.dataGridViewTextBoxColumn1.HeaderText = "TimeStamp";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 85;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Source";
            this.dataGridViewTextBoxColumn2.HeaderText = "Source";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Width = 66;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Severity";
            this.dataGridViewTextBoxColumn3.HeaderText = "Severity";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.Width = 70;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "EventId";
            this.dataGridViewTextBoxColumn4.HeaderText = "EventId";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 69;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Message";
            this.dataGridViewTextBoxColumn5.HeaderText = "Message";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.Width = 75;
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Top;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(3, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(580, 24);
            this.label5.TabIndex = 30;
            this.label5.Text = "Trace Log";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 647);
            this.Controls.Add(this.PanelMain);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormMain";
            this.Text = "SocketMeister Server Tester";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.pnlControl.ResumeLayout(false);
            this.panelControlHeader.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.PanelMainTests.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.PanelMain.ResumeLayout(false);
            this.PanelMainTrace.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControl;
        private Server ControlPolicyServer;
        private Server ControlServer;
        private System.Windows.Forms.Panel panelControlHeader;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private Server TestServer2;
        private Server TestServer1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private Server TestServer4;
        private Server TestServer3;
        private System.Windows.Forms.Panel PanelMainTests;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel PanelMain;
        private System.Windows.Forms.Panel PanelMainTrace;
        private System.Windows.Forms.DataGridView dGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.Label label5;
    }
}

