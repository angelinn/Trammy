using HtmlAgilityPack;
using SkgtService.Exceptions;
using SkgtService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers
{
    public class LineParser : BaseParser, ILineParser
    {
        public async Task<IEnumerable<SkgtObject>> GetLinesAsync(TransportType type)
        {
            string initialHtml = await client.GetStringAsync(LINE_URL);
            currentHtml = new HtmlDocument();
            currentHtml.LoadHtml(initialHtml);

            if (currentHtml.DocumentNode.SelectSingleNode("//*[@id=\"id_Captcha\"]") == null)
                throw new TramlineFiveException("Липсва captcha за търсене по спирка.");

            Dictionary<string, string> urlEncoded = GatherInputs(currentHtml.DocumentNode);

            urlEncoded["ctl00$ContentPlaceHolder1$ddlTransportType"] = ((int)type).ToString();
            HttpResponseMessage response = await client.PostAsync(LINE_URL, new FormUrlEncodedContent(urlEncoded));
            string linesHtml = await response.Content.ReadAsStringAsync();
            currentHtml.LoadHtml(linesHtml);

            var allLines = currentHtml.DocumentNode.SelectNodes("//*[@id=\"ContentPlaceHolder1_ddlLines\"]//option");
            return allLines.Select(l => new SkgtObject(l.InnerText, l.Attributes["value"].Value));
        }

        public async Task<IEnumerable<SkgtObject>> GetDirectionsAsync(SkgtObject selected)
        {
            Dictionary<string, string> urlEncoded = GatherInputs(currentHtml.DocumentNode);

            urlEncoded["ctl00$ContentPlaceHolder1$ddlLines"] = selected.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(LINE_URL, new FormUrlEncodedContent(urlEncoded));
            string html = await response.Content.ReadAsStringAsync();
            currentHtml.LoadHtml(html);

            List<SkgtObject> directions = new List<SkgtObject>();

            var directionNodes = currentHtml.DocumentNode.SelectNodes("//*[@id=\"ContentPlaceHolder1_rblRoute\"]//td");
            foreach (HtmlNode node in directionNodes)
            {
                HtmlNode input = node.SelectSingleNode("input");
                directions.Add(new SkgtObject(node.InnerText, input.Attributes["value"].Value));
            }

            return directions;
        }

        public async Task<IEnumerable<SkgtObject>> GetStopsAsync(SkgtObject direction)
        {
            Dictionary<string, string> urlEncoded = GatherInputs(currentHtml.DocumentNode);

            urlEncoded["ctl00$ContentPlaceHolder1$rblRoute"] = direction.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(LINE_URL, new FormUrlEncodedContent(urlEncoded));
            string html = await response.Content.ReadAsStringAsync();
            currentHtml.LoadHtml(html);

            List<SkgtObject> stops = new List<SkgtObject>();
            
            var options = currentHtml.DocumentNode.SelectNodes("//*[@id=\"ContentPlaceHolder1_ddlStops\"]//option");
            foreach (HtmlNode stop in options)
            {
                stops.Add(new SkgtObject(stop.InnerText, stop.Attributes["value"].Value));
            }

            return stops;
        }

        public async Task<Captcha> ChooseStopAsync(SkgtObject stop)
        {
            Dictionary<string, string> urlEncoded = GatherInputs(currentHtml.DocumentNode);

            urlEncoded["ctl00$ContentPlaceHolder1$ddlStops"] = stop.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(LINE_URL, new FormUrlEncodedContent(urlEncoded));
            string html = await response.Content.ReadAsStringAsync();

            byte[] bytes = await client.GetByteArrayAsync(CAPTCHA_URL);
            return new Captcha { BinaryContent = bytes };
        }

        public async Task<IEnumerable<string>> GetTimings(SkgtObject stop, SkgtObject direction, string captcha)
        {
            Dictionary<string, string> urlEncoded = GatherInputs(currentHtml.DocumentNode);

            urlEncoded["ctl00$ContentPlaceHolder1$CaptchaInput"] = captcha;
            urlEncoded["__EVENTTARGET"] = "ctl00$ContentPlaceHolder1$CaptchaInput";
            urlEncoded["ctl00$ContentPlaceHolder1$ddlStops"] = stop.SkgtValue;
            urlEncoded["ctl00$ContentPlaceHolder1$rblRoute"] = direction.SkgtValue;

            HttpResponseMessage response = await client.PostAsync(LINE_URL, new FormUrlEncodedContent(urlEncoded));

            currentHtml.LoadHtml(await response.Content.ReadAsStringAsync());
            var nodes = currentHtml.DocumentNode.SelectNodes("//div[contains(@id,'ContentPlaceHolder1_gvTimes_dvItem_')]");
            return nodes?.Select(n => n.InnerText);
        }
    }
}
