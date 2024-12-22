using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry;
using Sentry.Protocol;
using SkgtService.Exceptions;
using SkgtService.Models;
using SkgtService.Models.Json;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService;

public class StopsLoader
{
    private const string STOPS_URL = "https://sofiatraffic.bg/bg/public-transport";
    private const string LINES_URL = "https://sofiatraffic.bg/bg/trip/getLines";
    //private const string ROUTES_URL = "https://routes.sofiatraffic.bg/resources/routes.json";
    private const string GET_SCHEDULE_URL = "https://sofiatraffic.bg/bg/trip/getSchedule";
    private string PATH = String.Empty;
    private string ROUTES_PATH = String.Empty;
    private string LINES_PATH = String.Empty;

    public string Version { get; set; }

    public static event EventHandler<string> OnStopsUpdated;

    private Dictionary<string, List<Arrival>> stopLines = new();
    private SofiaHttpClient sofiaHttpClient;

    public StopsLoader(SofiaHttpClient sofiaHttpClient, StopsConfigurator stopsConfigurator)
    {
        PATH = Path.Combine(stopsConfigurator.DatabasePath, "stops.json");
        ROUTES_PATH = Path.Combine(stopsConfigurator.DatabasePath, "routes.json");
        LINES_PATH = Path.Combine(stopsConfigurator.DatabasePath, "lines.json");

        this.sofiaHttpClient = sofiaHttpClient;
    }

    public async Task<List<StopLocation>> LoadStopsAsync()
    {
        if (!File.Exists(PATH))
        {
            await UpdateStopsAsync();
        }

        try
        {
            string json = File.ReadAllText(PATH);
            if (string.IsNullOrEmpty(json))
            {
                File.Delete(PATH);
                return await LoadStopsAsync();
            }

            return JsonConvert.DeserializeObject<List<StopLocation>>(json);
        }
        catch (Exception ex)
        {
            File.Delete(PATH);
            return await LoadStopsAsync();
        }

    }

    //public  async Task<List<Routes>> LoadRoutesAsync()
    //{
    //    if (!File.Exists(ROUTES_PATH))
    //    {
    //        await UpdateRoutesAsync();
    //    }

    //    string json = File.ReadAllText(ROUTES_PATH);
    //    List<Routes> routes = JsonConvert.DeserializeObject<List<Routes>>(json);

    //    return routes;
    //}

    public List<StopLocation> LoadStops(Stream stream)
    {
        string json = String.Empty;
        using StreamReader reader = new StreamReader(stream);
        json = reader.ReadToEnd();

        return JsonConvert.DeserializeObject<List<StopLocation>>(json);
    }

    public async Task UpdateStopsAsync()
    {
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-inertia", "true" },
            {  "x-inertia-version", Version }
        };

        HttpResponseMessage response = await sofiaHttpClient.GetAsync(STOPS_URL, headers);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            response = await sofiaHttpClient.GetAsync(STOPS_URL);
        }

        string body = await response.Content.ReadAsStringAsync();
        if (body.Contains("<!DOCTYPE html>"))
        {
            int index = body.IndexOf("&quot;version&quot;:&quot;");

            Version = body.Substring(index + "&quot;version&quot;:&quot;".Length, 32);
            headers["x-inertia-version"] = Version;

            response = await sofiaHttpClient.GetAsync(STOPS_URL, headers);

            body = await response.Content.ReadAsStringAsync();
        }

        SentrySdk.CaptureMessage($"updatestops: {response.StatusCode}, length: {body.Length}");

        try
        {
            string stopsArray = JObject.Parse(body)["props"]["stops"].ToString();
            File.WriteAllText(PATH, stopsArray);
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            throw;
        }

        OnStopsUpdated?.Invoke(null, Version);
    }

    //public  async Task UpdateRoutesAsync()
    //{
    //    using HttpClient client = new HttpClient();

    //    byte[] stops = await client.GetByteArrayAsync(ROUTES_URL);
    //    File.WriteAllBytes(ROUTES_PATH, stops);

    //    //OnStopsUpdated?.Invoke(null, new EventArgs());
    //}

    public async Task GetLinesAsync()
    {
        HttpResponseMessage response = await sofiaHttpClient.PostAsync(LINES_URL, new StringContent("", null, "application/json"));
        string lines = await response.Content.ReadAsStringAsync();
        SentrySdk.CaptureMessage($"update lines: {response.StatusCode}, length: {lines.Length}");

        File.WriteAllText(LINES_PATH, lines);
    }

    public async Task<List<Line>> LoadLinesAsync()
    {
        if (!File.Exists(LINES_PATH))
        {
            await GetLinesAsync();
        }

        string json = File.ReadAllText(LINES_PATH);
        List<Line> lines = JsonConvert.DeserializeObject<List<Line>>(json);

        return lines;
    }


    public async Task<ScheduleResponse> GetSchedule(Line line)
    {
        string payload = JsonConvert.SerializeObject(line);
        HttpResponseMessage response = await sofiaHttpClient.PostAsync(GET_SCHEDULE_URL, new StringContent(payload, Encoding.UTF8, "application/json"));

        string responseJson = await response.Content.ReadAsStringAsync();
        ScheduleResponse schedule = JsonConvert.DeserializeObject<ScheduleResponse>(responseJson);
        return schedule;
    }

    public void ClearData()
    {
        if (File.Exists(PATH))
            File.Delete(PATH);

        if (File.Exists(ROUTES_PATH))
            File.Delete(ROUTES_PATH);
    }
}
