using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reader_UI
{
    public partial class ArchiverWindow : Form
    {
        Writer db = null;
        bool running = false;
        bool closeRequested = false;
        int startingPage;
        public ArchiverWindow(Writer idb)
        {
            db = idb;
            InitializeComponent();
            updateButton.Enabled = false;
            worker.ProgressChanged += worker_progress;
            FormClosing += Writer_Closing;
            FormClosed += Writer_Closed;
            cancelButton.Enabled = false;
            updateButton.Enabled = true;
            startAt.Items.Add("Jailbreak");
            startAt.Items.Add("Homestuck");
            startAt.Items.Add("    Act 1");
            startAt.SelectedIndex = 1;
        }
        void Writer_Closed(object sender, System.EventArgs e)
        {
            Program.Shutdown(this, db);
        }
        void Writer_Closing(object sender, FormClosingEventArgs e)
        {
            if (!running)
                return;
            e.Cancel = true;
            closeRequested = true;
            Cursor.Current = Cursors.WaitCursor;
            worker.CancelAsync();
            foreach (Control c in Controls)
            {
                c.Enabled = false;
            }
        }
        void worker_progress(object sender, ProgressChangedEventArgs e)
        {
            if ((string)e.UserState == "FormMessageClose")
            {
                running = false;
                Cursor.Current = Cursors.Default;
                if (!closeRequested)
                {
                    cancelButton.Enabled = false;
                    updateButton.Enabled = true;
                    startAt.Enabled = true;
                    return;
                }
                System.Threading.Thread.Sleep(1000);
                Close();
            }
            else
            {
                progressBar1.Value = e.ProgressPercentage;
                logOutput.AppendText((string)e.UserState + Environment.NewLine);
            }
        }
        private void openReader_Click(object sender, EventArgs e)
        {
            Program.Open(db, false);
        }
        int GetStartPage()
        {
            switch (startAt.SelectedIndex)
            {
                case 0:
                    return (int)Writer.StoryBoundaries.JAILBREAK_PAGE_ONE;
                case 1:
                case 2:
                    return (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE;
                default:
                    System.Diagnostics.Debugger.Break();
                    throw new Exception();
            }
        }
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            db.ResumeWork(worker, startingPage);
            worker.ReportProgress(100, "FormMessageClose");
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (running)
                return;
            running = true;
            updateButton.Enabled = false;
            cancelButton.Enabled = true;
            startAt.Enabled = false;
            startingPage = GetStartPage();
            worker.RunWorkerAsync();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (!running)
                return;
            Cursor.Current = Cursors.WaitCursor;
            cancelButton.Enabled = false;
            worker.CancelAsync();
        }
    }
}
