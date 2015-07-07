namespace Reader_UI
{
    partial class ArchiverWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArchiverWindow));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.logOutput = new System.Windows.Forms.TextBox();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.openReader = new System.Windows.Forms.Button();
            this.updateButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // logOutput
            // 
            resources.ApplyResources(this.logOutput, "logOutput");
            this.logOutput.Name = "logOutput";
            this.logOutput.ReadOnly = true;
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.worker_DoWork);
            // 
            // openReader
            // 
            resources.ApplyResources(this.openReader, "openReader");
            this.openReader.Name = "openReader";
            this.openReader.UseVisualStyleBackColor = true;
            this.openReader.Click += new System.EventHandler(this.openReader_Click);
            // 
            // updateButton
            // 
            resources.ApplyResources(this.updateButton, "updateButton");
            this.updateButton.Name = "updateButton";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // ArchiverWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.openReader);
            this.Controls.Add(this.logOutput);
            this.Controls.Add(this.progressBar1);
            this.Name = "ArchiverWindow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox logOutput;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.Button openReader;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Button cancelButton;
    }
}