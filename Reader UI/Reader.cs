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

        Database db;
        Panel mainPanel = null, headerPanel = null, comicPanel = null;
        Label[] mspaHeaderLink = new Label[REGULAR_NUMBER_OF_HEADER_LABELS];
        PictureBox[] candyCorn = new PictureBox[REGULAR_NUMBER_OF_HEADER_CANDY_CORNS];
        ProgressBar pageLoadingProgress = null;
        int pageRequest;
        Database.Page page = null;
        Database.Style previousStyle;
        AxShockwaveFlashObjects.AxShockwaveFlash flash = null;

        class GifStream
        {
            public PictureBox gif;
            public System.IO.MemoryStream loc;
        }

        //page stuff
        GrowLabel title = null;
        List<GifStream> gifs = new List<GifStream>();
        GrowLabel narrative = null;
        Label linkPrefix = null;
        LinkLabel next = null, tereziPassword = null;
        Panel pesterlog = null;

        public Reader(Database idb)
        {
            db = idb;
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            FormClosed += Reader_Closed;
            numericUpDown1.Maximum = db.lastPage;
            numericUpDown1.Minimum = (int)Database.PagesOfImportance.HOMESTUCK_PAGE_ONE;
            numericUpDown1.Value = numericUpDown1.Minimum;
            AcceptButton = jumpButton;
            Shown += Reader_Shown;
            for (int i = 0; i < mspaHeaderLink.Count(); ++i)
                mspaHeaderLink[i] = null;
            for (int i = 0; i < candyCorn.Count(); ++i)
                candyCorn[i] = null;
            mrAjax.RunWorkerCompleted += mrAjax_RunWorkerCompleted;
            FormClosing += Reader_FormClosing;
            Resize += Reader_Resize;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if(page != null)
            switch (keyData)
            {
                case Keys.F5:
                    WakeUpMr(page.number);
                    return true;
                case Keys.Left:
                    if (page.number > (int)Database.PagesOfImportance.HOMESTUCK_PAGE_ONE)
                        WakeUpMr(page.number - 1);
                    return true;
                case Keys.Right:
                    if (page.number < db.lastPage)
                        WakeUpMr(page.number + 1);
                    return true;
            }
            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
         }

        void Reader_Resize(object sender, EventArgs e)
        {
            Location = new Point(0, 0);
            WindowState = FormWindowState.Maximized;
        }


        void Reader_FormClosing(object sender, FormClosingEventArgs e)
        {
            CleanControls();
        }

        void mrAjax_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (page == null)
            {
                MessageBox.Show("An internal error occurred when retrieving the page. Usually this means you are out of memory or some resource on the page on mspa is 404'ing.");
            }else
                LoadPage();
        }

        void LoadPage()
        {
            var newStyle = db.GetStyle(pageRequest);
            if(previousStyle != newStyle)
                CurtainsUp(newStyle);
            switch (newStyle)
            {
                case Database.Style.REGULAR:
                    LoadRegularPage();
                    break;
            }

            //dump the garbage
            page.meta = null;
            page.meta2 = null;
            page.resources2 = null;
            page.resources = null;

            //fix scroll bar
            Update();
            AutoScrollPosition = new Point(0, 0);
        }
        //https://stackoverflow.com/questions/1874077/loading-a-flash-movie-from-a-memory-stream-or-a-byte-array
        private void InitFlashMovie(AxShockwaveFlashObjects.AxShockwaveFlash flashObj, byte[] swfFile)
        {
            using (System.IO.MemoryStream stm = new System.IO.MemoryStream())
            {
                using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stm))
                {
                    /* Write length of stream for AxHost.State */
                    writer.Write(8 + swfFile.Length);
                    /* Write Flash magic 'fUfU' */
                    writer.Write(0x55665566);
                    /* Length of swf file */
                    writer.Write(swfFile.Length);
                    writer.Write(swfFile);
                    stm.Seek(0, System.IO.SeekOrigin.Begin);
                    /* 1 == IPeristStreamInit */
                    flashObj.OcxState = new AxHost.State(stm, 1, false, null);
                    
                    
                }
            }
        }
        void SetFlashDimensions()
        {
            switch (page.number) { 
                case (int)Database.PagesOfImportance.CASCADE:
                    flash.Width = 950;
                    flash.Height = 650;
                    break;
                default:
                    flash.Width = REGULAR_FLASH_MOVIE_WIDTH;
                    flash.Height = REGULAR_FLASH_MOVIE_HEIGHT;
                    break;
            }
        }
        void LoadRegularPage()
        {
            comicPanel = new Panel();
            comicPanel.Width = REGULAR_COMIC_PANEL_WIDTH;
            comicPanel.Location = new Point(mainPanel.Width / 2 - comicPanel.Width / 2, REGULAR_COMIC_PANEL_Y_OFFSET);
            comicPanel.BackColor = Color.FromArgb(REGULAR_COMIC_PANEL_COLOUR_R, REGULAR_COMIC_PANEL_COLOUR_G, REGULAR_COMIC_PANEL_COLOUR_B);
            mainPanel.Controls.Add(comicPanel);

            title = new GrowLabel();
            title.Width = REGULAR_PAGE_TITLE_WIDTH;
            title.TextAlign = HorizontalAlignment.Center;
            title.Font = new System.Drawing.Font("Courier New", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title.Text = page.meta.title;
            comicPanel.Controls.Add(title);
            title.Location = new Point(comicPanel.Width / 2 - title.Width / 2, REGULAR_TITLE_Y_OFFSET);

            int currentHeight = title.Location.Y + title.Height + REGULAR_TITLE_Y_OFFSET;
            bool flashflag= false;
            for (int i = 0; i < page.resources.Count(); i++)
            {
                if (!flashflag && !Parser.IsGif(page.resources[i].originalFileName))
                {
                    flash = new AxShockwaveFlashObjects.AxShockwaveFlash();
                    comicPanel.Controls.Add(flash);
                    flash.Enabled = true;
                    //flash.AutoSize = true;
                    flash.ScaleMode = 1;
                    flash.AlignMode = 0;
                    InitFlashMovie(flash, page.resources[i].data);
                    //FlashHeaderReader swfreader = new FlashHeaderReader(page.resources[i].data);
                    SetFlashDimensions();


                    flash.Location = new Point(comicPanel.Width / 2 - flash.Width / 2, currentHeight);
                    currentHeight += flash.Height;
                    flash.Play();
                    flashflag = true;
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
                    if (i < page.resources.Count() - 1)
                        currentHeight += REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET;
                    gifs.Add(tempPB);
                }
            }

            currentHeight+=REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;

            int leftSide;
            if (page.meta.narr != null)
            {
                    narrative = new GrowLabel();
                    narrative.Width = REGULAR_NARRATIVE_WIDTH;
                    narrative.TextAlign = HorizontalAlignment.Center;
                    narrative.Font = new System.Drawing.Font("Courier New", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    narrative.Text = page.meta.narr.text.Trim();
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

                pesterlog.MaximumSize = new Size(REGULAR_PESTERLOG_WIDTH, Int32.MaxValue);
                pesterlog.BorderStyle = BorderStyle.FixedSingle;
                leftSide = comicPanel.Width / 2 - pesterlog.Width / 2;
                pesterlog.Location = new Point(leftSide, currentHeight);
                comicPanel.Controls.Add(pesterlog);

                currentHeight += pesterlog.Height;
            }

            currentHeight += REGULAR_SPACE_BETWEEN_CONTENT_AND_TEXT;

            if (page.links.Count() > 0)
            {

                linkPrefix = new Label();
                linkPrefix.AutoSize = true;
                linkPrefix.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                linkPrefix.Text = REGULAR_LINK_PREFIX;
                linkPrefix.Location = new Point(leftSide, currentHeight);
                comicPanel.Controls.Add(linkPrefix);    

                next = new GrowLinkLabel();
                next.Width = 600;
                next.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                next.Text = "    " + page.links[0].originalText;
                next.Location = new Point(leftSide, currentHeight);
                next.LinkClicked += next_LinkClicked;
                comicPanel.Controls.Add(next);

                linkPrefix.BringToFront();
            }

            comicPanel.Height = currentHeight + REGULAR_COMIC_PANEL_BOTTOM_PADDING;

            mainPanel.Height = comicPanel.Height + REGULAR_COMIC_PANEL_Y_OFFSET + REGULAR_COMIC_PANEL_BOTTOM_Y_OFFSET;

            RemoveControl(pageLoadingProgress);
        }

        void next_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            WakeUpMr(page.links[0].pageNumber);

        }
        void WakeUpMr(int pg)
        {

            if (mrAjax.IsBusy)
                return;
            numericUpDown1.Value = pg;
            ShowLoadingScreen();
            pageRequest = pg;
            mrAjax.RunWorkerAsync();
        }
        void Reader_Shown(object sender, EventArgs e)
        {
            Location = new Point(0, 0);
            WindowState = FormWindowState.Maximized;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            CurtainsUp();
            pageRequest = (int)Database.PagesOfImportance.HOMESTUCK_PAGE_ONE;
            mrAjax.RunWorkerAsync();
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
            headerPanel.AutoSize = true;

            mspaHeaderLink[0] = new Label();
            mspaHeaderLink[0].AutoSize = true;
            mspaHeaderLink[0].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[0].ForeColor = System.Drawing.Color.White;
            mspaHeaderLink[0].Location = new System.Drawing.Point(0, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[0].Text = REGULAR_LABEL_TEXT_1;
            headerPanel.Controls.Add(mspaHeaderLink[0]);

            candyCorn[0] = new PictureBox();
            candyCorn[0].Width = REGULAR_CANDYCORN_WIDTH;
            candyCorn[0].Height = REGULAR_CANDYCORN_HEIGHT;
            candyCorn[0].Location = new Point(mspaHeaderLink[0].Location.X + mspaHeaderLink[0].Width + REGULAR_HEADER_X_OFFSET, REGULAR_CANDYCORN_Y_OFFSET);
            candyCorn[0].Image = Properties.Resources.candyCorn;
            headerPanel.Controls.Add(candyCorn[0]);

            mspaHeaderLink[1] = new Label();
            mspaHeaderLink[1].AutoSize = true;
            mspaHeaderLink[1].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[1].ForeColor = Color.FromArgb(REGULAR_HEADER_GREEN_R, REGULAR_HEADER_GREEN_G, REGULAR_HEADER_GREEN_B);
            mspaHeaderLink[1].Location = new System.Drawing.Point(candyCorn[0].Location.X + candyCorn[0].Width + REGULAR_HEADER_X_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[1].Text = REGULAR_LABEL_TEXT_2;
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
            headerPanel.Controls.Add(mspaHeaderLink[3]);

            candyCorn[1] = new PictureBox();
            candyCorn[1].Width = REGULAR_CANDYCORN_WIDTH;
            candyCorn[1].Height = REGULAR_CANDYCORN_HEIGHT;
            candyCorn[1].Location = new Point(mspaHeaderLink[3].Location.X + mspaHeaderLink[3].Width + REGULAR_HEADER_X_OFFSET, REGULAR_CANDYCORN_Y_OFFSET);
            candyCorn[1].Image = Properties.Resources.candyCorn;
            headerPanel.Controls.Add(candyCorn[1]);

            mspaHeaderLink[4] = new Label();
            mspaHeaderLink[4].AutoSize = true;
            mspaHeaderLink[4].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[4].ForeColor = Color.FromArgb(REGULAR_HEADER_BLUE_R, REGULAR_HEADER_BLUE_G, REGULAR_HEADER_BLUE_B);
            mspaHeaderLink[4].Location = new System.Drawing.Point(candyCorn[1].Location.X + candyCorn[1].Width + REGULAR_HEADER_X_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[4].Text = REGULAR_LABEL_TEXT_5;
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
            headerPanel.Controls.Add(mspaHeaderLink[8]); 
            
            candyCorn[2] = new PictureBox();
            candyCorn[2].Width = REGULAR_CANDYCORN_WIDTH;
            candyCorn[2].Height = REGULAR_CANDYCORN_HEIGHT;
            candyCorn[2].Location = new Point(mspaHeaderLink[8].Location.X + mspaHeaderLink[8].Width + REGULAR_HEADER_X_OFFSET, REGULAR_CANDYCORN_Y_OFFSET);
            candyCorn[2].Image = Properties.Resources.candyCorn;
            headerPanel.Controls.Add(candyCorn[2]);

            mspaHeaderLink[9] = new Label();
            mspaHeaderLink[9].AutoSize = true;
            mspaHeaderLink[9].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[9].ForeColor = Color.FromArgb(REGULAR_HEADER_YELLOW_R, REGULAR_HEADER_YELLOW_G, REGULAR_HEADER_YELLOW_B);
            mspaHeaderLink[9].Location = new System.Drawing.Point(candyCorn[2].Location.X + candyCorn[2].Width + REGULAR_HEADER_X_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[9].Text = REGULAR_LABEL_TEXT_10;
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
            headerPanel.Controls.Add(mspaHeaderLink[11]);

            candyCorn[3] = new PictureBox();
            candyCorn[3].Width = REGULAR_CANDYCORN_WIDTH;
            candyCorn[3].Height = REGULAR_CANDYCORN_HEIGHT;
            candyCorn[3].Location = new Point(mspaHeaderLink[11].Location.X + mspaHeaderLink[11].Width + REGULAR_HEADER_X_OFFSET, REGULAR_CANDYCORN_Y_OFFSET);
            candyCorn[3].Image = Properties.Resources.candyCorn;
            headerPanel.Controls.Add(candyCorn[3]);

            mspaHeaderLink[12] = new Label();
            mspaHeaderLink[12].AutoSize = true;
            mspaHeaderLink[12].Font = new System.Drawing.Font("Arial", 7F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mspaHeaderLink[12].ForeColor = Color.FromArgb(REGULAR_HEADER_ORANGE_R, REGULAR_HEADER_ORANGE_G, REGULAR_HEADER_ORANGE_B);
            mspaHeaderLink[12].Location = new System.Drawing.Point(candyCorn[3].Location.X + candyCorn[3].Width + REGULAR_HEADER_X_OFFSET, REGULAR_MSPAHEADERLINK_Y_OFFSET);
            mspaHeaderLink[12].Text = REGULAR_LABEL_TEXT_13;
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
            headerPanel.Controls.Add(mspaHeaderLink[16]);

            Controls.Add(headerPanel);

            Update();

            headerPanel.Location = new Point(this.Width / 2 - headerPanel.Width / 2, 0);
            
        }
        void CleanComic()
        {

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
            RemoveControl(flash);
            RemoveControl(pesterlog);

            RemoveControl(comicPanel);
        }
        void CleanControls()
        {
            CleanComic();

            RemoveControl(mainPanel);
            for (int i = 0; i < mspaHeaderLink.Count(); ++i)
                RemoveControl(mspaHeaderLink[i]);
            for (int i = 0; i < candyCorn.Count(); ++i)
                RemoveControl(candyCorn[i]);
            RemoveControl(headerPanel);
            RemoveControl(pageLoadingProgress);
        }
        void CurtainsUp(Database.Style s = Database.Style.REGULAR)
        {
            previousStyle = s;

            CleanControls();

            switch (s) { 
                case Database.Style.REGULAR:
                    BackColor = Color.FromArgb(REGULAR_BACK_COLOUR_R,REGULAR_BACK_COLOUR_G,REGULAR_BACK_COLOUR_B);
                    
                    mainPanel = new Panel();
                    mainPanel.AutoSize = true;
                    mainPanel.MaximumSize = new System.Drawing.Size(REGULAR_PANEL_WIDTH, Int32.MaxValue);
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Location = new Point(this.Width / 2 - REGULAR_PANEL_WIDTH / 2, REGULAR_PANEL_Y_OFFSET);
                    mainPanel.BackColor = Color.FromArgb(REGULAR_PANEL_COLOUR_R, REGULAR_PANEL_COLOUR_G, REGULAR_PANEL_COLOUR_B);
                    Controls.Add(mainPanel);

                    SetupHeader();

                    ShowLoadingScreen();

                    break;
            }

        }
        void ShowLoadingScreen()
        {

            CleanComic();
            pageLoadingProgress = new ProgressBar();
            pageLoadingProgress.Style = ProgressBarStyle.Marquee;
            pageLoadingProgress.MarqueeAnimationSpeed = 32;
            pageLoadingProgress.Width = mainPanel.Width / 2;
            pageLoadingProgress.Location = new Point(mainPanel.Width / 4, mainPanel.Height / 2 - pageLoadingProgress.Height);
            mainPanel.Controls.Add(pageLoadingProgress);
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
            page = db.WaitPage(pageRequest);
        }

        private void goBack_Click(object sender, EventArgs e)
        {
            if(page.number > (int)Database.PagesOfImportance.HOMESTUCK_PAGE_ONE)
                WakeUpMr(page.number - 1);
        }



    }
}
