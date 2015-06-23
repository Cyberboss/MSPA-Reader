using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Reader_UI
{
    class Parser
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
        const string prepend = "http://www.mspaintadventures.com/?s=6&p=";
        const string gifRegex = @"http:\/\/(?!" + 
            @".*v2_blankstrip"  //stuff to ignore
            + @"|.*v2_blanksquare2"
            + @"|.*v2_blanksquare3"
            + @"|.*spacer"
            + @")(.*)\.gif";
        const string swfRegex = @"http:\/\/.*?\.swf";
        WebClient web = new WebClient();
        HttpClient client = new HttpClient();

        HtmlNode contentTable;

        List<string> resources = new List<string>();
        
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
                //TODO: Support for scratch and 2x pages

                if(true){    //regular or trickster
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
