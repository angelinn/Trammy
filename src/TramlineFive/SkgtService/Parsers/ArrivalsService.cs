using Newtonsoft.Json;
using SkgtService.Exceptions;
using SkgtService.Models;
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

    public ArrivalsService()
    {
        client.DefaultRequestHeaders.Add("User-Agent", OPERA_USER_AGENT);
    }

    public async Task<StopInfo> GetByStopCodeAsync(string stopCode)
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
        var routes = await StopsLoader.LoadRoutesAsync();

        foreach (Line line in info.Lines)
        {
            if (!routes.ContainsKey(line.VehicleType) || !routes[line.VehicleType].ContainsKey(line.Name.Replace("E", "Е")))
                continue;

            // routes.json comes with cyrilic E
            List<Way> ways = routes[line.VehicleType][line.Name.Replace("E", "Е")];
            if (ways.Count > 0)
            {
                List<Way> waysWithStop = ways.Where(w => w.Codes.FirstOrDefault(code => code == stopCode) != null).ToList();
                Way way = waysWithStop.Count > 1 ? waysWithStop.FirstOrDefault(w => w.Codes[0] == stopCode) : waysWithStop[0];

                if (way != null)
                {
                    line.Start = StopsLoader.StopsHash[way.Codes[0]].PublicName;
                    line.Direction = StopsLoader.StopsHash[way.Codes[^1]].PublicName;
                }
            }
        }

        return info;
    }
}
