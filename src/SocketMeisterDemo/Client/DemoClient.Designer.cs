
namespace SocketMeister
{
    partial class ClientForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnGetTimezoneDisplayName = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnGetTimezoneDisplayName
            // 
            this.btnGetTimezoneDisplayName.Location = new System.Drawing.Point(85, 62);
            this.btnGetTimezoneDisplayName.Name = "btnGetTimezoneDisplayName";
            this.btnGetTimezoneDisplayName.Size = new System.Drawing.Size(191, 51);
            this.btnGetTimezoneDisplayName.TabIndex = 0;
            this.btnGetTimezoneDisplayName.Text = "GetTimezoneDisplayName";
            this.btnGetTimezoneDisplayName.UseVisualStyleBackColor = true;
            this.btnGetTimezoneDisplayName.Click += new System.EventHandler(this.BtnGetTimezoneDisplayName_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 328);
            this.Controls.Add(this.btnGetTimezoneDisplayName);
            this.Name = "ClientForm";
            this.Text = "Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnGetTimezoneDisplayName;
    }
}

