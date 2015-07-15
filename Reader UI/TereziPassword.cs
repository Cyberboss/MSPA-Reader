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
    public partial class TereziPassword : Form
    {
        TereziDummy dum = new TereziDummy();
        System.IO.MemoryStream tms;
        bool wrong = false;
        public string GetText()
        {
            return textBox1.Text;
        }
        public void Wrong()
        {
            if(wrong)
                return;
            richTextBox1.Text = "<- WRONG! GO B4CK!!!";
            richTextBox1.Select(3, 6);
            richTextBox1.SelectionFont = new System.Drawing.Font("Verdana", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            richTextBox1.SelectAll();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            wrong = true;
        }
        public TereziPassword(EventHandler eh, byte[] ms)
        {
            tms = new System.IO.MemoryStream(ms);
            InitializeComponent();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            pictureBox1.Image = Image.FromStream(tms);
            submitButton.Click += eh;
            FormClosing += TereziPassword_FormClosing;
        }

        void TereziPassword_FormClosing(object sender, FormClosingEventArgs e)
        {
            dum.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dum.Show();
        }

        private void hiddenCancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
