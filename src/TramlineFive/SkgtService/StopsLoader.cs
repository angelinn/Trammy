using Newtonsoft.Json;
using SkgtService.Models;
using SkgtService.Models.Locations;
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
    public static List<StopLocation> Stops { get; private set; }
    public static Dictionary<string, StopLocation> StopsHash { get; private set; }
    public static List<Route> Routes { get; private set; }

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
        Stops = JsonConvert.DeserializeObject<List<StopLocation>>(json);

        if (StopsHash == null)
        {
            StopsHash = new Dictionary<string, StopLocation>();
            foreach (var stop in Stops)
            {
                StopsHash.Add(stop.Code, stop);
            }
        }

        return Stops;
    }

    public static async Task<List<Route>> LoadRoutesAsync()
    {
        if (!File.Exists(ROUTES_PATH))
        {
            await UpdateRoutesAsync();
        }
 
        string json = File.ReadAllText(ROUTES_PATH);
        Routes = JsonConvert.DeserializeObject<List<MainRouteObject>>(json).SelectMany(r => r.Lines).ToList();

        return Routes;
    }

    public static List<StopLocation> LoadStops(Stream stream)
    {
        string json = String.Empty;
        using StreamReader reader = new StreamReader(stream);
        json = reader.ReadToEnd();

        Stops = JsonConvert.DeserializeObject<List<StopLocation>>(json);

        return Stops;
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
