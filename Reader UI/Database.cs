using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
namespace Reader_UI
{
    public abstract class Database
    {
        Parser parser;

        enum PagesOfImportance
        {
            HOMESTUCK_PAGE_ONE = 001901,
            LAST_PAGE = 009594  //TODO: Add some dynamic page calculator
        }


        public abstract void Connect(string serverName, string username, string password);
        public abstract int ReadLastIndexedOrCreateDatabase();
        public abstract void WriteResource(Parser.Resource[] res, int page);
        public abstract void ArchivePageNumber(int page);
        public abstract void Transact();
        public abstract void Rollback();
        public abstract void Commit();
        public abstract void Close();

        public void ResumeWork(System.ComponentModel.BackgroundWorker bgw)
        {
            parser = new Parser();
            int currentPage = ReadLastIndexedOrCreateDatabase() + 1;
            if (currentPage == 1)
                currentPage = (int)PagesOfImportance.HOMESTUCK_PAGE_ONE;

            int pagesToParse = 245;
            int currentProgress = (int)(((float)(currentPage - 1 - (int)PagesOfImportance.HOMESTUCK_PAGE_ONE) / (float)(pagesToParse)) * 100.0f);
            if (!bgw.CancellationPending)
                bgw.ReportProgress(currentProgress, "Starting at page " + currentPage);
            while (currentPage - (int)PagesOfImportance.HOMESTUCK_PAGE_ONE <= pagesToParse && !bgw.CancellationPending)
            {
                currentProgress = (int)(((float)(currentPage - 1 - (int)PagesOfImportance.HOMESTUCK_PAGE_ONE) / (float)(pagesToParse)) * 100.0f);
                if (parser.LoadPage(currentPage) && !bgw.CancellationPending)
                {
                    try
                    {
                        Transact();
                        if (bgw.CancellationPending)
                        {
                            Rollback();
                            break;
                        }
                        var res = parser.GetResources();
                        WriteResource(res, currentPage);
                        if (bgw.CancellationPending)
                        {
                            Rollback();
                            break;
                        }
                        ArchivePageNumber(currentPage);
                        if (bgw.CancellationPending)
                        {
                            Rollback();
                            break;
                        }
                        Commit();
                        if (!bgw.CancellationPending)
                            bgw.ReportProgress(currentProgress,"Page " + currentPage + " archived. " + res.Count() + " resources.");
                    }
                    catch (Exception)
                    {
                        if (!bgw.CancellationPending)
                            bgw.ReportProgress(currentProgress, "Error in archivng page: " + currentPage);
                        pagesToParse++;
                        Rollback();
                    }
                }
                currentPage++;
            }
            if (!bgw.CancellationPending)
                bgw.ReportProgress(currentProgress, "Operation completed. " + (pagesToParse - (currentPage - 1 - (int)PagesOfImportance.HOMESTUCK_PAGE_ONE)) + " pages remaining.");
        }
    }
}
