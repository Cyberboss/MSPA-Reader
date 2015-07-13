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
        public class PageRange
        {
            public readonly string listName;
            public readonly int begin;
            public readonly int end;
            public PageRange(string lN, int b, int e)
            {
                listName = lN;
                begin = b;
                end = e;
            }
        }
        List<PageRange> toc;
        Writer db = null;
        bool running = false;
        bool closeRequested = false;
        int startingPage;
        int lastPage;
        public static List<PageRange> GetTableOfContents(Writer db)
        {
            var toc = new List<PageRange>();
            toc.Add(new PageRange(@"MS Paint Adventures",(int)Writer.StoryBoundaries.JAILBREAK_PAGE_ONE,db.lastPage));
            toc.Add(new PageRange(@"=> Jailbreak", (int)Writer.StoryBoundaries.JAILBREAK_PAGE_ONE, (int)Writer.StoryBoundaries.JAILBREAK_LAST_PAGE));
            toc.Add(new PageRange(@"=> Bard Quest",0,0));
            toc.Add(new PageRange(@"=> Problem Sleuth",0,0));
            toc.Add(new PageRange(@"=> Ryanquest",0,0));
            toc.Add(new PageRange(@"=> Homestuck Beta",(int)Writer.StoryBoundaries.HSB,(int)Writer.StoryBoundaries.EOHSB));
            toc.Add(new PageRange(@"=> Homestuck", (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE, db.lastPage));
            toc.Add(new PageRange(@"| => Part 1", (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE, (int)Writer.StoryBoundaries.HS_EOP1));
            toc.Add(new PageRange(@"| | => Act 1: The Note Desolation Plays",(int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE,(int)Writer.StoryBoundaries.HS_EOA1));
            toc.Add(new PageRange(@"| | => Act 2: Raise of the Conductor's Baton",(int)Writer.StoryBoundaries.HS_A2,(int)Writer.StoryBoundaries.HS_EOA2));
            toc.Add(new PageRange(@"| | => Act 3: Insane Corkscrew Haymakers", (int)Writer.StoryBoundaries.HS_A3, (int)Writer.StoryBoundaries.HS_EOA3));
            toc.Add(new PageRange(@"| | => Intermission 1: Don't Bleed on the Suits", (int)Writer.StoryBoundaries.HS_I1, (int)Writer.StoryBoundaries.HS_EOI1));
            toc.Add(new PageRange(@"| \ => Act 4: Flight of the Paradox Clones", (int)Writer.StoryBoundaries.HS_A4, (int)Writer.StoryBoundaries.HS_EOA4));
            toc.Add(new PageRange(@"| => Part 2",0,0));
            toc.Add(new PageRange(@"| | => Act 5",0,0));
            toc.Add(new PageRange(@"| | | => Act 1: MOB1US DOUBL3 R34CH4ROUND",0,0));
            toc.Add(new PageRange(@"| | | => Act 2: He is already here.",0,0));
            toc.Add(new PageRange(@"| | | \ => Scratch: excellent h[o]st",0,0));
            toc.Add(new PageRange(@"| | \ => Cascade",0,0));
            toc.Add(new PageRange(@"| \ => Intermission 2: The Man in the Cairo Overcoat",0,0));
            toc.Add(new PageRange(@"| => Part 3",0,0));
            toc.Add(new PageRange(@"| | => Act 6",0,0));
            toc.Add(new PageRange(@"| | | => Act 1: Through Broken Glass",0,0));
            toc.Add(new PageRange(@"| | | => Intermission 1: corpse party",0,0));
            toc.Add(new PageRange(@"| | | => Act 2: Your shit is wrecked.",0,0));
            toc.Add(new PageRange(@"| | | => Intermission 2: penis ouija",0,0));
            toc.Add(new PageRange(@"| | | => Act 3: Nobles",0,0));
            toc.Add(new PageRange(@"| | | => Intermission 3: Ballet of the Dancestors",0,0));
            toc.Add(new PageRange(@"| | | => Act 4: Void",0,0));
            toc.Add(new PageRange(@"| | | => Intermission 4: Dead",0,0));
            toc.Add(new PageRange(@"| | | => Act 5: Of Gods and Tricksters",0,0));
            toc.Add(new PageRange(@"| | | | => Act 1",0,0));
            toc.Add(new PageRange(@"| | | \ => Act 2",0,0));
            toc.Add(new PageRange(@"| | | => Intermission 5: I'M PUTTING YOU ON SPEAKER CRAB.",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 1",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 2",0,0));
            toc.Add(new PageRange(@"| | | | => Interfishin",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 3",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 4",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 5",0,0));
            toc.Add(new PageRange(@"| | | \ => Intermission 6",0,0));
            toc.Add(new PageRange(@"| | | => Act 6",0,0));
            toc.Add(new PageRange(@"| | | | => Act 1",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 1",0,0));
            toc.Add(new PageRange(@"| | | | => Act 2",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 2",0,0));
            toc.Add(new PageRange(@"| | | | => Act 3",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 3",0,0));
            toc.Add(new PageRange(@"| | | | => Act 4",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 4",0,0));
            toc.Add(new PageRange(@"| | | | => Act 5",0,0));
            toc.Add(new PageRange(@"| | | | => Intermission 5",0,0));
            return toc;
        }
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
            toc = GetTableOfContents(db);
            foreach(PageRange i in toc){
                startAt.Items.Add(i.listName);
            }
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
            startingPage = toc[startAt.SelectedIndex].begin;
            if (startingPage == 0)
            {
                MessageBox.Show("Page range not yet coded! If you see this in a release build, bug me on github http://github.com/cybnetsurfe3011/MSPA-Reader.");
                return false;
            }
            lastPage = toc[startAt.SelectedIndex].end;
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

        private void clearButton_Click(object sender, EventArgs e)
        {
            logOutput.Text = "";
            if (!running)
                progressBar1.Value = 0;
        }
    }
}
