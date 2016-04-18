using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using VideoLibrary;

namespace Reader_UI
{
    public abstract class Writer
    {
        public enum SpecialResources
        {
            CANDYCORNS = 100000,
            TRICKSTER_HEADER = 100001,
            X2_HEADER = 100002,
            TEREZI_PASSWORD = 100003,
        }
        public enum Versions{
            Database = 4, //update with every commit that affects db layout
            Program = 3,
            Release = Program + 1
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
            public Page(int no, Parser.Text t, Parser.Resource[] r, Parser.Link[] l)
            {
                number = no;
                meta = t;
                resources = r;
                links = l;
            }
            public Parser.Text meta,meta2;
            public Parser.Resource[] resources,resources2;
            public Parser.Link[] links,links2;
            public bool x2 = false;
        }
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
        class BGWSerializer
        {
            class ProgMessage{
                public int Progress {get; set;}
                public string Message {get; set;}
            }
            private ForegroundWorker bgw;

            object _klock = new object();
            object _wlock = new object();
            UInt64 k = 0;
            Dictionary<UInt64, List<ProgMessage>> messageQueue = new Dictionary<UInt64, List<ProgMessage>>();
            public BGWSerializer(ForegroundWorker b)
            {
                bgw = b;
            }
            public UInt64 GetKey()
            {
                lock (_klock)
                {
                    var ret = k;
                    k = checked(k + 1);
                    messageQueue.Add(ret, new List<ProgMessage>());
                    return ret;    //overflow really should never occur, an int 16 could handle this job
                }
            }
            public void ReportProgress(UInt64 key, int prog, string msg)
            {
                lock (_wlock)
                {
                    messageQueue[key].Add(new ProgMessage { Progress = prog, Message = msg });
                }
            }
            public void Commit(UInt64 key)
            {
                lock(_wlock){
                    var commital = messageQueue[key];
                    messageQueue.Remove(key);
                    foreach (var pm in commital)
                        bgw.ReportProgress(pm.Progress, pm.Message);
                }
            }
        }
        class WrapBGW
        {
            public UInt64 key;
            public BGWSerializer bgw;
            public WrapBGW(BGWSerializer b, UInt64 k)
            {
                bgw = b;
                key = k;
            }
            public void ReportProgress(int prog, string msg)
            {
                bgw.ReportProgress(key, prog, msg);
            }
        }
        class PageSavesManager
        {
            object _rwlock = new object();
            object _plock = new object();
            object _mlock = new object();
            object _proglock = new object();
            ForegroundWorker bgw;
            int runningWorkers = 0, expectedWorkers = 0, _pagesParsed, _currentProgress;
            List<int> missedPages = new List<int>();
            public PageSavesManager(int pp, ForegroundWorker b)
            {
                _pagesParsed = pp;
                bgw = b;
            }
            public int CurrentProgress
            {
                get
                {
                    lock (_proglock)
                    {
                        return _currentProgress;
                    }
                }
                set
                {
                    lock (_proglock)
                    {
                        _currentProgress = value;
                    }
                }
            }
            public int PagesParsed
            {
                get
                {
                    lock (_plock)
                    {
                        return _pagesParsed;
                    }
                }
            }
            public bool AddWorker()
            {
                lock (_rwlock)
                {
                    expectedWorkers--;
                    if (bgw.CancellationPending)
                        return false;
                    runningWorkers++;
                    return true;
                }
            }
            public void RemoveWorker(int pagefail)
            {
                if (pagefail != 0)
                {
                    lock (_mlock)
                    {
                        missedPages.Add(pagefail);
                    }
                }
                else
                {
                    lock (_plock)
                    {
                        _pagesParsed++;
                    }
                }
                lock (_rwlock)
                {
                    runningWorkers--;
                }
            }
            public bool WaitForWorkers()
            {
                lock (_rwlock)
                {
                    return runningWorkers == 0 && expectedWorkers == 0;
                }
            }
        
            public int MissedPagesCount()
            {
                lock (_mlock)
                {
                    return missedPages.Count;
                }
            }
            public int MissedPagesTop()
            {
                lock (_mlock)
                {
                    return missedPages[0];
                }
            }
            public int MissedPagesPop()
            {
                lock (_mlock)
                {
                    var ret = missedPages[0];
                    missedPages.RemoveAt(0);
                    return ret;
                }
            }

            public void ExpectWorker()
            {
                lock (_rwlock)
                {
                    expectedWorkers++;
                }
            }
        }
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
           COLLIDE = 009987,
           ACT7 = 010027,
        }
        public enum PasswordPages
        {
            _1 = 9135,
            _2 = 9150,
            _3 = 9188,
            _4 = 9204,
            _5 = 9222,
            _6 = 9263,
            _7 = 9109,
            _8 = 9058,
        }
       public enum StoryBoundaries
       {
           JAILBREAK_PAGE_ONE = 2,
           JAILBREAK_LAST_PAGE = 136,
           BQ = 170,
           EOBQ = 216,
           RQ = 137,
           EORQ = 151,
           PS = 219,
           PSC2 = 302,
           PSC3 = 402,
           PSC4 = 448,
           PSC5 = 546,
           PSC6 = 604,
           PSC7 = 666,
           PSC8 = 742,
           PSC9 = 816,
           PSC10 = 873,
           PSC11 = 953,
           PSC12 = 1030,
           PSC13 = 1069,
           PSC14 = 1149,
           PSC15 = 1257,
           PSC16 = 1299,
           PSC17 = 1406,
           PSC18 = 1466,
           PSC19 = 1507,
           PSC20 = 1589,
           PSC21 = 1655,
           PSC22 = 1708,
           PSE = 1841,
           EOPS = 1892,
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
           HS_A5A1 = 3389,
           HS_EOA5A1 = 4525,
           HS_A5A2 = 4526,
           HS_A5A2S = 5663,
           HS_EOA5A2S = 5983,
           HS_EOA5A2 = 6008,
           HS_CASCADE = 6009,
           HS_EOA5 = 6010,
           HS_I2 = 6011,
           HS_EOI2 = 6012,
           HS_A6 = 6013,
           HS_EOA6A1 = 6184,
           HS_A6I1 = 6195,
           HS_EOA6I1 = 6290,
           HS_A6A2 = 6320,
           HS_EOA6A2 = 6566,
           HS_A6I2 = 6567,
           HS_EOA6I2 = 6716,
           HS_A6A3 = 6720,
           HS_EOA6A3 = 7162,
           HS_A6I3 = 7163,
           HS_EOA6I3 = 7337,
           HS_A6A4 = 7338,
           HS_A6I4 = 7341,
           HS_EOA6I4 = 7411,
           HS_A6A5 = 7412,
           HS_EOA6A5A1 = 7613,
           HS_A6A5A2 = 7614,
           HS_EOA6A5A2 = 7677,
           HS_EOA6A5 = 7826,
           HS_A6I5 = 7827,
           HS_EOA6I5 = 8135,
           HS_A6A6 = 8143,
           HS_EOA6A6A1 = 8166,
           HS_A6A6I1 = 8178,
           HS_EOA6A6I1 = 8374,
           HS_A6A6A2 = 8175,
           HS_EOA6A6A2 = 8430,
           HS_A6A6I2 = 8431,
           HS_EOA6A6I2 = 8752,
           HS_A6A6A3 = 8753,
           HS_EOA6A6A3 = 8801,
           HS_A6A6I3 = 8801,
           HS_EOA6A6I3 = 8820,
           HS_A6A6A4 = 8821,
           HS_EOA6A6A4 = 8843,
           HS_A6A6I4 = 8844,
           HS_EOA6A6I4 = 9308,
           HS_A6A6A5 = 9309,
           HS_EOA6A6A5 = 9347,  //mental breakdown isn't green so i don't consider it part of the act
           HS_A6A6I5 = 9349,
           HS_A6A6A6 = 009987,
           HS_A7 = 010027,
           HS_END = 010028,
        }
        float totalMegabytesDownloaded = 0;


        public abstract void Connect(string DatabaseName, string serverName, string username, string password, int port, bool resetDatabase);
        public bool Initialize(System.ComponentModel.BackgroundWorker bgw)
        {
            if (ReadLastIndexedOrCreateDatabase(bgw))
            {
                using (var parser = new Parser())
                {
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
                    else if (lastPage != (int)StoryBoundaries.HS_END)
                    {
                        var pg = archivedPages.Prune();
                        if (pg != 0)
                        {
                            bgw.ReportProgress(0, "Reparsing last page... The may take a while if you stopped on a big page");
                            Prune(pg);
                            SavePage(pg);
                        }
                    }

                    if (!IconsAreParsed())
                    {
                        bgw.ReportProgress(0, "Downloading header icons...");
                        try
                        {
                            parser.LoadIcons();
                            WriteResource(parser.GetResources(), (int)SpecialResources.CANDYCORNS);
                        }
                        catch
                        {
                            MessageBox.Show("Unable to load icons above pages! Parsing failure!");
                            return false;
                        }
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
        public abstract void WritePageToDB(Page page);
        public abstract void WriteResource(Parser.Resource[] res, int page);
        public abstract void Close();
        public abstract bool TricksterParsed();
        public abstract bool x2HeaderParsed();
        public abstract byte[] Getx2Header();
        public abstract bool TereziParsed();
        public abstract byte[] GetTerezi();
        public abstract Parser.Resource[] GetTricksterShit();
        protected void ParseTrickster()
        {

            if (!TricksterParsed())
            {
                if (!wl.TestAndSet())
                {
                    using (var parser = new Parser())
                    {
                        parser.LoadTricksterResources();
                        WriteResource(parser.GetResources(), (int)SpecialResources.TRICKSTER_HEADER);
                    }
                }
            }
        }
        protected void Parsex2Header()
        {
            if (!x2HeaderParsed())
            {
                using (var parser = new Parser())
                {
                    parser.GetX2Header();
                    WriteResource(parser.GetResources(), (int)SpecialResources.X2_HEADER);
                }
            }
        }
        protected void ParseTerezi()
        {
            if (!TereziParsed())
            {
                using (var parser = new Parser())
                {
                    parser.GetTerezi();
                    WriteResource(parser.GetResources(), (int)SpecialResources.TEREZI_PASSWORD);
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
            if (Parser.IsScratch(pageno))
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
        public Page WaitPage(int pageno, ForegroundWorker bgw)
        {
            try
            {
                if (!wl.TestAndSet())
                {
                    try
                    {
                        if (!archivedPages.IsPageArchived(pageno))
                        {
                            var s = new BGWSerializer(bgw);
                            var k = s.GetKey();
                            var ret = SavePage(pageno, new WrapBGW(s, k));
                            s.Commit(k);
                            if (!ret)
                                return null;
                        }
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
                else if (!archivedPages.IsPageArchived(pageno))
                {
                    archivedPages.Request(pageno);
                    do
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (archivedPages.Failed())
                            return null;
                    } while (!archivedPages.IsPageArchived(pageno));

                }
                return GetPage(pageno, Parser.Is2x(pageno));
            }
            catch
            {
                return null;
            }
        }

        void HandleCascade(WrapBGW bgw, int progress)
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
                cascadeSegments[0] = new Parser.Resource(Parser.DownloadFile("http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_loaderExt.swf"), "cascade_loaderExt.swf");
            }
            catch
            {
                cascadeSegments[0] = new Parser.Resource(Parser.DownloadFile("http://cdn.mspaintadventures.com/cascade/cascade_loaderExt.swf"), "cascade_loaderExt.swf");
            }
            var fileSize = cascadeSegments[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, cascadeSegments[0].originalFileName + ": " + fileSize / 1024 + "KB");
            for(int i = 1; i <= 5; ++i){
            
                
                try
                {
                    cascadeSegments[i] = new Parser.Resource(Parser.DownloadFile("http://uploads.ungrounded.net/userassets/3591000/3591093/cascade_segment" + i + ".swf"), "cascade_segment" + i + ".swf");
                }
                catch
                {
                    cascadeSegments[i] = new Parser.Resource(Parser.DownloadFile("http://cdn.mspaintadventures.com/cascade/cascade_segment" + i + ".swf"), "cascade_segment" + i + ".swf");
                }
                var fileSize2 = cascadeSegments[i].data.Count();
                totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
                if (bgw != null)
                    bgw.ReportProgress(progress, cascadeSegments[i].originalFileName + ": " + fileSize2 / 1024 + "KB");
            }

            cascadeSegments[6] = new Parser.Resource(Parser.DownloadFile("http://www.mspaintadventures.com/images/header_cascade.gif"), "header_cascade.gif");

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "[S] Cascade.";

            WritePageToDB(new Page((int)PagesOfImportance.CASCADE, asdf, cascadeSegments, next));
            
            if (bgw != null)
                bgw.ReportProgress(progress, "Cascade committed!");
        }
        void HandlePageSmash(WrapBGW bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing Caliborn's hissy fit.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(Parser.DownloadFile("http://cdn.mspaintadventures.com/007395/05492.swf"), "05492.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.CALIBORN_PAGE_SMASH + 1);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "[S] Cascade.";
            WritePageToDB(new Page((int)PagesOfImportance.CALIBORN_PAGE_SMASH,asdf,FUCKYOU,lnk));

        }
        void HandlePageSmash2(WrapBGW bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing Caliborn's hissy fit.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(Parser.DownloadFile("http://www.mspaintadventures.com/007680/05777_2.swf"), "05777_2.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.CALIBORN_PAGE_SMASH2 + 1);
            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "[S] Cascade.";
            WritePageToDB(new Page((int)PagesOfImportance.CALIBORN_PAGE_SMASH2, asdf, FUCKYOU, lnk));

        }
        void HandleDota(WrapBGW bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing Hussie's rekage.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(Parser.DownloadFile("http://cdn.mspaintadventures.com/DOTA/04812.swf"), "04812.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.DOTA + 1);
            WritePageToDB(new Page((int)PagesOfImportance.DOTA, asdf, FUCKYOU, lnk));
        }
        void FailToHandleVriska(WrapBGW bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing the huge 8itch.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(Parser.DownloadFile("http://www.mspaintadventures.com/shes8ack/07402.swf"), "07402.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.SHES8ACK + 1);
            WritePageToDB(new Page((int)PagesOfImportance.SHES8ACK, asdf, FUCKYOU, lnk));
        }
        void FailMiserably(WrapBGW bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing death.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(Parser.DownloadFile("http://cdn.mspaintadventures.com/storyfiles/hs2/GAMEOVER/06898.swf"), "06898.swf");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("", (int)PagesOfImportance.GAMEOVER + 1);
            WritePageToDB(new Page((int)PagesOfImportance.GAMEOVER, asdf, FUCKYOU, lnk));
        }
        void HandleOvershine(WrapBGW bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing zap.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(Parser.DownloadFile("http://cdn.mspaintadventures.com/storyfiles/hs2/07401.gif"), "07401.gif");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            asdf.title = "";
            Parser.Link[] lnk = new Parser.Link[1];
            lnk[0] = new Parser.Link("[S][A6A6I4] ====>", (int)PagesOfImportance.OVERSHINE + 1);
            WritePageToDB(new Page((int)PagesOfImportance.OVERSHINE, asdf, FUCKYOU, lnk));
        }
        void HandleJailbreakLast(WrapBGW bgw, int progress)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing your victory.");
            Parser.Resource[] FUCKYOU = new Parser.Resource[1];
            FUCKYOU[0] = new Parser.Resource(Parser.DownloadFile("http://cdn.mspaintadventures.com/storyfiles/jb2/YOUWIN.gif"), "YOUWIN.gif");

            var fileSize2 = FUCKYOU[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, FUCKYOU[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Text asdf = new Parser.Text();
            asdf.narr = new Parser.Text.ScriptLine("#000000", "Would you like to play again?", 0);
            asdf.title = "Enjoy restful slumber.";
            WritePageToDB(new Page((int)PagesOfImportance.JAILBREAK_LAST_PAGE, asdf, FUCKYOU, new Parser.Link[0]));
        }
        void HandleYoutubeVideo(WrapBGW bgw, int progress, bool AIsCollide)
        {
            if (bgw != null)
                bgw.ReportProgress(progress, "Now parsing " + (AIsCollide ? "physical catharsis." : "the end...") + " This will take a while...");


            var VidObject = YouTube.Default.GetVideo(AIsCollide ? "https://www.youtube.com/watch?v=Y5wYN6rB_Rg" : "https://www.youtube.com/watch?v=FevMNMwvdPw");

            Parser.Resource[] TheVid = new Parser.Resource[1];
            TheVid[0] = new Parser.Resource(VidObject.GetBytes(),  "video" + VidObject.FileExtension);

            var fileSize2 = TheVid[0].data.Count();
            totalMegabytesDownloaded += (float)fileSize2 / (1024.0f * 1024.0f);
            if (bgw != null)
                bgw.ReportProgress(progress, TheVid[0].originalFileName + ": " + fileSize2 / 1024 + "KB");

            Parser.Text asdf = new Parser.Text();
            Parser.Link[] lnk = new Parser.Link[1];
            asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
            if (AIsCollide)
            {
                asdf.title = "[S] Collide";
                lnk[0] = new Parser.Link("END OF ACT 6", (int)PagesOfImportance.COLLIDE + 1);
            }
            else
            {
                asdf.narr = new Parser.Text.ScriptLine("#000000", "", 0);
                asdf.title = "[S] ACT 7";
                lnk[0] = new Parser.Link("==>", (int)PagesOfImportance.ACT7 + 1);
            }
            WritePageToDB(new Page(AIsCollide ? (int)PagesOfImportance.COLLIDE : (int)PagesOfImportance.ACT7, asdf, TheVid, new Parser.Link[0]));
        }
        public static int ValidRange(int pg)
        {
            if (pg < (int)StoryBoundaries.JAILBREAK_PAGE_ONE)
                return (int)StoryBoundaries.JAILBREAK_PAGE_ONE;
            if (pg <= (int)StoryBoundaries.JAILBREAK_LAST_PAGE)
                return pg;
            if (pg < (int)StoryBoundaries.RQ)
                return (int)StoryBoundaries.RQ;
            if (pg <= (int)StoryBoundaries.EORQ)
                return pg;
            if (pg < (int)StoryBoundaries.BQ)
                return (int)StoryBoundaries.BQ;
            if (pg <= (int)StoryBoundaries.EOBQ)
                return pg;
            if (pg < (int)StoryBoundaries.PS)
                return (int)StoryBoundaries.PS;
            if (pg <= (int)StoryBoundaries.EOPS)
                return pg;
            if (pg < (int)StoryBoundaries.HSB)
                return (int)StoryBoundaries.HSB;
            if (pg <= (int)StoryBoundaries.EOHSB)
                return pg;
            if (pg < (int)StoryBoundaries.HOMESTUCK_PAGE_ONE)
                return (int)StoryBoundaries.HOMESTUCK_PAGE_ONE;

            return pg;
        }
        class ThreadedSaveParams
        {
            public bool Request { get; set; }
            public PageSavesManager Manager { get; set; }
            public int CurrentPage { get; set; }
            public int PagesToParse { get; set; }
            public UInt64 BGWKey { get; set; }
            public BGWSerializer BGW { get; set; }
        }
        void LaunchThreadedSave(object state)
        {
            ThreadedSaveParams stuff = (ThreadedSaveParams)state;
            if (stuff.Request || stuff.Manager.AddWorker())
            {
                if (!SavePage(stuff.CurrentPage, new WrapBGW(stuff.BGW, stuff.BGWKey), stuff.Manager.CurrentProgress))
                {
                    stuff.BGW.ReportProgress(stuff.BGWKey, stuff.Manager.CurrentProgress, "Failed to parse page " + stuff.CurrentPage + "! Added to miss queue.");
                    stuff.BGW.Commit(stuff.BGWKey);
                    if (!stuff.Request)
                        stuff.Manager.RemoveWorker(stuff.CurrentPage);
                }
                else if (!stuff.Request)
                {
                    stuff.BGW.Commit(stuff.BGWKey);
                    stuff.Manager.RemoveWorker(0);
                }
            }

            stuff.Manager.CurrentProgress = (int)(((float)(stuff.Manager.PagesParsed) / (float)(stuff.PagesToParse)) * 100.0f);
        }
        public void ResumeWork(ForegroundWorker bgw2, int startPage, int lastPage)
        {

            var bgw = new BGWSerializer(bgw2);

            while (wl.TestAndSet()) { System.Threading.Thread.Sleep(1000); }
            try
            {
                bool missedRound = false;
                startPage = ValidRange(startPage);
                int pagesToParse = lastPage - startPage + 1;
                int currentPage = archivedPages.FindLowestPage(startPage, lastPage);;
                var manager = new Writer.PageSavesManager(archivedPages.GetParseCount(startPage, lastPage), bgw2);
                if (currentPage > lastPage)
                {
                    bgw2.ReportProgress(100, "Range already archived!");
                    return;
                }

                manager.CurrentProgress = (int)(((float)(manager.PagesParsed) / (float)(pagesToParse)) * 100.0f);
                if (!bgw2.CancellationPending)
                {
                    bgw2.ReportProgress(manager.CurrentProgress, "Starting archive operation at page " + startPage);
                }
                else
                {
                    bgw2.ReportProgress(manager.CurrentProgress, "Operation cancelled.");
                    return;
                }
                while (true)
                {

                    if (missedRound)
                    {
                        currentPage = manager.MissedPagesTop();
                        manager.CurrentProgress = (int)(((float)(manager.PagesParsed) / (float)(pagesToParse)) * 100.0f);
                    }

                    if (!bgw2.CancellationPending)
                    {
                        if (currentPage != startPage)
                            bgw2.ReportProgress(manager.CurrentProgress, "Resuming from " + currentPage);
                    }
                    else
                    {
                        bgw2.ReportProgress(manager.CurrentProgress, "Operation cancelled.");
                        return;
                    }

                    while (true)
                    {
                        UInt64 k = bgw.GetKey();
                        manager.ExpectWorker();
                        System.Threading.ThreadPool.QueueUserWorkItem(LaunchThreadedSave, new ThreadedSaveParams { Request = false, BGW = bgw, BGWKey = k, CurrentPage = currentPage, Manager = manager, PagesToParse = pagesToParse });
                        
                        if (missedRound)
                        {
                            if (manager.MissedPagesCount() == 0)
                                break;
                            currentPage = manager.MissedPagesPop();
                        }
                        else
                        {
                            if (currentPage >= lastPage)
                                break;
                            currentPage = ValidRange(archivedPages.FindLowestPage(currentPage + 1, lastPage));
                        }


                    }

                    while (!manager.WaitForWorkers() && !bgw2.CancellationPending)
                    {
                        Thread.Sleep(1);    //since we run alongside the threadpool we have to be quick about things
                        var req = (archivedPages.GetRequest());
                        if (req != 0)
                        {
                            UInt64 k = bgw.GetKey();
                            bgw.ReportProgress(k, manager.CurrentProgress, "Responding to user request for page " + req);
                            LaunchThreadedSave(new ThreadedSaveParams { Request = true, BGW = bgw, BGWKey = k, CurrentPage = req, Manager = manager, PagesToParse = pagesToParse });
                            if(!archivedPages.IsPageArchived(req))
                            archivedPages.FailFlag();
                        }
                    }
                    while (!manager.WaitForWorkers()) { Thread.Sleep(1); }
                    if (bgw2.CancellationPending || manager.MissedPagesCount() == 0)
                        break;
                    missedRound = true;
                    bgw2.ReportProgress(manager.CurrentProgress, "Missed " + manager.MissedPagesCount() + " pages. Iterating through missed queue.");
                }
                if (bgw2.CancellationPending)
                    bgw2.ReportProgress(manager.CurrentProgress, "Operation cancelled.");
                else
                    bgw2.ReportProgress(100, "Operation completed.");
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
        bool SavePage(int currentPage, WrapBGW bgw = null, int currentProgress = 0)
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
                        case PagesOfImportance.COLLIDE:
                        case PagesOfImportance.ACT7:
                            HandleYoutubeVideo(bgw, currentProgress,(PagesOfImportance)currentPage == PagesOfImportance.COLLIDE);
                            break;
                    }
                    archivedPages.Add(currentPage);
                }
                catch
                {
                    if (bgw != null)
                        bgw.ReportProgress(currentProgress, "Error parsing special page " + currentPage);
                    return false;
                }
                finally
                {
                    if (bgw != null)
                        bgw.ReportProgress(currentProgress, "");
                }
                return true;
            }
            if (archivedPages.IsPageArchived(currentPage))
                return true;

            if (Parser.IsTrickster(currentPage))
                ParseTrickster();
            if (Parser.Is2x(currentPage))
                Parsex2Header();
            if (Enum.IsDefined(typeof(PasswordPages), currentPage + 1))
                ParseTerezi();

            if(bgw != null && Parser.IsOpenBound(currentPage))
                bgw.ReportProgress(currentProgress, "Parsing an Openbound page. There are tons of tiny downloads and this will take a couple minutes...");

            using (var parser = new Parser())
            {
                if (parser.LoadPage(currentPage))
                {
                    int missedPages = 0;
                    if (!parser.x2Flag)
                    {
                        if (!WritePage(bgw, currentPage, currentProgress, 0, parser))
                            missedPages++;
                    }
                    else
                    {
                        if (!WritePage(bgw, currentPage, currentProgress, 1, parser))
                            missedPages += 2;
                        else
                        {
                            parser.Reparse();
                            if (!WritePage(bgw, currentPage, currentProgress, 2, parser))
                                missedPages += 2;
                        }
                    }
                    //simple enough, leave it to the reader to decode the multiple pages
                    return missedPages == 0;
                }
                else
                    return false;
            }
            
        }
        bool WritePage(WrapBGW bgw, int currentPage, int currentProgress, int x2phase, Parser parser)
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

                if (bgw != null && x2phase != 1)
                    bgw.ReportProgress(currentProgress, "");

                var p = new Page(currentPage, text, res, links);
                p.x2 = x2phase == 2;
                WritePageToDB(p);
                if (x2phase != 1)
                {
                    archivedPages.Add(currentPage);
                }

            }
            catch
            {
                Debugger.Break();
                if (bgw != null)
                    bgw.ReportProgress(currentProgress, "Error in archiving page: " + currentPage);
                return false;

            }
            return true;
        }
    }
}
