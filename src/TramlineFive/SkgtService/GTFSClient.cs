using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkgtService.Models.GTFS;
using SkgtService.Models.Json;

namespace SkgtService;

public class GTFSClient
{
    private readonly GTFSDownloader Downloader;
    private readonly GTFSRepository Repo;

    public List<GTFSRoute> Routes => Repo.Routes;
    public List<GTFSTrip> Trips => Repo.Trips;
    public List<GTFSStop> Stops => Repo.Stops;
    public List<GTFSStopTime> StopTimes => Repo.StopTimes;

    public GTFSClient(string gtfsUrl, string staticGtfsDir, string extractPath)
    {
        Downloader = new GTFSDownloader(gtfsUrl, staticGtfsDir, extractPath);
        Repo = new GTFSRepository(extractPath);
    }

    public async Task DownloadAndExtractAsync()
    {
        await Downloader.DownloadStaticDataAsync();
        Downloader.ExtractStaticData();
    }

    public void LoadAllData()
    {
        Console.WriteLine("Loading data...");
        Repo.LoadData();
        Console.WriteLine("Loaded");
    }

    private bool TripRunsOnDate(string tripId, DateTime date)
    {
        GTFSTrip trip = Repo.Trips.FirstOrDefault(t => t.TripId == tripId);

        if (trip == null) 
            return false;

        if (!Repo.Indexes.ServiceDates.TryGetValue(trip.ServiceId, out HashSet<DateTime> dates))
            return false;

        return dates.Contains(date.Date);
    }

    public List<GTFSRoute> GetRoutesForStop(string stopId, DateTime? date = null)
    {
        if (!Repo.Indexes.TripsByStop.TryGetValue(stopId, out List<GTFSTrip> trips))
            return new List<GTFSRoute>();

        HashSet<string> routeIds = new HashSet<string>();

        foreach (GTFSTrip trip in trips)
        {
            if (date.HasValue && !TripRunsOnDate(trip.TripId, date.Value))
                continue;

            routeIds.Add(trip.RouteId);
        }

        return Repo.Routes.Where(r => routeIds.Contains(r.RouteId)).ToList();

    }
    public List<GTFSTrip> GetTripsForRoute(string routeId) =>
        Repo.Indexes.TripsByRoute.TryGetValue(routeId, out List<GTFSTrip> trips)
            ? trips
            : new List<GTFSTrip>();

    private bool TryParseGtfsTime(string timeStr, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        if (TimeSpan.TryParse(timeStr, out time))
            return true;

        string[] parts = timeStr.Split(':');
        if (parts.Length != 3) return false;

        if (int.TryParse(parts[0], out int hours) &&
            int.TryParse(parts[1], out int minutes) &&
            int.TryParse(parts[2], out int seconds))
        {
            time = new TimeSpan(hours, minutes, seconds);
            return true;
        }

        return false;
    }
    public Dictionary<GTFSRoute, List<(GTFSTrip Trip, GTFSStopTime StopTime)>> GetNextDeparturesPerRoute(
        string stopId, DateTime dateTime, int maxPerRoute = 3)
    {
        var result = new Dictionary<GTFSRoute, List<(GTFSTrip, GTFSStopTime)>>();

        DateTime date = dateTime.Date;
        TimeSpan afterTime = dateTime.TimeOfDay;

        if (!Repo.Indexes.TripsByStop.TryGetValue(stopId, out List<GTFSTrip> tripsAtStop))
            return result; 

        List<GTFSRoute> routes = GetRoutesForStop(stopId, date);

        foreach (GTFSRoute route in routes)
        {
            IEnumerable<GTFSTrip> trips = tripsAtStop.Where(t => t.RouteId == route.RouteId);

            List<(GTFSTrip, GTFSStopTime)> nextDepartures = new List<(GTFSTrip, GTFSStopTime)>();

            foreach (GTFSTrip trip in trips)
            {
                if (!TripRunsOnDate(trip.TripId, date)) 
                    continue;

                string key = $"{trip.TripId}_{stopId}";
                if (Repo.Indexes.StopTimesByTripAndStop.TryGetValue(key, out GTFSStopTime stopTime))
                {
                    if (TryParseGtfsTime(stopTime.DepartureTime, out TimeSpan t) && t >= afterTime)
                    {
                        nextDepartures.Add((trip, stopTime));
                    }
                }
            }

            nextDepartures.Sort((a, b) =>
            {
                TryParseGtfsTime(a.Item2.DepartureTime, out TimeSpan t1);
                TryParseGtfsTime(b.Item2.DepartureTime, out TimeSpan t2);

                return t1.CompareTo(t2);
            });

            // 4. Take only the next `maxPerRoute` departures
            result[route] = nextDepartures.Take(maxPerRoute).ToList();
        }

        return result;
    }

    public GTFSStop GetStopById(string id)
    {
        return Repo.Indexes.StopsById.TryGetValue(id, out GTFSStop stop) ? stop : null;
    }

}
