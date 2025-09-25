using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using SkgtService.Models.GTFS;
using SkgtService.Models.Json;

namespace SkgtService;

public class GTFSRepository
{
    private readonly string StaticGtfsDir;
    public List<GTFSRoute> Routes { get; private set; } = new();
    public List<GTFSTrip> Trips { get; private set; } = new();
    public List<GTFSStop> Stops { get; private set; } = new();
    public List<GTFSStopTime> StopTimes { get; private set; } = new();
    public List<GTFSCalendarDate> CalendarDates { get; private set; } = new();

    public GTFSIndexes Indexes { get; private set; } = new();

    //public List<Agency> Agencies { get; private set; } = new
    //public List<Calendar> Calendars { get; private set; } = new();
    //public List<CalendarDate> CalendarDates { get; private set; } = new();

    public GTFSRepository(string staticGtfsDir)
    {
        StaticGtfsDir = staticGtfsDir;
    }

    public void LoadData()
    {
        Stops = LoadCSV<GTFSStop>("stops.txt");
        Routes = LoadCSV<GTFSRoute>("routes.txt");
        Trips = LoadCSV<GTFSTrip>("trips.txt");
        StopTimes = LoadCSV<GTFSStopTime>("stop_times.txt");
        CalendarDates = LoadCSV<GTFSCalendarDate>("calendar_dates.txt");

        Indexes.BuildIndexes(this);
    }

    private List<T> LoadCSV<T>(string gtfsCsvName)
    {
        string path = Path.Combine(StaticGtfsDir, gtfsCsvName);
        if (!File.Exists(path))
            return [];

        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header?.Trim(),
            MissingFieldFound = null,
            HeaderValidated = null // ignore missing headers
        };

        using StreamReader reader = new StreamReader(path);
        using CsvReader csv = new CsvReader(reader, config);
        return [.. csv.GetRecords<T>()];
    }
}
