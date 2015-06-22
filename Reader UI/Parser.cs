using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Net.Http;

namespace Reader_UI
{
    class Parser
    {
        const string prepend = "http://www.mspaintadventures.com/?s=6&p=";
        HtmlDocument html;

        HttpClient http = new HttpClient();
        
        
        public bool LoadPage(int pageno)
        {
            try
            {
                var response = http.GetByteArrayAsync(new Uri(prepend + pageno.ToString("D6"))).Result;
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                html = new HtmlDocument();
                html.LoadHtml(source);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
