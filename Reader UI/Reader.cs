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
    partial class Reader : MSPAForm
    {
        Database db;
        public Reader(Database idb)
        {
            db = idb;
            InitializeComponent();
            FormClosed += Reader_Closed;
            System.Threading.Thread.Sleep(1000);
            numericUpDown1.Maximum = db.lastPage;
            numericUpDown1.Minimum = (int)Database.PagesOfImportance.HOMESTUCK_PAGE_ONE;
            numericUpDown1.Value = numericUpDown1.Minimum;
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
            db.WaitPage((int)numericUpDown1.Value,false);
            MessageBox.Show("Page Loaded");
        }

    }
}
