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
            CASCADE = 006009,
            CALIBORN_PAGE_SMASH = 007395,
            DOTA = 006715,
        }
        float totalMegabytesDownloaded = 0;


        public abstract void Connect(string serverName, string username, string password);
        public abstract bool ReadLastIndexedOrCreateDatabase();
        public abstract void WriteResource(Parser.Resource[] res, int page);
        public abstract void WriteLinks(Parser.Link[] res, int page);
        public abstract void WriteText(Parser.Text tex, int page);
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
        void HandleCascade(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            //cascade is hosted on newgrounds
            //also its split into a loader and 5 segments
            /*
             * Thank you based /u/Niklink
http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_loaderExt.swf
http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_segment1.swf
http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_segment2.swf
http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_segment3.swf
http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_segment4.swf
http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_segment5.swf
             * 
             * They are actually also availiable on the www and cdn, so we can try those too
             */
            bgw.ReportProgress(progress, "Now parsing Cascade, page 6009");
            Parser.Resource[] cascadeSegments = new Parser.Resource[6];
            Parser.Link[] next = new Parser.Link[1];
            next[0] = new Parser.Link("END OF ACT 5", 6010);
            try
            {
                cascadeSegments[0] = new Parser.Resource(parser.DownloadFile("http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_loaderExt.swf"), "cascade_loaderExt.swf");
            }
            catch
            {
                cascadeSegments[0] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/cascade/cascade_loaderExt.swf"), "cascade_loaderExt.swf");
            }
            var fileSize = cascadeSegments[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize / (1024.0f * 1024.0f);
            bgw.ReportProgress(progress, cascadeSegments[0].originalFileName + ": " + fileSize / 1024 + "KB");
            for(int i = 1; i <= 5; ++i){
            
                
                try
                {
                    cascadeSegments[i] = new Parser.Resource(parser.DownloadFile("http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_segment" + i + ".swf"), "cascade_segment" + i + ".swf");
                }
                catch
                {
                    cascadeSegments[i] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/cascade/cascade_segment" + i + ".swf"), "cascade_segment" + i + ".swf");
                }
                var fileSize2 = cascadeSegments[i].data.Count();
                totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
                bgw.ReportProgress(progress, cascadeSegments[i].originalFileName + ": " + fileSize2 / 1024 + "KB");
            }
            bgw.ReportProgress(progress, "Saving to Database...");
            Transact();
            WriteResource(cascadeSegments, 6009);
            WriteLinks(next, 6009);
            ArchivePageNumber(6009);
            Commit();
            bgw.ReportProgress(progress, "Cascade committed!");
        }
        //eventually we'll have to apply a diff on these to get the flash to do what we want it to do when the link is clicked
        void HandlePageSmash(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            bgw.ReportProgress(progress, "Now parsing Caliborn's hissy fit.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/007395/05492.swf"), "05492.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Transact();
            WriteResource(FUCKYOU, 7395);
            ArchivePageNumber(7395);
            Commit();

        }
        void HandleDota(System.ComponentModel.BackgroundWorker bgw, int progress)
        {

            bgw.ReportProgress(progress, "Now parsing Hussie's rekage.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/DOTA/04812.swf"), "04812.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Transact();
            WriteResource(FUCKYOU, 6715);
            ArchivePageNumber(6715);
            Commit();
        }
        public void ResumeWork(System.ComponentModel.BackgroundWorker bgw)
        {

            if (parser == null)
                parser = new Parser();
            if (!ReadLastIndexedOrCreateDatabase())
            {
                if (!bgw.CancellationPending)
                    bgw.ReportProgress(0, "Error creating database.");
                return;
            }
            int lastPage = parser.GetLatestPage();
            int currentProgress;
            while (true){
                int missedPages = 0;

                int startPage = FindLowestPage((int)PagesOfImportance.HOMESTUCK_PAGE_ONE, lastPage);
                int currentPage = startPage;
                int pagesToParse = lastPage - startPage;
                currentProgress = (int)(((float)(currentPage - 1 - startPage) / (float)(pagesToParse)) * 100.0f);


                //debug set current page here
                //currentPage = 2324;
                //currentPage = 6708;
                //currentPage = 7326;
                //currentPage = 4163;
                currentPage = 1926;
                //
                if (!bgw.CancellationPending)
                    bgw.ReportProgress(currentProgress, "MSPA is up to page " + lastPage);
                else
                    return;
                if (!bgw.CancellationPending)
                    bgw.ReportProgress(currentProgress, "Starting archive operation at page " + currentPage);
                else
                    return;

                while (currentPage != lastPage + 1 && !bgw.CancellationPending)
                {

                    currentProgress = (int)(((float)(currentPage - 1 - startPage) / (float)(pagesToParse)) * 100.0f);
                    bgw.ReportProgress(currentProgress, "Page " + currentPage + ":");
                    if (Enum.IsDefined(typeof(PagesOfImportance), currentPage) && currentPage != (int)PagesOfImportance.HOMESTUCK_PAGE_ONE)
                    {
                        try
                        {
                            switch ((PagesOfImportance)currentPage)
                            {
                                case PagesOfImportance.CASCADE:
                                    HandleCascade(bgw, currentProgress);
                                    break;
                                case PagesOfImportance.CALIBORN_PAGE_SMASH:
                                    HandlePageSmash(bgw, currentProgress);
                                    break;
                                case PagesOfImportance.DOTA:
                                    HandleDota(bgw, currentProgress);
                                    break;
                            }
                        }
                        catch
                        {
                            Rollback();
                            bgw.ReportProgress(currentProgress, "Error parsing special page " + currentPage);
                        }
                        currentPage = FindLowestPage(currentPage + 1, lastPage);
                        continue;
                    }

                    if (!IsPageArchived(currentPage) && parser.LoadPage(currentPage) && !bgw.CancellationPending)
                    {
                        try
                        {
                            var res = parser.GetResources();
                            var links = parser.GetLinks();
                            var text = parser.GetText();

                            bgw.ReportProgress(currentProgress, text.title);
                            if (text.narr == null)
                            {
                                bgw.ReportProgress(currentProgress, text.promptType);
                                for (int i = 0; i < text.lines.Count(); ++i)
                                {
                                    if (!text.lines[i].isImg)
                                        bgw.ReportProgress(currentProgress, text.lines[i].subTexts.Count() + " special subtexts, Colour: " + text.lines[i].hexColour + ": " + text.lines[i].text);
                                    else
                                        bgw.ReportProgress(currentProgress, "Imageline");
                                }
                            }
                            else
                                bgw.ReportProgress(currentProgress, "Narrative: " + text.narr.text);
                            bgw.ReportProgress(currentProgress, text.linkPrefix);
                            for (int i = 0; i < links.Count(); ++i)
                                bgw.ReportProgress(currentProgress, "\"" + links[i].originalText + "\" links to " + links[i].pageNumber);
                            for (int i = 0; i < res.Count(); ++i)
                            {
                                var fileSize = res[i].data.Count();
                                totalMegabytesDownloaded += (float)fileSize / (1024.0f * 1024.0f);
                                bgw.ReportProgress(currentProgress, res[i].originalFileName + ": " + fileSize / 1024 + "KB");
                            }
                            bgw.ReportProgress(currentProgress, "Total Data Downloaded: " + (int)totalMegabytesDownloaded + "MB");

                            Transact();
                            WriteResource(res, currentPage);
                            WriteLinks(links, currentPage);
                            WriteText(text, currentPage);
                            ArchivePageNumber(currentPage);
                            Commit();
                        
                        }
                        catch
                        {
                            Debugger.Break();
                            missedPages++;
                            Rollback();
                            bgw.ReportProgress(currentProgress, "Error in archiving page: " + currentPage);
                        }
                    }
                    currentPage = FindLowestPage(currentPage + 1,lastPage);
                }
                if (!(!bgw.CancellationPending && missedPages != 0))
                    break;
                bgw.ReportProgress(currentProgress,"Missed " + missedPages + " pages. Looping back.");
            }

            bgw.ReportProgress(100, "Operation completed.");
        }
    }
}
