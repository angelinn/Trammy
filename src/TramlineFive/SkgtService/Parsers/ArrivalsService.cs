using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry;
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
    private const string ARRIVALS_API_URL = @"https://sofiatraffic.bg/bg/trip/getVirtualTable";
    private const string OPERA_USER_AGENT = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36 OPR/48.0.2685.35";

    private readonly PublicTransport publicTransport;
    private readonly SofiaHttpClient sofiaHttpClient;

    public ArrivalsService(PublicTransport publicTransport, SofiaHttpClient sofiaHttpClient)
    {
        this.publicTransport = publicTransport;
        this.sofiaHttpClient = sofiaHttpClient;
    }

    public async Task<StopResponse> GetByStopCodeAsync(string stopCode, TransportType? type = null)
    {
        string payload = String.Empty;
        if (type.HasValue)
            payload = $"{{ \"stop\": \"{stopCode}\", \"type\": {(int)type.Value} }}";
        else
            payload = $"{{ \"stop\": \"{stopCode}\" }}";
        
        HttpResponseMessage response = await sofiaHttpClient.PostAsync(ARRIVALS_API_URL, new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new StopNotFoundException();

            throw new StopRequestException();
        }

        string json = await response.Content.ReadAsStringAsync();
        JObject jsonO = JObject.Parse(json);

        List<LineArrivalInfo> stopsInfos = new List<LineArrivalInfo>();
        foreach (JToken child in jsonO.Children().Values())
        {
            LineArrivalInfo info = JsonConvert.DeserializeObject<LineArrivalInfo>(child.ToString());
            stopsInfos.Add(info);
        }

        StopResponse stopResponse = new StopResponse(stopCode, publicTransport.FindStop(stopCode).PublicName);

        stopResponse.Arrivals.AddRange(stopsInfos.Select(s => new ArrivalInformation(s)));

        return stopResponse;
    }
}
