using GalaSoft.MvvmLight.Messaging;
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
    private Dictionary<string, Dictionary<string, LineInformation>> lines = new();

    private List<StopInformation> stops;
    public List<StopInformation> Stops
    {
        get
        {
            if (stops == null)
                stops = stopsHash.Values.ToList();

            return stops;
        }
    }

    public Dictionary<string, Dictionary<string, LineInformation>> Lines => lines;

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
            Dictionary<string, LineInformation> singleLineRoutes = new();
            foreach (Route line in route.Lines)
            {
                LineInformation lineInformation = new LineInformation(line.Name, route.Type, line.Routes);
                singleLineRoutes.Add(line.Name, lineInformation);
                foreach (Way way in line.Routes)
                {
                    foreach (string code in way.Codes)
                    {
                        stopsHash[code].Lines.Add(lineInformation);
                        stopsHash[code].Lines = stopsHash[code].Lines.DistinctBy(line => line.Name).ToList();
                    }
                }
            }

            lines.Add(route.Type, singleLineRoutes);
        }
    }

    public StopInformation FindStop(string code)
    {
        if (stopsHash.TryGetValue(code, out StopInformation stopInformation))
            return stopInformation;

        return null;
    }
    public LineInformation FindByTypeAndLine(TransportType type, string line)
    {
        return FindByTypeAndLine(EnumToType(type), line);
    }

    public LineInformation FindByTypeAndLine(string type, string line)
    {
        if (line.StartsWith("E"))
        {
            type = "bus";
            line = line.Replace("E", "");
        }

        if (lines.TryGetValue(type, out var linesInfo))
        {
            if (linesInfo.TryGetValue(line, out LineInformation routeInformation))
                return routeInformation;
        }

        return null;
    }

    private string EnumToType(TransportType type)
    {
        return type switch
        {
            TransportType.Tram => "tram",
            TransportType.Trolley => "trolley",
            _ => "bus"
        };
    }

    public List<LineInformation> FindByType(TransportType type)
    {
        return FindByType(EnumToType(type));
    }

    public List<LineInformation> FindByType(string type)
    {
        if (lines.TryGetValue(type, out var linesInfo))
        {
            return linesInfo.Values.ToList();
        }

        return null;
    }

    public List<LineInformation> FindLineByTwoStops(string one, string other)
    {
        List<LineInformation> result = new List<LineInformation>();
        foreach (var allLines in lines.Values)
        {
            var currentLines = allLines.Values.Where(v => v.Routes.Any(r => r.Codes.Contains(one) && r.Codes.Contains(other)));
            if (currentLines.Any())
                result.AddRange(currentLines);
        }

        return result;
    } 

    public LineInformation FindStopsByLine(string line, string type)
    {
        if (lines.TryGetValue(type, out Dictionary<string, LineInformation> route))
        {
            if (route.TryGetValue(line, out LineInformation routes))
            {
                return routes;
            }

            throw new TramlineFiveException($"Could not find line with id {line} and type {type}");
        }

        throw new TramlineFiveException($"Could not find type {type}");
    }
}
