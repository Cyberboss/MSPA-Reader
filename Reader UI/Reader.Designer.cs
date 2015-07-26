namespace Reader_UI
{
    partial class Reader
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
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                if (trick != null)
                    trick.Dispose();
                if (openbound != null)
                    openbound.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Reader));
            this.openArchiver = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.jumpButton = new System.Windows.Forms.Button();
            this.goBack = new System.Windows.Forms.Button();
            this.loadButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.autoSave = new System.Windows.Forms.CheckBox();
            this.exitButton = new System.Windows.Forms.Button();
            this.minimizeButton = new System.Windows.Forms.Button();
            this.flashWarning = new System.Windows.Forms.Label();
            this.helpButton = new System.Windows.Forms.Button();
            this.startOverButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.uiToggleButton = new System.Windows.Forms.Button();
            this.toggleFullscreen = new System.Windows.Forms.Button();
            this.chapterSelector = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // openArchiver
            // 
            this.openArchiver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openArchiver.Location = new System.Drawing.Point(725, 252);
            this.openArchiver.Name = "openArchiver";
            this.openArchiver.Size = new System.Drawing.Size(101, 31);
            this.openArchiver.TabIndex = 0;
            this.openArchiver.Text = "Open Archiver";
            this.openArchiver.UseVisualStyleBackColor = true;
            this.openArchiver.Click += new System.EventHandler(this.openArchiver_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.numericUpDown1.Location = new System.Drawing.Point(112, 300);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1901,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(60, 20);
            this.numericUpDown1.TabIndex = 1;
            this.numericUpDown1.Value = new decimal(new int[] {
            1901,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // jumpButton
            // 
            this.jumpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.jumpButton.Location = new System.Drawing.Point(8, 269);
            this.jumpButton.Name = "jumpButton";
            this.jumpButton.Size = new System.Drawing.Size(118, 25);
            this.jumpButton.TabIndex = 2;
            this.jumpButton.Text = "Jump";
            this.jumpButton.UseVisualStyleBackColor = true;
            this.jumpButton.Click += new System.EventHandler(this.jumpPage_Click);
            // 
            // goBack
            // 
            this.goBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.goBack.Location = new System.Drawing.Point(132, 269);
            this.goBack.Name = "goBack";
            this.goBack.Size = new System.Drawing.Size(118, 25);
            this.goBack.TabIndex = 5;
            this.goBack.Text = "Go Back";
            this.goBack.UseVisualStyleBackColor = true;
            this.goBack.Click += new System.EventHandler(this.goBack_Click);
            // 
            // loadButton
            // 
            this.loadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.loadButton.Location = new System.Drawing.Point(8, 238);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(118, 25);
            this.loadButton.TabIndex = 6;
            this.loadButton.Text = "Load Game";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.saveButton.Location = new System.Drawing.Point(8, 207);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(118, 25);
            this.saveButton.TabIndex = 7;
            this.saveButton.Text = "Save Game";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // autoSave
            // 
            this.autoSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.autoSave.AutoSize = true;
            this.autoSave.Location = new System.Drawing.Point(178, 302);
            this.autoSave.Name = "autoSave";
            this.autoSave.Size = new System.Drawing.Size(76, 17);
            this.autoSave.TabIndex = 8;
            this.autoSave.Text = "Auto-Save";
            this.autoSave.UseVisualStyleBackColor = true;
            this.autoSave.CheckedChanged += new System.EventHandler(this.autoSave_CheckedChanged);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Location = new System.Drawing.Point(799, 11);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(27, 21);
            this.exitButton.TabIndex = 9;
            this.exitButton.Text = "X";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // minimizeButton
            // 
            this.minimizeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.minimizeButton.Location = new System.Drawing.Point(766, 11);
            this.minimizeButton.Name = "minimizeButton";
            this.minimizeButton.Size = new System.Drawing.Size(27, 21);
            this.minimizeButton.TabIndex = 10;
            this.minimizeButton.Text = "_";
            this.minimizeButton.UseVisualStyleBackColor = true;
            this.minimizeButton.Click += new System.EventHandler(this.minimizeButton_Click_1);
            // 
            // flashWarning
            // 
            this.flashWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.flashWarning.AutoSize = true;
            this.flashWarning.ForeColor = System.Drawing.Color.Red;
            this.flashWarning.Location = new System.Drawing.Point(625, 252);
            this.flashWarning.MaximumSize = new System.Drawing.Size(100, 0);
            this.flashWarning.Name = "flashWarning";
            this.flashWarning.Size = new System.Drawing.Size(94, 65);
            this.flashWarning.TabIndex = 11;
            this.flashWarning.Text = "PAGE CONTAINS FLASH. KEYBOARD NAVIGATION DISABLED";
            this.flashWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.flashWarning.Visible = false;
            // 
            // helpButton
            // 
            this.helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.helpButton.Location = new System.Drawing.Point(725, 215);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(101, 31);
            this.helpButton.TabIndex = 12;
            this.helpButton.Text = "Help and About";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // startOverButton
            // 
            this.startOverButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.startOverButton.Location = new System.Drawing.Point(132, 238);
            this.startOverButton.Name = "startOverButton";
            this.startOverButton.Size = new System.Drawing.Size(118, 25);
            this.startOverButton.TabIndex = 13;
            this.startOverButton.Text = "Start Over";
            this.startOverButton.UseVisualStyleBackColor = true;
            this.startOverButton.Click += new System.EventHandler(this.startOverButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deleteButton.Location = new System.Drawing.Point(132, 207);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(118, 25);
            this.deleteButton.TabIndex = 14;
            this.deleteButton.Text = "Delete Game";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // uiToggleButton
            // 
            this.uiToggleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.uiToggleButton.Location = new System.Drawing.Point(725, 290);
            this.uiToggleButton.Name = "uiToggleButton";
            this.uiToggleButton.Size = new System.Drawing.Size(101, 31);
            this.uiToggleButton.TabIndex = 16;
            this.uiToggleButton.Text = "Hide UI";
            this.uiToggleButton.UseVisualStyleBackColor = true;
            this.uiToggleButton.Click += new System.EventHandler(this.uiToggleButton_Click);
            // 
            // toggleFullscreen
            // 
            this.toggleFullscreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.toggleFullscreen.Location = new System.Drawing.Point(725, 178);
            this.toggleFullscreen.Name = "toggleFullscreen";
            this.toggleFullscreen.Size = new System.Drawing.Size(101, 31);
            this.toggleFullscreen.TabIndex = 17;
            this.toggleFullscreen.Text = "Toggle Fullscreen";
            this.toggleFullscreen.UseVisualStyleBackColor = true;
            this.toggleFullscreen.Click += new System.EventHandler(this.toggleFullscreen_Click);
            // 
            // chapterSelector
            // 
            this.chapterSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chapterSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.chapterSelector.DropDownWidth = 350;
            this.chapterSelector.FormattingEnabled = true;
            this.chapterSelector.Location = new System.Drawing.Point(8, 300);
            this.chapterSelector.Name = "chapterSelector";
            this.chapterSelector.Size = new System.Drawing.Size(98, 21);
            this.chapterSelector.TabIndex = 18;
            this.chapterSelector.SelectedIndexChanged += new System.EventHandler(this.chapterSelector_SelectedIndexChanged);
            // 
            // Reader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(838, 329);
            this.Controls.Add(this.chapterSelector);
            this.Controls.Add(this.toggleFullscreen);
            this.Controls.Add(this.uiToggleButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.startOverButton);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.flashWarning);
            this.Controls.Add(this.minimizeButton);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.autoSave);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.loadButton);
            this.Controls.Add(this.goBack);
            this.Controls.Add(this.jumpButton);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.openArchiver);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Reader";
            this.Text = "MS Paint Adventures";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button openArchiver;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button jumpButton;
        private System.Windows.Forms.Button goBack;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.CheckBox autoSave;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button minimizeButton;
        private System.Windows.Forms.Label flashWarning;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button startOverButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button uiToggleButton;
        private System.Windows.Forms.Button toggleFullscreen;
        private System.Windows.Forms.ComboBox chapterSelector;

    }
}

