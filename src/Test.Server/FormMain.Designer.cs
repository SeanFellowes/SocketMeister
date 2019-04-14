namespace Test.Server
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
            this.pnlControl = new System.Windows.Forms.Panel();
            this.panelControlHeader = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dGrid = new System.Windows.Forms.DataGridView();
            this.TimeStamp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Object = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClientID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TestServer4 = new Test.Server.Server();
            this.TestServer3 = new Test.Server.Server();
            this.TestServer2 = new Test.Server.Server();
            this.TestServer1 = new Test.Server.Server();
            this.ControlPolicyServer = new Test.Server.Server();
            this.ControlServer = new Test.Server.Server();
            this.pnlControl.SuspendLayout();
            this.panelControlHeader.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
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
            this.panel1.Size = new System.Drawing.Size(850, 104);
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
            // dGrid
            // 
            this.dGrid.AllowUserToAddRows = false;
            this.dGrid.AllowUserToDeleteRows = false;
            this.dGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TimeStamp,
            this.Object,
            this.Type,
            this.ClientID,
            this.Message});
            this.dGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGrid.Location = new System.Drawing.Point(0, 178);
            this.dGrid.Name = "dGrid";
            this.dGrid.Size = new System.Drawing.Size(850, 469);
            this.dGrid.TabIndex = 29;
            // 
            // TimeStamp
            // 
            this.TimeStamp.DataPropertyName = "TimeStamp";
            this.TimeStamp.HeaderText = "TimeStamp";
            this.TimeStamp.Name = "TimeStamp";
            this.TimeStamp.Width = 85;
            // 
            // Object
            // 
            this.Object.DataPropertyName = "Source";
            this.Object.HeaderText = "Source";
            this.Object.Name = "Object";
            this.Object.Width = 66;
            // 
            // Type
            // 
            this.Type.DataPropertyName = "Severity";
            this.Type.HeaderText = "Severity";
            this.Type.Name = "Type";
            this.Type.Width = 70;
            // 
            // ClientID
            // 
            this.ClientID.DataPropertyName = "EventId";
            this.ClientID.HeaderText = "EventId";
            this.ClientID.Name = "ClientID";
            this.ClientID.ReadOnly = true;
            this.ClientID.Width = 69;
            // 
            // Message
            // 
            this.Message.DataPropertyName = "Message";
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.Width = 75;
            // 
            // TestServer4
            // 
            this.TestServer4.BackColor = System.Drawing.Color.Transparent;
            this.TestServer4.Location = new System.Drawing.Point(307, 71);
            this.TestServer4.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer4.Name = "TestServer4";
            this.TestServer4.Port = 4513;
            this.TestServer4.ServerType = Test.Server.ServerType.SocketServer;
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
            this.TestServer3.ServerType = Test.Server.ServerType.SocketServer;
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
            this.TestServer2.ServerType = Test.Server.ServerType.SocketServer;
            this.TestServer2.Size = new System.Drawing.Size(300, 31);
            this.TestServer2.TabIndex = 6;
            // 
            // TestServer1
            // 
            this.TestServer1.Location = new System.Drawing.Point(3, 41);
            this.TestServer1.Margin = new System.Windows.Forms.Padding(2);
            this.TestServer1.Name = "TestServer1";
            this.TestServer1.Port = 4510;
            this.TestServer1.ServerType = Test.Server.ServerType.SocketServer;
            this.TestServer1.Size = new System.Drawing.Size(300, 31);
            this.TestServer1.TabIndex = 4;
            // 
            // ControlPolicyServer
            // 
            this.ControlPolicyServer.BackColor = System.Drawing.Color.OldLace;
            this.ControlPolicyServer.Location = new System.Drawing.Point(307, 41);
            this.ControlPolicyServer.Margin = new System.Windows.Forms.Padding(2);
            this.ControlPolicyServer.Name = "ControlPolicyServer";
            this.ControlPolicyServer.Port = 4505;
            this.ControlPolicyServer.ServerType = Test.Server.ServerType.PolicyServer;
            this.ControlPolicyServer.Size = new System.Drawing.Size(300, 27);
            this.ControlPolicyServer.TabIndex = 5;
            // 
            // ControlServer
            // 
            this.ControlServer.Location = new System.Drawing.Point(3, 41);
            this.ControlServer.Margin = new System.Windows.Forms.Padding(2);
            this.ControlServer.Name = "ControlServer";
            this.ControlServer.Port = 4505;
            this.ControlServer.ServerType = Test.Server.ServerType.SocketServer;
            this.ControlServer.Size = new System.Drawing.Size(300, 27);
            this.ControlServer.TabIndex = 4;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 647);
            this.Controls.Add(this.dGrid);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlControl);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormMain";
            this.Text = "SocketMeister Server Tester";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.pnlControl.ResumeLayout(false);
            this.panelControlHeader.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
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
        private System.Windows.Forms.DataGridView dGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeStamp;
        private System.Windows.Forms.DataGridViewTextBoxColumn Object;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClientID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
    }
}

