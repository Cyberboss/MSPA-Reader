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
            this.AcceptButton = okButton;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Database.DataSource dSource;
            switch (dataSourceInput.SelectedIndex)
            {
                case 0:
                    dSource = Database.DataSource.SQLSERVER;
                    break;
                default:
                    MessageBox.Show("Invalid datasource!");
                    return;
            }
            Database db = new Database(dSource);
            db.Connect(ipInput.Text, usernameInput.Text, passwordInput.Text);
        }


    }
}
