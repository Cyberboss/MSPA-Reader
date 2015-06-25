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
        bool resetFlag = false;

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
                string outputFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Application.StartupPath + @"\Database");
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

        override public void Connect(string serverName, string username, string password,bool reset)
        {
            resetFlag = reset;
            if (!compact)
            {
                connectionString = "Server=" + serverName + ";Initial Catalog=MSPAArchive;";
                if (username != "")
                    connectionString += "User ID=" + username + ";Password=" + password;
                else
                    connectionString += "Integrated Security=True;";
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
        Parser.Text.ScriptLine.SpecialSubText[] GetSpecialText(DbDataReader reader)
        {
            List<Parser.Text.ScriptLine.SpecialSubText> list = new List<Parser.Text.ScriptLine.SpecialSubText>();
            while (reader.Read())
            {
                // underline,colour,sbegin,length 
                list.Add(new Parser.Text.ScriptLine.SpecialSubText(reader.GetInt32(2),reader.GetInt32(3),reader.GetBoolean(0),reader.GetString(2)));
            }
            return list.ToArray();
        }
        Parser.Text GetMeta(int pageno, bool x2)
        {
            DbDataReader reader = null;
            try
            {
                DbCommand selector = sqlsRConn.CreateCommand();
                selector.CommandText = "SELECT title,promptType FROM PageMeta WHERE page_id = " + pageno + " AND x2 = " + (x2 ? 1 : 0);
            
                reader = selector.ExecuteReader();

                var meta = new Parser.Text();
                if (!reader.HasRows || !reader.Read())
                    throw new Exception();  //should never happen if called using waitpage

                if (!reader.IsDBNull(0))
                    meta.title = reader.GetString(0);
                if (!reader.IsDBNull(1))
                    meta.promptType = reader.GetString(1);

                reader.Close();

                selector.CommandText = "SELECT id,isNarrative,isImg,text,colour FROM Dialog WHERE page_id = " + pageno + " AND x2 = " + (x2 ? 1 : 0);

                reader = selector.ExecuteReader();
                if (!reader.HasRows || !reader.Read())
                    throw new Exception();  //should never happen if called using waitpage


                if (reader.GetBoolean(1))
                {//isNarrative
                    if (reader.GetBoolean(2))//isImg
                        meta.narr = new Parser.Text.ScriptLine(reader.GetString(3));
                    else
                        meta.narr = new Parser.Text.ScriptLine(reader.GetString(4), reader.GetString(3));
                    selector.CommandText = "SELECT underline,colour,sbegin,length FROM SpecialText WHERE dialog_id = " + reader.GetInt32(0);
                    reader.Close();
                    reader = selector.ExecuteReader();
                    meta.narr.subTexts = GetSpecialText(reader);
                    reader.Close();
                }
                else
                {
                    List<Parser.Text.ScriptLine> lines = new List<Parser.Text.ScriptLine>();
                    do
                    {
                        DbDataReader specReader = null;
                        try
                        {
                            Parser.Text.ScriptLine currentLine;
                            if (reader.GetBoolean(2))//isImg
                                currentLine = new Parser.Text.ScriptLine(reader.GetString(3));
                            else
                                currentLine = new Parser.Text.ScriptLine(reader.GetString(4), reader.GetString(3));

                            selector.CommandText = "SELECT underline,colour,sbegin,length FROM SpecialText WHERE dialog_id = " + reader.GetInt32(0);
                            specReader = selector.ExecuteReader();

                            currentLine.subTexts = GetSpecialText(reader);
                            specReader.Close();
                            lines.Add(currentLine);
                        }catch{
                            specReader.Close();
                            throw;
                        }

                    } while (reader.Read());
                    reader.Close();
                    meta.lines = lines.ToArray();
                }
                return meta;
            }catch{
                reader.Close();
                throw;
            }
        }
        Parser.Resource[] GetResources(int pageno, bool x2)
        {
            DbDataReader reader = null;
            try
            {
                DbCommand selector = sqlsRConn.CreateCommand();
                selector.CommandText = "SELECT data,original_filename,title_text FROM Resources WHERE page_id = " + pageno + " AND x2 = " + (x2 ? 1 : 0);

                reader = selector.ExecuteReader();

                List<Parser.Resource> res = new List<Parser.Resource>();

                while (reader.Read())
                {

                    if (!reader.IsDBNull(2))
                        res.Add(new Parser.Resource((byte[])reader.GetValue(0), reader.GetString(1), reader.GetString(2)));
                    else
                        res.Add(new Parser.Resource((byte[])reader.GetValue(0), reader.GetString(1)));

                }
                reader.Close();

                return res.ToArray();

            }
            catch
            {
                reader.Close();
                throw;
            }
        }
        Parser.Link[] GetLinks(int pageno, bool x2)
        {
            DbDataReader reader = null;
            try
            {
                DbCommand selector = sqlsRConn.CreateCommand();
                selector.CommandText = "SELECT linked_page_id, link_text FROM Links WHERE page_id = " + pageno + " AND x2 = " + (x2 ? 1 : 0);

                reader = selector.ExecuteReader();

                List<Parser.Link> res = new List<Parser.Link>();

                while (reader.Read())
                {
                    res.Add(new Parser.Link(reader.GetString(1),reader.GetInt32(0)));
                }
                reader.Close();
                return res.ToArray();

            }
            catch
            {
                reader.Close();
                throw;
            }
        }
        public override Page GetPage(int pageno, bool x2)
        {

            Page page = new Page();
                
            page.meta = GetMeta(pageno,false);
            page.resources = GetResources(pageno,false);
            page.links = GetLinks(pageno, false);

            if (x2)
            {
                page.x2 = true;
                page.meta2 = GetMeta(pageno, true);
                page.resources2 = GetResources(pageno, true);
                page.links = GetLinks(pageno, true);
            }
           

            return page;
        }
        override public bool ReadLastIndexedOrCreateDatabase()
        {
            DbDataReader myReader = null;
            try
            {
                if (resetFlag)
                    throw new Exception();
                DbCommand myCommand = sqlsWConn.CreateCommand();
                myCommand.CommandText = "SELECT DISTINCT page_id FROM PagesArchived";
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
                dropCommands.CommandText = "DROP TABLE SpecialText";
                try
                {
                    dropCommands.ExecuteNonQuery();
                }
                catch { }
                dropCommands.CommandText = "DROP TABLE Dialog";
                try
                {
                    dropCommands.ExecuteNonQuery();
                }
                catch { }
                dropCommands.CommandText = "DROP TABLE PageMeta";
                try
                {
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
                    Transact();
                    DbCommand creationCommands = sqlsWConn.CreateCommand();
                    creationCommands.CommandText = File.ReadAllText(Application.StartupPath + @"\DBCreation.sql");
                    creationCommands.Transaction = sqlsTrans;
                    creationCommands.ExecuteNonQuery();
                    Commit();
                }
                catch
                {
                    Rollback();
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
        override public void WriteResource(Parser.Resource[] res, int page,bool x2)
        {
            DbCommand resourceWrite = sqlsWConn.CreateCommand();
            resourceWrite.Transaction = sqlsTrans;
            resourceWrite.CommandText = "INSERT INTO Resources (page_id,x2,data,original_filename,title_text) VALUES (@page_id,@xt,@data,@originalFN,@title)";
            for (int i = 0; i < res.Count(); ++i)
            {
                resourceWrite.Parameters.Clear();
                AddParameterWithValue(resourceWrite, "@xt", x2);
                AddParameterWithValue(resourceWrite, "@page_id", page);
                AddParameterWithValue(resourceWrite, "@data", res[i].data);
                AddParameterWithValue(resourceWrite, "@originalFN", res[i].originalFileName);
                AddParameterWithValue(resourceWrite, "@title", res[i].titleText != null ? (object)res[i].titleText : (object)DBNull.Value);
                resourceWrite.ExecuteNonQuery();
            }
        }
        override public void WriteLinks(Parser.Link[] res, int page, bool x2)
        {
            DbCommand resourceWrite = sqlsWConn.CreateCommand();
            resourceWrite.Transaction = sqlsTrans;
            resourceWrite.CommandText = "INSERT INTO Links (page_id,x2,linked_page_id,link_text) VALUES (@page_id,@xt,@data,@originalFN)";
            for (int i = 0; i < res.Count(); ++i)
            {
                resourceWrite.Parameters.Clear();
                AddParameterWithValue(resourceWrite, "@xt", x2);
                AddParameterWithValue(resourceWrite, "@page_id", page);
                AddParameterWithValue(resourceWrite, "@data", res[i].pageNumber);
                AddParameterWithValue(resourceWrite, "@originalFN", res[i].originalText);
                resourceWrite.ExecuteNonQuery();
            }
        }
        override public void WriteText(Parser.Text tex, int page, bool x2)
        {
            DbCommand textWrite = sqlsWConn.CreateCommand();
            textWrite.CommandText = "INSERT INTO PageMeta VALUES (" + page + ",@xt,@tit,@pttt)";
            AddParameterWithValue(textWrite, "@xt", x2);
            AddParameterWithValue(textWrite, "@tit", tex.title != null ? (object)tex.title : (object)DBNull.Value);
            AddParameterWithValue(textWrite, "@pttt", tex.promptType != null ? (object)tex.promptType : (object)DBNull.Value);
            textWrite.Transaction = sqlsTrans;
            textWrite.ExecuteNonQuery();

            textWrite.CommandText = "INSERT INTO Dialog (page_id,x2,isNarrative,isImg,text,colour) VALUES (" + page + ",@xt, @narr,@isIm, @tex,@colour) SELECT SCOPE_IDENTITY()";

            if (tex.narr != null)
            {
                textWrite.Parameters.Clear();
                AddParameterWithValue(textWrite, "@xt", x2);
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
                    AddParameterWithValue(textWrite, "@xt", x2);
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
        public override void ArchivePageNumber(int page, bool x2)
        {
            DbCommand pageWrite = sqlsWConn.CreateCommand();
            pageWrite.Transaction = sqlsTrans;
            pageWrite.CommandText = "INSERT INTO PagesArchived VALUES (" + page + ", @xt)";
            AddParameterWithValue(pageWrite, "@xt", x2);
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
