
namespace SocketMeisterDemo
{
    partial class SimpleServer
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
            this.btnSendMsgToAllClients = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSendMsgToAllClients
            // 
            this.btnSendMsgToAllClients.Location = new System.Drawing.Point(12, 17);
            this.btnSendMsgToAllClients.Name = "btnSendMsgToAllClients";
            this.btnSendMsgToAllClients.Size = new System.Drawing.Size(371, 33);
            this.btnSendMsgToAllClients.TabIndex = 0;
            this.btnSendMsgToAllClients.Text = "Send Msg to All Clients";
            this.btnSendMsgToAllClients.UseVisualStyleBackColor = true;
            this.btnSendMsgToAllClients.Click += new System.EventHandler(this.btnSendMsgToAllClients_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 56);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(371, 33);
            this.button1.TabIndex = 1;
            this.button1.Text = "Send Msg to All \'Customer Master Data Changes\' Subscribers";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // SimpleServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 105);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnSendMsgToAllClients);
            this.Name = "SimpleServer";
            this.Text = "Simple Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSendMsgToAllClients;
        private System.Windows.Forms.Button button1;
    }
}

