using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;

namespace Reader_UI
{
    public class ForegroundWorker : Control
    {
        bool _cancellationPending = false, _running = false;
        object _cPlock = new object(), _rlock = new object();
        Thread _workThread;
        public bool CancellationPending
        {
            get
            {
                lock (_cPlock)
                {
                    return _cancellationPending;
                }
            }
            private set
            {
                lock (_cPlock)
                {
                    _cancellationPending = value;
                }
            }
        }
        public bool IsBusy
        {
            get
            {
                lock (_rlock)
                {
                    return _running;
                }
            }
            private set
            {
                lock (_rlock)
                {
                    _running = value;
                }
            }
        }
        public void CancelAsync()
        {
            if (IsBusy)
                CancellationPending = true;
        }
        public void RunWorkerAsync()
        {
            IsBusy = true;
            _workThread = new Thread(_DoWork);
            _workThread.Start();
        }
        public void ReportProgress(int prog, string message)
        {
            Invoke(ProgressChanged, new object[] { this, new ProgressChangedEventArgs(prog, message) });
        }
        void _DoWork()
        {
            DoWork(this, new EventArgs());
            CancellationPending = false;
            IsBusy = false;
            try
            {
                Invoke(RunWorkerCompleted, new object[] { this, new EventArgs() });
            }
            catch { }
        }
        public event EventHandler DoWork = new EventHandler((o, e) => { });
        public event ProgressChangedEventHandler ProgressChanged = new ProgressChangedEventHandler((o, e) => { });
        public event EventHandler RunWorkerCompleted = new EventHandler((o, e) => { });
    }
}
