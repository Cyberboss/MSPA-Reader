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
            dataSourceInput.Items.Add("MySQL");
            dataSourceInput.Items.Add("SQLite (Warning: Data Races)");
#if !linux
            dataSourceInput.Items.Add("SQL LocalDB");
#endif
            if (Properties.Settings.Default.serverType != 4)
            {
                dataSourceInput.SelectedIndex = Properties.Settings.Default.serverType;
            }
            else
            {
#if linux
                dataSourceInput.SelectedIndex = 2;
                
#else
                dataSourceInput.SelectedIndex = 3;
#endif
            }
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
            if (String.IsNullOrWhiteSpace(databaseNameInput.Text))
            {
                MessageBox.Show("Database name can't be empty!");
                return;
            }

            string dbName,dbFName;
            Writer db;
            switch (dataSourceInput.SelectedIndex)
            {
                case 0:
                    dbName = databaseNameInput.Text;
                    dbFName = ipInput.Text;
                    db = new DatabaseManager(DatabaseManager.DBType.SQLSERVER);
                    break;
                case 1:
                    dbName = databaseNameInput.Text;
                    dbFName = ipInput.Text;
                    db = new DatabaseManager(DatabaseManager.DBType.MYSQL);
                    break;
                case 2:
                    dbName = System.IO.Path.GetFileNameWithoutExtension(databaseNameInput.Text);
                    dbFName = System.IO.Path.GetDirectoryName(databaseNameInput.Text);
                    db = new DatabaseManager(DatabaseManager.DBType.SQLITE);
                    break;
                case 3:
                    dbName = System.IO.Path.GetFileNameWithoutExtension(databaseNameInput.Text);
                    dbFName = System.IO.Path.GetDirectoryName(databaseNameInput.Text);
                    db = new DatabaseManager(DatabaseManager.DBType.SQLLOCALDB);
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    return;
            }
            
            foreach (Control c in Controls)
            {
                c.Enabled = false;
            }
            UseWaitCursor = true;
            new Initializing(db, dbName, dbFName, usernameInput.Text, passwordInput.Text, resetDatabase.Checked, OnInitilializerClose).Show();
        }
        void OnInitilializerClose(object sender, FormClosedEventArgs e)
        {
            var db = ((Initializing)sender).db;
            if (!((Initializing)sender).Good())
            {
                UseWaitCursor = false;
                dataSourceInput_SelectedIndexChanged(null, null);
                MessageBox.Show("Unable to initialze database!", "Error");
            }
            else
            {
                if (resetDatabase.Checked)
                {
                    Properties.Settings.Default.lastReadPage = (int)Writer.PagesOfImportance.HOMESTUCK_PAGE_ONE;
                }
                if (checkBox1.Checked)
                    Program.Open(db, false, true);
                if (checkBox2.Checked)
                    Program.Open(db, true, true);


                if (dataSourceInput.SelectedIndex == 0
                    || dataSourceInput.SelectedIndex == 1)
                {
                    Properties.Settings.Default.ip = ipInput.Text;
                    Properties.Settings.Default.dbName = databaseNameInput.Text;
                }
                else
                {
                    Properties.Settings.Default.dbFileName = System.IO.Path.GetDirectoryName(databaseNameInput.Text) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(databaseNameInput.Text);
                }

                if (saveUsername.Checked)
                {
                    Properties.Settings.Default.saveUsername = true;
                    Properties.Settings.Default.username = usernameInput.Text;
                    if (savePassword.Checked)
                    {
                        Properties.Settings.Default.savePassword = true;
                        Properties.Settings.Default.password = passwordInput.Text;
                    }
                    else
                    {
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
        }

        private void dataSourceInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            dbPathSelect.Visible = false;
            ipPathLabel.Text = "IP Address";
            Properties.Settings.Default.serverType = dataSourceInput.SelectedIndex;
            switch (dataSourceInput.SelectedIndex)
            {
                case 0:
                case 1:
                    foreach (Control c in Controls)
                    {
                        c.Enabled = true;
                    }
                    ipInput.Text = Properties.Settings.Default.ip == "" ? "127.0.0.1" : Properties.Settings.Default.ip;
                    databaseNameInput.Text = Properties.Settings.Default.dbName;
                    databaseNameInput.ReadOnly = false;
                    break;
                case 2:
                case 3:
                    foreach (Control c in Controls)
                    {
                        c.Enabled = false;
                    }
                    dataSourceInput.Enabled = true;
                    okButton.Enabled = true;
                    dbPathSelect.Visible = true;
                    dbPathSelect.Enabled = true;

                    dbNameLabel.Enabled = true;
                    databaseNameInput.Text = Properties.Settings.Default.dbFileName == "" ? Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "MSPAArchive" : Properties.Settings.Default.dbFileName;
                    databaseNameInput.ReadOnly = true;
                    if (dataSourceInput.SelectedIndex == 3)
                    {
                        databaseNameInput.Text += ".mdf";
                    }
                    else
                    {
                        databaseNameInput.Text += ".sqlite3";
                    }
                    databaseNameInput.ReadOnly = true;
                    databaseNameInput.Enabled = true;
                    label1.Enabled = true;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
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
            var oFD = new SaveFileDialog();
            oFD.OverwritePrompt = false;
            oFD.Title = "Open or Create Database";
            if (dataSourceInput.SelectedIndex == 3)
            {
                oFD.Filter = "SQL Server Databases (*.mdf)|*.mdf";
                oFD.DefaultExt = ".mdf";
            }
            else
            {
                oFD.Filter = "SQLite3 Databases (*.sqlite3)|*.sqlite3";
                oFD.DefaultExt = ".sqlite3";
            }
            if (oFD.ShowDialog() == DialogResult.OK)
            {
                databaseNameInput.Text = oFD.FileName;
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
