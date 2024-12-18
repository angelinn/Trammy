﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry;
using Sentry.Protocol;
using SkgtService.Exceptions;
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
using static System.Formats.Asn1.AsnWriter;

namespace SkgtService;

public static class StopsLoader
{
    private const string STOPS_URL = "https://sofiatraffic.bg/bg/public-transport";
    private const string LINES_URL = "https://sofiatraffic.bg/bg/trip/getLines";
    //private const string ROUTES_URL = "https://routes.sofiatraffic.bg/resources/routes.json";
    private const string GET_SCHEDULE_URL = "https://sofiatraffic.bg/bg/trip/getSchedule";
    private static string PATH = String.Empty;
    private static string ROUTES_PATH = String.Empty;
    private static string LINES_PATH = String.Empty;

    public static event EventHandler OnStopsUpdated;

    private static Dictionary<string, List<Arrival>> stopLines = new();
    private static SofiaHttpClient sofiaHttpClient;

    public static void Initialize(string basePath, SofiaHttpClient httpClient)
    {
        PATH = Path.Combine(basePath, "stops.json");
        ROUTES_PATH = Path.Combine(basePath, "routes.json");
        LINES_PATH = Path.Combine(basePath, "lines.json");

        sofiaHttpClient = httpClient;
    }

    public static async Task<List<StopLocation>> LoadStopsAsync()
    {
        if (!File.Exists(PATH))
        {
            await UpdateStopsAsync();
        }

        string json = File.ReadAllText(PATH);
        string stopsArray = JObject.Parse(json)["props"]["stops"].ToString();
        return JsonConvert.DeserializeObject<List<StopLocation>>(stopsArray);

    }

    //public static async Task<List<Routes>> LoadRoutesAsync()
    //{
    //    if (!File.Exists(ROUTES_PATH))
    //    {
    //        await UpdateRoutesAsync();
    //    }

    //    string json = File.ReadAllText(ROUTES_PATH);
    //    List<Routes> routes = JsonConvert.DeserializeObject<List<Routes>>(json);

    //    return routes;
    //}

    public static List<StopLocation> LoadStops(Stream stream)
    {
        string json = String.Empty;
        using StreamReader reader = new StreamReader(stream);
        json = reader.ReadToEnd();

        return JsonConvert.DeserializeObject<List<StopLocation>>(json);
    }

    public static async Task UpdateStopsAsync()
    {
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "x-inertia", "true" },
            {  "x-inertia-version", "71cd55a5934e0cc54a86b740873377e4" }
        };

        HttpResponseMessage response = await sofiaHttpClient.GetAsync(STOPS_URL, headers);

        string stops = await response.Content.ReadAsStringAsync();
        SentrySdk.CaptureMessage($"updatestops: {response.StatusCode}, length: {stops.Length}");

        File.WriteAllText(PATH, stops);

        OnStopsUpdated?.Invoke(null, new EventArgs());
    }

    //public static async Task UpdateRoutesAsync()
    //{
    //    using HttpClient client = new HttpClient();

    //    byte[] stops = await client.GetByteArrayAsync(ROUTES_URL);
    //    File.WriteAllBytes(ROUTES_PATH, stops);

    //    //OnStopsUpdated?.Invoke(null, new EventArgs());
    //}

    public static async Task GetLinesAsync()
    {
        HttpResponseMessage response = await sofiaHttpClient.PostAsync(LINES_URL, new StringContent("", null, "application/json"));
        string lines = await response.Content.ReadAsStringAsync();
        SentrySdk.CaptureMessage($"update lines: {response.StatusCode}, length: {lines.Length}");

        File.WriteAllText(LINES_PATH, lines);
    }

    public static async Task<List<Line>> LoadLinesAsync()
    {
        if (!File.Exists(LINES_PATH))
        {
            await GetLinesAsync();
        }

        string json = File.ReadAllText(LINES_PATH);
        List<Line> lines = JsonConvert.DeserializeObject<List<Line>>(json);

        return lines;
    }


    public static async Task<ScheduleResponse> GetSchedule(Line line)
    {
        string payload = JsonConvert.SerializeObject(line);
        HttpResponseMessage response = await sofiaHttpClient.PostAsync(GET_SCHEDULE_URL, new StringContent(payload, Encoding.UTF8, "application/json"));

        string responseJson = await response.Content.ReadAsStringAsync();
        ScheduleResponse schedule = JsonConvert.DeserializeObject<ScheduleResponse>(responseJson);
        return schedule;
    }

    public static void ClearData()
    {
        if (File.Exists(PATH))
            File.Delete(PATH);

        if (File.Exists(ROUTES_PATH))
            File.Delete(ROUTES_PATH);
    }
}
