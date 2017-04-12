namespace KinBoard
{
    partial class MainForm
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
            this.LHandedBtn = new System.Windows.Forms.Button();
            this.RHandedBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LHandedBtn
            // 
            this.LHandedBtn.Location = new System.Drawing.Point(10, 27);
            this.LHandedBtn.Name = "LHandedBtn";
            this.LHandedBtn.Size = new System.Drawing.Size(153, 62);
            this.LHandedBtn.TabIndex = 0;
            this.LHandedBtn.Text = "Left-Handed";
            this.LHandedBtn.UseVisualStyleBackColor = true;
            this.LHandedBtn.Click += new System.EventHandler(this.LHandedBtn_Click);
            // 
            // RHandedBtn
            // 
            this.RHandedBtn.Location = new System.Drawing.Point(178, 27);
            this.RHandedBtn.Name = "RHandedBtn";
            this.RHandedBtn.Size = new System.Drawing.Size(153, 62);
            this.RHandedBtn.TabIndex = 1;
            this.RHandedBtn.Text = "Right-Handed";
            this.RHandedBtn.UseVisualStyleBackColor = true;
            this.RHandedBtn.Click += new System.EventHandler(this.RHandedBtn_Click);
            // 
            // KinBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 112);
            this.Controls.Add(this.RHandedBtn);
            this.Controls.Add(this.LHandedBtn);
            this.Name = "KinBoard";
            this.Text = "KinBoard";
            this.Load += new System.EventHandler(this.KinBoard_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LHandedBtn;
        private System.Windows.Forms.Button RHandedBtn;
    }
}