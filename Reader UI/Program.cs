using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;

namespace Reader_UI
{
    static class Program
    {
        public class NativeMethods
        {
            //MSDN said to use intptr and the anaylzer is still complaining :\
            public const int HWND_BROADCAST = 0xffff;
            public static readonly int WM_SHOWME = (int)RegisterWindowMessage("WM_SHOWME");
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "1"), DllImport("user32", CharSet = CharSet.Unicode)]
            public static extern bool PostMessage(IntPtr hwnd, IntPtr msg, IntPtr wparam, IntPtr lparam);
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return"), DllImport("user32", CharSet = CharSet.Unicode)]
            public static extern IntPtr RegisterWindowMessage(string message);
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "1"), DllImport("user32", CharSet = CharSet.Unicode)]
            public static extern IntPtr SendMessage(IntPtr hwnd, IntPtr wMsg, IntPtr wParam, IntPtr lParam);
        }
        public static DatabaseWriter dbw = null;
        public static Reader dbr = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static System.Threading.Mutex mutex = new System.Threading.Mutex(true, "{e416462f-1fe3-4bbf-a7ae-31a257441a37}");
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var lastPage = Properties.Settings.Default.lastPage;
                new DatabaseLogin().Show();
                Application.Run();       
                mutex.ReleaseMutex();
            }
            else
            {
                // send our Win32 message to make the currently running instance
                // jump on top of all the other windows
                NativeMethods.PostMessage(
                    (IntPtr)NativeMethods.HWND_BROADCAST,
                    (IntPtr)NativeMethods.WM_SHOWME,
                    IntPtr.Zero,
                    IntPtr.Zero);
            }     
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
