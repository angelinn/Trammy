using System;
using System.Collections.Generic;
using System.Text;
using SkgtService.Models;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    public class DesktopSkgtParser : ISkgtParser
    {
        private HttpClient client = new HttpClient();
        private const string VIRTUAL_TABLES_URL = @"https://skgt-bg.com/VirtualBoard/Web/SelectByStop.aspx";
        private const string OPERA_USER_AGENT = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.35";

        private HtmlDocument linesDocument;

        public DesktopSkgtParser()
        {
            client.DefaultRequestHeaders.Add("User-Agent", OPERA_USER_AGENT);
        }
        

        public async Task<string> ChooseLineAsync(Line target)
        {
            Dictionary<string, string> urlEncoded = new Dictionary<string, string>();

            var desc = linesDocument.DocumentNode.SelectNodes("//input");
            foreach (HtmlNode node in desc)
            {
                string value = (node.Attributes["value"] == null) ? String.Empty : node.Attributes["value"].Value;
                urlEncoded.Add(node.Attributes["name"].Value, value);
            }
            urlEncoded["ctl00$ContentPlaceHolder1$ddlLine"] = target.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(VIRTUAL_TABLES_URL, new FormUrlEncodedContent(urlEncoded));
            string html = await response.Content.ReadAsStringAsync();
            return null;
        }

        public async Task<List<Line>> GetLinesForStopAsync(string stopCode)
        {
            HtmlDocument doc = new HtmlWeb().Load(VIRTUAL_TABLES_URL);
            HtmlNode.ElementsFlags.Remove("option");

            Dictionary<string, string> urlEncoded = new Dictionary<string, string>();

            var desc = doc.DocumentNode.SelectNodes("//input");
            foreach (HtmlNode node in desc)
            {
                string value = (node.Attributes["value"] == null) ? String.Empty : node.Attributes["value"].Value;
                urlEncoded.Add(node.Attributes["name"].Value, value);
            }
            urlEncoded["ctl00$ContentPlaceHolder1$tbStopCode"] = stopCode;
            urlEncoded["ctl00$ContentPlaceHolder1$btnSearchLine.x"] = "0";
            urlEncoded["ctl00$ContentPlaceHolder1$btnSearchLine.y"] = "0";
            HttpResponseMessage response = await client.PostAsync(VIRTUAL_TABLES_URL, new FormUrlEncodedContent(urlEncoded));
            string lines = await response.Content.ReadAsStringAsync();

            linesDocument = new HtmlDocument();
            linesDocument.LoadHtml(lines);

            List<Line> currentLines = new List<Line>();
            var allLines = linesDocument.DocumentNode.SelectNodes("//select//option");
            foreach (HtmlNode aLine in allLines)
            {
                currentLines.Add(new Line(aLine.InnerText, aLine.Attributes["value"].Value));
            }

            return currentLines;
        }

        public string[] GetTimings(string captcha)
        {
            throw new NotImplementedException();
        }
    }
}
