//YOOOOO CHANGE THIS BEFORE RELEASING


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Security;
using System.Diagnostics;

namespace Reader_UI
{
    static class Program
    {

        public static ArchiverWindow dbw = null;
        public static Reader dbr = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        static string updateURL = "https://github.com/cybnetsurfe3011/MSPA-Reader/releases/download/v1." + (int)Writer.Versions.Program + "/MSPA.Reader.Release." + (int)Writer.Versions.Release + ".Update.exe";

        static System.Threading.Mutex mutex = new System.Threading.Mutex(true, System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString());//unique per build
        [STAThread]
        static void Main()
        {

            if (mutex.WaitOne(TimeSpan.Zero, true)) //make sure we're the only instance
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);   //must be done before showing message boxes
                try
                {
                    string oldUpdatePath = Application.StartupPath + System.IO.Path.DirectorySeparatorChar + System.AppDomain.CurrentDomain.FriendlyName.Replace(" Update.exe", ".exe");

                    if (oldUpdatePath != Application.StartupPath + System.IO.Path.DirectorySeparatorChar + System.AppDomain.CurrentDomain.FriendlyName)
                        try
                        {
                            if (System.IO.File.Exists(oldUpdatePath))
                            {
                                MessageBox.Show("Update successful!");
                                System.IO.File.Delete(oldUpdatePath);   //delete the old version
                            }
                            else
                            {

                                oldUpdatePath = oldUpdatePath.Replace(".exe", "");
                                if (System.IO.File.Exists(oldUpdatePath))
                                {
                                    MessageBox.Show("Update successful!");
                                    System.IO.File.Delete(oldUpdatePath); 
                                }
                            }
                        }
                        catch { }

                    try
                    {
                        var tmpParser = new Parser();
                        var res = tmpParser.CheckIfUpdateIsAvailable();
                        tmpParser.Dispose();
                        if (res != 0)
                        {
                            if (MessageBox.Show("Release version "+res+" available. Download now?",
                                             "Update Available",
                                             MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Parser p = new Parser();
                                try{
                                    string path = (Application.StartupPath + System.IO.Path.DirectorySeparatorChar + System.AppDomain.CurrentDomain.FriendlyName).Replace(".exe", " Update.exe");
                                    if (path == Application.StartupPath + System.IO.Path.DirectorySeparatorChar + System.AppDomain.CurrentDomain.FriendlyName)
                                        path += "Update.exe";
                                    System.IO.File.WriteAllBytes(path, p.DownloadFile(updateURL));
                                    System.Diagnostics.Process.Start(path);
                                    return;
                                }
                                catch {
                                    MessageBox.Show("Error downloading/saving the update! You can get it manually at https://github.com/cybnetsurfe3011/MSPA-Reader/releases.");
                                }
                                finally
                                {
                                    p.Dispose();
                                }
                            }
                        }

                    }
                    catch
                    {
                        MessageBox.Show("Unable to check for updates!");
                    }
                    try
                    {
                        DecryptSavedPassword();
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
                        EncryptSavedPassword();
                        Properties.Settings.Default.Save();
                    }
                }
                catch (Exception e){
                    MessageBox.Show("An unhandled exception occured. Please file a bug report at https://www.github.com/cybnetsurfe3011/MSPA-Reader containing the following: " + Environment.NewLine + Environment.NewLine + e.ToString(), "Critical Error!");
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
        }
        public static void ExecuteElevatedCommand(string Command, bool wait)
        {
            ProcessStartInfo ProcessInfo;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/K " + Command);
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = true;
#if !linux
            ProcessInfo.Verb = "runas";
#endif

            if (wait)
                Process.Start(ProcessInfo).WaitForExit();
            else
                Process.Start(ProcessInfo);
        }
        public static void Shutdown(Form window, Writer db)
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
        public static void Open(Writer db, bool writer, bool immediate = false)
        {
            if (writer)
            {
                if (dbw == null)
                {
                    if (((DatabaseManager)db).databaseType == DatabaseManager.DBType.SQLITE && dbr != null)
                    {
                        if (MessageBox.Show("Simultaneous reading and archiving is unsafe in sqlite mode. Close the reader to open the archiver?",
                                         "Conflict",
                                         MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            dbr.Close();
                            dbw = new ArchiverWindow(db);
                        }
                        else
                            return;
                    }
                    else
                        dbw = new ArchiverWindow(db);
                }
                dbw.Show();
                dbw.Focus();
            }
            else
            {
                if (dbr == null)
                {
                    if (((DatabaseManager)db).databaseType == DatabaseManager.DBType.SQLITE && dbw != null)
                    {
                        if (MessageBox.Show("Simultaneous reading and archiving is unsafe in sqlite mode. Close the archiver to open the reader?",
                                         "Conflict",
                                         MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            dbw.Close();
                            dbr = new Reader(db);
                        }
                        else
                            return;
                    }
                    else
                        dbr = new Reader(db);
                }
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
            byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                Convert.FromBase64String(encryptedData),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
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
