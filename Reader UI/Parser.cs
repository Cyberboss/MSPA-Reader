using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Reader_UI
{
    public class Parser : IDisposable
    {
        
        public class Resource
        {
            readonly public byte[] data;
            readonly public string originalFileName, titleText;
            public bool isInPesterLog = false;
            public Resource(byte[] idata, string ioFN, string tt = null)
            {
                data = idata;
                originalFileName = ioFN;
                titleText = tt;
            }
        }
        void Dispose(bool mgd)
        {
            web.Dispose();
            client.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~Parser()
        {
            Dispose(false);
        }
        public class Text
        {

            public class ScriptLine
            {
                public class SpecialSubText
                {
                    public readonly int begin, length;
                    public readonly bool isImg;
                    public readonly bool underlined;
                    public readonly string colour;
                    public SpecialSubText(int beg, int len, bool under, string col)
                    {
                        isImg = false;
                        begin = beg;
                        length = len;
                        underlined = under;
                        colour = col;
                    }
                    public SpecialSubText(int beg, int len, string imageName)
                    {
                        isImg = true;
                        begin = beg;
                        length = len;
                        underlined = false;
                        colour = imageName;
                    }
                    public SpecialSubText(int beg, int len, bool under, string col, bool i)
                    {

                        isImg = i;
                        begin = beg;
                        length = len;
                        underlined = under;
                        colour = col;
                    }
                }
                public readonly bool isImg;
                public string hexColour;
                public readonly string text;
                public SpecialSubText[] subTexts = null;
                public readonly int precedingLineBreaks;
                public ScriptLine(string hx, string tx, int prb)
                {
                    hexColour = hx;
                    precedingLineBreaks = prb;
                    text = tx;
                    isImg = false;
                }
                public ScriptLine(string resName, int prb)
                {
                    isImg = true;
                    hexColour = null;
                    text = resName;
                    precedingLineBreaks = prb;
                }
            }

            public string title = null;
            public ScriptLine narr = null;
            public string promptType = null;
            public ScriptLine[] lines = null;
            public string altText = null;
        }
        public void LoadIcons()
        {
            resources.Clear();
            resources.Add(new Resource(DownloadFile("http://cdn.mspaintadventures.com/images/candycorn.gif",true), "candycorn.gif"));
            resources.Add(new Resource(DownloadFile("http://cdn.mspaintadventures.com/images/candycorn_scratch.png", true), "candycorn_scratch.gif"));
            resources.Add(new Resource(DownloadFile("http://cdn.mspaintadventures.com/images/a6a6_tooth2.gif", true), "a6a6_tooth2.gif"));
        }
        public class Link
        {
            readonly public string originalText;
            readonly public int pageNumber;
            public Link(string oT, int pN)
            {
                originalText = oT;
                pageNumber = pN;
            }
        }
        //http://stackoverflow.com/questions/1585985/how-to-use-the-webclient-downloaddataasync-method-in-this-context
        class WebDownload : WebClient
        {
            /// <summary>
            /// Time in milliseconds
            /// </summary>

            bool isDownloading;
            byte[] res;
            object _sync = new object(); 

            public int Timeout { get; set; }

            public WebDownload() : this(20000) { }

            public WebDownload(int timeout)
            {
                this.Timeout = timeout;
                DownloadDataCompleted += WebDownload_DownloadDataCompleted;
            }

            void WebDownload_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
            {
                if (e.Cancelled)
                    return;
                
                lock (_sync)
                {
                    isDownloading = false;
                    if(e.Error == null)
                        res = e.Result;
                }
            }

            public byte[] DownloadData(string address, bool serial) 
            {
                res = null;
                isDownloading = true;
                DownloadDataAsync(new Uri(address));
                int count = 0;

                while (true)
                {
                    System.Threading.Thread.Sleep(10);
                    if (serial)
                        System.Windows.Forms.Application.DoEvents();
                    count+= 10;
                    lock (_sync)
                    {
                        if (!isDownloading)
                            if (res != null)
                                break;
                            else
                                throw new Exception("Download failed");
                        
                    }
                    if (count > Timeout)
                    {
                        CancelAsync();
                        throw new Exception("Download timed out");
                    }
                }
                return res;
            }
        }
        const string prepend = "http://www.mspaintadventures.com/?s=6&p=";
        const string gifRegex = @"http:\/\/(?!" + 
            @".*v2_blankstrip"  //stuff to ignore
            + @"|.*v2_blanksquare2"
            + @"|.*v2_blanksquare3"
            + @"|.*spacer"
            //the trickster comic bg
            + @"|.*bluetile"
            + @")(.*)\.gif";
        const string scratchHeaderImageRegex = "src=\\\"(.*?\\.gif)\\\"";
        const string scratchHeaderImageFilenameRegex = @".*\/(.*)";
        const string scratchTitleRegex = "title=\\\"(.*?)\\\"";
        const string swfRegex = @"http:\/\/.*?\.swf";
        const string linkNumberRegex = @"[0-9]{6}";
        const string logRegex = @"Dialoglog|Spritelog|Pesterlog";
        const string hexColourRegex = @"#[0-9A-Fa-f]{6}";
        const string underlineRegex = @"underline";
        const string pesterLogRegex = @"-- .*? --";
        //TODO: the above regex is way too vague
        const string chumhandleRegex = @"\[[G|C|A|T]{2}\]|\[EB\]";
        const string gifFileRegex = @".+\.gif";
        

        public bool x2Flag;

        WebDownload web = new WebDownload();
        HttpClient client = new HttpClient();

        HtmlNode contentTable,secondContentTable;

        List<Resource> resources = new List<Resource>();
        List<Link> links = new List<Link>();
        Text texts;
        List<HtmlNode> linkListForTextParse = new List<HtmlNode>();

        public static bool IsGif(string file)
        {
            return Regex.Match(file, gifFileRegex).Success;
        }
        public int GetLatestPage()
        {
            //if this fails we need to check the database
            try
            {
                var response = client.GetByteArrayAsync(new Uri("http://www.mspaintadventures.com/?viewlog=6")).Result;
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                var html = new HtmlDocument();
                html.LoadHtml(source);

                //look for view oldest to newest key phrase
                //magic from http://stackoverflow.com/questions/8948895/using-xpath-and-htmlagilitypack-to-find-all-elements-with-innertext-containing-a
                var labelHref = html.DocumentNode.SelectNodes("//*[text()[contains(., 'View oldest to newest')]]").First();
                var firstEntry = labelHref.ParentNode.ParentNode.SelectNodes("a").First();
                var linkText = firstEntry.Attributes["href"].Value;

                string pageNumberAsString = Regex.Match(linkText, linkNumberRegex).Value;

                return Convert.ToInt32(pageNumberAsString);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error retrieving lastest MSPA page. Range locked to currently archived pages");
                return 0;
            }
        }
        public void LoadTricksterResources(bool serial)
        {
            resources.Clear();
            resources.Add(new Resource(DownloadFile("http://cdn.mspaintadventures.com/images/trickster_sitegraphics/Z2.gif", serial), "Z2.gif"));
            resources.Add(new Resource(DownloadFile("http://cdn.mspaintadventures.com/images/trickster_sitegraphics/menu.swf", serial), "menu.swf"));
            resources.Add(new Resource(DownloadFile("http://mspaintadventures.com/images/trickster_sitegraphics/bluetile.gif", serial), "bluetile.gif"));
        }
        public Text GetText()
        {
            return texts;
        }
        void CheckLineForSpecialSubText(HtmlNode currentLine, Text.ScriptLine scriptLine)
        {
            var lineSpecialSubtext = currentLine.SelectNodes("span");
            var lineImages = currentLine.SelectNodes("img");
            if (lineImages == null && lineSpecialSubtext == null)
                return;

            List<Text.ScriptLine.SpecialSubText> sTs = new List<Text.ScriptLine.SpecialSubText>();
            if (lineSpecialSubtext != null)
            {
                //special subtext alert
                for (int j = 0; j < lineSpecialSubtext.Count(); ++j)
                {
                    var currentSpecialSubtext = lineSpecialSubtext.ElementAt(j);
                    bool underlined = Regex.Match(currentSpecialSubtext.OuterHtml, underlineRegex).Success;
                    var colourReg = Regex.Match(currentSpecialSubtext.OuterHtml, hexColourRegex);
                    string colour = colourReg.Success ? colourReg.Value : scriptLine.hexColour;
                    int begin = currentLine.InnerHtml.IndexOf(currentSpecialSubtext.OuterHtml);
                    int length = currentSpecialSubtext.InnerText.Length;
                    sTs.Add(new Text.ScriptLine.SpecialSubText(begin, length, underlined, colour));
                }
            }

            if (lineImages != null)
            {
                for (int j = 0; j < lineImages.Count(); ++j)
                {
                    var currentSpecialSubtext = lineImages.ElementAt(j);
                    var reg = Regex.Match(currentSpecialSubtext.OuterHtml, gifRegex);
                    string img = System.IO.Path.GetFileName(new Uri(reg.Value).LocalPath);
                    int begin = currentLine.InnerHtml.IndexOf(currentSpecialSubtext.OuterHtml);
                    int length = currentSpecialSubtext.InnerText.Length;
                    resources.Find(x => x.originalFileName == img).isInPesterLog = true;
                    sTs.Add(new Text.ScriptLine.SpecialSubText(begin, length, img));
                }
            }
            scriptLine.subTexts = sTs.ToArray();

        }
        void ParseText()
        {
            //most difficult part here
            //all text in homestuck is pure html formatting
            //so the styles are all over the place
            texts = new Text();
            {//title
                //easy enough, its the very first p in the content table
                //just clean it up a bit
                texts.title = contentTable.Descendants("p").First().InnerText.Trim();
            }
            
            /*
             * There are cases where there can be narritive and script on one page so we need something to independantly check if the narrative exists
             * ...
             * which is damn near impossible
             * 
             * 
             * try handling the script first then remove it from it's parent node to clean the doc somewhat
             * 
             * NEVER MIND ALL THAT THE ONE EDGE CASE I THOUGHT OF WAS PART OF THE GIF
             */


               //script
                //check if page HAS a dialoglog , find it and get the lines within
                var reg = Regex.Match(contentTable.InnerText, logRegex);
                if (reg.Success)
                {
                    texts.promptType = reg.Value;
                    var convParent = contentTable.SelectSingleNode(".//*[text()[contains(., '" + reg.Value + "')]]").ParentNode.ParentNode;
                    var logBox = convParent.SelectSingleNode(".//p");
                    var conversationLines = logBox.SelectNodes("span|img|br");   //this will grab lines 

                    if (conversationLines != null)
                    {
                        List<Text.ScriptLine> line = new List<Text.ScriptLine>();
                        List<Text.ScriptLine> logs = new List<Text.ScriptLine>();
                        
                        //conversation lines go in order, no stopping them.
                        //What we can do is insert them where they belong.
                        // by doing a once over once we've done everything
                        //inserting them appropriately

                        var logMessages = Regex.Matches(logBox.InnerText.Trim(), pesterLogRegex);
                        var logMessagesHtml = Regex.Matches(logBox.InnerHtml.Trim(), pesterLogRegex);
                        //the hard part is finding out where these go
                        if(logMessages != null)
                            for (int i = 0; i < logMessages.Count; i++ )
                            {
                                //TODO: figure out where these actually go
                                var tmp = new Text.ScriptLine("#000000", logMessages[i].Value,1);       //Assume 1 line break before a log msg

                                ///OH WE CAN DO THIS TO GET THE NODES!!
                                HtmlDocument tempDoc = new HtmlDocument();
                                tempDoc.LoadHtml(logMessagesHtml[i].Value);
                                CheckLineForSpecialSubText(tempDoc.DocumentNode, tmp);

                                logs.Add(tmp);
                            }

                        
                        int j = 0;
                        for (; j < conversationLines.Count() && conversationLines.ElementAt(j).Name == "br"; j++) { }
                        int precedingLineBreaks = j;
                        //now for each line we need the colour and the text
                        while (j < conversationLines.Count())
                        {
                            var currentLine = conversationLines.ElementAt(j);
                            Text.ScriptLine scriptLine;
                            if (currentLine.Name == "img")
                            {
                                //just add the image
                                var pathReg = Regex.Match(currentLine.OuterHtml,gifRegex);
                                var gifReg = Regex.Match(pathReg.Value, scratchHeaderImageFilenameRegex);
                                scriptLine = new Text.ScriptLine(gifReg.Groups[1].Value,precedingLineBreaks);

                                //find the resource that matches this image and mark it as pesterlogged
                                resources.Find(x => x.originalFileName == scriptLine.text).isInPesterLog = true;
                                j++;

                                continue;
                            }else{
                                //there is no way
                                if (Regex.Match(currentLine.InnerText, chumhandleRegex).Success)
                                {
                                    ++j;
                                    continue;
                                }

                                var hexReg = Regex.Match(currentLine.OuterHtml, hexColourRegex);

                                scriptLine = new Text.ScriptLine(hexReg.Success ? hexReg.Value : "#000000", currentLine.InnerText,precedingLineBreaks);

                                CheckLineForSpecialSubText(currentLine, scriptLine);
                            }

                            line.Add(scriptLine);
                            //increment i to find the breaks;
                            int jBegin = j + 1;
                            do
                            {
                                j++;
                            } while (j < conversationLines.Count() && conversationLines.ElementAt(j).Name == "br");

                            precedingLineBreaks = j - jBegin;

                        }

                        //now look through the lines adding whats expected
                        int linePositionCount = 0;
                        int logPositionCount = 0;

                        for (int i = 0; i < logBox.ChildNodes.Count && logPositionCount < logs.Count; ++i)
                        {
                            var currentNode = logBox.ChildNodes.ElementAt(i);
                            if (currentNode.Name == "span")
                            {
                                if (currentNode.InnerText == line[linePositionCount].text)
                                {
                                    linePositionCount++;
                                }
                            }
                            else if (currentNode.Name == "img")
                            {
                                linePositionCount++;
                            }
                            else if(logs[logPositionCount].text.Contains(currentNode.InnerText.Trim()) && currentNode.InnerText.Trim() != "")
                            {
                                //at this point we need to keep incrementing i until we stop matching this one
                                //we can expect EXACTLY this many matches
                                i += 1 + logs[logPositionCount].subTexts.Count() * 2;
                                line.Insert(linePositionCount, logs[logPositionCount]);
                                logPositionCount++;
                                linePositionCount++;
                            }
                        }

                        texts.lines = line.ToArray();
                    }
                    else
                    {
                        //Assume simple text in a box
                        texts.lines = new Text.ScriptLine[1];
                        var hexReg = Regex.Match(logBox.OuterHtml, hexColourRegex);
                        texts.lines[0] = new Text.ScriptLine(hexReg.Success ? hexReg.Value : "#000000", logBox.InnerText.Trim(),0);
                        CheckLineForSpecialSubText(logBox, texts.lines[0]);

                    }
                }
                else
                {
                    //check for narrative
                    //narrative
                    //I seriously don't know if this is reliable but narrative seems to come on the second p if it exists

                    //TODO: Support different fonts
                    try
                    {
                        var decs = contentTable.Descendants("p");
                        var narrative = decs.ElementAt(1);
                        if (narrative != null)
                        {
                            var hexReg = Regex.Match(narrative.OuterHtml, hexColourRegex);
                            Text.ScriptLine narr = new Text.ScriptLine(hexReg.Success ? hexReg.Value : "#000000", narrative.InnerText.Trim(),0);
                            CheckLineForSpecialSubText(narrative, narr);
                            texts.narr = narr;
                        }
                    }
                    catch
                    {
                        texts.narr = new Text.ScriptLine("#000000","",0);
                    }
                    
                }
            

        }
        void ParseResources(bool clear)
        {
            if(clear)
                resources.Clear();
            //we are mainly looking for .gifs and .swfs, there are some things we should ignore, such as /images/v2_blankstrip.gif
            var matches = Regex.Matches(contentTable.InnerHtml, gifRegex);

            for (int i = 0; i < matches.Count; i++)
            {
                resources.Add(new Resource(DownloadFile(matches[i].Value), System.IO.Path.GetFileName(new Uri(matches[i].Value).LocalPath)));
            }

            matches = Regex.Matches(contentTable.InnerHtml, swfRegex);
            List<string> matchNames = new List<string>();
            for (int i = 0; i < matches.Count; i++)
            {
                matchNames.Add(matches[i].Captures[0].Value);//filter out any double grabs
            }
            matchNames = matchNames.Distinct().ToList();

            for (int i = 0; i < matchNames.Count; i++) 
                resources.Add(new Resource(DownloadFile(matchNames[i]), System.IO.Path.GetFileName(new Uri(matchNames[i]).LocalPath)));

            resources = resources.Distinct().ToList();  
        }
        public Resource[] GetResources()
        {
            
            return resources.ToArray();
        }
        public void Reparse()
        {
            if (!x2Flag)
                throw new Exception();
            contentTable = secondContentTable;
            ParseResources(true);
            ParseLinks();
            ParseText();
        }
        public static bool IsTrickster(int pageno)
        {
            return (pageno >= 7614 && pageno <= 7677);
        }
        void ParseLinks()
        {
            links.Clear();
            linkListForTextParse.Clear();
            foreach (HtmlNode link in contentTable.Descendants().Where(z => z.Attributes.Contains("href")))
            {
                string actualLink = link.Attributes["href"].Value;
                //we want to ignore the everypage navigation links
                if (link.InnerText == "Go Back"
                    || link.InnerText == "Start Over"
                    || link.InnerText == "Save Game"
                    || link.InnerText == "(?)"
                    || link.InnerText == "Auto-Save!"
                    || link.InnerText == "Delete Game Data"
                    || link.InnerText == "Load Game")
                    continue;
                var res = Regex.Match(actualLink, linkNumberRegex);
                if (res.Success)
                {
                    links.Add(new Link(link.InnerText.Trim(), Convert.ToInt32(res.Value)));
                    linkListForTextParse.Add(link);
                }
            }
        }
        public static int GetPageNumberFromURL(string url)
        {
            var reg = Regex.Match(url, linkNumberRegex);
            if (!reg.Success)
            {
                Debugger.Break();
                return 0;
            }
            return Convert.ToInt32(reg.Value);
        }
        public static bool IsHomosuck(int pageno)
        {
            //ty based wiki http://mspaintadventures.wikia.com/wiki/Homestuck:_Act_6_Act_6
            return ((pageno >= 8143 && pageno <= 8177)
                || (pageno >= 8375 && pageno <= 8430)
                || (pageno >= 8753 && pageno <= 8800)   //8801 is GAMEOVER
                || (pageno >= 8821 && pageno <= 8843)
                || (pageno >= 9309 && pageno <= 9348));
        }
        public Link[] GetLinks()
        {
            return links.ToArray();
        }
        public byte[] DownloadFile(string file, bool serial = false)
        {
            try
            {
                return web.DownloadData(file, serial);
            }
            catch
            {
                //try the www if the cdn is jank
                return web.DownloadData(file.Replace("cdn.mspaintadventures.com", "www.mspaintadventures.com"), serial);
            }
        }
        void ScratchPreParse(HtmlDocument html)
        {
            //grab the header from the top of the page
            resources.Clear();
            var node = html.DocumentNode.Descendants("img").First();
            string innerHtml = node.OuterHtml;
            var match = Regex.Match(innerHtml, scratchHeaderImageRegex);

            string actualFilePath = "http://cdn.mspaintadventures.com/" + match.Groups[1].Value;
            byte[] data = DownloadFile(actualFilePath);
            string oFN = Regex.Match(actualFilePath, scratchHeaderImageFilenameRegex).Groups[1].Value;
            string title = Regex.Match(innerHtml, scratchTitleRegex).Groups[1].Value;
            resources.Add(new Resource(data, oFN, title));


            resources = resources.Distinct().ToList();  //filter out any double grabs
        }
        void ScratchPostParse(HtmlDocument html, int pageno)
        {
            //maunally add the special LE text
            if (pageno >= 5976 && pageno <=5981)
            {
                const string LESecretTextfilepref = "http://cdn.mspaintadventures.com/storyfiles/hs2/scraps/";
                string file = "LEtext"+ (pageno - 5975) +".gif";
                resources.Add(new Resource(DownloadFile(LESecretTextfilepref + file), file));
            }

            //grab the alt text
            var node = html.DocumentNode.Descendants("img").First();

            try
            {
                texts.altText = node.Attributes["title"].Value;
            }
            catch { }

        }
        public bool IsScratch(int page)
        {
            return page >= 5664 && page <= 5981;
        }
        public bool Is2x(int page)
        {
            return page >= 7688 && page <= 7825;
        }
        bool IsSBAHJ(int pageno)
        {
            return pageno == 5982;
        }
        public bool LoadPage(int pageno)
        {
            try
            {
                x2Flag = false;
                var response = client.GetByteArrayAsync(new Uri(prepend + pageno.ToString("D6"))).Result;
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                var html = new HtmlDocument();
                html.LoadHtml(source);

                if (IsScratch(pageno))
                {
                    ScratchPreParse(html);
                    contentTable = html.DocumentNode.Descendants("body").First().Descendants("table").First().Descendants("table").ElementAt(1).Descendants("table").First();
                    ParseResources(false);
                    ParseLinks();
                    ParseText();
                    ScratchPostParse(html,pageno);
                    return true;
                }
                else if (IsSBAHJ(pageno))
                {
                    contentTable = html.DocumentNode.Descendants("table").First().Descendants("table").ElementAt(1).Descendants("table").First();
                }
                else if (Is2x(pageno))
                {
                    x2Flag = true;
                    //i think he looks like a bitch

                    //same place in the html as regulars thankfully but that's about as easy as its's going to be from here
                    contentTable = html.DocumentNode.SelectSingleNode("//comment()[contains(., 'COMIC ONE')]").ParentNode.SelectSingleNode("table");
                    secondContentTable = html.DocumentNode.SelectSingleNode("//comment()[contains(., 'COMIC TWO')]").ParentNode.SelectSingleNode("table");

                    //essentially it's two pages of comics right next to each other. Simple enough for the parser. Fucking nightmare for the db and reader

                }
                else
                {
                    //regular, homosuck, or trickster
                    contentTable = html.DocumentNode.Descendants("table").First().SelectNodes("tr").ElementAt(1).SelectNodes("td").First().SelectNodes("table").First();
                }
                ParseResources(true);
                ParseLinks();
                ParseText();
            }
            catch
            {
                return false;
            }
            return true;
         }
    }
}
