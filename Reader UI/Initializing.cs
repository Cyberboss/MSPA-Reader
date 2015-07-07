using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reader_UI
{
    public partial class Initializing : Form
    {
        public Writer db;
        readonly string dbName, dbFName, username, password;
        readonly bool reset;
        bool working = true, success = true;
        public Initializing(Writer idb,string idbName, string idbFName,string  iusernameInput,string ipasswordInput, bool iresetDatabase, FormClosedEventHandler closeHandler, Form owner)
        {
            Owner = owner;
            dbName = idbName;
            dbFName = idbFName;
            username = iusernameInput;
            password = ipasswordInput;
            reset = iresetDatabase;
            db = idb;
            InitializeComponent();
            FormClosing += Initializing_FormClosing;
            FormClosed += closeHandler;
            initializer.ProgressChanged += initializer_ProgressChanged;
            initializer.RunWorkerCompleted += initializer_RunWorkerCompleted;
            initializer.RunWorkerAsync();
        }

        void Initializing_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = working;
        }
        public bool Good()
        {
            return success;
        }

        void initializer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            working = false;
            Close();
        }

        void initializer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            stateLabel.Text = (string)e.UserState;
        }

        private void initializer_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                db.Connect(dbName, dbFName, username, password, reset);
                success = db.Initialize(initializer);
            }
            catch
            {
                success = false;
            }
        }
    }
}
