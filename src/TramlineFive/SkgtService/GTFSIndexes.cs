using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkgtService.Models.GTFS;

namespace SkgtService;

public class GTFSIndexes
{
    public Dictionary<string, List<GTFSTrip>> TripsByRoute { get; private set; } = new();
    public Dictionary<string, List<GTFSStopTime>> StopTimesByTrip { get; private set; } = new();
    public Dictionary<string, GTFSStop> StopsById { get; private set; } = new();
    public Dictionary<string, HashSet<DateTime>> ServiceDates { get; private set; } = new();
    public Dictionary<string, List<GTFSTrip>> TripsByStop { get; private set; } = new();
    // Key: "tripId_stopId"
    // Value: StopTime object
    public Dictionary<string, GTFSStopTime> StopTimesByTripAndStop { get; private set; } = new();

    public void BuildIndexes(GTFSRepository repo)
    {
        Console.WriteLine("Building static indexes...");
        TripsByRoute = repo.Trips
            .GroupBy(t => t.RouteId)
            .ToDictionary(g => g.Key, g => g.ToList());

        StopTimesByTrip = repo.StopTimes
            .GroupBy(st => st.TripId)
            .ToDictionary(g => g.Key, g => g.OrderBy(st => st.StopSequence).ToList());

        StopsById = repo.Stops.ToDictionary(s => s.StopId, s => s);
        Console.WriteLine("Building service dates index...");
        LoadServiceDates(repo);
        Console.WriteLine("Building stops indexes...");
        BuildStopIndex(repo);
        Console.WriteLine("Building trip_stoptime index...");
        BuildStopTimesByTripAndStopIndex();
        Console.WriteLine("Done");
    }

    public void LoadServiceDates(GTFSRepository repo)
    {
        ServiceDates.Clear();

        foreach (GTFSCalendarDate cd in repo.CalendarDates)
        {
            if (!DateTime.TryParseExact(cd.Date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                continue; // skip invalid date formats

            if (!ServiceDates.TryGetValue(cd.ServiceId, out HashSet<DateTime> dates))
            {
                dates = new HashSet<DateTime>();
                ServiceDates[cd.ServiceId] = dates;
            }

            if (cd.ExceptionType == 1)
                dates.Add(date);      // service added
            else if (cd.ExceptionType == 2)
                dates.Remove(date);   // service removed
        }
    }
    public void BuildStopIndex(GTFSRepository repo)
    {
        TripsByStop = new Dictionary<string, List<GTFSTrip>>();
        
        foreach (GTFSTrip trip in repo.Trips)
        {
            if (!StopTimesByTrip.TryGetValue(trip.TripId, out List<GTFSStopTime> stopTimes))
                continue;

            foreach (GTFSStopTime st in stopTimes)
            {
                if (!TripsByStop.TryGetValue(st.StopId, out List<GTFSTrip> list))
                {
                    list = new List<GTFSTrip>();
                    TripsByStop[st.StopId] = list;
                }
                list.Add(trip);
            }
        }
    }

    public void BuildStopTimesByTripAndStopIndex()
    {
        StopTimesByTripAndStop = new Dictionary<string, GTFSStopTime>();

        foreach (var kvp in StopTimesByTrip) // StopTimesByTrip: tripId -> List<StopTime>
        {
            string tripId = kvp.Key;
            List<GTFSStopTime> stopTimes = kvp.Value;

            foreach (GTFSStopTime st in stopTimes)
            {
                string key = $"{tripId}_{st.StopId}";
                StopTimesByTripAndStop[key] = st;
            }
        }
    }



}
