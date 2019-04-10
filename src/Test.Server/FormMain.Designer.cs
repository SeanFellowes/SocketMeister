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
            this.SuspendLayout();
            // 
            // server1
            // 
            this.server1.Location = new System.Drawing.Point(12, 12);
            this.server1.Name = "server1";
            this.server1.Port = 4505;
            this.server1.Size = new System.Drawing.Size(511, 61);
            this.server1.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 202);
            this.Controls.Add(this.server1);
            this.Name = "FormMain";
            this.Text = "SocketMeister Server Tester";
            this.ResumeLayout(false);

        }

        #endregion

        private Server server1;
    }
}

