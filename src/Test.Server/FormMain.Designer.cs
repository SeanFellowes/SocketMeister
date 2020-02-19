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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.pnlControl = new System.Windows.Forms.Panel();
            this.panelControlHeader = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PanelMainTests = new System.Windows.Forms.Panel();
            this.pnlTests = new System.Windows.Forms.Panel();
            this.CH4 = new System.Windows.Forms.TextBox();
            this.CH3 = new System.Windows.Forms.TextBox();
            this.CH2 = new System.Windows.Forms.TextBox();
            this.CH1 = new System.Windows.Forms.TextBox();
            this.panel7 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
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
            this.TestServer4 = new SocketMeister.Test.Server();
            this.TestServer3 = new SocketMeister.Test.Server();
            this.TestServer2 = new SocketMeister.Test.Server();
            this.TestServer1 = new SocketMeister.Test.Server();
            this.ControlPolicyServer = new SocketMeister.Test.Server();
            this.ControlServer = new SocketMeister.Test.Server();
            this.pnlControl.SuspendLayout();
            this.panelControlHeader.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.PanelMainTests.SuspendLayout();
            this.pnlTests.SuspendLayout();
            this.panel7.SuspendLayout();
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
            this.panel1.Size = new System.Drawing.Size(850, 111);
            this.panel1.TabIndex = 3;
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
            this.PanelMainTests.Controls.Add(this.pnlTests);
            this.PanelMainTests.Controls.Add(this.panel7);
            this.PanelMainTests.Controls.Add(this.label7);
            this.PanelMainTests.Location = new System.Drawing.Point(13, 227);
            this.PanelMainTests.Name = "PanelMainTests";
            this.PanelMainTests.Padding = new System.Windows.Forms.Padding(3);
            this.PanelMainTests.Size = new System.Drawing.Size(799, 344);
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
            this.pnlTests.Location = new System.Drawing.Point(3, 27);
            this.pnlTests.Name = "pnlTests";
            this.pnlTests.Size = new System.Drawing.Size(793, 286);
            this.pnlTests.TabIndex = 35;
            this.pnlTests.Resize += new System.EventHandler(this.pnlTests_Resize);
            // 
            // CH4
            // 
            this.CH4.BackColor = System.Drawing.Color.Black;
            this.CH4.ForeColor = System.Drawing.Color.White;
            this.CH4.Location = new System.Drawing.Point(368, 4);
            this.CH4.Name = "CH4";
            this.CH4.Size = new System.Drawing.Size(58, 20);
            this.CH4.TabIndex = 3;
            this.CH4.Text = "%";
            // 
            // CH3
            // 
            this.CH3.BackColor = System.Drawing.Color.Black;
            this.CH3.ForeColor = System.Drawing.Color.White;
            this.CH3.Location = new System.Drawing.Point(267, 4);
            this.CH3.Name = "CH3";
            this.CH3.Size = new System.Drawing.Size(95, 20);
            this.CH3.TabIndex = 2;
            this.CH3.Text = "Status";
            // 
            // CH2
            // 
            this.CH2.BackColor = System.Drawing.Color.Black;
            this.CH2.ForeColor = System.Drawing.Color.White;
            this.CH2.Location = new System.Drawing.Point(62, 4);
            this.CH2.Name = "CH2";
            this.CH2.Size = new System.Drawing.Size(199, 20);
            this.CH2.TabIndex = 1;
            this.CH2.Text = "Description";
            // 
            // CH1
            // 
            this.CH1.BackColor = System.Drawing.Color.Black;
            this.CH1.ForeColor = System.Drawing.Color.White;
            this.CH1.Location = new System.Drawing.Point(6, 4);
            this.CH1.Name = "CH1";
            this.CH1.Size = new System.Drawing.Size(42, 20);
            this.CH1.TabIndex = 0;
            this.CH1.Text = "ID";
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.button2);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel7.Location = new System.Drawing.Point(3, 313);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(793, 28);
            this.panel7.TabIndex = 34;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 21);
            this.button2.TabIndex = 1;
            this.button2.Text = "Execute All Tests";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Top;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(3, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(793, 24);
            this.label7.TabIndex = 30;
            this.label7.Text = "Tests";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PanelMain
            // 
            this.PanelMain.Controls.Add(this.PanelMainTests);
            this.PanelMain.Controls.Add(this.PanelMainTrace);
            this.PanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelMain.Location = new System.Drawing.Point(0, 185);
            this.PanelMain.Name = "PanelMain";
            this.PanelMain.Size = new System.Drawing.Size(850, 715);
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
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.dataGridViewTextBoxColumn5});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.dGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGrid.Location = new System.Drawing.Point(3, 27);
            this.dGrid.Name = "dGrid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
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
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 900);
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
            this.pnlTests.ResumeLayout(false);
            this.pnlTests.PerformLayout();
            this.panel7.ResumeLayout(false);
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
        private System.Windows.Forms.Panel pnlTests;
        private System.Windows.Forms.TextBox CH4;
        private System.Windows.Forms.TextBox CH3;
        private System.Windows.Forms.TextBox CH2;
        private System.Windows.Forms.TextBox CH1;
    }
}

