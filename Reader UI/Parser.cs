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
    public class Parser
    {
        
        public class Resource
        {
            readonly public byte[] data;
            readonly public string originalFileName, titleText;
            public Resource(byte[] idata, string ioFN, string tt = null)
            {
                data = idata;
                originalFileName = ioFN;
                titleText = tt;
            }
        }
        public class Text
        {

            public class ScriptLine
            {
                public class SpecialSubText
                {
                    public readonly int begin, length;
                    public readonly bool underlined;
                    public readonly string colour;
                    public SpecialSubText(int beg, int len, bool under, string col)
                    {
                        begin = beg;
                        length = len;
                        underlined = under;
                        colour = col;
                    }
                }
                public readonly bool isImg;
                public readonly string hexColour;
                public readonly string text;
                public SpecialSubText[] subTexts = null;
                public ScriptLine(string hx, string tx)
                {
                    hexColour = hx;
                    text = tx;
                    isImg = false;
                }
                public ScriptLine(string resName)
                {
                    isImg = true;
                    hexColour = null;
                    text = resName;
                }
            }

            public string title = null;
            public ScriptLine narr = null;
            public string promptType = null;
            public ScriptLine[] lines = null;
            public string linkPrefix = null;
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
        const string prepend = "http://www.mspaintadventures.com/?s=6&p=";
        const string gifRegex = @"http:\/\/(?!" + 
            @".*v2_blankstrip"  //stuff to ignore
            + @"|.*v2_blanksquare2"
            + @"|.*v2_blanksquare3"
            + @"|.*spacer"
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

        WebClient web = new WebClient();
        HttpClient client = new HttpClient();

        HtmlNode contentTable;

        List<Resource> resources = new List<Resource>();
        List<Link> links = new List<Link>();
        Text texts;
        List<HtmlNode> linkListForTextParse = new List<HtmlNode>();

        public int GetLatestPage()
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
        public Text GetText()
        {
            return texts;
        }
        void CheckLineForSpecialSubText(HtmlNode currentLine, Text.ScriptLine scriptLine)
        {
            var lineSpecialSubtext = currentLine.SelectNodes("span");
            if (lineSpecialSubtext == null)
                return;

            //special subtext alert
            Text.ScriptLine.SpecialSubText[] sTs = new Text.ScriptLine.SpecialSubText[lineSpecialSubtext.Count()];
            for (int j = 0; j < lineSpecialSubtext.Count(); ++j)
            {
                var currentSpecialSubtext = lineSpecialSubtext.ElementAt(j);
                bool underlined = Regex.Match(currentSpecialSubtext.OuterHtml, underlineRegex).Success;
                var colourReg = Regex.Match(currentSpecialSubtext.OuterHtml, hexColourRegex);
                string colour = colourReg.Success ? colourReg.Value : scriptLine.hexColour;
                int begin = currentLine.InnerHtml.IndexOf(currentSpecialSubtext.OuterHtml);
                int length = currentSpecialSubtext.InnerText.Length;
                sTs[j] = new Text.ScriptLine.SpecialSubText(begin, length, underlined, colour);
            }
            scriptLine.subTexts = sTs;
            
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
                    var conversationLines = logBox.SelectNodes("span|img");   //this will grab lines 

                    if (conversationLines != null)
                    {
                        List<Text.ScriptLine> line = new List<Text.ScriptLine>();
                        int i = 0;
                        
                        var logMessages = Regex.Matches(logBox.InnerHtml,pesterLogRegex);
                        //the hard part is finding out where 


                        //now for each line we need the colour and the text
                        for (; i < conversationLines.Count(); ++i)
                        {
                            var currentLine = conversationLines.ElementAt(i);

                            if (currentLine.Name == "img")
                            {
                                //just add the image
                                var pathReg = Regex.Match(currentLine.OuterHtml,gifRegex);
                                var gifReg = Regex.Match(pathReg.Value, scratchHeaderImageFilenameRegex);
                                line.Add(new Text.ScriptLine(gifReg.Groups[1].Value));
                                continue;
                            }

                            var hexReg = Regex.Match(currentLine.OuterHtml, hexColourRegex);

                            var scriptLine = new Text.ScriptLine(hexReg.Success ? hexReg.Value : "#000000", currentLine.InnerText);

                            CheckLineForSpecialSubText(currentLine, scriptLine);

                            convParent.Remove();

                            line.Add(scriptLine);
                        }
                        texts.lines = line.ToArray();
                    }
                    else
                    {
                        //Assume text in a box
                        texts.lines = new Text.ScriptLine[1];
                        var hexReg = Regex.Match(logBox.OuterHtml, hexColourRegex);
                        texts.lines[0] = new Text.ScriptLine(hexReg.Success ? hexReg.Value : "#000000", logBox.InnerText.Trim());
                        CheckLineForSpecialSubText(logBox, texts.lines[0]);

                    }
                }
                else
                {
                    //check for narrative
                    //narrative
                    //I seriously don't know if this is reliable but narrative seems to come on the second p if it exists

                    //TODO: Support different fonts
                    var narrative = contentTable.Descendants("p").ElementAt(1);
                    if (narrative != null)
                    {
                        var hexReg = Regex.Match(narrative.OuterHtml, hexColourRegex);
                        Text.ScriptLine narr = new Text.ScriptLine(hexReg.Success ? hexReg.Value : "#000000", narrative.InnerText.Trim());
                        CheckLineForSpecialSubText(narrative, narr);
                        texts.narr = narr;
                    }

                    
                }
            

            {//link prefix
                //we find the link to the next page delete it from the contentTable and check the innertext of it's parent
                var link = linkListForTextParse.Last();
                var parent = link.ParentNode;
                link.Remove();
                texts.linkPrefix = parent.InnerText;
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
                resources.Add(new Resource(web.DownloadData(matches[i].Value), System.IO.Path.GetFileName(new Uri(matches[i].Value).LocalPath)));
            }

            matches = Regex.Matches(contentTable.InnerHtml, swfRegex);

            for (int i = 0; i < matches.Count; i++)
            {
                resources.Add(new Resource(web.DownloadData(matches[i].Captures[0].Value), System.IO.Path.GetFileName(new Uri(matches[i].Captures[0].Value).LocalPath)));
            }

            resources = resources.Distinct().ToList();  //filter out any double grabs
        }
        public Resource[] GetResources()
        {
            
            return resources.ToArray();
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
                var res = Regex.Match(actualLink.Trim(), linkNumberRegex);
                if (res.Success)
                {
                    links.Add(new Link(link.InnerText, Convert.ToInt32(res.Value)));
                    linkListForTextParse.Add(link);
                }
            }
        }
        public Link[] GetLinks()
        {
            return links.ToArray();
        }
        public byte[] DownloadFile(string file)
        {
            try
            {
                return web.DownloadData(file);
            }
            catch
            {
                //try the www if the cdn is jank
                return web.DownloadData(file.Replace("cdn.mspaintadventures.com", "www.mspaintadventures.com"));
            }
        }
        bool IsScratch(int page)
        {
            return page >= 5664 && page <= 5981;
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
        bool IsSBAHJ(int pageno)
        {
            return pageno == 5982;
        }
        public bool LoadPage(int pageno)
        {
            try
            {
                var response = client.GetByteArrayAsync(new Uri(prepend + pageno.ToString("D6"))).Result;
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                var html = new HtmlDocument();
                html.LoadHtml(source);
                //TODO: Support for 2x pages

                if (IsScratch(pageno))
                {
                    ScratchPreParse(html);
                    contentTable = html.DocumentNode.Descendants("body").First().Descendants("table").First().Descendants("table").ElementAt(1).Descendants("table").First();
                }
                else if (IsSBAHJ(pageno))
                {
                    contentTable = html.DocumentNode.Descendants("table").First().Descendants("table").ElementAt(1).Descendants("table").First();
                }
                else
                {
                    //regular, homosuck, or trickster
                    contentTable = html.DocumentNode.Descendants("table").First().SelectNodes("tr").ElementAt(1).SelectNodes("td").First().SelectNodes("table").First();
                }
                ParseResources(!IsScratch(pageno));
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
