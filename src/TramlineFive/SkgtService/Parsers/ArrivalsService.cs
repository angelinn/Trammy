using Newtonsoft.Json;
using SkgtService.Exceptions;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Parsers;

public class ArrivalsService
{
    private const string ARRIVALS_API_URL = @"https://api-arrivals.sofiatraffic.bg/api/v1/arrivals";
    private const string OPERA_USER_AGENT = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.35";

    private readonly HttpClient client = new HttpClient();
    private readonly PublicTransport publicTransport;

    public ArrivalsService(PublicTransport publicTransport)
    {
        client.DefaultRequestHeaders.Add("User-Agent", OPERA_USER_AGENT);
        this.publicTransport = publicTransport;
    }

    public async Task<StopResponse> GetByStopCodeAsync(string stopCode)
    {
        HttpResponseMessage response = await client.GetAsync($"{ARRIVALS_API_URL}/{stopCode}/");
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new StopNotFoundException();

            throw new StopRequestException();
        }

        string json = await response.Content.ReadAsStringAsync();
        StopInfo info = JsonConvert.DeserializeObject<StopInfo>(json);

        StopResponse stopResponse = new StopResponse(info);

        foreach (Line line in info.Lines)
        {
            DateTime firstArrival = DateTime.Parse(line.Arrivals[0].Time);
            // some lines come with negative arrival times
            if ((DateTime.Now - firstArrival) > TimeSpan.FromMinutes(5) || firstArrival > DateTime.Now.AddHours(5))
            {
                line.Name += " (R)";
                //pendingDelete.Add(line);
                //continue;
            }

            LineInformation lineInformation = publicTransport.FindByTypeAndLine(line.VehicleType, line.Name);
            if (lineInformation == null)
                continue;

            // routes.json comes with cyrilic E
            if (lineInformation.Routes.Count > 0)
            {
                List<LineRoute> waysWithStop = lineInformation.Routes.Where(w => w.Codes.FirstOrDefault(code => code == stopCode) != null).ToList();
                if (waysWithStop.Count == 0)
                {
                    continue;
                }

                LineRoute way = waysWithStop.Count > 1 ? waysWithStop.FirstOrDefault(w => w.Codes[0] == stopCode) : waysWithStop[0];

                if (way != null)
                {
                    line.Start = publicTransport.FindStop(way.Codes[0]).PublicName;
                    line.Direction = publicTransport.FindStop(way.Codes[^1]).PublicName;
                }
            }

            stopResponse.Arrivals.Add(new ArrivalInformation(line));
        }

        return stopResponse;
    }
}
