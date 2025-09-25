using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkgtService.Models.GTFS;
using SkgtService.Models.Json;
using TransitRealtime;

namespace SkgtService;

public class GTFSClient
{
    private static readonly TimeZoneInfo SofiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Helsinki");

    private readonly GTFSDownloader Downloader;
    private readonly GTFSRepository Repo;
    private readonly GTFSRTService RealtimeService;

    private FeedMessage gtfsRtTripUpdates;

    public List<GTFSRoute> Routes => Repo.Routes;
    public List<GTFSTrip> Trips => Repo.Trips;
    public List<GTFSStop> Stops => Repo.Stops;
    public List<GTFSStopTime> StopTimes => Repo.StopTimes;

    public GTFSClient(string gtfsUrl, string staticGtfsDir, string extractPath, string tripUpdatesUrl, string vehicleUpdatesUrl, string alertsUrl)
    {
        Downloader = new GTFSDownloader(gtfsUrl, staticGtfsDir, extractPath);
        Repo = new GTFSRepository(extractPath);
        RealtimeService = new GTFSRTService(tripUpdatesUrl, vehicleUpdatesUrl, alertsUrl);
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

    public Dictionary<GTFSRoute, List<(GTFSTrip Trip, GTFSStopTime StopTime, DateTime? PredictedDeparture)>> GetNextDeparturesPerRoute(
        string stopCode, DateTime dateTime, int maxPerRoute = 3)
    {
        var result = new Dictionary<GTFSRoute, List<(GTFSTrip, GTFSStopTime, DateTime?)>>();

        DateTime date = dateTime.Date;
        TimeSpan afterTime = dateTime.TimeOfDay;

        foreach (GTFSStop stop in Repo.Indexes.StopsByCode[stopCode])
        {
            List<GTFSRoute> routes = GetRoutesForStop(stop.StopId, date);
            List<GTFSTrip> tripsAtStop = Repo.Indexes.TripsByStop[stop.StopId];

            foreach (GTFSRoute route in routes)
            {
                IEnumerable<GTFSTrip> trips = tripsAtStop.Where(t => t.RouteId == route.RouteId);

                List<(GTFSTrip, GTFSStopTime, DateTime?)> nextDepartures = new List<(GTFSTrip, GTFSStopTime, DateTime?)>();

                foreach (GTFSTrip trip in trips)
                {
                    if (!TripRunsOnDate(trip.TripId, date))
                        continue;

                    string key = $"{trip.TripId}_{stop.StopId}";
                    if (Repo.Indexes.StopTimesByTripAndStop.TryGetValue(key, out GTFSStopTime stopTime))
                    {
                        // Use predicted departure if available, otherwise scheduled
                        DateTime? predictedDeparture = stopTime.PredictedDepartureTime;

                        DateTime referenceTime;
                        if (predictedDeparture.HasValue)
                            referenceTime = predictedDeparture.Value;
                        else if (TryParseGtfsTime(stopTime.DepartureTime, out TimeSpan t))
                            referenceTime = date + t;
                        else
                            continue; // skip if neither available

                        if (referenceTime.TimeOfDay >= afterTime)
                            nextDepartures.Add((trip, stopTime, predictedDeparture));
                    }
                }

                nextDepartures.Sort((a, b) =>
                {
                    DateTime timeA = a.Item3 ?? (date + TimeSpan.Parse(a.Item2.DepartureTime));
                    DateTime timeB = b.Item3 ?? (date + TimeSpan.Parse(b.Item2.DepartureTime));
                    return timeA.CompareTo(timeB);
                });


                // 4. Take only the next `maxPerRoute` departures
                result[route] = nextDepartures.Take(maxPerRoute).ToList();
            }
        }
        return result;
    }

    DateTime? lastRealtimeCheck = null;
    public async Task QueryRealtimeData()
    {
        if (lastRealtimeCheck != null && (DateTime.UtcNow - lastRealtimeCheck.Value).TotalSeconds < 60)
            return; // Avoid querying more than once per minute

        try
        {
            Console.WriteLine("Getting realtime trip updates...");
            gtfsRtTripUpdates = await RealtimeService.FetchTripUpdates();

            ApplyTripUpdates();
            lastRealtimeCheck = DateTime.UtcNow;
            Console.WriteLine("Applied trip updates.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching or applying trip updates: {ex.Message}");
        }
    }

    public GTFSStop GetStopById(string id)
    {
        return Repo.Indexes.StopsById.TryGetValue(id, out GTFSStop stop) ? stop : null;
    }

    public List<GTFSStop> GetStopByCode(string code)
    {
        return Repo.Indexes.StopsByCode.TryGetValue(code, out List<GTFSStop> stop) ? stop : null;
    }

    private void ApplyTripUpdates()
    {
        foreach (FeedEntity entity in gtfsRtTripUpdates.Entities.Where(e => e.TripUpdate != null))
        {
            foreach (TripUpdate.StopTimeUpdate stopTimeUpdate in entity.TripUpdate.StopTimeUpdates)
            {
                string stopId = stopTimeUpdate.StopId;
                string key = $"{entity.TripUpdate.Trip.TripId}_{stopId}";

                if (!Repo.Indexes.StopTimesByTripAndStop.TryGetValue(key, out GTFSStopTime staticStopTime))
                    continue;

                if (stopTimeUpdate.Arrival != null && stopTimeUpdate.Arrival.Time != 0)
                    staticStopTime.PredictedArrivalTime = UnixTimeStampToDateTime(stopTimeUpdate.Arrival.Time);

                if (stopTimeUpdate.Departure != null && stopTimeUpdate.Departure.Time != 0)
                    staticStopTime.PredictedDepartureTime = UnixTimeStampToDateTime(stopTimeUpdate.Departure.Time);
            }
        }
    }
    private DateTime UnixTimeStampToDateTime(long unixTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime, SofiaTimeZone);
    }

    public void LoadStops()
    {
        Repo.LoadStops();
    }
}
