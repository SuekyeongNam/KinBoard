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
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LHandedBtn
            // 
            this.LHandedBtn.Location = new System.Drawing.Point(121, 127);
            this.LHandedBtn.Name = "LHandedBtn";
            this.LHandedBtn.Size = new System.Drawing.Size(153, 62);
            this.LHandedBtn.TabIndex = 0;
            this.LHandedBtn.Text = "Left-Handed";
            this.LHandedBtn.UseVisualStyleBackColor = true;
            this.LHandedBtn.Click += new System.EventHandler(this.LHandedBtn_Click);
            // 
            // RHandedBtn
            // 
            this.RHandedBtn.Location = new System.Drawing.Point(289, 127);
            this.RHandedBtn.Name = "RHandedBtn";
            this.RHandedBtn.Size = new System.Drawing.Size(153, 62);
            this.RHandedBtn.TabIndex = 1;
            this.RHandedBtn.Text = "Right-Handed";
            this.RHandedBtn.UseVisualStyleBackColor = true;
            this.RHandedBtn.Click += new System.EventHandler(this.RHandedBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "label1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 215);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RHandedBtn);
            this.Controls.Add(this.LHandedBtn);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KinBoard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.KinBoard_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LHandedBtn;
        private System.Windows.Forms.Button RHandedBtn;
        private System.Windows.Forms.Label label1;
    }
}