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
        public enum DataSource { SQLCOMPACT, SQLSERVER, MYSQL, SQLITE };

        readonly DataSource dSource;
        SqlConnection sqlsConn = null;

        void ConnectSQLServer(string serverName, string username, string password)
        {
            sqlsConn = new SqlConnection("Data Source=" + serverName + ";Initial Catalog=MSPAArchive;User ID=" + username + ";Password=" + password);
            try
            {
                sqlsConn.Open();
            }
            catch (Exception)
            {
                sqlsConn = null;
                MessageBox.Show("Can not open connection to "+serverName+"! Check that the database MSPAArchive exists on the specified server and the user you entered has to dbo role.");
            }
            Environment.Exit(0);
        }
        public void Connect(string serverName, string username, string password)
        {
            switch (dSource)
            {
                case DataSource.SQLSERVER:
                    ConnectSQLServer(serverName, username, password);
                    break;
            }
        }
        public Database(DataSource source)
        {
            dSource = source;
        }
        ~Database()
        {
            switch (dSource)
            {
                case DataSource.SQLSERVER:
                    if (sqlsConn != null)
                        sqlsConn.Close();
                    break;
            }
        }
    }
}
