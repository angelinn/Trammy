using Octokit;
using SkgtService;
using SQLite;
using TransitRealtime;

using TramlineFive.DataAccess.Entities.GTFS;
using CsvHelper;
using System.Globalization;
using System.IO.Compression;
using CsvHelper.Configuration;
using NetTopologySuite.GeometriesGraph;
using Microsoft.Maui.Platform;
using TramlineFive.Common.ViewModels;
using TramlineFive.Common.GTFS;
using System.Threading.Tasks;
using TramlineFive.Common.Services.Interfaces;
using TramlineFive.Common.Services;
using TramlineFive.Console;
using TramlineFive.DataAccess;

const string GTFS_STATIC_DATA_URL = "https://gtfs.sofiatraffic.bg/api/v1/static";
const string DESTINATION_STATIC_ZIP_PATH = "gtfs_static.zip";
const string EXTRACT_PATH = "gtfs_data";

const string TRIP_UPDATES_URL = "https://gtfs.sofiatraffic.bg/api/v1/trip-updates";
const string VEHICLE_POSITION_URL = "https://gtfs.sofiatraffic.bg/api/v1/vehicle-positions";
const string ALERTS_URL = "https://gtfs.sofiatraffic.bg/api/v1/alerts";


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

            Console.WriteLine($"Vehicle ID: {vehicle.Vehicle.Id}, Trip: {vehicle.Trip.TripId}, Route: {vehicle.Trip.RouteId}, Position: ({vehicle.Position.Latitude}, {vehicle.Position.Longitude})");
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
    GTFSContext.DatabasePath = dbPath;

    string zipPath = Path.Combine(Path.GetTempPath(), "gtfs.zip");

    string extractPath = Path.Combine(Path.GetTempPath(), "gtfs");
    GTFSDownloader downloader = new GTFSDownloader(GTFS_STATIC_DATA_URL, zipPath, extractPath);

    await GTFSContext.CreateDatabaseFromStaticDataAsync(extractPath, (progress) =>
    {
        Console.Write($"\r[{(int)(progress.Progress * 100)}%] {progress.Status}");
        if (progress.Progress >= 1.0)
            Console.WriteLine();
    });
}

async Task QueryStop(string stopCode, bool generateDb = false)
{
    if (generateDb)
        await GenerateDB();

    ServiceCollection sc = new ServiceCollection();
    sc.AddSingleton<IApplicationService, DummyApplicationService>();
    sc.AddSingleton<INavigationService, DummyNavigationService>();

    ServiceContainer.ServiceProvider = sc.BuildServiceProvider();

    GTFSContext.DatabasePath = Path.Combine(Environment.CurrentDirectory, "gtfs_test.db");
    TramlineFiveContext.DatabasePath = Path.Combine(Environment.CurrentDirectory, "tramlinefive_test.db");

    VirtualTablesViewModel vm = new VirtualTablesViewModel(new GTFSClient(TRIP_UPDATES_URL, VEHICLE_POSITION_URL, ALERTS_URL));
    await vm.CheckStopAsync(stopCode);

    Console.WriteLine($"Спирка {vm.StopInfo.PublicName}");
    foreach (var routeArrival in vm.StopInfo.Arrivals)
    {
        foreach (var arrival in routeArrival.Arrivals)
        {
            if (arrival.Realtime)
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"{(arrival.Realtime ? "R" : string.Empty)} {routeArrival.LineName} [{routeArrival.Direction}] - {arrival.MinutesTillArrival}");
            Console.ResetColor();
        }
    }
}

//GenerateDB().Wait();
QueryStop("2193").Wait();