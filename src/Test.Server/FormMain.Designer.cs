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
            this.server1 = new Test.Server.Server();
            this.server2 = new Test.Server.Server();
            this.SuspendLayout();
            // 
            // server1
            // 
            this.server1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.server1.Dock = System.Windows.Forms.DockStyle.Top;
            this.server1.Location = new System.Drawing.Point(0, 0);
            this.server1.Margin = new System.Windows.Forms.Padding(2);
            this.server1.Name = "server1";
            this.server1.Port = 4505;
            this.server1.ServerType = Test.Server.ServerType.PolicyServer;
            this.server1.Size = new System.Drawing.Size(404, 43);
            this.server1.TabIndex = 0;
            // 
            // server2
            // 
            this.server2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.server2.Dock = System.Windows.Forms.DockStyle.Top;
            this.server2.Location = new System.Drawing.Point(0, 43);
            this.server2.Margin = new System.Windows.Forms.Padding(2);
            this.server2.Name = "server2";
            this.server2.Port = 4505;
            this.server2.ServerType = Test.Server.ServerType.SocketServer;
            this.server2.Size = new System.Drawing.Size(404, 43);
            this.server2.TabIndex = 1;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 164);
            this.Controls.Add(this.server2);
            this.Controls.Add(this.server1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FormMain";
            this.Text = "SocketMeister Server Tester";
            this.ResumeLayout(false);

        }

        #endregion

        private Server server1;
        private Server server2;
    }
}

