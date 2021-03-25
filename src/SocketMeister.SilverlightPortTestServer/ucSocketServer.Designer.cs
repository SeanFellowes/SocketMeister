
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
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.serviceStatusIndicator = new System.Windows.Forms.Button();
            this.lbPort = new System.Windows.Forms.Label();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(5, 5);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(32, 15);
            this.label15.TabIndex = 80;
            this.label15.Text = "Port:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(171, 0);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(48, 24);
            this.btnStop.TabIndex = 62;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(121, 0);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(48, 24);
            this.btnStart.TabIndex = 61;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // serviceStatusIndicator
            // 
            this.serviceStatusIndicator.BackColor = System.Drawing.Color.Red;
            this.serviceStatusIndicator.Enabled = false;
            this.serviceStatusIndicator.Location = new System.Drawing.Point(84, 0);
            this.serviceStatusIndicator.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.serviceStatusIndicator.Name = "serviceStatusIndicator";
            this.serviceStatusIndicator.Size = new System.Drawing.Size(24, 24);
            this.serviceStatusIndicator.TabIndex = 51;
            this.serviceStatusIndicator.UseVisualStyleBackColor = false;
            // 
            // lbPort
            // 
            this.lbPort.AutoSize = true;
            this.lbPort.ForeColor = System.Drawing.Color.Blue;
            this.lbPort.Location = new System.Drawing.Point(45, 5);
            this.lbPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbPort.Name = "lbPort";
            this.lbPort.Size = new System.Drawing.Size(31, 15);
            this.lbPort.TabIndex = 83;
            this.lbPort.Text = "4502";
            this.lbPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Enabled = false;
            this.btnSendMessage.Location = new System.Drawing.Point(227, 0);
            this.btnSendMessage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(96, 24);
            this.btnSendMessage.TabIndex = 87;
            this.btnSendMessage.Text = "Send Message";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.BtnSendMessage_Click);
            // 
            // ucSocketServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnSendMessage);
            this.Controls.Add(this.lbPort);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.serviceStatusIndicator);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ucSocketServer";
            this.Size = new System.Drawing.Size(349, 29);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lbPort;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button serviceStatusIndicator;
        private System.Windows.Forms.Button btnSendMessage;
    }
}
