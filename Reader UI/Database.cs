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
        Parser parser = null;

        protected List<int> archivedPages = new List<int>();
        enum PagesOfImportance
        {
            HOMESTUCK_PAGE_ONE = 001901,
        }
        float totalMegabytesDownloaded = 0;


        public abstract void Connect(string serverName, string username, string password);
        public abstract bool ReadLastIndexedOrCreateDatabase();
        public abstract void WriteResource(Parser.Resource[] res, int page);
        public abstract void WriteLinks(Parser.Link[] res, int page);
        public abstract void ArchivePageNumber(int page);
        public abstract void Transact();
        public abstract void Rollback();
        public abstract void Commit();
        public abstract void Close();

        bool IsPageArchived(int page)
        {
            return archivedPages.IndexOf(page) >= 0;
        }
        int FindLowestPage(int start, int end)
        {
            for (int i = start; i <= end; ++i)
                if (archivedPages.IndexOf(i) < 0)
                    return i;
            return end + 1;
        }
        public void ResumeWork(System.ComponentModel.BackgroundWorker bgw)
        {

            if (!ReadLastIndexedOrCreateDatabase())
            {
                if (!bgw.CancellationPending)
                    bgw.ReportProgress(0, "Error creating database.");
                return;
            }

            if (parser == null)
                parser = new Parser();

            int lastPage = parser.GetLatestPage();
            int startPage = FindLowestPage((int)PagesOfImportance.HOMESTUCK_PAGE_ONE, lastPage);
            int currentPage = startPage;
            int pagesToParse = lastPage - startPage;
            int currentProgress = (int)(((float)(currentPage - 1 - startPage) / (float)(pagesToParse)) * 100.0f);


            if (!bgw.CancellationPending)
                bgw.ReportProgress(currentProgress, "MSPA is up to page " + startPage);
            else
                return;
            if (!bgw.CancellationPending)
                bgw.ReportProgress(currentProgress, "Starting archive operation at page " + currentPage);
            else
                return;

            while (currentPage != lastPage + 1 && !bgw.CancellationPending)
            {
                currentProgress = (int)(((float)(currentPage - 1 - startPage) / (float)(pagesToParse)) * 100.0f);
                if (!IsPageArchived(currentPage) && parser.LoadPage(currentPage) && !bgw.CancellationPending)
                {
                    try
                    {
                        Transact();
                        if (bgw.CancellationPending)
                        {
                            Rollback();
                            return;
                        }
                        var res = parser.GetResources();
                        WriteResource(res, currentPage);
                        if (bgw.CancellationPending)
                        {
                            Rollback();
                            return;
                        }
                        var links = parser.GetLinks();
                        WriteLinks(links, currentPage);
                        if (bgw.CancellationPending)
                        {
                            Rollback();
                            return;
                        }
                        ArchivePageNumber(currentPage);
                        if (bgw.CancellationPending)
                        {
                            Rollback();
                            return;
                        }
                        Commit();
                        if (!bgw.CancellationPending)
                            bgw.ReportProgress(currentProgress, "Page " + currentPage + " archived. " + res.Count() + " resources.");
                        else
                            return;
                        for (int i = 0; i < links.Count(); ++i)
                        {
                            if (!bgw.CancellationPending)
                                bgw.ReportProgress(currentProgress, "\"" + links[i].originalText + "\" links to " + links[i].pageNumber);
                            else
                                return;
                        }
                        for (int i = 0; i < res.Count(); ++i)
                        {
                            var fileSize = res[i].data.Count();
                            totalMegabytesDownloaded += (float)fileSize / (1024.0f * 1024.0f);
                            if (!bgw.CancellationPending)
                                bgw.ReportProgress(currentProgress, res[i].originalFileName + ": " + fileSize / 1024 + "KB");
                            else
                                return;
                        }
                        if (!bgw.CancellationPending)
                            bgw.ReportProgress(currentProgress, "Total Data Downloaded: " + (int)totalMegabytesDownloaded + "MB");
                        else
                            return;
                    }
                    catch (Exception)
                    {
                        pagesToParse++;
                        Rollback();
                        if (!bgw.CancellationPending)
                            bgw.ReportProgress(currentProgress, "Error in archivng page: " + currentPage);
                        else
                            return;
                    }
                }
                currentPage = FindLowestPage(currentPage + 1,lastPage);
            }
            if (!bgw.CancellationPending)
                bgw.ReportProgress(currentProgress, "Operation completed. " + (pagesToParse - (currentPage - 1 - startPage)) + " pages remaining.");
        }
    }
}
