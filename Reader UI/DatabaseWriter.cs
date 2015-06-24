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
    public partial class DatabaseWriter : Form
    {
        Database db = null;
        bool running = false;
        public DatabaseWriter(Database idb)
        {
            db = idb;
            InitializeComponent();
            worker.ProgressChanged += worker_progress;
            worker.RunWorkerAsync();
            FormClosing += Writer_Closing;
            FormClosed += Writer_Closed;
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

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            running = true;
            db.ResumeWork(worker);
            running = false;
            worker.ReportProgress(100, "FormMessageClose");
        }
    }
}
