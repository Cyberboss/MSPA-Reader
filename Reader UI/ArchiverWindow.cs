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
            toc.Add(new PageRange(@"=> Ryanquest", (int)Writer.StoryBoundaries.RQ, (int)Writer.StoryBoundaries.EORQ));
            toc.Add(new PageRange(@"=> Bard Quest",(int)Writer.StoryBoundaries.BQ,(int)Writer.StoryBoundaries.EOBQ));
            toc.Add(new PageRange(@"=> Problem Sleuth", (int)Writer.StoryBoundaries.PS, (int)Writer.StoryBoundaries.EOPS));
            toc.Add(new PageRange(@"| => Chapter 1: COMPENSATION, ADEQUATE", (int)Writer.StoryBoundaries.PS, (int)Writer.StoryBoundaries.PSC2 - 1));
            toc.Add(new PageRange(@"| => Chapter 2: TOO HOT TO HANDLE", (int)Writer.StoryBoundaries.PSC2, (int)Writer.StoryBoundaries.PSC3 - 1));
            toc.Add(new PageRange(@"| => Chapter 3: PERSECUTED BY UNSCRUPULOUS WHORES", (int)Writer.StoryBoundaries.PSC3, (int)Writer.StoryBoundaries.PSC4 - 1));
            toc.Add(new PageRange(@"| => Chapter 4: HAIRPIN TRIGGER", (int)Writer.StoryBoundaries.PSC4, (int)Writer.StoryBoundaries.PSC5 - 1));
            toc.Add(new PageRange(@"| => Chapter 5: THE DEGENERATE GAMBLERS", (int)Writer.StoryBoundaries.PSC5, (int)Writer.StoryBoundaries.PSC6 - 1));
            toc.Add(new PageRange(@"| => Chapter 6: MORE WEIRD PUZZLE SHIT", (int)Writer.StoryBoundaries.PSC6, (int)Writer.StoryBoundaries.PSC7 - 1));
            toc.Add(new PageRange(@"| => Chapter 7: FIT OF HYSTERICS", (int)Writer.StoryBoundaries.PSC7, (int)Writer.StoryBoundaries.PSC8 - 1));
            toc.Add(new PageRange(@"| => Chapter 8: THE SLEAZY BROTHEL IN THE SKY", (int)Writer.StoryBoundaries.PSC8, (int)Writer.StoryBoundaries.PSC9 - 1));
            toc.Add(new PageRange(@"| => Chapter 9: THIS IS COMPLETE BULLSHIT", (int)Writer.StoryBoundaries.PSC9, (int)Writer.StoryBoundaries.PSC10 - 1));
            toc.Add(new PageRange(@"| => Chapter 10: THAT WOULD HAVE BEEN SO BADASS", (int)Writer.StoryBoundaries.PSC10, (int)Writer.StoryBoundaries.PSC11 - 1));
            toc.Add(new PageRange(@"| => Chapter 11: TWO LUMPS", (int)Writer.StoryBoundaries.PSC11, (int)Writer.StoryBoundaries.PSC12 - 1));
            toc.Add(new PageRange(@"| => Chapter 12: SUITOR TO THE SODAJERK'S CONFIDANTE", (int)Writer.StoryBoundaries.PSC12, (int)Writer.StoryBoundaries.PSC13 - 1));
            toc.Add(new PageRange(@"| => Chapter 13: DMK", (int)Writer.StoryBoundaries.PSC13, (int)Writer.StoryBoundaries.PSC14 - 1));
            toc.Add(new PageRange(@"| => Chapter 14: ACTUALLY, THIS IS A LOT OF FUN", (int)Writer.StoryBoundaries.PSC2, (int)Writer.StoryBoundaries.PSC15 - 1));
            toc.Add(new PageRange(@"| => Chapter 15: TRIPLE COMB RAVE", (int)Writer.StoryBoundaries.PSC15, (int)Writer.StoryBoundaries.PSC16 - 1));
            toc.Add(new PageRange(@"| => Chapter 16: SUPERNATURAL GOURD", (int)Writer.StoryBoundaries.PSC16, (int)Writer.StoryBoundaries.PSC17 - 1));
            toc.Add(new PageRange(@"| => Chapter 17: BLACK LIQUID SORROW", (int)Writer.StoryBoundaries.PSC17, (int)Writer.StoryBoundaries.PSC18 - 1));
            toc.Add(new PageRange(@"| => Chapter 18: SUPERSTRING STRATA", (int)Writer.StoryBoundaries.PSC18, (int)Writer.StoryBoundaries.PSC19 - 1));
            toc.Add(new PageRange(@"| => Chapter 19: ASCENSION/ALIGNMENT/CONJUGATION", (int)Writer.StoryBoundaries.PSC19, (int)Writer.StoryBoundaries.PSC20 - 1));
            toc.Add(new PageRange(@"| => Chapter 20: TEMPORAL REPLICOLLISION", (int)Writer.StoryBoundaries.PSC20, (int)Writer.StoryBoundaries.PSC21 - 1));
            toc.Add(new PageRange(@"| => Chapter 21: BHMK", (int)Writer.StoryBoundaries.PSC21, (int)Writer.StoryBoundaries.PSC22 - 1));
            toc.Add(new PageRange(@"| => Chapter 22: SEPULCHRITUDE", (int)Writer.StoryBoundaries.PSC22, (int)Writer.StoryBoundaries.PSE - 1));
            toc.Add(new PageRange(@"\ => Epilogue", (int)Writer.StoryBoundaries.PSE, (int)Writer.StoryBoundaries.EOPS));
            toc.Add(new PageRange(@"=> Homestuck Beta",(int)Writer.StoryBoundaries.HSB,(int)Writer.StoryBoundaries.EOHSB));
            toc.Add(new PageRange(@"=> Homestuck", (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE, db.lastPage));
            toc.Add(new PageRange(@"| => Part 1", (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE, (int)Writer.StoryBoundaries.HS_EOP1));
            toc.Add(new PageRange(@"| | => Act 1: The Note Desolation Plays",(int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE,(int)Writer.StoryBoundaries.HS_EOA1));
            toc.Add(new PageRange(@"| | => Act 2: Raise of the Conductor's Baton",(int)Writer.StoryBoundaries.HS_A2,(int)Writer.StoryBoundaries.HS_EOA2));
            toc.Add(new PageRange(@"| | => Act 3: Insane Corkscrew Haymakers", (int)Writer.StoryBoundaries.HS_A3, (int)Writer.StoryBoundaries.HS_EOA3));
            toc.Add(new PageRange(@"| | => Intermission 1: Don't Bleed on the Suits", (int)Writer.StoryBoundaries.HS_I1, (int)Writer.StoryBoundaries.HS_EOI1));
            toc.Add(new PageRange(@"| \ => Act 4: Flight of the Paradox Clones", (int)Writer.StoryBoundaries.HS_A4, (int)Writer.StoryBoundaries.HS_EOA4));
            toc.Add(new PageRange(@"| => Part 2",(int)Writer.StoryBoundaries.HS_A5A1,(int)Writer.StoryBoundaries.HS_EOI2));
            toc.Add(new PageRange(@"| | => Act 5",(int)Writer.StoryBoundaries.HS_A5A1,(int)Writer.StoryBoundaries.HS_EOA5));
            toc.Add(new PageRange(@"| | | => Act 1: MOB1US DOUBL3 R34CH4ROUND",(int)Writer.StoryBoundaries.HS_A5A1,(int)Writer.StoryBoundaries.HS_EOA5A1));
            toc.Add(new PageRange(@"| | | => Act 2: He is already here.",(int)Writer.StoryBoundaries.HS_A5A2,(int)Writer.StoryBoundaries.HS_EOA5A2));
            toc.Add(new PageRange(@"| | | \ => Scratch: excellent h[o]st",(int)Writer.StoryBoundaries.HS_A5A2S,(int)Writer.StoryBoundaries.HS_EOA5A2S));
            toc.Add(new PageRange(@"| | \ => Cascade", (int)Writer.StoryBoundaries.HS_CASCADE, (int)Writer.StoryBoundaries.HS_CASCADE));
            toc.Add(new PageRange(@"| \ => Intermission 2: The Man in the Cairo Overcoat",(int)Writer.StoryBoundaries.HS_I2,(int)Writer.StoryBoundaries.HS_EOI2));
            toc.Add(new PageRange(@"| => Part 3", (int)Writer.StoryBoundaries.HS_A6, db.lastPage));
            toc.Add(new PageRange(@"| | => Act 6", (int)Writer.StoryBoundaries.HS_A6, db.lastPage));
            toc.Add(new PageRange(@"| | | => Act 1: Through Broken Glass", (int)Writer.StoryBoundaries.HS_A6, (int)Writer.StoryBoundaries.HS_EOA6A1));
            toc.Add(new PageRange(@"| | | => Intermission 1: corpse party", (int)Writer.StoryBoundaries.HS_A6I1, (int)Writer.StoryBoundaries.HS_EOA6I1));
            toc.Add(new PageRange(@"| | | => Act 2: Your shit is wrecked.",(int)Writer.StoryBoundaries.HS_A6A2, (int)Writer.StoryBoundaries.HS_EOA6A2));
            toc.Add(new PageRange(@"| | | => Intermission 2: penis ouija",(int)Writer.StoryBoundaries.HS_A6I2, (int)Writer.StoryBoundaries.HS_EOA6I2));
            toc.Add(new PageRange(@"| | | => Act 3: Nobles",(int)Writer.StoryBoundaries.HS_A6A3, (int)Writer.StoryBoundaries.HS_EOA6A3));
            toc.Add(new PageRange(@"| | | => Intermission 3: Ballet of the Dancestors",(int)Writer.StoryBoundaries.HS_A6I3, (int)Writer.StoryBoundaries.HS_EOA6I3));
            toc.Add(new PageRange(@"| | | => Act 4: Void", (int)Writer.StoryBoundaries.HS_A6A4, (int)Writer.StoryBoundaries.HS_A6A4));
            toc.Add(new PageRange(@"| | | => Intermission 4: Dead", (int)Writer.StoryBoundaries.HS_A6I4, (int)Writer.StoryBoundaries.HS_EOA6I4));
            toc.Add(new PageRange(@"| | | => Act 5: Of Gods and Tricksters", (int)Writer.StoryBoundaries.HS_A6A5, (int)Writer.StoryBoundaries.HS_EOA6A5));
            toc.Add(new PageRange(@"| | | | => Act 1", (int)Writer.StoryBoundaries.HS_A6A5, (int)Writer.StoryBoundaries.HS_EOA6A5A1));
            toc.Add(new PageRange(@"| | | \ => Act 2", (int)Writer.StoryBoundaries.HS_A6A5A2, (int)Writer.StoryBoundaries.HS_EOA6A5A2));
            toc.Add(new PageRange(@"| | | => Intermission 5: I'M PUTTING YOU ON SPEAKER CRAB.", (int)Writer.StoryBoundaries.HS_A6I5, (int)Writer.StoryBoundaries.HS_EOA6I5));
            //no, fuck this
            toc.Add(new PageRange(@"| | | => Act 6", (int)Writer.StoryBoundaries.HS_A6A6, db.lastPage));
            toc.Add(new PageRange(@"| | | | => Act 1", (int)Writer.StoryBoundaries.HS_A6A6, (int)Writer.StoryBoundaries.HS_EOA6A6A1));
            toc.Add(new PageRange(@"| | | | => Intermission 1", (int)Writer.StoryBoundaries.HS_A6A6I1, (int)Writer.StoryBoundaries.HS_EOA6A6I1));
            toc.Add(new PageRange(@"| | | | => Act 2", (int)Writer.StoryBoundaries.HS_A6A6A2, (int)Writer.StoryBoundaries.HS_EOA6A6A2));
            toc.Add(new PageRange(@"| | | | => Intermission 2", (int)Writer.StoryBoundaries.HS_A6A6I2, (int)Writer.StoryBoundaries.HS_EOA6A6I2));
            toc.Add(new PageRange(@"| | | | => Act 3", (int)Writer.StoryBoundaries.HS_A6A6A3, (int)Writer.StoryBoundaries.HS_EOA6A6A3));
            toc.Add(new PageRange(@"| | | | => Intermission 3", (int)Writer.StoryBoundaries.HS_A6A6I3, (int)Writer.StoryBoundaries.HS_EOA6A6I3));
            toc.Add(new PageRange(@"| | | | => Act 4", (int)Writer.StoryBoundaries.HS_A6A6A4, (int)Writer.StoryBoundaries.HS_EOA6A6A4));
            toc.Add(new PageRange(@"| | | | => Intermission 4", (int)Writer.StoryBoundaries.HS_A6A6I4, (int)Writer.StoryBoundaries.HS_EOA6A6I4));
            toc.Add(new PageRange(@"| | | | => Act 5", (int)Writer.StoryBoundaries.HS_A6A6A5, (int)Writer.StoryBoundaries.HS_EOA6A6A5));
            toc.Add(new PageRange(@"| | | | => Intermission 5", (int)Writer.StoryBoundaries.HS_A6A6I5, db.lastPage));
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
                progressBar1.Value = Math.Max(Math.Min(e.ProgressPercentage,100),0);
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
