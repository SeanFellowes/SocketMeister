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
                Global.Server = null;
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
            this.tbBroadcast = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnSendM1Subs = new System.Windows.Forms.Button();
            this.btnSendM2Subs = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnBroadcast = new System.Windows.Forms.Button();
            this.pnlDiagnostics = new System.Windows.Forms.Panel();
            this.pnlSettings = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.cbCompressMessage = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.cbLogInfo = new System.Windows.Forms.CheckBox();
            this.serviceStatusIndicator = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblAbortedClients = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblBytesSent = new System.Windows.Forms.Label();
            this.lblBytesReceived = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lblMessage2Subscribers = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lblMessage1Subscribers = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblSubscriptionEndPoints = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblBroadcastRecipients = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblOutboundMessageQueue = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblTotalConnectedClients = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblTotalMessagesSent = new System.Windows.Forms.Label();
            this.lblTotalMessagesReceived = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlAutoBroadcast = new System.Windows.Forms.GroupBox();
            this.rbAutoBCFast = new System.Windows.Forms.RadioButton();
            this.rbAutoBCLarge = new System.Windows.Forms.RadioButton();
            this.rbAutoBCOff = new System.Windows.Forms.RadioButton();
            this.dGrid = new System.Windows.Forms.DataGridView();
            this.TimeStamp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Object = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClientID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EndPointID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MessageID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.pnlFooter.SuspendLayout();
            this.panelOuter.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pnlDiagnostics.SuspendLayout();
            this.pnlSettings.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.pnlAutoBroadcast.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFooter
            // 
            this.pnlFooter.AutoSize = true;
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pnlFooter.Controls.Add(this.lblStatus);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 648);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(3);
            this.pnlFooter.Size = new System.Drawing.Size(954, 20);
            this.pnlFooter.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Location = new System.Drawing.Point(3, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(948, 14);
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
            this.panelOuter.Size = new System.Drawing.Size(954, 493);
            this.panelOuter.TabIndex = 26;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(365, 3);
            this.panel2.Margin = new System.Windows.Forms.Padding(6);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(6, 1, 6, 6);
            this.panel2.Size = new System.Drawing.Size(586, 487);
            this.panel2.TabIndex = 45;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tbBroadcast);
            this.groupBox4.Controls.Add(this.panel3);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(6, 1);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox4.Size = new System.Drawing.Size(574, 480);
            this.groupBox4.TabIndex = 42;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Broadcast and Subscriber Messaging";
            // 
            // tbBroadcast
            // 
            this.tbBroadcast.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbBroadcast.Location = new System.Drawing.Point(6, 19);
            this.tbBroadcast.Multiline = true;
            this.tbBroadcast.Name = "tbBroadcast";
            this.tbBroadcast.Size = new System.Drawing.Size(562, 394);
            this.tbBroadcast.TabIndex = 45;
            this.tbBroadcast.Text = "Test message from server";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnSendM1Subs);
            this.panel3.Controls.Add(this.btnSendM2Subs);
            this.panel3.Controls.Add(this.panel1);
            this.panel3.Controls.Add(this.btnBroadcast);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(6, 413);
            this.panel3.Margin = new System.Windows.Forms.Padding(6);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(6);
            this.panel3.Size = new System.Drawing.Size(562, 61);
            this.panel3.TabIndex = 44;
            // 
            // btnSendM1Subs
            // 
            this.btnSendM1Subs.Location = new System.Drawing.Point(0, 0);
            this.btnSendM1Subs.Name = "btnSendM1Subs";
            this.btnSendM1Subs.Size = new System.Drawing.Size(75, 23);
            this.btnSendM1Subs.TabIndex = 0;
            // 
            // btnSendM2Subs
            // 
            this.btnSendM2Subs.Location = new System.Drawing.Point(0, 0);
            this.btnSendM2Subs.Name = "btnSendM2Subs";
            this.btnSendM2Subs.Size = new System.Drawing.Size(75, 23);
            this.btnSendM2Subs.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(400, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(16, 49);
            this.panel1.TabIndex = 40;
            // 
            // btnBroadcast
            // 
            this.btnBroadcast.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnBroadcast.Enabled = false;
            this.btnBroadcast.Location = new System.Drawing.Point(416, 6);
            this.btnBroadcast.Margin = new System.Windows.Forms.Padding(12, 3, 3, 3);
            this.btnBroadcast.Name = "btnBroadcast";
            this.btnBroadcast.Size = new System.Drawing.Size(140, 49);
            this.btnBroadcast.TabIndex = 37;
            this.btnBroadcast.Text = "Broadcast";
            this.btnBroadcast.UseVisualStyleBackColor = true;
            this.btnBroadcast.Click += new System.EventHandler(this.btnBroadcast_Click);
            // 
            // pnlDiagnostics
            // 
            this.pnlDiagnostics.Controls.Add(this.pnlSettings);
            this.pnlDiagnostics.Controls.Add(this.groupBox3);
            this.pnlDiagnostics.Controls.Add(this.groupBox2);
            this.pnlDiagnostics.Controls.Add(this.pnlAutoBroadcast);
            this.pnlDiagnostics.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlDiagnostics.Location = new System.Drawing.Point(3, 3);
            this.pnlDiagnostics.Margin = new System.Windows.Forms.Padding(6);
            this.pnlDiagnostics.Name = "pnlDiagnostics";
            this.pnlDiagnostics.Padding = new System.Windows.Forms.Padding(6);
            this.pnlDiagnostics.Size = new System.Drawing.Size(362, 487);
            this.pnlDiagnostics.TabIndex = 42;
            // 
            // pnlSettings
            // 
            this.pnlSettings.Controls.Add(this.label14);
            this.pnlSettings.Controls.Add(this.tbPort);
            this.pnlSettings.Controls.Add(this.label15);
            this.pnlSettings.Controls.Add(this.label13);
            this.pnlSettings.Controls.Add(this.cbCompressMessage);
            this.pnlSettings.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSettings.Enabled = false;
            this.pnlSettings.Location = new System.Drawing.Point(6, 5);
            this.pnlSettings.Margin = new System.Windows.Forms.Padding(6, 12, 6, 6);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Padding = new System.Windows.Forms.Padding(6);
            this.pnlSettings.Size = new System.Drawing.Size(350, 238);
            this.pnlSettings.TabIndex = 69;
            this.pnlSettings.TabStop = false;
            this.pnlSettings.Text = "Socket Server Settings";
            // 
            // label14
            // 
            this.label14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label14.Location = new System.Drawing.Point(173, 66);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(171, 42);
            this.label14.TabIndex = 82;
            this.label14.Text = "Port which the service will listen on. NOTE: Silverlight only works with ports 45" +
    "02-4534.";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(85, 78);
            this.tbPort.MaxLength = 5;
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(43, 20);
            this.tbPort.TabIndex = 81;
            this.tbPort.Text = "4505";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(21, 81);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(29, 13);
            this.label15.TabIndex = 80;
            this.label15.Text = "Port:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            this.label13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label13.Location = new System.Drawing.Point(173, 22);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(171, 29);
            this.label13.TabIndex = 79;
            this.label13.Text = "Automatically compress messages > 99 bytes";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbCompressMessage
            // 
            this.cbCompressMessage.AutoSize = true;
            this.cbCompressMessage.Checked = true;
            this.cbCompressMessage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCompressMessage.Location = new System.Drawing.Point(23, 29);
            this.cbCompressMessage.Name = "cbCompressMessage";
            this.cbCompressMessage.Size = new System.Drawing.Size(148, 17);
            this.cbCompressMessage.TabIndex = 61;
            this.cbCompressMessage.Text = "Compress Sent Messages";
            this.cbCompressMessage.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnStop);
            this.groupBox3.Controls.Add(this.btnStart);
            this.groupBox3.Controls.Add(this.cbLogInfo);
            this.groupBox3.Controls.Add(this.serviceStatusIndicator);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox3.Location = new System.Drawing.Point(6, 243);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox3.Size = new System.Drawing.Size(350, 58);
            this.groupBox3.TabIndex = 70;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Socket Server Status";
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(112, 21);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(80, 26);
            this.btnStop.TabIndex = 62;
            this.btnStop.Text = "Stop Service";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(24, 21);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(80, 26);
            this.btnStart.TabIndex = 61;
            this.btnStart.Text = "Start Service";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // cbLogInfo
            // 
            this.cbLogInfo.AutoSize = true;
            this.cbLogInfo.Checked = true;
            this.cbLogInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLogInfo.Location = new System.Drawing.Point(265, 26);
            this.cbLogInfo.Name = "cbLogInfo";
            this.cbLogInfo.Size = new System.Drawing.Size(65, 17);
            this.cbLogInfo.TabIndex = 52;
            this.cbLogInfo.Text = "Log Info";
            this.cbLogInfo.UseVisualStyleBackColor = true;
            // 
            // serviceStatusIndicator
            // 
            this.serviceStatusIndicator.BackColor = System.Drawing.Color.Red;
            this.serviceStatusIndicator.Enabled = false;
            this.serviceStatusIndicator.Location = new System.Drawing.Point(207, 22);
            this.serviceStatusIndicator.Name = "serviceStatusIndicator";
            this.serviceStatusIndicator.Size = new System.Drawing.Size(28, 23);
            this.serviceStatusIndicator.TabIndex = 51;
            this.serviceStatusIndicator.UseVisualStyleBackColor = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblAbortedClients);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.lblBytesSent);
            this.groupBox2.Controls.Add(this.lblBytesReceived);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.lblMessage2Subscribers);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.lblMessage1Subscribers);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.lblSubscriptionEndPoints);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.lblBroadcastRecipients);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.lblOutboundMessageQueue);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.lblTotalConnectedClients);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.lblTotalMessagesSent);
            this.groupBox2.Controls.Add(this.lblTotalMessagesReceived);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox2.Location = new System.Drawing.Point(6, 301);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox2.Size = new System.Drawing.Size(350, 135);
            this.groupBox2.TabIndex = 68;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Diagnostics";
            // 
            // lblAbortedClients
            // 
            this.lblAbortedClients.Location = new System.Drawing.Point(122, 38);
            this.lblAbortedClients.Name = "lblAbortedClients";
            this.lblAbortedClients.Size = new System.Drawing.Size(41, 13);
            this.lblAbortedClients.TabIndex = 88;
            this.lblAbortedClients.Text = "0";
            this.lblAbortedClients.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(11, 38);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(81, 13);
            this.label11.TabIndex = 87;
            this.label11.Text = "Aborted Clients:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBytesSent
            // 
            this.lblBytesSent.Location = new System.Drawing.Point(253, 116);
            this.lblBytesSent.Name = "lblBytesSent";
            this.lblBytesSent.Size = new System.Drawing.Size(90, 13);
            this.lblBytesSent.TabIndex = 86;
            this.lblBytesSent.Text = "0";
            this.lblBytesSent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBytesReceived
            // 
            this.lblBytesReceived.Location = new System.Drawing.Point(256, 99);
            this.lblBytesReceived.Name = "lblBytesReceived";
            this.lblBytesReceived.Size = new System.Drawing.Size(87, 13);
            this.lblBytesReceived.TabIndex = 85;
            this.lblBytesReceived.Text = "0";
            this.lblBytesReceived.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(193, 116);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(61, 13);
            this.label16.TabIndex = 84;
            this.label16.Text = "Bytes Sent:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(193, 99);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(68, 13);
            this.label17.TabIndex = 83;
            this.label17.Text = "Bytes Rcvd: ";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMessage2Subscribers
            // 
            this.lblMessage2Subscribers.Location = new System.Drawing.Point(122, 114);
            this.lblMessage2Subscribers.Name = "lblMessage2Subscribers";
            this.lblMessage2Subscribers.Size = new System.Drawing.Size(41, 13);
            this.lblMessage2Subscribers.TabIndex = 82;
            this.lblMessage2Subscribers.Text = "0";
            this.lblMessage2Subscribers.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(26, 114);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 13);
            this.label12.TabIndex = 81;
            this.label12.Text = "MESSAGE2: ";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMessage1Subscribers
            // 
            this.lblMessage1Subscribers.Location = new System.Drawing.Point(122, 97);
            this.lblMessage1Subscribers.Name = "lblMessage1Subscribers";
            this.lblMessage1Subscribers.Size = new System.Drawing.Size(41, 13);
            this.lblMessage1Subscribers.TabIndex = 80;
            this.lblMessage1Subscribers.Text = "0";
            this.lblMessage1Subscribers.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(26, 97);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 13);
            this.label10.TabIndex = 79;
            this.label10.Text = "MESSAGE1: ";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSubscriptionEndPoints
            // 
            this.lblSubscriptionEndPoints.Location = new System.Drawing.Point(122, 80);
            this.lblSubscriptionEndPoints.Name = "lblSubscriptionEndPoints";
            this.lblSubscriptionEndPoints.Size = new System.Drawing.Size(41, 13);
            this.lblSubscriptionEndPoints.TabIndex = 78;
            this.lblSubscriptionEndPoints.Text = "0";
            this.lblSubscriptionEndPoints.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 80);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(103, 13);
            this.label9.TabIndex = 77;
            this.label9.Text = "Total Subscriptions: ";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBroadcastRecipients
            // 
            this.lblBroadcastRecipients.Location = new System.Drawing.Point(122, 54);
            this.lblBroadcastRecipients.Name = "lblBroadcastRecipients";
            this.lblBroadcastRecipients.Size = new System.Drawing.Size(41, 13);
            this.lblBroadcastRecipients.TabIndex = 76;
            this.lblBroadcastRecipients.Text = "0";
            this.lblBroadcastRecipients.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 54);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(111, 13);
            this.label8.TabIndex = 75;
            this.label8.Text = "Broadcast Recipients:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOutboundMessageQueue
            // 
            this.lblOutboundMessageQueue.Location = new System.Drawing.Point(293, 69);
            this.lblOutboundMessageQueue.Name = "lblOutboundMessageQueue";
            this.lblOutboundMessageQueue.Size = new System.Drawing.Size(50, 13);
            this.lblOutboundMessageQueue.TabIndex = 74;
            this.lblOutboundMessageQueue.Text = "0";
            this.lblOutboundMessageQueue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(193, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(114, 28);
            this.label5.TabIndex = 73;
            this.label5.Text = "Queued Outbound Messages:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTotalConnectedClients
            // 
            this.lblTotalConnectedClients.Location = new System.Drawing.Point(122, 22);
            this.lblTotalConnectedClients.Name = "lblTotalConnectedClients";
            this.lblTotalConnectedClients.Size = new System.Drawing.Size(41, 13);
            this.lblTotalConnectedClients.TabIndex = 72;
            this.lblTotalConnectedClients.Text = "0";
            this.lblTotalConnectedClients.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 71;
            this.label4.Text = "Connected Clients:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTotalMessagesSent
            // 
            this.lblTotalMessagesSent.Location = new System.Drawing.Point(293, 39);
            this.lblTotalMessagesSent.Name = "lblTotalMessagesSent";
            this.lblTotalMessagesSent.Size = new System.Drawing.Size(50, 13);
            this.lblTotalMessagesSent.TabIndex = 70;
            this.lblTotalMessagesSent.Text = "0";
            this.lblTotalMessagesSent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTotalMessagesReceived
            // 
            this.lblTotalMessagesReceived.Location = new System.Drawing.Point(293, 22);
            this.lblTotalMessagesReceived.Name = "lblTotalMessagesReceived";
            this.lblTotalMessagesReceived.Size = new System.Drawing.Size(50, 13);
            this.lblTotalMessagesReceived.TabIndex = 69;
            this.lblTotalMessagesReceived.Text = "0";
            this.lblTotalMessagesReceived.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(193, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 68;
            this.label3.Text = "Messages Sent:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(193, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 67;
            this.label2.Text = "Messages Rcvd: ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlAutoBroadcast
            // 
            this.pnlAutoBroadcast.Controls.Add(this.rbAutoBCFast);
            this.pnlAutoBroadcast.Controls.Add(this.rbAutoBCLarge);
            this.pnlAutoBroadcast.Controls.Add(this.rbAutoBCOff);
            this.pnlAutoBroadcast.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAutoBroadcast.Location = new System.Drawing.Point(6, 436);
            this.pnlAutoBroadcast.Margin = new System.Windows.Forms.Padding(6);
            this.pnlAutoBroadcast.Name = "pnlAutoBroadcast";
            this.pnlAutoBroadcast.Padding = new System.Windows.Forms.Padding(6);
            this.pnlAutoBroadcast.Size = new System.Drawing.Size(350, 45);
            this.pnlAutoBroadcast.TabIndex = 67;
            this.pnlAutoBroadcast.TabStop = false;
            this.pnlAutoBroadcast.Text = "Auto Broadcast";
            // 
            // rbAutoBCFast
            // 
            this.rbAutoBCFast.AutoSize = true;
            this.rbAutoBCFast.Location = new System.Drawing.Point(179, 21);
            this.rbAutoBCFast.Name = "rbAutoBCFast";
            this.rbAutoBCFast.Size = new System.Drawing.Size(71, 17);
            this.rbAutoBCFast.TabIndex = 60;
            this.rbAutoBCFast.Text = "Fast Data";
            this.rbAutoBCFast.UseVisualStyleBackColor = true;
            // 
            // rbAutoBCLarge
            // 
            this.rbAutoBCLarge.AutoSize = true;
            this.rbAutoBCLarge.Location = new System.Drawing.Point(91, 20);
            this.rbAutoBCLarge.Name = "rbAutoBCLarge";
            this.rbAutoBCLarge.Size = new System.Drawing.Size(66, 17);
            this.rbAutoBCLarge.TabIndex = 59;
            this.rbAutoBCLarge.Text = "Big Data";
            this.rbAutoBCLarge.UseVisualStyleBackColor = true;
            // 
            // rbAutoBCOff
            // 
            this.rbAutoBCOff.AutoSize = true;
            this.rbAutoBCOff.Checked = true;
            this.rbAutoBCOff.Location = new System.Drawing.Point(31, 20);
            this.rbAutoBCOff.Name = "rbAutoBCOff";
            this.rbAutoBCOff.Size = new System.Drawing.Size(39, 17);
            this.rbAutoBCOff.TabIndex = 58;
            this.rbAutoBCOff.TabStop = true;
            this.rbAutoBCOff.Text = "Off";
            this.rbAutoBCOff.UseVisualStyleBackColor = true;
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
            this.dGrid.Location = new System.Drawing.Point(0, 493);
            this.dGrid.Name = "dGrid";
            this.dGrid.Size = new System.Drawing.Size(954, 155);
            this.dGrid.TabIndex = 28;
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
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(954, 668);
            this.Controls.Add(this.dGrid);
            this.Controls.Add(this.panelOuter);
            this.Controls.Add(this.pnlFooter);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "SocketMeister Test Server 2.0.6";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.Shown += new System.EventHandler(this.Main_Shown);
            this.pnlFooter.ResumeLayout(false);
            this.panelOuter.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.pnlDiagnostics.ResumeLayout(false);
            this.pnlSettings.ResumeLayout(false);
            this.pnlSettings.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.pnlAutoBroadcast.ResumeLayout(false);
            this.pnlAutoBroadcast.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panelOuter;
        private System.Windows.Forms.DataGridView dGrid;
        private System.Windows.Forms.Panel pnlDiagnostics;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox pnlSettings;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox cbCompressMessage;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.CheckBox cbLogInfo;
        private System.Windows.Forms.Button serviceStatusIndicator;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblMessage2Subscribers;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lblMessage1Subscribers;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblSubscriptionEndPoints;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblBroadcastRecipients;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblOutboundMessageQueue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblTotalConnectedClients;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTotalMessagesSent;
        private System.Windows.Forms.Label lblTotalMessagesReceived;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox pnlAutoBroadcast;
        private System.Windows.Forms.RadioButton rbAutoBCFast;
        private System.Windows.Forms.RadioButton rbAutoBCLarge;
        private System.Windows.Forms.RadioButton rbAutoBCOff;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeStamp;
        private System.Windows.Forms.DataGridViewTextBoxColumn Object;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClientID;
        private System.Windows.Forms.DataGridViewTextBoxColumn EndPointID;
        private System.Windows.Forms.DataGridViewTextBoxColumn MessageID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.Label lblBytesSent;
        private System.Windows.Forms.Label lblBytesReceived;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lblAbortedClients;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox tbBroadcast;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnSendM1Subs;
        private System.Windows.Forms.Button btnSendM2Subs;
        private System.Windows.Forms.Button btnBroadcast;
        //private ucThreadFarm ucThreadFarm;
        private System.Windows.Forms.Panel panel1;
    }
}

