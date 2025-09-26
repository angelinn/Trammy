using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkgtService.Models;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Entities.GTFS;
using TransitRealtime;

namespace SkgtService;

public class GTFSClient
{
    private static readonly TimeZoneInfo SofiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Helsinki");

    private readonly GTFSRepository Repo;
    private readonly GTFSRTService RealtimeService;

    private FeedMessage gtfsRtTripUpdates;

    private DateTime? lastRealtimeCheck = null;
    public Dictionary<string, DateTime> StopTimeCache { get; init; } = new();
    public List<StopWithType> Stops => Repo.Stops;
    public Dictionary<string, TransportType> StopDominantTypes { get; init; } = new();
    private Dictionary<(string TripId, string StopId), StopTime> tripIdStopIdStopTimeCache;

    public GTFSClient(string gtfsUrl, string staticGtfsDir, string extractPath, string tripUpdatesUrl, string vehicleUpdatesUrl, string alertsUrl)
    {
        Repo = new GTFSRepository();
        RealtimeService = new GTFSRTService(tripUpdatesUrl, vehicleUpdatesUrl, alertsUrl);
    }

    public async Task LoadDataAsync()
    {
        await Repo.LoadStopsAsync();
        tripIdStopIdStopTimeCache = await GTFSContext.LoadStopTimesNexthAsync(DateTime.Now, 2);

        //TripToRoute = GTFSContext.BuildTripToVehicleTypeMap();
    }

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

    public async Task<List<Stop>> GetStopsByCodeAsync(string stopCode)
    {
        return await GTFSContext.GetStopsByCodeAsync(stopCode);
    }

    private void ApplyTripUpdates()
    {
        foreach (FeedEntity entity in gtfsRtTripUpdates.Entities.Where(e => e.TripUpdate != null))
        {
            foreach (TripUpdate.StopTimeUpdate stopTimeUpdate in entity.TripUpdate.StopTimeUpdates)
            {
                if (stopTimeUpdate.Departure != null && stopTimeUpdate.Departure.Time != 0 && tripIdStopIdStopTimeCache.TryGetValue((entity.TripUpdate.Trip.TripId, stopTimeUpdate.StopId), out StopTime stopTime))
                {
                    StopTimeCache[$"{entity.TripUpdate.Trip.TripId}_{stopTimeUpdate.StopId}"] = UnixTimeStampToDateTime(stopTimeUpdate.Departure.Time);
                }
            }
        }
    }
    private DateTime UnixTimeStampToDateTime(long unixTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime, SofiaTimeZone);
    }

}
