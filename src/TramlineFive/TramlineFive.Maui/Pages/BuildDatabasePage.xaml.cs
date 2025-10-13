using CsvHelper;
using Microsoft.Maui.Controls;
using SkgtService;
using SQLite;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using TramlineFive.DataAccess;
using TramlineFive.DataAccess.Entities.GTFS;

namespace TramlineFive.Maui.Pages;

public partial class BuildDatabasePage : ContentPage
{
    public BuildDatabasePage()
    {
        InitializeComponent();
        Task.Run(() => BuildDatabaseAsync());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DeviceDisplay.Current.KeepScreenOn = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing(); 
        DeviceDisplay.Current.KeepScreenOn = false;
    }

    private async Task BuildDatabaseAsync()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = "Сваляне на GTFS данни...";
                ProgressBar.Progress = 0;
            });

            string zipPath = Path.Combine(FileSystem.CacheDirectory, "gtfs.zip");
            string extractPath = Path.Combine(FileSystem.CacheDirectory, "gtfs");
            GTFSDownloader downloader = new GTFSDownloader("https://gtfs.sofiatraffic.bg/api/v1/static", zipPath, extractPath);
            await downloader.DownloadStaticDataAsync();
            downloader.ExtractStaticData();

            // Create database
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "gtfs.db");
            if (File.Exists(dbPath))
                File.Delete(dbPath);

            SQLiteConnection db = new SQLiteConnection(dbPath);

            db.CreateTable<Stop>();
            db.CreateTable<Route>();
            db.CreateTable<Trip>();
            db.CreateTable<StopTime>();
            db.CreateTable<CalendarDate>();
            db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS IX_Trip_Stop ON StopTime(TripId, StopId)");

            // Insert CSVs
            await InsertCsvAsync<Stop>(db, Path.Combine(extractPath, "stops.txt"), "stops");
            await InsertCsvAsync<Route>(db, Path.Combine(extractPath, "routes.txt"), "routes");
            await InsertCsvAsync<Trip>(db, Path.Combine(extractPath, "trips.txt"), "trips");
            await InsertCsvAsync<CalendarDate>(db, Path.Combine(extractPath, "calendar_dates.txt"), "calendar_dates");
            await InsertCsvAsync<StopTime>(db, Path.Combine(extractPath, "stop_times.txt"), "stop_times");
            
            MainThread.BeginInvokeOnMainThread(() =>
            { 
                StatusLabel.Text = $"Оптимизиране на зареждане спирки...";
            });

            GTFSContext.CreateDominantTypesTable(db);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressBar.Progress = 1;
                App.Current.Windows[0].Page = new AppShell();
            });

        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = $"Error: {ex.Message}";
            });
        }
    }

    private async Task InsertCsvAsync<T>(SQLiteConnection db, string filePath, string tableName)
    {
        using StreamReader reader = new StreamReader(filePath);
        using CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,       // Ignore missing header validation
            PrepareHeaderForMatch = (args) => args.Header.Replace("_", "").ToLower() // Normalize
        });

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = $"Четене на {tableName}...";
        });

        List<T> records = csv.GetRecords<T>().ToList();
        int total = records.Count;
        int count = 0;

        await Task.Run(() =>
        {
            db.RunInTransaction(() =>
            {
                foreach (T record in records)
                {
                    db.InsertOrReplace(record);
                    count++;
                    if (count % 100 == 0) // update UI every 100 records
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ProgressBar.Progress = (double)count / total;
                            StatusLabel.Text = $"Въвеждане на {tableName}: {count}/{total}";
                        });
                    }
                }
            });
        });

        // Ensure final progress update
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ProgressBar.Progress = 1;
            StatusLabel.Text = $"Готово {tableName}";
        });
    }
}
