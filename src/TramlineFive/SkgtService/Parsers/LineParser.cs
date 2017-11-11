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
        public async Task<IEnumerable<Line>> GetLinesAsync(TransportType type)
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
            return allLines.Select(l => new Line(l.InnerText, l.Attributes["value"].Value));
        }

        public async Task<IEnumerable<Direction>> GetDirectionsAsync(Line selected)
        {
            Dictionary<string, string> urlEncoded = GatherInputs(currentHtml.DocumentNode);

            urlEncoded["ctl00$ContentPlaceHolder1$ddlLines"] = selected.SkgtValue;
            HttpResponseMessage response = await client.PostAsync(LINE_URL, new FormUrlEncodedContent(urlEncoded));
            string html = await response.Content.ReadAsStringAsync();
            currentHtml.LoadHtml(html);

            List<Direction> directions = new List<Direction>();

            var directionNodes = currentHtml.DocumentNode.SelectNodes("//*[@id=\"ContentPlaceHolder1_rblRoute\"]//td");
            foreach (HtmlNode node in directionNodes)
            {
                HtmlNode input = node.SelectSingleNode("input");
                directions.Add(new Direction { DisplayName = node.InnerText, SkgtValue = input.Attributes["value"].Value });
            }

            return directions;
        }
    }
}
