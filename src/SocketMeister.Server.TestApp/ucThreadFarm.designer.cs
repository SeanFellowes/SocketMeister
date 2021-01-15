namespace SocketMeister1.Server.TestApp
{
    partial class ucThreadFarm
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
            this.CH1 = new System.Windows.Forms.Label();
            this.CH2 = new System.Windows.Forms.Label();
            this.CH3 = new System.Windows.Forms.Label();
            this.CH7 = new System.Windows.Forms.Label();
            this.CH6 = new System.Windows.Forms.Label();
            this.CH4 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.CH4)).BeginInit();
            this.SuspendLayout();
            // 
            // CH1
            // 
            this.CH1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CH1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CH1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CH1.ForeColor = System.Drawing.Color.White;
            this.CH1.Location = new System.Drawing.Point(0, 0);
            this.CH1.Name = "CH1";
            this.CH1.Size = new System.Drawing.Size(33, 23);
            this.CH1.TabIndex = 0;
            this.CH1.Text = "#";
            this.CH1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CH2
            // 
            this.CH2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CH2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CH2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CH2.ForeColor = System.Drawing.Color.White;
            this.CH2.Location = new System.Drawing.Point(36, 0);
            this.CH2.Name = "CH2";
            this.CH2.Size = new System.Drawing.Size(162, 23);
            this.CH2.TabIndex = 1;
            this.CH2.Text = "Description";
            this.CH2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CH3
            // 
            this.CH3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CH3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CH3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CH3.ForeColor = System.Drawing.Color.White;
            this.CH3.Location = new System.Drawing.Point(201, 0);
            this.CH3.Name = "CH3";
            this.CH3.Size = new System.Drawing.Size(57, 23);
            this.CH3.TabIndex = 2;
            this.CH3.Text = "Running";
            this.CH3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CH7
            // 
            this.CH7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CH7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CH7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CH7.ForeColor = System.Drawing.Color.White;
            this.CH7.Location = new System.Drawing.Point(468, 0);
            this.CH7.Name = "CH7";
            this.CH7.Size = new System.Drawing.Size(172, 23);
            this.CH7.TabIndex = 5;
            this.CH7.Text = "Current Action";
            this.CH7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CH6
            // 
            this.CH6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CH6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CH6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CH6.ForeColor = System.Drawing.Color.White;
            this.CH6.Location = new System.Drawing.Point(402, 0);
            this.CH6.Name = "CH6";
            this.CH6.Size = new System.Drawing.Size(44, 23);
            this.CH6.TabIndex = 6;
            this.CH6.Text = "Errors";
            this.CH6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CH4
            // 
            this.CH4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CH4.BackgroundImage = global::SocketMeister1.Server.TestApp.Properties.Resources.gear;
            this.CH4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.CH4.Location = new System.Drawing.Point(271, 0);
            this.CH4.Margin = new System.Windows.Forms.Padding(0);
            this.CH4.Name = "CH4";
            this.CH4.Size = new System.Drawing.Size(23, 23);
            this.CH4.TabIndex = 7;
            this.CH4.TabStop = false;
            // 
            // ucThreadFarm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.CH4);
            this.Controls.Add(this.CH6);
            this.Controls.Add(this.CH7);
            this.Controls.Add(this.CH3);
            this.Controls.Add(this.CH2);
            this.Controls.Add(this.CH1);
            this.Name = "ucThreadFarm";
            this.Size = new System.Drawing.Size(643, 258);
            this.Resize += new System.EventHandler(this.ucThreadFarm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.CH4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label CH1;
        private System.Windows.Forms.Label CH2;
        private System.Windows.Forms.Label CH3;
        private System.Windows.Forms.Label CH7;
        private System.Windows.Forms.Label CH6;
        private System.Windows.Forms.PictureBox CH4;
    }
}
