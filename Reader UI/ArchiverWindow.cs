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
        int lastPage;
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
            startAt.Items.Add(@"MS Paint Adventures");
            startAt.Items.Add(@"=> Jailbreak");
            startAt.Items.Add(@"=> Bard Quest");
            startAt.Items.Add(@"=> Problem Sleuth");
            startAt.Items.Add(@"=> Ryanquest");
            startAt.Items.Add(@"=> Homestuck Beta");
            startAt.Items.Add(@"=> Homestuck");
            startAt.Items.Add(@"| => Part 1");
            startAt.Items.Add(@"| | => Act 1: The Note Desolation Plays");
            startAt.Items.Add(@"| | => Act 2: Raise of the Conductor's Baton");
            startAt.Items.Add(@"| | => Act 3: Insane Corkscrew Haymakers");
            startAt.Items.Add(@"| | => Intermission 1: Don't Bleed on the Suits");
            startAt.Items.Add(@"| \ => Act 4: Flight of the Paradox Clones");
            startAt.Items.Add(@"| => Part 2");
            startAt.Items.Add(@"| | => Act 5");
            startAt.Items.Add(@"| | | => Act 1: MOB1US DOUBL3 R34CH4ROUND");
            startAt.Items.Add(@"| | | => Act 2: He is already here.");
            startAt.Items.Add(@"| | | \ => Scratch: excellent h[o]st");
            startAt.Items.Add(@"| | \ => Cascade");
            startAt.Items.Add(@"| \ => Intermission 2: The Man in the Cairo Overcoat");
            startAt.Items.Add(@"| => Part 3");
            startAt.Items.Add(@"| | => Act 6");
            startAt.Items.Add(@"| | | => Act 1: Through Broken Glass");
            startAt.Items.Add(@"| | | => Intermission 1: corpse party");
            startAt.Items.Add(@"| | | => Act 2: Your shit is wrecked.");
            startAt.Items.Add(@"| | | => Intermission 2: penis ouija");
            startAt.Items.Add(@"| | | => Act 3: Nobles");
            startAt.Items.Add(@"| | | => Intermission 3: Ballet of the Dancestors");
            startAt.Items.Add(@"| | | => Act 4: Void");
            startAt.Items.Add(@"| | | => Intermission 4: Dead");
            startAt.Items.Add(@"| | | => Act 5: Of Gods and Tricksters");
            startAt.Items.Add(@"| | | | => Act 1");
            startAt.Items.Add(@"| | | \ => Act 2");
            startAt.Items.Add(@"| | | => Intermission 5: I'M PUTTING YOU ON SPEAKER CRAB.");
            startAt.Items.Add(@"| | | | => Intermission 1");
            startAt.Items.Add(@"| | | | => Intermission 2");
            startAt.Items.Add(@"| | | | => Interfishin");
            startAt.Items.Add(@"| | | | => Intermission 3");
            startAt.Items.Add(@"| | | | => Intermission 4");
            startAt.Items.Add(@"| | | | => Intermission 5");
            startAt.Items.Add(@"| | | \ => Intermission 6");
            startAt.Items.Add(@"| | | => Act 6");
            startAt.Items.Add(@"| | | | => Act 1");
            startAt.Items.Add(@"| | | | => Intermission 1");
            startAt.Items.Add(@"| | | | => Act 2");
            startAt.Items.Add(@"| | | | => Intermission 2");
            startAt.Items.Add(@"| | | | => Act 3");
            startAt.Items.Add(@"| | | | => Intermission 3");
            startAt.Items.Add(@"| | | | => Act 4");
            startAt.Items.Add(@"| | | | => Intermission 4");
            startAt.Items.Add(@"| | | | => Act 5");
            startAt.Items.Add(@"| | | | => Intermission 5");
            startAt.SelectedIndex = 0;
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
        bool SetPageBoundaries()
        {
            switch (startAt.SelectedIndex)
            {
                case 0:
                    startingPage = (int)Writer.StoryBoundaries.JAILBREAK_PAGE_ONE;
                    lastPage = db.lastPage;
                    break;
                case 1:
                    startingPage = (int)Writer.StoryBoundaries.JAILBREAK_PAGE_ONE;
                    lastPage = (int)Writer.StoryBoundaries.JAILBREAK_LAST_PAGE;
                    break;
                case 6:
                    startingPage = (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE;
                    lastPage = db.lastPage;
                    break;
                default:
                    MessageBox.Show("Page range not yet coded! If you see this in a release build, bug me on github http://github.com/cybnetsurfe3011/MSPA-Reader.");
                    return false;
            }
            return true;
        }
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            db.ResumeWork(worker, startingPage, lastPage);
            worker.ReportProgress(100, "FormMessageClose");
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (running || !SetPageBoundaries())
                return;
            running = true;
            updateButton.Enabled = false;
            cancelButton.Enabled = true;
            startAt.Enabled = false;
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
