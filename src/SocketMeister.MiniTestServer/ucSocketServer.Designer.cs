
namespace SocketMeister
{
    partial class UcSocketServer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label15 = new System.Windows.Forms.Label();
            this.cbCompressMessage = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.cbLogInfo = new System.Windows.Forms.CheckBox();
            this.serviceStatusIndicator = new System.Windows.Forms.Button();
            this.lblBytesSent = new System.Windows.Forms.Label();
            this.lblBytesReceived = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.lblTotalConnectedClients = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblTotalMessagesSent = new System.Windows.Forms.Label();
            this.lblTotalRequestsReceived = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rbAutoBCFast = new System.Windows.Forms.RadioButton();
            this.rbAutoBCLarge = new System.Windows.Forms.RadioButton();
            this.rbAutoBCOff = new System.Windows.Forms.RadioButton();
            this.pnlMain = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.nmReceiveProcessing = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nmSndTimeout = new System.Windows.Forms.NumericUpDown();
            this.btnBroadcastToSubscribers = new System.Windows.Forms.Button();
            this.lbPort = new System.Windows.Forms.Label();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmReceiveProcessing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmSndTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(9, 17);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(29, 13);
            this.label15.TabIndex = 80;
            this.label15.Text = "Port:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbCompressMessage
            // 
            this.cbCompressMessage.AutoSize = true;
            this.cbCompressMessage.Checked = true;
            this.cbCompressMessage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCompressMessage.Location = new System.Drawing.Point(12, 37);
            this.cbCompressMessage.Name = "cbCompressMessage";
            this.cbCompressMessage.Size = new System.Drawing.Size(125, 17);
            this.cbCompressMessage.TabIndex = 61;
            this.cbCompressMessage.Text = "Compress Sent Msgs";
            this.cbCompressMessage.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(91, 57);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(80, 26);
            this.btnStop.TabIndex = 62;
            this.btnStop.Text = "Stop Service";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(7, 57);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(80, 26);
            this.btnStart.TabIndex = 61;
            this.btnStart.Text = "Start Service";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // cbLogInfo
            // 
            this.cbLogInfo.AutoSize = true;
            this.cbLogInfo.Checked = true;
            this.cbLogInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLogInfo.Location = new System.Drawing.Point(40, 89);
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
            this.serviceStatusIndicator.Location = new System.Drawing.Point(9, 84);
            this.serviceStatusIndicator.Name = "serviceStatusIndicator";
            this.serviceStatusIndicator.Size = new System.Drawing.Size(25, 25);
            this.serviceStatusIndicator.TabIndex = 51;
            this.serviceStatusIndicator.UseVisualStyleBackColor = false;
            // 
            // lblBytesSent
            // 
            this.lblBytesSent.ForeColor = System.Drawing.Color.Blue;
            this.lblBytesSent.Location = new System.Drawing.Point(87, 173);
            this.lblBytesSent.Name = "lblBytesSent";
            this.lblBytesSent.Size = new System.Drawing.Size(90, 13);
            this.lblBytesSent.TabIndex = 86;
            this.lblBytesSent.Text = "0";
            this.lblBytesSent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBytesReceived
            // 
            this.lblBytesReceived.ForeColor = System.Drawing.Color.Blue;
            this.lblBytesReceived.Location = new System.Drawing.Point(90, 156);
            this.lblBytesReceived.Name = "lblBytesReceived";
            this.lblBytesReceived.Size = new System.Drawing.Size(87, 13);
            this.lblBytesReceived.TabIndex = 85;
            this.lblBytesReceived.Text = "0";
            this.lblBytesReceived.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(4, 173);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(61, 13);
            this.label16.TabIndex = 84;
            this.label16.Text = "Bytes Sent:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(4, 156);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(88, 13);
            this.label17.TabIndex = 83;
            this.label17.Text = "Bytes Received: ";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTotalConnectedClients
            // 
            this.lblTotalConnectedClients.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalConnectedClients.Location = new System.Drawing.Point(136, 140);
            this.lblTotalConnectedClients.Name = "lblTotalConnectedClients";
            this.lblTotalConnectedClients.Size = new System.Drawing.Size(41, 13);
            this.lblTotalConnectedClients.TabIndex = 72;
            this.lblTotalConnectedClients.Text = "0";
            this.lblTotalConnectedClients.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 140);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 71;
            this.label4.Text = "Connected Clients:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTotalMessagesSent
            // 
            this.lblTotalMessagesSent.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalMessagesSent.Location = new System.Drawing.Point(292, 173);
            this.lblTotalMessagesSent.Name = "lblTotalMessagesSent";
            this.lblTotalMessagesSent.Size = new System.Drawing.Size(59, 13);
            this.lblTotalMessagesSent.TabIndex = 70;
            this.lblTotalMessagesSent.Text = "0";
            this.lblTotalMessagesSent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTotalRequestsReceived
            // 
            this.lblTotalRequestsReceived.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalRequestsReceived.Location = new System.Drawing.Point(292, 156);
            this.lblTotalRequestsReceived.Name = "lblTotalRequestsReceived";
            this.lblTotalRequestsReceived.Size = new System.Drawing.Size(59, 13);
            this.lblTotalRequestsReceived.TabIndex = 69;
            this.lblTotalRequestsReceived.Text = "0";
            this.lblTotalRequestsReceived.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(185, 173);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 68;
            this.label3.Text = "Messages Sent:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(185, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 67;
            this.label2.Text = "Requests Received: ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rbAutoBCFast
            // 
            this.rbAutoBCFast.AutoSize = true;
            this.rbAutoBCFast.Location = new System.Drawing.Point(298, 118);
            this.rbAutoBCFast.Name = "rbAutoBCFast";
            this.rbAutoBCFast.Size = new System.Drawing.Size(71, 17);
            this.rbAutoBCFast.TabIndex = 60;
            this.rbAutoBCFast.Text = "Fast Data";
            this.rbAutoBCFast.UseVisualStyleBackColor = true;
            // 
            // rbAutoBCLarge
            // 
            this.rbAutoBCLarge.AutoSize = true;
            this.rbAutoBCLarge.Location = new System.Drawing.Point(217, 117);
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
            this.rbAutoBCOff.Location = new System.Drawing.Point(167, 117);
            this.rbAutoBCOff.Name = "rbAutoBCOff";
            this.rbAutoBCOff.Size = new System.Drawing.Size(39, 17);
            this.rbAutoBCOff.TabIndex = 58;
            this.rbAutoBCOff.TabStop = true;
            this.rbAutoBCOff.Text = "Off";
            this.rbAutoBCOff.UseVisualStyleBackColor = true;
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.label6);
            this.pnlMain.Controls.Add(this.nmReceiveProcessing);
            this.pnlMain.Controls.Add(this.label1);
            this.pnlMain.Controls.Add(this.nmSndTimeout);
            this.pnlMain.Controls.Add(this.btnBroadcastToSubscribers);
            this.pnlMain.Controls.Add(this.cbCompressMessage);
            this.pnlMain.Controls.Add(this.lbPort);
            this.pnlMain.Controls.Add(this.btnSendMessage);
            this.pnlMain.Controls.Add(this.lblBytesSent);
            this.pnlMain.Controls.Add(this.label15);
            this.pnlMain.Controls.Add(this.rbAutoBCFast);
            this.pnlMain.Controls.Add(this.lblBytesReceived);
            this.pnlMain.Controls.Add(this.label5);
            this.pnlMain.Controls.Add(this.label16);
            this.pnlMain.Controls.Add(this.rbAutoBCLarge);
            this.pnlMain.Controls.Add(this.label17);
            this.pnlMain.Controls.Add(this.cbLogInfo);
            this.pnlMain.Controls.Add(this.lblTotalConnectedClients);
            this.pnlMain.Controls.Add(this.rbAutoBCOff);
            this.pnlMain.Controls.Add(this.label4);
            this.pnlMain.Controls.Add(this.lblTotalMessagesSent);
            this.pnlMain.Controls.Add(this.btnStop);
            this.pnlMain.Controls.Add(this.lblTotalRequestsReceived);
            this.pnlMain.Controls.Add(this.label3);
            this.pnlMain.Controls.Add(this.btnStart);
            this.pnlMain.Controls.Add(this.label2);
            this.pnlMain.Controls.Add(this.serviceStatusIndicator);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(6);
            this.pnlMain.Size = new System.Drawing.Size(384, 198);
            this.pnlMain.TabIndex = 75;
            this.pnlMain.TabStop = false;
            this.pnlMain.Text = "Socket Server 1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(168, 37);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(130, 13);
            this.label6.TabIndex = 92;
            this.label6.Text = "Processing ms (Rcv Msg):";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nmReceiveProcessing
            // 
            this.nmReceiveProcessing.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.nmReceiveProcessing.Increment = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nmReceiveProcessing.Location = new System.Drawing.Point(306, 35);
            this.nmReceiveProcessing.Maximum = new decimal(new int[] {
            120000,
            0,
            0,
            0});
            this.nmReceiveProcessing.Name = "nmReceiveProcessing";
            this.nmReceiveProcessing.Size = new System.Drawing.Size(61, 20);
            this.nmReceiveProcessing.TabIndex = 91;
            this.nmReceiveProcessing.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nmReceiveProcessing.ValueChanged += new System.EventHandler(this.NmReceiveProcessing_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(167, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 13);
            this.label1.TabIndex = 90;
            this.label1.Text = "Timeout ms (Send Msg):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nmSndTimeout
            // 
            this.nmSndTimeout.Increment = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nmSndTimeout.Location = new System.Drawing.Point(306, 15);
            this.nmSndTimeout.Maximum = new decimal(new int[] {
            120000,
            0,
            0,
            0});
            this.nmSndTimeout.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nmSndTimeout.Name = "nmSndTimeout";
            this.nmSndTimeout.Size = new System.Drawing.Size(61, 20);
            this.nmSndTimeout.TabIndex = 89;
            this.nmSndTimeout.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nmSndTimeout.ValueChanged += new System.EventHandler(this.NmSndTimeout_ValueChanged);
            // 
            // btnBroadcastToSubscribers
            // 
            this.btnBroadcastToSubscribers.Enabled = false;
            this.btnBroadcastToSubscribers.Location = new System.Drawing.Point(177, 84);
            this.btnBroadcastToSubscribers.Name = "btnBroadcastToSubscribers";
            this.btnBroadcastToSubscribers.Size = new System.Drawing.Size(193, 26);
            this.btnBroadcastToSubscribers.TabIndex = 88;
            this.btnBroadcastToSubscribers.Text = "Broadcast to Subscribers";
            this.btnBroadcastToSubscribers.UseVisualStyleBackColor = true;
            this.btnBroadcastToSubscribers.Click += new System.EventHandler(this.BtnBroadcastToSubscribers_Click);
            // 
            // lbPort
            // 
            this.lbPort.AutoSize = true;
            this.lbPort.ForeColor = System.Drawing.Color.Blue;
            this.lbPort.Location = new System.Drawing.Point(39, 17);
            this.lbPort.Name = "lbPort";
            this.lbPort.Size = new System.Drawing.Size(31, 13);
            this.lbPort.TabIndex = 83;
            this.lbPort.Text = "4505";
            this.lbPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Enabled = false;
            this.btnSendMessage.Location = new System.Drawing.Point(177, 57);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(193, 26);
            this.btnSendMessage.TabIndex = 87;
            this.btnSendMessage.Text = "Send Message (Wait for Response)";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.BtnSendMessage_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(153, 13);
            this.label5.TabIndex = 81;
            this.label5.Text = "Automatic Message Generator:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UcSocketServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "UcSocketServer";
            this.Size = new System.Drawing.Size(384, 198);
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmReceiveProcessing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmSndTimeout)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lbPort;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.CheckBox cbCompressMessage;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.CheckBox cbLogInfo;
        private System.Windows.Forms.Button serviceStatusIndicator;
        private System.Windows.Forms.Label lblBytesSent;
        private System.Windows.Forms.Label lblBytesReceived;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lblTotalConnectedClients;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTotalMessagesSent;
        private System.Windows.Forms.Label lblTotalRequestsReceived;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rbAutoBCFast;
        private System.Windows.Forms.RadioButton rbAutoBCLarge;
        private System.Windows.Forms.RadioButton rbAutoBCOff;
        private System.Windows.Forms.GroupBox pnlMain;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.Button btnBroadcastToSubscribers;
        private System.Windows.Forms.NumericUpDown nmSndTimeout;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nmReceiveProcessing;
        private System.Windows.Forms.Label label1;
    }
}
