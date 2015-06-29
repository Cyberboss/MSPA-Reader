//YOOOOO CHANGE THIS BEFORE RELEASING


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Security;

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
            [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetForegroundWindow();
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
            public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
            public static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
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
                try
                {
                    if (Properties.Settings.Default.savePassword)
                        try
                        {
                            Properties.Settings.Default.password = ToInsecureString(DecryptString(Properties.Settings.Default.password));
                        }
                        catch {
                            Properties.Settings.Default.savePassword = false;
                        }
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    var lastPage = Properties.Settings.Default.lastReadPage;
                    new DatabaseLogin().Show();
                    Application.Run();
                    mutex.ReleaseMutex();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if(Properties.Settings.Default.savePassword)
                        Properties.Settings.Default.password = EncryptString(ToSecureString(Properties.Settings.Default.password));
                    Properties.Settings.Default.Save();
                }
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
        public static void Open(Database db, bool writer, bool immediate = false)
        {
            if (writer)
            {
                if (dbw == null)
                    dbw = new DatabaseWriter(db, immediate);
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

        //saved password encryption
        //http://weblogs.asp.net/jongalloway//encrypting-passwords-in-a-net-app-config-file
        //less so encryption and more obfuscation?
        //all in all, someone needs to review this
        static byte[] entropy = System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray(); //not exactly the most secure string, but it's unique per build

        public static string EncryptString(System.Security.SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        public static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }
}
