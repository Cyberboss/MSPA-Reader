using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Reader_UI
{
    partial class Reader : ReaderConstants
    {

        class LineOrPB
        {
            readonly bool isImg;
            GifStream strm = null;
            Control other = null;
            public void Dispose()
            {
                if (isImg)
                {
                    strm.gif.Dispose();
                    strm.loc.Dispose();
                }
                else
                {
                    other.Dispose();
                }
            }
            public LineOrPB(GifStream g)
            {
                isImg = true;
                strm = g;
            }
            public LineOrPB(Control o)
            {
                isImg = false;
                other = o;
            }
            public Control GetControl()
            {
                if (isImg)
                    return strm.gif;
                return other;
            }
        }
        class GifStream
        {
            public PictureBox gif;
            public System.IO.MemoryStream loc;
        }
        class ImageStream
        {
            public Image gif;
            public System.IO.MemoryStream loc;
        }
        class x2Handler
        {
            public Panel comicPanel = null;
            Panel pesterlog = null, mainPanel = null;
            Label linkPrefix = null;
            PictureBox next = null;
            Button pesterHideShow = null;
            List<LineOrPB> conversations = null;
            string promptType = null;

            int pLMinHeight, pLMaxHeight;
            bool pesterLogVisible = false;


            public x2Handler(Panel cP)
            {
                comicPanel = cP;
            }
            public x2Handler(Panel mP, Panel cP, Panel pl, Label lP, PictureBox n, Button pHS, List<LineOrPB> c, string pT, int pLMinH, int pLMaxH){

                comicPanel = cP;
                mainPanel = mP;
                pesterlog = pl;
                linkPrefix = lP;
                next = n;
                pesterHideShow = pHS;
                conversations = c;
                promptType = pT;
                pLMinHeight = pLMinH;
                pLMaxHeight = pLMaxH;

                pesterHideShow.Click += pesterHideShow_Click;

            }

            void pesterHideShow_Click(object sender, EventArgs e)
            {
                Toggle();
            }
            public void Toggle()
            {
                int currentHeight = REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;
                if (pesterLogVisible)
                    currentHeight += pLMinHeight;
                else
                    currentHeight += pLMaxHeight;

                var oldLoc = linkPrefix.Location.Y;
                linkPrefix.Location = new Point(linkPrefix.Location.X, currentHeight);
                next.Location = new Point(next.Location.X, next.Location.Y + (linkPrefix.Location.Y - oldLoc));
                comicPanel.Height = currentHeight + REGULAR_COMIC_PANEL_BOTTOM_PADDING;

                if (!pesterLogVisible)
                {
                    pesterHideShow.Text = "Hide " + promptType;
                    foreach (var line in conversations)
                        pesterlog.Controls.Add(line.GetControl());
                    pesterlog.MinimumSize = new Size(REGULAR_PESTERLOG_WIDTH, pesterlog.Height + REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT);
                }
                else
                {
                    pesterHideShow.Text = "Show " +promptType;
                    foreach (var line in conversations)
                        pesterlog.Controls.Remove(line.GetControl());
                    pesterlog.MinimumSize = new Size(REGULAR_PESTERLOG_WIDTH, REGULAR_PESTERLOG_HEIGHT);
                }
                mainPanel.Height = Math.Max(mainPanel.Height,comicPanel.Height + REGULAR_COMIC_PANEL_Y_OFFSET + REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET);
                pesterLogVisible = !pesterLogVisible;

            }
            public void Hide()
            {
                pesterLogVisible = true;
                Toggle();
            }
            public void Show()
            {
                pesterLogVisible = false;
                Toggle();
            }
            public void Kill()
            {
                comicPanel.Dispose();
                //leave most of the garbage for the collector so we don't double dispose
            }
        }
        Writer db;
        Panel mainPanel = null, headerPanel = null, comicPanel = null;
        x2Handler x2Panel = null;
        Label[] mspaHeaderLink = new Label[REGULAR_NUMBER_OF_HEADER_LABELS];
        PictureBox[] candyCorn = new PictureBox[REGULAR_NUMBER_OF_HEADER_CANDY_CORNS];
        ProgressBar pageLoadingProgress = null;
        int pageRequest;
        Writer.Page page = null;
        Writer.Style previousStyle;
        Button pesterHideShow = null;
        ImageStream currentIcon = null;
        class TricksterShit : IDisposable
        {
            MemoryStream[] bytes = new MemoryStream[2];
            public Image bg, fg;
            public WebBrowser flashHeader;
            Reader parent;
            public TricksterShit(Reader p,Parser.Resource[] shit)
            {
                parent = p;
                bytes[0] = new MemoryStream(shit[0].data);
                bytes[1] = new MemoryStream(shit[2].data);
                bg = Image.FromStream(bytes[0]);
                fg = Image.FromStream(bytes[1]);
                parent.BackgroundImage = bg;
                parent.mainPanel.BackgroundImage = fg;
                flashHeader = new WebBrowser();
                parent.Controls.Add(flashHeader);
                flashHeader.Width = 950;
                flashHeader.Height = 24;
                parent.InitFlashMovie(flashHeader,shit[1]);
                FixAlign();
            }
            public void Dispose()
            {
                parent.BackgroundImage = null;
                parent.mainPanel.BackgroundImage = null;
                bg.Dispose();
                fg.Dispose();
                flashHeader.Dispose();
                bytes[0].Dispose();
                bytes[1].Dispose();
            }
            public void FixAlign()
            {
                flashHeader.Location = new Point(parent.Width / 2 - flashHeader.Width / 2, 0);
            }
        }
        TricksterShit trick = null;

        List<LineOrPB> conversations = new List<LineOrPB>();
        Label errorLabel = null;
        TextBox progressLabel = null;
        bool pesterLogVisible;
        int pLMaxHeight, pLMinHeight;
        bool pageContainsFlash = false;

        Stack<int> pageQueue = new Stack<int>();


        //page stuff
        GrowRich title = null;
        List<GifStream> gifs = new List<GifStream>();
        WebBrowser flash = null;
        GrowRich narrative = null;
        Label linkPrefix = null;
        LinkLabel next = null, tereziPassword = null;
        Panel pesterlog = null;
        GifStream headerP = null;

        public Reader(Writer idb)
        {
            db = idb;
            InitializeComponent();
#if linux
            Properties.Settings.Default.fullscreen = false;
            toggleFullscreen.Visible = false;
#endif
            FormClosed += Reader_Closed;
            numericUpDown1.Maximum = db.lastPage;
            numericUpDown1.Minimum = (int)Writer.StoryBoundaries.JAILBREAK_PAGE_ONE;
            numericUpDown1.Value = numericUpDown1.Minimum;
            AcceptButton = jumpButton;
            Shown += Reader_Shown;
            for (int i = 0; i < mspaHeaderLink.Count(); ++i)
                mspaHeaderLink[i] = null;
            for (int i = 0; i < candyCorn.Count(); ++i)
                candyCorn[i] = null;
            mrAjax.RunWorkerCompleted += mrAjax_RunWorkerCompleted;
            mrAjax.ProgressChanged += mrAjax_ProgressChanged;
            FormClosing += Reader_FormClosing;
            Resize += Reader_Resize;
            autoSave.Checked = Properties.Settings.Default.autoSave;
            ResizeEnd += Reader_ResizeEnd;
            monitorGameOver.ProgressChanged += monitorGameOver_ProgressChanged;
        }

        void mrAjax_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressLabel == null)
                return;
            progressLabel.AppendText((string)e.UserState + Environment.NewLine);
        }

        void Reader_ResizeEnd(object sender, EventArgs e)
        {
            if(mainPanel != null)
                mainPanel.Location = new Point(Width / 2 - mainPanel.Width / 2, mainPanel.Location.Y);
            if(headerPanel != null)
                headerPanel.Location = new Point(Width / 2 - headerPanel.Width / 2, headerPanel.Location.Y);
            if (trick != null)
                trick.FixAlign();
        }
       
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5 || keyData == Keys.F11 ||(!pageContainsFlash && page != null))
            {

                switch (keyData)
                {
                    case Keys.H:
                        uiToggleButton_Click(null, null);
                        return true;
                    case Keys.F5:
                        CurtainsUp();
                        WakeUpMr(page.number);
                        return true;
                    case Keys.F11:
                        toggleFullscreen_Click(null, null);
                        return true;
                    case Keys.Left:
                        WakeUpMr(Writer.ValidRange(page.number - 1));
                        return true;
                    case Keys.Right:
                        if (page.number < db.lastPage)
                            WakeUpMr(page.number + 1);
                        return true;
                    case Keys.Space:
                        if (page != null && (page.meta.lines != null && page.meta.lines.Count() != 0)
                            || (page.meta2 != null && page.meta2.lines != null && page.meta2.lines.Count() != 0))
                        {
                            pesterHideShow_Click(null, null);
                            if (x2Panel != null)
                                if (pesterLogVisible)
                                    x2Panel.Show();
                                else
                                    x2Panel.Hide();

                        }
                        return true;
                    case Keys.Down:
                        VerticalScroll.Value = Math.Min(VerticalScroll.Value + 50, VerticalScroll.Maximum);
                        return true;
                    case Keys.Up:
                        VerticalScroll.Value = Math.Max(VerticalScroll.Value - 50, VerticalScroll.Minimum);
                        return true;
                }
            }
            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
         }

        void Reader_Resize(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.fullscreen)
                return;
            if (WindowState == FormWindowState.Minimized)
                return;
            if (WindowState != FormWindowState.Maximized)
            {

                FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                SizeGripStyle = System.Windows.Forms.SizeGripStyle.Auto;
            }
        }


        void Reader_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.maximized = WindowState == FormWindowState.Maximized;
            CleanControls();
        }

        void mrAjax_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (page == null)
            {
                page = new Writer.Page(pageRequest);
                MessageBox.Show("An internal error occurred when retrieving the page. Usually this means you are out of memory or some resource on the page cannot be downloaded. (Yes, I checked both the cdn and www).");
                RemoveControl(pageLoadingProgress);RemoveControl(progressLabel);
                errorLabel = new Label();
                errorLabel.AutoSize = true;
                errorLabel.Text = "Press F5 to try again.";
                mainPanel.Controls.Add(errorLabel);
                errorLabel.Location = new Point(mainPanel.Width / 2 - errorLabel.Width / 2, mainPanel.Height / 2 - errorLabel.Height / 2);
            }else
                LoadPage();

                flashWarning.Visible = pageContainsFlash && (uiToggleButton.Text != "Show UI");
             
        }
        void LoadSmash()
        {
            //nothing but the flash
            //for some reason the y offset is wrong

            //*
            flash = new WebBrowser();
            mainPanel.Controls.Add(flash);
            InitFlashMovie(flash, page.resources[0]);
            mainPanel.Height = flash.Height;
            flash.Location = new Point(0, 0);
             //* */
            pageContainsFlash = true;

            RemoveControl(pageLoadingProgress);RemoveControl(progressLabel);
        }
        void LoadScratchPage()
        {
            LoadRegularPage();
            //special header

            var tempPB = new GifStream();
            tempPB.loc = new System.IO.MemoryStream(page.resources[0].data);    //always the first
            tempPB.gif = new PictureBox();
            tempPB.gif.Image = Image.FromStream(tempPB.loc);
            tempPB.gif.SizeMode = PictureBoxSizeMode.CenterImage;
            tempPB.gif.Width = mainPanel.Width;
            tempPB.gif.Height = tempPB.gif.Image.Height;
            tempPB.gif.BackColor = Color.Black;

            //increase Y of all mainPanelItems and header by height
            foreach (Control i in mainPanel.Controls)
                i.Location = new Point(i.Location.X, i.Location.Y + SCRATCH_PANEL_Y_OFFSET);
            //mainPanel.Controls.Add(tempPB.gif);
            tempPB.gif.Location = new Point(mainPanel.Width / 2 - tempPB.gif.Width / 2, 0);
            mainPanel.Controls.Add(tempPB.gif);

            gifs.Add(tempPB);

            if (page.meta.altText != null)
            {
                var hoverText = new ToolTip();
                hoverText.AutoPopDelay = 5000;
                hoverText.InitialDelay = 500;
                hoverText.ReshowDelay = 500;
                // Force the ToolTip text to be displayed whether or not the form is active.
                hoverText.ShowAlways = true;
                hoverText.SetToolTip(tempPB.gif, page.meta.altText);
            }
            //therefore also Remove the first

            var shiftHeight = gifs[0].gif.Height;
            gifs[0].gif.Dispose();
            gifs[0].loc.Dispose();


            gifs.RemoveAt(0);


            //decrease Y of all comicPanel Items and header by height except title
            foreach (Control con in comicPanel.Controls)
                if (con != title)
                    con.Location = new Point(con.Location.X, con.Location.Y - shiftHeight);

            //recalculate bottoms

            comicPanel.Height -= shiftHeight;
            mainPanel.Height -= shiftHeight;

            //set colours

            comicPanel.BackColor = Color.FromArgb(SCRATCH_COMIC_PANEL_COLOUR_R, SCRATCH_COMIC_PANEL_COLOUR_G, SCRATCH_COMIC_PANEL_COLOUR_B);
            title.BackColor = Color.FromArgb(SCRATCH_COMIC_PANEL_COLOUR_R, SCRATCH_COMIC_PANEL_COLOUR_G, SCRATCH_COMIC_PANEL_COLOUR_B);
            title.ForeColor = Color.White;

            if (page.meta.narr != null)
            {
                narrative.BackColor = Color.FromArgb(SCRATCH_COMIC_PANEL_COLOUR_R, SCRATCH_COMIC_PANEL_COLOUR_G, SCRATCH_COMIC_PANEL_COLOUR_B);
            }
            else
            {
                pesterlog.BackColor = Color.FromArgb(SCRATCH_COMIC_PANEL_COLOUR_R, SCRATCH_COMIC_PANEL_COLOUR_G, SCRATCH_COMIC_PANEL_COLOUR_B);
                foreach (var line in conversations)
                {
                    line.GetControl().BackColor = Color.FromArgb(SCRATCH_COMIC_PANEL_COLOUR_R, SCRATCH_COMIC_PANEL_COLOUR_G, SCRATCH_COMIC_PANEL_COLOUR_B);
                }
            }

            //handle the top le text if in the range
            //TODO

            if (page.number >= 5976 && page.number <= 5981)
            {

                //kill off the second last gif

                var theLEText = gifs[gifs.Count - 2].gif.Height;
                comicPanel.Controls.Remove(gifs[gifs.Count - 2].gif);
                gifs[gifs.Count - 2].gif.BackColor = System.Drawing.Color.Transparent;


                gifs[gifs.Count - 2].gif.Visible = false;
                Controls.Add(gifs[gifs.Count - 2].gif);
                gifs[gifs.Count - 2].gif.BringToFront();
                //move narrative/pesterlog and link up

                if (page.meta.narr != null)
                {
                    narrative.Location = new Point(narrative.Location.X, narrative.Location.Y - theLEText);
                }
                else
                {
                    foreach (var lin in conversations)
                    {
                        lin.GetControl().Location = new Point(lin.GetControl().Location.X, lin.GetControl().Location.Y - theLEText);
                    }
                }

                linkPrefix.Location = new Point(linkPrefix.Location.X, linkPrefix.Location.Y - theLEText);
                next.Location = new Point(next.Location.X, next.Location.Y - theLEText);

                comicPanel.Height -= theLEText;
                mainPanel.Height -= theLEText;

                tempPB.gif.MouseMove += MoveLEText;
                tempPB.gif.MouseEnter += ShowLEText;
                tempPB.gif.MouseLeave += HideLEText;
            }
            linkPrefix.ForeColor = Color.White;
            next.LinkColor = Color.FromArgb(44, 255, 75);   //TODO:
        }

        void LoadHomosuckPage()
        {
            LoadRegularPage();

            //set colours

            //TODO Loads of MGNs
            comicPanel.BackColor = Color.FromArgb(7, 60, 0);
            title.BackColor = Color.FromArgb(7, 60, 0);
            title.ForeColor = Color.FromArgb(46, 215, 58);

            if (page.meta.narr != null)
            {
                narrative.BackColor = Color.FromArgb(7, 60, 0);
            }
            else
            {
                pesterlog.BackColor = Color.FromArgb(7, 60, 0);
                foreach (var line in conversations)
                {
                    line.GetControl().BackColor = Color.FromArgb(7, 60, 0);
                }
            }
            linkPrefix.BackColor = Color.FromArgb(7, 60, 0);
            next.BackColor = Color.FromArgb(7, 60, 0);
            next.LinkColor = Color.FromArgb(46, 215, 58);

            
        }
        void HideLEText(object sender, EventArgs e)
        {
            gifs[gifs.Count - 2].gif.Visible = false;
        }

        void ShowLEText(object sender, EventArgs e)
        {
            gifs[gifs.Count - 2].gif.Visible = true;
        }

        void MoveLEText(object sender, MouseEventArgs e)
        {

            var theLEText = gifs[gifs.Count - 2].gif;
            if (WindowState != FormWindowState.Maximized || e.X + mainPanel.Location.X + 5 + theLEText.Width < Width)
            {
                theLEText.Location = new Point(e.X + mainPanel.Location.X + 5, e.Y + 5);
            }
            else
                theLEText.Visible = false;
        }
        void LoadTricksterPage()
        {
            LoadRegularPage();
            comicPanel.BackColor = Color.FromArgb(255, 155, 253);
            title.ForeColor = Color.FromArgb(249, 253, 99);

            //TODO Loads of MGNs
            title.BackColor = Color.FromArgb(255, 155, 253);

            if (page.meta.narr != null)
            {
                narrative.BackColor = Color.FromArgb(255, 155, 253);
            }
            else
            {
                pesterlog.BackColor = Color.FromArgb(255, 155, 253);
                foreach (var line in conversations)
                {
                    line.GetControl().BackColor = Color.FromArgb(255, 155, 253); ;
                }
            }
            linkPrefix.BackColor = Color.FromArgb(255, 155, 253);
            linkPrefix.ForeColor = Color.FromArgb(249, 253, 99);
            next.BackColor = Color.FromArgb(255, 155, 253); ;
            next.LinkColor = Color.FromArgb(251, 6, 11);
        }
        void LoadSBAHJPage()
        {
            LoadRegularPage();

            var newFont = new System.Drawing.Font("Comic Sans MS", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            foreach (var l in mspaHeaderLink)
            {
                l.Font = newFont;
                l.BackColor = Color.Black;
            }
            foreach (var c in candyCorn)
            {
                c.BackColor = Color.Black;
            }
            comicPanel.BackColor = Color.FromArgb(20, 255, 35);

            //TODO Loads of MGNs
            title.BackColor = Color.FromArgb(20, 255, 35);

            if (page.meta.narr != null)
            {
                narrative.BackColor = Color.FromArgb(20, 255, 35);
            }
            else
            {
                pesterlog.BackColor = Color.FromArgb(20, 255, 35);
                foreach (var line in conversations)
                {
                    line.GetControl().BackColor = Color.FromArgb(20, 255, 35);
                }
            }

            newFont = new System.Drawing.Font("Comic Sans MS", title.Font.Size, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title.Font = newFont;
            linkPrefix.Font = newFont;
            next.Font = newFont;
        }
        void LoadX2Page()
        {
            LoadRegularPage();
            var oldMeta = page.meta;
            page.links = page.links2;
            page.meta = page.meta2;
            page.resources = page.resources2;

            var fakeLink = gifs.Last();
            if(pesterHideShow != null)
                pesterHideShow.Click -= pesterHideShow_Click;

            if (oldMeta.narr != null)
                x2Panel = new x2Handler(comicPanel);
            else
                x2Panel = new x2Handler(mainPanel,comicPanel, pesterlog, linkPrefix, fakeLink.gif, pesterHideShow, conversations, oldMeta.promptType, pLMinHeight, pLMaxHeight);

            conversations = new List<LineOrPB>();
            LoadRegularPage();
            x2Panel.comicPanel.Location = new Point(mainPanel.Width / 2 - (x2Panel.comicPanel.Width * 2 + 29) / 2, x2Panel.comicPanel.Location.Y);
            comicPanel.Location = new Point(x2Panel.comicPanel.Location.X + x2Panel.comicPanel.Width + 29, comicPanel.Location.Y);
            fakeLink.gif.Location = new Point(next.Location.X + linkPrefix.Width,next.Location.Y + next.Height);
            x2Panel.comicPanel.Height = comicPanel.Height;

            mainPanel.Height = comicPanel.Height + REGULAR_COMIC_PANEL_Y_OFFSET + REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET;
        }
        void LoadOvershine()
        {
            //panel
            comicPanel = new Panel();
            comicPanel.AutoSize = true;
            comicPanel.Width = REGULAR_COMIC_PANEL_WIDTH;
            comicPanel.MaximumSize = new Size(REGULAR_COMIC_PANEL_WIDTH, Int32.MaxValue);
            comicPanel.Location = new Point(mainPanel.Width / 2 - comicPanel.Width / 2, REGULAR_COMIC_PANEL_Y_OFFSET);
            comicPanel.BackColor = Color.FromArgb(REGULAR_COMIC_PANEL_COLOUR_R, REGULAR_COMIC_PANEL_COLOUR_G, REGULAR_COMIC_PANEL_COLOUR_B);

            var bright = new GifStream();
            bright.loc = new MemoryStream(page.resources[0].data);
            bright.gif = new PictureBox();
            var gif = bright.gif;
            gif.Image = Image.FromStream(bright.loc);
            gif.Width = gif.Image.Width;
            gif.Height = gif.Image.Height;


            comicPanel.Height = 743;

            mainPanel.Controls.Add(gif);
            gif.Location = new Point(0, 0);


            mainPanel.Controls.Add(comicPanel);


            //next page
            var currentHeight = gif.Height + REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;
            var leftSide = comicPanel.Width / 2 - REGULAR_PESTERLOG_WIDTH / 2;
            if (page.number < db.lastPage)
            {

                linkPrefix = new Label();
                linkPrefix.AutoSize = true;
                linkPrefix.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                linkPrefix.Text = REGULAR_LINK_PREFIX;
                linkPrefix.Location = new Point(leftSide, currentHeight);
                comicPanel.Controls.Add(linkPrefix);

                if (page.links != null && page.links.Count() > 0)
                {
                    next = new GrowLinkLabel();
                    next.Width = 600;
                    next.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    next.Text = "    " + page.links[0].originalText;
                    next.Location = new Point(leftSide, currentHeight);
                    next.LinkClicked += next_LinkClicked;
                    comicPanel.Controls.Add(next);
                }
                linkPrefix.BringToFront();
            }

            comicPanel.Height = currentHeight + REGULAR_COMIC_PANEL_BOTTOM_PADDING;

            mainPanel.Height = comicPanel.Height + REGULAR_COMIC_PANEL_Y_OFFSET + REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET;

            RemoveControl(pageLoadingProgress); RemoveControl(progressLabel);
        }
        void LoadJailbreak()
        {
            LoadRegularPage();
            comicPanel.MaximumSize = new Size(782,Int32.MaxValue);
            comicPanel.Width = 782;
            var oldLocX = comicPanel.Location.X;
            comicPanel.Location = new Point(mainPanel.Width / 2 - comicPanel.Width / 2, comicPanel.Location.Y);
            foreach (var tempPB in gifs)
            {
                tempPB.gif.Location = new Point(comicPanel.Width / 2 - tempPB.gif.Width / 2, tempPB.gif.Location.Y);
            }
            var leftSide = comicPanel.Width / 2 - REGULAR_NARRATIVE_WIDTH / 2;
            if(narrative != null)
                narrative.Location = new Point(leftSide, narrative.Location.Y);
            if (linkPrefix != null)
                linkPrefix.Location = new Point(leftSide, linkPrefix.Location.Y);
            if (next != null)
                next.Location = new Point(leftSide, next.Location.Y);
        }
        void LoadPage()
        {
            try
            {
                SuspendLayout();

                if (Properties.Settings.Default.autoSave)
                {
                    Properties.Settings.Default.lastReadPage = page.number;
                }
                pageQueue.Push(page.number);
                saveButton.Enabled = true;
                switch (db.GetStyle(pageRequest))
                {
                    case Writer.Style.JAILBREAK:
                        LoadJailbreak();
                        break;
                    case Writer.Style.X2:
                        LoadX2Page();
                        break;
                    case Writer.Style.TRICKSTER:
                        LoadTricksterPage();
                        break;
                    case Writer.Style.REGULAR:
                        LoadRegularPage();
                        break;
                    case Writer.Style.SCRATCH:
                        LoadScratchPage();
                        break;
                    case Writer.Style.CASCADE:
                        LoadCascade();
                        break;
                    case Writer.Style.HOMOSUCK:
                        LoadHomosuckPage();
                        break;
                    case Writer.Style.SBAHJ:
                        LoadSBAHJPage();
                        break;
                    case Writer.Style.OVERSHINE:
                        LoadOvershine();
                        break;
                    case Writer.Style.GAMEOVER:
                        flash = null;//prevent progress report from changing colours
                        monitorGameOver.RunWorkerAsync();
                        goto case Writer.Style.SMASH;
                    case Writer.Style.SHES8ACK:
                    case Writer.Style.DOTA:
                    case Writer.Style.SMASH:
                        LoadSmash();    //also works for dota, gameover, and Shesback, curtains up will set the bg
                        break;
                    default:
                        Debugger.Break();
                        LoadRegularPage();
                        break;
                }

                //dump the garbage
                page.resources2 = null;
                page.resources = null;

            }
            catch
            {
                throw;
            }
            finally
            {
                ResumeLayout();
                //fix scroll bar
                Update();
                AutoScrollPosition = new Point(0, 0);
                numericUpDown1.Value = page.number;
            }
        }
        //https://stackoverflow.com/questions/1874077/loading-a-flash-movie-from-a-memory-stream-or-a-byte-array
        //*
        private void InitFlashMovie(WebBrowser f ,Parser.Resource swfFile, bool setDimensions = true)
        {
            if(setDimensions)
                SetFlashDimensions();
            f.Navigate(WriteTempResource(swfFile));
            f.Navigating += flash_Navigating;
        }

        //ITS SO SIMPLE :D
        void flash_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            e.Cancel = true;
            var res = Parser.GetPageNumberFromURL(e.Url.OriginalString);
            if (res != 0)
                WakeUpMr(res);
        }
        
        void SetFlashDimensions()
        {
            if (flash == null || flash.Disposing || flash.IsDisposed)
                return;
            switch (page.number) { 
                case (int)Writer.PagesOfImportance.GAMEOVER:
                    flash.Width = 950;
                    flash.Height = 786;
                    break;
                case (int)Writer.PagesOfImportance.CALIBORN_PAGE_SMASH:
                case (int)Writer.PagesOfImportance.CALIBORN_PAGE_SMASH2:
                    flash.Width = 950;
                    flash.Height = 1160;
                    break;
                case (int)Writer.PagesOfImportance.CASCADE:
                case (int)Writer.PagesOfImportance.SHES8ACK:
                case (int)Writer.PagesOfImportance.DOTA:
                    flash.Width = 950;
                    flash.Height = 650;
                    break;
                default:
                    flash.Width = REGULAR_FLASH_MOVIE_WIDTH;
                    flash.Height = REGULAR_FLASH_MOVIE_HEIGHT;
                    break;
            }
        }//*/
        void LoadCascade()
        {

            //cascade has no comic panel, use the mainPanel
            comicPanel = mainPanel;
            comicPanel.Location = new Point(comicPanel.Location.X, 0);

            //title is part of the flash
            title = null;

            //special header

            var tempPB = new GifStream();
            tempPB.loc = new System.IO.MemoryStream(page.resources[6].data);
            tempPB.gif = new PictureBox();
            tempPB.gif.Image = Image.FromStream(tempPB.loc);
            tempPB.gif.Width = tempPB.gif.Image.Width;
            tempPB.gif.Height = tempPB.gif.Image.Height;
            tempPB.gif.Location = new Point(comicPanel.Width / 2 - tempPB.gif.Width / 2, 0);
            comicPanel.Controls.Add(tempPB.gif);
            gifs.Add(tempPB);

            //*

            //first write the segments
            WriteTempResource(page.resources[1]);
            WriteTempResource(page.resources[2]);
            WriteTempResource(page.resources[3]);
            WriteTempResource(page.resources[4]);
            WriteTempResource(page.resources[5]);

            flash = new WebBrowser();
            comicPanel.Controls.Add(flash);


            InitFlashMovie(flash, page.resources[0]);
            flash.Location = new Point(comicPanel.Width / 2 - flash.Width / 2, CASCADE_PANEL_Y_OFFSET + REGULAR_PANEL_Y_OFFSET + 40);
            
            // * */
            pageContainsFlash = true;



            linkPrefix = new Label();
            linkPrefix.AutoSize = true;
            linkPrefix.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            linkPrefix.Text = REGULAR_LINK_PREFIX;
            linkPrefix.Location = new Point(REGULAR_PANEL_WIDTH / 2  - REGULAR_PESTERLOG_WIDTH/2, flash.Location.Y + flash.Height + 23);
            comicPanel.Controls.Add(linkPrefix);

            next = new GrowLinkLabel();
            next.Width = 600;
            next.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            next.Text = "    " + page.links[0].originalText;
            next.Location = linkPrefix.Location; //TODO: MAGIC NUMBERS!!!
            next.LinkClicked += next_LinkClicked;
            comicPanel.Controls.Add(next);

            linkPrefix.BringToFront();


            mainPanel.Height = flash.Location.Y + flash.Height +
                CASCADE_BOTTOM_Y_OFFSET;

            headerPanel.BringToFront();

            RemoveControl(pageLoadingProgress);RemoveControl(progressLabel);
        }
        string WriteTempResource(Parser.Resource res)
        {
            string path = System.IO.Path.GetTempPath() + res.originalFileName;
            File.WriteAllBytes(path, res.data); //let the throw fall through and fail the page load (problems beyond our scope)
            return path;
        }
        void LoadRegularPage()
        {
            //panel
            comicPanel = new Panel();
            comicPanel.AutoSize = true;
            comicPanel.Width = REGULAR_COMIC_PANEL_WIDTH;
            comicPanel.MaximumSize = new Size(REGULAR_COMIC_PANEL_WIDTH, Int32.MaxValue);
            comicPanel.Location = new Point(mainPanel.Width / 2 - comicPanel.Width / 2, REGULAR_COMIC_PANEL_Y_OFFSET);
            comicPanel.BackColor = Color.FromArgb(REGULAR_COMIC_PANEL_COLOUR_R, REGULAR_COMIC_PANEL_COLOUR_G, REGULAR_COMIC_PANEL_COLOUR_B);
            mainPanel.Controls.Add(comicPanel);

            //title
            title = new GrowRich();
            title.Width = REGULAR_PAGE_TITLE_WIDTH;
            title.SelectionAlignment = HorizontalAlignment.Center;
            title.Font = new System.Drawing.Font("Courier New", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title.Text = page.meta.title;
            comicPanel.Controls.Add(title);
            title.Location = new Point(comicPanel.Width / 2 - title.Width / 2, REGULAR_TITLE_Y_OFFSET);

            //content
            int currentHeight = title.Location.Y + title.Height + REGULAR_TITLE_Y_OFFSET;
            for (int i = 0; i < page.resources.Count(); i++)
            {
                if (i == page.resources.Count() - 1 && Parser.Is2x(page.number) && page.resources != page.resources2)
                {
                    currentHeight -= REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET;
                    continue;
                }
                if (page.resources[i].isInPesterLog)
                    continue;
                if (!pageContainsFlash && !Parser.IsGif(page.resources[i].originalFileName))
                {
                    flash = new WebBrowser();
                    comicPanel.Controls.Add(flash);
                    InitFlashMovie(flash, page.resources[i]);

                    flash.Location = new Point(comicPanel.Width / 2 - flash.Width / 2, currentHeight);
                    currentHeight += flash.Height;
                    pageContainsFlash = true;
                }
                else if (Parser.IsGif(page.resources[i].originalFileName))
                {

                    var tempPB = new GifStream();
                    tempPB.loc = new System.IO.MemoryStream(page.resources[i].data);
                    tempPB.gif = new PictureBox();
                    tempPB.gif.Image = Image.FromStream(tempPB.loc);
                    tempPB.gif.Width = tempPB.gif.Image.Width;
                    tempPB.gif.Height = tempPB.gif.Image.Height;
                    tempPB.gif.Location = new Point(comicPanel.Width / 2 - tempPB.gif.Width / 2, currentHeight);
                    comicPanel.Controls.Add(tempPB.gif);
                    currentHeight += tempPB.gif.Height;
                    if (i < page.resources.Count() - 1 || (page.resources[page.resources.Count() - 1].isInPesterLog && i == page.resources.Count() - 1))
                        currentHeight += REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET;
                    gifs.Add(tempPB);
                }
            }

            currentHeight += REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;

            //words
            int leftSide;
            if (page.meta.narr != null)
            {
                narrative = new GrowRich();
                narrative.Width = REGULAR_NARRATIVE_WIDTH;
                narrative.SelectionAlignment = HorizontalAlignment.Center;
                narrative.Font = new System.Drawing.Font("Courier New", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                narrative.Text = page.meta.narr.text.Trim();
                narrative.ForeColor = System.Drawing.ColorTranslator.FromHtml(page.meta.narr.hexColour);
                leftSide = comicPanel.Width / 2 - narrative.Width / 2;
                narrative.Location = new Point(leftSide, currentHeight);
                currentHeight += narrative.Height;
                comicPanel.Controls.Add(narrative);

            }
            else
            {
                pesterlog = new Panel();
                pesterlog.AutoSize = true;
                pesterlog.Width = REGULAR_PESTERLOG_WIDTH;
                pesterlog.Height = REGULAR_PESTERLOG_HEIGHT;
                pesterlog.MinimumSize = new Size(REGULAR_PESTERLOG_WIDTH, REGULAR_PESTERLOG_HEIGHT);
                pesterlog.MaximumSize = new Size(REGULAR_PESTERLOG_WIDTH, Int32.MaxValue);
                pesterlog.BorderStyle = BorderStyle.FixedSingle;
                pesterlog.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

                leftSide = comicPanel.Width / 2 - pesterlog.Width / 2;
                pesterlog.Location = new Point(leftSide, currentHeight);
                comicPanel.Controls.Add(pesterlog);

                pesterHideShow = new Button();
                pesterHideShow.AutoSize = true;
                pesterHideShow.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                pesterHideShow.Text = "Show " + page.meta.promptType;
                pesterHideShow.Click += pesterHideShow_Click;
                pesterLogVisible = false;
                pesterlog.Controls.Add(pesterHideShow);
                pesterHideShow.Location = new Point(pesterlog.Width / 2 - pesterHideShow.Width / 2, 0);

                pLMaxHeight = currentHeight + REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;
                //log lines
                for (int i = 0; i < page.meta.lines.Count(); ++i)
                {
                    if (!page.meta.lines[i].isImg)
                    {
                        var tmpl = new GrowRich();
                        tmpl.Width = REGULAR_PESTERLOG_LINE_WIDTH;
                        tmpl.MaximumSize = new Size(tmpl.Width, Int32.MaxValue);
                        tmpl.Font = new System.Drawing.Font("Courier New", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        tmpl.BackColor = pesterlog.BackColor;   //can't change forecolour otherwise
                        tmpl.ForeColor = System.Drawing.ColorTranslator.FromHtml(page.meta.lines[i].hexColour);
                        tmpl.Text = "";

                        tmpl.Text += page.meta.lines[i].text;

                        if (page.meta.lines[i].subTexts.Count() != 0)
                            for (int j = 0; j < page.meta.lines[i].subTexts.Count(); ++j)
                            {
                                if (!page.meta.lines[i].subTexts[j].isImg)
                                {

                                    //font change
                                    tmpl.Select(page.meta.lines[i].subTexts[j].begin, page.meta.lines[i].subTexts[j].length);
                                    if (page.meta.lines[i].subTexts[j].underlined)
                                        tmpl.SelectionFont = new System.Drawing.Font("Courier New", 10.5F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                                    tmpl.SelectionColor = System.Drawing.ColorTranslator.FromHtml(page.meta.lines[i].subTexts[j].colour);
                                }
                                else
                                {
                                    //inline image

                                    //what needs to happen here is we need to advance the text so that the image can fit in which should just be " " times some factor of the width of the image
                                    //for now assume 1space = 10.5 pt = 14px
                                    Parser.Resource inlineImg = Array.Find(page.resources, x => x.isInPesterLog == true && x.originalFileName == page.meta.lines[i].subTexts[j].colour);
                                    var tmpPB = new GifStream();
                                    tmpPB.loc = new MemoryStream(inlineImg.data);
                                    tmpPB.gif = new PictureBox();
                                    tmpPB.gif.Image = Image.FromStream(tmpPB.loc);

                                    string spaces = "";
                                    int needed = tmpPB.gif.Image.Width / 14;
                                    for (int k = 0; k < needed; ++k)
                                        spaces += " ";

                                    tmpl.Text = tmpl.Text.Substring(0, page.meta.lines[i].subTexts[j].begin) + spaces + tmpl.Text.Substring(page.meta.lines[i].subTexts[j].begin);

                                    //just dispose the picture while we are testing this
                                    tmpPB.gif.Dispose();
                                    tmpPB.loc.Dispose();

                                }
                            }
                        tmpl.Location = new Point(pesterlog.ClientSize.Width / 2 - tmpl.Width / 2, pLMaxHeight - currentHeight);
                        pLMaxHeight += tmpl.Height;
                        conversations.Add(new LineOrPB(tmpl));
                    }
                    else
                    {
                        //find the resource
                        var tmpI = Array.Find(page.resources, x => x.isInPesterLog == true && x.originalFileName == page.meta.lines[i].text);
                        //TODO: Handle image lines ("SHE HAS WHAT????")
                        if (Parser.IsGif(page.resources[i].originalFileName))
                        {

                            var tempPB = new GifStream();
                            tempPB.loc = new System.IO.MemoryStream(tmpI.data);
                            tempPB.gif = new PictureBox();
                            tempPB.gif.Image = Image.FromStream(tempPB.loc);
                            tempPB.gif.Width = tempPB.gif.Image.Width;
                            tempPB.gif.Height = tempPB.gif.Image.Height;
                            tempPB.gif.Location = new Point(pesterlog.ClientSize.Width / 2 - REGULAR_PESTERLOG_LINE_WIDTH / 2, pLMaxHeight - currentHeight);
                            pLMaxHeight += tempPB.gif.Height;

                            conversations.Add(new LineOrPB(tempPB));
                        }
                        else
                        {
                            //there's problems
                            Debugger.Break(); //then let us bounce
                        }
                    }
                }


                currentHeight += pesterlog.Height;
                pLMinHeight = currentHeight;
                pLMaxHeight += REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;
            }

            currentHeight += REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;

            //add 2x fake link 

            if (Parser.Is2x(page.number) && page.resources != page.resources2)
            {
                var tempPB = new GifStream();
                tempPB.loc = new System.IO.MemoryStream(page.resources[page.resources.Count() - 1].data);
                tempPB.gif = new PictureBox();
                tempPB.gif.Image = Image.FromStream(tempPB.loc);
                tempPB.gif.Width = tempPB.gif.Image.Width;
                tempPB.gif.Height = tempPB.gif.Image.Height;
                tempPB.gif.Location = new Point(comicPanel.Width / 2 - tempPB.gif.Width / 2, currentHeight);
                comicPanel.Controls.Add(tempPB.gif);
                currentHeight += tempPB.gif.Height;
                gifs.Add(tempPB);
            }

            //next page
            if (page.number < db.lastPage)
            {

                linkPrefix = new Label();
                linkPrefix.AutoSize = true;
                linkPrefix.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                linkPrefix.Text = REGULAR_LINK_PREFIX;
                linkPrefix.Location = new Point(leftSide, currentHeight);
                comicPanel.Controls.Add(linkPrefix);

                if (page.links != null && page.links.Count() > 0)
                {
                    next = new GrowLinkLabel();
                    next.Width = 600;
                    next.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    next.Text = "    " + page.links[0].originalText;
                    next.Location = new Point(leftSide, currentHeight);
                    next.LinkClicked += next_LinkClicked;
                    comicPanel.Controls.Add(next);
                }
                linkPrefix.BringToFront();
            }

            comicPanel.Height = currentHeight + REGULAR_COMIC_PANEL_BOTTOM_PADDING;

            mainPanel.Height = comicPanel.Height + REGULAR_COMIC_PANEL_Y_OFFSET + REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET;


            if (page.meta.promptType == "CalibornLog")
                FixNarrativePrompt();

            RemoveControl(pageLoadingProgress);RemoveControl(progressLabel);

        }
        void FixNarrativePrompt()
        {
            pesterHideShow_Click(null, null);   //Show the prompt
            RemoveControl(pesterHideShow);
            pesterHideShow = null;
            pesterlog.BorderStyle = BorderStyle.None;
            pesterlog.BackColor = Color.White;
            foreach (Control c in pesterlog.Controls)
            {
                c.BackColor = Color.White;
            }
            comicPanel.Paint +=PaintNarrativePrompt;
        }

        void PaintNarrativePrompt(object sender, PaintEventArgs e)
        {
            //draw a border rectangle on the comic panel.
            e.Graphics.FillRectangle(new System.Drawing.SolidBrush(Color.FromArgb(198, 198, 198)), new Rectangle(pesterlog.Location.X - 4, pesterlog.Location.Y - 4, pesterlog.Width + 8, pesterlog.Height + 8));
        }
        void RelocateLinks()
        {
            int currentHeight = REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;
            if (pesterLogVisible)
                currentHeight += pLMinHeight;
            else
                currentHeight += pLMaxHeight;

            linkPrefix.Location = new Point(linkPrefix.Location.X, currentHeight);
            next.Location = new Point(next.Location.X, currentHeight);
            comicPanel.Height = currentHeight + REGULAR_COMIC_PANEL_BOTTOM_PADDING;

        }
        void pesterHideShow_Click(object sender, EventArgs e)
        {
            RelocateLinks();
            if (!pesterLogVisible)
            {
                pesterHideShow.Text = "Hide " + page.meta.promptType;
                foreach (var line in conversations)
                    pesterlog.Controls.Add(line.GetControl());
                pesterlog.MinimumSize = new Size(REGULAR_PESTERLOG_WIDTH, pesterlog.Height + REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT);
            }
            else
            {
                pesterHideShow.Text = "Show " + page.meta.promptType;
                foreach (var line in conversations)
                    pesterlog.Controls.Remove(line.GetControl());
                pesterlog.MinimumSize = new Size(REGULAR_PESTERLOG_WIDTH, REGULAR_PESTERLOG_HEIGHT);
            }
            mainPanel.Height = comicPanel.Height + REGULAR_COMIC_PANEL_Y_OFFSET + REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET;
            pesterLogVisible = !pesterLogVisible;

        }

        void next_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            WakeUpMr(page.links[0].pageNumber);

        }        
        void WakeUpMr(int pg)
        {

            if (mrAjax.IsBusy)
                return;
            saveButton.Enabled = false;
            pageContainsFlash = false;
            numericUpDown1.Value = pg;
            var newStyle = db.GetStyle(pg);
            if (previousStyle != newStyle)
                CurtainsUp(newStyle);
            ShowLoadingScreen();
            pageRequest = pg;
            mrAjax.RunWorkerAsync();
        }
        void Reader_Shown(object sender, EventArgs e)
        {
            Properties.Settings.Default.fullscreen =! Properties.Settings.Default.fullscreen;
            toggleFullscreen_Click(null, null);
            Reader_Resize(null, null);
            CurtainsUp();
            int pr;
            if (Properties.Settings.Default.lastReadPage >= (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE &&
                Properties.Settings.Default.lastReadPage <= db.lastPage)
                pr = Properties.Settings.Default.lastReadPage;
            else
                pr = (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE;
            WakeUpMr(pr);
        }
        void RemoveControl(Control c)
        {
            if (c != null)
            {
                c.Dispose();
            }
        }
        void SetupHeader()
        {

            headerPanel = new Panel();
            headerPanel.Width = REGULAR_PANEL_WIDTH;
            headerPanel.Height = HEADER_HEIGHT; //TODO: MG NMBR
            headerPanel.BackColor = Color.FromArgb(REGULAR_BACK_COLOUR_R, REGULAR_BACK_COLOUR_G, REGULAR_BACK_COLOUR_B);

            mspaHeaderLink[0] = new Label();
            mspaHeaderLink[0].AutoSize = true;
            mspaHeaderLink[0].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[0].ForeColor = System.Drawing.Color.White;
            mspaHeaderLink[0].Location = new System.Drawing.Point(HEADER_FIRST_LINK_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[0].Text = REGULAR_LABEL_TEXT_1;
            mspaHeaderLink[0].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com"); };
            mspaHeaderLink[0].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[0]);

            candyCorn[0] = new PictureBox();
            candyCorn[0].Width = REGULAR_CANDYCORN_WIDTH;
            candyCorn[0].Height = REGULAR_CANDYCORN_HEIGHT;
            candyCorn[0].Location = new Point(mspaHeaderLink[0].Location.X + mspaHeaderLink[0].Width + REGULAR_HEADER_X_OFFSET, REGULAR_CANDYCORN_Y_OFFSET);
            candyCorn[0].Image = null;
            candyCorn[0].SizeMode = PictureBoxSizeMode.StretchImage;
            candyCorn[0].Width = 16;
            candyCorn[0].Height = 16;
            headerPanel.Controls.Add(candyCorn[0]);

            mspaHeaderLink[1] = new Label();
            mspaHeaderLink[1].AutoSize = true;
            mspaHeaderLink[1].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[1].ForeColor = Color.FromArgb(REGULAR_HEADER_GREEN_R, REGULAR_HEADER_GREEN_G, REGULAR_HEADER_GREEN_B);
            mspaHeaderLink[1].Location = new System.Drawing.Point(candyCorn[0].Location.X + candyCorn[0].Width + REGULAR_HEADER_X_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[1].Text = REGULAR_LABEL_TEXT_2;
            mspaHeaderLink[1].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com?viewpage=archive"); };
            mspaHeaderLink[1].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[1]);

            mspaHeaderLink[2] = new Label();
            mspaHeaderLink[2].AutoSize = true;
            mspaHeaderLink[2].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[2].ForeColor = System.Drawing.Color.White;
            mspaHeaderLink[2].Location = new System.Drawing.Point(mspaHeaderLink[1].Location.X + mspaHeaderLink[1].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[2].Text = REGULAR_LABEL_TEXT_3;
            headerPanel.Controls.Add(mspaHeaderLink[2]);

            mspaHeaderLink[3] = new Label();
            mspaHeaderLink[3].AutoSize = true;
            mspaHeaderLink[3].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[3].ForeColor = Color.FromArgb(REGULAR_HEADER_GREEN_R, REGULAR_HEADER_GREEN_G, REGULAR_HEADER_GREEN_B);
            mspaHeaderLink[3].Location = new System.Drawing.Point(mspaHeaderLink[2].Location.X + mspaHeaderLink[2].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[3].Text = REGULAR_LABEL_TEXT_4;
            mspaHeaderLink[3].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com?viewpage=new"); };
            mspaHeaderLink[3].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[3]);

            candyCorn[1] = new PictureBox();
            candyCorn[1].Width = REGULAR_CANDYCORN_WIDTH;
            candyCorn[1].Height = REGULAR_CANDYCORN_HEIGHT;
            candyCorn[1].Location = new Point(mspaHeaderLink[3].Location.X + mspaHeaderLink[3].Width + REGULAR_HEADER_X_OFFSET, REGULAR_CANDYCORN_Y_OFFSET);
            candyCorn[1].Image = null;
            candyCorn[1].SizeMode = PictureBoxSizeMode.StretchImage;
            candyCorn[1].Width = 16;
            candyCorn[1].Height = 16;
            headerPanel.Controls.Add(candyCorn[1]);

            mspaHeaderLink[4] = new Label();
            mspaHeaderLink[4].AutoSize = true;
            mspaHeaderLink[4].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[4].ForeColor = Color.FromArgb(REGULAR_HEADER_BLUE_R, REGULAR_HEADER_BLUE_G, REGULAR_HEADER_BLUE_B);
            mspaHeaderLink[4].Location = new System.Drawing.Point(candyCorn[1].Location.X + candyCorn[1].Width + REGULAR_HEADER_X_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[4].Text = REGULAR_LABEL_TEXT_5;
            mspaHeaderLink[4].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com?viewmap=menu"); };
            mspaHeaderLink[4].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[4]);

            mspaHeaderLink[5] = new Label();
            mspaHeaderLink[5].AutoSize = true;
            mspaHeaderLink[5].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[5].ForeColor = System.Drawing.Color.White;
            mspaHeaderLink[5].Location = new System.Drawing.Point(mspaHeaderLink[4].Location.X + mspaHeaderLink[4].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[5].Text = REGULAR_LABEL_TEXT_6;
            headerPanel.Controls.Add(mspaHeaderLink[5]);


            mspaHeaderLink[6] = new Label();
            mspaHeaderLink[6].AutoSize = true;
            mspaHeaderLink[6].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[6].ForeColor = Color.FromArgb(REGULAR_HEADER_BLUE_R, REGULAR_HEADER_BLUE_G, REGULAR_HEADER_BLUE_B);
            mspaHeaderLink[6].Location = new System.Drawing.Point(mspaHeaderLink[5].Location.X + mspaHeaderLink[5].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[6].Text = REGULAR_LABEL_TEXT_7;
            mspaHeaderLink[6].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com?viewlog=menu"); };
            mspaHeaderLink[6].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[6]);

            mspaHeaderLink[7] = new Label();
            mspaHeaderLink[7].AutoSize = true;
            mspaHeaderLink[7].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[7].ForeColor = System.Drawing.Color.White;
            mspaHeaderLink[7].Location = new System.Drawing.Point(mspaHeaderLink[6].Location.X + mspaHeaderLink[6].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[7].Text = REGULAR_LABEL_TEXT_8;
            headerPanel.Controls.Add(mspaHeaderLink[7]);

            mspaHeaderLink[8] = new Label();
            mspaHeaderLink[8].AutoSize = true;
            mspaHeaderLink[8].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[8].ForeColor = Color.FromArgb(REGULAR_HEADER_BLUE_R, REGULAR_HEADER_BLUE_G, REGULAR_HEADER_BLUE_B);
            mspaHeaderLink[8].Location = new System.Drawing.Point(mspaHeaderLink[7].Location.X + mspaHeaderLink[7].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[8].Text = REGULAR_LABEL_TEXT_9;
            mspaHeaderLink[8].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com?search=menu"); };
            mspaHeaderLink[8].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[8]); 
            
            candyCorn[2] = new PictureBox();
            candyCorn[2].Width = REGULAR_CANDYCORN_WIDTH;
            candyCorn[2].Height = REGULAR_CANDYCORN_HEIGHT;
            candyCorn[2].Location = new Point(mspaHeaderLink[8].Location.X + mspaHeaderLink[8].Width + REGULAR_HEADER_X_OFFSET, REGULAR_CANDYCORN_Y_OFFSET);
            candyCorn[2].Image = null;
            candyCorn[2].SizeMode = PictureBoxSizeMode.StretchImage;
            candyCorn[2].Width = 16;
            candyCorn[2].Height = 16;
            headerPanel.Controls.Add(candyCorn[2]);

            mspaHeaderLink[9] = new Label();
            mspaHeaderLink[9].AutoSize = true;
            mspaHeaderLink[9].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[9].ForeColor = Color.FromArgb(REGULAR_HEADER_YELLOW_R, REGULAR_HEADER_YELLOW_G, REGULAR_HEADER_YELLOW_B);
            mspaHeaderLink[9].Location = new System.Drawing.Point(candyCorn[2].Location.X + candyCorn[2].Width + REGULAR_HEADER_X_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[9].Text = REGULAR_LABEL_TEXT_10;
            mspaHeaderLink[9].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com?viewpage=shop"); };
            mspaHeaderLink[9].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[9]);

            mspaHeaderLink[10] = new Label();
            mspaHeaderLink[10].AutoSize = true;
            mspaHeaderLink[10].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[10].ForeColor = System.Drawing.Color.White;
            mspaHeaderLink[10].Location = new System.Drawing.Point(mspaHeaderLink[9].Location.X + mspaHeaderLink[9].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[10].Text = REGULAR_LABEL_TEXT_11;
            headerPanel.Controls.Add(mspaHeaderLink[10]);


            mspaHeaderLink[11] = new Label();
            mspaHeaderLink[11].AutoSize = true;
            mspaHeaderLink[11].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[11].ForeColor = Color.FromArgb(REGULAR_HEADER_YELLOW_R, REGULAR_HEADER_YELLOW_G, REGULAR_HEADER_YELLOW_B);
            mspaHeaderLink[11].Location = new System.Drawing.Point(mspaHeaderLink[10].Location.X + mspaHeaderLink[10].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[11].Text = REGULAR_LABEL_TEXT_12;
            mspaHeaderLink[11].Click += (o, i) => { System.Diagnostics.Process.Start("http://homestuck.bandcamp.com/"); };
            mspaHeaderLink[11].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[11]);

            candyCorn[3] = new PictureBox();
            candyCorn[3].Width = REGULAR_CANDYCORN_WIDTH;
            candyCorn[3].Height = REGULAR_CANDYCORN_HEIGHT;
            candyCorn[3].Location = new Point(mspaHeaderLink[11].Location.X + mspaHeaderLink[11].Width + REGULAR_HEADER_X_OFFSET, REGULAR_CANDYCORN_Y_OFFSET);
            candyCorn[3].Image = null;
            candyCorn[3].SizeMode = PictureBoxSizeMode.StretchImage;
            candyCorn[3].Width = 16;
            candyCorn[3].Height = 16;
            headerPanel.Controls.Add(candyCorn[3]);

            mspaHeaderLink[12] = new Label();
            mspaHeaderLink[12].AutoSize = true;
            mspaHeaderLink[12].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[12].ForeColor = Color.FromArgb(REGULAR_HEADER_ORANGE_R, REGULAR_HEADER_ORANGE_G, REGULAR_HEADER_ORANGE_B);
            mspaHeaderLink[12].Location = new System.Drawing.Point(candyCorn[3].Location.X + candyCorn[3].Width + REGULAR_HEADER_X_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[12].Text = REGULAR_LABEL_TEXT_13;
            mspaHeaderLink[12].Click += (o, i) => { System.Diagnostics.Process.Start("http://mspaforums.com"); };
            mspaHeaderLink[12].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[12]);

            mspaHeaderLink[13] = new Label();
            mspaHeaderLink[13].AutoSize = true;
            mspaHeaderLink[13].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[13].ForeColor = System.Drawing.Color.White;
            mspaHeaderLink[13].Location = new System.Drawing.Point(mspaHeaderLink[12].Location.X + mspaHeaderLink[12].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[13].Text = REGULAR_LABEL_TEXT_14;
            headerPanel.Controls.Add(mspaHeaderLink[13]);


            mspaHeaderLink[14] = new Label();
            mspaHeaderLink[14].AutoSize = true;
            mspaHeaderLink[14].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[14].ForeColor = Color.FromArgb(REGULAR_HEADER_ORANGE_R, REGULAR_HEADER_ORANGE_G, REGULAR_HEADER_ORANGE_B);
            mspaHeaderLink[14].Location = new System.Drawing.Point(mspaHeaderLink[13].Location.X + mspaHeaderLink[13].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[14].Text = REGULAR_LABEL_TEXT_15;
            mspaHeaderLink[14].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com/unlock.html"); };
            mspaHeaderLink[14].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[14]);

            mspaHeaderLink[15] = new Label();
            mspaHeaderLink[15].AutoSize = true;
            mspaHeaderLink[15].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[15].ForeColor = System.Drawing.Color.White;
            mspaHeaderLink[15].Location = new System.Drawing.Point(mspaHeaderLink[14].Location.X + mspaHeaderLink[14].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[15].Text = REGULAR_LABEL_TEXT_16;
            headerPanel.Controls.Add(mspaHeaderLink[15]);

            mspaHeaderLink[16] = new Label();
            mspaHeaderLink[16].AutoSize = true;
            mspaHeaderLink[16].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[16].ForeColor = Color.FromArgb(REGULAR_HEADER_ORANGE_R, REGULAR_HEADER_ORANGE_G, REGULAR_HEADER_ORANGE_B);
            mspaHeaderLink[16].Location = new System.Drawing.Point(mspaHeaderLink[15].Location.X + mspaHeaderLink[15].Width, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[16].Text = REGULAR_LABEL_TEXT_17;
            mspaHeaderLink[16].Click += (o, i) => { System.Diagnostics.Process.Start("http://www.mspaintadventures.com/credits.html"); };
            mspaHeaderLink[16].Cursor = Cursors.Hand;
            headerPanel.Controls.Add(mspaHeaderLink[16]);

            Controls.Add(headerPanel);

            Update();

            headerPanel.Location = new Point(this.Width / 2 - headerPanel.Width / 2, 0);
            
        }
        void CleanComic()
        {
            RemoveControl(errorLabel);
            RemoveControl(title);
            foreach (var pic in gifs)
            {
                RemoveControl(pic.gif);
                pic.loc.Dispose();
            }
            gifs.Clear();
            RemoveControl(linkPrefix);
            RemoveControl(next);
            RemoveControl(tereziPassword);
            if (monitorGameOver.IsBusy)
            {
                var tmp = flash;
                flash = null; //prevent progress report from changing colours
                RemoveControl(tmp);
                monitorGameOver.CancelAsync();
            }else
                RemoveControl(flash);
            RemoveControl(pesterHideShow);
            foreach (var line in conversations)
                line.Dispose();
            conversations.Clear();
            RemoveControl(pesterlog);
            RemoveControl(comicPanel);
            if (x2Panel != null)
            {
                x2Panel.Kill();
                x2Panel = null;
            }
        }
        void CleanControls()
        {
            CleanComic();
            if (trick != null)
            {
                trick.Dispose();
                trick = null;
            }
            RemoveControl(mainPanel);
            for (int i = 0; i < mspaHeaderLink.Count(); ++i)
                RemoveControl(mspaHeaderLink[i]);
            for (int i = 0; i < candyCorn.Count(); ++i)
                RemoveControl(candyCorn[i]);
            RemoveControl(headerPanel);
            RemoveControl(pageLoadingProgress);
            RemoveControl(progressLabel);
            if (currentIcon != null)
            {
                currentIcon.gif.Dispose();
                currentIcon.loc.Dispose();
                currentIcon = null;
            }
            if (headerP != null)
            {
                headerP.gif.Dispose();
                headerP.loc.Dispose();
                headerP = null;
            }
        }
        void CurtainsUp(Writer.Style s = Writer.Style.REGULAR)
        {
            previousStyle = s;

            CleanControls();

            headerPanel = null;

            switch (s)
            {
                case Writer.Style.JAILBREAK:
                case Writer.Style.OVERSHINE:
                case Writer.Style.REGULAR:
                    BackColor = Color.FromArgb(REGULAR_BACK_COLOUR_R, REGULAR_BACK_COLOUR_G, REGULAR_BACK_COLOUR_B);

                    mainPanel = new Panel();
                    mainPanel.AutoSize = true;
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, REGULAR_PANEL_Y_OFFSET);
                    mainPanel.BackColor = Color.FromArgb(REGULAR_PANEL_COLOUR_R, REGULAR_PANEL_COLOUR_G, REGULAR_PANEL_COLOUR_B);
                    Controls.Add(mainPanel);

                    SetupHeader();

                    break;
                case Writer.Style.X2:
                    BackColor = Color.FromArgb(REGULAR_BACK_COLOUR_R, REGULAR_BACK_COLOUR_G, REGULAR_BACK_COLOUR_B);

                    mainPanel = new Panel();
                    mainPanel.AutoSize = true;
                    mainPanel.MaximumSize = new System.Drawing.Size(1660, Int32.MaxValue);
                    mainPanel.Width = 1660;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 66);
                    mainPanel.BackColor = Color.FromArgb(REGULAR_PANEL_COLOUR_R, REGULAR_PANEL_COLOUR_G, REGULAR_PANEL_COLOUR_B);
                    Controls.Add(mainPanel);

                    SetupHeader();

                    headerPanel.Location = new Point(headerPanel.Location.X, headerPanel.Location.Y + 47);
                    headerP = new GifStream();
                    headerP.loc = new MemoryStream(db.Getx2Header());
                    headerP.gif = new PictureBox();
                    headerP.gif.Image = Image.FromStream(headerP.loc);
                    headerP.gif.Width = mainPanel.Width;
                    headerP.gif.Height = 47;
                    Controls.Add(headerP.gif);
                    headerP.gif.Location = new Point(mainPanel.Location.X, 0);



                    break;
                case Writer.Style.CASCADE:
                    BackColor = Color.FromArgb(CASCADE_BACK_COLOUR_R, CASCADE_BACK_COLOUR_G, CASCADE_BACK_COLOUR_B);

                    mainPanel = new Panel();
                    mainPanel.AutoSize = true;
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 0);
                    mainPanel.BackColor = Color.FromArgb(CASCADE_PANEL_COLOUR_R, CASCADE_PANEL_COLOUR_G, CASCADE_PANEL_COLOUR_B);
                    Controls.Add(mainPanel);

                    SetupHeader();
                       headerPanel.Location = new Point(headerPanel.Location.X, CASCADE_PANEL_Y_OFFSET);

                    break;
                case Writer.Style.SMASH:
                    BackColor = Color.FromArgb(REGULAR_BACK_COLOUR_R, REGULAR_BACK_COLOUR_G, REGULAR_BACK_COLOUR_B);

                    
                    mainPanel = new Panel();
                    mainPanel.AutoSize = true;
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 0);
                    mainPanel.BackColor = Color.FromArgb(CASCADE_PANEL_COLOUR_R, CASCADE_PANEL_COLOUR_G, CASCADE_PANEL_COLOUR_B);
                    Controls.Add(mainPanel);
                    break;
                case Writer.Style.SCRATCH:
                    BackColor = Color.FromArgb(SCRATCH_BACK_COLOUR_R, SCRATCH_BACK_COLOUR_G, SCRATCH_BACK_COLOUR_B);
                    mainPanel = new Panel();
                    mainPanel.AutoSize = true;
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 0);
                    mainPanel.BackColor = Color.FromArgb(SCRATCH_PANEL_COLOUR_R, SCRATCH_PANEL_COLOUR_G, SCRATCH_PANEL_COLOUR_B);
                    Controls.Add(mainPanel);

                    SetupHeader();
                    headerPanel.Location = new Point(headerPanel.Location.X,SCRATCH_HEADER_Y);
                    //color header approprately
                    headerPanel.BackColor = Color.Black;
                    foreach (Control con in headerPanel.Controls)
                        con.BackColor = Color.Black;
                    headerPanel.BringToFront();

                    for (int i = 0; i < mspaHeaderLink.Count(); ++i)
                    {
                        mspaHeaderLink[i].ForeColor = Color.White;
                    }
                    SetCandy(Writer.IconTypes.CUEBALL);

                    break;
                case Writer.Style.DOTA:
                    BackColor = Color.Black;
                    
                    mainPanel = new Panel();
                    mainPanel.BackColor = Color.Black;
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 0);
                    Controls.Add(mainPanel);
                    break;
                case Writer.Style.SHES8ACK:
                    BackColor = Color.White;

                    mainPanel = new Panel();
                    mainPanel.BackColor = Color.White;
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 0);
                    Controls.Add(mainPanel);
                    break;

                case Writer.Style.HOMOSUCK:
                case Writer.Style.GAMEOVER:
                    BackColor = Color.FromArgb(4, 35, 0);
                    mainPanel = new Panel();
                    mainPanel.BackColor = Color.FromArgb(4, 35, 0);
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 0);
                    Controls.Add(mainPanel);

                    if (s == Writer.Style.HOMOSUCK)
                        StyleHomosuck();
                    break;
                case Writer.Style.TRICKSTER:
                    BackColor = Color.White;

                    mainPanel = new Panel();
                    mainPanel.BackColor = Color.White;
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 24);
                    Controls.Add(mainPanel);

                    trick = new TricksterShit(this,db.GetTricksterShit());
                    break;
                case Writer.Style.SBAHJ:
                    BackColor = Color.FromArgb(7,7,236);

                    mainPanel = new Panel();
                    mainPanel.BackColor = Color.FromArgb(244, 0, 236);
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - mainPanel.Width / 2, 24);
                    Controls.Add(mainPanel);

                    SetupHeader();

                    headerPanel.BackColor = Color.Black;
                    break;

                default:
                    Debugger.Break();
                    break;
            }
            if (candyCorn[0].Image == null)
                SetCandy(Writer.IconTypes.CANDYCORN);


        }
        void SetCandy(Writer.IconTypes ic)
        {


            currentIcon = new ImageStream();
            currentIcon.loc = new MemoryStream(db.GetIcon(ic));
            currentIcon.gif = Image.FromStream(currentIcon.loc);
            foreach (var corn in candyCorn)
            {
                corn.Image = currentIcon.gif;
            }
            
        }
        void StyleHomosuck()
        {
            mainPanel.BackColor = Color.FromArgb(22, 133, 0);
            mainPanel.Location = new Point(mainPanel.Location.X, HEADER_HEIGHT);

            SetupHeader();
            //color header approprately
            headerPanel.BackColor = Color.FromArgb(4, 35, 0);
            foreach (Control con in headerPanel.Controls)
                con.BackColor = Color.FromArgb(4, 35, 0);
            headerPanel.BringToFront();

            for (int i = 0; i < mspaHeaderLink.Count(); ++i)
            {
                mspaHeaderLink[i].ForeColor = Color.FromArgb(46, 215, 58);  //im so done with constants but w/e TODO:
            }
            SetCandy(Writer.IconTypes.CALIBORNTOOTH);
        }
        void ShowLoadingScreen()
        {

            CleanComic();
            pageLoadingProgress = new ProgressBar();
            pageLoadingProgress.Style = ProgressBarStyle.Marquee;
            pageLoadingProgress.MarqueeAnimationSpeed = 32;
            pageLoadingProgress.Width = mainPanel.Width / 2;
            pageLoadingProgress.Location = new Point(mainPanel.Width / 4, mainPanel.Height / 2 - pageLoadingProgress.Height);
            progressLabel = new TextBox();
            progressLabel.Multiline = true;
            progressLabel.BackColor = mainPanel.BackColor;
            if (headerPanel != null)
                progressLabel.ForeColor = mspaHeaderLink[0].ForeColor;
            progressLabel.TextAlign = HorizontalAlignment.Center;
            progressLabel.Width = pageLoadingProgress.Width;
            progressLabel.Height = mainPanel.Height / 3;
            mainPanel.Controls.Add(pageLoadingProgress);
            mainPanel.Controls.Add(progressLabel);
            progressLabel.Location = new Point(pageLoadingProgress.Location.X, pageLoadingProgress.Location.Y + pageLoadingProgress.Height);
            Update();
        }
        void Reader_Closed(object sender, System.EventArgs e)
        {
            Program.Shutdown(this, db);
        }

        private void openArchiver_Click(object sender, EventArgs e)
        {
            Program.Open(db, true);
        }

        private void jumpPage_Click(object sender, EventArgs e)
        {
            WakeUpMr((int)numericUpDown1.Value);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void mrAjax_DoWork(object sender, DoWorkEventArgs e)
        {
            page = null;
            page = db.WaitPage(pageRequest,mrAjax);
        }

        private void goBack_Click(object sender, EventArgs e)
        {
            if(pageQueue.Count > 0)
                pageQueue.Pop();
            if (pageQueue.Count == 0)
            {
                if (page != null)
                    WakeUpMr(Writer.ValidRange(page.number - 1));
            }
            else
                WakeUpMr(pageQueue.Pop());
        }

        private void autoSave_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.autoSave = autoSave.Checked;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (page == null)
                return;
            Properties.Settings.Default.lastReadPage = page.number;
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            WakeUpMr(Properties.Settings.Default.lastReadPage);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void minimizeButton_Click_1(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("You can use the arrow keys to navigate (on pages without flash)! Left and right change pages, Up and down scroll, spacebar toggles pesterlogs. The Go Back button has the functional equivalent of the browser back button. 'H' hides and shows the UI. F5 refreshes the page, F11 toggles windowed fullscreen (disabled on Mono). Use the archiver to download all of Homestuck at once. Made by Cyberboss (/u/Cyberboss_JHCB). Report bugs at https://github.com/cybnetsurfe3011/MSPA-Reader. MSPA belongs to Hussie not me, don't take credit for or sell this.");
        }

        private void startOverButton_Click(object sender, EventArgs e)
        {
            WakeUpMr((int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.lastReadPage = (int)Writer.StoryBoundaries.HOMESTUCK_PAGE_ONE;
        }

        private void uiToggleButton_Click(object sender, EventArgs e)
        {
            if (uiToggleButton.Text == "Hide UI")
            {
                uiToggleButton.Text = "Show UI";
                goBack.Visible = false;
                jumpButton.Visible = false;
                numericUpDown1.Visible = false;
                saveButton.Visible = false;
                autoSave.Visible = false;
                deleteButton.Visible = false;
                helpButton.Visible = false;
                openArchiver.Visible = false;
                loadButton.Visible = false;
                startOverButton.Visible = false;
                flashWarning.Visible = false;
                toggleFullscreen.Visible = false;
                AcceptButton = null;
            }
            else
            {
                uiToggleButton.Text = "Hide UI";
                goBack.Visible = true;
                jumpButton.Visible = true;
                numericUpDown1.Visible = true;
                saveButton.Visible = true;
                autoSave.Visible = true;
                deleteButton.Visible = true;
                helpButton.Visible = true;
                openArchiver.Visible = true;
                loadButton.Visible = true;
                startOverButton.Visible = true;
#if !linux
                toggleFullscreen.Visible = true;
#endif
                if (pageContainsFlash)
                {
                    flashWarning.Visible = true;
                }
                AcceptButton = jumpButton;

            }
        }

        private void toggleFullscreen_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.fullscreen = !Properties.Settings.Default.fullscreen;
            if (Properties.Settings.Default.fullscreen)
            {

                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            }
            else
            {

                FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
            exitButton.Visible = Properties.Settings.Default.fullscreen;
            minimizeButton.Visible = Properties.Settings.Default.fullscreen;
            WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
            
            Reader_ResizeEnd(null, null);
        }

        private void monitorGameOver_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!monitorGameOver.CancellationPending)
            {
                System.Threading.Thread.Sleep(100);
                monitorGameOver.ReportProgress(0);
            }
        }

        void monitorGameOver_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (flash == null)
                return;
            Bitmap pix = new Bitmap(1, 1);
            mainPanel.DrawToBitmap(pix, new Rectangle(0, 0, 1, 1));
            BackColor = pix.GetPixel(0, 0);
            pix.Dispose();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Value = Writer.ValidRange((int)numericUpDown1.Value);
        }

    }
}
