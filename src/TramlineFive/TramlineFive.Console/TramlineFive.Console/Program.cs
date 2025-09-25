using GTFS;
using GTFS.IO;
using Octokit;
using SkgtService;
using SkgtService.Models.GTFS;
using TransitRealtime;
using static SkgtService.GTFSRepository;


const string GTFS_STATIC_DATA_URL = "https://gtfs.sofiatraffic.bg/api/v1/static";
const string DESTINATION_STATIC_ZIP_PATH = "gtfs_static.zip";
const string EXTRACT_PATH = "gtfs_data";

const string TRIP_UPDATES_URL = "https://gtfs.sofiatraffic.bg/api/v1/trip-updates";
const string VEHICLE_POSITION_URL = "https://gtfs.sofiatraffic.bg/api/v1/vehicle-positions";
const string ALERTS_URL = "https://gtfs.sofiatraffic.bg/api/v1/alerts";
void ManualTestGTFS()
{
    GTFSDownloader downloader = new GTFSDownloader(GTFS_STATIC_DATA_URL, DESTINATION_STATIC_ZIP_PATH, EXTRACT_PATH);
    downloader.DownloadStaticDataAsync().Wait();
    downloader.ExtractStaticData();

    GTFSRepository repo = new GTFSRepository(EXTRACT_PATH);
    repo.LoadData();

    foreach (var stop in repo.Stops.Take(10))
    {
        Console.WriteLine($"{stop.StopId}: {stop.StopName} ({stop.StopLat}, {stop.StopLon})");
    }

    Console.WriteLine("\nSample of 10 routes:");
    foreach (var route in repo.Routes.Take(10))
    {
        Console.WriteLine($"{route.RouteId}: {route.RouteShortName} - {route.RouteLongName}");
    }

    Console.WriteLine("\nSample of 10 trips");
    foreach (var trip in repo.Trips.Take(10))
    {
        Console.WriteLine($"{trip.TripId}: route {trip.RouteId}, service {trip.ServiceId}, headsign {trip.TripHeadsign}");
    }


    Console.WriteLine($"Total {repo.Stops.Count} stops.");
    Console.WriteLine($"Total {repo.Routes.Count} routes.");
    Console.WriteLine($"Total {repo.Trips.Count} trips.");


    Console.WriteLine("Автобус 107 route A29");
    foreach (var trip in repo.Indexes.TripsByRoute["A29"])
    {
        Console.WriteLine($"   TripId: {trip.TripId} → Headsign: {trip.TripHeadsign}");
    }

    string selectedRouteId = "A29"; // example
    if (repo.Indexes.TripsByRoute.TryGetValue(selectedRouteId, out var tripsForRoute))
    {
        foreach (var trip in tripsForRoute)
        {
            Console.WriteLine($"Trip {trip.TripId} → {trip.TripHeadsign}");

            if (repo.Indexes.StopTimesByTrip.TryGetValue(trip.TripId, out var stopsForTrip))
            {
                foreach (var st in stopsForTrip)
                {
                    if (repo.Indexes.StopsById.TryGetValue(st.StopId, out var stop))
                    {
                        Console.WriteLine($"   Stop {stop.StopName} ({stop.StopId}) | Arr: {st.ArrivalTime} | Dep: {st.DepartureTime}");
                    }
                }
            }
        }
    }
}

async Task StartStopOfflineQuery()
{

    GTFSClient client = new GTFSClient(GTFS_STATIC_DATA_URL, DESTINATION_STATIC_ZIP_PATH, EXTRACT_PATH, TRIP_UPDATES_URL, VEHICLE_POSITION_URL, ALERTS_URL);
    await client.DownloadAndExtractAsync();
    client.LoadAllData();

    await client.QueryRealtimeData();

    Console.WriteLine("\nReady\n");
    while (true)
    {
        string stop = Console.ReadLine();
        Console.WriteLine($"\nGet next 3 departures for stop {stop}");
        var nextDepartures = client.GetNextDeparturesPerRoute(stop, DateTime.Now);
        foreach (var (route, departure) in nextDepartures)
        {
            Console.WriteLine($"Route {route.RouteShortName} ({route.RouteId})");
            foreach (var (trip, stopTime, predictedDeparture) in departure)
            {
                Console.WriteLine($"   Trip to {trip.TripHeadsign} at PREDICTED: {predictedDeparture}, SCHEDULED: {stopTime.DepartureTime} (TripId: {trip.TripId})");
            }
        }

        Console.WriteLine("\nDone\n");
    }
}


async Task TestGTFSRT()
{
    GTFSRTService rtService = new GTFSRTService(TRIP_UPDATES_URL, VEHICLE_POSITION_URL, ALERTS_URL);

    FeedMessage tripUpdates = await rtService.FetchTripUpdates();
    FeedMessage positions = await rtService.FetchVehicleUpdates();
    FeedMessage alerts = await rtService.FetchAlerts();


    if (tripUpdates.Entities[0].TripUpdate != null)
    {
        foreach (var these in tripUpdates.Entities.Where(e => e.TripUpdate.Trip.RouteId == "A29"))
        {
            var tripUpdate = these.TripUpdate;
            Console.WriteLine($"Trip ID: {tripUpdate.Trip.TripId}");
            foreach (var stopTimeUpdate in tripUpdate.StopTimeUpdates)
            {
                Console.WriteLine($"Stop ID: {stopTimeUpdate.StopId}, Arrival Delay: {stopTimeUpdate.Arrival.Time}");
            }
        }
    }


    if (positions.Entities[0].Vehicle != null)
    {
        foreach (var these in positions.Entities.Where(e => e.Vehicle.Trip.RouteId == "A29"))
        {
            var vehicle = these.Vehicle;
            Console.WriteLine($"Vehicle ID: {vehicle.Vehicle.Id}, Position: ({vehicle.Position.Latitude}, {vehicle.Position.Longitude})");
        }
    }

    // Iterate through the feed entities
    foreach (FeedEntity entity in alerts.Entities.Take(1))
    {
        if (entity.Alert != null)
        {
            var alert = entity.Alert;
            Console.WriteLine($"Alert: {alert.DescriptionText.Translations[0].Text}");
        }
    }
}

StartStopOfflineQuery().Wait();
