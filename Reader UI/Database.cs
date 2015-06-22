using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Reader_UI
{
    class Database
    {
        enum DataSource { SQLCOMPACT, SQLSERVER, MYSQL };

        readonly DataSource dSource;
        SqlConnection conn = null;

        private void ConnectSQLServer(string serverName, string username, string password)
        {
            conn = new SqlConnection("Data Source=" + serverName + ";Initial Catalog=MSPAArchive;User ID=" + username + ";Password=" + password);
            try
            {
                conn.Open();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not open connection ! ");
            }
        }
        public Database(DataSource source, bool create, string serverName, string username, string password)
        {
            dSource = source;
            switch (dSource){
                case DataSource.SQLSERVER:
                    ConnectSQLServer(serverName,username,password);
                    break;
                default:
                    throw new Exception();
            }
        }

    }
}
