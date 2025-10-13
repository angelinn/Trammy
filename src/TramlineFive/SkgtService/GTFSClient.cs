using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

    private readonly GTFSRTService RealtimeService;

    private FeedMessage gtfsRtTripUpdates;

    private DateTime? lastRealtimeCheck = null;

    public event EventHandler VehicleUpdatesUpdated;

    public List<StopWithType> Stops { get; private set; }
    public Dictionary<string, TransportType> StopDominantTypes { get; init; } = new();
    public Dictionary<(string routeId, string stopId), DateTime> PredictedArrivals { get; init; } = new();
    public Dictionary<string, VehiclePosition> VehiclePositions { get; init; } = new(); 

    public GTFSClient(string gtfsUrl, string staticGtfsDir, string extractPath, string tripUpdatesUrl, string vehicleUpdatesUrl, string alertsUrl)
    {
        RealtimeService = new GTFSRTService(tripUpdatesUrl, vehicleUpdatesUrl, alertsUrl);
    }

    public async Task LoadDataAsync()
    {
        Stops = await GTFSContext.FetchDominantStops();
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
    private bool queryUpdates;

    public void QueryVehicleUpdates()
    {
        Task.Run(async () =>
        {
            queryUpdates = true;

            while (queryUpdates)
            {
                FeedMessage vehicleUpdates = await RealtimeService.FetchVehicleUpdates();
                VehiclePositions.Clear();

                foreach (FeedEntity entity in vehicleUpdates.Entities.Where(e => e.Vehicle != null))
                {
                    VehiclePositions[entity.Vehicle.Trip.TripId] = entity.Vehicle;
                }

                var list = VehiclePositions.Keys.Where(k => k.Contains("A29")).ToList();
                VehicleUpdatesUpdated?.Invoke(this, null);

                await Task.Delay(10000);
            }
        });
    }

    public void StopVehicleUpdates()
    {
        queryUpdates = false;
    }

    public async Task<List<Stop>> GetStopsByCodeAsync(string stopCode)
    {
        return await GTFSContext.GetStopsByCodeAsync(stopCode);
    }

    // get all trip ids for this stop for today
    // check if any are present in realtime data
    private void ApplyTripUpdates()
    {
        PredictedArrivals.Clear();

        foreach (FeedEntity entity in gtfsRtTripUpdates.Entities.Where(e => e.TripUpdate != null))
        {
            foreach (TripUpdate.StopTimeUpdate stopTimeUpdate in entity.TripUpdate.StopTimeUpdates)
            {
                if (PredictedArrivals.ContainsKey((entity.TripUpdate.Trip.TripId, stopTimeUpdate.StopId)))
                {
                    Console.WriteLine("Duplicate tripId-stopId in PredictedArrivals");
                    System.Diagnostics.Debugger.Break();
                }

                PredictedArrivals[(entity.TripUpdate.Trip.TripId, stopTimeUpdate.StopId)] = UnixTimeStampToDateTime(stopTimeUpdate.Departure.Time);
            }
        }
    }

    private DateTime UnixTimeStampToDateTime(long unixTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime, SofiaTimeZone);
    }

}
