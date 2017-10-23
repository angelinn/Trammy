using System;
using System.Collections.Generic;
using System.Text;
using SkgtService.Models;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace SkgtService.Parsers
{
    public class DesktopSkgtParser : ISkgtParser
    {
        private readonly HttpClient client = new HttpClient();
        private const string VIRTUAL_TABLES_URL = @"https://skgt-bg.com/VirtualBoard/Web/SelectByStop.aspx";
        private const string CAPTCHA_URL = @"https://skgt-bg.com/VirtualBoard/Services/Captcha.ashx";
        private const string OPERA_USER_AGENT = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.35";

        private HtmlDocument currentHtml;

        public DesktopSkgtParser()
        {
            client.DefaultRequestHeaders.Add("User-Agent", OPERA_USER_AGENT);
        }
        
        public async Task<Captcha> ChooseLineAsync(Line target)
        {
            Dictionary<string, string> urlEncoded = new Dictionary<string, string>();

            var desc = currentHtml.DocumentNode.SelectNodes("//input");
            foreach (HtmlNode node in desc)
            {
                string value = (node.Attributes["value"] == null) ? String.Empty : node.Attributes["value"].Value;
                urlEncoded.Add(node.Attributes["name"].Value, value);
            }
            urlEncoded["ctl00$ContentPlaceHolder1$ddlLine"] = target.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(VIRTUAL_TABLES_URL, new FormUrlEncodedContent(urlEncoded));
            string html = await response.Content.ReadAsStringAsync();

            byte[] bytes = await client.GetByteArrayAsync(CAPTCHA_URL);
            return new Captcha { BinaryContent = bytes };
        }

        public async Task<Stop> GetLinesForStopAsync(string stopCode)
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

            currentHtml = new HtmlDocument();
            currentHtml.LoadHtml(lines);

            Stop stop = new Stop();
            
            var allLines = currentHtml.DocumentNode.SelectNodes("//select//option");
            foreach (HtmlNode aLine in allLines)
            {
                stop.Lines.Add(new Line(aLine.InnerText, aLine.Attributes["value"].Value));
            }

            stop.Name = currentHtml.DocumentNode.SelectSingleNode("//*[@id=\"ContentPlaceHolder1_lblStopName\"]").InnerText;
            stop.Direction = currentHtml.DocumentNode.SelectSingleNode("//*[@id=\"ContentPlaceHolder1_lblDescription\"]").InnerText;
            if (!String.IsNullOrEmpty(stop.Direction))
                stop.Direction = stop.Direction.Substring(1, stop.Direction.LastIndexOf("\"") - 1);

            return stop;
        }

        public async Task<IEnumerable<string>> GetTimings(Line line, string captcha)
        {
            Dictionary<string, string> urlEncoded = new Dictionary<string, string>();

            var desc = currentHtml.DocumentNode.SelectNodes("//input");
            foreach (HtmlNode node in desc)
            {
                string value = (node.Attributes["value"] == null) ? String.Empty : node.Attributes["value"].Value;
                urlEncoded.Add(node.Attributes["name"].Value, value);
            }
            
            urlEncoded["ctl00$ContentPlaceHolder1$CaptchaInput"] = captcha;
            urlEncoded["__EVENTTARGET"] = "ctl00$ContentPlaceHolder1$CaptchaInput";
            urlEncoded["ctl00$ContentPlaceHolder1$ddlLine"] = line.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(VIRTUAL_TABLES_URL, new FormUrlEncodedContent(urlEncoded));

            currentHtml.LoadHtml(await response.Content.ReadAsStringAsync());
            var nodes = currentHtml.DocumentNode.SelectNodes("//div[contains(@id,'ContentPlaceHolder1_gvTimes_dvItem_')]");
            return nodes?.Select(n => n.InnerText);
        }
    }
}
