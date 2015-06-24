using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reader_UI
{
    static class Program
    {
        public static DatabaseWriter dbw = null;
        public static Reader dbr = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new DatabaseLogin().Show();
            Application.Run();            
        }
        public static void Shutdown(Form window, Database db)
        {
            if (dbr == null && dbw == null)
            {
                Application.Exit();
                return;
            }
            else if (window == null && db == null)
                return;
            if ((window == dbw && dbr == null) || (window == dbr && dbw == null))
            {
                db.Close();
                Application.Exit();
                return;
            }
            if (window == dbr)
                dbr = null;
            else
                dbw = null;
        }
        public static void Open(Database db, bool writer)
        {
            if (writer)
            {
                if (dbw == null)
                    dbw = new DatabaseWriter(db);
                dbw.Show();
                dbw.Focus();
            }
            else
            {
                if (dbr == null)
                    dbr = new Reader(db);
                dbr.Show();
                dbr.Focus();
            } 
        }
    }
}
