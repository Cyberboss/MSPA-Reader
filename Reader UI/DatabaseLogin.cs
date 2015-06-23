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
    public partial class DatabaseLogin : Form
    {
        public DatabaseLogin()
        {
            InitializeComponent();
            dataSourceInput.Items.Add("SQL Server");
            dataSourceInput.SelectedIndex = 0;
            this.AcceptButton = okButton;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Database db = new SQLServerDatabase();
            foreach (Control c in Controls)
            {
                c.Enabled = false;
            }
            Cursor.Current = Cursors.WaitCursor;
            Update();
            try
            {
                db.Connect(ipInput.Text, usernameInput.Text, passwordInput.Text);
                Hide();
                new Reader(db).Show();
                Close();
            }
            catch (Exception)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Can not open connection to " + ipInput.Text + "! Check that the database MSPAArchive exists on the specified server and the user you entered has to dbo role.");
                foreach (Control c in Controls)
                {
                    c.Enabled = true;
                }
            }
        }


    }
}
