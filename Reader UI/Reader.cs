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
    partial class Reader : ReaderConstants
    {

        Database db;
        Panel mainPanel = null, headerPanel = null;
        Label[] mspaHeaderLink = new Label[REGULAR_NUMBER_OF_HEADER_LABELS];
        PictureBox[] candyCorn = new PictureBox[REGULAR_NUMBER_OF_HEADER_CANDY_CORNS];
        public Reader(Database idb)
        {
            db = idb;
            InitializeComponent();
            FormClosed += Reader_Closed;
            numericUpDown1.Maximum = db.lastPage;
            numericUpDown1.Minimum = (int)Database.PagesOfImportance.HOMESTUCK_PAGE_ONE;
            numericUpDown1.Value = numericUpDown1.Minimum;
            WindowState = FormWindowState.Maximized;
            Shown += Reader_Shown;
            for (int i = 0; i < mspaHeaderLink.Count(); ++i)
                mspaHeaderLink[i] = null;
            for (int i = 0; i < candyCorn.Count(); ++i)
                candyCorn[i] = null;
        }

        void Reader_Shown(object sender, EventArgs e)
        {
            CurtainsUp();
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
        void CurtainsUp(Database.Style s = Database.Style.REGULAR)
        {
            Update();
            RemoveControl(mainPanel);
            for (int i = 0; i < mspaHeaderLink.Count(); ++i)
                RemoveControl(mspaHeaderLink[i]);
            for (int i = 0; i < candyCorn.Count(); ++i)
                RemoveControl(candyCorn[i]);
            RemoveControl(headerPanel);
            switch (s) { 
                case Database.Style.REGULAR:
                    BackColor = Color.FromArgb(REGULAR_BACK_COLOUR_R,REGULAR_BACK_COLOUR_G,REGULAR_BACK_COLOUR_B);
                    
                    mainPanel = new Panel();
                    mainPanel.Width = REGULAR_PANEL_WIDTH;
                    mainPanel.Height = REGULAR_PANEL_HEIGHT;
                    mainPanel.Location = new Point(this.Width / 2 - REGULAR_PANEL_WIDTH / 2, REGULAR_PANEL_Y_OFFSET);
                    mainPanel.BackColor = Color.FromArgb(REGULAR_PANEL_COLOUR_R, REGULAR_PANEL_COLOUR_G, REGULAR_PANEL_COLOUR_B);
                    Controls.Add(mainPanel);

                    SetupHeader();

                    break;
            }

        }
        void Reader_Closed(object sender, System.EventArgs e)
        {
            Program.Shutdown(this, db);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.Open(db, true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var pg = db.WaitPage((int)numericUpDown1.Value,false);
            MessageBox.Show("Page Loaded");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }


    }
}
