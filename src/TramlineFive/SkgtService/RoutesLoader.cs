using Newtonsoft.Json;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService;

public class RoutesLoader
{
    private const string ROUTES_URL = "https://livemap.sofiatraffic.bg/api/lines-data";
    private readonly string ROUTES_PATH;
    private HttpClient httpClient = new();
    private readonly StopsConfigurator stopsConfigurator;

    private Dictionary<string, Route1> routes = new();

    public RoutesLoader(StopsConfigurator stopsConfigurator)
    {
        this.stopsConfigurator = stopsConfigurator;
        ROUTES_PATH = Path.Combine(stopsConfigurator.DatabasePath, "routes.json");
    }

    public async Task<(bool, string)> FetchRoutesFromAPI()
    {
        HttpResponseMessage response = await httpClient.GetAsync(ROUTES_URL);
        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(ROUTES_PATH, json);

            return (true, json);
        }

        return (false, string.Empty);
    }

    public async Task LoadRoutes()
    {
        if (routes.Count > 0)
            return;

        if (!File.Exists(ROUTES_PATH))
            await FetchRoutesFromAPI();

        string json = await File.ReadAllTextAsync(ROUTES_PATH);
        List<Route1> routesList = JsonConvert.DeserializeObject<List<Route1>>(json);

        foreach (Route1 route in routesList)
        {
            this.routes[route.Line] = route;
        }
    }

    public Route1 GetRoute(string line, TransportType transportType)
    {
        string prefix = transportType switch
        {
            TransportType.Bus => "A",
            TransportType.Tram => "TM",
            TransportType.Trolley => "TB",
            TransportType.Subway => "M",
            _ => string.Empty
        };

        if (routes.TryGetValue(prefix + line, out Route1 value))
            return value;

        if (routes.TryGetValue("TB" + line, out Route1 value1))
            return value1;

        return null;
    }
}
