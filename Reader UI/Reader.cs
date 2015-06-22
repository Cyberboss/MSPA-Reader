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
    public partial class Reader : Form
    {
        Database db;
        public Reader(Database idb)
        {
            db = idb;
            InitializeComponent();
            FormClosed += Reader_Closed;
            dbWriter.RunWorkerAsync();
        }
        void Reader_Closed(object sender, System.EventArgs e)
        {
            db.Close();
            Application.Exit();
        }

        private void dbWriter_DoWork(object sender, DoWorkEventArgs e)
        {
            db.ResumeWork();
        }
    }
}
