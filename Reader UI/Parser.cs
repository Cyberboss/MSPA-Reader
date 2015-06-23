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
            readonly public string originalFileName;
            public Resource(byte[] idata, string ioFN)
            {
                data = idata;
                originalFileName = ioFN;
            }
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
        const string swfRegex = @"http:\/\/.*?\.swf";
        const string linkNumberRegex = @"[0-9]{6}";
        WebClient web = new WebClient();
        HttpClient client = new HttpClient();

        HtmlNode contentTable;

        List<string> resources = new List<string>();
        List<Link> links = new List<Link>();

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
        void ParseText()
        {

        }
        void ParseResources()
        {
            resources.Clear();
            //we are mainly looking for .gifs and .swfs, there are some things we should ignore, such as /images/v2_blankstrip.gif
            var matches = Regex.Matches(contentTable.InnerHtml, gifRegex);

            for (int i = 0; i < matches.Count; i++)
            {
                resources.Add(matches[i].Value);
            }

            matches = Regex.Matches(contentTable.InnerHtml, swfRegex);

            for (int i = 0; i < matches.Count; i++)
            {
                resources.Add(matches[i].Captures[0].Value);
            }

            resources = resources.Distinct().ToList();  //filter out any double grabs
        }
        public Resource[] GetResources()
        {
            Resource[] reses = new Resource[resources.Count];

            for(int i = 0; i < resources.Count; ++i){
                //for each try once on the cdn then try the www throw otherwise
                try
                {
                    reses[i] = new Resource(web.DownloadData(resources[i]), System.IO.Path.GetFileName(new Uri(resources[i]).LocalPath));
                }
                catch (Exception)
                {
                    resources[i] = resources[i].Replace("cdn.mspaintadventures.com", "www.mspaintadventures.com");
                    reses[i] = new Resource(web.DownloadData(resources[i]), System.IO.Path.GetFileName(new Uri(resources[i]).LocalPath));
                }
            }

            return reses;
        }
        void ParseLinks()
        {
            links.Clear();
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
                var res = Regex.Match(Regex.Unescape(actualLink), linkNumberRegex);
                if (res.Success)
                    links.Add(new Link(link.InnerText,Convert.ToInt32(res.Value)));
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
                return web.DownloadData(file.Replace("cdn.mspaintadventures.com", "www.mspaintadventures.com"));
            }
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
                //TODO: Support for scratch, and 2x pages

                if(true){
                    //regular, homosuck, or trickster
                    contentTable = html.DocumentNode.Descendants("table").First().SelectNodes("tr").ElementAt(1).SelectNodes("td").First().SelectNodes("table").First();
                }
                ParseText();
                ParseResources();
                ParseLinks();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
