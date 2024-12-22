using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sentry;
using SkgtService.Exceptions;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace SkgtService;

public class PublicTransport
{
    private Dictionary<string, StopInformation> stopsHash = new();
    private Dictionary<string, Dictionary<string, Line>> lines = new();
    private Dictionary<string, List<Route>> routes = new();
    private Dictionary<Line, ScheduleResponse> schedules = new();

    public ManualResetEvent StopsReadyEvent = new(false);

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

    public Dictionary<string, Dictionary<string, Line>> Lines => lines;
    public Dictionary<Line, ScheduleResponse> Schedules => schedules;
 
    private readonly StopsLoader stopsLoader;

    public PublicTransport(StopsLoader stopsLoader)
    {
        this.stopsLoader = stopsLoader;
    }

    public async Task LoadLinesAsync()
    {
        var newLines = await stopsLoader.LoadLinesAsync();

        if (lines.Count > 0)
            return;

        Lines.Add("bus", new Dictionary<string, Line>());
        Lines.Add("tram", new Dictionary<string, Line>());
        Lines.Add("trolley", new Dictionary<string, Line>());
        Lines.Add("subway", new Dictionary<string, Line>());
        Lines.Add("nightbus", new Dictionary<string, Line>());

        foreach (Line line in newLines)
        {
            if (line.Name.StartsWith('E'))
                line.Type = TransportType.Bus;

            Lines[EnumToType(line.Type)].Add(line.Name, line);
        }
    }

    public async Task LoadData()
    {
        if (stopsHash.Count > 0)
            return;


        List<StopLocation> stops = await stopsLoader.LoadStopsAsync();
        SentrySdk.CaptureMessage($"Loaded {stops.Count} stops.");

        foreach (var stop in stops)
        {
            try
            {
                if (!string.IsNullOrEmpty(stop.Code))
                    stopsHash.Add(stop.Code, new StopInformation(stop));
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureMessage($"id: {stop.Code}, object: {JsonConvert.SerializeObject(stop)}");

                SentrySdk.CaptureException(ex);
            }
        }

        StopsReadyEvent.Set();
        //var loadedRoutes = await StopsLoader.LoadRoutesAsync();

        //foreach (Routes route in loadedRoutes)
        //{
        //    Dictionary<string, LineInformation> singleLineRoutes = new();
        //    foreach (Route line in route.Lines)
        //    {
        //        LineInformation lineInformation = new LineInformation(line.Name, route.Type, line.Routes);
        //        singleLineRoutes.Add(line.Name, lineInformation);
        //        foreach (Way way in line.Routes)
        //        {
        //            foreach (string code in way.Codes)
        //            {
        //                if (stopsHash.ContainsKey(code))
        //                {
        //                    stopsHash[code].Lines.Add(lineInformation);
        //                    stopsHash[code].Lines = stopsHash[code].Lines.DistinctBy(line => line.Name).ToList();
        //                }
        //            }
        //        }
        //    }

        //    lines.Add(route.Type, singleLineRoutes);
        //}
    }

    public StopInformation FindStop(string code)
    {
        //string zeroes = new string('0', 4 - code.Length);
        //if (zeroes.Length > 0)
        //    code = zeroes + code;

        if (stopsHash.TryGetValue(code, out StopInformation stopInformation))
            return stopInformation;

        return null;
    }
    public Line FindByTypeAndLine(TransportType type, string line)
    {
        return FindByTypeAndLine(EnumToType(type), line);
    }

    public Line FindByTypeAndLine(string type, string line)
    {
        if (line.StartsWith("E"))
        {
            type = "bus";
            line = line.Replace("E", "");
        }

        if (lines.TryGetValue(type, out var linesInfo))
        {
            if (linesInfo.TryGetValue(line, out Line routeInformation))
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
            TransportType.Bus => "bus",
            TransportType.Subway => "subway",
            _ => "bus"
        };
    }

    public List<Line> FindByType(TransportType type)
    {
        return FindByType(EnumToType(type));
    }

    public List<Line> FindByType(string type)
    {
        if (lines.TryGetValue(type, out var linesInfo))
        {
            return linesInfo.Values.ToList();
        }

        return null;
    }

    public List<LineInformation> FindLineByTwoStops(string one, string other)
    {
        return null;
        //List<LineInformation> result = new List<LineInformation>();
        //foreach (var allLines in lines.Values)
        //{
        //    var currentLines = allLines.Values.Where(v => v.Routes.Any(r => r.Codes.Contains(one) && r.Codes.Contains(other)));
        //    if (currentLines.Any())
        //        result.AddRange(currentLines);
        //}

        //return result;
    }

    public Line FindStopsByLine(string line, string type)
    {
        if (lines.TryGetValue(type, out Dictionary<string, Line> route))
        {
            if (route.TryGetValue(line, out Line routes))
            {
                return routes;
            }

            throw new TramlineFiveException($"Could not find line with id {line} and type {type}");
        }

        throw new TramlineFiveException($"Could not find type {type}");
    }

    public async Task LoadSchedule(Line line)
    {
        ScheduleResponse schedule = await stopsLoader.GetSchedule(line);
        schedules.Add(line, schedule);

        foreach (var route in schedule.Routes)
        {
            foreach (var stop in route.Segments)
            {
                if (!stopsHash.ContainsKey(stop.Stop.Code))
                {
                    string zeroes = new string('0', 4 - stop.Stop.Code.Length);
                    stopsHash[zeroes + stop.Stop.Code] = new StopInformation
                    {
                        Code = zeroes + stop.Stop.Code,
                        Lat = double.Parse(stop.Stop.Latitude),
                        Lon = double.Parse(stop.Stop.Longitude),
                        PublicName = stop.Stop.Name
                    };
                }
            }
        }
    }
}
