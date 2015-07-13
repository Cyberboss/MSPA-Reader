namespace Reader_UI
{
    partial class DatabaseLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseLogin));
            this.dataSourceInput = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ipPathLabel = new System.Windows.Forms.Label();
            this.ipInput = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.usernameInput = new System.Windows.Forms.TextBox();
            this.passwordInput = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.resetDatabase = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.dbPathSelect = new System.Windows.Forms.Button();
            this.savePassword = new System.Windows.Forms.CheckBox();
            this.saveUsername = new System.Windows.Forms.CheckBox();
            this.dbNameLabel = new System.Windows.Forms.Label();
            this.databaseNameInput = new System.Windows.Forms.TextBox();
            this.portSelector = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.portSelector)).BeginInit();
            this.SuspendLayout();
            // 
            // dataSourceInput
            // 
            this.dataSourceInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dataSourceInput.FormattingEnabled = true;
            this.dataSourceInput.Location = new System.Drawing.Point(112, 12);
            this.dataSourceInput.Name = "dataSourceInput";
            this.dataSourceInput.Size = new System.Drawing.Size(285, 21);
            this.dataSourceInput.TabIndex = 0;
            this.dataSourceInput.SelectedIndexChanged += new System.EventHandler(this.dataSourceInput_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Database Type";
            // 
            // ipPathLabel
            // 
            this.ipPathLabel.AutoSize = true;
            this.ipPathLabel.Location = new System.Drawing.Point(12, 100);
            this.ipPathLabel.Name = "ipPathLabel";
            this.ipPathLabel.Size = new System.Drawing.Size(86, 13);
            this.ipPathLabel.TabIndex = 2;
            this.ipPathLabel.Text = "IP Address : Port";
            // 
            // ipInput
            // 
            this.ipInput.Location = new System.Drawing.Point(112, 97);
            this.ipInput.Name = "ipInput";
            this.ipInput.Size = new System.Drawing.Size(285, 20);
            this.ipInput.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Username";
            // 
            // usernameInput
            // 
            this.usernameInput.Location = new System.Drawing.Point(112, 142);
            this.usernameInput.Name = "usernameInput";
            this.usernameInput.Size = new System.Drawing.Size(285, 20);
            this.usernameInput.TabIndex = 5;
            // 
            // passwordInput
            // 
            this.passwordInput.Location = new System.Drawing.Point(112, 187);
            this.passwordInput.Name = "passwordInput";
            this.passwordInput.Size = new System.Drawing.Size(285, 20);
            this.passwordInput.TabIndex = 6;
            this.passwordInput.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 185);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Password";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(165, 222);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "Connect";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // resetDatabase
            // 
            this.resetDatabase.AutoSize = true;
            this.resetDatabase.Location = new System.Drawing.Point(15, 226);
            this.resetDatabase.Name = "resetDatabase";
            this.resetDatabase.Size = new System.Drawing.Size(103, 17);
            this.resetDatabase.TabIndex = 10;
            this.resetDatabase.Text = "Reset Database";
            this.resetDatabase.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(273, 226);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(90, 17);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "Open Reader";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(382, 226);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(94, 17);
            this.checkBox2.TabIndex = 12;
            this.checkBox2.Text = "Open Archiver";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // dbPathSelect
            // 
            this.dbPathSelect.Location = new System.Drawing.Point(403, 55);
            this.dbPathSelect.Name = "dbPathSelect";
            this.dbPathSelect.Size = new System.Drawing.Size(31, 20);
            this.dbPathSelect.TabIndex = 13;
            this.dbPathSelect.Text = "...";
            this.dbPathSelect.UseVisualStyleBackColor = true;
            this.dbPathSelect.Visible = false;
            this.dbPathSelect.Click += new System.EventHandler(this.dbPathSelect_Click);
            // 
            // savePassword
            // 
            this.savePassword.AutoSize = true;
            this.savePassword.Location = new System.Drawing.Point(403, 190);
            this.savePassword.Name = "savePassword";
            this.savePassword.Size = new System.Drawing.Size(77, 17);
            this.savePassword.TabIndex = 14;
            this.savePassword.Text = "Remember";
            this.savePassword.UseVisualStyleBackColor = true;
            this.savePassword.CheckedChanged += new System.EventHandler(this.savePassword_CheckedChanged);
            // 
            // saveUsername
            // 
            this.saveUsername.AutoSize = true;
            this.saveUsername.Location = new System.Drawing.Point(403, 145);
            this.saveUsername.Name = "saveUsername";
            this.saveUsername.Size = new System.Drawing.Size(77, 17);
            this.saveUsername.TabIndex = 15;
            this.saveUsername.Text = "Remember";
            this.saveUsername.UseVisualStyleBackColor = true;
            this.saveUsername.CheckedChanged += new System.EventHandler(this.saveUsername_CheckedChanged);
            // 
            // dbNameLabel
            // 
            this.dbNameLabel.AutoSize = true;
            this.dbNameLabel.Location = new System.Drawing.Point(12, 55);
            this.dbNameLabel.Name = "dbNameLabel";
            this.dbNameLabel.Size = new System.Drawing.Size(84, 13);
            this.dbNameLabel.TabIndex = 16;
            this.dbNameLabel.Text = "Database Name";
            // 
            // databaseNameInput
            // 
            this.databaseNameInput.Location = new System.Drawing.Point(112, 55);
            this.databaseNameInput.Name = "databaseNameInput";
            this.databaseNameInput.Size = new System.Drawing.Size(285, 20);
            this.databaseNameInput.TabIndex = 17;
            // 
            // portSelector
            // 
            this.portSelector.Location = new System.Drawing.Point(403, 97);
            this.portSelector.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.portSelector.Name = "portSelector";
            this.portSelector.Size = new System.Drawing.Size(84, 20);
            this.portSelector.TabIndex = 18;
            this.portSelector.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DatabaseLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 257);
            this.Controls.Add(this.portSelector);
            this.Controls.Add(this.databaseNameInput);
            this.Controls.Add(this.dbNameLabel);
            this.Controls.Add(this.saveUsername);
            this.Controls.Add(this.savePassword);
            this.Controls.Add(this.dbPathSelect);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.resetDatabase);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.passwordInput);
            this.Controls.Add(this.usernameInput);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ipInput);
            this.Controls.Add(this.ipPathLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataSourceInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DatabaseLogin";
            this.Text = "Database Login";
            ((System.ComponentModel.ISupportInitialize)(this.portSelector)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox dataSourceInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ipPathLabel;
        private System.Windows.Forms.TextBox ipInput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox usernameInput;
        private System.Windows.Forms.TextBox passwordInput;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.CheckBox resetDatabase;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button dbPathSelect;
        private System.Windows.Forms.CheckBox savePassword;
        private System.Windows.Forms.CheckBox saveUsername;
        private System.Windows.Forms.Label dbNameLabel;
        private System.Windows.Forms.TextBox databaseNameInput;
        private System.Windows.Forms.NumericUpDown portSelector;
    }
}