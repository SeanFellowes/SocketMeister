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
            this.server5 = new Test.Server.Server();
            this.server4 = new Test.Server.Server();
            this.server1 = new Test.Server.Server();
            this.server2 = new Test.Server.Server();
            this.server3 = new Test.Server.Server();
            this.server6 = new Test.Server.Server();
            this.dGrid = new System.Windows.Forms.DataGridView();
            this.TimeStamp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Object = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClientID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EndPointID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MessageID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.pnlControl.Controls.Add(this.server1);
            this.pnlControl.Controls.Add(this.server2);
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
            this.panel1.Controls.Add(this.server3);
            this.panel1.Controls.Add(this.server6);
            this.panel1.Controls.Add(this.server5);
            this.panel1.Controls.Add(this.server4);
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
            // server5
            // 
            this.server5.BackColor = System.Drawing.Color.Honeydew;
            this.server5.Location = new System.Drawing.Point(3, 71);
            this.server5.Margin = new System.Windows.Forms.Padding(2);
            this.server5.Name = "server5";
            this.server5.Port = 4512;
            this.server5.ServerType = Test.Server.ServerType.SocketServer;
            this.server5.Size = new System.Drawing.Size(300, 31);
            this.server5.TabIndex = 6;
            // 
            // server4
            // 
            this.server4.Location = new System.Drawing.Point(3, 41);
            this.server4.Margin = new System.Windows.Forms.Padding(2);
            this.server4.Name = "server4";
            this.server4.Port = 4510;
            this.server4.ServerType = Test.Server.ServerType.SocketServer;
            this.server4.Size = new System.Drawing.Size(300, 31);
            this.server4.TabIndex = 4;
            // 
            // server1
            // 
            this.server1.BackColor = System.Drawing.Color.PapayaWhip;
            this.server1.Location = new System.Drawing.Point(307, 41);
            this.server1.Margin = new System.Windows.Forms.Padding(2);
            this.server1.Name = "server1";
            this.server1.Port = 4505;
            this.server1.ServerType = Test.Server.ServerType.PolicyServer;
            this.server1.Size = new System.Drawing.Size(300, 27);
            this.server1.TabIndex = 5;
            // 
            // server2
            // 
            this.server2.Location = new System.Drawing.Point(3, 41);
            this.server2.Margin = new System.Windows.Forms.Padding(2);
            this.server2.Name = "server2";
            this.server2.Port = 4505;
            this.server2.ServerType = Test.Server.ServerType.SocketServer;
            this.server2.Size = new System.Drawing.Size(300, 27);
            this.server2.TabIndex = 4;
            // 
            // server3
            // 
            this.server3.BackColor = System.Drawing.Color.Transparent;
            this.server3.Location = new System.Drawing.Point(307, 71);
            this.server3.Margin = new System.Windows.Forms.Padding(2);
            this.server3.Name = "server3";
            this.server3.Port = 4513;
            this.server3.ServerType = Test.Server.ServerType.SocketServer;
            this.server3.Size = new System.Drawing.Size(300, 31);
            this.server3.TabIndex = 8;
            // 
            // server6
            // 
            this.server6.BackColor = System.Drawing.Color.Honeydew;
            this.server6.Location = new System.Drawing.Point(307, 41);
            this.server6.Margin = new System.Windows.Forms.Padding(2);
            this.server6.Name = "server6";
            this.server6.Port = 4511;
            this.server6.ServerType = Test.Server.ServerType.SocketServer;
            this.server6.Size = new System.Drawing.Size(300, 31);
            this.server6.TabIndex = 7;
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
            this.EndPointID,
            this.MessageID,
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
            this.Object.DataPropertyName = "Object";
            this.Object.HeaderText = "Object";
            this.Object.Name = "Object";
            this.Object.Width = 63;
            // 
            // Type
            // 
            this.Type.DataPropertyName = "MessageType";
            this.Type.HeaderText = "Type";
            this.Type.Name = "Type";
            this.Type.Width = 56;
            // 
            // ClientID
            // 
            this.ClientID.DataPropertyName = "ClientID";
            this.ClientID.HeaderText = "ClientID";
            this.ClientID.Name = "ClientID";
            this.ClientID.ReadOnly = true;
            this.ClientID.Width = 69;
            // 
            // EndPointID
            // 
            this.EndPointID.DataPropertyName = "EndPointID";
            this.EndPointID.HeaderText = "EndPointID";
            this.EndPointID.Name = "EndPointID";
            this.EndPointID.ReadOnly = true;
            this.EndPointID.Width = 86;
            // 
            // MessageID
            // 
            this.MessageID.DataPropertyName = "MessageID";
            this.MessageID.HeaderText = "MessageID";
            this.MessageID.Name = "MessageID";
            this.MessageID.ReadOnly = true;
            this.MessageID.Width = 86;
            // 
            // Message
            // 
            this.Message.DataPropertyName = "Message";
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.Width = 75;
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
        private Server server1;
        private Server server2;
        private System.Windows.Forms.Panel panelControlHeader;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private Server server5;
        private Server server4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private Server server3;
        private Server server6;
        private System.Windows.Forms.DataGridView dGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeStamp;
        private System.Windows.Forms.DataGridViewTextBoxColumn Object;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClientID;
        private System.Windows.Forms.DataGridViewTextBoxColumn EndPointID;
        private System.Windows.Forms.DataGridViewTextBoxColumn MessageID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
    }
}

