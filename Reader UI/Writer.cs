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
        public enum SpecialResources
        {
            CANDYCORNS = 100000,
            TRICKSTER_HEADER = 100001,
            X2_HEADER = 100002,
        }
        public enum Versions{
            Database = 4, //update with every commit that affects db layout
            Program = 2,
            Release = Program + 1
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
            JAILBREAK,
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
            VOID,
            OVERSHINE,
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
        public abstract void Prune(int pageno);
        protected class ArchiveLock
        {
            private List<int> archivedPages = new List<int>();
            private int request = 0;
            private object _sync = new object();
            private bool ff = false;
            private bool pruned;

            public bool IsPageArchived(int page)
            {
                bool ret;
                lock (_sync)
                {
                    ret = archivedPages.IndexOf(page) >= 0;
                }
                return ret;
            }
            public int Prune()
            {
                int ret;
                lock (_sync)
                {
                    if (!pruned && archivedPages.Count() > 0)
                    {
                        ret = archivedPages.Max();
                        archivedPages.RemoveAt(archivedPages.IndexOf(ret));
                        pruned = true;
                    }else
                        ret = 0;
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
                return Writer.ValidRange(ret);
            }
            public int GetParseCount(int start, int end)
            {
                int ret = 0;
                lock (_sync)
                {
                    for (int i = start; i <= end; ++i)
                        if (archivedPages.IndexOf(i) >= 0)
                        {
                            ret++;
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
                    request = 0;
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
            public void FailFlag()
            {
                lock (_sync)
                {
                    ff = true;
        }
            }
            public bool Failed()
            {
                bool ret;
                lock (_sync)
                {
                    ret = ff;
                    ff = false;
                }
                return ret;
            }
        }

        protected ArchiveLock archivedPages = new ArchiveLock();
       public  enum PagesOfImportance
        {
           JAILBREAK_LAST_PAGE = 000136,
            CASCADE = 006009,
            CALIBORN_PAGE_SMASH = 007395,
            CALIBORN_PAGE_SMASH2 = 007680,
            DOTA = 006715,
            SHES8ACK = 009305,
            GAMEOVER = 008801,
           OVERSHINE = 009304,
        }
       public enum StoryBoundaries
       {
           JAILBREAK_PAGE_ONE = 2,
           JAILBREAK_LAST_PAGE = 136,
           HSB = 1893,
           EOHSB = 1900,
           HOMESTUCK_PAGE_ONE = 1901,
           HS_EOA1 = 2147,
           HS_A2 = 2149,        //yes i know the act boundaries aren't cohesive but that's not my problem, if the reader want's the inbetweens they should pick part 1 or all of homestuck
           HS_EOA2 = 2658,
           HS_A3 = 2660,
           HS_EOA3 = 3053,
           HS_I1 = 3054,
           HS_EOI1 = 3257,
           HS_A4 = 3258,
           HS_EOA4 = 3841,
           HS_EOP1 = 3888,  //because that's a lot of missing pages, since EOA4
       }
        float totalMegabytesDownloaded = 0;


        public abstract void Connect(string DatabaseName, string serverName, string username, string password, int port, bool resetDatabase);
        public bool Initialize(System.ComponentModel.BackgroundWorker bgw)
        {
            if (ReadLastIndexedOrCreateDatabase(bgw))
            {
                parser = new Parser();
                bgw.ReportProgress(0, "Checking MSPA for latest page...");
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
                else{
                    var pg = archivedPages.Prune();
                    if (pg != 0)
                    {
                        bgw.ReportProgress(0, "Reparsing last page...");
                        Prune(pg);
                        SavePage(pg);
                    }
                }

                if (!IconsAreParsed())
                {
                    bgw.ReportProgress(0, "Downloading header icons...");
                    Transact();
                    try
                    {
                        parser.LoadIcons();
                        WriteResource(parser.GetResources(), (int)SpecialResources.CANDYCORNS, false);
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
        public abstract bool ReadLastIndexedOrCreateDatabase(System.ComponentModel.BackgroundWorker bgw);
        public abstract void WriteResource(Parser.Resource[] res, int page, bool x2);
        public abstract void WriteLinks(Parser.Link[] res, int page);
        public abstract void WriteText(Parser.Text tex, int page, bool x2);
        public abstract void ArchivePageNumber(int page, bool x2);
        public abstract void Transact();
        public abstract void Rollback();
        public abstract void Commit();
        public abstract void Close();
        public abstract bool TricksterParsed();
        public abstract bool x2HeaderParsed();
        public abstract byte[] Getx2Header();

        public abstract Parser.Resource[] GetTricksterShit();
        protected void ParseTrickster(bool serial)
        {
            
            if (!TricksterParsed())
            {
                if (!wl.TestAndSet())
                {
                Transact();
                try
                {
                    parser.LoadTricksterResources(serial);
                        WriteResource(parser.GetResources(), (int)SpecialResources.TRICKSTER_HEADER, false);
                    Commit();
                }
                catch
                {
                    Rollback();
                    throw;
                }
            }
        }
        }
        protected void Parsex2Header(bool serial)
        {
            if (!x2HeaderParsed())
            {
                Transact();
                try
                {
                    parser.GetX2Header(serial);
                    WriteResource(parser.GetResources(), (int)SpecialResources.X2_HEADER, false);
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
            if (pageno <= (int)StoryBoundaries.JAILBREAK_LAST_PAGE)
                return Style.JAILBREAK;
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
            if ((pageno >= 9000 && pageno <= 9024) || (pageno >= 9174 && pageno <= 9183) || (pageno >= 9289 && pageno <= 9303))
                return Style.VOID;
            if (pageno == (int)PagesOfImportance.OVERSHINE)
                return Style.OVERSHINE;
            return Style.REGULAR;
        }
        public Page WaitPage(int pageno, System.ComponentModel.BackgroundWorker bgw)
        {
            try
            {
                if (Enum.IsDefined(typeof(SpecialResources), pageno))
                {
                    if (!wl.TestAndSet())
                    {
                        SavePage(pageno, bgw);
                        wl.StopRunning();
                    }
                    else
                    {
                        do
                        {
                            archivedPages.Request(pageno);
                            System.Threading.Thread.Sleep(1000);
                        } while (archivedPages.GetRequest() != 0);
                    }
                    return null;
                }
                if (!archivedPages.IsPageArchived(pageno))
                {
                    if (!wl.TestAndSet())
                    {
                        try
                        {
                            if (!SavePage(pageno,bgw))
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
                            if (archivedPages.Failed())
                                return null;
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
            next[0] = new Parser.Link("END OF ACT 5", (int)PagesOfImportance.CASCADE + 1);
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

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "[S] Cascade.";
            Transact();
            WriteResource(cascadeSegments, (int)PagesOfImportance.CASCADE, false);
            WriteLinks(next, (int)PagesOfImportance.CASCADE);
            WriteText(asdf, (int)PagesOfImportance.CASCADE, false);

            ///BRB
            ArchivePageNumber((int)PagesOfImportance.CASCADE,false);
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

            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.CALIBORN_PAGE_SMASH + 1);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "[S] Cascade.";
            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.CALIBORN_PAGE_SMASH, false);
            WriteLinks(lnk, (int)PagesOfImportance.CALIBORN_PAGE_SMASH);
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

            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.CALIBORN_PAGE_SMASH2 + 1);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "[S] Cascade.";
            Transact();
            WriteLinks(lnk, (int)PagesOfImportance.CALIBORN_PAGE_SMASH2);
            WriteResource(FUCKYOU, (int)PagesOfImportance.CALIBORN_PAGE_SMASH2, false);
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

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.DOTA + 1);
            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.DOTA, false);
            WriteLinks(lnk, (int)PagesOfImportance.DOTA);
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

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.SHES8ACK + 1);
            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.SHES8ACK, false);
            WriteLinks(lnk, (int)PagesOfImportance.SHES8ACK);
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

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.GAMEOVER + 1);
            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.GAMEOVER, false);
            WriteLinks(lnk, (int)PagesOfImportance.GAMEOVER);
            WriteText(asdf, (int)PagesOfImportance.GAMEOVER, false);
            ArchivePageNumber((int)PagesOfImportance.GAMEOVER, false);
            Commit();
        }
        void HandleOvershine(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing zap.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/storyfiles/hs2/07401.gif"), "07401.gif");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("[S][A6A6I4] ====>", (int)PagesOfImportance.OVERSHINE + 1);
            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.OVERSHINE, false);
            WriteLinks(lnk, (int)PagesOfImportance.OVERSHINE);
            WriteText(asdf, (int)PagesOfImportance.OVERSHINE, false);
            ArchivePageNumber((int)PagesOfImportance.OVERSHINE, false);
            Commit();
        }
        void HandleJailbreakLast(System.ComponentModel.BackgroundWorker bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing your victory.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(parser.DownloadFile("http://cdn.mspaintadventures.com/storyfiles/jb2/YOUWIN.gif"), "YOUWIN.gif");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "Would you like to play again?", 0);
            asdf.title = "Enjoy restful slumber.";
            Transact();
            WriteResource(FUCKYOU, (int)PagesOfImportance.JAILBREAK_LAST_PAGE, false);
            WriteLinks(new Parser.Link[0], (int)PagesOfImportance.JAILBREAK_LAST_PAGE);
            WriteText(asdf, (int)PagesOfImportance.JAILBREAK_LAST_PAGE, false);
            ArchivePageNumber((int)PagesOfImportance.JAILBREAK_LAST_PAGE, false);
            Commit();
        }
        public static int ValidRange(int pg)
        {
            if (pg < (int)StoryBoundaries.JAILBREAK_PAGE_ONE)
                return (int)StoryBoundaries.JAILBREAK_PAGE_ONE;
            if (pg <= (int)StoryBoundaries.JAILBREAK_LAST_PAGE)
                return pg;
            if (pg < (int)StoryBoundaries.HSB)
                return (int)StoryBoundaries.HSB;
            if (pg <= (int)StoryBoundaries.EOHSB)
                return pg;
            if (pg < (int)StoryBoundaries.HOMESTUCK_PAGE_ONE)
                return (int)StoryBoundaries.HOMESTUCK_PAGE_ONE;
            //TODO: Check for empty pages
            return pg;
        }
        public void ResumeWork(System.ComponentModel.BackgroundWorker bgw, int startPage, int lastPage)
        {

            int currentProgress;

            while (wl.TestAndSet()) { System.Threading.Thread.Sleep(1000); }
            try
            {
                List<int> missedPages = new List<int>();
                bool missedRound = false;
                int currentPage;
                startPage = ValidRange(startPage);
                int pagesToParse = lastPage - startPage;
                int pagesParsed = 0;
                while (true)
                {

                    if (!missedRound)
                    {
                        currentPage = archivedPages.FindLowestPage(startPage, lastPage);
                        pagesParsed = archivedPages.GetParseCount(startPage, lastPage);

                        currentProgress = (int)(((float)(pagesParsed) / (float)(pagesToParse)) * 100.0f);
                        if (!bgw.CancellationPending)
                        {
                            bgw.ReportProgress(currentProgress, "Starting archive operation at page " + startPage);
                        }
                        else
                        {
                            bgw.ReportProgress(currentProgress, "Operation cancelled.");
                            return;
                        }
                    }
                    else
                    {
                        currentPage = missedPages[0];
                        currentProgress = (int)(((float)(pagesParsed) / (float)(pagesToParse)) * 100.0f);
                        if (currentPage > lastPage)
                        {
                            bgw.ReportProgress(100, "Range already archived!");
                            return;
                        }
                    }

                    if (!bgw.CancellationPending)
                    {
                        if (currentPage != startPage)
                            bgw.ReportProgress(currentProgress, "Resuming from " + currentPage);
                    }
                    else
                    {
                        bgw.ReportProgress(currentProgress, "Operation cancelled.");
                        return;
                    }

                    int oldPage;
                    while (!bgw.CancellationPending)
                    {
                        oldPage = currentPage;
                        var req = (archivedPages.GetRequest());
                        if (req != 0)
                        {
                            currentPage = req;
                            bgw.ReportProgress(currentProgress, "Responding to User Request for Page " + req);
                        }
                        currentProgress = (int)(((float)(pagesParsed) / (float)(pagesToParse)) * 100.0f);

                        var oldMissedPages = missedPages;

                        if (!SavePage(currentPage, bgw, currentProgress))
                        {
                            bgw.ReportProgress(currentProgress, "Failed to parse page " + currentPage + "! Added to miss queue.");
                            missedPages.Add(currentPage);
                        }
                        else
                            pagesParsed++;

                        if (req != 0)
                            currentPage = oldPage;
                        else if (missedRound)
                        {
                            if (missedPages.Count() == 0)
                                break;
                            missedPages.RemoveAt(0);
                            currentPage = missedPages[0];
                        }
                        else
                        {
                            if (currentPage >= lastPage)
                                break;
                            currentPage = ValidRange(archivedPages.FindLowestPage(currentPage + 1, lastPage));
                        }
                    }
                    if (bgw.CancellationPending || missedPages.Count() == 0)
                        break;
                    missedRound = true;
                    bgw.ReportProgress(currentProgress, "Missed " + missedPages.Count() + " pages. Iterating through missed queue.");
                }
                if (bgw.CancellationPending)
                    bgw.ReportProgress(currentProgress, "Operation cancelled.");
                else
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
        bool SavePage(int currentPage, System.ComponentModel.BackgroundWorker bgw = null, int currentProgress = 0)
        {
            if (Enum.IsDefined(typeof(PagesOfImportance), currentPage))
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
                        case PagesOfImportance.OVERSHINE:
                            HandleOvershine(bgw, currentProgress);
                            break;
                        case PagesOfImportance.JAILBREAK_LAST_PAGE:
                            HandleJailbreakLast(bgw, currentProgress);
                            break;
                    }
                    archivedPages.Add(currentPage);
                }
                catch
                {
                    Rollback();
                    if (bgw != null)
                        bgw.ReportProgress(currentProgress, "Error parsing special page " + currentPage);
                    return false;
                }
                return true;
            }
            if ((Enum.IsDefined(typeof(SpecialResources), currentPage)))
            {
                try
                {
                    switch ((SpecialResources)currentPage)
                    {
                        case SpecialResources.TRICKSTER_HEADER:
                            ParseTrickster(false);
                            break;
                        case SpecialResources.X2_HEADER:
                            Parsex2Header(false);
                            break;
                        default:
                            Debugger.Break();
                            throw new Exception();
                    }
                }
                catch
                {
                    if (bgw != null)
                        bgw.ReportProgress(currentProgress, "Error parsing special page " + currentPage);
                    return false;
                }
                return true;
            }
            if (archivedPages.IsPageArchived(currentPage) || (bgw != null && bgw.CancellationPending))
                return true;

            if (Parser.IsTrickster(currentPage))
                ParseTrickster(bgw != null);
            if (Parser.Is2x(currentPage))
                Parsex2Header(bgw != null);

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
                return missedPages == 0;
            }
            else
                return false;
            
        }
        bool WritePage(System.ComponentModel.BackgroundWorker bgw, int currentPage, int currentProgress, int x2phase)
        {
            try
            {
                var res = parser.GetResources();
                var links = parser.GetLinks();
                var text = parser.GetText();

                if (bgw != null)
                    bgw.ReportProgress(currentProgress, "Page " + currentPage + ": " + text.title);

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
                WriteLinks(links, currentPage);
                WriteText(text, currentPage, x2phase == 2);
                ArchivePageNumber(currentPage, x2phase == 2);
                if (x2phase != 1)
                {
                    Commit();
                    archivedPages.Add(currentPage);
                }

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
