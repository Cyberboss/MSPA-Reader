using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Reader_UI
{
    public class Database
    {
        public enum DataSource { SQLCOMPACT, SQLSERVER, MYSQL, SQLITE };

        Parser parser;

        enum PagesOfImportance
        {
            HOMESTUCK_PAGE_ONE = 001901,
            LAST_PAGE = 009594  //TODO: Add some dynamic page calculator
        }

        readonly DataSource dSource;
        SqlConnection sqlsRConn = null,sqlsWConn = null;
        string connectionString = null;

        void ConnectSQLServer(string serverName, string username, string password, bool read)
        {
            if (read)
            {
                connectionString = "Data Source=" + serverName + ";MultipleActiveResultSets=true;Initial Catalog=MSPAArchive;User ID=" + username + ";Password=" + password;
                sqlsRConn = new SqlConnection(connectionString);
                sqlsRConn.Open();
            }
            else
            {
                sqlsWConn = new SqlConnection(connectionString);
                sqlsWConn.Open();
            }
        }
        public void Connect(string serverName, string username, string password, bool read = true)
        {
            switch (dSource)
            {
                case DataSource.SQLSERVER:
                    ConnectSQLServer(serverName, username, password, read);
                    break;
            }
        }
        int SQLSReadLastIndexedOrCreateDatabase()
        {
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT MAX(page_id) FROM PagesArchived",sqlsWConn);
                myReader = myCommand.ExecuteReader();
                myReader.Read();
                int res = myReader.GetInt32(0);
                myReader.Close();
                return res;
            }
            catch (Exception)
            {
                //Assume databse either
                //a) hasn't been created
                //b) is corrupt
                //c) hasn't parsed page 1

                //drop any tables that may exist
                try { new SqlCommand("DROP TABLE Conversations", sqlsWConn).ExecuteNonQuery(); }
                catch (Exception) { }
                try { new SqlCommand("DROP TABLE Links", sqlsWConn).ExecuteNonQuery(); }
                catch (Exception) { }
                try { new SqlCommand("DROP TABLE PagesArchived", sqlsWConn).ExecuteNonQuery(); }
                catch (Exception) { }
                try { new SqlCommand("DROP TABLE Resources", sqlsWConn).ExecuteNonQuery(); }
                catch (Exception) { }
                try
                {
                    SqlCommand creationCommands = new SqlCommand("CREATE TABLE [Conversations](	[id] [int] NOT NULL,	[page_id] [int] NOT NULL,	[text] [nvarchar](max) NULL, CONSTRAINT [PK_Conversations] PRIMARY KEY CLUSTERED (	[id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]",sqlsWConn);

                    creationCommands.ExecuteNonQuery();
                    creationCommands.CommandText = "CREATE TABLE [Links](	[id] [int] NOT NULL,	[page_id] [int] NOT NULL,	[linked_page_id] [int] NULL,	[link_text] [nvarchar](50) NULL, CONSTRAINT [PK_Links] PRIMARY KEY CLUSTERED (	[id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]";
                    creationCommands.ExecuteNonQuery();
                    creationCommands.CommandText = "CREATE TABLE [PagesArchived](	[page_id] [int] NOT NULL, CONSTRAINT [PK_PagesArchived] PRIMARY KEY CLUSTERED (	[page_id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]";
                    creationCommands.ExecuteNonQuery();
                    creationCommands.CommandText = "CREATE TABLE [Resources](	[id] [int] NOT NULL,	[page_id] [int] NOT NULL,	[data] [varbinary](max) NULL,	[original_filename] [nvarchar](max) NULL, CONSTRAINT [PK_Resources] PRIMARY KEY CLUSTERED (	[id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";
                    creationCommands.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    MessageBox.Show("Error creating database, make sure the specified account has read/write permissions.");
                    Application.Exit();
                }
                return 0;
            }
        }
        int ReadLastIndexedOrCreateDatabase()
        {
            switch (dSource)
            {
                case DataSource.SQLSERVER:
                    return SQLSReadLastIndexedOrCreateDatabase();
                default:
                    throw new Exception();
            }
        }

        public void ResumeWork()
        {
            parser = new Parser();
            Connect(null, null, null, false);
            int currentPage = ReadLastIndexedOrCreateDatabase() + 1;
            if (currentPage == 1)
                currentPage = (int)PagesOfImportance.HOMESTUCK_PAGE_ONE;

            while (currentPage < (int)PagesOfImportance.LAST_PAGE)
            {
                parser.LoadPage(currentPage);
            }
        }
        public Database(DataSource source)
        {
            dSource = source;
        }
        public void Close()
        {
            switch (dSource)
            {
                case DataSource.SQLSERVER:
                    if (sqlsRConn != null)
                    {
                        sqlsRConn.Close();
                        sqlsRConn = null;
                    }
                    if (sqlsWConn != null)
                    {
                        sqlsWConn.Close();
                        sqlsRConn = null;
                    }
                    break;
            }
        }
    }
}
