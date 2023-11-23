using Newtonsoft.Json;
using SkgtService.Exceptions;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService;

public static class StopsLoader
{
    private const string URL = "https://routes.sofiatraffic.bg/resources/stops-bg.json";
    private const string ROUTES_URL = "https://routes.sofiatraffic.bg/resources/routes.json";
    private static string PATH = String.Empty;
    private static string ROUTES_PATH = String.Empty;

    public static event EventHandler OnStopsUpdated;

    private static Dictionary<string, List<Line>> stopLines = new();

    public static void Initialize(string basePath)
    {
        PATH = Path.Combine(basePath, "stops.json");
        ROUTES_PATH = Path.Combine(basePath, "routes.json");
    }

    public static async Task<List<StopLocation>> LoadStopsAsync()
    {
        if (!File.Exists(PATH))
        {
            await UpdateStopsAsync();
        }

        string json = File.ReadAllText(PATH);
        return JsonConvert.DeserializeObject<List<StopLocation>>(json);

    }

    public static async Task<List<Routes>> LoadRoutesAsync()
    {
        if (!File.Exists(ROUTES_PATH))
        {
            await UpdateRoutesAsync();
        }

        string json = File.ReadAllText(ROUTES_PATH);
        List<Routes> routes = JsonConvert.DeserializeObject<List<Routes>>(json);

        return routes;
    }

    public static List<StopLocation> LoadStops(Stream stream)
    {
        string json = String.Empty;
        using StreamReader reader = new StreamReader(stream);
        json = reader.ReadToEnd();

        return JsonConvert.DeserializeObject<List<StopLocation>>(json);
    }

    public static async Task UpdateStopsAsync()
    {
        using HttpClient client = new HttpClient();

        byte[] stops = await client.GetByteArrayAsync(URL);
        File.WriteAllBytes(PATH, stops);

        OnStopsUpdated?.Invoke(null, new EventArgs());
    }

    public static async Task UpdateRoutesAsync()
    {
        using HttpClient client = new HttpClient();

        byte[] stops = await client.GetByteArrayAsync(ROUTES_URL);
        File.WriteAllBytes(ROUTES_PATH, stops);

        //OnStopsUpdated?.Invoke(null, new EventArgs());
    }
}
