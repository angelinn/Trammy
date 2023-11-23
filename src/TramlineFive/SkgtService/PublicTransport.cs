using SkgtService.Exceptions;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace SkgtService;

public class PublicTransport
{
    private Dictionary<string, StopInformation> stopsHash = new();
    private Dictionary<string, Dictionary<string, RouteInformation>> routes = new();

    public List<StopInformation> Stops => stopsHash.Values.ToList();

    public async Task LoadData()
    {
        if (stopsHash.Count > 0)
            return;

        List<StopLocation> stops = await StopsLoader.LoadStopsAsync();

        foreach (var stop in stops)
        {
            stopsHash.Add(stop.Code, new StopInformation(stop));
        }

        var loadedRoutes = await StopsLoader.LoadRoutesAsync();
        foreach (Routes route in loadedRoutes)
        {
            Dictionary<string, RouteInformation> singleLineRoutes = new();
            foreach (Route line in route.Lines)
            {
                singleLineRoutes.Add(line.Name, new RouteInformation(route.Type, line.Name, line.Routes));
                foreach (Way way in line.Routes)
                {
                    foreach (string code in way.Codes)
                    {
                        stopsHash[code].Lines.Add(new LineInformation(line.Name, route.Type));
                        stopsHash[code].Lines = stopsHash[code].Lines.DistinctBy(line => line.Name).ToList();
                    }
                }
            }

            routes.Add(route.Type, singleLineRoutes);
        }
    }

    public StopInformation FindStop(string code)
    {
        if (stopsHash.TryGetValue(code, out StopInformation stopInformation))
            return stopInformation;

        return null;
    }

    public RouteInformation FindByTypeAndLine(string type, string line)
    {
        if (routes.TryGetValue(type, out var linesInfo))
        {
            if (linesInfo.TryGetValue(line, out RouteInformation routeInformation))
                return routeInformation;
        }

        return null;
    }

    public List<RouteInformation> FindByType(string type)
    {
        if (routes.TryGetValue(type, out var linesInfo))
        {
            return linesInfo.Values.ToList();
        }

        return null;
    }


    public RouteInformation FindStopsByLine(string line, string type)
    {
        if (routes.TryGetValue(type, out Dictionary<string, RouteInformation> route))
        {
            if (route.TryGetValue(line, out RouteInformation routes))
            {
                return routes;
            }

            throw new TramlineFiveException($"Could not find line with id {line} and type {type}");
        }

        throw new TramlineFiveException($"Could not find type {type}");
    }
}
