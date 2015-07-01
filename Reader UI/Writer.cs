using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Concurrent;
namespace Reader_UI
{
    public abstract class Writer : IDisposable
    {
        protected enum DB{
            Version = 1 //update with every commit that affects db layout
        }
        void Dispose(bool mgd)
        {
            if(parser != null)
                parser.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~Writer()
        {
            Dispose(false);
        }
        public enum Style
        {
            REGULAR,
            SCRATCH,
            X2,
            SBAHJ,
            HOMOSUCK,
            TRICKSTER,
            CASCADE,
            DOTA,
            SHES8ACK,
            SMASH,
            GAMEOVER,
        }
        public class Page
        {
            public readonly int number;
            public Page(int no)
            {
                number = no;
            }
            public Parser.Text meta,meta2;
            public Parser.Resource[] resources,resources2;
            public Parser.Link[] links,links2;
            public bool x2 = false;
        }

        Parser parser = null;
        public int lastPage;
        class WorkerLock
        {
            bool running = false;
            private object _sync = new object();
            public bool TestAndSet()
            {
                bool isRun;
                lock (_sync)
                {
                    isRun = running;
                    running = true;
                }
                return isRun;
            }
            public void StopRunning()
            {

                lock (_sync)
                {
                    running = false;
                }
            }
        }
        WorkerLock wl = new WorkerLock();
        protected class ArchiveLock
        {
            private List<int> archivedPages = new List<int>();
            private int request = 0;
            private object _sync = new object();

            public bool IsPageArchived(int page)
            {
                bool ret;
                lock (_sync)
                {
                    ret = archivedPages.IndexOf(page) >= 0;
                }
                return ret;
            }
            public int FindHighestPage(){
                int ret;
                lock (_sync)
                {
                    if (archivedPages.Count() == 0)
                        ret = 0;
                    else
                        ret = archivedPages.Max();
                }
                return ret;
            }
            public int FindLowestPage(int start, int end)
            {
                int ret;
                lock (_sync)
                {
                    ret = end + 1;
                    for (int i = start; i <= end; ++i)
                        if (archivedPages.IndexOf(i) < 0)
                        {
                            ret = i;
                            break;
                        }
                }
                return ret;
            }
            public void Add(int page)
            {
                lock (_sync)
                {
                    archivedPages.Add(page);
                }
            }
            public int GetRequest()
            {
                int ret;
                lock (_sync)
                {
                    ret = request;
                }
                if (IsPageArchived(ret))
                {
                    Request(0);
                    return 0;
                }
                return ret;
            }
            public void Request(int pgno)
            {
                lock (_sync)
                {
                    request = pgno;
                }
            }
        }

        protected ArchiveLock archivedPages = new ArchiveLock();
       public  enum PagesOfImportance
        {
            HOMESTUCK_PAGE_ONE = 001901,
            CASCADE = 006009,
            CALIBORN_PAGE_SMASH = 007395,
            CALIBORN_PAGE_SMASH2 = 007680,
            DOTA = 006715,
            SHES8ACK = 009305,
            GAMEOVER = 008801,
        }
        float totalMegabytesDownloaded = 0;


        public abstract void Connect(string serverName, string username, string password, bool resetDatabase);
        public bool Initialize()
        {
            if (ReadLastIndexedOrCreateDatabase())
            {
                parser = new Parser();
                lastPage = parser.GetLatestPage();
                if (lastPage == 0)
                {
                    lastPage = archivedPages.FindHighestPage();
                    if (lastPage == 0)
                    {
                        MessageBox.Show("The database is empty. Cannot read MSPA.");
                        return false;
                    }
                }

                if (!IconsAreParsed())
                {
                    Transact();
                    try
                    {
                        parser.LoadIcons();
                        WriteResource(parser.GetResources(), 100000, false);
                        Commit();
                    }
                    catch
                    {
                        Rollback();
                        MessageBox.Show("Unable to load icons above pages! Parsing failure!");
                        return false;
                    }
                }
                return true;
            }
            MessageBox.Show("Error creating the database!");
            return false;
        }
        public enum IconTypes{
            CANDYCORN,
            CUEBALL,
            CALIBORNTOOTH,
        }
        
        public abstract byte[] GetIcon(IconTypes ic);
        public abstract bool IconsAreParsed();
        public abstract bool ReadLastIndexedOrCreateDatabase();
        public abstract void WriteResource(Parser.Resource[] res, int page, bool x2);
        public abstract void WriteLinks(Parser.Link[] res, int page, bool x2);
        public abstract void WriteText(Parser.Text tex, int page, bool x2);
        public abstract void ArchivePageNumber(int page, bool x2);
        public abstract void Transact();
        public abstract void Rollback();
        public abstract void Commit();
        public abstract void Close();
        public abstract bool TricksterParsed();

        public abstract Parser.Resource[] GetTricksterShit();
        protected void ParseTrickster(bool serial)
        {
            if (!TricksterParsed())
            {
                Transact();
                try
                {
                    parser.LoadTricksterResources(serial);
                    WriteResource(parser.GetResources(), 100001, false);
                    Commit();
                }
                catch
                {
                    Rollback();
                    throw;
                }
            }
        }

        public abstract Page GetPage(int pageno,bool x2);
        public Style GetStyle(int pageno){
            if (pageno == (int)PagesOfImportance.CASCADE)
                return Style.CASCADE;
            if (pageno == (int)PagesOfImportance.DOTA)
                return Style.DOTA;
            if (pageno == (int)PagesOfImportance.SHES8ACK)
                return Style.SHES8ACK;
            if (Parser.IsHomosuck(pageno))
                return Style.HOMOSUCK;
            if (Parser.Is2x(pageno))
                return Style.X2;
            if (pageno == (int)PagesOfImportance.CALIBORN_PAGE_SMASH || pageno == (int)PagesOfImportance.CALIBORN_PAGE_SMASH2)
                return Style.SMASH;
            if (parser.IsScratch(pageno))
                return Style.SCRATCH;
            if (pageno == (int)PagesOfImportance.GAMEOVER)
                return Style.GAMEOVER;
            if (Parser.IsTrickster(pageno))
                return Style.TRICKSTER;
            if (pageno == 5982)
                return Style.SBAHJ;
            return Style.REGULAR;
        }
        public Page WaitPage(int pageno)
        {
            try
            {
                if (!archivedPages.IsPageArchived(pageno))
                {
                    if (!wl.TestAndSet())
                    {
                        try
                        {
                            if (SavePage(pageno) > 0)
                                return null;
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            wl.StopRunning();
                        }
                    }
                    else
                    {
                        archivedPages.Request(pageno);
                        do
                        {
                            System.Threading.Thread.Sleep(1000);
                        } while (!archivedPages.IsPageArchived(pageno));
                    }
                }
                return GetPage(pageno, Parser.Is2x(pageno));
            }
            catch
            {
                return null;
            }
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
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing Cascade, page 6009");
            Parser.Resource[] cascadeSegments = new Parser.Resource[7];
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
            if (bgw != null)
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
                if (bgw != null)
                    bgw.ReportProgress(progress, cascadeSegments[i].originalFileName + ": " + fileSize2 / 1024 + "KB");
            }

            cascadeSegments[6] = new Parser.Resource(parser.DownloadFile("http://www.mspaintadventures.com/images/header_cascade.gif"), "header_cascade.gif");
            
            Transact();
            WriteResource(cascadeSegments, 6009,false);
            WriteLinks(next, 6009,false);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000","",0);
            asdf.title = "[S] Cascade.";
            WriteText(asdf, 6009, false);

            ///BRB
            ArchivePageNumber(6009,false);
            Commit();
            if (bgw != null)
                bgw.ReportProgress(progress, "Cascade committed!");
        }
        void HandlePageSmash(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing Caliborn's hissy fit.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/007395/05492.swf"), "05492.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.CALIBORN_PAGE_SMASH, false);
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.CALIBORN_PAGE_SMASH + 1);
            WriteLinks(lnk, (int)PagesOfImportance.CALIBORN_PAGE_SMASH, false);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "[S] Cascade.";
            WriteText(asdf, (int)PagesOfImportance.CALIBORN_PAGE_SMASH, false);
            ArchivePageNumber((int)PagesOfImportance.CALIBORN_PAGE_SMASH, false);
            Commit();

        }
        void HandlePageSmash2(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing Caliborn's hissy fit.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://www.mspaintadventures.com/007680/05777_2.swf"), "05777_2.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.CALIBORN_PAGE_SMASH2, false);
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.CALIBORN_PAGE_SMASH2 + 1);
            WriteLinks(lnk, (int)PagesOfImportance.CALIBORN_PAGE_SMASH2, false);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "[S] Cascade.";
            WriteText(asdf, (int)PagesOfImportance.CALIBORN_PAGE_SMASH2, false);
            ArchivePageNumber((int)PagesOfImportance.CALIBORN_PAGE_SMASH2, false);
            Commit();

        }
        void HandleDota(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing Hussie's rekage.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/DOTA/04812.swf"), "04812.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.DOTA, false);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.DOTA + 1);
            WriteLinks(lnk, (int)PagesOfImportance.DOTA, false);
            WriteText(asdf, (int)PagesOfImportance.DOTA, false);
            ArchivePageNumber((int)PagesOfImportance.DOTA, false);
            Commit();
        }
        void FailToHandleVriska(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing the huge 8itch.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://www.mspaintadventures.com/shes8ack/07402.swf"), "07402.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.SHES8ACK, false);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.SHES8ACK + 1);
            WriteLinks(lnk, (int)PagesOfImportance.SHES8ACK, false);
            WriteText(asdf, (int)PagesOfImportance.SHES8ACK, false);
            ArchivePageNumber((int)PagesOfImportance.SHES8ACK, false);
            Commit();
        }
        void FailMiserably(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing death.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/storyfiles/hs2/GAMEOVER/06898.swf"), "06898.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.GAMEOVER, false);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.GAMEOVER + 1);
            WriteLinks(lnk, (int)PagesOfImportance.GAMEOVER, false);
            WriteText(asdf, (int)PagesOfImportance.GAMEOVER, false);
            ArchivePageNumber((int)PagesOfImportance.GAMEOVER, false);
            Commit();
        }
        public void ResumeWork(System.ComponentModel.BackgroundWorker bgw)
        {

            int currentProgress;

            while (wl.TestAndSet()) { System.Threading.Thread.Sleep(1000); }
            try
            {
                while (true)
                {
                    int missedPages = 0;

                    int startPage = archivedPages.FindLowestPage((int)PagesOfImportance.HOMESTUCK_PAGE_ONE, lastPage);
                    int currentPage = startPage;
                    int pagesToParse = lastPage - startPage;
                    currentProgress = (int)(((float)(currentPage - 1 - startPage) / (float)(pagesToParse)) * 100.0f);


                    //debug set current page here
                    //currentPage = 6708;
                    //currentPage = 7326;
                    //currentPage = 4163;
                    //currentPage = 1926;
                    //currentPage = 7690;
                    //currentPage = 6009;

                    if (!bgw.CancellationPending)
                        bgw.ReportProgress(currentProgress, "MSPA is up to page " + lastPage);
                    else
                        return;
                    if (!bgw.CancellationPending)
                        bgw.ReportProgress(currentProgress, "Starting archive operation at page " + currentPage);
                    else
                        return;

                    int oldPage = currentPage;
                    while (currentPage != lastPage + 1 && !bgw.CancellationPending)
                    {
                        var req = (archivedPages.GetRequest());
                        if (req != 0)
                        {
                            currentPage = req;
                            bgw.ReportProgress(currentProgress, "Responding to User Request for Page " + req);
                        }
                        currentProgress = (int)(((float)(currentPage - 1 - startPage) / (float)(pagesToParse)) * 100.0f);

                        var oldMissedPages = missedPages;


                        missedPages += SavePage(currentPage,bgw,currentProgress);

                        if (req != 0 && currentPage == req && oldMissedPages == missedPages)
                        {
                            currentPage = oldPage;
                            archivedPages.Request(0);
                        }

                        currentPage = archivedPages.FindLowestPage(currentPage + 1, lastPage);
                    }
                    if (!(!bgw.CancellationPending && missedPages != 0))
                        break;
                    bgw.ReportProgress(currentProgress, "Missed " + missedPages + " pages. Looping back.");
                }

                bgw.ReportProgress(100, "Operation completed.");
            }
            catch
            {
                throw;
            }
            finally
            {
                wl.StopRunning();
            }
        }
        int SavePage(int currentPage, System.ComponentModel.BackgroundWorker bgw = null, int currentProgress = 0)
        {
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
                        case PagesOfImportance.CALIBORN_PAGE_SMASH2:
                            HandlePageSmash2(bgw, currentProgress);
                            break;
                        case PagesOfImportance.DOTA:
                            HandleDota(bgw, currentProgress);
                            break;
                        case PagesOfImportance.SHES8ACK:
                            FailToHandleVriska(bgw, currentProgress);
                            break;
                        case PagesOfImportance.GAMEOVER:
                            FailMiserably(bgw, currentProgress);
                            break;
                    }
                }
                catch
                {
                    Rollback();
                    if(bgw != null)
                        bgw.ReportProgress(currentProgress, "Error parsing special page " + currentPage);
                    return 1;
                }
                return 0;
            }
            if (archivedPages.IsPageArchived(currentPage) || (bgw != null && bgw.CancellationPending))
                return 0;

            if (Parser.IsTrickster(currentPage))
                ParseTrickster(bgw != null);

            if (parser.LoadPage(currentPage))
            {
                int missedPages = 0;
                if (!parser.x2Flag)
                {
                    if (!WritePage(bgw, currentPage, currentProgress, 0))
                        missedPages++;
                }
                else
                {
                    if (!WritePage(bgw, currentPage, currentProgress, 1))
                        missedPages += 2;
                    else
                    {
                        parser.Reparse();
                        if (!WritePage(bgw, currentPage, currentProgress, 2))
                            missedPages += 2;
                    }
                }
                //simple enough, leave it to the reader to decode the multiple pages
                return missedPages;
            }
            else if (!parser.x2Flag)
                return 1;
            else
                return 2;
            
        }
        bool WritePage(System.ComponentModel.BackgroundWorker bgw, int currentPage, int currentProgress, int x2phase)
        {
            try
            {
                var res = parser.GetResources();
                var links = parser.GetLinks();
                var text = parser.GetText();

                if (bgw != null)
                    bgw.ReportProgress(currentProgress, "Page " + currentPage + ":" + text.title);

                if (text.narr == null)
                {
                    if (bgw != null)
                        bgw.ReportProgress(currentProgress, text.promptType);
                    for (int i = 0; i < text.lines.Count(); ++i)
                    {
                        if (!text.lines[i].isImg)
                        {
                            if (bgw != null)
                                bgw.ReportProgress(currentProgress, (text.lines[i].subTexts != null ? text.lines[i].subTexts.Count() : 0) + " special subtexts, Colour: " + text.lines[i].hexColour + ": " + text.lines[i].text);
                        }
                        else
                            if (bgw != null)
                                bgw.ReportProgress(currentProgress, "Imageline");
                    }
                }
                else if(bgw != null)
                    bgw.ReportProgress(currentProgress, "Narrative: " + text.narr.text);

                if(bgw != null)
                    for (int i = 0; i < links.Count(); ++i)
                        bgw.ReportProgress(currentProgress, "\"" + links[i].originalText + "\" links to " + links[i].pageNumber);
                for (int i = 0; i < res.Count(); ++i)
                {
                    var fileSize = res[i].data.Count();
                    totalMegabytesDownloaded += (float)fileSize / (1024.0f * 1024.0f); 
                    if (bgw != null)
                        bgw.ReportProgress(currentProgress, res[i].originalFileName + ": " + fileSize / 1024 + "KB");
                } 
                if (bgw != null)
                    bgw.ReportProgress(currentProgress, "Total Data Downloaded: " + (int)totalMegabytesDownloaded + "MB");
               
                if(x2phase != 2)
                    Transact();
                WriteResource(res, currentPage, x2phase == 2);
                WriteLinks(links, currentPage, x2phase == 2);
                WriteText(text, currentPage, x2phase == 2);
                ArchivePageNumber(currentPage, x2phase == 2);
                if(x2phase != 1)
                    Commit();

            }
            catch
            {
                Debugger.Break();
                Rollback(); 
                if (bgw != null)
                    bgw.ReportProgress(currentProgress, "Error in archiving page: " + currentPage);
                return false;

            }
            return true;
        }
    }
}
