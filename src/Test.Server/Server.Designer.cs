namespace Test.Server
{
    partial class Server
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.LabelPort = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.StartStopButton = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.StatusIndicator = new System.Windows.Forms.Panel();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(6);
            this.panel1.Size = new System.Drawing.Size(482, 53);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.LabelPort);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(336, 6);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(6);
            this.panel2.Size = new System.Drawing.Size(140, 41);
            this.panel2.TabIndex = 0;
            // 
            // LabelPort
            // 
            this.LabelPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelPort.Location = new System.Drawing.Point(6, 6);
            this.LabelPort.Name = "LabelPort";
            this.LabelPort.Size = new System.Drawing.Size(128, 29);
            this.LabelPort.TabIndex = 0;
            this.LabelPort.Text = "Port 65000";
            this.LabelPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.LabelStatus);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Controls.Add(this.StartStopButton);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(6, 6);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(6);
            this.panel3.Size = new System.Drawing.Size(276, 41);
            this.panel3.TabIndex = 1;
            // 
            // StartStopButton
            // 
            this.StartStopButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.StartStopButton.Location = new System.Drawing.Point(6, 6);
            this.StartStopButton.Name = "StartStopButton";
            this.StartStopButton.Size = new System.Drawing.Size(86, 29);
            this.StartStopButton.TabIndex = 0;
            this.StartStopButton.Text = "Start";
            this.StartStopButton.UseVisualStyleBackColor = true;
            this.StartStopButton.Click += new System.EventHandler(this.StartStopButton_Click);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.StatusIndicator);
            this.panel4.Location = new System.Drawing.Point(97, 6);
            this.panel4.Margin = new System.Windows.Forms.Padding(12);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(3);
            this.panel4.Size = new System.Drawing.Size(32, 30);
            this.panel4.TabIndex = 1;
            // 
            // StatusIndicator
            // 
            this.StatusIndicator.BackColor = System.Drawing.Color.Red;
            this.StatusIndicator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.StatusIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusIndicator.Location = new System.Drawing.Point(3, 3);
            this.StatusIndicator.Name = "StatusIndicator";
            this.StatusIndicator.Padding = new System.Windows.Forms.Padding(6);
            this.StatusIndicator.Size = new System.Drawing.Size(24, 22);
            this.StatusIndicator.TabIndex = 2;
            // 
            // LabelStatus
            // 
            this.LabelStatus.Location = new System.Drawing.Point(131, 6);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(139, 29);
            this.LabelStatus.TabIndex = 2;
            this.LabelStatus.Text = "Starting";
            this.LabelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "Server";
            this.Size = new System.Drawing.Size(482, 168);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label LabelStatus;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel StatusIndicator;
        private System.Windows.Forms.Button StartStopButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label LabelPort;
    }
}
