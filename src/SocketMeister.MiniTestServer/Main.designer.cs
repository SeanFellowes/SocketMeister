namespace SocketMeister
{
    partial class Main
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
                //if (Global.Server != null) Global.Server.Dispose();
                //Server = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panelOuter = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tbMessageText = new System.Windows.Forms.TextBox();
            this.pnlDiagnostics = new System.Windows.Forms.Panel();
            this.ucSocketServer2 = new SocketMeister.ucSocketServer();
            this.ucSocketServer1 = new SocketMeister.ucSocketServer();
            this.dGrid = new System.Windows.Forms.DataGridView();
            this.TimeStamp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MessageID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.pnlFooter.SuspendLayout();
            this.panelOuter.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.pnlDiagnostics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFooter
            // 
            this.pnlFooter.AutoSize = true;
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 657);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(3);
            this.pnlFooter.Size = new System.Drawing.Size(824, 20);
            this.pnlFooter.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Location = new System.Drawing.Point(3, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(818, 14);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "lblStatus";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelOuter
            // 
            this.panelOuter.Controls.Add(this.panel2);
            this.panelOuter.Controls.Add(this.pnlDiagnostics);
            this.panelOuter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelOuter.Location = new System.Drawing.Point(0, 0);
            this.panelOuter.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panelOuter.Name = "panelOuter";
            this.panelOuter.Padding = new System.Windows.Forms.Padding(3);
            this.panelOuter.Size = new System.Drawing.Size(824, 410);
            this.panelOuter.TabIndex = 26;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(406, 3);
            this.panel2.Margin = new System.Windows.Forms.Padding(6);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(6, 1, 6, 6);
            this.panel2.Size = new System.Drawing.Size(415, 404);
            this.panel2.TabIndex = 45;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tbMessageText);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(6, 1);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox4.Size = new System.Drawing.Size(403, 397);
            this.groupBox4.TabIndex = 42;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Message to Send";
            // 
            // tbMessageText
            // 
            this.tbMessageText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMessageText.Location = new System.Drawing.Point(6, 19);
            this.tbMessageText.Multiline = true;
            this.tbMessageText.Name = "tbMessageText";
            this.tbMessageText.Size = new System.Drawing.Size(391, 372);
            this.tbMessageText.TabIndex = 45;
            this.tbMessageText.Text = resources.GetString("tbMessageText.Text");
            this.tbMessageText.TextChanged += new System.EventHandler(this.tbMessage_TextChanged);
            // 
            // pnlDiagnostics
            // 
            this.pnlDiagnostics.Controls.Add(this.ucSocketServer2);
            this.pnlDiagnostics.Controls.Add(this.ucSocketServer1);
            this.pnlDiagnostics.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlDiagnostics.Location = new System.Drawing.Point(3, 3);
            this.pnlDiagnostics.Margin = new System.Windows.Forms.Padding(6);
            this.pnlDiagnostics.Name = "pnlDiagnostics";
            this.pnlDiagnostics.Padding = new System.Windows.Forms.Padding(6);
            this.pnlDiagnostics.Size = new System.Drawing.Size(403, 404);
            this.pnlDiagnostics.TabIndex = 42;
            // 
            // ucSocketServer2
            // 
            this.ucSocketServer2.Location = new System.Drawing.Point(9, 206);
            this.ucSocketServer2.MessageText = null;
            this.ucSocketServer2.Name = "ucSocketServer2";
            this.ucSocketServer2.NextAutomatedSend = new System.DateTime(2021, 1, 20, 11, 33, 38, 463);
            this.ucSocketServer2.Port = 4506;
            this.ucSocketServer2.ServerId = 2;
            this.ucSocketServer2.Size = new System.Drawing.Size(384, 193);
            this.ucSocketServer2.TabIndex = 1;
            // 
            // ucSocketServer1
            // 
            this.ucSocketServer1.Location = new System.Drawing.Point(10, 6);
            this.ucSocketServer1.MessageText = null;
            this.ucSocketServer1.Name = "ucSocketServer1";
            this.ucSocketServer1.NextAutomatedSend = new System.DateTime(2021, 1, 20, 11, 33, 38, 467);
            this.ucSocketServer1.Port = 4505;
            this.ucSocketServer1.ServerId = 1;
            this.ucSocketServer1.Size = new System.Drawing.Size(384, 196);
            this.ucSocketServer1.TabIndex = 0;
            // 
            // dGrid
            // 
            this.dGrid.AllowUserToAddRows = false;
            this.dGrid.AllowUserToDeleteRows = false;
            this.dGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TimeStamp,
            this.Type,
            this.Server,
            this.MessageID,
            this.Message});
            this.dGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGrid.Location = new System.Drawing.Point(0, 410);
            this.dGrid.Name = "dGrid";
            this.dGrid.Size = new System.Drawing.Size(824, 247);
            this.dGrid.TabIndex = 28;
            // 
            // TimeStamp
            // 
            this.TimeStamp.DataPropertyName = "TimeStamp";
            this.TimeStamp.HeaderText = "TimeStamp";
            this.TimeStamp.Name = "TimeStamp";
            this.TimeStamp.Width = 85;
            // 
            // Type
            // 
            this.Type.DataPropertyName = "Severity";
            this.Type.HeaderText = "Severity";
            this.Type.Name = "Type";
            this.Type.Width = 70;
            // 
            // Server
            // 
            this.Server.DataPropertyName = "Server";
            this.Server.HeaderText = "Server";
            this.Server.Name = "Server";
            this.Server.Width = 63;
            // 
            // MessageID
            // 
            this.MessageID.DataPropertyName = "Source";
            this.MessageID.HeaderText = "Source";
            this.MessageID.Name = "MessageID";
            this.MessageID.ReadOnly = true;
            this.MessageID.Width = 66;
            // 
            // Message
            // 
            this.Message.DataPropertyName = "Text";
            this.Message.HeaderText = "Log Text";
            this.Message.Name = "Message";
            this.Message.Width = 74;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(824, 677);
            this.Controls.Add(this.dGrid);
            this.Controls.Add(this.panelOuter);
            this.Controls.Add(this.pnlFooter);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SocketMeister Mini Test Server 2.0.7";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.pnlFooter.ResumeLayout(false);
            this.panelOuter.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.pnlDiagnostics.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panelOuter;
        private System.Windows.Forms.Panel pnlDiagnostics;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox tbMessageText;
        private System.Windows.Forms.DataGridView dGrid;
        private ucSocketServer ucSocketServer2;
        private ucSocketServer ucSocketServer1;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeStamp;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Server;
        private System.Windows.Forms.DataGridViewTextBoxColumn MessageID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
    }
}

