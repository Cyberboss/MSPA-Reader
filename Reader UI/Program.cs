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

        public static DatabaseWriter dbw = null;
        public static Reader dbr = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static System.Threading.Mutex mutex = new System.Threading.Mutex(true, System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString());//unique per build
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
                try
                {
                    DecryptSavedPassword();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    var lastPage = Properties.Settings.Default.lastReadPage;
                    new DatabaseLogin().Show();
                    Application.Run();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    mutex.ReleaseMutex();
                    EncryptSavedPassword();
                    Properties.Settings.Default.Save();
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
        private static void DecryptSavedPassword()
        {

            if (!Properties.Settings.Default.savePassword)
                return;
            try
            {
                Properties.Settings.Default.password = ToInsecureString(DecryptString(Properties.Settings.Default.password));
            }
            catch
            {
                Properties.Settings.Default.savePassword = false;
                Properties.Settings.Default.password = "";
            }

        }
        private static void EncryptSavedPassword()
        {

            if (!Properties.Settings.Default.savePassword)
                return;
            try
            {
                Properties.Settings.Default.password = EncryptString(ToSecureString(Properties.Settings.Default.password));
            }
            catch
            {
                Properties.Settings.Default.savePassword = false;
                Properties.Settings.Default.password = "";
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
