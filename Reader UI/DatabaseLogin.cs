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
            dataSourceInput.Items.Add("SQL LocalDB: Database/MSPAArchive.mdf");
            dataSourceInput.SelectedIndex = 1;
            AcceptButton = okButton;
            FormClosed += DatabaseLogin_Closed;
        }
        private void DatabaseLogin_Closed(object sender, EventArgs e)
        {
            Program.Shutdown(null, null);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Database db;
            switch (dataSourceInput.SelectedIndex)
            {
                case 0:
                    db = new SQLServerDatabase(false);
                    break;
                case 1:
                    db = new SQLServerDatabase(true);
                    break;
                default:
                    MessageBox.Show("Invalid database selection.... How???");
                    return;
            }
            
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
                Program.Open(db, false);
                Program.Open(db, true);
                Close();
            }
            catch (Exception)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Can not open connection to " + ipInput.Text + "! Check that the database MSPAArchive exists on the specified server and the user you entered has to dbo role.");
                dataSourceInput_SelectedIndexChanged(null, null);
            }
        }

        private void dataSourceInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (dataSourceInput.SelectedIndex)
            {
                case 0:
                    foreach (Control c in Controls)
                    {
                        c.Enabled = true;
                    }
                    break;
                case 1:
                    foreach (Control c in Controls)
                    {
                        c.Enabled = false;
                    }
                    dataSourceInput.Enabled = true;
                    okButton.Enabled = true;
                    label1.Enabled = true;
                    break;
                default:
                    MessageBox.Show("How are you managing to screw up a .NET program???");
                    break;
            }
        }


    }
}
