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
            dataSourceInput.Items.Add("SQL LocalDB");
            dataSourceInput.Items.Add("SQLite");
            dataSourceInput.SelectedIndex = Properties.Settings.Default.serverType;
            dataSourceInput_SelectedIndexChanged(null, null);
            saveUsername.Checked = Properties.Settings.Default.saveUsername;
            savePassword.Checked = Properties.Settings.Default.savePassword;

            if (Properties.Settings.Default.saveUsername)
            {
                usernameInput.Text = Properties.Settings.Default.username;
                if (Properties.Settings.Default.savePassword)
                    passwordInput.Text = Properties.Settings.Default.password;
            }

            AcceptButton = okButton;
            FormClosed += DatabaseLogin_Closed;
        }
        private void DatabaseLogin_Closed(object sender, EventArgs e)
        {
            Program.Shutdown(null, null);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Writer db;
            switch (dataSourceInput.SelectedIndex)
            {
                case 0:
                    db = new SQL(SQL.DBType.SQLSERVER);
                    break;
                case 1:
                    db = new SQL(SQL.DBType.SQLLOCALDB);
                    break;
                case 2:
                    db = new SQL(SQL.DBType.SQLITE);
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
                    Properties.Settings.Default.lastReadPage = (int)Writer.PagesOfImportance.HOMESTUCK_PAGE_ONE;
                }
                if(checkBox1.Checked)
                    Program.Open(db, false,true);
                if (checkBox2.Checked)
                    Program.Open(db, true,true);

                if (dataSourceInput.SelectedIndex != 1)
                    Properties.Settings.Default.ip = ipInput.Text;

                if (saveUsername.Checked)
                {
                    Properties.Settings.Default.saveUsername = true;
                    Properties.Settings.Default.username = usernameInput.Text;
                    if (savePassword.Checked)
                    {
                        Properties.Settings.Default.savePassword = true;
                        Properties.Settings.Default.password = passwordInput.Text;
                    }
                    else {
                        Properties.Settings.Default.savePassword = false;
                        Properties.Settings.Default.password = "";
                    }
                }
                else
                {
                    Properties.Settings.Default.saveUsername = false;
                    Properties.Settings.Default.username = "";
                    Properties.Settings.Default.savePassword = false;
                    Properties.Settings.Default.password = "";
                }


                Close();
            }
            catch
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Can not open connection to " + ipInput.Text + "! Check that the database MSPAArchive exists on the specified server and the user you entered has to dbo role.");
                var oldIp = ipInput.Text;
                dataSourceInput_SelectedIndexChanged(null, null);
                ipInput.Text = oldIp;
            }
        }

        private void dataSourceInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            dbPathSelect.Visible = false;
            ipPathLabel.Text = "IP Address";
            Properties.Settings.Default.serverType = dataSourceInput.SelectedIndex;
            switch (dataSourceInput.SelectedIndex)
            {
                case 0:
                    foreach (Control c in Controls)
                    {
                        c.Enabled = true;
                    }
                    ipInput.Text = Properties.Settings.Default.ip == "" ? "127.0.0.1" : Properties.Settings.Default.ip;
                    ipInput.ReadOnly = false;
                    break;
                case 1:
                case 2:
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

        private void savePassword_CheckedChanged(object sender, EventArgs e)
        {
            saveUsername.Checked |= savePassword.Checked;
        }

        private void saveUsername_CheckedChanged(object sender, EventArgs e)
        {
            savePassword.Checked &= saveUsername.Checked;
        }




    }
}
