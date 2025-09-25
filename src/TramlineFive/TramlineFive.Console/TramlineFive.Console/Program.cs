using SkgtService;
using SkgtService.Models.GTFS;
using static SkgtService.GTFSRepository;


const string GTFS_STATIC_DATA_URL = "https://gtfs.sofiatraffic.bg/api/v1/static";
const string DESTINATION_STATIC_ZIP_PATH = "gtfs_static.zip";
const string EXTRACT_PATH = "gtfs_data";

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

GTFSClient client = new GTFSClient(GTFS_STATIC_DATA_URL, DESTINATION_STATIC_ZIP_PATH, EXTRACT_PATH);
client.DownloadAndExtractAsync().Wait();
client.LoadAllData();

Console.WriteLine("\nReady\n");
while (true)
{
    string stop = Console.ReadLine();
    GTFSStop gtfsStop = client.GetStopById(stop);
    gtfsStop.
    Console.WriteLine($"\nGet next 3 departures for stop {stop}");
    var nextDepartures = client.GetNextDeparturesPerRoute(stop, DateTime.Now);
    foreach (var (route, departure) in nextDepartures)
    {
        Console.WriteLine($"Route {route.RouteShortName} ({route.RouteId})");
        foreach (var (trip, stopTime) in departure)
        {
            Console.WriteLine($"   Trip to {trip.TripHeadsign} at {stopTime.DepartureTime} (TripId: {trip.TripId})");
        }
    }

    Console.WriteLine("\nDone\n");
}