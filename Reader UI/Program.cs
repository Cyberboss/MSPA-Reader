﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Reader_UI
{
    static class Program
    {
        internal class NativeMethods
        {
            public const int HWND_BROADCAST = 0xffff;
            public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
            [DllImport("user32")]
            public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
            [DllImport("user32")]
            public static extern int RegisterWindowMessage(string message);
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
                    NativeMethods.WM_SHOWME,
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
