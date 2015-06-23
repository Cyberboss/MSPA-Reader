using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Reader_UI
{
    class SQLServerDatabase : Database
    {

        SqlConnection sqlsRConn = null, sqlsWConn = null;
        SqlTransaction sqlsTrans = null;
        string connectionString = null;

        override public void Connect(string serverName, string username, string password)
        {
            connectionString = "Data Source=" + serverName + ";Initial Catalog=MSPAArchive;User ID=" + username + ";Password=" + password;
            sqlsRConn = new SqlConnection("MultipleActiveResultSets=true;" + connectionString);
            sqlsRConn.Open();
            sqlsWConn = new SqlConnection(connectionString);
            sqlsWConn.Open();
            
        }
        override public bool ReadLastIndexedOrCreateDatabase()
        {
            SqlDataReader myReader = null;
            try
            {
                SqlCommand myCommand = new SqlCommand("SELECT * FROM PagesArchived", sqlsWConn);
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    archivedPages.Add(myReader.GetInt32(0));
                }
                myReader.Close();
                return true;
            }
            catch (Exception)
            {
                try { myReader.Close(); }
                catch (Exception) { }
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
                    SqlCommand creationCommands = new SqlCommand("CREATE TABLE [Conversations](	[id] [int] NOT NULL IDENTITY (1,1),	[page_id] [int] NOT NULL,	[text] [nvarchar](max) NULL, CONSTRAINT [PK_Conversations] PRIMARY KEY CLUSTERED (	[id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]", sqlsWConn);

                    creationCommands.ExecuteNonQuery();
                    creationCommands.CommandText = "CREATE TABLE [Links](	[id] [int] NOT NULL IDENTITY (1,1),	[page_id] [int] NOT NULL,	[linked_page_id] [int] NULL,	[link_text] [nvarchar](50) NULL, CONSTRAINT [PK_Links] PRIMARY KEY CLUSTERED (	[id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]";
                    creationCommands.ExecuteNonQuery();
                    creationCommands.CommandText = "CREATE TABLE [PagesArchived](	[page_id] [int] NOT NULL, CONSTRAINT [PK_PagesArchived] PRIMARY KEY CLUSTERED (	[page_id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]";
                    creationCommands.ExecuteNonQuery();
                    creationCommands.CommandText = "CREATE TABLE [Resources](	[id] [int] NOT NULL IDENTITY (1,1),	[page_id] [int] NOT NULL,	[data] [varbinary](max) NULL,	[original_filename] [nvarchar](max) NULL, CONSTRAINT [PK_Resources] PRIMARY KEY CLUSTERED (	[id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";
                    creationCommands.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    MessageBox.Show("Error creating database, make sure the specified account has read/write permissions.");
                    return false;
                }
                return true;
            }
        }


        override public void WriteResource(Parser.Resource[] res, int page)
        {
            SqlCommand resourceWrite = sqlsWConn.CreateCommand();
            resourceWrite.Transaction = sqlsTrans;
            resourceWrite.CommandText = "INSERT INTO Resources (page_id,data,original_filename) VALUES (@page_id,@data,@originalFN)";
            for (int i = 0; i < res.Count(); ++i)
            {
                resourceWrite.Parameters.Clear();
                resourceWrite.Parameters.AddWithValue("@page_id", page);
                resourceWrite.Parameters.AddWithValue("@data", res[i].data);
                resourceWrite.Parameters.AddWithValue("@originalFN", res[i].originalFileName);
                resourceWrite.ExecuteNonQuery();
            }
        }
        public override void ArchivePageNumber(int page)
        {
            SqlCommand pageWrite = sqlsWConn.CreateCommand();
            pageWrite.Transaction = sqlsTrans;
            pageWrite.CommandText = "INSERT INTO PagesArchived VALUES (" + page + ")";
            pageWrite.ExecuteNonQuery();
            archivedPages.Add(page);
        }
        override public void Rollback()
        {
            if (sqlsTrans == null)
                throw new Exception();
            sqlsTrans.Rollback();
            sqlsTrans = null;
        }
        override public void Transact()
        {
            if (sqlsTrans != null)
                throw new Exception();
            sqlsTrans = sqlsWConn.BeginTransaction();
        }
        override public void Commit()
        {
            if (sqlsTrans == null)
                throw new Exception();
            sqlsTrans.Commit();
            sqlsTrans = null;
        }
        override public void Close()
        {
            if (sqlsRConn != null)
            {
                sqlsRConn.Close();
                sqlsRConn = null;
            }
            if (sqlsWConn != null)
            {
                sqlsTrans = null;
                sqlsWConn.Close();
                sqlsRConn = null;
            }
        }
    }
}
