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
using MySql.Data.MySqlClient;
#if linux
using Mono.Data.Sqlite;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
#else
using System.Data.SQLite;
#endif
namespace Reader_UI
{
    class DatabaseManager : Writer
    {



        DbConnection sqlsRConn = null, sqlsWConn = null;
        MSPADatabase writer = null;
        string connectionString = null;

        public enum DBType
        {
            SQLSERVER,
            SQLLOCALDB,
            SQLITE,
            MYSQL
        }

        public readonly DBType databaseType;
        bool resetFlag = false;

        public DatabaseManager(DBType com)
        {
            databaseType = com;
        }

        //local db handlers
        //we thank you base-ed god for this code
        //https://social.msdn.microsoft.com/Forums/sqlserver/en-US/268c3411-102a-4272-b305-b14e29604313/localdb-create-connect-to-database-programmatically-?forum=sqlsetupandupgrade
        public static string GetLocalDB(string dbName, bool deleteIfExists, string folder)
        {
            try
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), folder);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
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
                    cmd.CommandText = "CREATE DATABASE " + dbName + " ON (NAME = N'" + dbName + "', FILENAME = '" + dbFileName + "')";
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static bool DetachDatabase(string dbName)
        {
            try
            {
                string connectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand(); 
                    cmd.CommandText = "exec sp_detach_db '"+dbName+"'";
                    cmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        string dbNameForServers;
        override public void Connect(string databaseName, string serverFolderName, string username, string password, int port, bool reset)
        {
            resetFlag = reset;
            dbNameForServers = databaseName;
            switch (databaseType)
            {
                case DBType.SQLSERVER:
                    {
                        var cb = new SqlConnectionStringBuilder();
                        cb.PersistSecurityInfo = true;
                        connectionString = cb.ToString() + ";Server=" + serverFolderName;
                        if (username != "")
                            connectionString +=  "," + port + ";Initial Catalog=" + databaseName + ";" + "User ID=" + username + ";Password=" + password;
                        else
                            connectionString += ";Initial Catalog=" + databaseName + ";Integrated Security=True;";
                        sqlsRConn = new SqlConnection(connectionString);
                        sqlsWConn = new SqlConnection(connectionString.Replace("Initial Catalog=" + databaseName + ";", ""));
                        break;
                    }
                case DBType.SQLLOCALDB:
                    {
                        connectionString = GetLocalDB(databaseName, false, serverFolderName);
                        sqlsRConn = new SqlConnection(connectionString);
                        sqlsWConn = new SqlConnection(connectionString);
                        break;
                    }
                case DBType.SQLITE:
                    connectionString = "data source=" + serverFolderName + System.IO.Path.DirectorySeparatorChar + databaseName + ".sqlite3; Version=3";
                    sqlsRConn = new SQLiteConnection(connectionString);
                    sqlsWConn = new SQLiteConnection(connectionString);
                    break;
                case DBType.MYSQL:

                    MySqlConnectionStringBuilder cnx = new MySqlConnectionStringBuilder();
                    
                    cnx.Database = databaseName;
                    cnx.Password = password;
                    cnx.PersistSecurityInfo = true;
                    cnx.UserID = username;
                    cnx.Server = serverFolderName;
                    cnx.Port = (uint)port;

                    connectionString = cnx.ToString();
                    sqlsRConn = new MySqlConnection(connectionString);
                    sqlsWConn = new MySqlConnection(connectionString.Replace("database=" + databaseName + ";", ""));
                    break;
            }
            sqlsWConn.Open();

        }
        public override byte[] GetIcon(Writer.IconTypes ic)
        {
            var reader = new MSPADatabase(sqlsRConn);
            var icos = from b in writer.Resources
                       where b.pageId == (int)SpecialResources.CANDYCORNS
                       select b;


            byte[] res;
            switch (ic)
            {
                case IconTypes.CANDYCORN:
                    var theone = from b in icos
                                 where b.originalFileName == "candycorn.gif"
                                 select b;

                    res = theone.First().data;
                    break;
                case IconTypes.CUEBALL:
                    var theone2 = from b in icos
                                  where b.originalFileName == "candycorn_scratch.png"
                                  select b;
                    res = theone2.First().data;
                    break;
                case IconTypes.CALIBORNTOOTH:
                    var theone3 = from b in icos
                                  where b.originalFileName == "a6a6_tooth2.gif"
                                  select b;
                    res = theone3.First().data;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    throw new Exception();
            }
            reader.Dispose();
            return res;
        }
        public override byte[] Getx2Header()
        {
            Parsex2Header(true);
            var reader = new MSPADatabase(sqlsRConn);

            var icos = from b in writer.Resources
                       where b.pageId == 100002
                       select b;
            byte[] res = icos.First().data;
            reader.Dispose();
            return res;
        }
        public override bool TricksterParsed()
        {
            var icos = from b in writer.Resources
                       where b.pageId == (int)SpecialResources.TRICKSTER_HEADER
                       select b;
            return icos.Count() != 0;
        }
        public override Parser.Resource[] GetTricksterShit()
        {
            var reader = new MSPADatabase(sqlsRConn);
            var selection = from b in reader.Resources
                            where b.pageId == (int)SpecialResources.TRICKSTER_HEADER
                            select b;
            Parser.Resource[] res = new Parser.Resource[selection.Count()];
            for (int i = 0; i < selection.Count(); ++i){
                var elem = selection.ElementAt(i);
                res[i] = new Parser.Resource(elem.data, elem.originalFileName, elem.titleText);
                res[i].isInPesterLog = elem.isInPesterLog;
            }
            reader.Dispose();
            return res; 
        }
        public override Page GetPage(int pageno, bool x2)
        {
            var reader = new MSPADatabase(sqlsRConn);
            var res = reader.ToWriterObject(pageno, x2);
            reader.Dispose();
            return res;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        override public bool ReadLastIndexedOrCreateDatabase(System.ComponentModel.BackgroundWorker bgw)
        {
            try
            {

                bgw.ReportProgress(0, "Checking database version...");
                DbCommand myCommand = sqlsWConn.CreateCommand();
                bool autoDrop = false;
                //first check that the db version matches ours it's fine since we haven't initialized any EF models yet and we don't want it auto dropping
                if (databaseType == DBType.MYSQL || databaseType == DBType.SQLSERVER)
                    myCommand.CommandText = "USE " + dbNameForServers + ";";
                myCommand.CommandText += "SELECT DatabaseVersion FROM Versions";
                try
                {
                    if (!resetFlag && Convert.ToInt32(myCommand.ExecuteScalar()) != (int)Versions.Database)
                    { //if the table doesn't exist assume corrupt and overwrite
                        autoDrop = (MessageBox.Show("Database version differs from that of the program. Wipe database and create updated version?",
                                     "Version Mismatch",
                                         MessageBoxButtons.YesNo) == DialogResult.Yes);
                        if (!autoDrop)
                            return false;
                    }
                }
                catch
                {
                    autoDrop = true;
                }
                sqlsWConn.Close();  //we have to close the connection or entity framework can't figure out how to unlock the db for dropping
                sqlsWConn.ConnectionString = connectionString;  //reset connection string

                if (resetFlag || autoDrop)
                    bgw.ReportProgress(0, "Creating Entity Framework (This may take some time)...");
                else
                    bgw.ReportProgress(0, "Initializing Entity Framework...");

                writer = MSPADatabase.Initialize(sqlsWConn, resetFlag || autoDrop, databaseType);

                if (writer.Versions.Count() == 0)
                {
                    var tmp = new MSPADatabase.Version();
                    tmp.DatabaseVersion = (int)Versions.Database;
                    writer.Versions.Add(tmp);
                }
                sqlsRConn.Open();

                bgw.ReportProgress(0, "Listing archived pages...");
                foreach (var page in writer.ArchivedPages)
                    archivedPages.Add(page.pageId);

                return true;
            }
            catch
            {
                if (databaseType == DBType.SQLLOCALDB)
                    MessageBox.Show("Error creating database, make sure the application has read/write permissions in the working directory.");
                else
                    MessageBox.Show("Error creating database, make sure the specified account has read/write permissions.");
                return false;
            }
        }
        override public void WriteResource(Parser.Resource[] res, int page, bool x2)
        {
            foreach (var link in res)
            {
                writer.Resources.Add(new MSPADatabase.Resource(link, page, x2));
            }
        }
        public override bool IconsAreParsed()
        {
            var icos = from b in writer.Resources
                        where b.pageId == (int)SpecialResources.CANDYCORNS
                        select b;
            return icos.Count() != 0;
        }

        public override bool x2HeaderParsed()
            {
            var icos = from b in writer.Resources
                       where b.pageId == 100002
                       select b;
            return icos.Count() != 0;
            }
        override public void WriteLinks(Parser.Link[] res, int pageno)
                {
            foreach(var link in res)
                        {
                writer.Links.Add(new MSPADatabase.Link(link,pageno));
                }
            }
        override public void WriteText(Parser.Text tex, int pageno, bool x2)
        {
            writer.PageMeta.Add(new MSPADatabase.Text(tex,pageno,x2));
        }
        public override void ArchivePageNumber(int page, bool x2)
        {
            if (archivedPages.IsPageArchived(page))
            {//should never happen
                System.Diagnostics.Debugger.Break();
                throw new Exception();
            }
            writer.ArchivedPages.Add(new MSPADatabase.Archives(page,x2));
        }

        public override void Prune(int pageno)
        {
            writer.Prune(pageno);
        }
        override public void Rollback()
        {
            writer.Rollback();
        }
        override public void Transact()
        {
            writer.Transact();
        }
        override public void Commit()
        {
            writer.Commit();
        }
        override public void Close()
        {
           
            if (sqlsRConn != null)
            {
                sqlsRConn.Close();
                sqlsRConn = null;
            }
            if (writer != null)
                writer.Dispose();
            else if (sqlsWConn != null)
            {
                sqlsWConn.Close();
            }
            sqlsWConn = null;
        }
    }
}
