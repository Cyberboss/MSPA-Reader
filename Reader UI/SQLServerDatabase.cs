using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace Reader_UI
{
    class SQLServerDatabase : Database
    {

        DbConnection sqlsRConn = null, sqlsWConn = null;
        DbTransaction sqlsTrans = null;
        string connectionString = null;

        readonly bool compact;
        public SQLServerDatabase(bool com)
        {
            compact = com;
        }

        //local db handlers
        //we thank you base-ed god for this code
        //https://social.msdn.microsoft.com/Forums/sqlserver/en-US/268c3411-102a-4272-b305-b14e29604313/localdb-create-connect-to-database-programmatically-?forum=sqlsetupandupgrade
        public static string GetLocalDB(string dbName, bool deleteIfExists = false)
        {
            try
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Application.StartupPath + @"\..\Database");
                string mdfFilename = dbName + ".mdf";
                string dbFileName = Path.Combine(outputFolder, mdfFilename);
                string logFileName = Path.Combine(outputFolder, String.Format("{0}_log.ldf", dbName));
                // Create Data Directory If It Doesn't Already Exist.
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                // If the file exists, and we want to delete old data, remove it here and create a new database.
                if (File.Exists(dbFileName) && deleteIfExists)
                {
                    if (File.Exists(logFileName)) File.Delete(logFileName);
                    File.Delete(dbFileName);
                    CreateDatabase(dbName, dbFileName);
                }
                // If the database does not already exist, create it.
                else if (!File.Exists(dbFileName))
                {
                    CreateDatabase(dbName, dbFileName);
                }

                // Open newly created, or old database.
                return String.Format(@"Data Source=(LocalDB)\v11.0;AttachDBFileName={1};Initial Catalog={0};Integrated Security=True;", dbName, dbFileName);
                
            }
            catch
            {
                throw;
            }
        }
        public static bool CreateDatabase(string dbName, string dbFileName)
        {
            try
            {
                string connectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();


                    DetachDatabase(dbName);

                    cmd.CommandText = String.Format("CREATE DATABASE {0} ON (NAME = N'{0}', FILENAME = '{1}')", dbName, dbFileName);
                    cmd.ExecuteNonQuery();
                }

                if (File.Exists(dbFileName)) return true;
                else return false;
            }
            catch
            {
                throw;
            }
        }
        public static bool DetachDatabase(string dbName)
        {
            try
            {
                string connectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = String.Format("exec sp_detach_db '{0}'", dbName);
                    cmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        override public void Connect(string serverName, string username, string password)
        {
            if (!compact)
            {
                connectionString = "Data Source=" + serverName + ";Initial Catalog=MSPAArchive;User ID=" + username + ";Password=" + password;
            }
            else
            {
                connectionString = GetLocalDB("MSPAArchive");
            }

            sqlsRConn = new SqlConnection(connectionString);
            sqlsWConn = new SqlConnection(connectionString);
            sqlsRConn.Open();
            sqlsWConn.Open();
        }
        override public bool ReadLastIndexedOrCreateDatabase()
        {
            DbDataReader myReader = null;
            try
            {
                DbCommand myCommand = sqlsWConn.CreateCommand();
                myCommand.CommandText = "SELECT * FROM PagesArchived";
                myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    archivedPages.Add(myReader.GetInt32(0));
                }
                myReader.Close();
                return true;
            }
            catch
            {
                try { myReader.Close(); }
                catch { }
                //Assume databse either
                //a) hasn't been created
                //b) is corrupt
                //c) hasn't parsed page 1

                //drop any tables that may exist

                DbCommand dropCommands = sqlsWConn.CreateCommand();
                dropCommands.CommandText = "DROP TABLE Conversations";
                try {
                    dropCommands.ExecuteNonQuery();
                }
                catch{ }
                dropCommands.CommandText = "DROP TABLE Links";
                try
                {
                    dropCommands.ExecuteNonQuery();
                }
                catch { }
                dropCommands.CommandText = "DROP TABLE PagesArchived";
                try
                {
                    dropCommands.ExecuteNonQuery();
                }
                catch  { }
                dropCommands.CommandText = "DROP TABLE Resources";
                try
                {
                    dropCommands.ExecuteNonQuery();
                }
                catch { }
                try
                {
                    DbCommand creationCommands = sqlsWConn.CreateCommand();

                    creationCommands.CommandText = File.ReadAllText(Application.StartupPath + @"\..\DBCreation.sql");
                    creationCommands.ExecuteNonQuery();
                }
                catch
                {
                    if(compact)
                        MessageBox.Show("Error creating database, make sure the application has read/write permissions in the working directory.");
                    else
                        MessageBox.Show("Error creating database, make sure the specified account has read/write permissions.");
                    return false;
                }
                return true;
            }
        }

        static void AddParameterWithValue(DbCommand command, string parameterName, object parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            command.Parameters.Add(parameter);
        }
        override public void WriteResource(Parser.Resource[] res, int page)
        {
            DbCommand resourceWrite = sqlsWConn.CreateCommand();
            resourceWrite.Transaction = sqlsTrans;
            resourceWrite.CommandText = "INSERT INTO Resources (page_id,data,original_filename,title_text) VALUES (@page_id,@data,@originalFN,@title)";
            for (int i = 0; i < res.Count(); ++i)
            {
                resourceWrite.Parameters.Clear();
                AddParameterWithValue(resourceWrite, "@page_id", page);
                AddParameterWithValue(resourceWrite, "@data", res[i].data);
                AddParameterWithValue(resourceWrite, "@originalFN", res[i].originalFileName);
                AddParameterWithValue(resourceWrite, "@title", res[i].titleText != null ? (object)res[i].titleText : (object)DBNull.Value);
                resourceWrite.ExecuteNonQuery();
            }
        }
        override public void WriteLinks(Parser.Link[] res, int page)
        {
            DbCommand resourceWrite = sqlsWConn.CreateCommand();
            resourceWrite.Transaction = sqlsTrans;
            resourceWrite.CommandText = "INSERT INTO Links (page_id,linked_page_id,link_text) VALUES (@page_id,@data,@originalFN)";
            for (int i = 0; i < res.Count(); ++i)
            {
                resourceWrite.Parameters.Clear();
                AddParameterWithValue(resourceWrite, "@page_id", page);
                AddParameterWithValue(resourceWrite, "@data", res[i].pageNumber);
                AddParameterWithValue(resourceWrite, "@originalFN", res[i].originalText);
                resourceWrite.ExecuteNonQuery();
            }
        }
        override public void WriteText(Parser.Text tex, int page)
        {
            DbCommand textWrite = sqlsWConn.CreateCommand();
            textWrite.CommandText = "INSERT INTO PageMeta VALUES ("+page+",@tit,@pttt,@lp)";
            AddParameterWithValue(textWrite, "@tit", tex.title != null ? (object)tex.title : (object)DBNull.Value);
            AddParameterWithValue(textWrite, "@pttt", tex.promptType != null ? (object)tex.promptType : (object)DBNull.Value);
            AddParameterWithValue(textWrite, "@lp", tex.linkPrefix != null ? (object)tex.linkPrefix : (object)DBNull.Value);
            textWrite.Transaction = sqlsTrans;
            textWrite.ExecuteNonQuery();

            textWrite.CommandText = "INSERT INTO Dialog (page_id,isNarrative,isImg,text,colour) VALUES (" + page + ", @narr,@isIm, @tex,@colour) SELECT SCOPE_IDENTITY()";

            if (tex.narr != null)
            {

                AddParameterWithValue(textWrite, "@narr", true);
                AddParameterWithValue(textWrite, "@isIm", tex.narr.isImg);
                AddParameterWithValue(textWrite, "@tex", tex.narr.text);
                AddParameterWithValue(textWrite, "@colour", tex.narr.hexColour);
                textWrite.ExecuteNonQuery();
            }
            else
            {
                DbCommand specWrite = sqlsWConn.CreateCommand();
                specWrite.Transaction = sqlsTrans;
                specWrite.CommandText = "INSERT INTO SpecialText (dialog_id,underline,colour,sbegin,length) VALUES (@did,@ul,@col,@sbeg,@len)";
                for (int i = 0; i < tex.lines.Count(); ++i)
                {
                    textWrite.Parameters.Clear();
                    AddParameterWithValue(textWrite, "@narr", false);
                    AddParameterWithValue(textWrite, "@isIm", tex.lines[i].isImg);
                    AddParameterWithValue(textWrite, "@tex", tex.lines[i].text);
                    AddParameterWithValue(textWrite, "@colour", tex.lines[i].hexColour != null ? (object)tex.lines[i].hexColour : (object)DBNull.Value);
                    var diaId = (int)(decimal)textWrite.ExecuteScalar();
                    if(tex.lines[i].subTexts != null)
                        for (int j = 0; j < tex.lines[i].subTexts.Count(); ++j)
                        {
                            specWrite.Parameters.Clear();
                            AddParameterWithValue(specWrite, "@did", diaId);
                            AddParameterWithValue(specWrite, "@ul", tex.lines[i].subTexts[j].underlined);
                            AddParameterWithValue(specWrite, "@col", tex.lines[i].subTexts[j].colour);
                            AddParameterWithValue(specWrite, "@sbeg", tex.lines[i].subTexts[j].begin);
                            AddParameterWithValue(specWrite, "@len", tex.lines[i].subTexts[j].length);
                            specWrite.ExecuteNonQuery();
                        }
                }
            }
        }
        public override void ArchivePageNumber(int page)
        {
            DbCommand pageWrite = sqlsWConn.CreateCommand();
            pageWrite.Transaction = sqlsTrans;
            pageWrite.CommandText = "INSERT INTO PagesArchived VALUES (" + page + ")";
            pageWrite.ExecuteNonQuery();
            archivedPages.Add(page);
        }
        override public void Rollback()
        {
            if (sqlsTrans == null)
                return;
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
