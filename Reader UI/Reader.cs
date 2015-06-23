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
    partial class Reader : Form
    {
        Database db;
        public Reader(Database idb)
        {
            db = idb;
            InitializeComponent();
            FormClosed += Reader_Closed;
        }
        void Reader_Closed(object sender, System.EventArgs e)
        {
            Program.Shutdown(this, db);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.Open(db, true);
        }

    }
}
