using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
namespace Reader_UI
{
    abstract class Database
    {
        Parser parser;

        enum PagesOfImportance
        {
            HOMESTUCK_PAGE_ONE = 001901,
            LAST_PAGE = 009594  //TODO: Add some dynamic page calculator
        }


        public abstract void Connect(string serverName, string username, string password, bool read = true);
        public abstract int ReadLastIndexedOrCreateDatabase();
        public abstract void WriteResource(Parser.Resource[] res, int page);
        public abstract void Transact();
        public abstract void Rollback();
        public abstract void Commit();
        public abstract void Close();

        public void ResumeWork()
        {
            parser = new Parser();
            Connect(null, null, null, false);
            int currentPage = ReadLastIndexedOrCreateDatabase() + 1;
            if (currentPage == 1)
                currentPage = (int)PagesOfImportance.HOMESTUCK_PAGE_ONE;

            while (currentPage <= 2146)
            {
                if (parser.LoadPage(currentPage))
                {
                    try
                    {
                        Transact();
                        WriteResource(parser.GetResources(),currentPage);
                        Commit();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Save failure on page " + currentPage);
                        Rollback();
                    }
                }
                currentPage++;
            }
            MessageBox.Show("done");
            Application.Exit();
        }
    }
}
