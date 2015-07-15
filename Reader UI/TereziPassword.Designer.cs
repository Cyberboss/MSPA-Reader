namespace Reader_UI
{
    partial class TereziPassword
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
            tms.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.submitButton = new System.Windows.Forms.Button();
            this.hintButton = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.hiddenCancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(-31, 14);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(222, 128);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 10.5F);
            this.label1.Location = new System.Drawing.Point(220, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "3NT3R P4SSWORD: ";
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Verdana", 10.5F);
            this.textBox1.Location = new System.Drawing.Point(188, 42);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(141, 25);
            this.textBox1.TabIndex = 2;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox1.UseSystemPasswordChar = true;
            // 
            // submitButton
            // 
            this.submitButton.Font = new System.Drawing.Font("Verdana", 10.5F);
            this.submitButton.Location = new System.Drawing.Point(335, 42);
            this.submitButton.Name = "submitButton";
            this.submitButton.Size = new System.Drawing.Size(75, 25);
            this.submitButton.TabIndex = 3;
            this.submitButton.Text = "Submit";
            this.submitButton.UseVisualStyleBackColor = true;
            // 
            // hintButton
            // 
            this.hintButton.Font = new System.Drawing.Font("Verdana", 10.5F);
            this.hintButton.Location = new System.Drawing.Point(218, 112);
            this.hintButton.Name = "hintButton";
            this.hintButton.Size = new System.Drawing.Size(153, 28);
            this.hintButton.TabIndex = 4;
            this.hintButton.Text = "P4SSWORD H1NT";
            this.hintButton.UseVisualStyleBackColor = true;
            this.hintButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Font = new System.Drawing.Font("Verdana", 10.5F);
            this.richTextBox1.Location = new System.Drawing.Point(180, 81);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(230, 25);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = "<- OR GO B4CK!!!";
            // 
            // hiddenCancelButton
            // 
            this.hiddenCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.hiddenCancelButton.Location = new System.Drawing.Point(460, 103);
            this.hiddenCancelButton.Name = "hiddenCancelButton";
            this.hiddenCancelButton.Size = new System.Drawing.Size(140, 23);
            this.hiddenCancelButton.TabIndex = 6;
            this.hiddenCancelButton.Text = "button1";
            this.hiddenCancelButton.UseVisualStyleBackColor = true;
            this.hiddenCancelButton.Click += new System.EventHandler(this.hiddenCancelButton_Click);
            // 
            // TereziPassword
            // 
            this.AcceptButton = this.submitButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.hiddenCancelButton;
            this.ClientSize = new System.Drawing.Size(440, 153);
            this.Controls.Add(this.hiddenCancelButton);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.hintButton);
            this.Controls.Add(this.submitButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TereziPassword";
            this.ShowIcon = false;
            this.Text = "[???????]";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button submitButton;
        private System.Windows.Forms.Button hintButton;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button hiddenCancelButton;
    }
}