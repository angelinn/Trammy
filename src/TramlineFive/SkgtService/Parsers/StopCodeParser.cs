using System;
using System.Collections.Generic;
using System.Text;
using SkgtService.Models;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using SkgtService.Exceptions;

namespace SkgtService.Parsers
{
    public class StopCodeParser : BaseParser, IStopCodeParser
    {
        public async Task<Captcha> ChooseLineAsync(SkgtObject target)
        {
            Dictionary<string, string> urlEncoded = GatherInputs(currentHtml.DocumentNode);

            urlEncoded["ctl00$ContentPlaceHolder1$ddlLine"] = target.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(STOP_CODE_URL, new FormUrlEncodedContent(urlEncoded));
            string html = await response.Content.ReadAsStringAsync();

            byte[] bytes = await client.GetByteArrayAsync(CAPTCHA_URL);
            return new Captcha { BinaryContent = bytes };
        }

        public async Task<StopInfo> GetLinesForStopAsync(string stopCode)
        {
            string initialHtml = await client.GetStringAsync(STOP_CODE_URL);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(initialHtml);

            if (doc.DocumentNode.SelectSingleNode("//*[@id=\"id_Captcha\"]") == null)
                throw new TramlineFiveException("Липсва captcha за търсене по спирка.");

            Dictionary<string, string> urlEncoded = GatherInputs(doc.DocumentNode);

            urlEncoded["ctl00$ContentPlaceHolder1$tbStopCode"] = stopCode;
            urlEncoded["ctl00$ContentPlaceHolder1$btnSearchLine.x"] = "0";
            urlEncoded["ctl00$ContentPlaceHolder1$btnSearchLine.y"] = "0";
            HttpResponseMessage response = await client.PostAsync(STOP_CODE_URL, new FormUrlEncodedContent(urlEncoded));
            string lines = await response.Content.ReadAsStringAsync();

            currentHtml = new HtmlDocument();
            currentHtml.LoadHtml(lines);

            StopInfo stop = new StopInfo();
            
            var allLines = currentHtml.DocumentNode.SelectNodes("//select//option");
            foreach (HtmlNode aLine in allLines)
            {
                stop.Lines.Add(new SkgtObject(aLine.InnerText, aLine.Attributes["value"].Value));
            }

            stop.Name = currentHtml.DocumentNode.SelectSingleNode("//*[@id=\"ContentPlaceHolder1_lblStopName\"]").InnerText;
            stop.Direction = currentHtml.DocumentNode.SelectSingleNode("//*[@id=\"ContentPlaceHolder1_lblDescription\"]").InnerText;
            if (!String.IsNullOrEmpty(stop.Direction))
                stop.Direction = stop.Direction.Substring(1, stop.Direction.LastIndexOf("\"") - 1);

            return stop;
        }

        public async Task<IEnumerable<string>> GetTimings(SkgtObject line, string captcha)
        {
            Dictionary<string, string> urlEncoded = GatherInputs(currentHtml.DocumentNode);
            
            urlEncoded["ctl00$ContentPlaceHolder1$CaptchaInput"] = captcha;
            urlEncoded["__EVENTTARGET"] = "ctl00$ContentPlaceHolder1$CaptchaInput";
            urlEncoded["ctl00$ContentPlaceHolder1$ddlLine"] = line.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(STOP_CODE_URL, new FormUrlEncodedContent(urlEncoded));

            currentHtml.LoadHtml(await response.Content.ReadAsStringAsync());
            var nodes = currentHtml.DocumentNode.SelectNodes("//div[contains(@id,'ContentPlaceHolder1_gvTimes_dvItem_')]");
            return nodes?.Select(n => n.InnerText);
        }
    }
}
