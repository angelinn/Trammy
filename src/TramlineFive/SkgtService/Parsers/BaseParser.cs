using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SkgtService.Parsers
{
    public abstract class BaseParser
    {
        protected readonly HttpClient client = new HttpClient();
        protected const string STOP_CODE_URL = @"https://skgt-bg.com/VirtualBoard/Web/SelectByStop.aspx";
        protected const string LINE_URL = @"https://skgt-bg.com/VirtualBoard/Web/SelectByLine.aspx";
        protected const string CAPTCHA_URL = @"https://skgt-bg.com/VirtualBoard/Services/Captcha.ashx";
        protected const string OPERA_USER_AGENT = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.35";

        protected HtmlDocument currentHtml;

        public BaseParser()
        {
            client.DefaultRequestHeaders.Add("User-Agent", OPERA_USER_AGENT);
            HtmlNode.ElementsFlags.Remove("option");
        }

        protected Dictionary<string, string> GatherInputs(HtmlNode rootNode)
        {
            Dictionary<string, string> urlEncoded = new Dictionary<string, string>();

            HtmlNodeCollection desc = rootNode.SelectNodes("//input");
            foreach (HtmlNode node in desc)
            {
                string value = (node.Attributes["value"] == null) ? String.Empty : node.Attributes["value"].Value;
                urlEncoded.Add(node.Attributes["name"].Value, value);
            }

            return urlEncoded;
        }
    }
}
