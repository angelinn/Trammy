using Octokit;
using SkgtService;
using SkgtService.Models.GTFS;
using SQLite;
using TransitRealtime;
using static SkgtService.GTFSRepository;

using TramlineFive.DataAccess.Entities.GTFS;
using CsvHelper;
using System.Globalization;
using System.IO.Compression;
using CsvHelper.Configuration;
using NetTopologySuite.GeometriesGraph;

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
        var nextDepartures = client.GetNextDeparturesPerRoute(stop, DateTime.Now, 3);
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

async Task GenerateDB()
{
    string dbPath = Path.Combine(Environment.CurrentDirectory, "gtfs_test.db");
    var db = new SQLiteConnection(dbPath);

    Console.WriteLine("Creating tables...");
    db.CreateTable<Stop>();
    db.CreateTable<Route>();
    db.CreateTable<Trip>();
    db.CreateTable<StopTime>();
    Console.WriteLine("Tables created.");

    // 1️⃣ Download GTFS zip
    string gtfsUrl = "https://gtfs.sofiatraffic.bg/api/v1/static";
    string zipPath = Path.Combine(Path.GetTempPath(), "gtfs.zip");
    Console.WriteLine("Downloading GTFS...");
    using (var client = new HttpClient())
    {
        var bytes = await client.GetByteArrayAsync(gtfsUrl);
        await File.WriteAllBytesAsync(zipPath, bytes);
    }

    // 2️⃣ Extract zip
    string extractPath = Path.Combine(Path.GetTempPath(), "gtfs");
    if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
    ZipFile.ExtractToDirectory(zipPath, extractPath);
    Console.WriteLine("GTFS extracted.");

    // 3️⃣ Import CSVs
    await ImportCsvAsync<Stop>(db, Path.Combine(extractPath, "stops.txt"), "stops");
    await ImportCsvAsync<Route>(db, Path.Combine(extractPath, "routes.txt"), "routes");
    await ImportCsvAsync<Trip>(db, Path.Combine(extractPath, "trips.txt"), "trips");
    await ImportCsvAsync<StopTime>(db, Path.Combine(extractPath, "stop_times.txt"), "stop_times");

    Console.WriteLine("All CSVs imported.");
}

static async Task ImportCsvAsync<T>(SQLiteConnection db, string filePath, string tableName)
{
    Console.WriteLine($"Importing {tableName}...");

    using var reader = new StreamReader(filePath);
    using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HeaderValidated = null,       // Ignore missing header validation
        PrepareHeaderForMatch = (args) => args.Header.Replace("_", "").ToLower() // Normalize
    });
    var records = csv.GetRecords<T>().ToList();

    int total = records.Count;
    int count = 0;

    await Task.Run(() =>
    {
        db.RunInTransaction(() =>
        {
            foreach (var record in records)
            {
                db.InsertOrReplace(record);
                count++;
                if (count % 100 == 0)
                {
                    Console.WriteLine($"{tableName}: {count}/{total} rows inserted...");
                }
            }
        });
    });

    Console.WriteLine($"{tableName} import complete. Total rows: {total}");
}
static List<(Route route, List<(Trip trip, StopTime stopTime, DateTime predicted)>)> GetNextDeparturesPerStop(
    SQLiteConnection db, string stopCode, DateTime now, int countPerRoute)
{
    var stop = db.Table<Stop>().FirstOrDefault(s => s.StopCode == stopCode);
    if (stop == null) return new List<(Route, List<(Trip, StopTime, DateTime)>)>();

    var stopTimes = db.Table<StopTime>()
                      .Where(st => st.StopId == stop.StopId)
                      .ToList();

    var result = new List<(Route, List<(Trip, StopTime, DateTime)>)>();

    var tripsByRoute = stopTimes
        .Join(db.Table<Trip>(), st => st.TripId, t => t.TripId, (st, t) => new { st, t })
        .Join(db.Table<Route>(), x => x.t.RouteId, r => r.RouteId, (x, r) => new { x.st, x.t, r })
        .GroupBy(x => x.r.RouteId);

    foreach (var g in tripsByRoute)
    {
        var departures = g
            .Select(x =>
            {
                DateTime dep;
                if (!DateTime.TryParseExact(x.st.DepartureTime, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dep))
                    dep = now; // fallback
                return (x.t, x.st, dep);
            })
            .Where(x => x.Item3 >= now)
            .OrderBy(x => x.Item3)
            .Take(countPerRoute)
            .ToList();

        if (departures.Any())
            result.Add((g.First().r, departures));
    }

    return result;
}

static TimeSpan ParseGtfsTime(string time)
{
    var parts = time.Split(':');
    if (parts.Length != 3) throw new FormatException($"Invalid GTFS time: {time}");

    int hours = int.Parse(parts[0]);
    int minutes = int.Parse(parts[1]);
    int seconds = int.Parse(parts[2]);

    return new TimeSpan(hours, minutes, seconds);
}


static List<(Route route, List<(Trip trip, StopTime stopTime, DateTime predicted)>)> GetNextDeparturesPerStopQuery(
    SQLiteConnection db, string stopCode, DateTime now, int countPerRoute)
{
    var stopIds = db.Table<Stop>().Where(s => s.StopCode == stopCode).Select(s => s.StopId).ToList();
    if (stopIds.Count == 0) 
        return new List<(Route, List<(Trip, StopTime, DateTime)>)>();

    string nowStr = now.ToString("HH:mm:ss");

    // Single SQL query fetching everything
    var sql = @$"
SELECT r.RouteId, r.RouteShortName, t.TripId, t.TripHeadsign,
       st.StopId, st.StopSequence, st.DepartureTime
FROM StopTime st
JOIN Trip t ON t.TripId = st.TripId
JOIN Route r ON r.RouteId = t.RouteId
WHERE st.StopId IN ({string.Join(',', stopIds.Select(s => $"'{s}'"))})
  AND st.DepartureTime >= ?
ORDER BY r.RouteId, st.DepartureTime";

    var departures = db.Query<StopDepartureFull>(sql, nowStr);

    // Group by route
    var grouped = departures
        .GroupBy(d => d.RouteId)
        .Select(g =>
        {
            var routeObj = new Route
            {
                RouteId = g.Key,
                RouteShortName = g.First().RouteShortName
            };

            var list = g.Take(countPerRoute)
             .Select(d =>
             {
                 var trip = new Trip { TripId = d.TripId, TripHeadsign = d.TripHeadsign, RouteId = d.RouteId };
                 var stopTime = new StopTime { TripId = d.TripId, StopId = d.StopId, StopSequence = d.StopSequence, DepartureTime = d.DepartureTime };

                 // Use TimeSpan instead of DateTime.ParseExact
                 TimeSpan ts = ParseGtfsTime(d.DepartureTime);
                 DateTime predicted = DateTime.Today + ts; // adjust for service day
                 return (trip, stopTime, predicted);
             }).ToList();


            return (routeObj, list);
        }).ToList();

    return grouped;
}

void TestDB()
{
    string dbPath = Path.Combine(Environment.CurrentDirectory, "gtfs_test.db");
    var db = new SQLiteConnection(dbPath);

    Console.WriteLine("\nReady\n");
    while (true)
    {
        Console.Write("Enter stop code: ");
        string stop = Console.ReadLine();
        var nextDepartures = GetNextDeparturesPerStopQuery(db, stop, DateTime.Now, 3);

        Console.WriteLine($"\nNext 3 departures for stop {stop}:");
        foreach (var (route, departures) in nextDepartures)
        {
            Console.WriteLine($"Route {route.RouteShortName} ({route.RouteId})");
            foreach (var (trip, stopTime, predicted) in departures)
            {
                Console.WriteLine($"   Trip to {trip.TripHeadsign} at PREDICTED: {predicted}, SCHEDULED: {stopTime.DepartureTime} (TripId: {trip.TripId})");
            }
        }

        Console.WriteLine("\nDone\n");
    }
}

TestDB();
