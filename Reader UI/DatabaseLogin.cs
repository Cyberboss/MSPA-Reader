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
    public partial class DatabaseLogin : MSPAForm
    {
        public DatabaseLogin()
        {
            InitializeComponent();
            dataSourceInput.Items.Add("SQL Server");
            dataSourceInput.Items.Add("SQL LocalDB");
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
                db.Connect(ipInput.Text, usernameInput.Text, passwordInput.Text, resetDatabase.Checked);
                if (!db.Initialize())
                {
                    db.Close();
                    Cursor.Current = Cursors.Default;
                    dataSourceInput_SelectedIndexChanged(null, null);
                    return;
                }
                Hide();
                if (resetDatabase.Checked)
                {
                    Properties.Settings.Default.lastPage = (int)Database.PagesOfImportance.HOMESTUCK_PAGE_ONE;
                    Properties.Settings.Default.Save();
                }
                if(checkBox1.Checked)
                    Program.Open(db, false,true);
                //if (checkBox2.Checked)
                 //   Program.Open(db, true, true);
                Close();
            }
            catch
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Can not open connection to " + ipInput.Text + "! Check that the database MSPAArchive exists on the specified server and the user you entered has to dbo role.");
                dataSourceInput_SelectedIndexChanged(null, null);
            }
        }

        private void dataSourceInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            dbPathSelect.Visible = false;
            ipPathLabel.Text = "IP Address";
            switch (dataSourceInput.SelectedIndex)
            {
                case 0:
                    foreach (Control c in Controls)
                    {
                        c.Enabled = true;
                    }
                    ipInput.Text = "";
                    ipInput.ReadOnly = false;
                    break;
                case 1:
                    foreach (Control c in Controls)
                    {
                        c.Enabled = false;
                    }
                    dataSourceInput.Enabled = true;
                    okButton.Enabled = true;
                    dbPathSelect.Visible = true;
                    dbPathSelect.Enabled = true;
                    ipPathLabel.Enabled = true;
                    ipPathLabel.Text = "Database Folder";
                    ipInput.Text = Application.StartupPath;
                    ipInput.ReadOnly = true;
                    ipInput.Enabled = true;
                    label1.Enabled = true;
                    break;
                default:
                    MessageBox.Show("How are you managing to screw up a .NET program???");
                    break;
            }
            resetDatabase.Enabled = true;
            checkBox1.Enabled = true;
            checkBox2.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            okButton.Enabled = (!(!checkBox2.Checked && !checkBox1.Checked));
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            okButton.Enabled = (!(!checkBox2.Checked && !checkBox1.Checked));
        }

        private void dbPathSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog oFD = new FolderBrowserDialog();
            if (oFD.ShowDialog() == DialogResult.OK)
            {
                ipInput.Text = oFD.SelectedPath;
            }
        }



    }
}
